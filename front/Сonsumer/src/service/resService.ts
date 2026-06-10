import { ResApi } from "@/api/resApi";
import type { ApiResponse } from "@/interface/ApiContracts/ApiResponse";

export class ResService {
  private api = new ResApi();

  requestCode(email: string): Promise<ApiResponse<string>> {
    return this.api.requestCode(email.trim().toLowerCase());
  }

  checkCode(email: string, code: string): Promise<ApiResponse<string>> {
    return this.api.checkCode(
      email.trim().toLowerCase(),
      code.trim().toUpperCase()
    );
  }

  changePassword(
    email: string,
    password: string,
    resetToken: string
  ): Promise<ApiResponse<string>> {
    return this.api.changePassword(
      email.trim().toLowerCase(),
      password,
      resetToken
    );
  }
}
