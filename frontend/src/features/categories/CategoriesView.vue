<script setup lang="ts">
import { categoryLabel } from "../../i18n/categories";
import { t } from "../../i18n";
import { computed, onMounted, ref } from "vue";
import { categoriesApi, type CategoryInput } from "./categoriesApi";
import type { Category, TransactionType } from "../../types/models";
import { useResource } from "../../composables/useResource";
import { useNotifications } from "../../stores/notifications";
import { errorMessage } from "../../utils/errors";
import CategoryForm from "./CategoryForm.vue";
import BaseModal from "../../components/BaseModal.vue";
import ConfirmDialog from "../../components/ConfirmDialog.vue";
import LoadingState from "../../components/LoadingState.vue";
import EmptyState from "../../components/EmptyState.vue";
import ErrorState from "../../components/ErrorState.vue";
const toast = useNotifications();
const { data, loading, error, load } = useResource(categoriesApi.list);
const type = ref<TransactionType>("Expense");
const visible = computed(
  () => data.value?.filter((c) => c.type === type.value) ?? [],
);
const editing = ref<Category>();
const pending = ref<Category>();
const open = ref(false);
const busy = ref(false);
const mutationError = ref("");
const icons: Record<string, string> = {
  tag: "◇",
  receipt: "▤",
  wallet: "▣",
  home: "⌂",
  star: "☆",
};
onMounted(load);
function edit(category?: Category) {
  editing.value = category;
  mutationError.value = "";
  open.value = true;
}
async function save(input: CategoryInput) {
  if (busy.value) return;
  busy.value = true;
  mutationError.value = "";
  try {
    if (editing.value) await categoriesApi.update(editing.value.id, input);
    else await categoriesApi.create(input);
    type.value = input.type;
    open.value = false;
    toast.show("Kategori kaydedildi.");
    await load();
  } catch (e) {
    mutationError.value = errorMessage(e);
  } finally {
    busy.value = false;
  }
}
async function remove() {
  if (!pending.value || busy.value) return;
  busy.value = true;
  mutationError.value = "";
  try {
    await categoriesApi.remove(pending.value.id);
    pending.value = undefined;
    toast.show("Kategori silindi.");
    await load();
  } catch (e) {
    mutationError.value = errorMessage(e);
  } finally {
    busy.value = false;
  }
}
</script>
<template>
  <header class="page-heading">
    <div>
      <p class="eyebrow">{{ t("DÜZENLEYİN") }}</p>
      <h1>{{ t("Kategoriler") }}</h1>
      <p class="muted">
        {{ t("Gelir ve giderlerinize anlamlı başlıklar verin.") }}
      </p>
    </div>
    <button @click="edit()">{{ t("+ Kategori ekle") }}</button>
  </header>
  <div class="tabs" :aria-label="t('Kategori türü')">
    <button
      :class="{ active: type === 'Expense' }"
      :aria-pressed="type === 'Expense'"
      @click="type = 'Expense'"
    >
      {{ t("Gider") }}</button
    ><button
      :class="{ active: type === 'Income' }"
      :aria-pressed="type === 'Income'"
      @click="type = 'Income'"
    >
      {{ t("Gelir") }}
    </button>
  </div>
  <LoadingState v-if="loading" /><ErrorState
    v-else-if="error"
    :message="error"
    retry
    @retry="load"
  />
  <template v-else
    ><EmptyState v-if="!visible.length" :title="t('Kategori bulunamadı')" />
    <div class="grid grid-3">
      <article v-for="category in visible" :key="category.id" class="panel">
        <div class="split">
          <span
            style="font-size: 1.6rem"
            :style="{ color: category.color }"
            aria-hidden="true"
            >{{ icons[category.icon] ?? "◇" }}</span
          ><span v-if="category.isSystem" class="tag">{{ t("Sistem") }}</span>
        </div>
        <h2 style="margin-top: 1rem">{{ categoryLabel(category) }}</h2>
        <small v-if="category.isSystem">{{
          t("Varsayılan kategori · değiştirilemez")
        }}</small>
        <div v-else class="row-actions">
          <button class="secondary small" @click="edit(category)">
            {{ t("Düzenle") }}</button
          ><button
            class="secondary small"
            @click="
              pending = category;
              mutationError = '';
            "
          >
            {{ t("Sil") }}
          </button>
        </div>
      </article>
    </div>
  </template>
  <BaseModal
    v-if="open"
    :title="editing ? t('Kategoriyi düzenle') : t('Yeni kategori')"
    :busy="busy"
    @close="open = false"
    ><ErrorState v-if="mutationError" :message="mutationError" /><CategoryForm
      :initial="editing"
      :type="type"
      :busy="busy"
      @submit="save"
      @cancel="open = false"
  /></BaseModal>
  <ConfirmDialog
    v-if="pending"
    :message="
      t('Kategori silinecek. İşlem veya bütçeye bağlı kategoriler silinemez.')
    "
    :busy="busy"
    :error="mutationError"
    @close="pending = undefined"
    @confirm="remove"
  />
</template>
