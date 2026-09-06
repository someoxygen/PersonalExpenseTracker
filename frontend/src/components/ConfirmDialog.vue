<script setup lang="ts">
import { t } from "../i18n";
import BaseModal from "./BaseModal.vue";
defineProps<{
  title?: string;
  message: string;
  busy?: boolean;
  error?: string;
}>();
defineEmits<{ confirm: []; close: [] }>();
</script>
<template>
  <BaseModal
    :title="title ?? t('İşlemi onaylayın')"
    :busy="busy"
    @close="$emit('close')"
    ><p v-if="error" class="error-state" role="alert">{{ t(error) }}</p>
    <p>{{ message }}</p>
    <div class="form-actions">
      <button class="secondary" :disabled="busy" @click="$emit('close')">
        {{ t("Vazgeç") }}</button
      ><button class="danger" :disabled="busy" @click="$emit('confirm')">
        {{ busy ? t("İşleniyor…") : t("Onayla") }}
      </button>
    </div></BaseModal
  >
</template>
