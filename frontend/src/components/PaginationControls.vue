<script setup lang="ts">
import { t } from "../i18n";
defineProps<{
  page: number;
  totalPages: number;
  totalCount: number;
  busy?: boolean;
}>();
defineEmits<{ change: [page: number] }>();
</script>
<template>
  <div class="pagination">
    <small
      >{{ totalCount }} {{ t("kayıt · Sayfa") }} {{ page }} /
      {{ Math.max(1, totalPages) }}</small
    >
    <div class="row-actions">
      <button
        class="secondary small"
        :disabled="busy || page <= 1"
        @click="$emit('change', page - 1)"
      >
        {{ t("Önceki") }}</button
      ><button
        class="secondary small"
        :disabled="busy || page >= totalPages"
        @click="$emit('change', page + 1)"
      >
        {{ t("Sonraki") }}
      </button>
    </div>
  </div>
</template>
