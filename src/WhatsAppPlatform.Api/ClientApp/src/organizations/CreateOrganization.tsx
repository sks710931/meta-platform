import { useEffect, useRef, useState } from "react";
import type { FormEvent } from "react";
import { Building2, X } from "lucide-react";
import { Alert } from "../components/Alert";
import { Button } from "../components/Button";
import { FormField } from "../components/FormField";
import { ApiError, createOrganization, errorMessage } from "./organizationsApi";

interface Props {
  onCreated: (organizationId: string) => void;
  onCancel: () => void;
}

export function CreateOrganization({ onCreated, onCancel }: Props) {
  const dialog = useRef<HTMLDialogElement>(null);
  useEffect(() => {
    const previous = document.activeElement;
    const element = dialog.current;
    element?.showModal();
    element?.querySelector("input")?.focus();
    return () => {
      element?.close();
      if (previous instanceof HTMLElement) previous.focus();
    };
  }, []);
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
    <dialog
      ref={dialog}
      aria-labelledby="create-title"
      onCancel={onCancel}
      className="m-auto w-[calc(100%-2rem)] max-w-lg rounded-2xl border border-slate-200 bg-white p-0 text-slate-900 shadow-xl backdrop:bg-slate-950/35"
    >
      <div className="flex items-start justify-between border-b border-slate-100 p-6">
        <div>
          <div className="mb-4 flex size-10 items-center justify-center rounded-xl bg-indigo-50 text-indigo-600">
            <Building2 aria-hidden="true" className="size-5" />
          </div>
          <h2 id="create-title" className="text-lg font-semibold tracking-tight">
            New organization
          </h2>
          <p className="mt-1.5 text-sm text-slate-500">Create a workspace for a customer or business.</p>
        </div>
        <Button
          variant="ghost"
          className="px-2"
          aria-label="Close dialog"
          disabled={saving}
          onClick={onCancel}
          icon={<X aria-hidden="true" className="size-4" />}
        />
      </div>
      <form
        onSubmit={(event) => {
          void submit(event);
        }}
      >
        <div className="space-y-5 p-6">
          <FormField
            label="Organization name"
            id="organization-name"
            autoFocus
            value={name}
            placeholder="e.g. Acme Ltd"
            onChange={(event) => setName(event.target.value)}
            required
            disabled={saving}
            error={validationError}
            helper="Use the name your team will recognize."
          />
          {serverError && <Alert title="Unable to create organization">{serverError}</Alert>}
        </div>
        <div className="flex flex-wrap justify-end gap-3 rounded-b-2xl border-t border-slate-100 bg-slate-50/60 px-6 py-4">
          <Button variant="secondary" onClick={onCancel}>
            Cancel
          </Button>
          <Button type="submit" loading={saving}>
            {saving ? "Creating…" : "Create Organization"}
          </Button>
        </div>
      </form>
    </dialog>
  );
}
