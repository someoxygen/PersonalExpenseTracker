<script setup lang="ts">
import { categoryName } from "../../i18n/categories";
import { t } from "../../i18n";
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
          <th>{{ t("Açıklama / kategori") }}</th>
          <th>{{ t("Hesap") }}</th>
          <th>{{ t("Tarih") }}</th>
          <th>{{ t("Tutar") }}</th>
          <th>{{ t("İşlemler") }}</th>
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
          <td>
            <div class="row-actions">
              <RouterLink
                class="button secondary small"
                :to="`/transactions/${item.id}/edit`"
                :aria-label="
                  t('{name} düzenle', {
                    name: item.description || categoryName(item),
                  })
                "
                >{{ t("Düzenle") }}</RouterLink
              ><button
                class="secondary small"
                :aria-label="
                  t('{name} sil', {
                    name: item.description || categoryName(item),
                  })
                "
                @click="$emit('remove', item)"
              >
                {{ t("Sil") }}
              </button>
            </div>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>
