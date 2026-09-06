<script setup lang="ts">
import { t } from "../i18n";
defineProps<{ label?: string }>();
</script>
<template>
  <div class="loading-state" role="status" aria-live="polite">
    <span class="loading-dot" aria-hidden="true"></span
    >{{ label ?? t("Yükleniyor…") }}
  </div>
</template>
