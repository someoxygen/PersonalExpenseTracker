<script setup lang="ts">
import { t } from "./i18n";
import { RouterView } from "vue-router";
import { useNotifications } from "./stores/notifications";
const notifications = useNotifications();
</script>
<template>
  <a class="skip-link" href="#main">{{ t("İçeriğe geç") }}</a
  ><RouterView />
  <div v-if="notifications.message" class="toast" role="status">
    {{ t(notifications.message) }}
  </div>
</template>
