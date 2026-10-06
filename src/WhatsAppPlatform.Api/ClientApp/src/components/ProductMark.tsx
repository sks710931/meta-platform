import { MessageCircleMore } from "lucide-react";
export function ProductMark() {
  return (
    <div className="flex items-center gap-3">
      <div className="flex size-10 shrink-0 items-center justify-center rounded-xl bg-indigo-600 text-white shadow-sm">
        <MessageCircleMore aria-hidden="true" className="size-6" />
      </div>
      <div>
        <p className="text-sm font-semibold tracking-tight text-slate-950">WhatsApp Platform</p>
        <p className="mt-0.5 text-xs text-slate-500">Administration console</p>
      </div>
    </div>
  );
}
