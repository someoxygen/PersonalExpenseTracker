<script setup lang="ts">
import { t } from "../../i18n";
import { onMounted, ref } from "vue";
import { accountsApi, type AccountInput } from "./accountsApi";
import type { Account } from "../../types/models";
import { useAuthStore } from "../../stores/auth";
import { useNotifications } from "../../stores/notifications";
import { useResource } from "../../composables/useResource";
import { errorMessage } from "../../utils/errors";
import AccountCard from "./AccountCard.vue";
import AccountForm from "./AccountForm.vue";
import TransfersPanel from "./TransfersPanel.vue";
import BaseModal from "../../components/BaseModal.vue";
import ConfirmDialog from "../../components/ConfirmDialog.vue";
import LoadingState from "../../components/LoadingState.vue";
import EmptyState from "../../components/EmptyState.vue";
import ErrorState from "../../components/ErrorState.vue";
const auth = useAuthStore();
const toast = useNotifications();
const { data, loading, error, load } = useResource(accountsApi.list);
const editing = ref<Account>();
const open = ref(false);
const pending = ref<Account>();
const busy = ref(false);
const mutationError = ref("");
onMounted(load);
function edit(account?: Account) {
  editing.value = account;
  mutationError.value = "";
  open.value = true;
}
async function save(input: AccountInput) {
  if (busy.value) return;
  busy.value = true;
  mutationError.value = "";
  try {
    if (editing.value) await accountsApi.update(editing.value.id, input);
    else await accountsApi.create(input);
    open.value = false;
    toast.show("Hesap kaydedildi.");
    await load();
  } catch (e) {
    mutationError.value = errorMessage(e);
  } finally {
    busy.value = false;
  }
}
async function deactivate() {
  if (!pending.value || busy.value) return;
  busy.value = true;
  mutationError.value = "";
  try {
    await accountsApi.deactivate(pending.value.id);
    pending.value = undefined;
    toast.show("Hesap pasife alındı.");
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
      <p class="eyebrow">{{ t("VARLIKLARINIZ") }}</p>
      <h1>{{ t("Hesaplar") }}</h1>
      <p class="muted">
        {{ t("Banka, nakit ve kart bakiyelerinizi takip edin.") }}
      </p>
    </div>
    <button @click="edit()">{{ t("+ Hesap ekle") }}</button>
  </header>
  <LoadingState v-if="loading" /><ErrorState
    v-else-if="error"
    :message="error"
    retry
    @retry="load"
  />
  <template v-else-if="data"
    ><EmptyState
      v-if="!data.length"
      :title="t('İlk hesabınızı oluşturun')"
      :description="t('Gelir ve giderleriniz bu hesaplara bağlanacak.')"
    />
    <div class="grid grid-3">
      <AccountCard
        v-for="account in data"
        :key="account.id"
        :account="account"
        @edit="edit(account)"
        @deactivate="
          pending = account;
          mutationError = '';
        "
      />
    </div>
  </template>
  <TransfersPanel
    v-if="data"
    :accounts="data"
    :currency="auth.user?.currency ?? 'TRY'"
    @changed="load"
  />
  <BaseModal
    v-if="open"
    :title="editing ? t('Hesabı düzenle') : t('Yeni hesap')"
    :busy="busy"
    @close="open = false"
    ><ErrorState v-if="mutationError" :message="mutationError" /><AccountForm
      :initial="editing"
      :currency="auth.user?.currency ?? 'TRY'"
      :busy="busy"
      @submit="save"
      @cancel="open = false"
  /></BaseModal>
  <ConfirmDialog
    v-if="pending"
    :message="
      t(
        'Hesap pasife alınacak. Geçmiş işlemler ve bakiye korunur; yeni işlemler için hesabı tekrar aktifleştirmeniz gerekir.',
      )
    "
    :busy="busy"
    :error="mutationError"
    @close="pending = undefined"
    @confirm="deactivate"
  />
</template>
