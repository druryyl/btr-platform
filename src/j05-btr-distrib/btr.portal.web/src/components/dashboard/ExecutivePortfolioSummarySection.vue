<script setup lang="ts">
import { computed } from 'vue'
import ExecutiveAttentionCard from '@/components/dashboard/ExecutiveAttentionCard.vue'
import { formatPercent } from '@/services/formatters'
import type { DashboardExecutivePortfolioAttention } from '@/models/dashboard'

const props = defineProps<{
  portfolio: DashboardExecutivePortfolioAttention | null | undefined
  loading?: boolean
}>()

const route = computed(
  () => props.portfolio?.DashboardRoute ?? '/dashboard/customer-portfolio',
)

const unavailable = computed(() => props.portfolio != null && !props.portfolio.IsAvailable)
</script>

<template>
  <section class="executive-portfolio-summary">
    <h2 class="executive-portfolio-summary__title">Customer Portfolio</h2>
    <div class="executive-portfolio-summary__grid">
      <ExecutiveAttentionCard
        title="Portfolio Healthy %"
        icon="pi pi-heart"
        :route="route"
        :loading="loading"
        :requires-attention="
          portfolio?.PortfolioHealthyPercent != null && portfolio.PortfolioHealthyPercent < 75
        "
        :unavailable="unavailable"
      >
        <div class="metric">
          <span class="metric__label">Healthy Customers</span>
          <span class="metric__value">
            {{
              portfolio?.IsAvailable
                ? formatPercent(portfolio.PortfolioHealthyPercent)
                : '—'
            }}
          </span>
        </div>
      </ExecutiveAttentionCard>

      <ExecutiveAttentionCard
        title="Customers At Risk"
        icon="pi pi-exclamation-circle"
        :route="route"
        :loading="loading"
        :requires-attention="(portfolio?.CustomersAtRiskCount ?? 0) > 0"
        :unavailable="unavailable"
      >
        <div class="metric">
          <span class="metric__label">At Risk Count</span>
          <span class="metric__value">
            {{ portfolio?.IsAvailable ? portfolio.CustomersAtRiskCount : '—' }}
          </span>
        </div>
      </ExecutiveAttentionCard>

      <ExecutiveAttentionCard
        title="Strategic At Risk"
        icon="pi pi-star"
        :route="route"
        :loading="loading"
        :requires-attention="(portfolio?.StrategicCustomersAtRiskCount ?? 0) > 0"
        :unavailable="unavailable"
      >
        <div class="metric">
          <span class="metric__label">Strategic Customers</span>
          <span class="metric__value">
            {{
              portfolio?.IsAvailable ? portfolio.StrategicCustomersAtRiskCount : '—'
            }}
          </span>
        </div>
      </ExecutiveAttentionCard>
    </div>
  </section>
</template>

<style scoped>
.executive-portfolio-summary {
  margin-bottom: 2rem;
}

.executive-portfolio-summary__title {
  margin: 0 0 1rem;
  font-size: 1.1rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.04em;
  color: var(--p-text-muted-color);
}

.executive-portfolio-summary__grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 1rem;
}

.metric {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.metric__label {
  font-size: 0.875rem;
  color: var(--p-text-muted-color);
}

.metric__value {
  font-size: 1.1rem;
  font-weight: 700;
  color: var(--p-text-color);
}

@media (max-width: 960px) {
  .executive-portfolio-summary__grid {
    grid-template-columns: 1fr;
  }
}
</style>
