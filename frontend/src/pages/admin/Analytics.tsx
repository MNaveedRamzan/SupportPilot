import { useEffect, useState, useCallback } from "react";
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
  PieChart,
  Pie,
  Cell,
} from "recharts";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { ErrorState } from "@/components/common/ErrorState";
import { fetchAnalytics, type AnalyticsResponse } from "@/api/dashboard";

// Matches the thresholds used in SentimentBadge, for visual consistency
// between the conversations table and this chart.
const SENTIMENT_COLORS = {
  calm: "#22c55e",
  neutral: "#eab308",
  frustrated: "#ef4444",
};

export function Analytics() {
  const [data, setData] = useState<AnalyticsResponse | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadAnalytics = useCallback(() => {
    setIsLoading(true);
    setError(null);
    fetchAnalytics()
      .then(setData)
      .catch(() => setError("Failed to load analytics."))
      .finally(() => setIsLoading(false));
  }, []);

  useEffect(() => {
    loadAnalytics();
  }, [loadAnalytics]);

  if (error) {
    return <ErrorState message={error} onRetry={loadAnalytics} />;
  }

  if (isLoading || !data) {
    return (
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-4 mt-6">
        <Card>
          <CardHeader><Skeleton className="h-5 w-40" /></CardHeader>
          <CardContent><Skeleton className="h-64 w-full" /></CardContent>
        </Card>
        <Card>
          <CardHeader><Skeleton className="h-5 w-40" /></CardHeader>
          <CardContent><Skeleton className="h-64 w-full" /></CardContent>
        </Card>
      </div>
    );
  }

  const trendData = data.escalationTrend.map((d) => ({
    date: new Date(d.date).toLocaleDateString(undefined, {
      month: "short",
      day: "numeric",
    }),
    Total: d.totalConversations,
    Escalated: d.escalatedConversations,
  }));

  const pieData = [
    { name: "Calm", value: data.sentimentBreakdown.calmCount, color: SENTIMENT_COLORS.calm },
    { name: "Neutral", value: data.sentimentBreakdown.neutralCount, color: SENTIMENT_COLORS.neutral },
    { name: "Frustrated", value: data.sentimentBreakdown.frustratedCount, color: SENTIMENT_COLORS.frustrated },
  ].filter((slice) => slice.value > 0);

  const hasPieData = pieData.length > 0;

  return (
    <div className="grid grid-cols-1 lg:grid-cols-2 gap-4 mt-6">
      <Card>
        <CardHeader>
          <CardTitle className="text-sm font-medium">
            Escalation Trend (Last 7 Days)
          </CardTitle>
        </CardHeader>
        <CardContent>
          <ResponsiveContainer width="100%" height={280}>
            <LineChart data={trendData}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="date" fontSize={12} />
              <YAxis allowDecimals={false} fontSize={12} />
              <Tooltip />
              <Legend />
              <Line type="monotone" dataKey="Total" stroke="#3b82f6" strokeWidth={2} />
              <Line type="monotone" dataKey="Escalated" stroke="#ef4444" strokeWidth={2} />
            </LineChart>
          </ResponsiveContainer>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle className="text-sm font-medium">
            Sentiment Distribution
          </CardTitle>
        </CardHeader>
        <CardContent>
          {hasPieData ? (
            <ResponsiveContainer width="100%" height={280}>
              <PieChart>
                <Pie
                  data={pieData}
                  dataKey="value"
                  nameKey="name"
                  cx="50%"
                  cy="50%"
                  outerRadius={90}
                  label
                >
                  {pieData.map((slice) => (
                    <Cell key={slice.name} fill={slice.color} />
                  ))}
                </Pie>
                <Tooltip />
                <Legend />
              </PieChart>
            </ResponsiveContainer>
          ) : (
            <p className="text-sm text-muted-foreground text-center py-16">
              No sentiment data yet.
            </p>
          )}
        </CardContent>
      </Card>
    </div>
  );
}