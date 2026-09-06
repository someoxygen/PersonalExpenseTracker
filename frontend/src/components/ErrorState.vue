<script setup lang="ts">
defineProps<{ message: string; retry?: boolean }>();
defineEmits<{ retry: [] }>();
</script>
<template>
  <div class="error-state" role="alert">
    <p>{{ message }}</p>
    <button v-if="retry" class="secondary" @click="$emit('retry')">
      Tekrar dene
    </button>
  </div>
</template>
