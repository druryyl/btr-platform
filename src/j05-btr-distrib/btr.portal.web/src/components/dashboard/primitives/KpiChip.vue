<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps<{
  label: string
  status: 'healthy' | 'warning' | 'critical' | 'unknown' | 'stable'
}>()

const chipClass = computed(() => `kpi-chip--${props.status.toLowerCase()}`)
</script>

<template>
  <span class="kpi-chip" :class="chipClass">{{ label }}</span>
</template>

<style scoped>
.kpi-chip {
  display: inline-flex;
  align-items: center;
  padding: 0.1875rem 0.625rem;
  border-radius: var(--dashboard-radius-chip);
  font-size: 0.6875rem;
  font-weight: 700;
  letter-spacing: 0.04em;
  text-transform: uppercase;
  line-height: 1.4;
}

.kpi-chip--healthy {
  background: var(--kpi-status-healthy-bg);
  color: var(--kpi-status-healthy-color);
}

.kpi-chip--warning {
  background: var(--kpi-status-warning-bg);
  color: var(--kpi-status-warning-color);
}

.kpi-chip--critical {
  background: var(--kpi-status-critical-bg);
  color: var(--kpi-status-critical-color);
}

.kpi-chip--unknown {
  background: var(--kpi-status-unknown-bg);
  color: var(--kpi-status-unknown-color);
}

.kpi-chip--stable {
  background: var(--kpi-status-stable-bg);
  color: var(--kpi-status-stable-color);
}
</style>
