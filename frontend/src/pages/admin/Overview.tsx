import { useEffect, useState, useCallback } from "react";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { ErrorState } from "@/components/common/ErrorState";
import { fetchMetrics, type MetricsResponse } from "@/api/dashboard";

/**
 * Metrics overview — four cards summarizing conversation and ticket health.
 * Fetched fresh on mount; no caching (Day 3 decision: real-time query over
 * precomputed, since data volume is still small).
 */
export function Overview() {
  const [metrics, setMetrics] = useState<MetricsResponse | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const loadMetrics = useCallback(() => {
    setIsLoading(true);
    setError(null);
    fetchMetrics()
      .then(setMetrics)
      .catch(() => setError("Failed to load metrics."))
      .finally(() => setIsLoading(false));
  }, []);

  useEffect(() => {
    loadMetrics();
  }, [loadMetrics]);

  if (error) {
    return <ErrorState message={error} onRetry={loadMetrics} />;
  }

  if (isLoading || !metrics) {
    return (
      <div className="grid grid-cols-2 md:grid-cols-3 gap-4 mt-6">
        {Array.from({ length: 6 }).map((_, i) => (
          <Card key={i}>
            <CardHeader>
              <Skeleton className="h-4 w-24" />
            </CardHeader>
            <CardContent>
              <Skeleton className="h-8 w-16" />
            </CardContent>
          </Card>
        ))}
      </div>
    );
  }

  const cards = [
    { label: "Total Conversations", value: metrics.totalConversations },
    { label: "Escalated Conversations", value: metrics.escalatedConversations },
    {
      label: "Escalation Rate",
      value: `${(metrics.escalationRate * 100).toFixed(1)}%`,
    },
    { label: "Total Tickets", value: metrics.totalTickets },
    { label: "Open Tickets", value: metrics.openTickets },
    {
      label: "Avg. Sentiment Score",
      value:
        metrics.averageSentimentScore !== null
          ? metrics.averageSentimentScore.toFixed(2)
          : "—",
    },
  ];

  return (
    <div className="grid grid-cols-2 md:grid-cols-3 gap-4 mt-6">
      {cards.map((card) => (
        <Card key={card.label}>
          <CardHeader>
            <CardTitle className="text-sm text-gray-500 font-normal">
              {card.label}
            </CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-2xl font-semibold">{card.value}</p>
          </CardContent>
        </Card>
      ))}
    </div>
  );
}