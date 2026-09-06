<script setup lang="ts">
import { computed } from "vue";
import type { CategoryExpense } from "../../types/models";
import { money } from "../../utils/format";
import EmptyState from "../../components/EmptyState.vue";
const props = defineProps<{ items: CategoryExpense[]; currency: string }>();
const segments = computed(() => {
  let offset = 0;
  return props.items.map((item) => {
    const segment = { ...item, offset };
    offset += item.percentage;
    return segment;
  });
});
</script>
<template>
  <section class="panel">
    <div class="panel-header">
      <h2>Harcama dağılımı</h2>
      <span class="muted">Bu ay</span>
    </div>
    <EmptyState
      v-if="!items.length"
      title="Henüz gider yok"
      description="Kategorilere göre harcamalarınız burada görünecek."
    />
    <template v-else>
      <svg
        viewBox="0 0 220 190"
        style="max-height: 190px; width: 100%"
        role="img"
        aria-label="Gider kategorilerinin yüzdeleri"
      >
        <circle
          cx="110"
          cy="92"
          r="64"
          fill="none"
          stroke="#edf2e8"
          stroke-width="24"
        />
        <circle
          v-for="item in segments"
          :key="item.categoryId"
          cx="110"
          cy="92"
          r="64"
          fill="none"
          :stroke="item.color"
          stroke-width="24"
          pathLength="100"
          :stroke-dasharray="`${item.percentage} ${100 - item.percentage}`"
          :stroke-dashoffset="-item.offset"
          transform="rotate(-90 110 92)"
        >
          <title>{{ item.categoryName }}: %{{ item.percentage }}</title>
        </circle>
        <text x="110" y="98" text-anchor="middle" fill="#315942" font-size="15">
          Bu ay
        </text>
      </svg>
      <div
        v-for="item in items"
        :key="item.categoryId"
        class="split"
        style="margin-top: 0.65rem; font-size: 0.86rem"
      >
        <span
          ><i class="color-dot" :style="{ background: item.color }"></i
          >{{ item.categoryName }}</span
        ><span
          >{{ money(item.amount, currency) }} · %{{ item.percentage }}</span
        >
      </div>
    </template>
  </section>
</template>
