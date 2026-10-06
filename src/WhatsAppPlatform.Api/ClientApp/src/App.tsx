import { useState } from "react";
import { CreateOrganization } from "./organizations/CreateOrganization";
import { OrganizationDetails } from "./organizations/OrganizationDetails";
import { OrganizationsList } from "./organizations/OrganizationsList";

type View = { kind: "list" } | { kind: "create" } | { kind: "details"; organizationId: string };

export function App() {
  const [view, setView] = useState<View>({ kind: "list" });
  const [created, setCreated] = useState(false);
  function showDetails(organizationId: string) {
    setCreated(false);
    setView({ kind: "details", organizationId });
  }
  return (
    <main>
      <h1>WhatsApp Platform</h1>
      {created && <p role="status">Organization created successfully.</p>}
      {view.kind === "list" && <OrganizationsList onCreate={() => { setCreated(false); setView({ kind: "create" }); }} onSelect={showDetails} />}
      {view.kind === "create" && <CreateOrganization onCancel={() => setView({ kind: "list" })}
        onCreated={(organizationId) => { setCreated(true); setView({ kind: "details", organizationId }); }} />}
      {view.kind === "details" && <OrganizationDetails organizationId={view.organizationId}
        onBack={() => { setCreated(false); setView({ kind: "list" }); }} />}
    </main>
  );
}
