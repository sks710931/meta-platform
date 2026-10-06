import { Building2, ChevronRight, LogOut, MessageSquare, ShieldCheck, UserRound } from "lucide-react";
import type { CurrentUser } from "../auth/authApi";
import { ProductMark } from "./ProductMark";
import { Button } from "./Button";
export type NavigationItem = "organizations" | "accounts";
interface Props {
  user: CurrentUser;
  active: NavigationItem;
  accountsAvailable: boolean;
  onNavigate: (item: NavigationItem) => void;
  onLogout: () => void;
  signingOut: boolean;
}
export function Sidebar({ user, active, accountsAvailable, onNavigate, onLogout, signingOut }: Props) {
  return (
    <div className="flex h-full flex-col overflow-y-auto bg-white px-4 py-6">
      <div className="px-2">
        <ProductMark />
      </div>
      <p className="mt-10 px-3 text-[10px] font-semibold tracking-widest text-slate-400 uppercase">Workspace</p>
      <nav aria-label="Main navigation" className="mt-3 space-y-1">
        {(
          [
            { id: "organizations", label: "Organizations", icon: Building2 },
            { id: "accounts", label: "WhatsApp Accounts", icon: MessageSquare },
          ] satisfies { id: NavigationItem; label: string; icon: typeof Building2 }[]
        ).map(({ id, label, icon: Icon }) => (
          <button
            key={id}
            onClick={() => onNavigate(id)}
            disabled={id === "accounts" && !accountsAvailable}
            title={
              id === "accounts" && !accountsAvailable ? "Select an organization to manage its accounts" : undefined
            }
            aria-current={active === id ? "page" : undefined}
            className={`flex min-h-11 w-full items-center gap-3 rounded-lg px-3 text-left text-sm font-medium disabled:cursor-not-allowed disabled:text-slate-400 ${active === id ? "bg-indigo-50 text-indigo-700" : "text-slate-600 hover:bg-slate-50 disabled:hover:bg-transparent"}`}
          >
            <Icon aria-hidden="true" className="size-4 shrink-0" />
            <span className="flex-1">{label}</span>
            {active === id && <ChevronRight aria-hidden="true" className="size-3.5" />}
          </button>
        ))}
      </nav>
      {!accountsAvailable && (
        <p className="mt-3 px-3 text-xs leading-5 text-slate-400">
          Account management is available inside an organization.
        </p>
      )}
      <div className="mt-auto border-t border-slate-100 pt-5">
        <div className="mb-4 flex items-center gap-3 px-2">
          <div className="flex size-9 shrink-0 items-center justify-center rounded-full bg-slate-100 text-slate-500">
            <UserRound aria-hidden="true" className="size-4" />
          </div>
          <div className="min-w-0">
            <p title={user.email} className="truncate text-xs font-medium text-slate-800">
              {user.email}
            </p>
            <p className="mt-1 flex items-center gap-1 text-[11px] text-slate-500">
              {user.isPlatformAdmin && <ShieldCheck aria-hidden="true" className="size-3" />}
              {user.isPlatformAdmin ? "Platform administrator" : "Organization workspace"}
            </p>
          </div>
        </div>
        <Button
          variant="ghost"
          className="w-full justify-start"
          loading={signingOut}
          icon={<LogOut aria-hidden="true" className="size-4" />}
          onClick={onLogout}
        >
          {signingOut ? "Signing out…" : "Log out"}
        </Button>
      </div>
    </div>
  );
}
