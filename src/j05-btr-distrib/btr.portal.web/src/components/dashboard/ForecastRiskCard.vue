<script setup lang="ts">
import { computed } from 'vue'
import { RouterLink } from 'vue-router'
import Card from 'primevue/card'
import Tag from 'primevue/tag'
import type { AchievementBand } from '@/models/dashboard'
import { formatCurrency, formatPercent } from '@/services/formatters'

const props = defineProps<{
  riskBand: AchievementBand | string | null
  forecastAchievementPercent: number | null
  requiredDailySales: number | null
  targetGap: number
  loading: boolean
}>()

const bandSeverity = computed(() => {
  switch (props.riskBand) {
    case 'Healthy':
      return 'success'
    case 'Warning':
      return 'warn'
    case 'Critical':
      return 'danger'
    default:
      return 'secondary'
  }
})

const actionHint = computed(() => {
  if (props.riskBand === 'Healthy') {
    return 'Projected month-end performance is on track against target.'
  }

  if (props.riskBand === 'Warning') {
    return `Increase daily billing pace. Required daily sales: ${formatCurrency(props.requiredDailySales ?? 0)}.`
  }

  if (props.riskBand === 'Critical') {
    return `Immediate corrective action recommended. Required daily sales: ${formatCurrency(props.requiredDailySales ?? 0)}.`
  }

  return 'Configure a monthly target to enable forecast risk comparison.'
})

const gapLabel = computed(() => {
  if (props.targetGap > 0) {
    return `Projected shortfall: ${formatCurrency(props.targetGap)}`
  }

  if (props.targetGap < 0) {
    return `Projected surplus: ${formatCurrency(Math.abs(props.targetGap))}`
  }

  return 'Projected to meet target exactly.'
})
</script>

<template>
  <Card class="forecast-risk-card">
    <template #title>
      <div class="forecast-risk-card__title">
        <i class="pi pi-exclamation-circle" aria-hidden="true" />
        <span>Forecast Risk</span>
      </div>
    </template>

    <template #content>
      <div v-if="loading" class="forecast-risk-card__loading">Loading risk indicator…</div>

      <template v-else>
        <div class="forecast-risk-card__header">
          <Tag
            v-if="riskBand"
            :value="riskBand"
            :severity="bandSeverity"
          />
          <span class="forecast-risk-card__achievement">
            Forecast achievement: {{ formatPercent(forecastAchievementPercent) }}
          </span>
        </div>

        <p class="forecast-risk-card__gap">{{ gapLabel }}</p>
        <p class="forecast-risk-card__hint">{{ actionHint }}</p>

        <RouterLink to="/dashboard/sales" class="forecast-risk-card__link">
          View Sales Dashboard →
        </RouterLink>
      </template>
    </template>
  </Card>
</template>

<style scoped>
.forecast-risk-card__title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.forecast-risk-card__loading {
  color: var(--p-text-muted-color);
}

.forecast-risk-card__header {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 0.75rem;
  margin-bottom: 0.75rem;
}

.forecast-risk-card__achievement {
  font-size: 0.9375rem;
  color: var(--p-text-color);
}

.forecast-risk-card__gap {
  margin: 0 0 0.5rem;
  font-weight: 600;
  color: var(--p-text-color);
}

.forecast-risk-card__hint {
  margin: 0 0 1rem;
  color: var(--p-text-muted-color);
  line-height: 1.5;
}

.forecast-risk-card__link {
  color: var(--p-primary-color);
  text-decoration: none;
  font-weight: 600;
}

.forecast-risk-card__link:hover {
  text-decoration: underline;
}
</style>
