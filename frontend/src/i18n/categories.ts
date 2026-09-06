import { shallowRef } from "vue";
import type { Category } from "../types/models";
import { locale } from ".";

const systemIds = shallowRef(new Set<string>());
const names: Record<string, string> = {
  Market: "Groceries",
  Yemek: "Dining",
  Kira: "Rent",
  Fatura: "Bills",
  Ulaşım: "Transport",
  Eğlence: "Entertainment",
  Sağlık: "Health",
  Eğitim: "Education",
  Alışveriş: "Shopping",
  Diğer: "Other",
  Maaş: "Salary",
  Freelance: "Freelance",
  Yatırım: "Investment",
  Prim: "Bonus",
};
export function registerCategories(categories: Category[]) {
  systemIds.value = new Set(
    categories.filter((c) => c.isSystem).map((c) => c.id),
  );
}
export function categoryName(record: {
  categoryId: string;
  categoryName: string;
}): string {
  return locale.value === "en" && systemIds.value.has(record.categoryId)
    ? (names[record.categoryName] ?? record.categoryName)
    : record.categoryName;
}
export function categoryLabel(category: Category): string {
  return locale.value === "en" && category.isSystem
    ? (names[category.name] ?? category.name)
    : category.name;
}
