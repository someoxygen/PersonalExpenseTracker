export type TransactionType = "Income" | "Expense";
export type AccountType = "Cash" | "Bank" | "CreditCard" | "Savings" | "Other";
export interface User {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  currency: string;
}
export interface AuthResponse {
  token: string;
  expiresAt: string;
  user: User;
}
export interface Category {
  id: string;
  name: string;
  type: TransactionType;
  icon: string;
  color: string;
  isSystem: boolean;
}
export interface Account {
  id: string;
  name: string;
  type: AccountType;
  initialBalance: number;
  currentBalance: number;
  currency: string;
  isActive: boolean;
}
export interface Transaction {
  id: string;
  accountId: string;
  accountName: string;
  categoryId: string;
  categoryName: string;
  type: TransactionType;
  amount: number;
  description: string | null;
  transactionDate: string;
  createdAt: string;
}
export interface TransactionInput {
  accountId: string;
  categoryId: string;
  type: TransactionType;
  amount: number;
  description: string | null;
  transactionDate: string;
}
export interface Page<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}
export interface Budget {
  id: string;
  categoryId: string;
  categoryName: string;
  budgetAmount: number;
  spent: number;
  remaining: number;
  percentage: number;
  month: number;
  year: number;
}
export interface Summary {
  currentBalance: number;
  thisMonthIncome: number;
  thisMonthExpense: number;
  netBalance: number;
  budgetAmount: number;
  budgetSpent: number;
  budgetPercentage: number;
  currency: string;
  financialDate: string;
}
export interface MonthlyFinance {
  year: number;
  month: number;
  income: number;
  expense: number;
}
export interface CategoryExpense {
  categoryId: string;
  categoryName: string;
  color: string;
  amount: number;
  percentage: number;
}
export interface Transfer {
  id: string;
  sourceAccountId: string;
  sourceAccountName: string;
  targetAccountId: string;
  targetAccountName: string;
  amount: number;
  transactionDate: string;
  description: string | null;
}
