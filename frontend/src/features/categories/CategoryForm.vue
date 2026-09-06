<script setup lang="ts">
import { t } from "../../i18n";
import { reactive } from "vue";
import type { CategoryInput } from "./categoriesApi";
import type { TransactionType } from "../../types/models";
const props = defineProps<{
  initial?: CategoryInput;
  type: TransactionType;
  busy: boolean;
}>();
defineEmits<{ submit: [input: CategoryInput]; cancel: [] }>();
const form = reactive<CategoryInput>({
  name: props.initial?.name ?? "",
  type: props.initial?.type ?? props.type,
  icon: props.initial?.icon ?? "tag",
  color: props.initial?.color ?? "#76956B",
});
</script>
<template>
  <form @submit.prevent="!busy && $emit('submit', { ...form })">
    <fieldset :disabled="busy" style="border: 0; padding: 0; margin: 0">
      <div class="form-grid">
        <label class="field full"
          >{{ t("Kategori adı")
          }}<input v-model.trim="form.name" required maxlength="80"
        /></label>
        <label class="field"
          >{{ t("Tür")
          }}<select v-model="form.type" :disabled="!!initial" required>
            <option value="Expense">{{ t("Gider") }}</option>
            <option value="Income">{{ t("Gelir") }}</option>
          </select></label
        >
        <label class="field"
          >{{ t("Simge")
          }}<select v-model="form.icon">
            <option value="tag">{{ t("◇ Etiket") }}</option>
            <option value="receipt">{{ t("▤ Harcama") }}</option>
            <option value="wallet">{{ t("▣ Cüzdan") }}</option>
            <option value="home">{{ t("⌂ Ev") }}</option>
            <option value="star">{{ t("☆ Özel") }}</option>
          </select></label
        >
        <label class="field"
          >{{ t("Renk") }}<input v-model="form.color" type="color" required
        /></label>
      </div>
      <p v-if="initial" class="muted" style="margin-top: 1rem">
        {{
          t("Kategori türü geçmiş kayıtların tutarlılığı için değiştirilemez.")
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
