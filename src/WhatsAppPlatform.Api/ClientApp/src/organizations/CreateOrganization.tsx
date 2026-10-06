import { useEffect, useRef, useState } from "react";
import type { FormEvent } from "react";
import { ApiError, createOrganization, errorMessage } from "./organizationsApi";

interface Props {
  onCreated: (organizationId: string) => void;
  onCancel: () => void;
}

export function CreateOrganization({ onCreated, onCancel }: Props) {
  const [name, setName] = useState("");
  const [saving, setSaving] = useState(false);
  const [validationError, setValidationError] = useState<string | null>(null);
  const [serverError, setServerError] = useState<string | null>(null);
  const pending = useRef<AbortController | null>(null);
  useEffect(() => () => pending.current?.abort(), []);

  async function submit(event: FormEvent<HTMLFormElement>): Promise<void> {
    event.preventDefault();
    setValidationError(null);
    setServerError(null);
    if (name.trim().length === 0) {
      setValidationError("Please enter an organization name.");
      return;
    }
    const controller = new AbortController();
    pending.current?.abort();
    pending.current = controller;
    setSaving(true);
    try {
      const organization = await createOrganization(name, controller.signal);
      if (!controller.signal.aborted) onCreated(organization.organizationId);
    } catch (reason: unknown) {
      if (!controller.signal.aborted) {
        if (reason instanceof ApiError && reason.status === 400) setValidationError(reason.message);
        else setServerError(errorMessage(reason));
      }
    } finally {
      if (!controller.signal.aborted) setSaving(false);
    }
  }

  return (
    <section>
      <h2>Create organization</h2>
      <form onSubmit={(event) => { void submit(event); }}>
        <label htmlFor="organization-name">Name</label>
        <input id="organization-name" value={name} onChange={(event) => setName(event.target.value)}
          required disabled={saving} aria-invalid={validationError !== null}
          aria-describedby={validationError ? "name-error" : undefined} />
        {validationError && <p id="name-error" role="alert">{validationError}</p>}
        {serverError && <p role="alert">{serverError}</p>}
        <button type="submit" disabled={saving}>{saving ? "Creating…" : "Create"}</button>
        <button type="button" onClick={onCancel}>Cancel</button>
      </form>
    </section>
  );
}
