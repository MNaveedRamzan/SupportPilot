import { describe, it, expect } from "vitest";
import { appendChunkToLastAssistantMessage } from "./useChatConnection";
import type { ChatMessage } from "../types/chat";

describe("appendChunkToLastAssistantMessage", () => {
  it("appends the chunk to the last message when it is a streaming assistant message", () => {
    const messages: ChatMessage[] = [
      { id: "1", role: "user", content: "Hello", isStreaming: false },
      { id: "2", role: "assistant", content: "Hi there", isStreaming: true },
    ];

    const result = appendChunkToLastAssistantMessage(messages, ", how are you?");

    expect(result[1].content).toBe("Hi there, how are you?");
  });

  it("does not modify the message when the last message is not streaming", () => {
    const messages: ChatMessage[] = [
      { id: "1", role: "user", content: "Hello", isStreaming: false },
      { id: "2", role: "assistant", content: "Hi there", isStreaming: false },
    ];

    const result = appendChunkToLastAssistantMessage(messages, " extra text");

    expect(result[1].content).toBe("Hi there");
  });

  it("does not modify the message when the last message is from the user", () => {
    const messages: ChatMessage[] = [
      { id: "1", role: "assistant", content: "Done", isStreaming: false },
      { id: "2", role: "user", content: "Another question", isStreaming: false },
    ];

    const result = appendChunkToLastAssistantMessage(messages, " chunk");

    expect(result[1].content).toBe("Another question");
  });

  it("returns an empty array unchanged when there are no messages", () => {
    const result = appendChunkToLastAssistantMessage([], "chunk");

    expect(result).toEqual([]);
  });

  it("does not mutate the original messages array", () => {
    const original: ChatMessage[] = [
      { id: "1", role: "assistant", content: "Hi", isStreaming: true },
    ];

    appendChunkToLastAssistantMessage(original, " there");

    expect(original[0].content).toBe("Hi");
  });
});