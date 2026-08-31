import { useState } from "react";
import { Card, CardHeader, CardTitle, CardDescription, CardContent, CardFooter } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";

interface IntegrationConfig {
  id: string;
  name: string;
  description: string;
  status: "available" | "comingSoon";
}

const INTEGRATIONS: IntegrationConfig[] = [
  {
    id: "slack",
    name: "Slack",
    description: "Post escalated conversations directly to a Slack channel so your support team sees them in real time.",
    status: "available",
  },
  {
    id: "teams",
    name: "Microsoft Teams",
    description: "Route escalations and daily summaries to a Teams channel.",
    status: "comingSoon",
  },
  {
    id: "zendesk",
    name: "Zendesk",
    description: "Sync escalated tickets into an existing Zendesk workspace.",
    status: "comingSoon",
  },
];

/**
 * UI-only integration stub. Demonstrates the intended extension point for
 * outbound notifications (Slack/Teams/Zendesk) without a real backend
 * webhook implementation. A production build would add an IWebhookService
 * interface — following the same provider-abstraction pattern already used
 * for IChatProvider — with real OAuth flows and webhook delivery.
 *
 * Connecting here only toggles local UI state; no request leaves the browser.
 */
export function Integrations() {
  const [connectedIds, setConnectedIds] = useState<Set<string>>(new Set());

  const toggleConnection = (id: string) => {
    setConnectedIds((prev) => {
      const next = new Set(prev);
      if (next.has(id)) {
        next.delete(id);
      } else {
        next.add(id);
      }
      return next;
    });
  };

  return (
    <div className="mt-6 space-y-4">
      <p className="text-sm text-muted-foreground">
        Connect SupportPilot to your team's tools to route escalations and notifications automatically.
      </p>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        {INTEGRATIONS.map((integration) => {
          const isConnected = connectedIds.has(integration.id);
          const isComingSoon = integration.status === "comingSoon";

          return (
            <Card key={integration.id}>
              <CardHeader>
                <div className="flex items-center justify-between">
                  <CardTitle className="text-base">{integration.name}</CardTitle>
                  {isComingSoon ? (
                    <Badge variant="secondary">Coming soon</Badge>
                  ) : isConnected ? (
                    <Badge className="bg-green-100 text-green-800 hover:bg-green-100">Connected</Badge>
                  ) : null}
                </div>
                <CardDescription>{integration.description}</CardDescription>
              </CardHeader>
              <CardContent />
              <CardFooter>
                <Button
                  variant={isConnected ? "outline" : "default"}
                  size="sm"
                  disabled={isComingSoon}
                  onClick={() => toggleConnection(integration.id)}
                >
                  {isConnected ? "Disconnect" : "Connect"}
                </Button>
              </CardFooter>
            </Card>
          );
        })}
      </div>
    </div>
  );
}