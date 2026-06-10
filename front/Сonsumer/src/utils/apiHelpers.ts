import axios from "axios";

import type { ApiResponse } from "@/interface/ApiContracts/ApiResponse";

type RawApiResponse<T> = ApiResponse<T> & {
  Success?: boolean;
  Data?: T;
  Error?: ApiResponse<T>["error"] & { Message?: string };
};

export function isApiSuccess<T>(
  res: RawApiResponse<T> | null | undefined
): boolean {
  if (!res) return false;
  if (typeof res.success === "boolean") return res.success;
  if (typeof res.Success === "boolean") return res.Success;
  return false;
}

export function apiErrorMessage(
  res: RawApiResponse<unknown> | null | undefined,
  fallback: string
): string {
  return res?.error?.message ?? res?.Error?.message ?? res?.Error?.Message ?? fallback;
}

export function apiErrorCode(
  res: RawApiResponse<unknown> | null | undefined
): string | undefined {
  const err = res?.error ?? res?.Error;
  if (!err || typeof err !== "object") return undefined;

  const row = err as { code?: string; Code?: string };
  return row.code ?? row.Code;
}

export function readAuthTokens(
  raw: unknown
): { accessToken: string; refreshToken: string } | null {
  if (!raw || typeof raw !== "object") return null;

  const item = raw as Record<string, unknown>;
  const accessToken = item.accessToken ?? item.AccessToken;
  const refreshToken = item.refreshToken ?? item.RefreshToken;

  if (typeof accessToken === "string" && typeof refreshToken === "string") {
    return { accessToken, refreshToken };
  }

  return null;
}

export function readRevokeSessionResult(
  raw: unknown
): { revoked: boolean; wasCurrentSession: boolean } | null {
  if (!raw || typeof raw !== "object") return null;

  const item = raw as Record<string, unknown>;
  const revoked = item.revoked ?? item.Revoked;
  const wasCurrent = item.wasCurrentSession ?? item.WasCurrentSession;

  return {
    revoked: Boolean(revoked),
    wasCurrentSession: Boolean(wasCurrent),
  };
}

export function getApiData<T>(res: RawApiResponse<T> | null | undefined): T | undefined {
  if (!res) return undefined;
  if (res.data !== undefined) return res.data;
  if (res.Data !== undefined) return res.Data;
  return undefined;
}

export async function withApiCatch<T>(
  request: () => Promise<ApiResponse<T>>,
  fallbackMessage: string
): Promise<ApiResponse<T>> {
  try {
    return await request();
  } catch (error) {
    if (axios.isAxiosError(error)) {
      const body = error.response?.data as RawApiResponse<T> | undefined;

      if (body && (typeof body.success === "boolean" || typeof body.Success === "boolean")) {
        return body;
      }

      const status = error.response?.status;
      let message = fallbackMessage;

      if (status === 502) {
        message =
          "Сервис чатов недоступен. Запустите ChatService (порт 5098) через back.slnLaunch.";
      } else if (status === 401) {
        message = "Нужна авторизация";
      } else if (status === 403) {
        message = "Нет доступа";
      } else if (typeof error.response?.data === "string" && error.response.data.trim()) {
        message = error.response.data;
      }

      return {
        success: false,
        error: { code: String(status ?? "NETWORK"), message },
      };
    }

    return {
      success: false,
      error: { code: "NETWORK", message: fallbackMessage },
    };
  }
}
