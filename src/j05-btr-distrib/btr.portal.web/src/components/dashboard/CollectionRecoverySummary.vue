<script setup lang="ts">
import { computed } from 'vue'
import Card from 'primevue/card'
import type { DashboardCollectionRecoverySummary } from '@/models/dashboard'
import { formatCurrency, formatPercent } from '@/services/formatters'

const props = defineProps<{
  summary: DashboardCollectionRecoverySummary | null
  loading: boolean
}>()

const recoveryBandClass = computed(() => {
  const pct = props.summary?.RecoveryVsBillingPercent
  if (pct == null) return ''
  if (pct >= 100) return 'collection-recovery__value--healthy'
  if (pct >= 80) return 'collection-recovery__value--warning'
  return 'collection-recovery__value--critical'
})

const mixSegments = computed(() => {
  const s = props.summary
  if (!s) return []

  return [
    { key: 'cash', label: 'Cash', percent: s.PaymentMixCashPercent, color: '#22c55e' },
    { key: 'giro', label: 'Giro', percent: s.PaymentMixGiroPercent, color: '#3b82f6' },
    { key: 'adjustment', label: 'Adjustment', percent: s.PaymentMixAdjustmentPercent, color: '#94a3b8' },
  ].filter((seg) => seg.percent != null && seg.percent > 0)
})
</script>

<template>
  <section class="collection-recovery">
    <h2 class="collection-recovery__heading">Recovery Summary</h2>
    <Card>
      <template #content>
        <div class="collection-recovery__grid">
          <div class="collection-recovery__metric">
            <span class="collection-recovery__label">Cash Collected MTD</span>
            <span
              class="collection-recovery__value"
              title="Cash payments received this month"
            >
              {{ summary ? formatCurrency(summary.CashCollectedMtd) : '—' }}
            </span>
          </div>
          <div class="collection-recovery__metric">
            <span class="collection-recovery__label">Recovery vs Billing %</span>
            <span class="collection-recovery__value" :class="recoveryBandClass">
              {{ summary?.RecoveryVsBillingPercent != null ? formatPercent(summary.RecoveryVsBillingPercent) : '—' }}
            </span>
          </div>
        </div>

        <div v-if="mixSegments.length > 0" class="collection-recovery__mix">
          <span class="collection-recovery__label">Payment Mix</span>
          <div class="collection-recovery__bar" role="img" aria-label="Payment mix breakdown">
            <div
              v-for="seg in mixSegments"
              :key="seg.key"
              class="collection-recovery__segment"
              :style="{ width: `${seg.percent}%`, backgroundColor: seg.color }"
              :title="`${seg.label}: ${formatPercent(seg.percent)}`"
            />
          </div>
          <div class="collection-recovery__legend">
            <span v-for="seg in mixSegments" :key="seg.key" class="collection-recovery__legend-item">
              <span class="collection-recovery__swatch" :style="{ backgroundColor: seg.color }" />
              {{ seg.label }} {{ formatPercent(seg.percent) }}
            </span>
          </div>
        </div>
      </template>
    </Card>
  </section>
</template>

<style scoped>
.collection-recovery__heading {
  margin: 0 0 1rem;
  font-size: 1.25rem;
}

.collection-recovery__grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1.5rem;
  margin-bottom: 1.5rem;
}

.collection-recovery__label {
  display: block;
  font-size: 0.875rem;
  color: var(--p-text-muted-color);
}

.collection-recovery__value {
  display: block;
  font-size: 1.5rem;
  font-weight: 700;
  margin-top: 0.25rem;
}

.collection-recovery__value--healthy {
  color: #16a34a;
}

.collection-recovery__value--warning {
  color: #d97706;
}

.collection-recovery__value--critical {
  color: #dc2626;
}

.collection-recovery__bar {
  display: flex;
  height: 1.25rem;
  border-radius: 0.375rem;
  overflow: hidden;
  margin-top: 0.5rem;
}

.collection-recovery__segment {
  min-width: 2px;
  height: 100%;
}

.collection-recovery__legend {
  display: flex;
  flex-wrap: wrap;
  gap: 1rem;
  margin-top: 0.75rem;
  font-size: 0.875rem;
}

.collection-recovery__legend-item {
  display: inline-flex;
  align-items: center;
  gap: 0.375rem;
}

.collection-recovery__swatch {
  width: 0.75rem;
  height: 0.75rem;
  border-radius: 2px;
}
</style>
