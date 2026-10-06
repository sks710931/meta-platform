import { useEffect, useRef, useState } from "react";
import type { FormEvent } from "react";
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
    const email = values.get("email"); const password = values.get("password");
    if (typeof email !== "string" || typeof password !== "string") return;
    const controller = new AbortController(); pending.current = controller;
    setLoading(true); setError(null);
    try { await login(email, password, controller.signal); await onAuthenticated(); }
    catch (reason: unknown) { if (!controller.signal.aborted) setError(reason instanceof Error ? reason.message : "Unable to sign in."); }
    finally { form.reset(); if (!controller.signal.aborted) setLoading(false); }
  }
  return <section><h2>Sign in</h2><form onSubmit={(event) => { void submit(event); }}>
    <label>Email <input name="email" type="email" autoComplete="username" required maxLength={256} disabled={loading} /></label>
    <label>Password <input name="password" type="password" autoComplete="current-password" required maxLength={1024} disabled={loading} /></label>
    <button disabled={loading}>{loading ? "Signing in…" : "Sign in"}</button>
    {error && <p role="alert">{error}</p>}
  </form></section>;
}
