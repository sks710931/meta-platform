import { apiFetch } from "./apiFetch";

export interface UserOrganization { organizationId: string; organizationName: string; role: "OrganizationAdmin" | "Member" }
export interface CurrentUser { userId: string; email: string; isPlatformAdmin: boolean; organizations: UserOrganization[] }

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === "object" && value !== null && !Array.isArray(value);
}
function record(value: unknown): Record<string, unknown> {
  if (!isRecord(value)) throw new Error("Invalid user response.");
  return value;
}
export async function currentUser(signal: AbortSignal): Promise<CurrentUser | null> {
  const response = await apiFetch("/api/auth/me", { signal });
  if (response.status === 401) return null;
  if (!response.ok) throw new Error("Unable to load your session. Please retry.");
  const value: unknown = await response.json();
  const user = record(value);
  if (typeof user.userId !== "string" || typeof user.email !== "string" || typeof user.isPlatformAdmin !== "boolean" ||
      !Array.isArray(user.organizations)) throw new Error("Invalid user response.");
  return { userId: user.userId, email: user.email, isPlatformAdmin: user.isPlatformAdmin,
    organizations: user.organizations.map((item: unknown) => {
      const org = record(item);
      if (typeof org.organizationId !== "string" || typeof org.organizationName !== "string" ||
          (org.role !== "Member" && org.role !== "OrganizationAdmin")) throw new Error("Invalid membership response.");
      return { organizationId: org.organizationId, organizationName: org.organizationName, role: org.role };
    }) };
}
export async function login(email: string, password: string, signal: AbortSignal): Promise<void> {
  const response = await apiFetch("/api/auth/login", { method: "POST", signal,
    headers: { "Content-Type": "application/json" }, body: JSON.stringify({ email, password }) });
  if (!response.ok) throw new Error(response.status === 401 ? "Invalid email or password."
    : response.status === 429 ? "Too many login attempts. Please wait and try again."
    : "Unable to sign in. Please try again.");
}
export async function logout(signal: AbortSignal): Promise<void> {
  const response = await apiFetch("/api/auth/logout", { method: "POST", signal });
  if (!response.ok && response.status !== 401) throw new Error("Unable to sign out. Please retry.");
}
