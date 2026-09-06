<script setup lang="ts">
import { t } from "../i18n";
defineProps<{ message: string; retry?: boolean }>();
defineEmits<{ retry: [] }>();
</script>
<template>
  <div class="error-state" role="alert">
    <p>{{ t(message) }}</p>
    <button v-if="retry" class="secondary" @click="$emit('retry')">
      {{ t("Tekrar dene") }}
    </button>
  </div>
</template>
