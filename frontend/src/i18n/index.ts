import { computed, readonly, ref, watch } from "vue";
import en from "./en.json";

export type Locale = "tr" | "en";
const storageKey = "expense-tracker-language";
function savedLocale(): Locale {
  try {
    return localStorage.getItem(storageKey) === "en" ? "en" : "tr";
  } catch {
    return "tr";
  }
}
const currentLocale = ref<Locale>(savedLocale());
export const locale = readonly(currentLocale);
export const intlLocale = computed(() =>
  locale.value === "en" ? "en-US" : "tr-TR",
);
export function setLocale(value: string) {
  if (value !== "tr" && value !== "en") return;
  currentLocale.value = value;
  try {
    localStorage.setItem(storageKey, value);
  } catch {
    // Language switching also works when browser storage is unavailable.
  }
}
watch(
  currentLocale,
  (value) => {
    document.documentElement.lang = value;
    document.title = t("Expense Tracker · Kişisel Harcama Takibi");
    document
      .querySelector('meta[name="description"]')
      ?.setAttribute(
        "content",
        t("Kişisel gelir, gider ve bütçe takip uygulaması."),
      );
  },
  { immediate: true },
);

// Turkish source text is the key and the fallback for untranslated messages.
export function t(
  key: string,
  params: Record<string, string | number> = {},
): string {
  const text =
    locale.value === "en" && Object.hasOwn(en, key)
      ? en[key as keyof typeof en]
      : key;
  return text.replace(/\{(\w+)\}/g, (match, name: string) =>
    String(params[name] ?? match),
  );
}
