import { api } from "@/api/apiUrl";
import type { ApiResponse } from "@/interface/ApiContracts/ApiResponse";
import { withApiCatch } from "@/utils/apiHelpers";

export class RegApi {
  sendEmail(email: string): Promise<ApiResponse<string>> {
    return withApiCatch(
      () =>
        api
          .get<ApiResponse<string>>("/reg/send-email-code", {
            params: { email },
          })
          .then((res) => res.data),
      "Не удалось отправить код на email"
    );
  }

  checkCode(email: string, code: string): Promise<ApiResponse<string>> {
    return withApiCatch(
      () =>
        api
          .get<ApiResponse<string>>("/reg/check-code", {
            params: { email, code },
          })
          .then((res) => res.data),
      "Не удалось проверить код"
    );
  }

  registerUser(
    email: string,
    login: string,
    password: string
  ): Promise<ApiResponse<string>> {
    return withApiCatch(
      () =>
        api
          .post<ApiResponse<string>>("/reg/register-user", {
            email,
            login,
            password,
          })
          .then((res) => res.data),
      "Не удалось зарегистрироваться"
    );
  }
}
