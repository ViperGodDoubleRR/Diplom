import { RegApi } from "@/api/regApi";
import type { ApiResponse } from "@/interface/ApiContracts/ApiResponse";
export class RegService {
  private api = new RegApi();

  async sendEmail(email: string) {
    const response = await this.api.sendEmail(email);
    console.log(response);
    return response;
  }
  async checkCode(email:string,code:string):Promise<ApiResponse<string>>{
    const response= await this.api.checkCode(email,code);
    return response;
  }
  async registerUser(email:string,login:string,password:string):Promise<ApiResponse<string>>{
    const response = await this.api.registerUser(email,login,password);
    return response;
  }
}
