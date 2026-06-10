import { api } from "@/api/apiUrl";
import type { ApiResponse } from "@/interface/ApiContracts/ApiResponse";
import type { ChangePasswordRequest } from "@/interface/DTO/ChangePasswordRequest";
import { withApiCatch } from "@/utils/apiHelpers";

export class ResApi {
  requestCode(email: string): Promise<ApiResponse<string>> {
    return withApiCatch(
      () =>
        api
          .get<ApiResponse<string>>("/auth/res-request-code", {
            params: { email },
          })
          .then((res) => res.data),
      "Не удалось отправить код для сброса пароля"
    );
  }

  checkCode(email: string, code: string): Promise<ApiResponse<string>> {
    return withApiCatch(
      () =>
        api
          .get<ApiResponse<string>>("/auth/res-check-code", {
            params: { email, code },
          })
          .then((res) => res.data),
      "Не удалось проверить код"
    );
  }

  changePassword(
    email: string,
    password: string,
    resetToken: string
  ): Promise<ApiResponse<string>> {
    const body: ChangePasswordRequest = {
      email,
      password,
      resetToken,
    };

    return withApiCatch(
      () =>
        api
          .post<ApiResponse<string>>("/auth/res-change-password", body)
          .then((res) => res.data),
      "Не удалось сменить пароль"
    );
  }
}
