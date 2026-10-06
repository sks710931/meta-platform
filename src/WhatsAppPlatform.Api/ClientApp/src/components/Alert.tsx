import { AlertCircle, CheckCircle2, Info } from "lucide-react";
import type { ReactNode } from "react";
interface Props {
  children: ReactNode;
  title?: string;
  tone?: "error" | "success" | "info";
  action?: ReactNode;
}
export function Alert({ children, title, tone = "error", action }: Props) {
  const Icon = tone === "success" ? CheckCircle2 : tone === "info" ? Info : AlertCircle;
  return (
    <div
      role={tone === "error" ? "alert" : "status"}
      className={`flex items-start gap-3 rounded-xl border p-4 text-sm ${tone === "success" ? "border-emerald-200 bg-emerald-50 text-emerald-800" : tone === "info" ? "border-blue-200 bg-blue-50 text-blue-800" : "border-rose-200 bg-rose-50/60 text-rose-800"}`}
    >
      <Icon aria-hidden="true" className="mt-0.5 size-4 shrink-0" />
      <div className="min-w-0 flex-1">
        {title && <p className="mb-1 font-semibold">{title}</p>}
        <div className="break-words leading-relaxed">{children}</div>
        {action && <div className="mt-3">{action}</div>}
      </div>
    </div>
  );
}
