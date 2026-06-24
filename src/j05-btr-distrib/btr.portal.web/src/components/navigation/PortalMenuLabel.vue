<script setup lang="ts">
import { computed } from 'vue'
import { findMenuItemByRoute, formatMenuLabel } from '@/navigation/portalMenuHelpers'

const props = defineProps<{
  route?: string | null
  code?: string
  label?: string
}>()

const displayCode = computed(() => {
  if (props.code) return props.code
  return findMenuItemByRoute(props.route)?.code ?? null
})

const displayLabel = computed(() => {
  if (props.label) return props.label
  return findMenuItemByRoute(props.route)?.label ?? props.route ?? ''
})

const formatted = computed(() => {
  if (displayCode.value && displayLabel.value) {
    return formatMenuLabel(displayCode.value, displayLabel.value)
  }
  return displayLabel.value
})
</script>

<template>
  <span class="portal-menu-label">
    <template v-if="displayCode">
      <span class="portal-menu-label__code">{{ displayCode }}</span>
      <span class="portal-menu-label__separator" aria-hidden="true">·</span>
      <span class="portal-menu-label__text">{{ displayLabel }}</span>
    </template>
    <template v-else>
      <span class="portal-menu-label__text">{{ formatted }}</span>
    </template>
  </span>
</template>

<style scoped>
.portal-menu-label {
  display: inline-flex;
  align-items: baseline;
  gap: 0.375rem;
  min-width: 0;
}

.portal-menu-label__code {
  flex-shrink: 0;
  font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace;
  font-size: 0.75rem;
  font-weight: 700;
  letter-spacing: 0.02em;
  color: var(--p-text-muted-color);
}

.portal-menu-label__separator {
  flex-shrink: 0;
  color: var(--p-text-muted-color);
}

.portal-menu-label__text {
  min-width: 0;
}
</style>
