import { useEffect, useState } from "react";
import { WhatsAppAccountsSection } from "../whatsappAccounts/WhatsAppAccountsSection";
import { errorMessage, getOrganization } from "./organizationsApi";
import type { Organization } from "./organizationsApi";

interface Props {
  organizationId: string;
  onBack: () => void;
}

export function OrganizationDetails({ organizationId, onBack }: Props) {
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
      .then((item) => { if (!controller.signal.aborted) setOrganization(item); })
      .catch((reason: unknown) => { if (!controller.signal.aborted) setError(errorMessage(reason)); })
      .finally(() => { if (!controller.signal.aborted) setLoading(false); });
    return () => controller.abort();
  }, [organizationId, attempt]);

  return (
    <section>
      <h2>Organization details</h2>
      <button onClick={onBack}>Back to organizations</button>
      {loading && <p role="status">Loading organization…</p>}
      {error && <><p role="alert">{error}</p><button onClick={() => setAttempt(attempt + 1)}>Retry</button></>}
      {!loading && organization && (
        <>
        <dl>
          <dt>Id</dt><dd>{organization.organizationId}</dd>
          <dt>Name</dt><dd>{organization.name}</dd>
          <dt>Status</dt><dd>{organization.status}</dd>
          <dt>CreatedAt (UTC)</dt><dd><time dateTime={organization.createdAt}>{new Date(organization.createdAt).toISOString()}</time></dd>
        </dl>
        <WhatsAppAccountsSection key={organization.organizationId} organizationId={organization.organizationId} />
        </>
      )}
    </section>
  );
}
