import { useEffect, useRef, useState } from "react";
import type { ReactNode } from "react";
import { Building2, Menu, X } from "lucide-react";
import { useCurrentUser, useSessionActions } from "../auth/AuthenticationShell";
import { Sidebar } from "./Sidebar";
import type { NavigationItem } from "./Sidebar";
import { Button } from "./Button";
import { Alert } from "./Alert";
interface Props {
  title: string;
  context?: string | undefined;
  active: NavigationItem;
  accountsAvailable: boolean;
  onNavigate: (item: NavigationItem) => void;
  children: ReactNode;
}
export function AppShell({ title, context, active, accountsAvailable, onNavigate, children }: Props) {
  const user = useCurrentUser();
  const { signOut, signingOut, error } = useSessionActions();
  const [menuOpen, setMenuOpen] = useState(false);
  const dialog = useRef<HTMLDialogElement>(null);
  useEffect(() => {
    if (menuOpen) dialog.current?.showModal();
    else dialog.current?.close();
  }, [menuOpen]);
  useEffect(() => {
    const desktop = window.matchMedia("(min-width: 64rem)");
    const closeOnDesktop = () => {
      if (desktop.matches) setMenuOpen(false);
    };
    desktop.addEventListener("change", closeOnDesktop);
    return () => desktop.removeEventListener("change", closeOnDesktop);
  }, []);
  function navigate(item: NavigationItem): void {
    onNavigate(item);
    setMenuOpen(false);
  }
  const sidebar = (
    <Sidebar
      user={user}
      active={active}
      accountsAvailable={accountsAvailable}
      onNavigate={navigate}
      onLogout={() => {
        void signOut();
      }}
      signingOut={signingOut}
    />
  );
  return (
    <div className="min-h-dvh lg:pl-64">
      <a
        href="#main-content"
        className="sr-only focus:not-sr-only focus:fixed focus:top-3 focus:left-3 focus:z-50 focus:rounded-lg focus:bg-white focus:p-3"
      >
        Skip to content
      </a>
      <aside className="fixed inset-y-0 left-0 z-30 hidden w-64 border-r border-slate-200 lg:block">{sidebar}</aside>
      <dialog
        ref={dialog}
        onCancel={() => setMenuOpen(false)}
        aria-label="Navigation drawer"
        className="fixed inset-y-0 left-0 m-0 h-dvh max-h-none w-72 max-w-[85vw] border-0 bg-white p-0 shadow-xl backdrop:bg-slate-950/35 lg:hidden"
      >
        <Button
          variant="ghost"
          className="absolute top-2 right-2 px-2"
          aria-label="Close navigation"
          onClick={() => setMenuOpen(false)}
          icon={<X aria-hidden="true" className="size-4" />}
        />
        {sidebar}
      </dialog>
      <header className="sticky top-0 z-20 flex min-h-18 items-center gap-3 border-b border-slate-200 bg-white px-4 sm:px-8">
        <Button
          variant="ghost"
          className="px-2 lg:hidden"
          aria-label="Open navigation"
          aria-expanded={menuOpen}
          onClick={() => setMenuOpen(true)}
          icon={<Menu aria-hidden="true" className="size-5" />}
        />
        <p className="min-w-0 flex-1 truncate text-sm font-semibold text-slate-700">{title}</p>
        {context && (
          <span
            title={context}
            className="flex max-w-[50%] items-center gap-2 rounded-lg border border-slate-200 px-3 py-1.5 text-xs text-slate-600"
          >
            <Building2 aria-hidden="true" className="size-3.5 shrink-0" />
            <span className="truncate">{context}</span>
          </span>
        )}
        <span title={user.email} className="hidden max-w-48 truncate text-xs text-slate-500 xl:block">
          {user.email}
        </span>
      </header>
      <main
        id="main-content"
        tabIndex={-1}
        className="mx-auto max-w-7xl space-y-6 px-4 py-7 outline-none sm:px-8 sm:py-9"
      >
        {error && <Alert title="Unable to sign out">{error}</Alert>}
        {children}
      </main>
    </div>
  );
}
