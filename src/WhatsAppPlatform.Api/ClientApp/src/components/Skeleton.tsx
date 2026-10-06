export function Skeleton({ className = "h-4 w-full" }: { className?: string }) {
  return <div aria-hidden="true" className={`rounded bg-slate-200/70 motion-safe:animate-pulse ${className}`} />;
}
