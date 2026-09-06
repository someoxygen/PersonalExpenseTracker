<script setup lang="ts">
import { t } from "../../i18n";
import { computed, reactive, ref, watch } from "vue";
import { RouterLink, useRoute, useRouter } from "vue-router";
import { authApi } from "./authApi";
import { useAuthStore } from "../../stores/auth";
import { errorMessage } from "../../utils/errors";
import ErrorState from "../../components/ErrorState.vue";
import LanguageSelector from "../../components/LanguageSelector.vue";
const route = useRoute();
const router = useRouter();
const auth = useAuthStore();
const registering = computed(() => route.name === "register");
const form = reactive({ firstName: "", lastName: "", email: "", password: "" });
const busy = ref(false);
const error = ref("");
watch(registering, () => {
  error.value = "";
  form.password = "";
});
async function submit() {
  if (busy.value) return;
  busy.value = true;
  error.value = "";
  try {
    const result = registering.value
      ? await authApi.register(form)
      : await authApi.login({ email: form.email, password: form.password });
    auth.accept(result);
    auth.user = await authApi.me();
    form.password = "";
    const redirect = route.query.redirect;
    await router.replace(
      typeof redirect === "string" &&
        redirect.startsWith("/") &&
        !redirect.startsWith("//") &&
        !redirect.startsWith("/login") &&
        !redirect.startsWith("/register")
        ? redirect
        : "/dashboard",
    );
  } catch (e) {
    auth.clear();
    error.value = errorMessage(e);
  } finally {
    busy.value = false;
  }
}
</script>
<template>
  <div class="auth-shell">
    <aside class="auth-story">
      <RouterLink to="/" class="wordmark">Expense Tracker.</RouterLink>
      <p class="eyebrow">{{ t("PARANIZI TANIYIN") }}</p>
      <h1>{{ t("Bugünü görün.") }}<br />{{ t("Yarını planlayın.") }}</h1>
      <p>
        {{
          t(
            "Gelirinize, harcamalarınıza ve bütçenize tek bir yerden bakın. Küçük adımlarla daha bilinçli kararlar alın.",
          )
        }}
      </p>
      <div class="auth-art" aria-hidden="true">
        <span style="height: 35%"></span><span style="height: 50%"></span
        ><span style="height: 42%"></span><span style="height: 72%"></span
        ><span style="height: 90%"></span>
      </div>
    </aside>
    <main id="main" class="auth-content" tabindex="-1">
      <div class="auth-card">
        <LanguageSelector />
        <h1>
          {{
            registering ? t("Yeni bir başlangıç.") : t("Tekrar hoş geldiniz.")
          }}
        </h1>
        <p class="muted">
          {{
            registering
              ? t("Kişisel finans alanınızı oluşturun.")
              : t("Hesabınıza giriş yaparak devam edin.")
          }}
        </p>
        <ErrorState v-if="error" :message="error" />
        <form @submit.prevent="submit">
          <div v-if="registering" class="form-grid">
            <label class="field"
              >{{ t("Ad")
              }}<input
                v-model.trim="form.firstName"
                autocomplete="given-name"
                required
                maxlength="100"
                :disabled="busy"
            /></label>
            <label class="field"
              >{{ t("Soyad")
              }}<input
                v-model.trim="form.lastName"
                autocomplete="family-name"
                required
                maxlength="100"
                :disabled="busy"
            /></label>
          </div>
          <label class="field"
            >{{ t("E-posta")
            }}<input
              v-model.trim="form.email"
              type="email"
              autocomplete="email"
              required
              maxlength="254"
              :disabled="busy"
          /></label>
          <label class="field"
            >{{ t("Parola")
            }}<input
              v-model="form.password"
              type="password"
              :autocomplete="registering ? 'new-password' : 'current-password'"
              :minlength="registering ? 12 : 1"
              maxlength="128"
              required
              :disabled="busy"
          /></label>
          <small v-if="registering">{{
            t("En az 12 karakter kullanın.")
          }}</small>
          <button type="submit" :disabled="busy">
            {{
              busy
                ? t("İşleniyor…")
                : registering
                  ? t("Hesap oluştur")
                  : t("Giriş yap")
            }}
          </button>
        </form>
        <p class="auth-footer">
          {{
            registering
              ? t("Zaten hesabınız var mı?")
              : t("Henüz hesabınız yok mu?")
          }}
          <RouterLink :to="registering ? '/login' : '/register'">{{
            registering ? t("Giriş yapın") : t("Hesap oluşturun")
          }}</RouterLink>
        </p>
      </div>
    </main>
  </div>
</template>
