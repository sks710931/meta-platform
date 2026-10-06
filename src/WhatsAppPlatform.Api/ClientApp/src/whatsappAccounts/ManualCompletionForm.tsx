import { useEffect, useRef, useState } from "react";
import type { FormEvent } from "react";
import { completeSession, whatsappError } from "./whatsappApi";

interface Props { sessionId: string; onCompleted: () => void }

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
    pending.current?.abort(); pending.current = controller;
    setSaving(true); setError(null);
    try {
      await completeSession(sessionId, { externalWhatsAppAccountId: accountId, externalMessagingAccountId: messagingId,
        displayName, phoneNumbers: [{ externalPhoneNumberId: phoneId, displayPhoneNumber: displayPhone, verifiedName }] }, controller.signal);
      if (!controller.signal.aborted) onCompleted();
    } catch (reason: unknown) { if (!controller.signal.aborted) setError(whatsappError(reason)); }
    finally { if (!controller.signal.aborted) setSaving(false); }
  }
  return (
    <form onSubmit={(event) => { void submit(event); }}>
      <h4>Development-only manual completion</h4>
      <p>Simulate a successful signup using test identifiers. This does not connect to Meta.</p>
      <fieldset disabled={saving}>
        <label>External WhatsApp account ID<input required maxLength={100} value={accountId} onChange={(e) => setAccountId(e.target.value)} /></label>
        <label>External messaging account ID<input required maxLength={100} value={messagingId} onChange={(e) => setMessagingId(e.target.value)} /></label>
        <label>Display name (optional)<input maxLength={200} value={displayName} onChange={(e) => setDisplayName(e.target.value)} /></label>
        <label>External phone number ID<input required maxLength={100} value={phoneId} onChange={(e) => setPhoneId(e.target.value)} /></label>
        <label>Display phone number<input required maxLength={50} value={displayPhone} onChange={(e) => setDisplayPhone(e.target.value)} /></label>
        <label>Verified name (optional)<input maxLength={200} value={verifiedName} onChange={(e) => setVerifiedName(e.target.value)} /></label>
        <button type="submit">{saving ? "Completing…" : "Simulate successful signup"}</button>
      </fieldset>
      {error && <p role="alert">{error}</p>}
    </form>
  );
}
