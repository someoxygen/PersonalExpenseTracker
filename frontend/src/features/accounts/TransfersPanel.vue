<script setup lang="ts">
import { computed, onMounted, reactive, ref } from "vue";
import type { Account } from "../../types/models";
import { transfersApi } from "./transfersApi";
import { useResource } from "../../composables/useResource";
import { useNotifications } from "../../stores/notifications";
import { errorMessage } from "../../utils/errors";
import { today, money, financialDate } from "../../utils/format";
import BaseModal from "../../components/BaseModal.vue";
import ErrorState from "../../components/ErrorState.vue";
import EmptyState from "../../components/EmptyState.vue";
import LoadingState from "../../components/LoadingState.vue";
import PaginationControls from "../../components/PaginationControls.vue";
const props = defineProps<{ accounts: Account[]; currency: string }>();
const emit = defineEmits<{ changed: [] }>();
const toast = useNotifications();
const active = computed(() => props.accounts.filter((a) => a.isActive));
const page = ref(1);
const { data, loading, error, load } = useResource(() =>
  transfersApi.list(page.value),
);
const open = ref(false);
const busy = ref(false);
const saveError = ref("");
const form = reactive({
  sourceAccountId: "",
  targetAccountId: "",
  amount: 0,
  transactionDate: today(),
  description: "",
});
onMounted(load);
function begin() {
  Object.assign(form, {
    sourceAccountId: "",
    targetAccountId: "",
    amount: 0,
    transactionDate: today(),
    description: "",
  });
  saveError.value = "";
  open.value = true;
}
async function save() {
  if (busy.value) return;
  if (form.sourceAccountId === form.targetAccountId) {
    saveError.value = "Kaynak ve hedef hesap farklı olmalı.";
    return;
  }
  busy.value = true;
  saveError.value = "";
  try {
    await transfersApi.create({ ...form, amount: Number(form.amount) });
    open.value = false;
    page.value = 1;
    toast.show("Transfer kaydedildi.");
    emit("changed");
    await load();
  } catch (e) {
    saveError.value = errorMessage(e);
  } finally {
    busy.value = false;
  }
}
function changePage(value: number) {
  page.value = value;
  void load();
}
</script>
<template>
  <section class="panel" style="margin-top: 1.5rem">
    <div class="panel-header">
      <h2>Transfer geçmişi</h2>
      <button :disabled="active.length < 2" @click="begin">
        + Transfer yap
      </button>
    </div>
    <p class="muted">
      Hesaplar arası hareketler gelir veya gider sayılmaz. Transfer için iki
      aktif hesap gerekir.
    </p>
    <LoadingState v-if="loading" /><ErrorState
      v-else-if="error"
      :message="error"
      retry
      @retry="load"
    />
    <template v-else-if="data"
      ><EmptyState v-if="!data.items.length" title="Henüz transfer yok" />
      <div v-else class="table-wrap">
        <table class="responsive-table">
          <thead>
            <tr>
              <th>Kaynak → hedef</th>
              <th>Tarih</th>
              <th>Tutar</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="item in data.items" :key="item.id">
              <td class="description">
                {{ item.sourceAccountName }} → {{ item.targetAccountName
                }}<br /><small>{{ item.description }}</small>
              </td>
              <td data-label="Tarih">
                {{ financialDate(item.transactionDate) }}
              </td>
              <td data-label="Tutar" class="amount">
                {{ money(item.amount, currency) }}
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      <PaginationControls
        :page="page"
        :total-count="data.totalCount"
        :total-pages="data.totalPages"
        @change="changePage"
      />
    </template>
    <BaseModal
      v-if="open"
      title="Hesaplar arası transfer"
      :busy="busy"
      @close="open = false"
      ><ErrorState v-if="saveError" :message="saveError" />
      <form @submit.prevent="save">
        <fieldset :disabled="busy" style="border: 0; margin: 0; padding: 0">
          <div class="form-grid">
            <label class="field"
              >Kaynak hesap<select v-model="form.sourceAccountId" required>
                <option value="" disabled>Seçin</option>
                <option v-for="a in active" :key="a.id" :value="a.id">
                  {{ a.name }}
                </option>
              </select></label
            >
            <label class="field"
              >Hedef hesap<select v-model="form.targetAccountId" required>
                <option value="" disabled>Seçin</option>
                <option
                  v-for="a in active"
                  :key="a.id"
                  :value="a.id"
                  :disabled="a.id === form.sourceAccountId"
                >
                  {{ a.name }}
                </option>
              </select></label
            >
            <label class="field"
              >Tutar<input
                v-model.number="form.amount"
                type="number"
                min=".01"
                step=".01"
                required
            /></label>
            <label class="field"
              >Tarih<input
                v-model="form.transactionDate"
                type="date"
                min="2000-01-01"
                max="2100-12-31"
                required
            /></label>
            <label class="field full"
              >Açıklama<textarea
                v-model="form.description"
                maxlength="500"
              ></textarea>
            </label>
          </div>
          <div class="form-actions">
            <button type="button" class="secondary" @click="open = false">
              Vazgeç</button
            ><button type="submit">
              {{ busy ? "İşleniyor…" : "Transferi kaydet" }}
            </button>
          </div>
        </fieldset>
      </form>
    </BaseModal>
  </section>
</template>
