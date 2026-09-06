import { httpClient } from "../../api/httpClient";
import type { Category } from "../../types/models";
export type CategoryInput = Pick<Category, "name" | "type" | "icon" | "color">;
export const categoriesApi = {
  list: async () => (await httpClient.get<Category[]>("/api/categories")).data,
  create: async (input: CategoryInput) =>
    (await httpClient.post<Category>("/api/categories", input)).data,
  update: async (id: string, input: CategoryInput) => {
    await httpClient.put(`/api/categories/${id}`, input);
  },
  remove: async (id: string) => {
    await httpClient.delete(`/api/categories/${id}`);
  },
};
