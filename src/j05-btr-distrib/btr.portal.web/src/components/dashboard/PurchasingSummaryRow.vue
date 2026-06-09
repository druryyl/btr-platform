<script setup lang="ts">
import Card from 'primevue/card'
import ProgressSpinner from 'primevue/progressspinner'
import type { DashboardPurchasingSummaryRow } from '@/models/dashboard'
import { formatCurrency, formatNumber, formatPercent } from '@/services/formatters'

defineProps<{
  summary: DashboardPurchasingSummaryRow | null
  loading: boolean
}>()
</script>

<template>
  <section class="purchasing-summary">
    <h2 class="purchasing-summary__heading">Purchasing Summary</h2>
    <Card>
      <template #content>
        <div v-if="loading" class="purchasing-summary__loading">
          <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
        </div>
        <div v-else class="purchasing-summary__row">
          <div class="metric">
            <span class="metric__label">Grand Total Purchase</span>
            <span class="metric__value">
              {{ summary ? formatCurrency(summary.GrandTotalPurchase) : '—' }}
            </span>
          </div>
          <div class="metric">
            <span class="metric__label">Total Invoice</span>
            <span class="metric__value">
              {{ summary ? formatNumber(summary.TotalInvoice) : '—' }}
            </span>
          </div>
          <div class="metric">
            <span class="metric__label">Posted %</span>
            <span class="metric__value">
              {{ summary?.PostedPercent != null ? formatPercent(summary.PostedPercent) : '—' }}
            </span>
          </div>
          <div class="metric">
            <span
              class="metric__label"
              title="Unposted (BELUM) is normal immediately after invoice entry. Qualified backlog counts invoices unposted for 3+ days."
            >
              Pending Posting Value (all BELUM)
            </span>
            <span class="metric__value">
              {{ summary ? formatCurrency(summary.PendingPostingValue) : '—' }}
            </span>
          </div>
          <div class="metric">
            <span
              class="metric__label"
              title="Age-qualified BELUM invoices (3+ days since last update)."
            >
              Qualified Backlog
            </span>
            <span class="metric__value">
              {{
                summary
                  ? `${formatNumber(summary.QualifiedBacklogCount)} · ${formatCurrency(summary.QualifiedBacklogValue)}`
                  : '—'
              }}
            </span>
          </div>
        </div>
      </template>
    </Card>
  </section>
</template>

<style scoped>
.purchasing-summary__heading {
  margin: 0 0 1rem;
  font-size: 1.25rem;
}

.purchasing-summary__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.purchasing-summary__row {
  display: grid;
  grid-template-columns: repeat(5, minmax(0, 1fr));
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
}

@media (max-width: 1100px) {
  .purchasing-summary__row {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (max-width: 640px) {
  .purchasing-summary__row {
    grid-template-columns: 1fr;
  }
}
</style>
