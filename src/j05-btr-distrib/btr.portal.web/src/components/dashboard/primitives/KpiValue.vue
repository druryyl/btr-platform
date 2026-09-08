<script setup lang="ts">
import { computed } from 'vue'
import CompactValue from '@/components/dashboard/primitives/CompactValue.vue'

const props = withDefaults(
  defineProps<{
    value: string
    variant?: 'primary' | 'secondary'
    empty?: boolean
    title?: string
  }>(),
  {
    variant: 'secondary',
    empty: false,
    title: undefined,
  },
)

const valueClass = computed(() => ({
  'kpi-value--primary': props.variant === 'primary',
  'kpi-value--secondary': props.variant === 'secondary',
  'kpi-value--empty': props.empty,
}))
</script>

<template>
  <span class="kpi-value" :class="valueClass" :title="title ?? undefined">
    <CompactValue :value="value" />
  </span>
</template>

<style scoped>
.kpi-value {
  color: var(--p-text-color);
  line-height: 1.2;
  font-variant-numeric: tabular-nums;
}

.kpi-value--primary {
  font-size: 1.625rem;
  font-weight: 800;
  letter-spacing: -0.02em;
}

.kpi-value--secondary {
  font-size: 1.0625rem;
  font-weight: 700;
}

.kpi-value--empty {
  font-size: 0.9375rem;
  font-weight: 600;
  color: var(--p-text-muted-color);
}
</style>
