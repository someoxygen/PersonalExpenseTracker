<script setup lang="ts">
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
          >Kategori adı<input v-model.trim="form.name" required maxlength="80"
        /></label>
        <label class="field"
          >Tür<select v-model="form.type" :disabled="!!initial" required>
            <option value="Expense">Gider</option>
            <option value="Income">Gelir</option>
          </select></label
        >
        <label class="field"
          >Simge<select v-model="form.icon">
            <option value="tag">◇ Etiket</option>
            <option value="receipt">▤ Harcama</option>
            <option value="wallet">▣ Cüzdan</option>
            <option value="home">⌂ Ev</option>
            <option value="star">☆ Özel</option>
          </select></label
        >
        <label class="field"
          >Renk<input v-model="form.color" type="color" required
        /></label>
      </div>
      <p v-if="initial" class="muted" style="margin-top: 1rem">
        Kategori türü geçmiş kayıtların tutarlılığı için değiştirilemez.
      </p>
      <div class="form-actions">
        <button type="button" class="secondary" @click="$emit('cancel')">
          Vazgeç</button
        ><button type="submit">{{ busy ? "Kaydediliyor…" : "Kaydet" }}</button>
      </div>
    </fieldset>
  </form>
</template>
