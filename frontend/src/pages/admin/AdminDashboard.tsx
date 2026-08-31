import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Link } from "react-router-dom";
import { Overview } from "./Overview";
import { ConversationsList } from "./ConversationsList";
import { KnowledgeBase } from "./KnowledgeBase";
import { Analytics } from "./Analytics";
import { Integrations } from "./Integrations";

/**
 * Admin dashboard shell. Tabs switch between backend capabilities —
 * metrics, analytics, conversation review, knowledge base management,
 * and integration stubs — without needing separate routes for each.
 */
export function AdminDashboard() {
  return (
    <div className="min-h-screen bg-gray-50 p-6">
      <div className="max-w-6xl mx-auto space-y-6">
        <div className="flex items-center justify-between">
          <h1 className="text-2xl font-semibold">SupportPilot Admin</h1>
          <Link to="/" className="text-sm text-gray-500 hover:text-gray-700">
            ← Back to Chat
          </Link>
        </div>

        <Tabs defaultValue="overview">
          <TabsList>
            <TabsTrigger value="overview">Overview</TabsTrigger>
            <TabsTrigger value="analytics">Analytics</TabsTrigger>
            <TabsTrigger value="conversations">Conversations</TabsTrigger>
            <TabsTrigger value="knowledge-base">Knowledge Base</TabsTrigger>
            <TabsTrigger value="integrations">Integrations</TabsTrigger>
          </TabsList>

          <TabsContent value="overview">
            <Overview />
          </TabsContent>

          <TabsContent value="analytics">
            <Analytics />
          </TabsContent>

          <TabsContent value="conversations">
            <ConversationsList />
          </TabsContent>

          <TabsContent value="knowledge-base">
            <KnowledgeBase />
          </TabsContent>

          <TabsContent value="integrations">
            <Integrations />
          </TabsContent>
        </Tabs>
      </div>
    </div>
  );
}