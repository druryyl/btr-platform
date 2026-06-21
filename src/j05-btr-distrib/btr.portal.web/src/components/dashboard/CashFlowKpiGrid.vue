<script setup lang="ts">
import SalesForecastKpiRow, {
  type SalesForecastKpiMetric,
} from '@/components/dashboard/SalesForecastKpiRow.vue'

defineProps<{
  metrics: SalesForecastKpiMetric[]
}>()
</script>

<template>
  <SalesForecastKpiRow :metrics="metrics" />
</template>
