import { httpClient } from "../../api/httpClient";
import type {
  Summary,
  MonthlyFinance,
  CategoryExpense,
  Transaction,
  Budget,
} from "../../types/models";
export const dashboardApi = {
  summary: async () =>
    (await httpClient.get<Summary>("/api/dashboard/summary")).data,
  monthly: async () =>
    (await httpClient.get<MonthlyFinance[]>("/api/dashboard/monthly")).data,
  categories: async () =>
    (
      await httpClient.get<CategoryExpense[]>(
        "/api/dashboard/category-expenses",
      )
    ).data,
  recent: async () =>
    (await httpClient.get<Transaction[]>("/api/dashboard/recent-transactions"))
      .data,
  budgets: async () =>
    (await httpClient.get<Budget[]>("/api/dashboard/budgets")).data,
};
