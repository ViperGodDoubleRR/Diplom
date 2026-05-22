import type { ApiResponse } from "@/interface/ApiContracts/ApiResponse";
import axios from "axios";

export class ResApi {
  private api = axios.create({
    baseURL: "http://localhost:5107/auth"
  });

  async requestCode(email: string): Promise<ApiResponse<string>> {
    const response= await this.api.get<ApiResponse<string>>("res-request-code", {
      params: { email: email }
    });
    return response.data;
  }

  async checkCode(email: string, code: string):Promise<ApiResponse<string>> {
    const response = await this.api.get<ApiResponse<string>>("res-check-code", {
      params: { email: email, code: code }
    });
    return response.data;
  }
  async changePassword(email:string,password:string):Promise<ApiResponse<string>>{
    console.log(email,password)
    const response= await this.api.post<ApiResponse<string>>("res-change-password",{
      email:email,password:password}
    );
    return response.data;
  }
}
