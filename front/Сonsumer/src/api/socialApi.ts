import { api } from "@/api/apiUrl";
import type { ApiResponse } from "@/interface/ApiContracts/ApiResponse";
import type { Friend } from "@/interface/models/profile/Friend";
import type { BlackList } from "@/interface/models/profile/BlackList";
import type { UserPreview } from "@/interface/models/profile/UserPreview";
import type { RenameFriendDto } from "@/interface/DTO/social/RenameFriendDto";

export class SocialApi {

  // ======================
  // FRIENDS
  // ======================
  async getFriends(): Promise<ApiResponse<Friend[]>> {
    const res = await api.get<ApiResponse<Friend[]>>("user/social/friends");
    return res.data;
  }

  async addFriend(userId: string): Promise<ApiResponse<boolean>> {
    const res = await api.post<ApiResponse<boolean>>("user/social/friends", {
      userId,
    });
    return res.data;
  }

  async removeFriend(userId: string): Promise<ApiResponse<boolean>> {
    const res = await api.delete<ApiResponse<boolean>>(
      `user/social/friends/${userId}`
    );
    return res.data;
  }

  // ======================
  // BLOCK
  // ======================
  async getBlocked(): Promise<ApiResponse<BlackList[]>> {
    const res = await api.get<ApiResponse<BlackList[]>>("user/social/blocked");
    return res.data;
  }

  async blockUser(userId: string): Promise<ApiResponse<boolean>> {
    const res = await api.post<ApiResponse<boolean>>("user/social/block", {
      userId,
    });
    return res.data;
  }

  async unblockUser(userId: string): Promise<ApiResponse<boolean>> {
    const res = await api.delete<ApiResponse<boolean>>(
      `user/social/block/${userId}`
    );
    return res.data;
  }

  // ======================
  // USERS SEARCH
  // ======================
  async searchUsers(params: {
  search: string;
  page: number;
  pageSize: number;
}): Promise<ApiResponse<UserPreview[]>> {
  const res = await api.get("user/social/users", {
    params: {
      Search: params.search ?? "",
      Page: params.page,
      PageSize: params.pageSize,
    },
  });

  return res.data;
}
// SocialApi.ts

async renameFriend(dto: RenameFriendDto): Promise<ApiResponse<boolean>> {
  const res = await api.patch<ApiResponse<boolean>>(
    `user/social/friends/${dto.userId}/rename`,
    {
      login: dto.login,
    }
  );

  return res.data;
}
}
