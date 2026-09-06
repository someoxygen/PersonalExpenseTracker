import { test, expect } from "@playwright/test";
test("register, me, logout, invalid login and valid login", async ({
  page,
}) => {
  const email = `browser-${Date.now()}@example.com`;
  const password = "Browser-test-password!";
  await page.goto("/dashboard");
  await expect(page).toHaveURL(/\/login/);
  await page.getByRole("link", { name: "Hesap oluşturun" }).click();
  await page.getByRole("textbox", { name: "Ad", exact: true }).fill("Browser");
  await page.getByRole("textbox", { name: "Soyad" }).fill("Test");
  await page.getByRole("textbox", { name: "E-posta" }).fill(email);
  await page.getByLabel("Parola", { exact: true }).fill(password);
  await page
    .getByRole("button", { name: "Hesap oluştur", exact: true })
    .click();
  await expect(page).toHaveURL(/\/dashboard$/);
  await expect(page.getByText("Toplam bakiye", { exact: true })).toBeVisible();
  await expect(
    page.getByRole("heading", { name: "Henüz gider yok" }),
  ).toBeVisible();
  await page.getByRole("link", { name: "Profil", exact: true }).click();
  await expect(
    page.getByRole("definition").filter({ hasText: email.toUpperCase() }),
  ).toBeVisible();
  await page.getByRole("button", { name: "Çıkış yap" }).click();
  await expect(page).toHaveURL(/\/login$/);
  await page.getByRole("textbox", { name: "E-posta" }).fill(email);
  await page.getByLabel("Parola", { exact: true }).fill("wrong-password");
  await page.getByRole("button", { name: "Giriş yap", exact: true }).click();
  await expect(page.getByRole("alert")).toBeVisible();
  await page.getByLabel("Parola", { exact: true }).fill(password);
  await page.getByRole("button", { name: "Giriş yap", exact: true }).click();
  await expect(page).toHaveURL(/\/dashboard$/);
  // The session is intentionally memory-only.
  await page.reload();
  await expect(page).toHaveURL(/\/login/);
});
