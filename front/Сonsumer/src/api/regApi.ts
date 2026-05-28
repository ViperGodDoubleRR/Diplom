import { api } from "@/api/apiUrl";
import type { ApiResponse } from "@/interface/ApiContracts/ApiResponse";

export class RegApi {
  async sendEmail(email: string): Promise<ApiResponse<string>> {
    const res = await api.get("/reg/send-email-code", {
      params: { email },
    });

    return res.data;
  }

  async checkCode(email: string, code: string): Promise<ApiResponse<string>> {
    const res = await api.get("/reg/check-code", {
      params: { email, code },
    });

    return res.data;
  }

  async registerUser(email: string, login: string, password: string) {
    const res = await api.post("/reg/register-user", {
      email,
      login,
      password,
    });

    return res.data;
  }
}
