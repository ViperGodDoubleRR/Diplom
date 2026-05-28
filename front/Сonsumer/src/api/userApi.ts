import { api } from "@/api/apiUrl";
import type { ApiResponse } from "@/interface/ApiContracts/ApiResponse";
import type { User } from "@/interface/models/profile/user";
import type { UpdateUserDto } from "@/interface/DTO/profile/UpdateUserDto";
import type { Media } from "@/interface/models/profile/Media";
import type { ViewUser } from "@/interface/models/profile/ViewUser";
export class UserApi {
  async getUser(): Promise<ApiResponse<User>> {
    const response = await api.get<ApiResponse<User>>("/user/get-user");
    return response.data;
  }
  async updateUser(dto: UpdateUserDto): Promise<ApiResponse<User>> {
    const response = await api.put<ApiResponse<User>>(
      "/user/update",
      dto
    );

    return response.data;
  };
  async uploadMedia(form: FormData): Promise<ApiResponse<Media>> {
    const response = await api.post<ApiResponse<Media>>(
      "/user/media-upload",
      form,
      {
        headers: {
          "Content-Type": "multipart/form-data"
        }
      }
    );

    return response.data;
  }

async getUserMedia(): Promise<ApiResponse<Media[]>> {
  const response = await api.get<ApiResponse<Media[]>>(
    "/user/media"
  );

  return response.data;
}
async deleteCurrentMedia(mediaId: number): Promise<ApiResponse<boolean>> {
    const response = await api.delete(`/user/media/${mediaId}`);
    return response.data;
  }

  async replaceCurrentMedia(mediaId: number, form: FormData): Promise<ApiResponse<Media>> {
    const response = await api.put(`/user/media/replace/${mediaId}`, form, {
      headers: { "Content-Type": "multipart/form-data" }
    });

    return response.data;
  }

  async deleteAllMedia(): Promise<ApiResponse<boolean>> {
    const response = await api.delete(`/user/media`);
    return response.data;
  }
  async getUserById(userId: string): Promise<ApiResponse<ViewUser>> {
  const response = await api.get<ApiResponse<ViewUser>>(
    `/user/${userId}`
  );

  return response.data;
}
}
