<script setup lang="ts">
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
        >Ara<input
          v-model="form.search"
          type="search"
          placeholder="Açıklamada ara"
          maxlength="200"
      /></label>
      <label class="field"
        >Tür<select v-model="form.type" @change="form.categoryId = ''">
          <option value="">Tümü</option>
          <option value="Income">Gelir</option>
          <option value="Expense">Gider</option>
        </select></label
      >
      <label class="field"
        >Hesap<select v-model="form.accountId">
          <option value="">Tüm hesaplar</option>
          <option v-for="a in accounts" :key="a.id" :value="a.id">
            {{ a.name }}{{ a.isActive ? "" : " (pasif)" }}
          </option>
        </select></label
      >
      <label class="field"
        >Kategori<select v-model="form.categoryId">
          <option value="">Tüm kategoriler</option>
          <option v-for="c in categories" :key="c.id" :value="c.id">
            {{ c.name }}
          </option>
        </select></label
      >
      <label class="field"
        >Başlangıç<input v-model="form.startDate" type="date" /></label
      ><label class="field"
        >Bitiş<input v-model="form.endDate" type="date" :min="form.startDate"
      /></label>
      <label class="field"
        >En az tutar<input
          v-model="form.minAmount"
          type="number"
          min="0"
          step=".01" /></label
      ><label class="field"
        >En çok tutar<input
          v-model="form.maxAmount"
          type="number"
          :min="form.minAmount || 0"
          step=".01"
      /></label>
      <label class="field"
        >Sıralama<select v-model="form.sortBy">
          <option value="transactionDate">İşlem tarihi</option>
          <option value="amount">Tutar</option>
          <option value="createdAt">Eklenme tarihi</option>
        </select></label
      >
      <label class="field"
        >Yön<select v-model="form.sortDirection">
          <option value="desc">Azalan</option>
          <option value="asc">Artan</option>
        </select></label
      >
    </div>
    <div class="row-actions">
      <button type="button" class="secondary" @click="reset">Temizle</button
      ><button type="submit">Filtrele</button>
    </div>
  </form>
</template>
