import { createContext, useContext, useEffect, useRef, useState } from "react";
import type { ReactNode } from "react";
import { currentUser, logout } from "./authApi";
import type { CurrentUser } from "./authApi";
import { Login } from "./Login";

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
    setLoading(true); setError(null);
    currentUser(controller.signal).then((value) => { if (!controller.signal.aborted) setUser(value); })
      .catch((reason: unknown) => { if (!controller.signal.aborted) setError(reason instanceof Error ? reason.message : "Unable to load your session."); })
      .finally(() => { if (!controller.signal.aborted) setLoading(false); });
    const expire = () => { setUser(null); setError(null); };
    window.addEventListener("platform-session-expired", expire);
    return () => { controller.abort(); window.removeEventListener("platform-session-expired", expire); };
  }, [attempt]);
  async function refresh(): Promise<void> {
    const controller = new AbortController(); pending.current = controller;
    const value = await currentUser(controller.signal);
    if (!controller.signal.aborted) setUser(value);
  }
  async function signOut(): Promise<void> {
    const controller = new AbortController(); pending.current = controller;
    setSigningOut(true); setError(null);
    try { await logout(controller.signal); if (!controller.signal.aborted) setUser(null); }
    catch (reason: unknown) { if (!controller.signal.aborted) setError(reason instanceof Error ? reason.message : "Unable to sign out."); }
    finally { if (!controller.signal.aborted) setSigningOut(false); }
  }
  if (loading) return <p role="status">Loading session…</p>;
  if (error && !user) return <><p role="alert">{error}</p><button onClick={() => setAttempt((value) => value + 1)}>Retry</button></>;
  if (!user) return <Login onAuthenticated={refresh} />;
  return <UserContext.Provider value={user}><header><span>Signed in as {user.email}</span>
    <button disabled={signingOut} onClick={() => { void signOut(); }}>Log out</button></header>
    {error && <p role="alert">{error}</p>}{children}</UserContext.Provider>;
}
