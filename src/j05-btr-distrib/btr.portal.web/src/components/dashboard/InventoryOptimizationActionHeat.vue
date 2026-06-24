<script setup lang="ts">
import { computed } from 'vue'
import Card from 'primevue/card'
import ProgressSpinner from 'primevue/progressspinner'
import type { DashboardInventoryOptimizationActionHeatItem } from '@/models/dashboard'

const props = defineProps<{
  heatSummary: DashboardInventoryOptimizationActionHeatItem[]
  loading: boolean
}>()

const actionTypes = computed(() =>
  [...new Set((props.heatSummary ?? []).map((item) => item.ActionLabel))],
)

const categories = ['Critical', 'High', 'Medium', 'Low']

const cellMap = computed(() => {
  const map = new Map<string, number>()
  for (const cell of props.heatSummary ?? []) {
    map.set(`${cell.ActionLabel}|${cell.Category}`, cell.ActionCount)
  }
  return map
})

function cellCount(actionLabel: string, category: string): number {
  return cellMap.value.get(`${actionLabel}|${category}`) ?? 0
}
</script>

<template>
  <Card class="inventory-optimization-action-heat">
    <template #title>Action Heat Summary</template>
    <template #content>
      <div v-if="loading" class="inventory-optimization-action-heat__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>
      <div v-else class="inventory-optimization-action-heat__grid-wrap">
        <table class="inventory-optimization-action-heat__grid">
          <thead>
            <tr>
              <th scope="col" />
              <th v-for="category in categories" :key="category" scope="col">{{ category }}</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="actionLabel in actionTypes" :key="actionLabel">
              <th scope="row">{{ actionLabel }}</th>
              <td v-for="category in categories" :key="`${actionLabel}-${category}`">
                {{ cellCount(actionLabel, category) }}
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </template>
  </Card>
</template>

<style scoped>
.inventory-optimization-action-heat__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.inventory-optimization-action-heat__grid {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.875rem;
}

.inventory-optimization-action-heat__grid th,
.inventory-optimization-action-heat__grid td {
  border: 1px solid var(--p-surface-200);
  padding: 0.5rem 0.75rem;
  text-align: center;
}

.inventory-optimization-action-heat__grid th[scope='row'] {
  text-align: left;
  font-weight: 600;
}
</style>
