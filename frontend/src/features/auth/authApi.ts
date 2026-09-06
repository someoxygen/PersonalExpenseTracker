import { httpClient } from "../../api/httpClient";
import type { AuthResponse, User } from "../../types/models";
export interface LoginInput {
  email: string;
  password: string;
}
export interface RegisterInput extends LoginInput {
  firstName: string;
  lastName: string;
}
export const authApi = {
  login: async (input: LoginInput) =>
    (await httpClient.post<AuthResponse>("/api/auth/login", input)).data,
  register: async (input: RegisterInput) =>
    (await httpClient.post<AuthResponse>("/api/auth/register", input)).data,
  me: async () => (await httpClient.get<User>("/api/auth/me")).data,
};
