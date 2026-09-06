import { test, expect, type Page } from "@playwright/test";

const user = {
  id: "user",
  firstName: "Ada",
  lastName: "Test",
  email: "ada@example.com",
  currency: "TRY",
};
const categories = [
  {
    id: "system",
    name: "Market",
    type: "Expense",
    icon: "tag",
    color: "#76956B",
    isSystem: true,
  },
  {
    id: "custom",
    name: "Yemek",
    type: "Expense",
    icon: "tag",
    color: "#76956B",
    isSystem: false,
  },
];
const account = {
  id: "account",
  name: "Banka hesabım",
  type: "Bank",
  initialBalance: 2000,
  currentBalance: 1234.5,
  currency: "TRY",
  isActive: true,
};
const transaction = {
  id: "transaction",
  accountId: account.id,
  accountName: account.name,
  categoryId: "system",
  categoryName: "Market",
  type: "Expense",
  amount: 765.5,
  description: null,
  transactionDate: "2026-09-06",
  createdAt: "2026-09-06T12:00:00Z",
};
const budget = {
  id: "budget",
  categoryId: "system",
  categoryName: "Market",
  budgetAmount: 1000,
  spent: 765.5,
  remaining: 234.5,
  percentage: 76.55,
  month: 9,
  year: 2026,
};

async function mockApi(page: Page) {
  await page.route(/^https?:\/\/[^/]+\/api\//, async (route) => {
    const path = new URL(route.request().url()).pathname;
    const data: Record<string, unknown> = {
      "/api/auth/login": {
        user,
        token: "test-token",
        expiresAt: new Date(Date.now() + 3600000).toISOString(),
      },
      "/api/auth/me": user,
      "/api/categories": categories,
      "/api/accounts": [account],
      "/api/transfers": {
        items: [],
        page: 1,
        pageSize: 10,
        totalCount: 0,
        totalPages: 0,
      },
      "/api/transactions": {
        items: [transaction],
        page: 1,
        pageSize: 20,
        totalCount: 1,
        totalPages: 1,
      },
      "/api/budgets": [budget],
      "/api/dashboard/summary": {
        currentBalance: 1234.5,
        thisMonthIncome: 2000,
        thisMonthExpense: 765.5,
        netBalance: 1234.5,
        budgetAmount: 1000,
        budgetSpent: 765.5,
        budgetPercentage: 76.55,
        currency: "TRY",
        financialDate: "2026-09-06",
      },
      "/api/dashboard/monthly": [
        { year: 2026, month: 9, income: 2000, expense: 765.5 },
      ],
      "/api/dashboard/category-expenses": [
        {
          categoryId: "system",
          categoryName: "Market",
          color: "#76956B",
          amount: 765.5,
          percentage: 100,
        },
      ],
      "/api/dashboard/recent-transactions": [transaction],
      "/api/dashboard/budgets": [budget],
    };
    if (!(path in data)) throw new Error(`Unexpected API request: ${path}`);
    await route.fulfill({ json: data[path] });
  });
}

async function loginEnglish(page: Page) {
  await mockApi(page);
  await page.goto("/login");
  await page
    .getByRole("combobox", { name: "Dil", exact: true })
    .selectOption("en");
  await page.getByLabel("Email", { exact: true }).fill(user.email);
  await page.getByLabel("Password", { exact: true }).fill("example-password");
  await page.getByRole("button", { name: "Log in", exact: true }).click();
  await expect(
    page.getByRole("heading", { name: "Hello, Ada." }),
  ).toBeVisible();
}

test("guest language switch preserves form input and survives navigation and reload", async ({
  page,
}) => {
  await page.goto("/login");
  await expect(page.locator("html")).toHaveAttribute("lang", "tr");
  await page.getByLabel("E-posta", { exact: true }).fill(user.email);
  await page
    .getByRole("combobox", { name: "Dil", exact: true })
    .selectOption("en");
  await expect(
    page.getByRole("heading", { name: "Welcome back." }),
  ).toBeVisible();
  await expect(page.getByLabel("Email", { exact: true })).toHaveValue(
    user.email,
  );
  await page
    .getByRole("link", { name: "Create an account", exact: true })
    .click();
  await expect(page.getByLabel("First name", { exact: true })).toBeVisible();
  await expect(page.getByText("Use at least 12 characters.")).toBeVisible();
  await page.reload();
  await expect(page.locator("html")).toHaveAttribute("lang", "en");
  await page
    .getByRole("combobox", { name: "Language", exact: true })
    .selectOption("tr");
  await expect(
    page.getByRole("heading", { name: "Yeni bir başlangıç." }),
  ).toBeVisible();
  await page.reload();
  await expect(page.locator("html")).toHaveAttribute("lang", "tr");
});

