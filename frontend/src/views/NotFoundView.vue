<script setup lang="ts">
import { t } from "../i18n";
import { RouterLink } from "vue-router";
</script>

<template>
  <main id="main" class="welcome" tabindex="-1">
    <p class="eyebrow">404</p>
    <h1>{{ t("Sayfa bulunamadı.") }}</h1>
    <p class="intro">{{ t("Bu adres mevcut değil.") }}</p>
    <RouterLink to="/">{{ t("Ana sayfaya dön") }}</RouterLink>
  </main>
</template>
