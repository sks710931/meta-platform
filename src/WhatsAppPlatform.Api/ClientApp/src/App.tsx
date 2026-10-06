import { AuthenticationShell } from "./auth/AuthenticationShell";
import { useState } from "react";
import { CreateOrganization } from "./organizations/CreateOrganization";
import { OrganizationDetails } from "./organizations/OrganizationDetails";
import { OrganizationsList } from "./organizations/OrganizationsList";
import { AppShell } from "./components/AppShell";
import { Alert } from "./components/Alert";
import type { NavigationItem } from "./components/Sidebar";

type View = { kind: "list" } | { kind: "create" } | { kind: "details"; organizationId: string };
export function App() {
  return (
    <AuthenticationShell>
      <AuthenticatedApp />
    </AuthenticationShell>
  );
}
function AuthenticatedApp() {
  const [view, setView] = useState<View>({ kind: "list" });
  const [created, setCreated] = useState(false);
  const [organizationName, setOrganizationName] = useState("");
  const [active, setActive] = useState<NavigationItem>("organizations");
  function showDetails(organizationId: string): void {
    setCreated(false);
    setOrganizationName("");
    setActive("organizations");
    setView({ kind: "details", organizationId });
  }
  function showOrganizations(): void {
    setCreated(false);
    setActive("organizations");
    setView({ kind: "list" });
  }
  function navigate(item: NavigationItem): void {
    if (item === "organizations") showOrganizations();
    else if (view.kind === "details") {
      setActive("accounts");
      const section = document.getElementById("whatsapp-accounts");
      section?.scrollIntoView({ block: "start" });
      section?.focus({ preventScroll: true });
    }
  }
  return (
    <AppShell
      title={view.kind === "details" ? "Organization overview" : "Workspace overview"}
      context={view.kind === "details" ? organizationName || "Organization" : undefined}
      active={active}
      accountsAvailable={view.kind === "details"}
      onNavigate={navigate}
    >
      {created && (
        <Alert tone="success" title="Organization created">
          Your organization is ready. You can now connect a WhatsApp account.
        </Alert>
      )}
      {view.kind !== "details" && (
        <OrganizationsList
          onCreate={() => {
            setCreated(false);
            setView({ kind: "create" });
          }}
          onSelect={showDetails}
        />
      )}
      {view.kind === "create" && (
        <CreateOrganization
          onCancel={showOrganizations}
          onCreated={(organizationId) => {
            setCreated(true);
            setOrganizationName("");
            setView({ kind: "details", organizationId });
          }}
        />
      )}
      {view.kind === "details" && (
        <OrganizationDetails
          organizationId={view.organizationId}
          onBack={showOrganizations}
          onContext={setOrganizationName}
        />
      )}
    </AppShell>
  );
}
