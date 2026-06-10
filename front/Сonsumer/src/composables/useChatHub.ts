import * as signalR from "@microsoft/signalr";

const API_BASE = import.meta.env.VITE_API_URL ?? "http://localhost:5107";
const HUB_ENABLED = import.meta.env.VITE_ENABLE_CHAT_HUB === "true";

function hubUrl(): string {
  if (import.meta.env.VITE_CHAT_HUB_URL) {
    return import.meta.env.VITE_CHAT_HUB_URL as string;
  }
  return `${API_BASE}/chat/hubs/chat`;
}

function readAccessToken(): string {
  const token = localStorage.getItem("accessToken");
  if (!token || token === "undefined" || token === "null") return "";
  return token;
}

function buildConnection(url: string, handlers: ChatHubHandlers) {
  const conn = new signalR.HubConnectionBuilder()
    .withUrl(url, {
      accessTokenFactory: readAccessToken,
      transport:
        signalR.HttpTransportType.WebSockets |
        signalR.HttpTransportType.ServerSentEvents |
        signalR.HttpTransportType.LongPolling,
    })
    .withAutomaticReconnect([0, 5000, 15000])
    .configureLogging(signalR.LogLevel.None)
    .build();

  conn.on("messageSent", (payload) => handlers.onMessageSent?.(payload));
  conn.on("messageUpdated", (payload) => handlers.onMessageUpdated?.(payload));
  conn.on("messageDeleted", (payload) => handlers.onMessageDeleted?.(payload));

  return conn;
}

export type ChatHubHandlers = {
  onMessageSent?: (payload: unknown) => void;
  onMessageUpdated?: (payload: unknown) => void;
  onMessageDeleted?: (payload: { chatId: number; messageId: number }) => void;
};

let connection: signalR.HubConnection | null = null;
let starting: Promise<signalR.HubConnection | null> | null = null;
let hubDisabledForSession = false;

export function isChatHubEnabled() {
  return HUB_ENABLED;
}

export function resetChatHubSession() {
  hubDisabledForSession = false;
}

export async function ensureChatHubConnection(
  handlers: ChatHubHandlers
): Promise<signalR.HubConnection | null> {
  if (!HUB_ENABLED || hubDisabledForSession || !readAccessToken()) {
    return null;
  }

  if (connection?.state === signalR.HubConnectionState.Connected) {
    return connection;
  }

  if (starting) {
    return starting;
  }

  starting = (async () => {
    try {
      if (connection) {
        await connection.stop().catch(() => undefined);
        connection = null;
      }

      connection = buildConnection(hubUrl(), handlers);
      await connection.start();
      return connection;
    } catch {
      connection = null;
      hubDisabledForSession = true;
      return null;
    }
  })().finally(() => {
    starting = null;
  });

  return starting;
}

export async function joinChatHubGroup(chatId: number) {
  const conn = connection;
  if (!conn || conn.state !== signalR.HubConnectionState.Connected) return;

  try {
    await conn.invoke("JoinChat", chatId);
  } catch {
    /* ignore */
  }
}

export async function leaveChatHubGroup(chatId: number) {
  const conn = connection;
  if (!conn || conn.state !== signalR.HubConnectionState.Connected) return;

  try {
    await conn.invoke("LeaveChat", chatId);
  } catch {
    /* ignore */
  }
}

export async function stopChatHub() {
  if (!connection) return;
  await connection.stop().catch(() => undefined);
  connection = null;
  hubDisabledForSession = false;
}
