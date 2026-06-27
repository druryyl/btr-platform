<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps<{
  value: number | null | undefined
  status?: 'healthy' | 'warning' | 'critical' | 'unknown' | null
}>()

const clampedPercent = computed(() => {
  if (props.value == null) return 0
  return Math.min(100, Math.max(0, props.value))
})

const resolvedStatus = computed(() => {
  if (props.status) return props.status
  if (props.value == null) return 'unknown'
  if (props.value >= 100) return 'healthy'
  if (props.value >= 80) return 'warning'
  return 'critical'
})

const showBar = computed(() => props.value != null)
</script>

<template>
  <div v-if="showBar" class="kpi-progress" role="presentation">
    <div
      class="kpi-progress__fill"
      :class="`kpi-progress__fill--${resolvedStatus}`"
      :style="{ width: `${clampedPercent}%` }"
    />
  </div>
</template>

<style scoped>
.kpi-progress {
  height: 3px;
  border-radius: var(--dashboard-radius-chip);
  background: rgb(15 23 42 / 8%);
  overflow: hidden;
  margin-top: 0.125rem;
}

.kpi-progress__fill {
  height: 100%;
  border-radius: inherit;
  transition: width var(--dashboard-transition);
}

.kpi-progress__fill--healthy {
  background: var(--kpi-status-healthy-color);
}

.kpi-progress__fill--warning {
  background: var(--kpi-status-warning-color);
}

.kpi-progress__fill--critical {
  background: var(--kpi-status-critical-color);
}

.kpi-progress__fill--unknown {
  background: var(--kpi-status-unknown-color);
}
</style>
