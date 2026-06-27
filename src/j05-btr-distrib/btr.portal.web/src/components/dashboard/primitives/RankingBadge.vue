<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps<{
  rank: number
}>()

const badgeClass = computed(() => {
  if (props.rank === 1) return 'ranking-badge--gold'
  if (props.rank === 2) return 'ranking-badge--silver'
  if (props.rank === 3) return 'ranking-badge--bronze'
  return 'ranking-badge--default'
})

const showMedal = computed(() => props.rank >= 1 && props.rank <= 3)
</script>

<template>
  <span class="ranking-badge" :class="badgeClass">
    <template v-if="showMedal">{{ rank }}</template>
    <template v-else>{{ rank }}</template>
  </span>
</template>

<style scoped>
.ranking-badge {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 1.5rem;
  height: 1.5rem;
  padding: 0 0.375rem;
  border-radius: var(--dashboard-radius-sm);
  font-size: 0.8125rem;
  font-weight: 700;
}

.ranking-badge--gold {
  background: var(--rank-gold-bg);
  color: var(--rank-gold);
}

.ranking-badge--silver {
  background: var(--rank-silver-bg);
  color: var(--rank-silver);
}

.ranking-badge--bronze {
  background: var(--rank-bronze-bg);
  color: var(--rank-bronze);
}

.ranking-badge--default {
  background: transparent;
  color: var(--p-text-muted-color);
  font-weight: 600;
}
</style>
