import { useEffect, useRef, useState } from "react";
import type { FormEvent } from "react";
import { Alert } from "../components/Alert";
import { Button } from "../components/Button";
import { FormField } from "../components/FormField";
import { completeSession, whatsappError } from "./whatsappApi";

interface Props {
  sessionId: string;
  onCompleted: () => void;
}

export function ManualCompletionForm({ sessionId, onCompleted }: Props) {
  const [accountId, setAccountId] = useState("");
  const [messagingId, setMessagingId] = useState("");
  const [displayName, setDisplayName] = useState("");
  const [phoneId, setPhoneId] = useState("");
  const [displayPhone, setDisplayPhone] = useState("");
  const [verifiedName, setVerifiedName] = useState("");
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const pending = useRef<AbortController | null>(null);
  useEffect(() => () => pending.current?.abort(), []);

  async function submit(event: FormEvent<HTMLFormElement>): Promise<void> {
    event.preventDefault();
    const controller = new AbortController();
    pending.current?.abort();
    pending.current = controller;
    setSaving(true);
    setError(null);
    try {
      await completeSession(
        sessionId,
        {
          externalWhatsAppAccountId: accountId,
          externalMessagingAccountId: messagingId,
          displayName,
          phoneNumbers: [{ externalPhoneNumberId: phoneId, displayPhoneNumber: displayPhone, verifiedName }],
        },
        controller.signal,
      );
      if (!controller.signal.aborted) onCompleted();
    } catch (reason: unknown) {
      if (!controller.signal.aborted) setError(whatsappError(reason));
    } finally {
      if (!controller.signal.aborted) setSaving(false);
    }
  }
  return (
    <form
      className="space-y-4"
      onSubmit={(event) => {
        void submit(event);
      }}
    >
      <div>
        <h4 className="text-sm font-semibold text-slate-700">Manual onboarding fallback</h4>
        <p className="mt-1.5 text-xs leading-5 text-slate-500">
          Simulate signup with test identifiers. This does not connect to Meta.
        </p>
      </div>
      <fieldset disabled={saving} className="grid min-w-0 gap-4 sm:grid-cols-2">
        <FormField
          label="External WhatsApp account ID"
          required
          maxLength={100}
          value={accountId}
          onChange={(e) => setAccountId(e.target.value)}
        />
        <FormField
          label="External messaging account ID"
          required
          maxLength={100}
          value={messagingId}
          onChange={(e) => setMessagingId(e.target.value)}
        />
        <FormField
          label="Display name (optional)"
          maxLength={200}
          value={displayName}
          onChange={(e) => setDisplayName(e.target.value)}
        />
        <FormField
          label="External phone number ID"
          required
          maxLength={100}
          value={phoneId}
          onChange={(e) => setPhoneId(e.target.value)}
        />
        <FormField
          label="Display phone number"
          required
          maxLength={50}
          value={displayPhone}
          onChange={(e) => setDisplayPhone(e.target.value)}
        />
        <FormField
          label="Verified name (optional)"
          maxLength={200}
          value={verifiedName}
          onChange={(e) => setVerifiedName(e.target.value)}
        />
        <div className="sm:col-span-2">
          <Button type="submit" variant="secondary" loading={saving}>
            {saving ? "Completing…" : "Simulate successful signup"}
          </Button>
        </div>
      </fieldset>
      {error && <Alert title="Manual completion failed">{error}</Alert>}
    </form>
  );
}
