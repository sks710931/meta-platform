import { useEffect, useRef, useState } from "react";
import type { FormEvent } from "react";
import { ArrowRight, LockKeyhole } from "lucide-react";
import { Alert } from "../components/Alert";
import { Button } from "../components/Button";
import { FormField } from "../components/FormField";
import { ProductMark } from "../components/ProductMark";
import { login } from "./authApi";

export function Login({ onAuthenticated }: { onAuthenticated: () => Promise<void> }) {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const pending = useRef<AbortController | null>(null);
  useEffect(() => () => pending.current?.abort(), []);
  async function submit(event: FormEvent<HTMLFormElement>): Promise<void> {
    event.preventDefault();
    const form = event.currentTarget;
    const values = new FormData(form);
    const email = values.get("email");
    const password = values.get("password");
    if (typeof email !== "string" || typeof password !== "string") return;
    const controller = new AbortController();
    pending.current = controller;
    setLoading(true);
    setError(null);
    try {
      await login(email, password, controller.signal);
      await onAuthenticated();
    } catch (reason: unknown) {
      if (!controller.signal.aborted) setError(reason instanceof Error ? reason.message : "Unable to sign in.");
    } finally {
      form.reset();
      if (!controller.signal.aborted) setLoading(false);
    }
  }
  return (
    <main className="flex min-h-dvh items-center justify-center px-4 py-12">
      <div className="w-full max-w-[440px]">
        <div className="mb-8 flex justify-center">
          <ProductMark />
        </div>
        <section
          aria-labelledby="login-title"
          className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm sm:p-9"
        >
          <h1 id="login-title" className="text-2xl font-semibold tracking-tight text-slate-950">
            Welcome back
          </h1>
          <p className="mt-2 text-sm leading-6 text-slate-500">
            Sign in to manage your organizations and connected WhatsApp accounts.
          </p>
          <form
            className="mt-7 space-y-5"
            onSubmit={(event) => {
              void submit(event);
            }}
          >
            <FormField
              label="Email address"
              name="email"
              type="email"
              autoComplete="username"
              placeholder="you@company.com"
              required
              maxLength={256}
              disabled={loading}
            />
            <FormField
              label="Password"
              name="password"
              type="password"
              autoComplete="current-password"
              placeholder="Enter your password"
              required
              maxLength={1024}
              disabled={loading}
            />
            {error && <Alert title="Unable to sign in">{error}</Alert>}
            <Button
              type="submit"
              className="w-full"
              loading={loading}
              icon={<ArrowRight aria-hidden="true" className="size-4" />}
            >
              {loading ? "Signing in…" : "Sign in"}
            </Button>
          </form>
        </section>
        <p className="mt-6 flex items-center justify-center gap-2 text-xs text-slate-500">
          <LockKeyhole aria-hidden="true" className="size-3.5" />
          Secure access to your workspace
        </p>
      </div>
    </main>
  );
}
