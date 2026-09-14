<script setup lang="ts">
import { computed } from 'vue'
import ProgressSpinner from 'primevue/progressspinner'
import type { DashboardCollectionResponse } from '@/models/dashboard'
import { formatCurrency, formatPercent } from '@/services/formatters'

const props = withDefaults(
  defineProps<{
    collection?: DashboardCollectionResponse | null
    businessDate?: Date | null
    loading?: boolean
  }>(),
  {
    collection: null,
    businessDate: null,
    loading: false,
  },
)

const emit = defineEmits<{
  alertClick: []
}>()

const unavailable = computed(
  () => !props.loading && (props.collection == null || !props.collection.IsAvailable),
)

const cashCollectedMtd = computed<number | null>(() => {
  const cards = props.collection?.AttentionCards
  if (cards?.CashCollectedMtd != null) return cards.CashCollectedMtd

  const recovery = props.collection?.RecoverySummary
  if (recovery?.CashCollectedMtd != null) return recovery.CashCollectedMtd

  return null
})

const recoveryVsBillingPercent = computed<number | null>(() => {
  const cards = props.collection?.AttentionCards
  if (cards?.RecoveryVsBillingPercent != null) return cards.RecoveryVsBillingPercent

  const recovery = props.collection?.RecoverySummary
  if (recovery?.RecoveryVsBillingPercent != null) return recovery.RecoveryVsBillingPercent

  return null
})

const cashCollectedText = computed(() =>
  cashCollectedMtd.value == null ? 'No Data' : formatCurrency(cashCollectedMtd.value),
)

const recoveryVsBillingText = computed(() =>
  recoveryVsBillingPercent.value == null ? 'No Data' : formatPercent(recoveryVsBillingPercent.value),
)

const showOverdueAlert = computed(
  () => !unavailable.value && props.collection?.AttentionCards?.ExposureRequiresAttention === true,
)

const mtdContextLabel = computed(() => {
  if (!props.businessDate) return 'MTD'
  const formatted = new Intl.DateTimeFormat('id-ID', { dateStyle: 'medium' }).format(
    props.businessDate,
  )
  return `MTD · as of ${formatted}`
})

function onAlertClick(): void {
  emit('alertClick')
}
</script>

<template>
  <section class="collection-health" aria-labelledby="collection-health-title">
    <div class="collection-health__header">
      <h2 id="collection-health-title" class="collection-health__title">Collection Health (MTD)</h2>
      <span class="collection-health__context">{{ mtdContextLabel }}</span>
    </div>

    <div v-if="loading" class="collection-health__loading">
      <ProgressSpinner style="width: 2rem; height: 2rem" stroke-width="4" />
    </div>

    <p v-else-if="unavailable" class="collection-health__empty">
      Collection data is unavailable. Sales metrics are unaffected.
    </p>

    <template v-else>
      <div class="collection-health__cards">
        <article class="collection-health__card">
          <span class="collection-health__card-label">Cash Collected MTD</span>
          <span class="collection-health__card-value" :title="cashCollectedText">
            {{ cashCollectedText }}
          </span>
          <span class="collection-health__card-subtitle">Cash recovered month-to-date</span>
        </article>

        <article class="collection-health__card">
          <span class="collection-health__card-label">Recovery vs Billing %</span>
          <span class="collection-health__card-value" :title="recoveryVsBillingText">
            {{ recoveryVsBillingText }}
          </span>
          <span class="collection-health__card-subtitle">Receivables recovered vs billed</span>
        </article>
      </div>

      <button
        v-if="showOverdueAlert"
        type="button"
        class="collection-health__alert"
        @click="onAlertClick"
      >
        <i class="pi pi-exclamation-triangle" aria-hidden="true" />
        <span class="collection-health__alert-text">Overdue exposure requires attention</span>
        <i class="pi pi-arrow-right collection-health__alert-arrow" aria-hidden="true" />
      </button>
    </template>
  </section>
</template>

<style scoped>
.collection-health {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.collection-health__header {
  display: flex;
  flex-wrap: wrap;
  align-items: baseline;
  justify-content: space-between;
  gap: 0.5rem;
}

.collection-health__title {
  margin: 0;
  font-size: 1.125rem;
}

.collection-health__context {
  font-size: 0.6875rem;
  font-weight: 700;
  letter-spacing: 0.08em;
  text-transform: uppercase;
  color: var(--p-text-muted-color, #94a3b8);
}

.collection-health__loading {
  display: flex;
  justify-content: center;
  padding: 1.5rem 0;
}

.collection-health__empty {
  margin: 0;
  padding: 0.875rem;
  border-radius: var(--dashboard-radius);
  background: var(--kpi-status-unknown-bg, #f1f5f9);
  color: var(--kpi-status-unknown-color, #64748b);
}

.collection-health__cards {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(14rem, 1fr));
  gap: 0.75rem;
}

/* Collection accent (domain-collection) — never a sales KPI group color (GAP-005). */
.collection-health__card {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  padding: 0.875rem;
  border-radius: var(--dashboard-radius);
  border-top: 3px solid var(--domain-collection-color, #ea580c);
  background: var(--domain-collection-tint, #fff8f3);
  box-shadow: var(--dashboard-shadow-idle);
  min-width: 0;
}

.collection-health__card-label {
  font-size: 0.6875rem;
  font-weight: 600;
  color: var(--p-text-muted-color, #64748b);
}

.collection-health__card-value {
  font-size: 1.5rem;
  font-weight: 800;
  letter-spacing: -0.03em;
  font-variant-numeric: tabular-nums;
  color: var(--p-text-color);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.collection-health__card-subtitle {
  font-size: 0.625rem;
  color: var(--p-text-muted-color, #94a3b8);
  opacity: 0.75;
}

.collection-health__alert {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  align-self: flex-start;
  padding: 0.375rem 0.75rem;
  border: none;
  border-radius: var(--dashboard-radius-chip, 999px);
  background: var(--kpi-status-critical-bg, #ffe4e6);
  color: var(--kpi-status-critical-color, #9f1239);
  font-size: 0.75rem;
  font-weight: 700;
  cursor: pointer;
  transition: box-shadow var(--dashboard-transition, 175ms ease);
}

.collection-health__alert:hover {
  box-shadow: var(--dashboard-shadow-hover);
}

.collection-health__alert-arrow {
  font-size: 0.6875rem;
}
</style>
