import { httpClient } from "../../api/httpClient";
import type {
  Transaction,
  TransactionInput,
  Page,
  TransactionType,
} from "../../types/models";
export interface TransactionFiltersModel {
  accountId: string;
  categoryId: string;
  type: TransactionType | "";
  startDate: string;
  endDate: string;
  minAmount: string;
  maxAmount: string;
  search: string;
  sortBy: string;
  sortDirection: string;
}
export const emptyFilters = (): TransactionFiltersModel => ({
  accountId: "",
  categoryId: "",
  type: "",
  startDate: "",
  endDate: "",
  minAmount: "",
  maxAmount: "",
  search: "",
  sortBy: "transactionDate",
  sortDirection: "desc",
});
export function transactionParams(
  filters: TransactionFiltersModel,
  page: number,
  pageSize: number,
) {
  return {
    ...Object.fromEntries(
      Object.entries(filters).filter(([, value]) => value !== ""),
    ),
    page,
    pageSize,
  };
}
export const transactionsApi = {
  list: async (
    filters: TransactionFiltersModel,
    page: number,
    pageSize: number,
  ) =>
    (
      await httpClient.get<Page<Transaction>>("/api/transactions", {
        params: transactionParams(filters, page, pageSize),
      })
    ).data,
  get: async (id: string) =>
    (await httpClient.get<Transaction>(`/api/transactions/${id}`)).data,
  create: async (input: TransactionInput) =>
    (await httpClient.post<Transaction>("/api/transactions", input)).data,
  update: async (id: string, input: TransactionInput) => {
    await httpClient.put(`/api/transactions/${id}`, input);
  },
  remove: async (id: string) => {
    await httpClient.delete(`/api/transactions/${id}`);
  },
};
