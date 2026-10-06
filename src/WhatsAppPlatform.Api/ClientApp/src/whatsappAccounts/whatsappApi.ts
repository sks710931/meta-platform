import { apiFetch } from "../auth/apiFetch";
export interface PhoneNumberView { phoneNumberId: string; displayPhoneNumber: string; verifiedName: string | null; status: string }
export interface AccountView { whatsAppAccountId: string; displayName: string; status: string; connectedAt: string | null; phoneNumbers: PhoneNumberView[] }
export interface SessionView { sessionId: string; organizationId: string; status: "Pending" | "Completed" | "Failed" | "Expired"; startedAt: string; expiresAt: string; completedAt: string | null; manualCompletionAvailable: boolean }
export interface CompletionInput {
  externalWhatsAppAccountId: string; externalMessagingAccountId: string; displayName: string;
  phoneNumbers: { externalPhoneNumberId: string; displayPhoneNumber: string; verifiedName: string }[];
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === "object" && value !== null && !Array.isArray(value);
}
function record(value: unknown): Record<string, unknown> {
  if (!isRecord(value)) throw new Error("Invalid response.");
  return value;
}
function text(value: unknown): string { if (typeof value !== "string") throw new Error("Invalid response."); return value; }
function nullableText(value: unknown): string | null { return value === null ? null : text(value); }
function parseAccount(value: unknown): AccountView {
  const item = record(value);
  if (!Array.isArray(item.phoneNumbers)) throw new Error("Invalid response.");
  return { whatsAppAccountId: text(item.whatsAppAccountId), displayName: text(item.displayName), status: text(item.status),
    connectedAt: nullableText(item.connectedAt), phoneNumbers: item.phoneNumbers.map((raw: unknown) => {
      const phone = record(raw);
      return { phoneNumberId: text(phone.phoneNumberId), displayPhoneNumber: text(phone.displayPhoneNumber),
        verifiedName: nullableText(phone.verifiedName), status: text(phone.status) };
    }) };
}
function parseSession(value: unknown): SessionView {
  const item = record(value);
  if ((item.status !== "Pending" && item.status !== "Completed" && item.status !== "Failed" && item.status !== "Expired") ||
      typeof item.manualCompletionAvailable !== "boolean") throw new Error("Invalid response.");
  return { sessionId: text(item.sessionId), organizationId: text(item.organizationId), status: item.status,
    startedAt: text(item.startedAt), expiresAt: text(item.expiresAt), completedAt: nullableText(item.completedAt),
    manualCompletionAvailable: item.manualCompletionAvailable };
}
export class WhatsAppApiError extends Error {}
async function body(response: Response): Promise<unknown> {
  if (!response.ok) {
    const fallback = response.status === 403 ? "You do not have permission to perform this action."
      : response.status === 401 ? "Your session expired. Please sign in." : response.status >= 500 ? "The server could not process onboarding. Please retry."
      : `Onboarding request failed (${response.status}). Refresh the session or start again.`;
    const raw: unknown = await response.json().catch(() => undefined);
    let message = fallback;
    if (response.status < 500 && typeof raw === "object" && raw !== null && !Array.isArray(raw)) {
      const error = record(raw);
      if (typeof error.detail === "string") message = error.detail;
      else if (typeof error.errors === "object" && error.errors !== null && !Array.isArray(error.errors)) {
        const errors = Object.values(record(error.errors)).flatMap((item: unknown) =>
          Array.isArray(item) ? item.filter((entry: unknown): entry is string => typeof entry === "string") : []);
        if (errors.length > 0) message = errors.join(" ");
      }
    }
    throw new WhatsAppApiError(message);
  }
  const value: unknown = await response.json();
  return value;
}
export function whatsappError(reason: unknown): string {
  return reason instanceof WhatsAppApiError ? reason.message : "Unable to reach onboarding. Please retry.";
}
export async function listAccounts(organizationId: string, signal: AbortSignal): Promise<AccountView[]> {
  const value = await body(await apiFetch(`/api/organizations/${encodeURIComponent(organizationId)}/whatsapp-accounts`, { signal }));
  if (!Array.isArray(value)) throw new Error("Invalid response.");
  return value.map((item: unknown) => parseAccount(item));
}
export async function startSession(organizationId: string, signal: AbortSignal): Promise<SessionView> {
  return parseSession(await body(await apiFetch(`/api/organizations/${encodeURIComponent(organizationId)}/whatsapp/onboarding-sessions`, { method: "POST", signal })));
}
export async function getSession(sessionId: string, signal: AbortSignal): Promise<SessionView> {
  return parseSession(await body(await apiFetch(`/api/whatsapp/onboarding-sessions/${encodeURIComponent(sessionId)}`, { signal })));
}
export async function completeSession(sessionId: string, input: CompletionInput, signal: AbortSignal): Promise<void> {
  await body(await apiFetch(`/api/whatsapp/onboarding-sessions/${encodeURIComponent(sessionId)}/complete`, {
    method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify(input), signal,
  }));
}
