<script setup lang="ts">
import Select from 'primevue/select'
import type { PeerGroupRule } from '@/models/entityAnalytics'

defineProps<{
  rules: PeerGroupRule[]
  modelValue: string | null
}>()

const emit = defineEmits<{
  'update:modelValue': [value: string]
}>()
</script>

<template>
  <div class="iw-peer-group-selector">
    <label class="iw-peer-group-selector__label" for="peer-group-rule">Peer group</label>
    <Select
      id="peer-group-rule"
      :model-value="modelValue"
      :options="rules"
      option-label="DisplayLabel"
      option-value="RuleId"
      placeholder="Peer group"
      @update:model-value="emit('update:modelValue', $event)"
    />
  </div>
</template>

<style scoped>
.iw-peer-group-selector {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  flex-wrap: wrap;
}

.iw-peer-group-selector__label {
  font-size: 0.8125rem;
  color: var(--iw-text-muted, #64748b);
  white-space: nowrap;
}
</style>
