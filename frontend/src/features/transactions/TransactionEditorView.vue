<script setup lang="ts">
import { t } from "../../i18n";
import { computed, ref, watch } from "vue";
import { RouterLink, useRoute, useRouter } from "vue-router";
import { accountsApi } from "../accounts/accountsApi";
import { categoriesApi } from "../categories/categoriesApi";
import { transactionsApi } from "./transactionsApi";
import type { TransactionInput } from "../../types/models";
import { useResource } from "../../composables/useResource";
import { useNotifications } from "../../stores/notifications";
import { errorMessage } from "../../utils/errors";
import TransactionForm from "./TransactionForm.vue";
import LoadingState from "../../components/LoadingState.vue";
import ErrorState from "../../components/ErrorState.vue";
import EmptyState from "../../components/EmptyState.vue";
const route = useRoute();
const router = useRouter();
const toast = useNotifications();
const id = computed(() =>
  typeof route.params.id === "string" ? route.params.id : undefined,
);
const busy = ref(false);
const saveError = ref("");
const { data, loading, error, load } = useResource(async () => {
  const [accounts, categories, transaction] = await Promise.all([
    accountsApi.list(),
    categoriesApi.list(),
    id.value ? transactionsApi.get(id.value) : Promise.resolve(undefined),
  ]);
  return { accounts, categories, transaction };
});
watch(
  id,
  () => {
    saveError.value = "";
    void load();
  },
  { immediate: true },
);
async function save(input: TransactionInput) {
  if (busy.value) return;
  busy.value = true;
  saveError.value = "";
  try {
    if (id.value) await transactionsApi.update(id.value, input);
    else await transactionsApi.create(input);
    toast.show("İşlem kaydedildi.");
    await router.push("/transactions");
  } catch (e) {
    saveError.value = errorMessage(e);
  } finally {
    busy.value = false;
  }
}
</script>
<template>
  <header class="page-heading">
    <div>
      <p class="eyebrow">{{ t("İŞLEMLER") }}</p>
      <h1>{{ id ? t("İşlemi düzenle") : t("Yeni işlem") }}</h1>
    </div>
    <RouterLink to="/transactions">{{ t("← İşlemlere dön") }}</RouterLink>
  </header>
  <section class="panel" style="max-width: 780px">
    <LoadingState v-if="loading" /><ErrorState
      v-else-if="error"
      :message="error"
      retry
      @retry="load"
    />
    <template v-else-if="data">
      <ErrorState v-if="saveError" :message="saveError" />
      <EmptyState
        v-if="!data.accounts.some((a) => a.isActive)"
        :title="t('Önce bir hesap oluşturun')"
        :description="t('İşlem eklemek için aktif bir hesabınız olmalı.')"
        ><RouterLink class="button" to="/accounts">{{
          t("Hesaplara git")
        }}</RouterLink></EmptyState
      >
      <TransactionForm
        v-else
        :key="id ?? 'new'"
        :initial="data.transaction"
        :accounts="data.accounts"
        :categories="data.categories"
        :busy="busy"
        @submit="save"
        @cancel="router.push('/transactions')"
      />
    </template>
  </section>
</template>
