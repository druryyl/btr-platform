<script setup lang="ts">
import KpiCard from '@/components/KpiCard.vue'

defineProps<{
  title: string
  icon: string
  loading?: boolean
  unavailable?: boolean
}>()
</script>

<template>
  <div class="location-attention-card__wrapper">
    <KpiCard
      :title="title"
      :icon="icon"
      :loading="loading"
      class="location-attention-card"
      :class="{ 'location-attention-card--unavailable': unavailable }"
    >
      <div v-if="unavailable" class="location-attention-card__unavailable">
        Data unavailable
      </div>
      <slot v-else />
    </KpiCard>
  </div>
</template>

<style scoped>
.location-attention-card__wrapper {
  display: block;
  height: 100%;
}

.location-attention-card {
  height: 100%;
}

.location-attention-card--unavailable {
  opacity: 0.7;
}

.location-attention-card__unavailable {
  color: var(--p-text-muted-color);
  font-size: 0.9rem;
}
</style>
