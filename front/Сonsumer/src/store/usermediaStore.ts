import { defineStore } from "pinia";
import { UserService } from "@/service/userService";
import { MediaType } from "@/interface/models/profile/MediaType";

const service = new UserService();

export const useMediaStore = defineStore("media", {
  state: () => ({
    loading: false,
  }),

  actions: {

    // =====================
    // UPLOAD (ADD)
    // =====================
    async uploadMedia(file: File, mediaType: MediaType) {
      this.loading = true;
      try {
        const form = new FormData();
        form.append("file", file);
        form.append("mediaType", mediaType);

        return await service.uploadMedia(form);
      } finally {
        this.loading = false;
      }
    },

    // =====================
    // GET ALL MEDIA
    // =====================
    async getMyMedia() {
      this.loading = true;
      try {
        return await service.getMyMedia();
      } finally {
        this.loading = false;
      }
    },

    // =====================
    // DELETE CURRENT (helper)
    // =====================
    async deleteCurrent(mediaId: number) {
      this.loading = true;
      try {
        return await service.deleteCurrentMedia(mediaId);
      } finally {
        this.loading = false;
      }
    },

    // =====================
    // REPLACE CURRENT (FIXED 🔥)
    // =====================
    async replaceCurrent(mediaId: number, file: File, mediaType: MediaType) {
      this.loading = true;
      try {
        return await service.replaceCurrentMedia(mediaId, file, mediaType);
      } finally {
        this.loading = false;
      }
    },

    // =====================
    // DELETE ALL
    // =====================
    async deleteAll() {
      this.loading = true;
      try {
        return await service.deleteAllMedia();
      } finally {
        this.loading = false;
      }
    },
  },
});
