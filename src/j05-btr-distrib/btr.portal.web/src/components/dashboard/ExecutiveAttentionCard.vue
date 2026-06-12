<script setup lang="ts">
import { computed } from 'vue'
import { RouterLink } from 'vue-router'
import KpiCard from '@/components/KpiCard.vue'
import type { AchievementBand } from '@/models/dashboard'
import { usePresentationStore } from '@/stores/presentationStore'

const props = defineProps<{
  title: string
  icon: string
  route: string
  loading?: boolean
  requiresAttention?: boolean
  achievementBand?: AchievementBand | null
  unavailable?: boolean
}>()

const presentation = usePresentationStore()

const showUnavailableLabel = computed(
  () => Boolean(props.unavailable) && !presentation.hidePlatformDiagnostics,
)

const showContent = computed(
  () => !props.unavailable || presentation.hidePlatformDiagnostics,
)

const bandClass = computed(() => {
  if (!props.achievementBand) return null

  switch (props.achievementBand) {
    case 'Healthy':
      return 'executive-attention-card__band--healthy'
    case 'Warning':
      return 'executive-attention-card__band--warning'
    case 'Critical':
      return 'executive-attention-card__band--critical'
    default:
      return 'executive-attention-card__band--unknown'
  }
})
</script>

<template>
  <RouterLink :to="route" class="executive-attention-card__link">
    <KpiCard
      :title="title"
      :icon="icon"
      :loading="loading"
      class="executive-attention-card"
      :class="{
        'executive-attention-card--attention': requiresAttention && !achievementBand,
        'executive-attention-card--unavailable': showUnavailableLabel,
      }"
    >
      <div v-if="showUnavailableLabel" class="executive-attention-card__unavailable">
        Data unavailable
      </div>
      <template v-else-if="showContent">
        <div v-if="achievementBand" class="executive-attention-card__band-row">
          <span class="executive-attention-card__band" :class="bandClass">
            {{ achievementBand }}
          </span>
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
  transition: box-shadow 0.15s ease;
}

.executive-attention-card:hover {
  box-shadow: 0 4px 12px rgb(0 0 0 / 8%);
}

.executive-attention-card--attention {
  border-left: 4px solid var(--p-orange-500);
}

.executive-attention-card--unavailable {
  opacity: 0.7;
}

.executive-attention-card__band-row {
  margin-bottom: 0.25rem;
}

.executive-attention-card__band {
  display: inline-block;
  padding: 0.125rem 0.5rem;
  border-radius: 999px;
  font-size: 0.75rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.03em;
}

.executive-attention-card__band--healthy {
  background: var(--p-green-100);
  color: var(--p-green-700);
}

.executive-attention-card__band--warning {
  background: var(--p-amber-100);
  color: var(--p-amber-800);
}

.executive-attention-card__band--critical {
  background: var(--p-red-100);
  color: var(--p-red-700);
}

.executive-attention-card__band--unknown {
  background: var(--p-surface-200);
  color: var(--p-text-muted-color);
}

.executive-attention-card__unavailable {
  color: var(--p-text-muted-color);
  font-size: 0.9rem;
}
</style>
