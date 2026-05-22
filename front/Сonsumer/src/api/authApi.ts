import type { ApiResponse } from "@/interface/ApiContracts/ApiResponse";
import axios from "axios";
import type { AuthGo } from "@/interface/DTO/AuthGo";
export class AuthApi {
  private api = axios.create({
    baseURL: "http://localhost:5107/auth"
  });

  async requestCode(email: string): Promise<ApiResponse<string>> {
    const response= await this.api.get<ApiResponse<string>>("auth-request-code", {
      params: { email: email }
    });
    return response.data;
  }
  async authGo(data:AuthGo):Promise<ApiResponse<string>>{
    console.log(data)
    const response= await this.api.post("authorized-user",data);
    return response.data;
  }
}
