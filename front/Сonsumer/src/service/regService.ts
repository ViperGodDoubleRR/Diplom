import { RegApi } from "@/api/regApi";
import type { ApiResponse } from "@/interface/ApiContracts/ApiResponse";

export class RegService {
  private api = new RegApi();

  sendEmail(email: string): Promise<ApiResponse<string>> {
    return this.api.sendEmail(email.trim().toLowerCase());
  }

  checkCode(email: string, code: string): Promise<ApiResponse<string>> {
    return this.api.checkCode(
      email.trim().toLowerCase(),
      code.trim().toUpperCase()
    );
  }

  registerUser(
    email: string,
    login: string,
    password: string
  ): Promise<ApiResponse<string>> {
    return this.api.registerUser(
      email.trim().toLowerCase(),
      login.trim(),
      password
    );
  }
}