test("signed-in pages, formats, system categories and dialogs follow the selected language", async ({
  page,
}) => {
  const errors: string[] = [];
  page.on("pageerror", (error) => errors.push(error.message));
  await loginEnglish(page);
  await expect(page.getByText("Total balance", { exact: true })).toBeVisible();
  await expect(page.getByText("TRY", { exact: false }).first()).toBeVisible();
  await expect(
    page.getByText("Sep 6, 2026", { exact: false }).first(),
  ).toBeVisible();
  await expect(
    page.getByText("Groceries", { exact: true }).first(),
  ).toBeVisible();
  await expect(page.getByText("76.55%", { exact: true })).toBeVisible();
  await page.screenshot({
    path: "test-results/i18n-dashboard-en.png",
    fullPage: true,
  });
  await page
    .getByRole("combobox", { name: "Language", exact: true })
    .selectOption("tr");
  await expect(page.getByText("Toplam bakiye", { exact: true })).toBeVisible();
  await expect(page.getByText("Market", { exact: true }).first()).toBeVisible();
  await expect(page.getByText("%76,55", { exact: true })).toBeVisible();
  await page
    .getByRole("combobox", { name: "Dil", exact: true })
    .selectOption("en");

  await page.getByRole("link", { name: "Accounts", exact: true }).click();
  await expect(page.getByRole("heading", { name: account.name })).toBeVisible();
  await expect(page.getByText("Bank", { exact: true })).toBeVisible();
  await page.getByRole("button", { name: "+ Add account" }).click();
  await expect(page.getByRole("dialog", { name: "New account" })).toBeVisible();
  await page.getByLabel("Account name", { exact: true }).fill("Hesabım");
  await page.getByRole("button", { name: "Cancel", exact: true }).click();

  await page.getByRole("link", { name: "Categories", exact: true }).click();
  await expect(
    page.getByRole("heading", { name: "Groceries", exact: true }),
  ).toBeVisible();
  await expect(
    page.getByRole("heading", { name: "Yemek", exact: true }),
  ).toBeVisible();
  await page.getByRole("button", { name: "+ Add category" }).click();
  await expect(page.getByLabel("Category name", { exact: true })).toBeVisible();
  await page.getByRole("button", { name: "Cancel", exact: true }).click();

  await page.getByRole("link", { name: "Budgets", exact: true }).click();
  await page.getByRole("button", { name: "+ Add budget" }).click();
  await expect(page.getByLabel("Budget amount", { exact: true })).toBeVisible();
  await expect(
    page
      .getByRole("combobox", { name: "Category", exact: true })
      .locator("option"),
  ).toContainText(["Select an expense category", "Groceries", "Yemek"]);
  await page.getByRole("button", { name: "Cancel", exact: true }).click();

  await page.getByRole("link", { name: "Transactions", exact: true }).click();
  await expect(page.getByPlaceholder("Search descriptions")).toBeVisible();
  await page
    .getByRole("button", { name: "Delete Groceries", exact: true })
    .click();
  await expect(
    page.getByRole("dialog", { name: "Confirm action" }),
  ).toContainText("permanently deleted");
  await page.getByRole("button", { name: "Cancel", exact: true }).click();
  await page.getByRole("link", { name: "+ Add transaction" }).click();
  await expect(
    page.getByRole("combobox", { name: "Transaction type", exact: true }),
  ).toBeVisible();
  await expect(
    page.getByPlaceholder("A short note about the transaction"),
  ).toBeVisible();

  await page.getByRole("link", { name: "Profile", exact: true }).click();
  await expect(page.getByText("Full name", { exact: true })).toBeVisible();
  await page.getByRole("button", { name: "Log out", exact: true }).click();
  await expect(
    page.getByRole("heading", { name: "Welcome back." }),
  ).toBeVisible();
  expect(errors).toEqual([]);
});

test("mobile language selector remains visible with the menu closed", async ({
  page,
}) => {
  await page.setViewportSize({ width: 390, height: 844 });
  await loginEnglish(page);
  await expect(
    page.getByRole("combobox", { name: "Language", exact: true }),
  ).toBeVisible();
  await page
    .getByRole("combobox", { name: "Language", exact: true })
    .selectOption("tr");
  await expect(
    page.getByRole("heading", { name: "Merhaba, Ada." }),
  ).toBeVisible();
  await page
    .getByRole("combobox", { name: "Dil", exact: true })
    .selectOption("en");
  expect(
    await page.evaluate(
      () => document.documentElement.scrollWidth <= window.innerWidth,
    ),
  ).toBe(true);
  await page.screenshot({
    path: "test-results/i18n-mobile-en.png",
    fullPage: true,
  });
});

test("errors update with language and blocked storage does not prevent switching", async ({
  page,
}) => {
  await page.addInitScript(() => {
    const getItem = Storage.prototype.getItem;
    const setItem = Storage.prototype.setItem;
    Storage.prototype.getItem = function (key) {
      if (key === "expense-tracker-language")
        throw new Error("Storage blocked");
      return getItem.call(this, key);
    };
    Storage.prototype.setItem = function (key, value) {
      if (key === "expense-tracker-language")
        throw new Error("Storage blocked");
      return setItem.call(this, key, value);
    };
  });
  await page.route("**/api/auth/login", (route) =>
    route.fulfill({
      status: 401,
      json: { title: "Invalid email or password." },
    }),
  );
  await page.goto("/login");
  await page.getByLabel("E-posta", { exact: true }).fill(user.email);
  await page.getByLabel("Parola", { exact: true }).fill("wrong-password");
  await page.getByRole("button", { name: "Giriş yap", exact: true }).click();
  await expect(page.getByRole("alert")).toContainText("geçersiz");
  await page
    .getByRole("combobox", { name: "Dil", exact: true })
    .selectOption("en");
  await expect(page.getByRole("alert")).toContainText(
    "Your session or login details are invalid",
  );
  await expect(page.locator("html")).toHaveAttribute("lang", "en");
});
