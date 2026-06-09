<script setup lang="ts">
import KpiCard from '@/components/KpiCard.vue'

defineProps<{
  title: string
  icon: string
  loading?: boolean
  requiresAttention?: boolean
  unavailable?: boolean
}>()
</script>

<template>
  <div class="purchasing-attention-card__wrapper">
    <KpiCard
      :title="title"
      :icon="icon"
      :loading="loading"
      class="purchasing-attention-card"
      :class="{
        'purchasing-attention-card--attention': requiresAttention,
        'purchasing-attention-card--unavailable': unavailable,
      }"
    >
      <div v-if="unavailable" class="purchasing-attention-card__unavailable">
        Data unavailable
      </div>
      <slot v-else />
    </KpiCard>
  </div>
</template>

<style scoped>
.purchasing-attention-card__wrapper {
  display: block;
  height: 100%;
}

.purchasing-attention-card {
  height: 100%;
}

.purchasing-attention-card--attention {
  border-left: 4px solid var(--p-orange-500);
}

.purchasing-attention-card--unavailable {
  opacity: 0.7;
}

.purchasing-attention-card__unavailable {
  color: var(--p-text-muted-color);
  font-size: 0.9rem;
}
</style>
