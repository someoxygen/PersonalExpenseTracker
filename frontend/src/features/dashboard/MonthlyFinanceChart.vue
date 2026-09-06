<script setup lang="ts">
import { computed } from "vue";
import type { MonthlyFinance } from "../../types/models";
import { money, monthName } from "../../utils/format";
import EmptyState from "../../components/EmptyState.vue";
const props = defineProps<{ items: MonthlyFinance[]; currency: string }>();
const maximum = computed(() =>
  Math.max(1, ...props.items.flatMap((item) => [item.income, item.expense])),
);
const hasData = computed(() =>
  props.items.some((item) => item.income > 0 || item.expense > 0),
);
</script>
<template>
  <section class="panel">
    <div class="panel-header">
      <h2>Gelir ve gider</h2>
      <span class="muted">Son 6 ay</span>
    </div>
    <div class="legend">
      <span><i style="background: #759566"></i>Gelir</span
      ><span><i style="background: #dbab83"></i>Gider</span>
    </div>
    <EmptyState
      v-if="!hasData"
      title="Henüz işlem yok"
      description="Gelir ve gider eklediğinizde aylık görünümünüz burada oluşacak."
    />
    <template v-else>
      <svg
        class="chart"
        viewBox="0 0 600 260"
        role="img"
        aria-label="Son altı ayın gelir ve gider karşılaştırması"
      >
        <line
          v-for="y in [35, 80, 125, 170, 215]"
          :key="y"
          x1="35"
          x2="590"
          :y1="y"
          :y2="y"
          stroke="#e9eee4"
        />
        <g
          v-for="(item, index) in items"
          :key="item.year + '-' + item.month"
          :transform="`translate(${45 + index * (540 / items.length)},0)`"
        >
          <rect
            x="0"
            :y="215 - (item.income / maximum) * 180"
            width="25"
            :height="(item.income / maximum) * 180"
            rx="4"
            fill="#759566"
          >
            <title>{{ money(item.income, currency) }} gelir</title>
          </rect>
          <rect
            x="31"
            :y="215 - (item.expense / maximum) * 180"
            width="25"
            :height="(item.expense / maximum) * 180"
            rx="4"
            fill="#dbab83"
          >
            <title>{{ money(item.expense, currency) }} gider</title>
          </rect>
          <text
            x="27"
            y="245"
            text-anchor="middle"
            fill="#70836d"
            font-size="12"
          >
            {{ monthName(item.month) }}
          </text>
        </g>
      </svg>
      <table class="sr-only">
        <caption>
          Aylık gelir ve gider değerleri
        </caption>
        <thead>
          <tr>
            <th>Ay</th>
            <th>Gelir</th>
            <th>Gider</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="item in items" :key="item.month">
            <td>{{ item.year }} {{ monthName(item.month) }}</td>
            <td>{{ money(item.income, currency) }}</td>
            <td>{{ money(item.expense, currency) }}</td>
          </tr>
        </tbody>
      </table>
    </template>
  </section>
</template>
