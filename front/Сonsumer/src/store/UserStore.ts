import { defineStore } from "pinia";
import type { User } from "@/models/User";

export const useUserStore = defineStore("user", {
  state: (): { user: User[] } => ({
    user: [],
  }),
  actions:{
    GetUser(){
      return this.user;
    }
  }
});
