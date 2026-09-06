import { computed, ref } from "vue";
import { defineStore } from "pinia";
import { session } from "../api/session";
import type { AuthResponse, User } from "../types/models";
export const useAuthStore = defineStore("auth", () => {
  const user = ref<User | null>(null);
  const token = ref<string | null>(null);
  let expiryTimer: ReturnType<typeof setTimeout> | undefined;
  const isAuthenticated = computed(() => token.value !== null);
  function clear() {
    clearTimeout(expiryTimer);
    user.value = null;
    token.value = null;
    session.setToken(null);
  }
  function accept(auth: AuthResponse) {
    clear();
    user.value = auth.user;
    token.value = auth.token;
    session.setToken(auth.token);
    expiryTimer = setTimeout(
      () => session.expire(),
      Math.max(0, new Date(auth.expiresAt).getTime() - Date.now()),
    );
  }
  return { user, token, isAuthenticated, accept, clear };
});
