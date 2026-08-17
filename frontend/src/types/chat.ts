/**
 * A single message in the chat conversation, rendered as one bubble.
 * isStreaming is true while tokens are still arriving for this message,
 * so the UI can show a typing indicator until the stream completes.
 */
export interface ChatMessage {
  id: string;
  role: "user" | "assistant";
  content: string;
  isStreaming: boolean;
}