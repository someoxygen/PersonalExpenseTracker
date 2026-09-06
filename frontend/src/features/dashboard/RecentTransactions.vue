<script setup lang="ts">
import { categoryName } from "../../i18n/categories";
import { t } from "../../i18n";
import { RouterLink } from "vue-router";
import type { Transaction } from "../../types/models";
import { money, financialDate } from "../../utils/format";
import EmptyState from "../../components/EmptyState.vue";
defineProps<{ items: Transaction[]; currency: string }>();
</script>
<template>
  <section class="panel">
    <div class="panel-header">
      <h2>{{ t("Son işlemler") }}</h2>
      <RouterLink to="/transactions">{{ t("Tümünü gör →") }}</RouterLink>
    </div>
    <EmptyState
      v-if="!items.length"
      :title="t('İlk işleminizi ekleyin')"
      :description="t('Gelir ve gider geçmişiniz burada görünecek.')"
    />
    <div v-else class="table-wrap">
      <table class="responsive-table">
        <thead>
          <tr>
            <th>{{ t("İşlem") }}</th>
            <th>{{ t("Hesap") }}</th>
            <th>{{ t("Tarih") }}</th>
            <th>{{ t("Tutar") }}</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="item in items" :key="item.id">
            <td class="description">
              <strong>{{ item.description || categoryName(item) }}</strong
              ><br /><small>{{ categoryName(item) }}</small>
            </td>
            <td :data-label="t('Hesap')">{{ item.accountName }}</td>
            <td :data-label="t('Tarih')">
              {{ financialDate(item.transactionDate) }}
            </td>
            <td
              :data-label="t('Tutar')"
              class="amount"
              :class="item.type === 'Income' ? 'positive' : 'negative'"
            >
              {{ item.type === "Income" ? "+" : "−"
              }}{{ money(item.amount, currency) }}
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </section>
</template>
