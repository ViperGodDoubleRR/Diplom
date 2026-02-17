import { RegApi } from "@/api/regApi";
export class RegService {
  private api = new RegApi();

  async sendEmail(email: string) {
    const response = await this.api.sendEmail(email);
    return response;
  }
  async checkCode(email:string,code:string){
    const response= await this.api.checkCode(email,code);
    return response;
  }
  async registerUser(email:string,login:string,password:string):Promise<string>{
    const response = await this.api.registerUser(email,login,password);
    return response;
  }
}
