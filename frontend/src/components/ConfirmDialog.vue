<script setup lang="ts">
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
    :title="title ?? 'İşlemi onaylayın'"
    :busy="busy"
    @close="$emit('close')"
    ><p v-if="error" class="error-state" role="alert">{{ error }}</p>
    <p>{{ message }}</p>
    <div class="form-actions">
      <button class="secondary" :disabled="busy" @click="$emit('close')">
        Vazgeç</button
      ><button class="danger" :disabled="busy" @click="$emit('confirm')">
        {{ busy ? "İşleniyor…" : "Onayla" }}
      </button>
    </div></BaseModal
  >
</template>
