import { createContext, useContext, useEffect, useRef, useState } from "react";
import type { ReactNode } from "react";
import { currentUser, logout } from "./authApi";
import type { CurrentUser } from "./authApi";
import { Alert } from "../components/Alert";
import { Button } from "../components/Button";
import { ProductMark } from "../components/ProductMark";
import { Spinner } from "../components/Spinner";
import { Login } from "./Login";

interface SessionActions {
  signOut: () => Promise<void>;
  signingOut: boolean;
  error: string | null;
}
const SessionContext = createContext<SessionActions | null>(null);
export function useSessionActions(): SessionActions {
  const actions = useContext(SessionContext);
  if (!actions) throw new Error("Session actions require authentication.");
  return actions;
}
const UserContext = createContext<CurrentUser | null>(null);
export function useCurrentUser(): CurrentUser {
  const user = useContext(UserContext);
  if (!user) throw new Error("An authenticated view requires a user.");
  return user;
}
export function AuthenticationShell({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<CurrentUser | null>(null);
  const [loading, setLoading] = useState(true);
  const [signingOut, setSigningOut] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [attempt, setAttempt] = useState(0);
  const pending = useRef<AbortController | null>(null);
  useEffect(() => () => pending.current?.abort(), []);
  useEffect(() => {
    const controller = new AbortController();
    setLoading(true);
    setError(null);
    currentUser(controller.signal)
      .then((value) => {
        if (!controller.signal.aborted) setUser(value);
      })
      .catch((reason: unknown) => {
        if (!controller.signal.aborted)
          setError(reason instanceof Error ? reason.message : "Unable to load your session.");
      })
      .finally(() => {
        if (!controller.signal.aborted) setLoading(false);
      });
    const expire = () => {
      setUser(null);
      setError(null);
    };
    window.addEventListener("platform-session-expired", expire);
    return () => {
      controller.abort();
      window.removeEventListener("platform-session-expired", expire);
    };
  }, [attempt]);
  async function refresh(): Promise<void> {
    const controller = new AbortController();
    pending.current = controller;
    const value = await currentUser(controller.signal);
    if (!controller.signal.aborted) setUser(value);
  }
  async function signOut(): Promise<void> {
    const controller = new AbortController();
    pending.current = controller;
    setSigningOut(true);
    setError(null);
    try {
      await logout(controller.signal);
      if (!controller.signal.aborted) setUser(null);
    } catch (reason: unknown) {
      if (!controller.signal.aborted) setError(reason instanceof Error ? reason.message : "Unable to sign out.");
    } finally {
      if (!controller.signal.aborted) setSigningOut(false);
    }
  }
  if (loading)
    return (
      <div className="flex min-h-dvh flex-col items-center justify-center gap-8">
        <ProductMark />
        <p role="status" className="flex items-center gap-2 text-sm text-slate-500">
          <Spinner />
          Loading your workspace…
        </p>
      </div>
    );
  if (error && !user)
    return (
      <div className="mx-auto flex min-h-dvh max-w-md flex-col justify-center gap-6 px-6">
        <ProductMark />
        <Alert
          title="Unable to load your workspace"
          action={
            <Button variant="secondary" onClick={() => setAttempt((value) => value + 1)}>
              Try again
            </Button>
          }
        >
          {error}
        </Alert>
      </div>
    );
  if (!user) return <Login onAuthenticated={refresh} />;
  return (
    <UserContext.Provider value={user}>
      <SessionContext.Provider value={{ signOut, signingOut, error }}>{children}</SessionContext.Provider>
    </UserContext.Provider>
  );
}
