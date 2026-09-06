<script setup lang="ts">
import { t } from "../i18n";
import { onBeforeUnmount, onMounted, ref, useId } from "vue";
defineProps<{ title: string; busy?: boolean }>();
const emit = defineEmits<{ close: [] }>();
const dialog = ref<HTMLDialogElement>();
const titleId = useId();
let opener: HTMLElement | null = null;
onMounted(() => {
  opener =
    document.activeElement instanceof HTMLElement
      ? document.activeElement
      : null;
  dialog.value?.showModal();
});
onBeforeUnmount(() => {
  dialog.value?.close();
  if (opener?.isConnected) opener.focus();
  else document.getElementById("main")?.focus();
});
</script>
<template>
  <dialog
    ref="dialog"
    :aria-labelledby="titleId"
    @cancel.prevent="!busy && emit('close')"
  >
    <div class="modal-heading">
      <h2 :id="titleId">{{ title }}</h2>
      <button
        type="button"
        class="icon-button"
        :aria-label="t('Kapat')"
        :disabled="busy"
        @click="emit('close')"
      >
        ×
      </button>
    </div>
    <slot />
  </dialog>
</template>
