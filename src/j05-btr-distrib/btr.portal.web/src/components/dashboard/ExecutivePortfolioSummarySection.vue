<script setup lang="ts">
import { computed } from 'vue'
import ExecutiveAttentionCard from '@/components/dashboard/ExecutiveAttentionCard.vue'
import DashboardSectionHeader from '@/components/dashboard/primitives/DashboardSectionHeader.vue'
import DashboardMetric from '@/components/dashboard/primitives/DashboardMetric.vue'
import {
  formatDashboardEmpty,
  formatDashboardPercent,
} from '@/services/dashboardEmptyStates'
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
    <DashboardSectionHeader
      title="Customer Portfolio"
      icon="pi pi-heart"
      domain="portfolio"
    />
    <div class="executive-portfolio-summary__grid">
      <ExecutiveAttentionCard
        title="Portfolio Healthy %"
        icon="pi pi-heart"
        domain="portfolio"
        :route="route"
        :loading="loading"
        :requires-attention="
          portfolio?.PortfolioHealthyPercent != null && portfolio.PortfolioHealthyPercent < 75
        "
        :unavailable="unavailable"
      >
        <DashboardMetric
          label="Healthy Customers"
          :value="
            portfolio?.IsAvailable
              ? formatDashboardPercent(portfolio.PortfolioHealthyPercent)
              : formatDashboardEmpty('not-available')
          "
          variant="primary"
          :empty="!portfolio?.IsAvailable || portfolio.PortfolioHealthyPercent == null"
          :progress="portfolio?.PortfolioHealthyPercent ?? null"
        />
      </ExecutiveAttentionCard>

      <ExecutiveAttentionCard
        title="Customers At Risk"
        icon="pi pi-exclamation-circle"
        domain="customer"
        :route="route"
        :loading="loading"
        :requires-attention="(portfolio?.CustomersAtRiskCount ?? 0) > 0"
        :unavailable="unavailable"
      >
        <DashboardMetric
          label="At Risk Count"
          :value="
            portfolio?.IsAvailable
              ? String(portfolio.CustomersAtRiskCount)
              : formatDashboardEmpty('not-available')
          "
          variant="primary"
          :empty="!portfolio?.IsAvailable"
        />
      </ExecutiveAttentionCard>

      <ExecutiveAttentionCard
        title="Strategic At Risk"
        icon="pi pi-star"
        domain="customer"
        :route="route"
        :loading="loading"
        :requires-attention="(portfolio?.StrategicCustomersAtRiskCount ?? 0) > 0"
        :unavailable="unavailable"
      >
        <DashboardMetric
          label="Strategic Customers"
          :value="
            portfolio?.IsAvailable
              ? String(portfolio.StrategicCustomersAtRiskCount)
              : formatDashboardEmpty('not-available')
          "
          variant="primary"
          :empty="!portfolio?.IsAvailable"
        />
      </ExecutiveAttentionCard>
    </div>
  </section>
</template>

<style scoped>
.executive-portfolio-summary {
  margin-bottom: 2rem;
}

.executive-portfolio-summary__grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 1rem;
}

@media (max-width: 960px) {
  .executive-portfolio-summary__grid {
    grid-template-columns: 1fr;
  }
}
</style>
