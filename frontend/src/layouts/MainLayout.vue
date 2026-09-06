<script setup lang="ts">
import { t } from "../i18n";
import { onMounted, ref } from "vue";
import { RouterLink, RouterView, useRouter } from "vue-router";
import { useAuthStore } from "../stores/auth";
import LanguageSelector from "../components/LanguageSelector.vue";
import LoadingState from "../components/LoadingState.vue";
import ErrorState from "../components/ErrorState.vue";
import { categoriesApi } from "../features/categories/categoriesApi";
import { useResource } from "../composables/useResource";
const categories = useResource(categoriesApi.list);
onMounted(categories.load);
const auth = useAuthStore();
const router = useRouter();
const menuOpen = ref(false);
const links = [
  { to: "/dashboard", label: "Genel bakış", icon: "◫" },
  { to: "/transactions", label: "İşlemler", icon: "↕" },
  { to: "/accounts", label: "Hesaplar", icon: "▤" },
  { to: "/categories", label: "Kategoriler", icon: "◇" },
  { to: "/budgets", label: "Bütçeler", icon: "◉" },
  { to: "/profile", label: "Profil", icon: "○" },
];
function logout() {
  auth.clear();
  void router.replace("/login");
}
</script>
<template>
  <div class="app-shell">
    <header class="mobile-header">
      <RouterLink to="/dashboard" class="wordmark">Expense Tracker.</RouterLink
      ><button
        class="secondary"
        :aria-expanded="menuOpen"
        aria-controls="navigation"
        @click="menuOpen = !menuOpen"
      >
        {{ t("Menü") }}
      </button>
    </header>
    <aside class="sidebar" :class="{ open: menuOpen }">
      <RouterLink to="/dashboard" class="wordmark"
        >Expense Tracker<span>.</span></RouterLink
      >
      <p class="sidebar-caption">{{ t("KİŞİSEL FİNANS") }}</p>
      <nav id="navigation" :aria-label="t('Ana menü')">
        <RouterLink
          v-for="link in links"
          :key="link.to"
          :to="link.to"
          @click="menuOpen = false"
          ><span aria-hidden="true">{{ link.icon }}</span
          >{{ t(link.label) }}</RouterLink
        >
      </nav>
      <div class="sidebar-profile">
        <LanguageSelector class="desktop-language" />
        <strong>{{ auth.user?.firstName }} {{ auth.user?.lastName }}</strong
        ><small>{{ auth.user?.currency }} {{ t("· Kişisel hesap") }}</small
        ><button class="secondary" @click="logout">{{ t("Çıkış yap") }}</button>
      </div>
    </aside>
    <main id="main" class="workspace" tabindex="-1">
      <div class="mobile-language"><LanguageSelector /></div>
      <LoadingState v-if="categories.loading.value" />
      <ErrorState
        v-else-if="categories.error.value"
        :message="categories.error.value"
        retry
        @retry="categories.load"
      />
      <RouterView v-else-if="categories.data.value" />
    </main>
  </div>
</template>
