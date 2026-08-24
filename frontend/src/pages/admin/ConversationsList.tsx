import { useEffect, useState, useCallback } from "react";
import { SentimentBadge } from "@/components/common/SentimentBadge";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { EmptyState } from "@/components/common/EmptyState";
import { ErrorState } from "@/components/common/ErrorState";
import {
  Sheet,
  SheetContent,
  SheetHeader,
  SheetTitle,
} from "@/components/ui/sheet";
import {
  fetchConversations,
  fetchConversationById,
  type ConversationSummary,
  type Conversation,
} from "@/api/dashboard";

const PAGE_SIZE = 20;

/**
 * Paginated conversations table. Uses the backend's offset pagination
 * (page/pageSize) directly — no client-side slicing of a large dataset.
 * Clicking a row opens the full transcript in a side panel.
 */
export function ConversationsList() {
  const [items, setItems] = useState<ConversationSummary[]>([]);
  const [page, setPage] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const [selectedConversation, setSelectedConversation] =
    useState<Conversation | null>(null);
  const [isTranscriptOpen, setIsTranscriptOpen] = useState(false);
  const [isLoadingTranscript, setIsLoadingTranscript] = useState(false);
  const [transcriptError, setTranscriptError] = useState<string | null>(null);

  const loadConversations = useCallback(() => {
    setIsLoading(true);
    setError(null);
    fetchConversations(page, PAGE_SIZE)
      .then((result) => {
        setItems(result.items);
        setTotalCount(result.totalCount);
      })
      .catch(() => setError("Failed to load conversations."))
      .finally(() => setIsLoading(false));
  }, [page]);

  useEffect(() => {
    loadConversations();
  }, [loadConversations]);

  const totalPages = Math.max(1, Math.ceil(totalCount / PAGE_SIZE));

  const handleRowClick = async (conversationId: string) => {
    setIsTranscriptOpen(true);
    setIsLoadingTranscript(true);
    setTranscriptError(null);
    try {
      const conversation = await fetchConversationById(conversationId);
      setSelectedConversation(conversation);
    } catch {
      setTranscriptError("Failed to load conversation transcript.");
    } finally {
      setIsLoadingTranscript(false);
    }
  };

  if (error) {
    return <ErrorState message={error} onRetry={loadConversations} />;
  }

  return (
    <div className="mt-6 space-y-4">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Created</TableHead>
            <TableHead>Status</TableHead>
            <TableHead>Sentiment</TableHead>
            <TableHead>Messages</TableHead>
            <TableHead>Last Message</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {isLoading ? (
            Array.from({ length: 5 }).map((_, i) => (
              <TableRow key={i}>
                <TableCell><Skeleton className="h-4 w-32" /></TableCell>
                <TableCell><Skeleton className="h-5 w-16" /></TableCell>
                <TableCell><Skeleton className="h-5 w-20" /></TableCell>
                <TableCell><Skeleton className="h-4 w-8" /></TableCell>
                <TableCell><Skeleton className="h-4 w-full" /></TableCell>
              </TableRow>
            ))
          ) : items.length === 0 ? (
            <TableRow>
              <TableCell colSpan={5} className="p-0">
                <EmptyState
                  title="No conversations yet"
                  description="Conversations will appear here once customers start chatting."
                />
              </TableCell>
            </TableRow>
          ) : (
            items.map((conversation) => (
              <TableRow
                key={conversation.id}
                className="cursor-pointer hover:bg-gray-50"
                onClick={() => handleRowClick(conversation.id)}
              >
                <TableCell>
                  {new Date(conversation.createdAt).toLocaleString()}
                </TableCell>
                <TableCell>
                  {conversation.isEscalated ? (
                    <Badge variant="destructive">Escalated</Badge>
                  ) : (
                    <Badge variant="secondary">Normal</Badge>
                  )}
                </TableCell>
                <TableCell>
                  <SentimentBadge score={conversation.averageSentimentScore} />
                </TableCell>
                <TableCell>{conversation.messageCount}</TableCell>
                <TableCell className="max-w-md truncate text-gray-600">
                  {conversation.lastMessagePreview ?? "—"}
                </TableCell>
              </TableRow>
            ))
          )}
        </TableBody>
      </Table>

      {!isLoading && items.length > 0 && (
        <div className="flex items-center justify-between">
          <p className="text-sm text-gray-500">
            Page {page} of {totalPages} ({totalCount} total)
          </p>
          <div className="flex gap-2">
            <Button
              variant="outline"
              size="sm"
              disabled={page <= 1}
              onClick={() => setPage((p) => p - 1)}
            >
              Previous
            </Button>
            <Button
              variant="outline"
              size="sm"
              disabled={page >= totalPages}
              onClick={() => setPage((p) => p + 1)}
            >
              Next
            </Button>
          </div>
        </div>
      )}

      <Sheet open={isTranscriptOpen} onOpenChange={setIsTranscriptOpen}>
        <SheetContent className="w-full sm:max-w-lg overflow-y-auto">
          <SheetHeader>
            <SheetTitle>Conversation Transcript</SheetTitle>
          </SheetHeader>

          <div className="px-4 pb-4 space-y-3">
            {isLoadingTranscript ? (
              Array.from({ length: 3 }).map((_, i) => (
                <Skeleton key={i} className="h-16 w-full rounded-md" />
              ))
            ) : transcriptError ? (
              <ErrorState message={transcriptError} />
            ) : selectedConversation ? (
              selectedConversation.messages.map((message) => (
                <div
                  key={message.id}
                  className={`rounded-md p-3 text-sm ${
                    message.role === "User"
                      ? "bg-blue-50 text-blue-900"
                      : "bg-gray-50 text-gray-800"
                  }`}
                >
                  <div className="flex items-center justify-between mb-1">
                    <span className="font-medium">{message.role}</span>
                    {message.sentimentScore !== null && (
                      <SentimentBadge score={message.sentimentScore} />
                    )}
                  </div>
                  <p>{message.content}</p>
                </div>
              ))
            ) : null}
          </div>
        </SheetContent>
      </Sheet>
    </div>
  );
}