import { ref } from "vue";
import { defineStore } from "pinia";
export const useNotifications = defineStore("notifications", () => {
  const message = ref("");
  let timer: ReturnType<typeof setTimeout> | undefined;
  function show(text: string) {
    clearTimeout(timer);
    message.value = text;
    timer = setTimeout(() => {
      message.value = "";
    }, 5000);
  }
  return { message, show };
});
