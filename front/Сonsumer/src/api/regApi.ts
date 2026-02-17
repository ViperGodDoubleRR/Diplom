import axios from "axios";

export class RegApi {
  private api = axios.create({
    baseURL: "http://localhost:5291/reg"
  });

  async sendEmail(email: string) {
    const response = await this.api.get("send-email-code", {
      params: { email: email }
    });
    return response.data;
  }

  async checkCode(email: string, code: string):Promise<boolean> {
    const response = await this.api.get("check-code", {
      params: { email: email, code: code }
    });
    return response.data;
  }
  async registerUser(email:string,login:string,password:string):Promise<string>{
    console.log(email,login,password)
    const response= await this.api.post("register-user",{
      email:email,login:login,password:password}
    );
    return response.data;
  }
}
