<script setup lang="ts">
import SelectButton from 'primevue/selectbutton'
import type { MapPreset } from '@/models/entityAnalytics'

const props = defineProps<{
  presets: MapPreset[]
  modelValue: string | null
}>()

const emit = defineEmits<{
  'update:modelValue': [value: string]
}>()

const options = () =>
  props.presets.map((p) => ({
    label: p.DisplayName,
    value: p.PresetId,
  }))
</script>

<template>
  <SelectButton
    :model-value="modelValue"
    :options="options()"
    option-label="label"
    option-value="value"
    :allow-empty="false"
    @update:model-value="emit('update:modelValue', $event)"
  />
</template>
