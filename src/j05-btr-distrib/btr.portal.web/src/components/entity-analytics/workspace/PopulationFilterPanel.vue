<script setup lang="ts">
import { computed } from 'vue'
import Checkbox from 'primevue/checkbox'
import Select from 'primevue/select'
import Button from 'primevue/button'
import type { PopulationMapPoint } from '@/models/entityAnalytics'

const props = defineProps<{
  points: PopulationMapPoint[]
  dimensionFilter: string | null
  attentionOnly: boolean
}>()

const emit = defineEmits<{
  'update:dimensionFilter': [value: string | null]
  'update:attentionOnly': [value: boolean]
  clear: []
}>()

const dimensionOptions = computed(() => {
  const values = new Set<string>()
  for (const point of props.points) {
    if (point.DimensionValue?.trim()) values.add(point.DimensionValue.trim())
  }
  return Array.from(values).sort().map((v) => ({ label: v, value: v }))
})

const selectedDimension = computed({
  get: () => props.dimensionFilter,
  set: (value: string | null) => emit('update:dimensionFilter', value),
})

const attention = computed({
  get: () => props.attentionOnly,
  set: (value: boolean) => emit('update:attentionOnly', value),
})
</script>

<template>
  <div class="iw-filter-panel">
    <Select
      v-if="dimensionOptions.length"
      v-model="selectedDimension"
      :options="dimensionOptions"
      option-label="label"
      option-value="value"
      placeholder="Peer group filter"
      show-clear
      class="iw-filter-panel__select"
    />
    <label class="iw-filter-panel__check">
      <Checkbox v-model="attention" binary />
      <span>Attention signals only</span>
    </label>
    <Button label="Clear filters" text size="small" @click="emit('clear')" />
  </div>
</template>

<style scoped>
.iw-filter-panel {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 0.75rem;
}

.iw-filter-panel__select {
  min-width: 12rem;
}

.iw-filter-panel__check {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.8125rem;
  color: var(--iw-text-secondary, #64748b);
}
</style>
