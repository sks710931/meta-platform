import { ArrowLeft, Building2, CalendarDays } from "lucide-react";
import { Alert } from "../components/Alert";
import { Button } from "../components/Button";
import { DateTime } from "../components/DateTime";
import { Skeleton } from "../components/Skeleton";
import { StatusBadge } from "../components/StatusBadge";
import { useEffect, useState } from "react";
import { WhatsAppAccountsSection } from "../whatsappAccounts/WhatsAppAccountsSection";
import { errorMessage, getOrganization } from "./organizationsApi";
import type { Organization } from "./organizationsApi";

interface Props {
  organizationId: string;
  onBack: () => void;
  onContext: (name: string) => void;
}

export function OrganizationDetails({ organizationId, onBack, onContext }: Props) {
  const [organization, setOrganization] = useState<Organization | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [attempt, setAttempt] = useState(0);

  useEffect(() => {
    const controller = new AbortController();
    setLoading(true);
    setError(null);
    setOrganization(null);
    getOrganization(organizationId, controller.signal)
      .then((item) => {
        if (!controller.signal.aborted) {
          setOrganization(item);
          onContext(item.name);
        }
      })
      .catch((reason: unknown) => {
        if (!controller.signal.aborted) setError(errorMessage(reason));
      })
      .finally(() => {
        if (!controller.signal.aborted) setLoading(false);
      });
    return () => controller.abort();
  }, [organizationId, attempt, onContext]);

  return (
    <section className="space-y-7">
      <Button
        variant="ghost"
        size="small"
        className="-ml-3"
        onClick={onBack}
        icon={<ArrowLeft aria-hidden="true" className="size-4" />}
      >
        Organizations
      </Button>
      {loading && (
        <div role="status" aria-label="Loading organization" className="space-y-6">
          <span className="sr-only">Loading organization…</span>
          <Skeleton className="h-9 w-56" />
          <Skeleton className="h-28 w-full" />
          <Skeleton className="h-56 w-full" />
        </div>
      )}
      {error && (
        <Alert
          title="Unable to load organization"
          action={
            <Button variant="secondary" size="small" onClick={() => setAttempt(attempt + 1)}>
              Try again
            </Button>
          }
        >
          {error}
        </Alert>
      )}
      {!loading && organization && (
        <>
          <div className="flex flex-wrap items-center gap-3">
            <div className="flex size-12 items-center justify-center rounded-xl border border-slate-200 bg-white text-slate-500 shadow-xs">
              <Building2 aria-hidden="true" className="size-5" />
            </div>
            <h1 className="min-w-0 break-words text-2xl font-semibold tracking-tight">{organization.name}</h1>
            <StatusBadge status={organization.status} />
          </div>
          <div className="grid gap-6 rounded-xl border border-slate-200 bg-white p-5 shadow-xs sm:grid-cols-2 sm:p-6">
            <div>
              <p className="text-xs font-medium text-slate-500">Organization ID</p>
              <p className="mt-2 break-all font-mono text-xs text-slate-700">{organization.organizationId}</p>
            </div>
            <div>
              <p className="text-xs font-medium text-slate-500">Created</p>
              <p className="mt-2 flex items-center gap-2 text-sm text-slate-700">
                <CalendarDays aria-hidden="true" className="size-3.5 text-slate-400" />
                <DateTime value={organization.createdAt} format="dateTime" />
              </p>
            </div>
          </div>
          <WhatsAppAccountsSection key={organization.organizationId} organizationId={organization.organizationId} />
        </>
      )}
    </section>
  );
}
