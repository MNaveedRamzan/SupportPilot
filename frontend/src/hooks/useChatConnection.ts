import { useEffect, useRef, useState, useCallback } from "react";
import * as signalR from "@microsoft/signalr";
import { createChatConnection } from "../api/signalr";
import type { ChatMessage } from "../types/chat";

/**
 * Appends a streamed chunk to the last message, but only if that message is
 * an in-progress assistant reply. Extracted as a pure function (no React
 * state, no side effects) so it can be unit tested directly.
 */
export function appendChunkToLastAssistantMessage(
  messages: ChatMessage[],
  chunk: string
): ChatMessage[] {
  const updated = [...messages];
  const last = updated[updated.length - 1];
  if (last && last.role === "assistant" && last.isStreaming) {
    updated[updated.length - 1] = { ...last, content: last.content + chunk };
  }
  return updated;
}

/**
 * Manages the SignalR connection lifecycle and streaming chat state.
 * Connects on mount, disconnects on unmount. Tracks conversationId across
 * turns so the backend persists the full conversation instead of treating
 * each message as an isolated call.
 */
export function useChatConnection() {
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [isConnected, setIsConnected] = useState(false);
  const connectionRef = useRef<signalR.HubConnection | null>(null);
  const conversationIdRef = useRef<string | null>(null);

  useEffect(() => {
    const connection = createChatConnection();
    connectionRef.current = connection;

    // Sent once, right after a new conversation is created server-side.
    // Stored in a ref (not state) so sendMessage always reads the latest
    // value without needing to be recreated on every conversation change.
    connection.on("ConversationStarted", (conversationId: string) => {
      conversationIdRef.current = conversationId;
    });

    connection.on("ReceiveChunk", (chunk: string) => {
      setMessages((prev) => appendChunkToLastAssistantMessage(prev, chunk));
    });

    connection.on("ReceiveComplete", () => {
      setMessages((prev) => {
        const updated = [...prev];
        const last = updated[updated.length - 1];
        if (last && last.role === "assistant") {
          updated[updated.length - 1] = { ...last, isStreaming: false };
        }
        return updated;
      });
    });

    // Fired when sentiment auto-escalation creates a ticket for this
    // conversation. No UI action yet — just logged for now.
    connection.on("Escalated", (ticketId: string) => {
      console.log("Conversation escalated. Ticket ID:", ticketId);
    });

    connection
      .start()
      .then(() => setIsConnected(true))
      .catch((err) => console.error("SignalR connection failed:", err));

    return () => {
      connection.stop();
    };
  }, []);

  const sendMessage = useCallback(async (text: string) => {
    const connection = connectionRef.current;
    if (!connection || connection.state !== signalR.HubConnectionState.Connected) {
      console.error("Cannot send: connection not ready.");
      return;
    }

    setMessages((prev) => [
      ...prev,
      { id: crypto.randomUUID(), role: "user", content: text, isStreaming: false },
      { id: crypto.randomUUID(), role: "assistant", content: "", isStreaming: true },
    ]);

    await connection.invoke("Ask", text, conversationIdRef.current);
  }, []);

  return { messages, isConnected, sendMessage };
}