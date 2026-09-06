<script setup lang="ts">
import { categoryLabel } from "../../i18n/categories";
import { t } from "../../i18n";
import { computed, reactive, watch } from "vue";
import type { Account, Category, TransactionInput } from "../../types/models";
import { today } from "../../utils/format";
const props = defineProps<{
  initial?: TransactionInput;
  accounts: Account[];
  categories: Category[];
  busy: boolean;
}>();
const emit = defineEmits<{ submit: [input: TransactionInput]; cancel: [] }>();
const form = reactive<TransactionInput>({
  accountId: props.initial?.accountId ?? "",
  categoryId: props.initial?.categoryId ?? "",
  type: props.initial?.type ?? "Expense",
  amount: props.initial?.amount ?? 0,
  description: props.initial?.description ?? "",
  transactionDate: props.initial?.transactionDate ?? today(),
});
const categories = computed(() =>
  props.categories.filter((x) => x.type === form.type),
);
watch(
  () => form.type,
  () => {
    if (!categories.value.some((x) => x.id === form.categoryId))
      form.categoryId = "";
  },
);
function submit() {
  if (
    props.busy ||
    !Number.isFinite(Number(form.amount)) ||
    Number(form.amount) <= 0
  )
    return;
  emit("submit", { ...form, amount: Number(form.amount) });
}
</script>
<template>
  <form @submit.prevent="submit">
    <fieldset :disabled="busy" style="border: 0; padding: 0; margin: 0">
      <div class="form-grid">
        <label class="field"
          >{{ t("İşlem türü")
          }}<select v-model="form.type" required>
            <option value="Expense">{{ t("Gider") }}</option>
            <option value="Income">{{ t("Gelir") }}</option>
          </select></label
        >
        <label class="field"
          >{{ t("Tutar")
          }}<input
            v-model.number="form.amount"
            type="number"
            min=".01"
            max="9999999999999999.99"
            step=".01"
            required
        /></label>
        <label class="field"
          >{{ t("Hesap")
          }}<select v-model="form.accountId" required>
            <option value="" disabled>{{ t("Hesap seçin") }}</option>
            <option
              v-for="a in accounts"
              :key="a.id"
              :value="a.id"
              :disabled="!a.isActive"
            >
              {{ a.name }}{{ a.isActive ? "" : t(" (pasif)") }}
            </option>
          </select></label
        >
        <label class="field"
          >{{ t("Kategori")
          }}<select v-model="form.categoryId" required>
            <option value="" disabled>{{ t("Kategori seçin") }}</option>
            <option v-for="c in categories" :key="c.id" :value="c.id">
              {{ categoryLabel(c) }}
            </option>
          </select></label
        >
        <label class="field"
          >{{ t("İşlem tarihi")
          }}<input
            v-model="form.transactionDate"
            type="date"
            min="2000-01-01"
            max="2100-12-31"
            required
        /></label>
        <label class="field full"
          >{{ t("Açıklama")
          }}<textarea
            v-model="form.description"
            maxlength="500"
            :placeholder="t('İşlem hakkında kısa bir not')"
          ></textarea>
        </label>
      </div>
      <p class="muted" style="margin-top: 1rem; font-size: 0.85rem">
        {{
          t(
            "İleri tarihli işlemler, tarihleri geldiğinde bakiye ve raporlara dahil edilir.",
          )
        }}
      </p>
      <div class="form-actions">
        <button type="button" class="secondary" @click="$emit('cancel')">
          {{ t("Vazgeç") }}</button
        ><button type="submit">
          {{ busy ? t("Kaydediliyor…") : t("Kaydet") }}
        </button>
      </div>
    </fieldset>
  </form>
</template>
