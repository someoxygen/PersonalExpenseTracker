import { test, expect } from "@playwright/test";
test("create, filter, edit and delete a transaction through the UI", async ({
  page,
  request,
}) => {
  const email = `transactions-${Date.now()}@example.com`;
  const password = "Browser-test-password!";
  const registration = await request.post(
    "http://localhost:5080/api/auth/register",
    { data: { firstName: "Finance", lastName: "Test", email, password } },
  );
  expect(registration.status()).toBe(201);
  const auth = (await registration.json()) as { token: string };
  const account = await request.post("http://localhost:5080/api/accounts", {
    headers: { Authorization: `Bearer ${auth.token}` },
    data: {
      name: "Test Bank",
      type: "Bank",
      initialBalance: 1000,
      currency: "TRY",
    },
  });
  expect(account.status()).toBe(201);
  await page.goto("/login");
  await page.getByLabel("E-posta").fill(email);
  await page.getByLabel("Parola", { exact: true }).fill(password);
  await page.getByRole("button", { name: "Giriş yap", exact: true }).click();
  await expect(page).toHaveURL(/dashboard$/);
  await page.getByRole("link", { name: "+ İşlem ekle", exact: true }).click();
  await page.getByLabel("Tutar", { exact: true }).fill("100");
  await page
    .getByRole("combobox", { name: "Hesap", exact: true })
    .selectOption({ label: "Test Bank" });
  await page
    .getByRole("combobox", { name: "Kategori", exact: true })
    .selectOption({ label: "Market" });
  await page.getByLabel("Açıklama", { exact: true }).fill("Weekly shop");
  await page.getByRole("button", { name: "Kaydet", exact: true }).click();
  await expect(
    page.getByRole("cell").filter({ hasText: "Weekly shop" }),
  ).toBeVisible();
  await page.getByRole("link", { name: "Weekly shop düzenle" }).click();
  await page.getByLabel("Tutar", { exact: true }).fill("150");
  await page.getByRole("button", { name: "Kaydet", exact: true }).click();
  await page.getByLabel("Ara", { exact: true }).fill("No matching transaction");
  await page.getByRole("button", { name: "Filtrele", exact: true }).click();
  await expect(
    page.getByRole("heading", { name: "İşlem bulunamadı" }),
  ).toBeVisible();
  await page.getByRole("button", { name: "Temizle", exact: true }).click();
  await page.getByRole("button", { name: "Weekly shop sil" }).click();
  await expect(page.getByRole("dialog")).toBeVisible();
  await page.getByRole("button", { name: "Onayla", exact: true }).click();
  await expect(
    page.getByRole("heading", { name: "İşlem bulunamadı" }),
  ).toBeVisible();
});
