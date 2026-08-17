import { useEffect, useRef } from "react";
import { useChatConnection } from "../../hooks/useChatConnection";
import { MessageBubble } from "./MessageBubble";
import { ChatInput } from "./ChatInput";

/**
 * Top-level chat UI: message history + input, wired to the SignalR connection.
 * Auto-scrolls to the latest message as new content streams in.
 */
export function ChatWindow() {
  const { messages, isConnected, sendMessage } = useChatConnection();
  const bottomRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages]);

  const isStreaming = messages[messages.length - 1]?.isStreaming ?? false;

  return (
    <div className="flex flex-col h-[600px] w-full max-w-2xl mx-auto border border-gray-200 rounded-xl shadow-sm bg-white">
      <div className="px-4 py-3 border-b border-gray-200">
        <h1 className="text-base font-semibold text-gray-900">SupportPilot</h1>
        <p className="text-xs text-gray-500">
          {isConnected ? "Connected" : "Connecting..."}
        </p>
      </div>

      <div className="flex-1 overflow-y-auto px-4 py-3">
        {messages.length === 0 && (
          <p className="text-sm text-gray-400 text-center mt-8">
            Ask a question to get started.
          </p>
        )}
        {messages.map((m) => (
          <MessageBubble key={m.id} message={m} />
        ))}
        <div ref={bottomRef} />
      </div>

      <ChatInput onSend={sendMessage} disabled={isStreaming || !isConnected} />
    </div>
  );
}