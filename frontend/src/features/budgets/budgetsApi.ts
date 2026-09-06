import { httpClient } from "../../api/httpClient";
import type { Budget } from "../../types/models";
export interface BudgetInput {
  categoryId: string;
  amount: number;
  month: number;
  year: number;
}
export const budgetsApi = {
  list: async (month: number, year: number) =>
    (
      await httpClient.get<Budget[]>("/api/budgets", {
        params: { month, year },
      })
    ).data,
  create: async (input: BudgetInput) =>
    (await httpClient.post<Budget>("/api/budgets", input)).data,
  update: async (id: string, input: BudgetInput) => {
    await httpClient.put(`/api/budgets/${id}`, input);
  },
  remove: async (id: string) => {
    await httpClient.delete(`/api/budgets/${id}`);
  },
};
