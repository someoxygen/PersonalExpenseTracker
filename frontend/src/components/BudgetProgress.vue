<script setup lang="ts">
import type { Budget } from "../types/models";
import { money } from "../utils/format";
defineProps<{ budget: Budget; currency: string }>();
</script>
<template>
  <div class="budget-row">
    <div class="split">
      <strong>{{ budget.categoryName }}</strong
      ><span>%{{ budget.percentage }}</span>
    </div>
    <div
      class="progress-track"
      role="progressbar"
      :aria-label="`${budget.categoryName} bütçe kullanımı`"
      :aria-valuenow="Math.min(100, budget.percentage)"
      aria-valuemin="0"
      aria-valuemax="100"
      :aria-valuetext="`Yüzde ${budget.percentage} kullanıldı`"
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
        >{{ money(budget.remaining, currency) }} kalan</small
      >
    </div>
  </div>
</template>
