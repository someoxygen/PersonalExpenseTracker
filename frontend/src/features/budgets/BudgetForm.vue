<script setup lang="ts">
import { computed, reactive } from "vue";
import type { BudgetInput } from "./budgetsApi";
import type { Category } from "../../types/models";
const props = defineProps<{
  initial?: BudgetInput;
  categories: Category[];
  month: number;
  year: number;
  busy: boolean;
}>();
defineEmits<{ submit: [input: BudgetInput]; cancel: [] }>();
const form = reactive<BudgetInput>(
  props.initial
    ? { ...props.initial }
    : { categoryId: "", amount: 0, month: props.month, year: props.year },
);
const expenses = computed(() =>
  props.categories.filter((c) => c.type === "Expense"),
);
</script>
<template>
  <form
    @submit.prevent="
      !busy &&
      $emit('submit', {
        ...form,
        amount: Number(form.amount),
        month: Number(form.month),
        year: Number(form.year),
      })
    "
  >
    <fieldset :disabled="busy" style="border: 0; padding: 0; margin: 0">
      <div class="form-grid">
        <label class="field full"
          >Kategori<select v-model="form.categoryId" required>
            <option value="" disabled>Gider kategorisi seçin</option>
            <option v-for="c in expenses" :key="c.id" :value="c.id">
              {{ c.name }}
            </option>
          </select></label
        >
        <label class="field full"
          >Bütçe tutarı<input
            v-model.number="form.amount"
            type="number"
            step=".01"
            min=".01"
            required
        /></label>
        <label class="field"
          >Ay<input
            v-model.number="form.month"
            type="number"
            min="1"
            max="12"
            required /></label
        ><label class="field"
          >Yıl<input
            v-model.number="form.year"
            type="number"
            min="2000"
            max="2100"
            required
        /></label>
      </div>
      <div class="form-actions">
        <button type="button" class="secondary" @click="$emit('cancel')">
          Vazgeç</button
        ><button type="submit">{{ busy ? "Kaydediliyor…" : "Kaydet" }}</button>
      </div>
    </fieldset>
  </form>
</template>
