import { test, expect } from "@playwright/test";
test("manage accounts, transfers, categories and budgets on desktop and mobile", async ({
  page,
}) => {
  test.setTimeout(90_000);
  const failures: string[] = [];
  page.on("pageerror", (error) => failures.push(error.message));
  await page.goto("/register");
  await page.getByRole("textbox", { name: "Ad", exact: true }).fill("Deniz");
  await page.getByRole("textbox", { name: "Soyad" }).fill("Yılmaz");
  await page.getByLabel("E-posta").fill(`management-${Date.now()}@example.com`);
  await page
    .getByLabel("Parola", { exact: true })
    .fill("Browser-test-password!");
  await page
    .getByRole("button", { name: "Hesap oluştur", exact: true })
    .click();
  await expect(page).toHaveURL(/dashboard$/);
  await page.getByRole("link", { name: "Hesaplar", exact: true }).click();
  await page.getByRole("button", { name: "+ Hesap ekle" }).click();
  await page.getByRole("dialog").press("Escape");
  await expect(page.getByRole("dialog")).not.toBeVisible();
  await expect(
    page.getByRole("button", { name: "+ Hesap ekle" }),
  ).toBeFocused();
  for (const [name, balance] of [
    ["Banka hesabım", "2000"],
    ["Nakit", "0"],
  ]) {
    await page.getByRole("button", { name: "+ Hesap ekle" }).click();
    await page.getByLabel("Hesap adı", { exact: true }).fill(name!);
    await page.getByLabel("Başlangıç bakiyesi", { exact: true }).fill(balance!);
    await page.getByRole("button", { name: "Kaydet", exact: true }).click();
    await expect(page.getByRole("dialog")).not.toBeVisible();
    await expect(
      page.getByRole("heading", { name: name!, exact: true }),
    ).toBeVisible();
  }
  await page.getByRole("button", { name: "+ Transfer yap" }).click();
  await page
    .getByRole("combobox", { name: "Kaynak hesap", exact: true })
    .selectOption({ label: "Banka hesabım" });
  await page
    .getByRole("combobox", { name: "Hedef hesap", exact: true })
    .selectOption({ label: "Nakit" });
  await page.getByLabel("Tutar", { exact: true }).fill("100");
  await page.getByRole("button", { name: "Transferi kaydet" }).click();
  await expect(page.getByRole("dialog")).not.toBeVisible();
  await expect(
    page.getByRole("cell").filter({ hasText: "Banka hesabım → Nakit" }),
  ).toBeVisible();
  await page.getByRole("link", { name: "Genel bakış", exact: true }).click();
  await page.getByRole("link", { name: "+ İşlem ekle" }).click();
  await page.getByLabel("Tutar", { exact: true }).fill("250");
  await page
    .getByRole("combobox", { name: "Hesap", exact: true })
    .selectOption({ label: "Banka hesabım" });
  await page
    .getByRole("combobox", { name: "Kategori", exact: true })
    .selectOption({ label: "Market" });
  await page.getByLabel("Açıklama", { exact: true }).fill("Market alışverişi");
  await page.getByRole("button", { name: "Kaydet", exact: true }).click();
  await expect(page).toHaveURL(/transactions$/);
  await page.getByRole("link", { name: "Bütçeler", exact: true }).click();
  await page.getByRole("button", { name: "+ Bütçe ekle" }).click();
  await page
    .getByRole("combobox", { name: "Kategori", exact: true })
    .selectOption({ label: "Market" });
  await page.getByLabel("Bütçe tutarı", { exact: true }).fill("1000");
  await page.getByRole("button", { name: "Kaydet", exact: true }).click();
  await expect(page.getByRole("progressbar")).toHaveAttribute(
    "aria-valuenow",
    "25",
  );
  await page.getByRole("button", { name: "Düzenle", exact: true }).click();
  await page.getByLabel("Bütçe tutarı", { exact: true }).fill("500");
  await page.getByRole("button", { name: "Kaydet", exact: true }).click();
  await expect(page.getByRole("progressbar")).toHaveAttribute(
    "aria-valuenow",
    "50",
  );
  await page.getByRole("link", { name: "Kategoriler", exact: true }).click();
  await page.getByRole("button", { name: "+ Kategori ekle" }).click();
  await page.getByLabel("Kategori adı", { exact: true }).fill("Kitaplar");
  await page.getByRole("button", { name: "Kaydet", exact: true }).click();
  const category = page.getByRole("article").filter({
    has: page.getByRole("heading", { name: "Kitaplar", exact: true }),
  });
  await category.getByRole("button", { name: "Düzenle", exact: true }).click();
  await page.getByLabel("Kategori adı", { exact: true }).fill("Kitap ve dergi");
  await page.getByRole("button", { name: "Kaydet", exact: true }).click();
  await page
    .getByRole("article")
    .filter({ hasText: "Kitap ve dergi" })
    .getByRole("button", { name: "Sil", exact: true })
    .click();
  await page.getByRole("button", { name: "Onayla", exact: true }).click();
  await expect(
    page.getByRole("heading", { name: "Kitap ve dergi" }),
  ).not.toBeVisible();
  await page.getByRole("link", { name: "Genel bakış", exact: true }).click();
  await expect(
    page.getByRole("img", {
      name: "Son altı ayın gelir ve gider karşılaştırması",
    }),
  ).toBeVisible();
  await expect(page.getByRole("alert")).not.toBeVisible();
  await page.screenshot({
    path: "test-results/dashboard-desktop.png",
    fullPage: true,
  });
  await page.setViewportSize({ width: 390, height: 844 });
  await page.screenshot({
    path: "test-results/dashboard-mobile.png",
    fullPage: true,
  });
  expect(
    await page.evaluate(
      () => document.documentElement.scrollWidth <= window.innerWidth,
    ),
  ).toBe(true);
  await page.getByRole("button", { name: "Menü", exact: true }).click();
  await page.getByRole("link", { name: "Bütçeler", exact: true }).click();
  await page.getByRole("button", { name: "Sil", exact: true }).click();
  await page.getByRole("button", { name: "Onayla", exact: true }).click();
  await expect(
    page.getByRole("heading", { name: "Bu ay için bütçe yok" }),
  ).toBeVisible();
  expect(failures).toEqual([]);
});
