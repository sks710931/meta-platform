export function DateTime({ value, format = "date" }: { value: string; format?: "date" | "dateTime" }) {
  const date = new Date(value);
  const label = new Intl.DateTimeFormat(undefined, {
    year: "numeric",
    month: "short",
    day: "numeric",
    ...(format === "dateTime" ? { hour: "numeric", minute: "2-digit" } : {}),
  }).format(date);
  return (
    <time dateTime={value} title={date.toLocaleString()}>
      {label}
    </time>
  );
}
