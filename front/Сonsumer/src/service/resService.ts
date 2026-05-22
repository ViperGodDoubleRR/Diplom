import { ResApi } from "@/api/resApi";
import type { ApiResponse } from "@/interface/ApiContracts/ApiResponse";
export class ResService {
  private api = new ResApi();

  async requestCode(email: string) {
    const response = await this.api.requestCode(email);
    console.log(response);
    return response;
  }
  async checkCode(email:string,code:string):Promise<ApiResponse<string>>{
    const response= await this.api.checkCode(email,code);
    return response;
  }
  async changePassword(email:string,password:string):Promise<ApiResponse<string>>{
    const response = await this.api.changePassword(email,password);
    return response;
  }
}
