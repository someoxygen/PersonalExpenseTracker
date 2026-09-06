<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import { budgetsApi, type BudgetInput } from "./budgetsApi";
import { categoriesApi } from "../categories/categoriesApi";
import type { Budget } from "../../types/models";
import { useResource } from "../../composables/useResource";
import { useAuthStore } from "../../stores/auth";
import { useNotifications } from "../../stores/notifications";
import { errorMessage } from "../../utils/errors";
import { today, monthName } from "../../utils/format";
import BudgetForm from "./BudgetForm.vue";
import BudgetProgress from "../../components/BudgetProgress.vue";
import BaseModal from "../../components/BaseModal.vue";
import ConfirmDialog from "../../components/ConfirmDialog.vue";
import LoadingState from "../../components/LoadingState.vue";
import EmptyState from "../../components/EmptyState.vue";
import ErrorState from "../../components/ErrorState.vue";
const auth = useAuthStore();
const toast = useNotifications();
const period = today().split("-");
const month = ref(Number(period[1]));
const year = ref(Number(period[0]));
const { data, loading, error, load } = useResource(async () => ({
  budgets: await budgetsApi.list(month.value, year.value),
  categories: await categoriesApi.list(),
}));
const editing = ref<Budget>();
const pending = ref<Budget>();
const open = ref(false);
const initial = computed<BudgetInput | undefined>(() =>
  editing.value
    ? {
        categoryId: editing.value.categoryId,
        amount: editing.value.budgetAmount,
        month: editing.value.month,
        year: editing.value.year,
      }
    : undefined,
);
const busy = ref(false);
const mutationError = ref("");
onMounted(load);
function edit(budget?: Budget) {
  editing.value = budget;
  mutationError.value = "";
  open.value = true;
}
async function save(input: BudgetInput) {
  if (busy.value) return;
  busy.value = true;
  mutationError.value = "";
  try {
    if (editing.value) await budgetsApi.update(editing.value.id, input);
    else await budgetsApi.create(input);
    month.value = input.month;
    year.value = input.year;
    open.value = false;
    toast.show("Bütçe kaydedildi.");
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
    await budgetsApi.remove(pending.value.id);
    pending.value = undefined;
    toast.show("Bütçe silindi.");
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
      <p class="eyebrow">PLANLAYIN</p>
      <h1>Bütçeler</h1>
      <p class="muted">Aylık hedeflerinizin neresindesiniz?</p>
    </div>
    <button :disabled="!data" @click="edit()">+ Bütçe ekle</button>
  </header>
  <form
    class="form-grid"
    style="max-width: 460px; margin-bottom: 1.5rem"
    @submit.prevent="load"
  >
    <label class="field"
      >Ay<select v-model.number="month" @change="load">
        <option v-for="m in 12" :key="m" :value="m">{{ monthName(m) }}</option>
      </select></label
    ><label class="field"
      >Yıl<input
        v-model.number="year"
        type="number"
        min="2000"
        max="2100"
        required
        @change="load"
    /></label>
  </form>
  <LoadingState v-if="loading" /><ErrorState
    v-else-if="error"
    :message="error"
    retry
    @retry="load"
  />
  <template v-else-if="data"
    ><EmptyState
      v-if="!data.budgets.length"
      title="Bu ay için bütçe yok"
      description="Gider kategorilerinize bütçe belirleyerek başlayın."
    />
    <div class="grid grid-3">
      <article v-for="budget in data.budgets" :key="budget.id" class="panel">
        <BudgetProgress
          :budget="budget"
          :currency="auth.user?.currency ?? 'TRY'"
        />
        <div class="row-actions" style="margin-top: 1rem">
          <button class="secondary small" @click="edit(budget)">Düzenle</button
          ><button
            class="secondary small"
            @click="
              pending = budget;
              mutationError = '';
            "
          >
            Sil
          </button>
        </div>
      </article>
    </div>
  </template>
  <BaseModal
    v-if="open && data"
    :title="editing ? 'Bütçeyi düzenle' : 'Yeni bütçe'"
    :busy="busy"
    @close="open = false"
    ><ErrorState v-if="mutationError" :message="mutationError" /><BudgetForm
      :initial="initial"
      :categories="data.categories"
      :month="month"
      :year="year"
      :busy="busy"
      @submit="save"
      @cancel="open = false"
  /></BaseModal>
  <ConfirmDialog
    v-if="pending"
    message="Bütçe silinecek. İlgili gelir ve gider kayıtları korunur."
    :busy="busy"
    :error="mutationError"
    @close="pending = undefined"
    @confirm="remove"
  />
</template>
