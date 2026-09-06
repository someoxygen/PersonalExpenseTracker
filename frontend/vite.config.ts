import vue from "@vitejs/plugin-vue";
import { defineConfig } from "vite";

// https://vite.dev/config/
export default defineConfig({
  plugins: [vue()],
  server: { host: "localhost", port: 5173, strictPort: true },
  preview: { host: "localhost", port: 4173, strictPort: true },
});
