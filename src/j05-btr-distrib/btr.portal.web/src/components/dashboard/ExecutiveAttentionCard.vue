<script setup lang="ts">
import { computed } from 'vue'
import { RouterLink } from 'vue-router'
import KpiCard from '@/components/KpiCard.vue'
import KpiChip from '@/components/dashboard/primitives/KpiChip.vue'
import type { AchievementBand } from '@/models/dashboard'
import type { DashboardDomain } from '@/services/dashboardDomains'
import { usePresentationStore } from '@/stores/presentationStore'

const props = defineProps<{
  title: string
  icon: string
  route: string
  domain: DashboardDomain
  loading?: boolean
  requiresAttention?: boolean
  achievementBand?: AchievementBand | null
  unavailable?: boolean
  hero?: boolean
}>()

const presentation = usePresentationStore()

const showUnavailableLabel = computed(
  () => Boolean(props.unavailable) && !presentation.hidePlatformDiagnostics,
)

const showContent = computed(
  () => !props.unavailable || presentation.hidePlatformDiagnostics,
)

const chipStatus = computed(() => {
  if (!props.achievementBand) return null

  switch (props.achievementBand) {
    case 'Healthy':
      return 'healthy' as const
    case 'Warning':
      return 'warning' as const
    case 'Critical':
      return 'critical' as const
    default:
      return 'unknown' as const
  }
})
</script>

<template>
  <RouterLink :to="route" class="executive-attention-card__link">
    <KpiCard
      :title="title"
      :icon="icon"
      :domain="domain"
      :hero="hero"
      :loading="loading"
      class="executive-attention-card"
      :class="{
        'executive-attention-card--attention': requiresAttention && !achievementBand,
        'executive-attention-card--unavailable': showUnavailableLabel,
      }"
    >
      <div v-if="showUnavailableLabel" class="executive-attention-card__unavailable">
        Not Available
      </div>
      <template v-else-if="showContent">
        <div v-if="achievementBand && chipStatus" class="executive-attention-card__band-row">
          <KpiChip :label="achievementBand" :status="chipStatus" />
        </div>
        <slot />
      </template>
    </KpiCard>
  </RouterLink>
</template>

<style scoped>
.executive-attention-card__link {
  text-decoration: none;
  color: inherit;
  display: block;
  height: 100%;
}

.executive-attention-card {
  height: 100%;
}

.executive-attention-card--attention :deep(.p-card) {
  border-left: 4px solid var(--domain-collection-color);
}

.executive-attention-card--unavailable {
  opacity: 0.75;
}

.executive-attention-card__band-row {
  margin-bottom: 0.125rem;
}

.executive-attention-card__unavailable {
  color: var(--p-text-muted-color);
  font-size: 0.9375rem;
  font-weight: 600;
}
</style>
