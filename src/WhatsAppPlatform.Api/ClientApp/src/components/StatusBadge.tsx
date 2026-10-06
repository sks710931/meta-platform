const styles: Record<string, string> = {
  Active: "bg-emerald-50 text-emerald-700 ring-emerald-600/15",
  Connected: "bg-emerald-50 text-emerald-700 ring-emerald-600/15",
  Registered: "bg-blue-50 text-blue-700 ring-blue-600/15",
  Completed: "bg-emerald-50 text-emerald-700 ring-emerald-600/15",
  Pending: "bg-amber-50 text-amber-800 ring-amber-600/15",
  Suspended: "bg-amber-50 text-amber-800 ring-amber-600/15",
  Disconnected: "bg-slate-100 text-slate-600 ring-slate-600/15",
  Expired: "bg-slate-100 text-slate-600 ring-slate-600/15",
  Failed: "bg-rose-50 text-rose-700 ring-rose-600/15",
};
export function StatusBadge({ status }: { status: string }) {
  return (
    <span
      className={`inline-flex items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-medium ring-1 ring-inset ${styles[status] ?? styles.Disconnected}`}
    >
      <span aria-hidden="true" className="size-1.5 rounded-full bg-current opacity-70" />
      {status}
    </span>
  );
}
