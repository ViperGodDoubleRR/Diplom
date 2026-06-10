import { api } from "@/api/apiUrl";
import type { ApiResponse } from "@/interface/ApiContracts/ApiResponse";
import type { Session } from "@/interface/models/settings/Session";
import { withApiCatch } from "@/utils/apiHelpers";
import { getRefreshToken } from "@/utils/authGuard";

function refreshHeaders() {
  return {
    "X-Refresh-Token": getRefreshToken() ?? "",
  };
}

export class SettingsApi {
  getSessions(): Promise<ApiResponse<Session[]>> {
    return withApiCatch(
      () =>
        api
          .get<ApiResponse<Session[]>>("/auth/sessions", {
            headers: refreshHeaders(),
          })
          .then((res) => res.data),
      "Не удалось загрузить сессии"
    );
  }

  revokeSession(
    sessionId: number
  ): Promise<ApiResponse<{ revoked: boolean; wasCurrentSession: boolean }>> {
    return withApiCatch(
      () =>
        api
          .delete<
            ApiResponse<{ revoked: boolean; wasCurrentSession: boolean }>
          >(`/auth/sessions/${sessionId}`, {
            headers: refreshHeaders(),
            data: { refreshToken: getRefreshToken() },
          })
          .then((res) => res.data),
      "Не удалось отозвать сессию"
    );
  }

  revokeOtherSessions(): Promise<ApiResponse<number>> {
    return withApiCatch(
      () =>
        api
          .post<ApiResponse<number>>(
            "/auth/sessions/revoke-others",
            { refreshToken: getRefreshToken() },
            { headers: refreshHeaders() }
          )
          .then((res) => res.data),
      "Не удалось завершить другие сессии"
    );
  }

  requestChangeEmail(
    newEmail: string,
    currentPassword: string
  ): Promise<ApiResponse<string>> {
    return withApiCatch(
      () =>
        api
          .post<ApiResponse<string>>("/auth/change-email/request", {
            newEmail,
            currentPassword,
          })
          .then((res) => res.data),
      "Не удалось отправить код"
    );
  }

  confirmChangeEmail(
    newEmail: string,
    code: string,
    currentPassword: string
  ): Promise<ApiResponse<string>> {
    return withApiCatch(
      () =>
        api
          .post<ApiResponse<string>>("/auth/change-email/confirm", {
            newEmail,
            code,
            currentPassword,
          })
          .then((res) => res.data),
      "Не удалось сменить email"
    );
  }

  changePassword(
    currentPassword: string,
    newPassword: string
  ): Promise<ApiResponse<string>> {
    return withApiCatch(
      () =>
        api
          .post<ApiResponse<string>>("/auth/change-password", {
            currentPassword,
            newPassword,
            refreshToken: getRefreshToken(),
          })
          .then((res) => res.data),
      "Не удалось сменить пароль"
    );
  }
}
