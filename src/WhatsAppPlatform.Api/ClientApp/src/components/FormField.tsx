import { useId } from "react";
import type { InputHTMLAttributes } from "react";
interface Props extends InputHTMLAttributes<HTMLInputElement> {
  label: string;
  error?: string | null;
  helper?: string;
}
export function FormField({ label, error, helper, id, className = "", ...props }: Props) {
  const generated = useId();
  const inputId = id ?? generated;
  return (
    <div className="space-y-2">
      <label htmlFor={inputId} className="block text-sm font-medium text-slate-700">
        {label}
      </label>
      <input
        {...props}
        id={inputId}
        aria-invalid={error ? true : undefined}
        aria-describedby={error || helper ? `${inputId}-description` : undefined}
        className={`block min-h-11 w-full rounded-lg border bg-white px-3 py-2.5 text-sm text-slate-900 shadow-xs outline-none placeholder:text-slate-400 focus:border-indigo-500 focus:ring-3 focus:ring-indigo-500/15 disabled:cursor-not-allowed disabled:bg-slate-50 disabled:text-slate-500 ${error ? "border-rose-400" : "border-slate-300"} ${className}`}
      />
      {(error || helper) && (
        <p
          id={`${inputId}-description`}
          role={error ? "alert" : undefined}
          className={`text-xs leading-5 ${error ? "text-rose-700" : "text-slate-500"}`}
        >
          {error ?? helper}
        </p>
      )}
    </div>
  );
}
