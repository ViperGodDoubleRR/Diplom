import { api } from "@/api/apiUrl";
import type { ApiResponse } from "@/interface/ApiContracts/ApiResponse";
import type { AuthGo } from "@/interface/DTO/AuthGo";
import type { AuthGoResponse } from "@/interface/DTO/AuthGoResponse";
import { withApiCatch } from "@/utils/apiHelpers";

export class AuthApi {
  requestCode(email: string): Promise<ApiResponse<string>> {
    return withApiCatch(
      () =>
        api
          .get<ApiResponse<string>>("/auth/auth-request-code", {
            params: { email },
          })
          .then((res) => res.data),
      "Не удалось отправить код на email"
    );
  }

  authGo(data: AuthGo): Promise<ApiResponse<AuthGoResponse>> {
    return withApiCatch(
      () =>
        api
          .post<ApiResponse<AuthGoResponse>>("/auth/authorized-user", data)
          .then((res) => res.data),
      "Не удалось войти в аккаунт"
    );
  }
}
