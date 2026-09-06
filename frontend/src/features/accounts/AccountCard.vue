<script setup lang="ts">
import { t } from "../../i18n";
import type { Account, AccountType } from "../../types/models";
import { money } from "../../utils/format";
defineProps<{ account: Account }>();
defineEmits<{ edit: []; deactivate: [] }>();
const names: Record<AccountType, string> = {
  Cash: "Nakit",
  Bank: "Banka",
  CreditCard: "Kredi kartı",
  Savings: "Birikim",
  Other: "Diğer",
};
</script>
<template>
  <article class="panel">
    <div class="split">
      <span class="tag">{{ t(names[account.type]) }}</span
      ><span v-if="!account.isActive" class="tag">{{ t("Pasif") }}</span>
    </div>
    <h2 style="margin: 1.2rem 0 0.5rem">{{ account.name }}</h2>
    <strong
      style="font-size: 1.7rem; font-weight: 650"
      :class="{ negative: account.currentBalance < 0 }"
      >{{ money(account.currentBalance, account.currency) }}</strong
    >
    <p class="muted" style="margin-top: 0.5rem; font-size: 0.85rem">
      {{ t("Başlangıç:") }}
      {{ money(account.initialBalance, account.currency) }}
    </p>
    <div class="row-actions">
      <button class="secondary small" @click="$emit('edit')">
        {{ t("Düzenle") }}</button
      ><button
        v-if="account.isActive"
        class="secondary small"
        @click="$emit('deactivate')"
      >
        {{ t("Pasife al") }}
      </button>
    </div>
  </article>
</template>
