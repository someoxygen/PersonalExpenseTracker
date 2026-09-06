<script setup lang="ts">
import { t } from "../../i18n";
import type { Summary } from "../../types/models";
import { money } from "../../utils/format";
defineProps<{ summary: Summary }>();
</script>
<template>
  <div class="kpis">
    <section class="panel kpi">
      <p>{{ t("Toplam bakiye") }}</p>
      <strong>{{ money(summary.currentBalance, summary.currency) }}</strong>
    </section>
    <section class="panel kpi">
      <p>{{ t("Bu ay gelir") }}</p>
      <strong class="positive">{{
        money(summary.thisMonthIncome, summary.currency)
      }}</strong>
    </section>
    <section class="panel kpi">
      <p>{{ t("Bu ay gider") }}</p>
      <strong class="negative">{{
        money(summary.thisMonthExpense, summary.currency)
      }}</strong>
    </section>
    <section class="panel kpi">
      <p>{{ t("Bu ay net") }}</p>
      <strong>{{ money(summary.netBalance, summary.currency) }}</strong>
    </section>
  </div>
</template>
