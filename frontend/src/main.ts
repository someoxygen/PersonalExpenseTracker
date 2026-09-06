import { createApp } from "vue";
import { createPinia } from "pinia";
import { router } from "./router";
import { useAuthStore } from "./stores/auth";
import { session } from "./api/session";
import "./style.css";
import App from "./App.vue";
const app = createApp(App);
app.use(createPinia());
session.onUnauthorized(() => {
  useAuthStore().clear();
  void router.replace({
    name: "login",
    query: { redirect: router.currentRoute.value.fullPath },
  });
});
app.use(router).mount("#app");
