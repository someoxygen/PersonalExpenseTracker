<script setup lang="ts">
import { RouterLink } from "vue-router";
</script>

<template>
  <main id="main" class="welcome" tabindex="-1">
    <p class="eyebrow">404</p>
    <h1>Sayfa bulunamadı.</h1>
    <p class="intro">Bu adres mevcut değil.</p>
    <RouterLink to="/">Ana sayfaya dön</RouterLink>
  </main>
</template>
