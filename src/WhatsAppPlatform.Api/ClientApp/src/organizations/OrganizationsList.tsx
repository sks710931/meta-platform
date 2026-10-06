import { useEffect, useState } from "react";
import { errorMessage, listOrganizations } from "./organizationsApi";
import type { Organization } from "./organizationsApi";

interface Props {
  onCreate: () => void;
  onSelect: (organizationId: string) => void;
}

export function OrganizationsList({ onCreate, onSelect }: Props) {
  const [organizations, setOrganizations] = useState<Organization[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [attempt, setAttempt] = useState(0);

  useEffect(() => {
    const controller = new AbortController();
    setLoading(true);
    setError(null);
    listOrganizations(controller.signal)
      .then((items) => { if (!controller.signal.aborted) setOrganizations(items); })
      .catch((reason: unknown) => { if (!controller.signal.aborted) setError(errorMessage(reason)); })
      .finally(() => { if (!controller.signal.aborted) setLoading(false); });
    return () => controller.abort();
  }, [attempt]);

  return (
    <section>
      <h2>Organizations</h2>
      <button onClick={onCreate}>Create organization</button>
      {loading && <p role="status">Loading organizations…</p>}
      {error && <><p role="alert">{error}</p><button onClick={() => setAttempt(attempt + 1)}>Retry</button></>}
      {!loading && !error && (organizations.length === 0 ? <p>No organizations yet.</p> : (
        <table>
          <thead><tr><th>Name</th><th>Status</th><th>Created date</th></tr></thead>
          <tbody>{organizations.map((organization) => (
            <tr key={organization.organizationId}>
              <td><button onClick={() => onSelect(organization.organizationId)}>{organization.name}</button></td>
              <td>{organization.status}</td>
              <td><time dateTime={organization.createdAt}>{new Date(organization.createdAt).toLocaleDateString()}</time></td>
            </tr>
          ))}</tbody>
        </table>
      ))}
    </section>
  );
}
