<script setup lang="ts">
import { RouterLink } from "vue-router";
import type { Transaction } from "../../types/models";
import { money, financialDate } from "../../utils/format";
import EmptyState from "../../components/EmptyState.vue";
defineProps<{ items: Transaction[]; currency: string }>();
</script>
<template>
  <section class="panel">
    <div class="panel-header">
      <h2>Son işlemler</h2>
      <RouterLink to="/transactions">Tümünü gör →</RouterLink>
    </div>
    <EmptyState
      v-if="!items.length"
      title="İlk işleminizi ekleyin"
      description="Gelir ve gider geçmişiniz burada görünecek."
    />
    <div v-else class="table-wrap">
      <table class="responsive-table">
        <thead>
          <tr>
            <th>İşlem</th>
            <th>Hesap</th>
            <th>Tarih</th>
            <th>Tutar</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="item in items" :key="item.id">
            <td class="description">
              <strong>{{ item.description || item.categoryName }}</strong
              ><br /><small>{{ item.categoryName }}</small>
            </td>
            <td data-label="Hesap">{{ item.accountName }}</td>
            <td data-label="Tarih">
              {{ financialDate(item.transactionDate) }}
            </td>
            <td
              data-label="Tutar"
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
