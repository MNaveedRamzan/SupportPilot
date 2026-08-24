interface SentimentBadgeProps {
  score: number | null;
}

/**
 * Color-coded sentiment indicator. Thresholds mirror the backend's
 * escalation threshold (0.7) so the red zone here visually matches the
 * point at which a conversation would auto-escalate.
 */
export function SentimentBadge({ score }: SentimentBadgeProps) {
  if (score === null) {
    return <span className="text-xs text-muted-foreground">—</span>;
  }

  const { color, label } =
    score < 0.3
      ? { color: "bg-green-500", label: "Calm" }
      : score < 0.7
        ? { color: "bg-yellow-500", label: "Neutral" }
        : { color: "bg-red-500", label: "Frustrated" };

  return (
    <span className="inline-flex items-center gap-1.5 text-xs">
      <span className={`h-2 w-2 rounded-full ${color}`} />
      <span className="text-muted-foreground">{label}</span>
      <span className="text-muted-foreground">({score.toFixed(2)})</span>
    </span>
  );
}