<script setup lang="ts">
import { categoryLabel } from "../../i18n/categories";
import { t } from "../../i18n";
import { computed, reactive } from "vue";
import type { Account, Category } from "../../types/models";
import { emptyFilters, type TransactionFiltersModel } from "./transactionsApi";
const props = defineProps<{ accounts: Account[]; categories: Category[] }>();
const emit = defineEmits<{ apply: [filters: TransactionFiltersModel] }>();
const form = reactive(emptyFilters());
const categories = computed(() =>
  props.categories.filter((x) => !form.type || x.type === form.type),
);
function reset() {
  Object.assign(form, emptyFilters());
  emit("apply", { ...form });
}
</script>
<template>
  <form
    class="panel"
    style="margin-bottom: 1.5rem"
    @submit.prevent="emit('apply', { ...form })"
  >
    <div class="filters">
      <label class="field"
        >{{ t("Ara")
        }}<input
          v-model="form.search"
          type="search"
          :placeholder="t('Açıklamada ara')"
          maxlength="200"
      /></label>
      <label class="field"
        >{{ t("Tür")
        }}<select v-model="form.type" @change="form.categoryId = ''">
          <option value="">{{ t("Tümü") }}</option>
          <option value="Income">{{ t("Gelir") }}</option>
          <option value="Expense">{{ t("Gider") }}</option>
        </select></label
      >
      <label class="field"
        >{{ t("Hesap")
        }}<select v-model="form.accountId">
          <option value="">{{ t("Tüm hesaplar") }}</option>
          <option v-for="a in accounts" :key="a.id" :value="a.id">
            {{ a.name }}{{ a.isActive ? "" : t(" (pasif)") }}
          </option>
        </select></label
      >
      <label class="field"
        >{{ t("Kategori")
        }}<select v-model="form.categoryId">
          <option value="">{{ t("Tüm kategoriler") }}</option>
          <option v-for="c in categories" :key="c.id" :value="c.id">
            {{ categoryLabel(c) }}
          </option>
        </select></label
      >
      <label class="field"
        >{{ t("Başlangıç")
        }}<input v-model="form.startDate" type="date" /></label
      ><label class="field"
        >{{ t("Bitiş")
        }}<input v-model="form.endDate" type="date" :min="form.startDate"
      /></label>
      <label class="field"
        >{{ t("En az tutar")
        }}<input
          v-model="form.minAmount"
          type="number"
          min="0"
          step=".01" /></label
      ><label class="field"
        >{{ t("En çok tutar")
        }}<input
          v-model="form.maxAmount"
          type="number"
          :min="form.minAmount || 0"
          step=".01"
      /></label>
      <label class="field"
        >{{ t("Sıralama")
        }}<select v-model="form.sortBy">
          <option value="transactionDate">{{ t("İşlem tarihi") }}</option>
          <option value="amount">{{ t("Tutar") }}</option>
          <option value="createdAt">{{ t("Eklenme tarihi") }}</option>
        </select></label
      >
      <label class="field"
        >{{ t("Yön")
        }}<select v-model="form.sortDirection">
          <option value="desc">{{ t("Azalan") }}</option>
          <option value="asc">{{ t("Artan") }}</option>
        </select></label
      >
    </div>
    <div class="row-actions">
      <button type="button" class="secondary" @click="reset">
        {{ t("Temizle") }}</button
      ><button type="submit">{{ t("Filtrele") }}</button>
    </div>
  </form>
</template>
