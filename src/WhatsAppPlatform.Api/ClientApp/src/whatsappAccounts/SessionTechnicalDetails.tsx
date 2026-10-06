import { ChevronDown } from "lucide-react";
import { DateTime } from "../components/DateTime";
import type { SessionView } from "./whatsappApi";
export function SessionTechnicalDetails({ session }: { session: SessionView }) {
  return (
    <details className="group border-t border-slate-100 pt-4">
      <summary className="flex list-none items-center gap-2 text-xs font-medium text-slate-500">
        <ChevronDown aria-hidden="true" className="size-3.5 group-open:rotate-180" />
        Technical details
      </summary>
      <dl className="mt-4 grid gap-3 rounded-lg bg-slate-50 p-4 text-xs sm:grid-cols-2">
        <div>
          <dt className="text-slate-500">Session ID</dt>
          <dd className="mt-1.5 break-all font-mono text-slate-700">{session.sessionId}</dd>
        </div>
        <div>
          <dt className="text-slate-500">Status</dt>
          <dd className="mt-1.5 text-slate-700">{session.status}</dd>
        </div>
        <div>
          <dt className="text-slate-500">Created</dt>
          <dd className="mt-1.5 text-slate-700">
            <DateTime value={session.startedAt} format="dateTime" />
          </dd>
        </div>
        <div>
          <dt className="text-slate-500">Expires</dt>
          <dd className="mt-1.5 text-slate-700">
            <DateTime value={session.expiresAt} format="dateTime" />
          </dd>
        </div>
        {session.completedAt && (
          <div>
            <dt className="text-slate-500">Completed</dt>
            <dd className="mt-1.5 text-slate-700">
              <DateTime value={session.completedAt} format="dateTime" />
            </dd>
          </div>
        )}
      </dl>
    </details>
  );
}
