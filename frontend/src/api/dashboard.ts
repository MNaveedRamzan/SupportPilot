import { authFetch } from "./client";

// --- Types matching backend DTOs ---

export interface MetricsResponse {
  totalConversations: number;
  escalatedConversations: number;
  escalationRate: number;
  totalTickets: number;
  openTickets: number;
  averageSentimentScore: number | null;
}

export interface ConversationSummary {
  id: string;
  createdAt: string;
  isEscalated: boolean;
  linkedTicketId: string | null;
  messageCount: number;
  lastMessagePreview: string | null;
  averageSentimentScore: number | null;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface Message {
  id: string;
  conversationId: string;
  role: "System" | "User" | "Assistant";
  content: string;
  sentimentScore: number | null;
  createdAt: string;
}

export interface Conversation {
  id: string;
  createdAt: string;
  isEscalated: boolean;
  linkedTicketId: string | null;
  messages: Message[];
}

export interface KnowledgeArticle {
  id: string;
  text: string;
}

// --- API calls ---

export async function fetchMetrics(): Promise<MetricsResponse> {
  const res = await authFetch("/Metrics");
  if (!res.ok) throw new Error("Failed to fetch metrics");
  return res.json();
}

export async function fetchConversations(
  page: number,
  pageSize: number
): Promise<PagedResult<ConversationSummary>> {
  const res = await authFetch(`/Conversations?page=${page}&pageSize=${pageSize}`);
  if (!res.ok) throw new Error("Failed to fetch conversations");
  return res.json();
}

export async function fetchConversationById(id: string): Promise<Conversation> {
  const res = await authFetch(`/Conversations/${id}`);
  if (!res.ok) throw new Error("Failed to fetch conversation");
  return res.json();
}

export async function fetchKnowledgeArticles(): Promise<KnowledgeArticle[]> {
  const res = await authFetch("/KnowledgeBase");
  if (!res.ok) throw new Error("Failed to fetch knowledge base articles");
  return res.json();
}

export async function addKnowledgeArticle(text: string): Promise<KnowledgeArticle> {
  const res = await authFetch("/KnowledgeBase", {
    method: "POST",
    body: JSON.stringify({ text }),
  });
  if (!res.ok) throw new Error("Failed to add article");
  return res.json();
}

export async function deleteKnowledgeArticle(id: string): Promise<void> {
  const res = await authFetch(`/KnowledgeBase/${id}`, {
    method: "DELETE",
  });
  if (!res.ok) throw new Error("Failed to delete article");
}