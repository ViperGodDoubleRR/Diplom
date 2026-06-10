import { SettingsApi } from "@/api/settingsApi";
import type { ApiResponse } from "@/interface/ApiContracts/ApiResponse";
import type { Session } from "@/interface/models/settings/Session";

const api = new SettingsApi();

export class SettingsService {
  getSessions(): Promise<ApiResponse<Session[]>> {
    return api.getSessions();
  }

  revokeSession(
    sessionId: number
  ): Promise<ApiResponse<{ revoked: boolean; wasCurrentSession: boolean }>> {
    return api.revokeSession(sessionId);
  }

  revokeOtherSessions(): Promise<ApiResponse<number>> {
    return api.revokeOtherSessions();
  }

  requestChangeEmail(
    newEmail: string,
    currentPassword: string
  ): Promise<ApiResponse<string>> {
    return api.requestChangeEmail(newEmail.trim().toLowerCase(), currentPassword);
  }

  confirmChangeEmail(
    newEmail: string,
    code: string,
    currentPassword: string
  ): Promise<ApiResponse<string>> {
    return api.confirmChangeEmail(
      newEmail.trim().toLowerCase(),
      code.trim().toUpperCase(),
      currentPassword
    );
  }

  changePassword(
    currentPassword: string,
    newPassword: string
  ): Promise<ApiResponse<string>> {
    return api.changePassword(currentPassword, newPassword);
  }
}
