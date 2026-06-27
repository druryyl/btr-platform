<script setup lang="ts">
import { computed } from 'vue'
import Card from 'primevue/card'
import Chart from 'primevue/chart'
import ProgressSpinner from 'primevue/progressspinner'
import { createChartOptions } from '@/services/chartLayout'
import { formatCurrency, formatNumber, formatPercent } from '@/services/formatters'

export interface FieldActivityComparisonItem {
  label: string
  value: number
  salesPersonId: string
}

const props = defineProps<{
  title: string
  items: FieldActivityComparisonItem[]
  loading?: boolean
  valueKind?: 'percent' | 'number' | 'currency'
}>()

const emit = defineEmits<{
  barClick: [salesPersonId: string]
}>()

const chartHeight = computed(() => Math.max(224, props.items.length * 26))

const hasData = computed(() => props.items.some((item) => item.value > 0))

const chartData = computed(() => ({
  labels: props.items.map((item) => item.label),
  datasets: [
    {
      data: props.items.map((item) => item.value),
      backgroundColor: '#2563eb',
    },
  ],
}))

function formatValue(value: number): string {
  if (props.valueKind === 'percent') return formatPercent(value)
  if (props.valueKind === 'currency') return formatCurrency(value)
  return formatNumber(value)
}

const chartOptions = computed(() =>
  createChartOptions({
    indexAxis: 'y' as const,
    onClick: (_event, elements, chart) => {
      if (!elements.length) return
      const index = elements[0].index
      const item = props.items[index]
      if (item?.salesPersonId) emit('barClick', item.salesPersonId)
    },
    plugins: {
      tooltip: {
        callbacks: {
          label: (context: { parsed: { x: number } }) => ` ${formatValue(context.parsed.x)}`,
        },
      },
    },
    scales: {
      x: {
        ticks: {
          callback: (value: string | number) => formatValue(Number(value)),
        },
      },
    },
  }),
)
</script>

<template>
  <Card class="field-activity-comparison-chart portal-chart-card">
    <template #title>
      <div class="field-activity-comparison-chart__title">
        <i class="pi pi-chart-bar" aria-hidden="true" />
        <span>{{ title }}</span>
      </div>
    </template>

    <template #content>
      <div v-if="loading" class="field-activity-comparison-chart__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>
      <template v-else>
        <div
          v-if="hasData"
          class="field-activity-comparison-chart__scroll"
        >
          <div
            class="field-activity-comparison-chart__canvas portal-chart-canvas portal-chart-canvas--fluid"
            :style="{ height: `${chartHeight}px` }"
          >
            <Chart type="bar" :data="chartData" :options="chartOptions" />
          </div>
        </div>
        <p v-else class="field-activity-comparison-chart__empty">No data for this chart.</p>
      </template>
    </template>
  </Card>
</template>

<style scoped>
.field-activity-comparison-chart__title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.field-activity-comparison-chart__scroll {
  max-height: 480px;
  overflow-y: auto;
}

.field-activity-comparison-chart__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.field-activity-comparison-chart__empty {
  margin: 0;
  color: var(--p-text-muted-color, #64748b);
}
</style>
