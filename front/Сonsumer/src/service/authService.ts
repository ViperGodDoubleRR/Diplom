import { AuthApi } from "@/api/authApi";
import type { ApiResponse } from "@/interface/ApiContracts/ApiResponse";
import type { AuthGo } from "@/interface/DTO/AuthGo";
import type { AuthGoResponse } from "@/interface/DTO/AuthGoResponse";

export class AuthService {
  private api = new AuthApi();

  async requestCode(email: string): Promise<ApiResponse<string>> {
    return await this.api.requestCode(email);
  }

  async authGo(data: AuthGo): Promise<ApiResponse<AuthGoResponse>> {
    return await this.api.authGo(data);
  }
}
