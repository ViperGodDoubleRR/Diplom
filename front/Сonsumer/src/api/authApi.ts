import { api } from "@/api/apiUrl";
import type { ApiResponse } from "@/interface/ApiContracts/ApiResponse";
import type { AuthGo } from "@/interface/DTO/AuthGo";
import type { AuthGoResponse } from "@/interface/DTO/AuthGoResponse";

export class AuthApi {
  async requestCode(email: string): Promise<ApiResponse<string>> {
    const res = await api.get("/auth/auth-request-code", {
      params: { email },
    });

    return res.data;
  }

  async authGo(data: AuthGo): Promise<ApiResponse<AuthGoResponse>> {
    const res = await api.post("/auth/authorized-user", data);
    return res.data;
  }
}
