<script setup lang="ts">
import { computed } from 'vue'
import Card from 'primevue/card'
import ProgressSpinner from 'primevue/progressspinner'
import type { DashboardInventoryForecastHeatCellItem } from '@/models/dashboard'

const props = defineProps<{
  heatSummary: DashboardInventoryForecastHeatCellItem[]
  loading: boolean
}>()

const dosBands = ['Low DOS (≤14d)', 'Med DOS (15–60d)', 'High DOS (>60d)']
const valueBands = ['Low Value', 'Med Value', 'High Value']

const cellMap = computed(() => {
  const map = new Map<string, number>()
  for (const cell of props.heatSummary ?? []) {
    map.set(`${cell.DosBand}|${cell.ValueBand}`, cell.ItemCount)
  }
  return map
})

function cellCount(dosBand: string, valueBand: string): number {
  return cellMap.value.get(`${dosBand}|${valueBand}`) ?? 0
}
</script>

<template>
  <Card class="inventory-risk-heat-summary">
    <template #title>
      <div class="inventory-risk-heat-summary__title">
        <i class="pi pi-th-large" aria-hidden="true" />
        <span>Risk Heat Summary</span>
      </div>
    </template>

    <template #content>
      <div v-if="loading" class="inventory-risk-heat-summary__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>

      <div v-else class="inventory-risk-heat-summary__grid-wrap">
        <table class="inventory-risk-heat-summary__grid">
          <thead>
            <tr>
              <th scope="col" />
              <th v-for="valueBand in valueBands" :key="valueBand" scope="col">
                {{ valueBand }}
              </th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="dosBand in dosBands" :key="dosBand">
              <th scope="row">{{ dosBand }}</th>
              <td
                v-for="valueBand in valueBands"
                :key="`${dosBand}-${valueBand}`"
                class="inventory-risk-heat-summary__cell"
              >
                {{ cellCount(dosBand, valueBand) }}
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </template>
  </Card>
</template>

<style scoped>
.inventory-risk-heat-summary__title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.inventory-risk-heat-summary__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.inventory-risk-heat-summary__grid {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.875rem;
}

.inventory-risk-heat-summary__grid th,
.inventory-risk-heat-summary__grid td {
  border: 1px solid var(--p-surface-200);
  padding: 0.5rem;
  text-align: center;
}

.inventory-risk-heat-summary__grid th[scope='row'] {
  text-align: left;
  font-weight: 600;
}

.inventory-risk-heat-summary__cell {
  font-weight: 700;
  background: var(--p-surface-50);
}
</style>
