import { Check } from "lucide-react";
import type { SessionView } from "./whatsappApi";
interface Props {
  status: SessionView["status"];
  phase: "ready" | "popup" | "verifying";
}
export function OnboardingProgress({ status, phase }: Props) {
  const completed = status === "Completed";
  const activeStep = completed ? 4 : phase === "verifying" ? 2 : 1;
  return (
    <ol aria-label="Onboarding progress" className="grid grid-cols-2 gap-4 sm:grid-cols-4">
      {["Session created", "Connect with Meta", "Verify account", "Connected"].map((label, index) => (
        <li
          key={label}
          aria-current={!completed && status === "Pending" && index === activeStep ? "step" : undefined}
          className="flex items-center gap-2.5"
        >
          <span
            className={`flex size-7 shrink-0 items-center justify-center rounded-full text-xs font-semibold ${completed || index === 0 ? "bg-emerald-50 text-emerald-700" : index === activeStep && status === "Pending" ? "bg-indigo-600 text-white" : "bg-slate-100 text-slate-400"}`}
          >
            {completed || index === 0 ? <Check aria-hidden="true" className="size-3.5" /> : index + 1}
          </span>
          <span
            className={`text-xs leading-5 ${index === activeStep ? "font-semibold text-slate-800" : "text-slate-500"}`}
          >
            {label}
          </span>
        </li>
      ))}
    </ol>
  );
}
