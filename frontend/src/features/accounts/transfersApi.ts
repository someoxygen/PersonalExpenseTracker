import { httpClient } from "../../api/httpClient";
import type { Page, Transfer } from "../../types/models";
export interface TransferInput {
  sourceAccountId: string;
  targetAccountId: string;
  amount: number;
  transactionDate: string;
  description: string;
}
export const transfersApi = {
  list: async (page: number) =>
    (
      await httpClient.get<Page<Transfer>>("/api/transfers", {
        params: { page, pageSize: 10 },
      })
    ).data,
  create: async (input: TransferInput) =>
    (await httpClient.post<Transfer>("/api/transfers", input)).data,
};
