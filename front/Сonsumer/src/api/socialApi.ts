import { api } from "@/api/apiUrl";
import type { ApiResponse } from "@/interface/ApiContracts/ApiResponse";
import { withApiCatch } from "@/utils/apiHelpers";
import type { Friend } from "@/interface/models/profile/Friend";
import type { BlackList } from "@/interface/models/profile/BlackList";
import type { UserPreview } from "@/interface/models/profile/UserPreview";
import type { RenameFriendDto } from "@/interface/DTO/social/RenameFriendDto";

export class SocialApi {

  // ======================
  // FRIENDS
  // ======================
  async getFriends(): Promise<ApiResponse<Friend[]>> {
    const res = await api.get<ApiResponse<Friend[]>>("/user/social/friends");
    return res.data;
  }

  async addFriend(userId: string): Promise<ApiResponse<boolean>> {
    const res = await api.post<ApiResponse<boolean>>("/user/social/friends", {
      userId,
    });
    return res.data;
  }

  async removeFriend(userId: string): Promise<ApiResponse<boolean>> {
    const res = await api.delete<ApiResponse<boolean>>(
      `/user/social/friends/${userId}`
    );
    return res.data;
  }

  // ======================
  // BLOCK
  // ======================
  async getBlocked(): Promise<ApiResponse<BlackList[]>> {
    const res = await api.get<ApiResponse<BlackList[]>>("/user/social/blocked");
    return res.data;
  }

  async blockUser(userId: string): Promise<ApiResponse<boolean>> {
    const res = await api.post<ApiResponse<boolean>>("/user/social/block", {
      userId,
    });
    return res.data;
  }

  async unblockUser(userId: string): Promise<ApiResponse<boolean>> {
    const res = await api.delete<ApiResponse<boolean>>(
      `/user/social/block/${userId}`
    );
    return res.data;
  }

  // ======================
  // USERS SEARCH
  // ======================
  searchUsers(params: {
    search: string;
    page: number;
    pageSize: number;
  }): Promise<ApiResponse<UserPreview[]>> {
    return withApiCatch(
      () =>
        api
          .get<ApiResponse<UserPreview[]>>("/user/social/users", {
            params: {
              Search: params.search ?? "",
              Page: params.page,
              PageSize: params.pageSize,
            },
          })
          .then((res) => res.data),
      "Не удалось найти пользователей"
    );
  }

  async renameFriend(dto: RenameFriendDto): Promise<ApiResponse<string>> {
    const res = await api.patch<ApiResponse<string>>(
      `/user/social/friends/${dto.userId}/rename`,
      { login: dto.login }
    );

    return res.data;
  }
}
