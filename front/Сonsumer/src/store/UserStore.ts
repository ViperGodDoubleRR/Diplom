import { defineStore } from "pinia";
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
    // =========================
    // GET USER
    // =========================
    async getMy() {
  try {
    this.loading = true;

    const response = await service.getMe();

    if (response.success && response.data) {
      const mediaStore = useMediaStore();
      const mediaRes = await mediaStore.getMyMedia();

      this.user = {
        ...response.data,
        media: mediaRes.success ? (mediaRes.data ?? []) : [],
      };
    } else {
      this.user = null;
    }
  } finally {
    this.loading = false;
  }
},

    // =========================
    // UPDATE PROFILE
    // =========================
    async updateProfile(dto: UpdateUserDto) {
      try {
        this.updateLoading = true;

        const response = await service.updateMe(dto);

        if (response.success && response.data) {
          this.user = {
  ...response.data,
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
        friends: user.friends ?? [],
        blackList: user.blackList ?? [],
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
    },
  },
});
