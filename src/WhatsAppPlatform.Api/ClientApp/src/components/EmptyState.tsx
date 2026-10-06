import type { ReactNode } from "react";
interface Props {
  icon: ReactNode;
  title: string;
  children: ReactNode;
  action?: ReactNode;
}
export function EmptyState({ icon, title, children, action }: Props) {
  return (
    <div className="flex flex-col items-center rounded-xl border border-dashed border-slate-300 bg-white px-6 py-14 text-center">
      <div
        aria-hidden="true"
        className="mb-5 flex size-14 items-center justify-center rounded-2xl bg-slate-100 text-slate-500"
      >
        {icon}
      </div>
      <h3 className="text-base font-semibold tracking-tight text-slate-900">{title}</h3>
      <p className="mt-2 max-w-md text-sm leading-6 text-slate-500">{children}</p>
      {action && <div className="mt-6">{action}</div>}
    </div>
  );
}
