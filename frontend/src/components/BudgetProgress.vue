<script setup lang="ts">
import { categoryName } from "../i18n/categories";
import { t } from "../i18n";
import type { Budget } from "../types/models";
import { money, percentage } from "../utils/format";
defineProps<{ budget: Budget; currency: string }>();
</script>
<template>
  <div class="budget-row">
    <div class="split">
      <strong>{{ categoryName(budget) }}</strong
      ><span>{{ percentage(budget.percentage) }}</span>
    </div>
    <div
      class="progress-track"
      role="progressbar"
      :aria-label="t('{name} bütçe kullanımı', { name: categoryName(budget) })"
      :aria-valuenow="Math.min(100, budget.percentage)"
      aria-valuemin="0"
      aria-valuemax="100"
      :aria-valuetext="
        t('Yüzde {percentage} kullanıldı', { percentage: budget.percentage })
      "
    >
      <div
        class="progress-fill"
        :class="{ over: budget.percentage > 100 }"
        :style="{ width: Math.min(100, budget.percentage) + '%' }"
      ></div>
    </div>
    <div class="split muted">
      <small
        >{{ money(budget.spent, currency) }} /
        {{ money(budget.budgetAmount, currency) }}</small
      ><small :class="{ negative: budget.remaining < 0 }"
        >{{ money(budget.remaining, currency) }} {{ t("kalan") }}</small
      >
    </div>
  </div>
</template>
