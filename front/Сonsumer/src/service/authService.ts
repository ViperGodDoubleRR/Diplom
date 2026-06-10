import { AuthApi } from "@/api/authApi";
import type { ApiResponse } from "@/interface/ApiContracts/ApiResponse";
import type { AuthGo } from "@/interface/DTO/AuthGo";
import type { AuthGoResponse } from "@/interface/DTO/AuthGoResponse";

export class AuthService {
  private api = new AuthApi();

  requestCode(email: string): Promise<ApiResponse<string>> {
    return this.api.requestCode(email.trim().toLowerCase());
  }

  authGo(data: AuthGo): Promise<ApiResponse<AuthGoResponse>> {
    return this.api.authGo({
      ...data,
      email: data.email.trim().toLowerCase(),
      code: data.code.trim().toUpperCase(),
    });
  }
}
