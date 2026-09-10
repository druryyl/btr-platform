<script setup lang="ts">
import { computed } from 'vue'
import Card from 'primevue/card'
import Chart from 'primevue/chart'
import { createChartOptions } from '@/services/chartLayout'
import type { ActiveElement, ChartEvent } from 'chart.js'
import ProgressSpinner from 'primevue/progressspinner'
import { formatCurrency } from '@/services/formatters'
import type { DashboardInventoryBreakdownItem } from '@/models/dashboard'

const props = defineProps<{
  title: string
  items: DashboardInventoryBreakdownItem[]
  loading: boolean
  clickable?: boolean
}>()

const emit = defineEmits<{
  'bar-click': [item: DashboardInventoryBreakdownItem]
}>()

const hasData = computed(() =>
  props.items.some((item) => item.InventoryValue > 0),
)

const chartHeight = computed(() =>
  Math.max(224, props.items.length * 26),
)

const chartData = computed(() => ({
  labels: props.items.map((item) => item.Name),
  datasets: [
    {
      data: props.items.map((item) => item.InventoryValue),
      backgroundColor: '#3b82f6',
    },
  ],
}))

const chartOptions = computed(() => {
  const options = createChartOptions({
    indexAxis: 'y' as const,
    plugins: {
      tooltip: {
        callbacks: {
          label: (context: { parsed: { x: number } }) =>
            ` ${formatCurrency(context.parsed.x)}`,
        },
      },
    },
    scales: {
      x: {
        ticks: {
          callback: (value: string | number) => formatCurrency(Number(value)),
        },
      },
    },
  })

  if (props.clickable) {
    options.onClick = (_event: ChartEvent, elements: ActiveElement[]) => {
      if (!elements.length) {
        return
      }
      const index = (elements[0] as { index?: number }).index
      if (typeof index !== 'number') {
        return
      }
      const item = props.items[index]
      if (item) {
        emit('bar-click', item)
      }
    }
  }

  return options
})
</script>

<template>
  <Card class="inventory-horizontal-bar-chart portal-chart-card">
    <template #title>
      <div class="inventory-horizontal-bar-chart__title">
        <i class="pi pi-chart-bar" aria-hidden="true" />
        <span>{{ title }}</span>
      </div>
    </template>

    <template #content>
      <div v-if="loading" class="inventory-horizontal-bar-chart__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>

      <template v-else>
        <div
          v-if="hasData"
          class="inventory-horizontal-bar-chart__canvas portal-chart-canvas portal-chart-canvas--fluid"
          :class="{ 'inventory-horizontal-bar-chart__canvas--clickable': clickable }"
          :style="{ height: `${chartHeight}px` }"
        >
          <Chart type="bar" :data="chartData" :options="chartOptions" />
        </div>
        <p v-else class="inventory-horizontal-bar-chart__empty">
          No inventory data available.
        </p>
        <p
          v-if="clickable && hasData"
          class="inventory-horizontal-bar-chart__hint"
        >
          Click a bar to open the Principal Performance dashboard.
        </p>
      </template>
    </template>
  </Card>
</template>

<style scoped>
.inventory-horizontal-bar-chart__title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.inventory-horizontal-bar-chart__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.inventory-horizontal-bar-chart__empty {
  margin: 0;
  padding: 2rem 0;
  text-align: center;
  color: var(--p-text-muted-color);
}

.inventory-horizontal-bar-chart__hint {
  margin: 0;
  padding-top: 0.5rem;
  text-align: center;
  font-size: var(--p-text-sm-font-size);
  color: var(--p-text-muted-color);
}

.inventory-horizontal-bar-chart__canvas--clickable {
  cursor: pointer;
}
</style>
