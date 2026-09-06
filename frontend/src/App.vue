<script setup lang="ts">
import { RouterView } from "vue-router";
import { useNotifications } from "./stores/notifications";
const notifications = useNotifications();
</script>
<template>
  <a class="skip-link" href="#main">İçeriğe geç</a><RouterView />
  <div v-if="notifications.message" class="toast" role="status">
    {{ notifications.message }}
  </div>
</template>
