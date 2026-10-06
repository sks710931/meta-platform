import { ArrowRight, Building2, Plus, RefreshCw } from "lucide-react";
import { Alert } from "../components/Alert";
import { Button } from "../components/Button";
import { DateTime } from "../components/DateTime";
import { EmptyState } from "../components/EmptyState";
import { PageHeader } from "../components/PageHeader";
import { Skeleton } from "../components/Skeleton";
import { StatusBadge } from "../components/StatusBadge";
import { useCurrentUser } from "../auth/AuthenticationShell";
import { useEffect, useState } from "react";
import { errorMessage, listOrganizations } from "./organizationsApi";
import type { Organization } from "./organizationsApi";

interface Props {
  onCreate: () => void;
  onSelect: (organizationId: string) => void;
}

export function OrganizationsList({ onCreate, onSelect }: Props) {
  const user = useCurrentUser();
  const [organizations, setOrganizations] = useState<Organization[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [attempt, setAttempt] = useState(0);

  useEffect(() => {
    const controller = new AbortController();
    setLoading(true);
    setError(null);
    listOrganizations(controller.signal)
      .then((items) => {
        if (!controller.signal.aborted) setOrganizations(items);
      })
      .catch((reason: unknown) => {
        if (!controller.signal.aborted) setError(errorMessage(reason));
      })
      .finally(() => {
        if (!controller.signal.aborted) setLoading(false);
      });
    return () => controller.abort();
  }, [attempt]);

  const createButton = user.isPlatformAdmin ? (
    <Button onClick={onCreate} icon={<Plus aria-hidden="true" className="size-4" />}>
      New Organization
    </Button>
  ) : undefined;
  return (
    <section className="space-y-7">
      <PageHeader
        title="Organizations"
        subtitle="Manage customers and their connected WhatsApp Business accounts."
        action={createButton}
      />
      {loading && (
        <div
          role="status"
          aria-label="Loading organizations"
          className="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-xs"
        >
          <span className="sr-only">Loading organizations…</span>
          <div className="border-b border-slate-100 px-6 py-4">
            <Skeleton className="h-3 w-28" />
          </div>
          {[0, 1, 2, 3].map((row) => (
            <div key={row} className="flex gap-8 border-b border-slate-100 px-6 py-6 last:border-0">
              <Skeleton className="h-10 w-10 shrink-0" />
              <div className="flex-1 space-y-3">
                <Skeleton className="h-3 w-2/5" />
                <Skeleton className="h-2 w-1/4" />
              </div>
              <Skeleton className="h-6 w-16" />
            </div>
          ))}
        </div>
      )}
      {error && (
        <Alert
          title="Unable to load organizations"
          action={
            <Button
              variant="secondary"
              size="small"
              icon={<RefreshCw aria-hidden="true" className="size-3.5" />}
              onClick={() => setAttempt(attempt + 1)}
            >
              Try again
            </Button>
          }
        >
          {error}
        </Alert>
      )}
      {!loading &&
        !error &&
        (organizations.length === 0 ? (
          <EmptyState icon={<Building2 className="size-6" />} title="No organizations yet" action={createButton}>
            {user.isPlatformAdmin
              ? "Create your first organization to start connecting customer WhatsApp accounts."
              : "Organizations you belong to will appear here. Contact your administrator for access."}
          </EmptyState>
        ) : (
          <div className="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-xs">
            <div className="hidden grid-cols-[minmax(0,1fr)_120px_160px_40px] gap-4 border-b border-slate-200 bg-slate-50/60 px-6 py-3 text-xs font-medium text-slate-500 md:grid">
              <span>Organization</span>
              <span>Status</span>
              <span>Created</span>
              <span className="sr-only">Action</span>
            </div>
            <ul className="divide-y divide-slate-100">
              {organizations.map((organization) => (
                <li key={organization.organizationId}>
                  <button
                    onClick={() => onSelect(organization.organizationId)}
                    className="group grid w-full grid-cols-[minmax(0,1fr)_auto] items-center gap-4 px-5 py-5 text-left hover:bg-slate-50 focus-visible:-outline-offset-2 md:grid-cols-[minmax(0,1fr)_120px_160px_40px] md:px-6"
                  >
                    <div className="flex min-w-0 items-center gap-3">
                      <div className="flex size-10 shrink-0 items-center justify-center rounded-lg border border-slate-200 bg-slate-50 text-slate-500">
                        <Building2 aria-hidden="true" className="size-4" />
                      </div>
                      <div className="min-w-0">
                        <p className="truncate text-sm font-semibold text-slate-800 group-hover:text-indigo-700">
                          {organization.name}
                        </p>
                        <p className="mt-1 text-xs text-slate-500 md:hidden">
                          Created <DateTime value={organization.createdAt} />
                        </p>
                      </div>
                    </div>
                    <div>
                      <StatusBadge status={organization.status} />
                    </div>
                    <span className="hidden text-sm text-slate-500 md:block">
                      <DateTime value={organization.createdAt} />
                    </span>
                    <ArrowRight
                      aria-hidden="true"
                      className="hidden size-4 justify-self-end text-slate-400 group-hover:text-indigo-600 md:block"
                    />
                  </button>
                </li>
              ))}
            </ul>
            <div className="border-t border-slate-100 px-6 py-3 text-xs text-slate-500">
              {organizations.length} {organizations.length === 1 ? "organization" : "organizations"}
            </div>
          </div>
        ))}
    </section>
  );
}
