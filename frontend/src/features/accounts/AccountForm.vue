<script setup lang="ts">
import { reactive } from "vue";
import type { AccountInput } from "./accountsApi";
const props = defineProps<{
  initial?: AccountInput;
  currency: string;
  busy: boolean;
}>();
defineEmits<{ submit: [input: AccountInput]; cancel: [] }>();
const form = reactive<AccountInput>({
  name: props.initial?.name ?? "",
  type: props.initial?.type ?? "Bank",
  initialBalance: props.initial?.initialBalance ?? 0,
  currency: props.initial?.currency ?? props.currency,
  isActive: props.initial?.isActive ?? true,
});
</script>
<template>
  <form
    @submit.prevent="
      !busy &&
      $emit('submit', { ...form, initialBalance: Number(form.initialBalance) })
    "
  >
    <fieldset :disabled="busy" style="border: 0; padding: 0; margin: 0">
      <div class="form-grid">
        <label class="field full"
          >Hesap adı<input v-model.trim="form.name" required maxlength="100"
        /></label>
        <label class="field"
          >Hesap türü<select v-model="form.type" required>
            <option value="Cash">Nakit</option>
            <option value="Bank">Banka</option>
            <option value="CreditCard">Kredi kartı</option>
            <option value="Savings">Birikim</option>
            <option value="Other">Diğer</option>
          </select></label
        >
        <label class="field"
          >Başlangıç bakiyesi<input
            v-model.number="form.initialBalance"
            type="number"
            step=".01"
            required
        /></label>
        <label class="field"
          >Para birimi<input :value="form.currency" readonly
        /></label>
        <label class="check"
          ><input v-model="form.isActive" type="checkbox" />Hesap aktif</label
        >
      </div>
      <p class="muted" style="margin-top: 1rem">
        Başlangıç bakiyesi düzenlendiğinde toplam bakiye yeniden hesaplanır.
        Kredi kartı borcunu negatif başlangıç bakiyesi olarak girebilirsiniz.
      </p>
      <div class="form-actions">
        <button type="button" class="secondary" @click="$emit('cancel')">
          Vazgeç</button
        ><button type="submit">{{ busy ? "Kaydediliyor…" : "Kaydet" }}</button>
      </div>
    </fieldset>
  </form>
</template>
