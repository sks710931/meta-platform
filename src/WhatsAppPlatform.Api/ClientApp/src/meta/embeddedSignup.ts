export interface MetaSignupConfiguration { appId: string; configurationId: string; graphApiVersion: string }
interface LoginResponse { authResponse?: { code?: string } }
interface FacebookSdk {
  init(options: { appId: string; version: string; xfbml: boolean; autoLogAppEvents: boolean }): void;
  login(callback: (response: LoginResponse) => void, options: {
    config_id: string; response_type: "code"; override_default_response_type: true; extras: { setup: Record<string, never> };
  }): void;
}
declare global { interface Window { FB?: FacebookSdk; fbAsyncInit?: () => void } }
export type SignupErrorKind = "cancelled" | "popupClosed" | "sdkFailure" | "providerFailure";
export class EmbeddedSignupError extends Error {
  constructor(readonly kind: SignupErrorKind) {
    super({ cancelled: "Meta signup was cancelled. You can start again.",
      popupClosed: "Meta did not return an authorization result. The popup may have closed or been blocked. Please try again.",
      sdkFailure: "Unable to load the Meta SDK. Check your connection and browser settings.",
      providerFailure: "Meta could not complete signup. Please try again." }[kind]);
  }
}
let loading: Promise<void> | null = null;
let initializedApp: string | null = null;
export function prepareEmbeddedSignup(config: MetaSignupConfiguration): Promise<void> {
  if (!loading) loading = new Promise<void>((resolve, reject) => {
    if (window.FB) { resolve(); return; }
    const script = document.createElement("script");
    script.src = "https://connect.facebook.net/en_US/sdk.js";
    script.async = true; script.crossOrigin = "anonymous";
    const timeout = window.setTimeout(() => fail(), 15000);
    function fail(): void {
      window.clearTimeout(timeout); loading = null; script.remove();
      reject(new EmbeddedSignupError("sdkFailure"));
    }
    window.fbAsyncInit = () => {
      window.clearTimeout(timeout);
      if (window.FB) resolve(); else fail();
    };
    script.onerror = fail;
    document.head.appendChild(script);
  });
  return loading.then(() => {
    if (!window.FB) throw new EmbeddedSignupError("sdkFailure");
    if (initializedApp !== config.appId) {
      window.FB.init({ appId: config.appId, version: config.graphApiVersion, xfbml: false, autoLogAppEvents: false });
      initializedApp = config.appId;
    }
  });
}

// Call directly from a user gesture, after preparing the SDK, to avoid blocked popups.
// Authorization code is returned transiently to the caller, never logged or persisted.
export function launchEmbeddedSignup(config: MetaSignupConfiguration, signal: AbortSignal): Promise<string> {
  return new Promise<string>((resolve, reject) => {
    const sdk = window.FB;
    if (!sdk || initializedApp !== config.appId) { reject(new EmbeddedSignupError("sdkFailure")); return; }
    let settled = false;
    let cancelled = false;
    const timer = window.setTimeout(() => fail(new EmbeddedSignupError("popupClosed")), 10 * 60 * 1000);
    function cleanup(): void {
      window.clearTimeout(timer); window.removeEventListener("message", onMessage); signal.removeEventListener("abort", onAbort);
    }
    function fail(error: Error): void { if (!settled) { settled = true; cleanup(); reject(error); } }
    function onAbort(): void { fail(new DOMException("Signup interrupted", "AbortError")); }
    function onMessage(event: MessageEvent<unknown>): void {
      if (!["https://www.facebook.com", "https://web.facebook.com"].includes(event.origin)) return;
      let value: unknown = event.data;
      if (typeof value === "string") { try { value = JSON.parse(value); } catch { return; } }
      if (typeof value !== "object" || value === null || !("type" in value) || value.type !== "WA_EMBEDDED_SIGNUP" || !("event" in value)) return;
      // Ignore asset hints and provider error text; backend discovers resources and UI uses safe messages.
      if (value.event === "CANCEL") { cancelled = true; fail(new EmbeddedSignupError("cancelled")); }
      if (value.event === "ERROR") fail(new EmbeddedSignupError("providerFailure"));
      // FINISH is not an authorization code; final-screen closure can still succeed.
    }
    window.addEventListener("message", onMessage); signal.addEventListener("abort", onAbort, { once: true });
    if (signal.aborted) { onAbort(); return; }
    try {
      sdk.login((response) => {
        if (settled) return;
        const code = response.authResponse?.code;
        if (typeof code === "string" && code.length > 0) { settled = true; cleanup(); resolve(code); }
        else fail(new EmbeddedSignupError(cancelled ? "cancelled" : "popupClosed"));
      }, { config_id: config.configurationId, response_type: "code", override_default_response_type: true, extras: { setup: {} } });
    } catch { fail(new EmbeddedSignupError("sdkFailure")); }
  });
}
