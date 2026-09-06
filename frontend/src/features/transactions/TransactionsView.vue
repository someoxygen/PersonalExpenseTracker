<script setup lang="ts">
import { t } from "../../i18n";
import { onMounted, reactive, ref } from "vue";
import { RouterLink } from "vue-router";
import {
  transactionsApi,
  emptyFilters,
  type TransactionFiltersModel,
} from "./transactionsApi";
import { accountsApi } from "../accounts/accountsApi";
import { categoriesApi } from "../categories/categoriesApi";
import { useResource } from "../../composables/useResource";
import { useAuthStore } from "../../stores/auth";
import { useNotifications } from "../../stores/notifications";
import type { Transaction } from "../../types/models";
import { errorMessage } from "../../utils/errors";
import TransactionFilters from "./TransactionFilters.vue";
import TransactionTable from "./TransactionTable.vue";
import LoadingState from "../../components/LoadingState.vue";
import EmptyState from "../../components/EmptyState.vue";
import ErrorState from "../../components/ErrorState.vue";
import ConfirmDialog from "../../components/ConfirmDialog.vue";
import PaginationControls from "../../components/PaginationControls.vue";
const auth = useAuthStore();
const toast = useNotifications();
const filters = reactive(emptyFilters());
const page = ref(1);
const lookup = useResource(async () => ({
  accounts: await accountsApi.list(),
  categories: await categoriesApi.list(),
}));
const { data, loading, error, load } = useResource(() =>
  transactionsApi.list(filters, page.value, 20),
);
const pendingDelete = ref<Transaction>();
const deleting = ref(false);
const deleteError = ref("");
onMounted(() => {
  void lookup.load();
  void load();
});
function apply(value: TransactionFiltersModel) {
  Object.assign(filters, value);
  page.value = 1;
  void load();
}
function changePage(value: number) {
  page.value = value;
  void load();
}
async function remove() {
  if (!pendingDelete.value || deleting.value) return;
  deleting.value = true;
  deleteError.value = "";
  try {
    await transactionsApi.remove(pendingDelete.value.id);
    pendingDelete.value = undefined;
    toast.show("İşlem silindi.");
    if (data.value?.items.length === 1 && page.value > 1) page.value--;
    await load();
  } catch (e) {
    deleteError.value = errorMessage(e);
  } finally {
    deleting.value = false;
  }
}
</script>
<template>
  <header class="page-heading">
    <div>
      <p class="eyebrow">{{ t("GELİR VE GİDER") }}</p>
      <h1>{{ t("İşlemler") }}</h1>
      <p class="muted">{{ t("Hareketlerinizi inceleyin ve yönetin.") }}</p>
    </div>
    <RouterLink class="button" to="/transactions/new">{{
      t("+ İşlem ekle")
    }}</RouterLink>
  </header>
  <ErrorState
    v-if="lookup.error.value"
    :message="lookup.error.value"
    retry
    @retry="lookup.load"
  />
  <TransactionFilters
    v-if="lookup.data.value"
    :accounts="lookup.data.value.accounts"
    :categories="lookup.data.value.categories"
    @apply="apply"
  />
  <section class="panel">
    <LoadingState v-if="loading" /><ErrorState
      v-else-if="error"
      :message="error"
      retry
      @retry="load"
    />
    <template v-else-if="data">
      <EmptyState
        v-if="!data.items.length"
        :title="t('İşlem bulunamadı')"
        :description="
          t('Yeni bir işlem ekleyin veya filtrelerinizi değiştirin.')
        "
      />
      <TransactionTable
        v-else
        :items="data.items"
        :currency="auth.user?.currency ?? 'TRY'"
        @remove="
          pendingDelete = $event;
          deleteError = '';
        "
      />
      <PaginationControls
        :page="page"
        :total-pages="data.totalPages"
        :total-count="data.totalCount"
        @change="changePage"
      />
    </template>
  </section>
  <ConfirmDialog
    v-if="pendingDelete"
    :message="
      t(
        'Bu işlem kalıcı olarak silinecek ve hesabınızın bakiyesi yeniden hesaplanacak.',
      )
    "
    :busy="deleting"
    :error="deleteError"
    @close="pendingDelete = undefined"
    @confirm="remove"
  />
</template>
