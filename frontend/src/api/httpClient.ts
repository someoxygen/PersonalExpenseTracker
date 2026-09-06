import axios from "axios";
import { session } from "./session";
const baseURL = import.meta.env.VITE_API_BASE_URL;
if (!baseURL)
  throw new Error("VITE_API_BASE_URL is required. Copy .env.example to .env.");
export const httpClient = axios.create({
  baseURL,
  timeout: 15_000,
  headers: { Accept: "application/json" },
});
httpClient.interceptors.request.use((config) => {
  const token = session.getToken();
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});
httpClient.interceptors.response.use(
  (response) => response,
  (error: unknown) => {
    if (
      axios.isAxiosError(error) &&
      error.response?.status === 401 &&
      !error.config?.url?.match(/auth\/(login|register)$/) &&
      error.config?.headers.Authorization === `Bearer ${session.getToken()}`
    )
      session.expire();
    return Promise.reject(error);
  },
);
