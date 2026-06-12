<script setup lang="ts">
import { computed } from 'vue'
import KpiCard from '@/components/KpiCard.vue'
import { usePresentationStore } from '@/stores/presentationStore'

const props = defineProps<{
  title: string
  icon: string
  loading?: boolean
  unavailable?: boolean
}>()

const presentation = usePresentationStore()

const showUnavailableLabel = computed(
  () => Boolean(props.unavailable) && !presentation.hidePlatformDiagnostics,
)

const canNavigate = computed(
  () => !props.unavailable || presentation.hidePlatformDiagnostics,
)
</script>

<template>
  <div class="location-attention-card__wrapper">
    <KpiCard
      :title="title"
      :icon="icon"
      :loading="loading"
      class="location-attention-card"
      :class="{ 'location-attention-card--unavailable': showUnavailableLabel }"
    >
      <div v-if="showUnavailableLabel" class="location-attention-card__unavailable">
        Data unavailable
      </div>
      <slot v-else-if="canNavigate" />
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
