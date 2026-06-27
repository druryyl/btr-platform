<script setup lang="ts">
import { computed } from 'vue'
import Card from 'primevue/card'
import Chart from 'primevue/chart'
import ProgressSpinner from 'primevue/progressspinner'
import { createChartOptions } from '@/services/chartLayout'
import type { FieldActivityWilayahBreakdownRow } from '@/models/fieldActivity'
import { formatNumber } from '@/services/formatters'

const props = defineProps<{
  items: FieldActivityWilayahBreakdownRow[]
  loading?: boolean
}>()

const chartHeight = computed(() => Math.max(224, props.items.length * 26))
const hasData = computed(() => props.items.some((item) => item.ActualVisits > 0))

const chartData = computed(() => ({
  labels: props.items.map((item) => item.WilayahName),
  datasets: [
    {
      data: props.items.map((item) => item.ActualVisits),
      backgroundColor: '#0d9488',
    },
  ],
}))

const chartOptions = computed(() =>
  createChartOptions({
    indexAxis: 'y' as const,
    plugins: {
      tooltip: {
        callbacks: {
          label: (context: { parsed: { x: number } }) => ` ${formatNumber(context.parsed.x)} visits`,
        },
      },
    },
  }),
)
</script>

<template>
  <Card class="field-activity-wilayah-chart portal-chart-card">
    <template #title>
      <div class="field-activity-wilayah-chart__title">
        <i class="pi pi-map" aria-hidden="true" />
        <span>Visits by Wilayah</span>
      </div>
    </template>

    <template #content>
      <div v-if="loading" class="field-activity-wilayah-chart__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>
      <template v-else>
        <div v-if="hasData" class="field-activity-wilayah-chart__scroll">
          <div
            class="field-activity-wilayah-chart__canvas portal-chart-canvas portal-chart-canvas--fluid"
            :style="{ height: `${chartHeight}px` }"
          >
            <Chart type="bar" :data="chartData" :options="chartOptions" />
          </div>
        </div>
        <p v-else class="field-activity-wilayah-chart__empty">No wilayah visit data.</p>
      </template>
    </template>
  </Card>
</template>

<style scoped>
.field-activity-wilayah-chart__title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.field-activity-wilayah-chart__scroll {
  max-height: 480px;
  overflow-y: auto;
}

.field-activity-wilayah-chart__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.field-activity-wilayah-chart__empty {
  margin: 0;
  color: var(--p-text-muted-color, #64748b);
}
</style>
