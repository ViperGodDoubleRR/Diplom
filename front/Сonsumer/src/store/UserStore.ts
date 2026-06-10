import { defineStore } from "pinia";

import { resetChatHubSession, stopChatHub } from "@/composables/useChatHub";
import { getApiData, isApiSuccess } from "@/utils/apiHelpers";
import type { User } from "@/interface/models/profile/user";
import { UserService } from "@/service/userService";
import type { UpdateUserDto } from "@/interface/DTO/profile/UpdateUserDto";
import { useMediaStore } from "./usermediaStore";

const service = new UserService();

export const useUserStore = defineStore("user", {
  state: () => ({
    user: null as User | null,
    loading: false,
    updateLoading: false,
  }),

  getters: {
    isAuth: (state) => !!state.user,
  },

  actions: {
    async getMy() {
      try {
        this.loading = true;

        const response = await service.getMe();

        const profile = getApiData(response);

        if (isApiSuccess(response) && profile) {
          const mediaStore = useMediaStore();
          const mediaRes = await mediaStore.getMyMedia();

          this.user = {
            ...profile,
            media: isApiSuccess(mediaRes) ? (getApiData(mediaRes) ?? []) : [],
          };
        } else {
          this.user = null;
        }
      } finally {
        this.loading = false;
      }
    },

    async refreshMedia() {
      if (!this.user) return;

      const mediaStore = useMediaStore();
      const mediaRes = await mediaStore.getMyMedia();

      if (mediaRes.success) {
        this.user = {
          ...this.user,
          media: mediaRes.data ?? [],
        };
      }
    },

    async updateProfile(dto: UpdateUserDto) {
      try {
        this.updateLoading = true;

        const response = await service.updateMe(dto);

        const profile = getApiData(response);

        if (isApiSuccess(response) && profile) {
          this.user = {
            ...profile,
            media: this.user?.media ?? [],
          };
        }

        return response;
      } finally {
        this.updateLoading = false;
      }
    },

    setUser(user: User) {
      this.user = {
        ...user,
        media: user.media ?? [],
      };
    },

    clearUser() {
      this.user = null;
    },

    logout() {
      this.clearUser();
      localStorage.removeItem("accessToken");
      localStorage.removeItem("refreshToken");
      void stopChatHub();
      resetChatHubSession();
    },
  },
});
