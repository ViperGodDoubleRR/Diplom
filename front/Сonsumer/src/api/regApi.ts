import type { ApiResponse } from "@/interface/ApiContracts/ApiResponse";
import axios from "axios";

export class RegApi {
  private api = axios.create({
    baseURL: "http://localhost:5107/reg"
  });

  async sendEmail(email: string): Promise<ApiResponse<string>> {
    const response= await this.api.get<ApiResponse<string>>("send-email-code", {
      params: { email: email }
    });
    return response.data;
  }

  async checkCode(email: string, code: string):Promise<ApiResponse<string>> {
    const response = await this.api.get("check-code", {
      params: { email: email, code: code }
    });
    return response.data;
  }
  async registerUser(email:string,login:string,password:string):Promise<ApiResponse<string>>{
    console.log(email,login,password)
    const response= await this.api.post("register-user",{
      email:email,login:login,password:password}
    );
    return response.data;
  }
}
