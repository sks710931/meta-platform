import { MessageSquare, Plus, RefreshCw } from "lucide-react";
import { Alert } from "../components/Alert";
import { Button } from "../components/Button";
import { EmptyState } from "../components/EmptyState";
import { Skeleton } from "../components/Skeleton";
import { AccountCard } from "./AccountCard";
import { getMetaConfiguration } from "../meta/metaApi";
import { prepareEmbeddedSignup, EmbeddedSignupError } from "../meta/embeddedSignup";
import type { MetaSignupConfiguration } from "../meta/embeddedSignup";
import { useCurrentUser } from "../auth/AuthenticationShell";
import { useEffect, useRef, useState } from "react";
import { listAccounts, startSession, getSession, whatsappError } from "./whatsappApi";
import type { AccountView, SessionView } from "./whatsappApi";
import { OnboardingSessionPanel } from "./OnboardingSessionPanel";

export function WhatsAppAccountsSection({ organizationId }: { organizationId: string }) {
  const user = useCurrentUser();
  const canAdminister =
    user.isPlatformAdmin ||
    user.organizations.some((org) => org.organizationId === organizationId && org.role === "OrganizationAdmin");
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
    const controller = new AbortController();
    setLoading(true);
    setError(null);
    listAccounts(organizationId, controller.signal)
      .then((items) => {
        if (!controller.signal.aborted) setAccounts(items);
      })
      .catch((reason: unknown) => {
        if (!controller.signal.aborted) setError(whatsappError(reason));
      })
      .finally(() => {
        if (!controller.signal.aborted) setLoading(false);
      });
    return () => controller.abort();
  }, [organizationId, attempt]);

  useEffect(() => {
    const controller = new AbortController();
    getMetaConfiguration(controller.signal)
      .then(async (config) => {
        if (controller.signal.aborted) return;
        if (config) await prepareEmbeddedSignup(config);
        if (!controller.signal.aborted) {
          setMeta(config);
          setMetaReady(true);
        }
      })
      .catch((reason: unknown) => {
        if (!controller.signal.aborted) {
          setConnectError(
            reason instanceof EmbeddedSignupError
              ? reason.message
              : "Unable to prepare Meta signup. Refresh to retry; Development manual fallback remains available.",
          );
          setMetaReady(true);
        }
      });
    return () => controller.abort();
  }, []);

  async function connect(): Promise<void> {
    const controller = new AbortController();
    pending.current?.abort();
    pending.current = controller;
    setConnecting(true);
    setConnectError(null);
    try {
      const created = await startSession(organizationId, controller.signal);
      if (!controller.signal.aborted) setSession(created);
    } catch (reason: unknown) {
      if (!controller.signal.aborted) setConnectError(whatsappError(reason));
    } finally {
      if (!controller.signal.aborted) setConnecting(false);
    }
  }
  const connectButton = canAdminister ? (
    <Button
      disabled={!metaReady}
      loading={connecting}
      onClick={() => {
        void connect();
      }}
      icon={<Plus aria-hidden="true" className="size-4" />}
    >
      {connecting ? "Starting session…" : "Connect WhatsApp Account"}
    </Button>
  ) : undefined;
  function completed(): void {
    setAttempt((value) => value + 1);
    const controller = new AbortController();
    pending.current = controller;
    if (!session) return;
    void getSession(session.sessionId, controller.signal)
      .then((updated) => {
        if (!controller.signal.aborted) setSession(updated);
      })
      .catch((reason: unknown) => {
        if (!controller.signal.aborted) setConnectError(whatsappError(reason));
      });
  }
  return (
    <section
      id="whatsapp-accounts"
      tabIndex={-1}
      className="scroll-mt-24 space-y-6 border-t border-slate-200 pt-8 outline-none"
    >
      <div className="flex flex-col gap-4 xl:flex-row xl:items-center xl:justify-between">
        <div>
          <h2 className="text-lg font-semibold tracking-tight">WhatsApp Business Accounts</h2>
          <p className="mt-1.5 text-sm leading-6 text-slate-500">
            Manage accounts and phone numbers connected to this organization.
          </p>
        </div>
        {accounts.length > 0 && session?.status !== "Pending" && connectButton}
      </div>
      {connectError && <Alert title="Unable to start onboarding">{connectError}</Alert>}
      {session && (
        <OnboardingSessionPanel
          key={session.sessionId}
          session={session}
          onSession={setSession}
          configuration={meta}
          onAccountsChanged={() => setAttempt((value) => value + 1)}
          onMetaCompleted={completed}
          onRestart={() => {
            void connect();
          }}
          restarting={connecting}
        />
      )}
      {loading && (
        <div role="status" aria-label="Loading WhatsApp accounts" className="grid gap-5 xl:grid-cols-2">
          <span className="sr-only">Loading accounts…</span>
          {[0, 1].map((key) => (
            <div key={key} className="space-y-5 rounded-xl border border-slate-200 bg-white p-6">
              <Skeleton className="h-5 w-1/2" />
              <Skeleton className="h-3 w-2/5" />
              <Skeleton className="h-16 w-full" />
            </div>
          ))}
        </div>
      )}
      {error && (
        <Alert
          title="Unable to load WhatsApp accounts"
          action={
            <Button
              variant="secondary"
              size="small"
              icon={<RefreshCw aria-hidden="true" className="size-3.5" />}
              onClick={() => setAttempt(attempt + 1)}
            >
              Try again
            </Button>
          }
        >
          {error}
        </Alert>
      )}
      {!loading &&
        !error &&
        (accounts.length === 0 ? (
          <EmptyState
            icon={<MessageSquare className="size-6" />}
            title="No WhatsApp accounts connected"
            action={session ? undefined : connectButton}
          >
            Connect a WhatsApp Business account to manage its phone numbers here. Messaging and billing will be
            available in future releases.
          </EmptyState>
        ) : (
          <div className="grid gap-5 xl:grid-cols-2">
            {accounts.map((account) => (
              <AccountCard key={account.whatsAppAccountId} account={account} />
            ))}
          </div>
        ))}
    </section>
  );
}
