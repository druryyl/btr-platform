<script setup lang="ts">
import { ref } from 'vue'
import AutoComplete from 'primevue/autocomplete'
import type { EntitySearchResult } from '@/models/entityAnalytics'
import { useEntityAnalyticsStore } from '@/stores/entityAnalyticsStore'

const props = defineProps<{
  entityType: string
  modelValue: EntitySearchResult | null
  placeholder?: string
  disabled?: boolean
}>()

const emit = defineEmits<{
  'update:modelValue': [value: EntitySearchResult | null]
}>()

const store = useEntityAnalyticsStore()
const suggestions = ref<EntitySearchResult[]>([])

async function onComplete(event: { query: string }) {
  suggestions.value = await store.searchEntityPicker(props.entityType, event.query)
}

function onSelect(item: EntitySearchResult) {
  emit('update:modelValue', item)
}

function onClear() {
  emit('update:modelValue', null)
}
</script>

<template>
  <AutoComplete
    :model-value="modelValue"
    :suggestions="suggestions"
    option-label="DisplayName"
    :placeholder="placeholder ?? 'Search entity code or name'"
    :disabled="disabled"
    force-selection
    dropdown
    class="entity-picker"
    @complete="onComplete"
    @item-select="onSelect($event.value)"
    @clear="onClear"
  >
    <template #option="{ option }">
      <div class="entity-picker__option">
        <strong>{{ option.EntityCode }}</strong>
        <span>{{ option.DisplayName }}</span>
      </div>
    </template>
  </AutoComplete>
</template>

<style scoped>
.entity-picker {
  width: 100%;
}

.entity-picker__option {
  display: flex;
  flex-direction: column;
  gap: 0.1rem;
}

.entity-picker__option span {
  color: var(--p-text-muted-color, #64748b);
  font-size: 0.85rem;
}
</style>
