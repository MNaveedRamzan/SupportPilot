import type { ChatMessage } from "../../types/chat";

interface MessageBubbleProps {
  message: ChatMessage;
}

/**
 * Renders a single chat message as a bubble, aligned right for the user
 * and left for the assistant. Shows a typing indicator while streaming,
 * or an error indicator if the AI provider failed mid-response.
 */
export function MessageBubble({ message }: MessageBubbleProps) {
  const isUser = message.role === "user";

  return (
    <div className={`flex ${isUser ? "justify-end" : "justify-start"} mb-3`}>
      <div
        className={`max-w-[75%] rounded-2xl px-4 py-2 text-sm ${
          isUser
            ? "bg-blue-600 text-white rounded-br-sm"
            : message.hasError
              ? "bg-red-50 text-red-800 rounded-bl-sm"
              : "bg-gray-100 text-gray-900 rounded-bl-sm"
        }`}
      >
        {message.content}
        {message.isStreaming && (
          <span className="inline-block w-1.5 h-4 ml-1 bg-gray-500 animate-pulse align-middle" />
        )}
        {message.hasError && (
          <span className="block text-xs text-red-600 mt-1">
            ⚠ Response failed
          </span>
        )}
      </div>
    </div>
  );
}