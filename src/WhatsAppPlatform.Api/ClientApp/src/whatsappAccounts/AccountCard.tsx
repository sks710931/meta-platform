import { CalendarDays, MessageSquare, Phone, BadgeCheck } from "lucide-react";
import { DateTime } from "../components/DateTime";
import { StatusBadge } from "../components/StatusBadge";
import type { AccountView } from "./whatsappApi";
export function AccountCard({ account }: { account: AccountView }) {
  return (
    <article className="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-xs">
      <div className="p-5 sm:p-6">
        <div className="flex items-start gap-3">
          <div className="flex size-10 shrink-0 items-center justify-center rounded-xl bg-indigo-50 text-indigo-600">
            <MessageSquare aria-hidden="true" className="size-5" />
          </div>
          <div className="min-w-0 flex-1">
            <h3 className="break-words text-sm font-semibold text-slate-900">{account.displayName}</h3>
            <p className="mt-1.5 flex flex-wrap items-center gap-1.5 text-xs text-slate-500">
              <CalendarDays aria-hidden="true" className="size-3" />
              {account.connectedAt ? (
                <>
                  Connected <DateTime value={account.connectedAt} />
                </>
              ) : (
                "Not connected"
              )}
            </p>
          </div>
          <StatusBadge status={account.status} />
        </div>
      </div>
      <ul className="mx-5 mb-5 divide-y divide-slate-100 rounded-lg border border-slate-100 bg-slate-50/60 sm:mx-6 sm:mb-6">
        {account.phoneNumbers.map((phone) => (
          <li key={phone.phoneNumberId} className="flex flex-wrap items-center gap-3 p-3.5">
            <Phone aria-hidden="true" className="size-4 shrink-0 text-slate-400" />
            <div className="min-w-0 flex-1">
              <p className="break-words text-sm font-medium text-slate-800">{phone.displayPhoneNumber}</p>
              {phone.verifiedName && (
                <p className="mt-1 flex items-center gap-1 text-xs text-slate-500">
                  <BadgeCheck aria-hidden="true" className="size-3.5 shrink-0" />
                  <span className="break-words">{phone.verifiedName}</span>
                </p>
              )}
            </div>
            <StatusBadge status={phone.status} />
          </li>
        ))}
      </ul>
      <div className="border-t border-slate-100 px-5 py-3 text-xs text-slate-500 sm:px-6">
        {account.phoneNumbers.length} {account.phoneNumbers.length === 1 ? "phone number" : "phone numbers"}
      </div>
    </article>
  );
}
