import { defineStore } from "pinia";

import { SocialService } from "@/service/socialService";
import { getApiData, isApiSuccess } from "@/utils/apiHelpers";
import { DIRECTORY_USERS_PAGE_SIZE } from "@/constants/socialConstants";
import {
  normalizeSocialUsers,
  type SocialListUser,
} from "@/utils/socialUser";

const service = new SocialService();

export const useSocialStore = defineStore("social", {
  state: () => ({
    friends: [] as SocialListUser[],
    blocked: [] as SocialListUser[],
    users: [] as SocialListUser[],
    directoryUsers: [] as SocialListUser[],

    loading: false,
    directoryLoading: false,
    directoryHasMore: false,

    page: 1,
    search: "",
    directorySearchSeq: 0,
  }),

  actions: {
    async getFriends() {
      this.loading = true;

      try {
        const res = await service.getFriends();

        if (isApiSuccess(res)) {
          this.friends = normalizeSocialUsers(getApiData(res));
        }

        return res;
      } finally {
        this.loading = false;
      }
    },

    async addFriend(userId: string) {
      this.loading = true;

      try {
        const res = await service.addFriend(userId);

        if (isApiSuccess(res)) {
          await this.getFriends();
        }

        return res;
      } finally {
        this.loading = false;
      }
    },

    async removeFriend(userId: string) {
      this.loading = true;

      try {
        const res = await service.removeFriend(userId);

        if (isApiSuccess(res)) {
          this.friends = this.friends.filter(
            (friend) => friend.id !== userId
          );
        }

        return res;
      } finally {
        this.loading = false;
      }
    },

    // =========================
    // BLOCKED
    // =========================

    async getBlocked() {
      this.loading = true;

      try {
        const res = await service.getBlocked();

        if (isApiSuccess(res)) {
          this.blocked = normalizeSocialUsers(getApiData(res));
        }

        return res;
      } finally {
        this.loading = false;
      }
    },

    async blockUser(userId: string) {
      this.loading = true;

      try {
        const res = await service.blockUser(userId);

        if (isApiSuccess(res)) {
          await this.getBlocked();

          this.friends = this.friends.filter(
            (friend) => friend.id !== userId
          );
        }

        return res;
      } finally {
        this.loading = false;
      }
    },

    async unblockUser(userId: string) {
      this.loading = true;

      try {
        const res = await service.unblockUser(userId);

        if (isApiSuccess(res)) {
          this.blocked = this.blocked.filter(
            (user) => user.id !== userId
          );
        }

        return res;
      } finally {
        this.loading = false;
      }
    },

    // =========================
    // SEARCH USERS
    // =========================

    async searchUsers(params: {
      search: string;
      page: number;
      pageSize: number;
    }) {
      this.loading = true;

      try {
        const res = await service.searchUsers(params);

        const users = getApiData(res);
        if (isApiSuccess(res) && users) {
          this.users = normalizeSocialUsers(users);
        } else {
          this.users = [];
        }

        return res;
      } finally {
        this.loading = false;
      }
    },

    async searchDirectoryUsers(params: {
      search: string;
      page: number;
      pageSize?: number;
    }) {
      const pageSize = params.pageSize ?? DIRECTORY_USERS_PAGE_SIZE;
      const seq = ++this.directorySearchSeq;
      this.directoryLoading = true;
      this.search = params.search;
      this.page = params.page;

      try {
        const res = await service.searchUsers({
          search: params.search,
          page: params.page,
          pageSize,
        });

        if (seq !== this.directorySearchSeq) {
          return res;
        }

        const users = getApiData(res);
        if (isApiSuccess(res) && users) {
          const normalized = normalizeSocialUsers(users);
          this.directoryUsers = normalized;
          this.directoryHasMore = normalized.length === pageSize;
        } else {
          this.directoryUsers = [];
          this.directoryHasMore = false;
        }

        return res;
      } finally {
        if (seq === this.directorySearchSeq) {
          this.directoryLoading = false;
        }
      }
    },

    async renameFriend(userId: string, newLogin: string) {
      this.loading = true;

      try {
        const res = await service.renameFriend({ userId, login: newLogin });

        const updatedLogin = getApiData(res);
        if (isApiSuccess(res) && updatedLogin) {
          const friend = this.friends.find((f) => f.id === userId);
          if (friend) friend.login = updatedLogin;
        }

        return res;
      } finally {
        this.loading = false;
      }
    },
    // =========================
    // HELPERS
    // =========================

    clearUsers() {
      this.users = [];
    },

    clearDirectoryUsers() {
      this.directoryUsers = [];
      this.directoryHasMore = false;
    },

    clearFriends() {
      this.friends = [];
    },

    clearBlocked() {
      this.blocked = [];
    },

    clearAll() {
      this.users = [];
      this.directoryUsers = [];
      this.friends = [];
      this.blocked = [];
    },
  },
});
