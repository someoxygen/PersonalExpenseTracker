import { httpClient } from "../../api/httpClient";
import type { Account } from "../../types/models";
export type AccountInput = Pick<
  Account,
  "name" | "type" | "initialBalance" | "currency" | "isActive"
>;
export const accountsApi = {
  list: async () => (await httpClient.get<Account[]>("/api/accounts")).data,
  create: async (input: AccountInput) =>
    (await httpClient.post<Account>("/api/accounts", input)).data,
  update: async (id: string, input: AccountInput) => {
    await httpClient.put(`/api/accounts/${id}`, input);
  },
  deactivate: async (id: string) => {
    await httpClient.delete(`/api/accounts/${id}`);
  },
};
