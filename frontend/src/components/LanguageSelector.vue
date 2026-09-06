<script setup lang="ts">
import { locale, setLocale, t } from "../i18n";
</script>

<template>
  <label class="language-selector">
    <span>{{ t("Dil") }}</span>
    <select
      :value="locale"
      @change="setLocale(($event.target as HTMLSelectElement).value)"
    >
      <option value="tr" lang="tr">Türkçe</option>
      <option value="en" lang="en">English</option>
    </select>
  </label>
</template>
