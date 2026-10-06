import { apiFetch } from "../auth/apiFetch";
import type { MetaSignupConfiguration } from "./embeddedSignup";

export async function getMetaConfiguration(signal: AbortSignal): Promise<MetaSignupConfiguration | null> {
  const response = await apiFetch("/api/whatsapp/embedded-signup/configuration", { signal });
  if (!response.ok) throw new Error("Unable to load Meta signup configuration.");
  const value: unknown = await response.json();
  if (typeof value !== "object" || value === null || !("enabled" in value)) throw new Error("Invalid configuration response.");
  if (value.enabled === false) return null;
  if (value.enabled !== true || !("appId" in value) || typeof value.appId !== "string" ||
    !("configurationId" in value) || typeof value.configurationId !== "string" ||
    !("graphApiVersion" in value) || typeof value.graphApiVersion !== "string") throw new Error("Invalid configuration response.");
  return { appId: value.appId, configurationId: value.configurationId, graphApiVersion: value.graphApiVersion };
}

export class MetaCompletionError extends Error {}
export async function completeMetaSession(sessionId: string, authorizationCode: string | null, signal: AbortSignal): Promise<void> {
  const response = await apiFetch(`/api/whatsapp/onboarding-sessions/${encodeURIComponent(sessionId)}/meta-complete`, {
    method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ authorizationCode }), signal,
  });
  if (response.ok) return;
  const problem: unknown = await response.json().catch(() => null);
  const restartRequired = typeof problem === "object" && problem !== null &&
    "onboardingError" in problem && problem.onboardingError === "restart_required";
  // Fixed status-based messages: no Graph response or secret-bearing provider exception is rendered.
  const message = response.status === 401 ? "Your application session expired. Please sign in."
    : response.status === 403 ? "Organization administrator access is required."
    : response.status === 404 ? "Onboarding session is unavailable."
    : response.status === 410 ? "Onboarding session expired. Start a new session."
    : response.status === 422 ? "Meta could not verify one authorized account with phone numbers. Retry the saved result or start again selecting one account."
    : response.status === 503 ? "Meta onboarding is unavailable. Retry the saved result shortly."
    : response.status === 400 ? "The authorization result was invalid. Start a new signup."
    : restartRequired ? "Meta authorization could not be completed or safely saved. Start a new signup; this code will not be retried."
    : response.status === 409 ? "Signup is processing or conflicts with an existing result. Retry the saved result, or start a new session."
    : "Onboarding failed. Retry the saved result; if no credential was saved, start a new signup.";
  throw new MetaCompletionError(message);
}
