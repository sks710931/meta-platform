import { useEffect, useRef, useState } from "react";
import { ExternalLink, RotateCcw } from "lucide-react";
import { Alert } from "../components/Alert";
import { Button } from "../components/Button";
import { completeMetaSession, MetaCompletionError } from "./metaApi";
import { EmbeddedSignupError, launchEmbeddedSignup } from "./embeddedSignup";
import type { MetaSignupConfiguration } from "./embeddedSignup";

interface Props {
  sessionId: string;
  configuration: MetaSignupConfiguration;
  onCompleted: () => void;
  onPhase: (phase: "ready" | "popup" | "verifying") => void;
  phase: "ready" | "popup" | "verifying";
}
export function MetaSignupControls({ sessionId, configuration, onCompleted, onPhase, phase }: Props) {
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [errorTone, setErrorTone] = useState<"error" | "info">("error");
  const [success, setSuccess] = useState(false);
  const pending = useRef<AbortController | null>(null);
  useEffect(() => () => pending.current?.abort(), []);
  async function complete(launch: boolean): Promise<void> {
    const controller = new AbortController();
    pending.current?.abort();
    pending.current = controller;
    setBusy(true);
    setError(null);
    try {
      // Code never enters React state. Submit immediately; retry uses persisted server state only.
      onPhase(launch ? "popup" : "verifying");
      const authorizationCode = launch ? await launchEmbeddedSignup(configuration, controller.signal) : null;
      onPhase("verifying");
      await completeMetaSession(sessionId, authorizationCode, controller.signal);
      if (!controller.signal.aborted) {
        setSuccess(true);
        onCompleted();
      }
    } catch (reason: unknown) {
      if (!controller.signal.aborted) {
        setErrorTone(
          reason instanceof EmbeddedSignupError && (reason.kind === "cancelled" || reason.kind === "popupClosed")
            ? "info"
            : "error",
        );
        setError(
          reason instanceof EmbeddedSignupError || reason instanceof MetaCompletionError
            ? reason.message
            : "Could not reach the server. Retry the saved result before starting a new signup.",
        );
      }
    } finally {
      if (!controller.signal.aborted) {
        setBusy(false);
        onPhase("ready");
      }
    }
  }
  return (
    <section className="space-y-4">
      {!success && (
        <>
          <p className="text-sm leading-6 text-slate-500">
            Continue to Meta to choose your business account and securely authorize access.
          </p>
          <div className="flex flex-wrap gap-3">
            <Button
              loading={busy}
              icon={<ExternalLink aria-hidden="true" className="size-4" />}
              onClick={() => {
                void complete(true);
              }}
            >
              {busy ? (phase === "verifying" ? "Verifying account…" : "Connecting to Meta…") : "Continue with Meta"}
            </Button>
            <Button
              variant="secondary"
              disabled={busy}
              icon={<RotateCcw aria-hidden="true" className="size-4" />}
              onClick={() => {
                void complete(false);
              }}
            >
              Retry saved result
            </Button>
          </div>
          {busy && (
            <p role="status" className="text-xs text-slate-500">
              {phase === "popup"
                ? "Complete signup in the Meta popup. You can return here when finished."
                : "Verifying your account securely. Keep this page open."}
            </p>
          )}
        </>
      )}
      {success && (
        <Alert tone="success" title="WhatsApp account connected">
          Your connected account is ready to view.
        </Alert>
      )}
      {error && (
        <Alert tone={errorTone} title="Onboarding needs your attention">
          {error}
        </Alert>
      )}
    </section>
  );
}
