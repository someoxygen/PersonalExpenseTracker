<script setup lang="ts">
import { onMounted } from "vue";
import { authApi } from "./authApi";
import { useAuthStore } from "../../stores/auth";
import { useResource } from "../../composables/useResource";
import LoadingState from "../../components/LoadingState.vue";
import ErrorState from "../../components/ErrorState.vue";
const auth = useAuthStore();
const { loading, error, load } = useResource(async () => {
  const user = await authApi.me();
  auth.user = user;
  return user;
});
onMounted(load);
</script>
<template>
  <header class="page-heading">
    <div>
      <p class="eyebrow">HESABINIZ</p>
      <h1>Profil</h1>
    </div>
  </header>
  <LoadingState v-if="loading" /><ErrorState
    v-else-if="error"
    :message="error"
    retry
    @retry="load"
  />
  <section v-else-if="auth.user" class="panel">
    <dl class="details-list">
      <div>
        <dt>Ad soyad</dt>
        <dd>{{ auth.user.firstName }} {{ auth.user.lastName }}</dd>
      </div>
      <div>
        <dt>E-posta</dt>
        <dd>{{ auth.user.email }}</dd>
      </div>
      <div>
        <dt>Para birimi</dt>
        <dd>{{ auth.user.currency }}</dd>
      </div>
    </dl>
    <p class="notice">
      Güvenliğiniz için oturumunuz süre sonunda kapanır. Sayfayı yenilediğinizde
      yeniden giriş yapmanız gerekir.
    </p>
  </section>
</template>
