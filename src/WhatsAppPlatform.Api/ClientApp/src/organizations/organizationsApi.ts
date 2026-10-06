export interface Organization {
  organizationId: string;
  name: string;
  status: "Active" | "Suspended";
  createdAt: string;
}

export class ApiError extends Error {
  constructor(public readonly status: number, message: string) {
    super(message);
  }
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === "object" && value !== null && !Array.isArray(value);
}

function parseOrganization(value: unknown): Organization {
  if (!isRecord(value) || typeof value.organizationId !== "string" ||
      typeof value.name !== "string" || typeof value.createdAt !== "string" ||
      Number.isNaN(Date.parse(value.createdAt)) ||
      (value.status !== "Active" && value.status !== "Suspended")) {
    throw new Error("The server returned an invalid organization response.");
  }
  return {
    organizationId: value.organizationId,
    name: value.name,
    status: value.status,
    createdAt: value.createdAt,
  };
}

async function readResponse(response: Response): Promise<unknown> {
  if (!response.ok) {
    let message = response.status === 404 ? "Organization not found." : "The request failed. Please try again.";
    if (response.status === 400) {
      message = "Please enter a valid organization name.";
      const body: unknown = await response.json().catch(() => undefined);
      if (isRecord(body) && isRecord(body.errors)) {
        const errors = Object.values(body.errors).flatMap((value: unknown) =>
          Array.isArray(value) ? value.filter((item: unknown): item is string => typeof item === "string") : []);
        if (errors.length > 0) message = errors.join(" ");
      }
    }
    throw new ApiError(response.status, message);
  }
  const body: unknown = await response.json();
  return body;
}

export async function listOrganizations(signal: AbortSignal): Promise<Organization[]> {
  const body = await readResponse(await fetch("/api/organizations", { signal }));
  if (!Array.isArray(body)) throw new Error("The server returned an invalid organizations list.");
  return body.map((item: unknown) => parseOrganization(item));
}

export async function createOrganization(name: string, signal: AbortSignal): Promise<Organization> {
  const response = await fetch("/api/organizations", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ name }),
    signal,
  });
  return parseOrganization(await readResponse(response));
}

export async function getOrganization(organizationId: string, signal: AbortSignal): Promise<Organization> {
  const response = await fetch(`/api/organizations/${encodeURIComponent(organizationId)}`, { signal });
  return parseOrganization(await readResponse(response));
}

export function errorMessage(error: unknown): string {
  return error instanceof ApiError ? error.message : "Unable to load or save organizations. Please try again.";
}
