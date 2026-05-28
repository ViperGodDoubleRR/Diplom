import { UserApi } from "@/api/userApi";
import type { ApiResponse } from "@/interface/ApiContracts/ApiResponse";
import type { User } from "@/interface/models/profile/user";
import type { UpdateUserDto } from "@/interface/DTO/profile/UpdateUserDto";
import type { Media } from "@/interface/models/profile/Media";
import type { ViewUser } from "@/interface/models/profile/ViewUser";
export class UserService {
  private api = new UserApi();

  async getMe(): Promise<ApiResponse<User>> {
    return await this.api.getUser();
  }
  async getUserById(userId: string): Promise<ApiResponse<ViewUser>> {
  return await this.api.getUserById(userId);
}
  async updateMe(dto: UpdateUserDto): Promise<ApiResponse<User>> {
    return await this.api.updateUser(dto);
  }
  async uploadMedia(form: FormData): Promise<ApiResponse<Media>> {
    return await this.api.uploadMedia(form);
  }

  async getMyMedia(): Promise<ApiResponse<Media[]>> {
  return await this.api.getUserMedia();
}
 async deleteCurrentMedia(mediaId: number): Promise<ApiResponse<boolean>> {
    return this.api.deleteCurrentMedia(mediaId);
  }

  async replaceCurrentMedia(mediaId: number, file: File, mediaType: string): Promise<ApiResponse<Media>> {
    const form = new FormData();
    form.append("file", file);
    form.append("mediaType", mediaType);

    return this.api.replaceCurrentMedia(mediaId, form);
  }

  async deleteAllMedia(): Promise<ApiResponse<boolean>> {
    return this.api.deleteAllMedia();
  }
}
