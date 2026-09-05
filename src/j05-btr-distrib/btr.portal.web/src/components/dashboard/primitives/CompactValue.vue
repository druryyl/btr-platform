<script setup lang="ts">
import { computed } from 'vue'
import { splitCompactSuffix } from '@/services/formatters'

const props = defineProps<{
  value: string
}>()

const parts = computed(() => splitCompactSuffix(props.value))
</script>

<template>
  <span class="compact-value">
    {{ parts.mantissa }}<sup v-if="parts.suffix" class="compact-value__suffix">{{
      parts.suffix
    }}</sup>
  </span>
</template>

<style scoped>
.compact-value__suffix {
  font-size: 0.62em;
  vertical-align: super;
  line-height: 0;
}
</style>