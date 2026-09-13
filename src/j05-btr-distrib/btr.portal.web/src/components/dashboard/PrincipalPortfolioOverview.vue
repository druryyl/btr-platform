<script setup lang="ts">
import { computed } from 'vue'
import type { PortfolioOverview } from '@/services/principalPortfolio'
import { formatWithNoData } from '@/services/principalNoData'
import { formatCurrency, formatCurrencyCompact, formatNumber, formatPercent } from '@/services/formatters'

const props = defineProps<{
  overview: PortfolioOverview | null
}>()

function currencyDisplay(value: number | null | undefined): string {
  return formatWithNoData(value, (v) => formatCurrencyCompact(v))
}

function currencyTitle(value: number | null | undefined): string | undefined {
  if (value == null) return undefined
  return formatCurrency(value)
}

function percentDisplay(ratio: number | null | undefined): string {
  return formatWithNoData(ratio, (v) => formatPercent(v * 100))
}

const totalSalesDisplay = computed(() =>
  props.overview ? currencyDisplay(props.overview.totalSales) : 'No Data',
)
const totalSalesTitle = computed(() =>
  props.overview ? currencyTitle(props.overview.totalSales) : undefined,
)
const totalTargetDisplay = computed(() =>
  props.overview ? currencyDisplay(props.overview.totalTarget) : 'No Data',
)
const totalTargetTitle = computed(() =>
  props.overview ? currencyTitle(props.overview.totalTarget) : undefined,
)
const achievementDisplay = computed(() =>
  props.overview ? percentDisplay(props.overview.achievementPercentage) : 'No Data',
)
const coverageDisplay = computed(() =>
  props.overview ? percentDisplay(props.overview.coveragePercentage) : 'No Data',
)
const returnDisplay = computed(() =>
  props.overview ? percentDisplay(props.overview.portfolioReturnPercentage) : 'No Data',
)
const coveragePopulationDisplay = computed(() => {
  if (!props.overview) return 'No Data'
  const count = props.overview.principalsWithCoverageData
  return `${formatNumber(count)} principal${count === 1 ? '' : 's'} with coverage data`
})
</script>

<template>
  <section
    class="principal-portfolio-overview"
    aria-label="Portfolio overview"
    data-testid="portfolio-overview"
  >
    <h2 class="principal-portfolio-overview__title">Portfolio Overview</h2>
    <div class="principal-portfolio-overview__grid">
      <div class="principal-portfolio-overview__card">
        <span class="principal-portfolio-overview__label">Total Sales</span>
        <span class="principal-portfolio-overview__value" :title="totalSalesTitle ?? undefined">
          {{ totalSalesDisplay }}
        </span>
      </div>
      <div class="principal-portfolio-overview__card">
        <span class="principal-portfolio-overview__label">Total Target</span>
        <span class="principal-portfolio-overview__value" :title="totalTargetTitle ?? undefined">
          {{ totalTargetDisplay }}
        </span>
      </div>
      <div
        class="principal-portfolio-overview__card principal-portfolio-overview__card--primary"
      >
        <span class="principal-portfolio-overview__label">Portfolio Achievement %</span>
        <span class="principal-portfolio-overview__value">{{ achievementDisplay }}</span>
        <span class="principal-portfolio-overview__hint">Portfolio level, not a row sum</span>
      </div>
      <div class="principal-portfolio-overview__card">
        <span class="principal-portfolio-overview__label">Portfolio Coverage %</span>
        <span class="principal-portfolio-overview__value">{{ coverageDisplay }}</span>
        <span class="principal-portfolio-overview__hint">{{ coveragePopulationDisplay }}</span>
      </div>
      <div
        class="principal-portfolio-overview__card principal-portfolio-overview__card--risk"
      >
        <span class="principal-portfolio-overview__label">Portfolio Return %</span>
        <span class="principal-portfolio-overview__value">{{ returnDisplay }}</span>
        <span class="principal-portfolio-overview__hint">
          Portfolio-level quality ratio &middot; higher is worse
        </span>
      </div>
    </div>
  </section>
</template>

<style scoped>
.principal-portfolio-overview {
  margin-bottom: 1rem;
  padding: 1rem;
  background: var(--p-surface-0);
  border: 1px solid var(--p-surface-200);
  border-radius: var(--dashboard-radius-sm);
  box-shadow: var(--dashboard-shadow-idle);
}

.principal-portfolio-overview__title {
  margin: 0 0 0.75rem;
  font-size: 1rem;
}

.principal-portfolio-overview__grid {
  display: grid;
  grid-template-columns: repeat(5, minmax(0, 1fr));
  gap: 1rem;
}

.principal-portfolio-overview__card {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  padding: 0.75rem;
  border: 1px solid var(--p-surface-200);
  border-radius: var(--dashboard-radius-sm);
  border-top: 3px solid var(--p-primary-color, #2563eb);
  min-width: 0;
}

.principal-portfolio-overview__card--primary {
  border-top-color: var(--p-primary-color, #2563eb);
  background: color-mix(in srgb, var(--p-primary-color, #2563eb) 4%, white);
}

.principal-portfolio-overview__card--risk {
  border-top-color: #dc2626;
  background: color-mix(in srgb, #dc2626 4%, white);
}

.principal-portfolio-overview__label {
  font-size: 0.6875rem;
  font-weight: 600;
  color: var(--p-text-muted-color, #64748b);
  letter-spacing: 0.01em;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.principal-portfolio-overview__value {
  font-size: 1.5rem;
  font-weight: 800;
  line-height: 1.15;
  letter-spacing: -0.03em;
  font-variant-numeric: tabular-nums;
  color: var(--p-text-color);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.principal-portfolio-overview__card--primary .principal-portfolio-overview__value {
  font-size: 1.75rem;
}

.principal-portfolio-overview__hint {
  font-size: 0.75rem;
  color: var(--p-text-muted-color);
}

@media (max-width: 1100px) {
  .principal-portfolio-overview__grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (max-width: 600px) {
  .principal-portfolio-overview__grid {
    grid-template-columns: 1fr;
  }
}
</style>
