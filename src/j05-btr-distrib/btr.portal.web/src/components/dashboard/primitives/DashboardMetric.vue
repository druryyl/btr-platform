<script setup lang="ts">
import MetricLabel from '@/components/dashboard/primitives/MetricLabel.vue'
import KpiValue from '@/components/dashboard/primitives/KpiValue.vue'
import KpiProgress from '@/components/dashboard/primitives/KpiProgress.vue'

withDefaults(
  defineProps<{
    label: string
    value: string
    variant?: 'primary' | 'secondary'
    empty?: boolean
    progress?: number | null
    progressStatus?: 'healthy' | 'warning' | 'critical' | 'unknown' | null
  }>(),
  {
    variant: 'secondary',
    empty: false,
    progress: undefined,
    progressStatus: null,
  },
)
</script>

<template>
  <div class="dashboard-metric">
    <MetricLabel :label="label" />
    <KpiValue :value="value" :variant="variant" :empty="empty" />
    <KpiProgress
      v-if="progress !== undefined"
      :value="progress"
      :status="progressStatus"
    />
  </div>
</template>

<style scoped>
.dashboard-metric {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}
</style>
