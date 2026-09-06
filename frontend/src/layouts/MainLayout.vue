<script setup lang="ts">
import { ref } from "vue";
import { RouterLink, RouterView, useRouter } from "vue-router";
import { useAuthStore } from "../stores/auth";
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
        Menü
      </button>
    </header>
    <aside class="sidebar" :class="{ open: menuOpen }">
      <RouterLink to="/dashboard" class="wordmark"
        >Expense Tracker<span>.</span></RouterLink
      >
      <p class="sidebar-caption">KİŞİSEL FİNANS</p>
      <nav id="navigation" aria-label="Ana menü">
        <RouterLink
          v-for="link in links"
          :key="link.to"
          :to="link.to"
          @click="menuOpen = false"
          ><span aria-hidden="true">{{ link.icon }}</span
          >{{ link.label }}</RouterLink
        >
      </nav>
      <div class="sidebar-profile">
        <strong>{{ auth.user?.firstName }} {{ auth.user?.lastName }}</strong
        ><small>{{ auth.user?.currency }} · Kişisel hesap</small
        ><button class="secondary" @click="logout">Çıkış yap</button>
      </div>
    </aside>
    <main id="main" class="workspace" tabindex="-1"><RouterView /></main>
  </div>
</template>
