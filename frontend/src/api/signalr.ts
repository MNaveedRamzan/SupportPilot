import * as signalR from "@microsoft/signalr";

// Centralized so the URL only needs to change in one place when deploying
// (local dev today, Render's backend URL once SupportPilot goes live in Week 4).
const HUB_URL = `${import.meta.env.VITE_API_BASE_URL}/hubs/chat`;

/**
 * Builds a new SignalR connection to the chat hub. Does not start the
 * connection — the caller controls the lifecycle (start/stop).
 *
 * accessTokenFactory attaches the JWT as a query string param
 * (?access_token=...) during the WebSocket handshake, since browsers can't
 * set custom headers on WebSocket connections. The backend's OnMessageReceived
 * handler (Program.cs) reads it from there for the /hubs/chat path.
 */
export function createChatConnection(): signalR.HubConnection {
  return new signalR.HubConnectionBuilder()
    .withUrl(HUB_URL, {
      accessTokenFactory: () => localStorage.getItem("token") ?? "",
    })
    .withAutomaticReconnect()
    .build();
}