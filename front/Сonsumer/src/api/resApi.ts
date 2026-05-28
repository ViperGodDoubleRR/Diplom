import { api } from "@/api/apiUrl";
import type { ApiResponse } from "@/interface/ApiContracts/ApiResponse";

export class ResApi {
  async requestCode(email: string): Promise<ApiResponse<string>> {
    const response = await api.get<ApiResponse<string>>(
      "/auth/res-request-code",
      {
        params: { email },
      }
    );

    return response.data;
  }

  async checkCode(email: string, code: string): Promise<ApiResponse<string>> {
    const response = await api.get<ApiResponse<string>>(
      "/auth/res-check-code",
      {
        params: { email, code },
      }
    );

    return response.data;
  }

  async changePassword(
    email: string,
    password: string
  ): Promise<ApiResponse<string>> {
    console.log("CHANGE PASSWORD:", { email, password });

    const response = await api.post<ApiResponse<string>>(
      "/auth/res-change-password",
      {
        email,
        password,
      }
    );

    return response.data;
  }
}
