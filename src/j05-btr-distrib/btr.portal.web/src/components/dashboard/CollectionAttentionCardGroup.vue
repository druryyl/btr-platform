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
  <div class="collection-attention-card__wrapper">
    <KpiCard
      :title="title"
      :icon="icon"
      :loading="loading"
      class="collection-attention-card"
      :class="{
        'collection-attention-card--attention': requiresAttention,
        'collection-attention-card--unavailable': unavailable,
      }"
    >
      <div v-if="unavailable" class="collection-attention-card__unavailable">
        Data unavailable
      </div>
      <slot v-else />
    </KpiCard>
  </div>
</template>

<style scoped>
.collection-attention-card__wrapper {
  display: block;
  height: 100%;
}

.collection-attention-card {
  height: 100%;
}

.collection-attention-card--attention {
  border-left: 4px solid var(--p-orange-500);
}

.collection-attention-card--unavailable {
  opacity: 0.7;
}

.collection-attention-card__unavailable {
  color: var(--p-text-muted-color);
  font-size: 0.9rem;
}
</style>
