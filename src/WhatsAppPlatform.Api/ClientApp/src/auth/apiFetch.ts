export async function apiFetch(url: string, options: RequestInit = {}): Promise<Response> {
  const method = (options.method ?? "GET").toUpperCase();
  const headers = new Headers(options.headers);
  if (!["GET", "HEAD", "OPTIONS"].includes(method)) {
    const tokenResponse = await fetch("/api/auth/csrf", { credentials: "same-origin", signal: options.signal ?? null });
    if (!tokenResponse.ok) throw new Error("Unable to protect the request. Please retry.");
    const body: unknown = await tokenResponse.json();
    if (typeof body !== "object" || body === null || !("requestToken" in body) || typeof body.requestToken !== "string")
      throw new Error("Invalid antiforgery response.");
    headers.set("X-CSRF-TOKEN", body.requestToken);
  }
  const response = await fetch(url, { ...options, headers, credentials: "same-origin" });
  if (response.status === 401 && url !== "/api/auth/login") window.dispatchEvent(new Event("platform-session-expired"));
  return response;
}
