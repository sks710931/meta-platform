import type { ButtonHTMLAttributes, ReactNode } from "react";
import { Spinner } from "./Spinner";
const variants = {
  primary: "border-indigo-600 bg-indigo-600 text-white shadow-sm hover:border-indigo-700 hover:bg-indigo-700",
  secondary: "border-slate-200 bg-white text-slate-700 shadow-sm hover:border-slate-300 hover:bg-slate-50",
  danger: "border-rose-200 bg-rose-50 text-rose-700 hover:bg-rose-100",
  ghost: "border-transparent bg-transparent text-slate-600 hover:bg-slate-100 hover:text-slate-900",
};
interface Props extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: keyof typeof variants;
  size?: "normal" | "small";
  loading?: boolean;
  icon?: ReactNode;
}
export function Button({
  variant = "primary",
  size = "normal",
  loading = false,
  icon,
  children,
  className = "",
  disabled,
  type = "button",
  ...props
}: Props) {
  return (
    <button
      {...props}
      type={type}
      disabled={disabled || loading}
      aria-busy={loading || undefined}
      className={`inline-flex shrink-0 items-center justify-center gap-2 rounded-lg border font-medium transition-colors disabled:cursor-not-allowed disabled:opacity-50 ${size === "small" ? "min-h-9 px-3 text-xs" : "min-h-10 px-4 py-2 text-sm"} ${variants[variant]} ${className}`}
    >
      {loading ? <Spinner /> : icon}
      {children}
    </button>
  );
}
