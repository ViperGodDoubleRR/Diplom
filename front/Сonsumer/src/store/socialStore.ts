import { defineStore } from "pinia";

import type { Friend } from "@/interface/models/profile/Friend";
import type { BlackList } from "@/interface/models/profile/BlackList";
import type { UserPreview } from "@/interface/models/profile/UserPreview";

import { SocialService } from "@/service/socialService";

const service = new SocialService();

export const useSocialStore = defineStore("social", {
  state: () => ({
    friends: [] as Friend[],
    blocked: [] as BlackList[],
    users: [] as UserPreview[],

    loading: false,

    page: 1,
    search: "",
  }),

  actions: {
    async getFriends() {
      this.loading = true;

      try {
        const res = await service.getFriends();

        if (res.success) {
          this.friends = res.data ?? [];
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

        if (res.success) {
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

        if (res.success) {
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

        if (res.success) {
          this.blocked = res.data ?? [];
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

        if (res.success) {
          await this.getBlocked();

          // optional:
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

        if (res.success) {
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

        if (res.success) {
          this.users = res.data ?? [];
        }

        return res;
      } finally {
        this.loading = false;
      }
    },
    async renameFriend(userId: string, newLogin: string) {
  this.loading = true;

  try {
    const res = await service.renameFriend({userId,login: newLogin,});

    if (res.success) {
      // обновим локально friends, чтобы UI не дергался
      const friend = this.friends.find(f => f.id === userId);

      if (friend) {
        friend.login = newLogin;
      }
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

    clearFriends() {
      this.friends = [];
    },

    clearBlocked() {
      this.blocked = [];
    },

    clearAll() {
      this.users = [];
      this.friends = [];
      this.blocked = [];
    },
  },
});
