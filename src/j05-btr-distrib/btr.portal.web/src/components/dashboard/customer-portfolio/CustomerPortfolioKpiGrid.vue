<script setup lang="ts">
import InventoryForecastKpiGrid from '@/components/dashboard/InventoryForecastKpiGrid.vue'
import type { SalesForecastKpiMetric } from '@/components/dashboard/SalesForecastKpiRow.vue'

defineProps<{
  metrics: SalesForecastKpiMetric[]
}>()
</script>

<template>
  <InventoryForecastKpiGrid :metrics="metrics" />
</template>
