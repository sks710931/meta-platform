import { useEffect, useRef, useState } from "react";
import { ChevronDown, Code2, ExternalLink, RefreshCw } from "lucide-react";
import { getSession, whatsappError } from "./whatsappApi";
import type { SessionView } from "./whatsappApi";
import { ManualCompletionForm } from "./ManualCompletionForm";
import { MetaSignupControls } from "../meta/MetaSignupControls";
import type { MetaSignupConfiguration } from "../meta/embeddedSignup";
import { Alert } from "../components/Alert";
import { Button } from "../components/Button";
import { OnboardingProgress } from "./OnboardingProgress";
import { SessionTechnicalDetails } from "./SessionTechnicalDetails";
import { StatusBadge } from "../components/StatusBadge";

interface Props {
  session: SessionView;
  onSession: (session: SessionView) => void;
  onAccountsChanged: () => void;
  configuration: MetaSignupConfiguration | null;
  onMetaCompleted: () => void;
  onRestart: () => void;
  restarting: boolean;
}
export function OnboardingSessionPanel({
  session,
  onSession,
  onAccountsChanged,
  configuration,
  onMetaCompleted,
  onRestart,
  restarting,
}: Props) {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [phase, setPhase] = useState<"ready" | "popup" | "verifying">("ready");
  const pending = useRef<AbortController | null>(null);
  useEffect(() => () => pending.current?.abort(), []);
  async function refresh(): Promise<void> {
    const controller = new AbortController();
    pending.current?.abort();
    pending.current = controller;
    setLoading(true);
    setError(null);
    try {
      const updated = await getSession(session.sessionId, controller.signal);
      if (!controller.signal.aborted) onSession(updated);
    } catch (reason: unknown) {
      if (!controller.signal.aborted) setError(whatsappError(reason));
    } finally {
      if (!controller.signal.aborted) setLoading(false);
    }
  }
  const completed = session.status === "Completed";
  return (
    <section
      aria-labelledby="onboarding-title"
      className="overflow-hidden rounded-xl border border-indigo-200 bg-white shadow-xs"
    >
      <div className="flex flex-wrap items-center gap-3 border-b border-slate-100 px-5 py-5 sm:px-6">
        <div className="flex size-9 items-center justify-center rounded-lg bg-indigo-50 text-indigo-600">
          <ExternalLink aria-hidden="true" className="size-4" />
        </div>
        <div className="min-w-0 flex-1">
          <h3 id="onboarding-title" className="text-sm font-semibold">
            Connect WhatsApp Business
          </h3>
          <p className="mt-1 text-xs leading-5 text-slate-500">Securely connect your customer account through Meta.</p>
        </div>
        <StatusBadge status={session.status} />
      </div>
      <div className="space-y-5 p-5 sm:p-6">
        <OnboardingProgress status={session.status} phase={phase} />
        {completed && (
          <Alert tone="success" title="WhatsApp account connected">
            Your account and phone numbers are now listed below.
          </Alert>
        )}
        {(session.status === "Expired" || session.status === "Failed") && (
          <Alert
            tone="info"
            title={session.status === "Expired" ? "This session has expired" : "Onboarding was not completed"}
            action={
              <Button loading={restarting} onClick={onRestart}>
                Start a new session
              </Button>
            }
          >
            Start a new session to connect your account securely.
          </Alert>
        )}
        {session.status === "Pending" && configuration && (
          <MetaSignupControls
            sessionId={session.sessionId}
            configuration={configuration}
            onCompleted={onMetaCompleted}
            onPhase={setPhase}
            phase={phase}
          />
        )}
        {session.status === "Pending" && !configuration && (
          <Alert tone="info" title="Meta signup is unavailable">
            {session.manualCompletionAvailable
              ? "Use the developer fallback below for local testing, or configure Meta signup."
              : "Contact your platform administrator to enable Meta signup."}
          </Alert>
        )}
        {session.status === "Pending" && (
          <div className="flex flex-wrap gap-2">
            <Button
              variant="ghost"
              size="small"
              loading={loading}
              icon={<RefreshCw aria-hidden="true" className="size-3.5" />}
              onClick={() => {
                void refresh();
              }}
            >
              Refresh status
            </Button>
            <Button variant="ghost" size="small" disabled={phase !== "ready"} loading={restarting} onClick={onRestart}>
              Start a new session
            </Button>
          </div>
        )}
        {error && <Alert title="Unable to refresh onboarding">{error}</Alert>}
        <SessionTechnicalDetails session={session} />
        {session.status === "Pending" && session.manualCompletionAvailable && (
          <details className="group border-t border-slate-100 pt-4">
            <summary className="flex flex-wrap list-none items-center gap-2 text-xs font-medium text-slate-500">
              <Code2 aria-hidden="true" className="size-3.5" />
              Developer tools
              <span className="rounded border border-slate-200 bg-slate-50 px-1.5 py-0.5 text-[10px]">
                Development only
              </span>
              <ChevronDown aria-hidden="true" className="ml-auto size-3.5 group-open:rotate-180" />
            </summary>
            <div className="mt-4 rounded-lg border border-slate-200 bg-slate-50 p-4">
              <ManualCompletionForm
                sessionId={session.sessionId}
                onCompleted={() => {
                  onAccountsChanged();
                  void refresh();
                }}
              />
            </div>
          </details>
        )}
      </div>
    </section>
  );
}
