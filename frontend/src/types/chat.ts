/**
 * A single message in the chat conversation, rendered as one bubble.
 * isStreaming is true while tokens are still arriving for this message,
 * so the UI can show a typing indicator until the stream completes.
 * hasError is true if the AI provider failed mid-stream — the message
 * shows whatever partial content arrived, plus an error indicator, instead
 * of hanging on the typing indicator forever.
 */
export interface ChatMessage {
  id: string;
  role: "user" | "assistant";
  content: string;
  isStreaming: boolean;
  hasError?: boolean;
}