import { getMetaConfiguration } from "../meta/metaApi";
import { prepareEmbeddedSignup, EmbeddedSignupError } from "../meta/embeddedSignup";
import type { MetaSignupConfiguration } from "../meta/embeddedSignup";
import { MetaSignupControls } from "../meta/MetaSignupControls";
import { useCurrentUser } from "../auth/AuthenticationShell";
import { useEffect, useRef, useState } from "react";
import { listAccounts, startSession, getSession, whatsappError } from "./whatsappApi";
import type { AccountView, SessionView } from "./whatsappApi";
import { OnboardingSessionPanel } from "./OnboardingSessionPanel";

export function WhatsAppAccountsSection({ organizationId }: { organizationId: string }) {
  const user = useCurrentUser();
  const canAdminister = user.isPlatformAdmin || user.organizations.some((org) => org.organizationId === organizationId && org.role === "OrganizationAdmin");
  const [meta, setMeta] = useState<MetaSignupConfiguration | null>(null);
  const [metaReady, setMetaReady] = useState(false);
  const [accounts, setAccounts] = useState<AccountView[]>([]);
  const [session, setSession] = useState<SessionView | null>(null);
  const [loading, setLoading] = useState(true);
  const [connecting, setConnecting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [connectError, setConnectError] = useState<string | null>(null);
  const [attempt, setAttempt] = useState(0);
  const pending = useRef<AbortController | null>(null);
  useEffect(() => () => pending.current?.abort(), []);
  useEffect(() => {
    const controller = new AbortController(); setLoading(true); setError(null);
    listAccounts(organizationId, controller.signal)
      .then((items) => { if (!controller.signal.aborted) setAccounts(items); })
      .catch((reason: unknown) => { if (!controller.signal.aborted) setError(whatsappError(reason)); })
      .finally(() => { if (!controller.signal.aborted) setLoading(false); });
    return () => controller.abort();
  }, [organizationId, attempt]);

  useEffect(() => {
    const controller = new AbortController();
    getMetaConfiguration(controller.signal).then(async (config) => {
      if (controller.signal.aborted) return;
      if (config) await prepareEmbeddedSignup(config);
      if (!controller.signal.aborted) { setMeta(config); setMetaReady(true); }
    }).catch((reason: unknown) => {
      if (!controller.signal.aborted) {
        setConnectError(reason instanceof EmbeddedSignupError ? reason.message : "Unable to prepare Meta signup. Refresh to retry; Development manual fallback remains available.");
        setMetaReady(true);
      }
    });
    return () => controller.abort();
  }, []);

  async function connect(): Promise<void> {
    const controller = new AbortController(); pending.current?.abort(); pending.current = controller;
    setConnecting(true); setConnectError(null);
    try { const created = await startSession(organizationId, controller.signal); if (!controller.signal.aborted) setSession(created); }
    catch (reason: unknown) { if (!controller.signal.aborted) setConnectError(whatsappError(reason)); }
    finally { if (!controller.signal.aborted) setConnecting(false); }
  }
  return (
    <section>
      <h2>WhatsApp Accounts</h2>
      {canAdminister && <button disabled={connecting || !metaReady} onClick={() => { void connect(); }}>{connecting ? "Starting…" : "Connect WhatsApp Account"}</button>}
      {connectError && <p role="alert">{connectError}</p>}
      {loading && <p role="status">Loading accounts…</p>}
      {error && <><p role="alert">{error}</p><button onClick={() => setAttempt(attempt + 1)}>Retry</button></>}
      {!loading && !error && (accounts.length === 0 ? <p>No accounts connected yet.</p> : accounts.map((account) => (
        <article key={account.whatsAppAccountId}>
          <h3>{account.displayName}</h3><p>Status: {account.status}</p>
          <p>Connected (UTC): {account.connectedAt ?? "Not connected"}</p>
          <ul>{account.phoneNumbers.map((phone) => <li key={phone.phoneNumberId}>{phone.displayPhoneNumber}{phone.verifiedName ? ` — ${phone.verifiedName}` : ""} ({phone.status})</li>)}</ul>
        </article>
      )))}
      {session?.status === "Pending" && meta && <MetaSignupControls key={session.sessionId} sessionId={session.sessionId} configuration={meta}
        onCompleted={() => {
          setAttempt((value) => value + 1);
          const controller = new AbortController(); pending.current = controller;
          void getSession(session.sessionId, controller.signal).then((updated) => {
            if (!controller.signal.aborted) setSession(updated);
          }).catch((reason: unknown) => { if (!controller.signal.aborted) setConnectError(whatsappError(reason)); });
        }} />}
      {session && <OnboardingSessionPanel key={session.sessionId} session={session} onSession={setSession}
        onAccountsChanged={() => setAttempt((value) => value + 1)} />}
    </section>
  );
}
