import { useEffect, useRef, useState } from "react";
import { getSession, whatsappError } from "./whatsappApi";
import type { SessionView } from "./whatsappApi";
import { ManualCompletionForm } from "./ManualCompletionForm";

interface Props { session: SessionView; onSession: (session: SessionView) => void; onAccountsChanged: () => void }

export function OnboardingSessionPanel({ session, onSession, onAccountsChanged }: Props) {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const pending = useRef<AbortController | null>(null);
  useEffect(() => () => pending.current?.abort(), []);
  async function refresh(): Promise<void> {
    const controller = new AbortController(); pending.current?.abort(); pending.current = controller;
    setLoading(true); setError(null);
    try { const updated = await getSession(session.sessionId, controller.signal); if (!controller.signal.aborted) onSession(updated); }
    catch (reason: unknown) { if (!controller.signal.aborted) setError(whatsappError(reason)); }
    finally { if (!controller.signal.aborted) setLoading(false); }
  }
  return (
    <section>
      <h3>Onboarding session</h3>
      <dl><dt>Session</dt><dd>{session.sessionId}</dd><dt>Status</dt><dd>{session.status}</dd>
        <dt>Expires (UTC)</dt><dd>{session.expiresAt}</dd>
        {session.completedAt && <><dt>Completed (UTC)</dt><dd>{session.completedAt}</dd></>}
      </dl>
      <button disabled={loading} onClick={() => { void refresh(); }}>{loading ? "Refreshing…" : "Refresh session"}</button>
      {error && <p role="alert">{error}</p>}
      {session.status === "Pending" && session.manualCompletionAvailable && (
        <details><summary>Development manual fallback</summary><ManualCompletionForm key={session.sessionId} sessionId={session.sessionId}
          onCompleted={() => { onAccountsChanged(); void refresh(); }} /></details>
      )}
      {!session.manualCompletionAvailable && session.status === "Pending" && <p>Manual completion is unavailable. Continue with Meta above when signup is configured.</p>}
    </section>
  );
}
