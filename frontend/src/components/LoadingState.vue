<script setup lang="ts">
withDefaults(defineProps<{ label?: string }>(), { label: "Yükleniyor…" });
</script>
<template>
  <div class="loading-state" role="status" aria-live="polite">
    <span class="loading-dot" aria-hidden="true"></span>{{ label }}
  </div>
</template>
