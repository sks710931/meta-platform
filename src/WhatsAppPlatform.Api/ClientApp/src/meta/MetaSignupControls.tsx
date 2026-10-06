import { useEffect, useRef, useState } from "react";
import { completeMetaSession, MetaCompletionError } from "./metaApi";
import { EmbeddedSignupError, launchEmbeddedSignup } from "./embeddedSignup";
import type { MetaSignupConfiguration } from "./embeddedSignup";

interface Props { sessionId: string; configuration: MetaSignupConfiguration; onCompleted: () => void }
export function MetaSignupControls({ sessionId, configuration, onCompleted }: Props) {
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);
  const pending = useRef<AbortController | null>(null);
  useEffect(() => () => pending.current?.abort(), []);
  async function complete(launch: boolean): Promise<void> {
    const controller = new AbortController(); pending.current?.abort(); pending.current = controller;
    setBusy(true); setError(null);
    try {
      // Code never enters React state. Submit immediately; retry uses persisted server state only.
      await completeMetaSession(sessionId, launch ? await launchEmbeddedSignup(configuration, controller.signal) : null, controller.signal);
      if (!controller.signal.aborted) { setSuccess(true); onCompleted(); }
    } catch (reason: unknown) {
      if (!controller.signal.aborted) setError(reason instanceof EmbeddedSignupError || reason instanceof MetaCompletionError
        ? reason.message : "Could not reach the server. Retry the saved result before starting a new signup.");
    } finally { if (!controller.signal.aborted) setBusy(false); }
  }
  return <section>
    {!success && <>
      <p>Your session is ready. Continue to open Meta securely in a popup.</p>
      <button disabled={busy} onClick={() => { void complete(true); }}>{busy ? "Connecting…" : "Continue with Meta"}</button>
      <button disabled={busy} onClick={() => { void complete(false); }}>Retry saved result</button>
    </>}
    {success && <p role="status">WhatsApp account connected.</p>}
    {error && <p role="alert">{error}</p>}
  </section>;
}
