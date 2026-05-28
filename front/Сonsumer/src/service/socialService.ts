import { SocialApi } from "@/api/socialApi";
import type { ApiResponse } from "@/interface/ApiContracts/ApiResponse";
import type { Friend } from "@/interface/models/profile/Friend";
import type { BlackList } from "@/interface/models/profile/BlackList";
import type { UserPreview } from "@/interface/models/profile/UserPreview";
import type { RenameFriendDto } from "@/interface/DTO/social/RenameFriendDto";

const api = new SocialApi();

export class SocialService {
  // ======================
  // FRIENDS
  // ======================

  async getFriends(): Promise<ApiResponse<Friend[]>> {
    return await api.getFriends();
  }

  async addFriend(userId: string): Promise<ApiResponse<boolean>> {
    return await api.addFriend(userId);
  }

  async removeFriend(userId: string): Promise<ApiResponse<boolean>> {
    return await api.removeFriend(userId);
  }

  // ======================
  // BLOCK
  // ======================

  async getBlocked(): Promise<ApiResponse<BlackList[]>> {
    return await api.getBlocked();
  }

  async blockUser(userId: string): Promise<ApiResponse<boolean>> {
    return await api.blockUser(userId);
  }

  async unblockUser(userId: string): Promise<ApiResponse<boolean>> {
    return await api.unblockUser(userId);
  }

  // ======================
  // SEARCH USERS
  // ======================

  async searchUsers(params: {
  search: string;
  page: number;
  pageSize: number;
}): Promise<ApiResponse<UserPreview[]>> {
  return await api.searchUsers(params);
}
// SocialService.ts

async renameFriend(dto: RenameFriendDto): Promise<ApiResponse<boolean>> {
  return await api.renameFriend(dto);
}
}
