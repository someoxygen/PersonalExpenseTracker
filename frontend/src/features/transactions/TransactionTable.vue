<script setup lang="ts">
import { RouterLink } from "vue-router";
import type { Transaction } from "../../types/models";
import { money, financialDate } from "../../utils/format";
defineProps<{ items: Transaction[]; currency: string }>();
defineEmits<{ remove: [transaction: Transaction] }>();
</script>
<template>
  <div class="table-wrap">
    <table class="responsive-table">
      <thead>
        <tr>
          <th>Açıklama / kategori</th>
          <th>Hesap</th>
          <th>Tarih</th>
          <th>Tutar</th>
          <th>İşlemler</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="item in items" :key="item.id">
          <td class="description">
            <strong>{{ item.description || item.categoryName }}</strong
            ><br /><small>{{ item.categoryName }}</small>
          </td>
          <td data-label="Hesap">{{ item.accountName }}</td>
          <td data-label="Tarih">{{ financialDate(item.transactionDate) }}</td>
          <td
            data-label="Tutar"
            class="amount"
            :class="item.type === 'Income' ? 'positive' : 'negative'"
          >
            {{ item.type === "Income" ? "+" : "−"
            }}{{ money(item.amount, currency) }}
          </td>
          <td>
            <div class="row-actions">
              <RouterLink
                class="button secondary small"
                :to="`/transactions/${item.id}/edit`"
                :aria-label="`${item.description || item.categoryName} düzenle`"
                >Düzenle</RouterLink
              ><button
                class="secondary small"
                :aria-label="`${item.description || item.categoryName} sil`"
                @click="$emit('remove', item)"
              >
                Sil
              </button>
            </div>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>
