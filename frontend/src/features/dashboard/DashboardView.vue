<script setup lang="ts">
import { onMounted } from "vue";
import { RouterLink } from "vue-router";
import { dashboardApi } from "./dashboardApi";
import { useResource } from "../../composables/useResource";
import { useAuthStore } from "../../stores/auth";
import { financialDate, money } from "../../utils/format";
import LoadingState from "../../components/LoadingState.vue";
import ErrorState from "../../components/ErrorState.vue";
import EmptyState from "../../components/EmptyState.vue";
import BudgetProgress from "../../components/BudgetProgress.vue";
import DashboardSummaryCards from "./DashboardSummaryCards.vue";
import MonthlyFinanceChart from "./MonthlyFinanceChart.vue";
import ExpenseCategoryChart from "./ExpenseCategoryChart.vue";
import RecentTransactions from "./RecentTransactions.vue";
const auth = useAuthStore();
const { data, loading, error, load } = useResource(async () => {
  const [summary, monthly, categories, recent, budgets] = await Promise.all([
    dashboardApi.summary(),
    dashboardApi.monthly(),
    dashboardApi.categories(),
    dashboardApi.recent(),
    dashboardApi.budgets(),
  ]);
  return { summary, monthly, categories, recent, budgets };
});
onMounted(load);
</script>
<template>
  <header class="page-heading">
    <div>
      <p class="eyebrow">GENEL BAKIŞ</p>
      <h1>Merhaba, {{ auth.user?.firstName }}.</h1>
      <p class="muted">
        Finansal durumunuza bir bakış<span v-if="data">
          · {{ financialDate(data.summary.financialDate) }}</span
        >
      </p>
    </div>
    <RouterLink class="button" to="/transactions/new">+ İşlem ekle</RouterLink>
  </header>
  <LoadingState
    v-if="loading"
    label="Finansal özetiniz hazırlanıyor…"
  /><ErrorState v-else-if="error" :message="error" retry @retry="load" />
  <template v-else-if="data">
    <DashboardSummaryCards :summary="data.summary" />
    <div class="grid grid-2">
      <MonthlyFinanceChart
        :items="data.monthly"
        :currency="data.summary.currency"
      /><ExpenseCategoryChart
        :items="data.categories"
        :currency="data.summary.currency"
      />
    </div>
    <div class="grid grid-2" style="margin-top: 1.5rem">
      <RecentTransactions
        :items="data.recent"
        :currency="data.summary.currency"
      />
      <section class="panel">
        <div class="panel-header">
          <h2>Aylık bütçeler</h2>
          <RouterLink to="/budgets">Yönet →</RouterLink>
        </div>
        <p v-if="data.budgets.length" class="muted">
          {{ money(data.summary.budgetSpent, data.summary.currency) }}
          kullanıldı · %{{ data.summary.budgetPercentage }}
        </p>
        <EmptyState
          v-if="!data.budgets.length"
          title="Bütçenizi planlayın"
          description="Kategorilerinize aylık limitler belirleyin."
        />
        <BudgetProgress
          v-for="budget in data.budgets"
          :key="budget.id"
          :budget="budget"
          :currency="data.summary.currency"
        />
      </section>
    </div>
  </template>
</template>
