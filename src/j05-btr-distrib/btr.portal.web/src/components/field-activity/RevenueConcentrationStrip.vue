<script setup lang="ts">
import { computed } from 'vue'
import Card from 'primevue/card'
import ProgressSpinner from 'primevue/progressspinner'
import type { FieldActivitySalesmanOverviewRow } from '@/models/fieldActivity'
import {
  buildRevenueConcentration,
  CONCENTRATION_OTHERS_KEY,
  type ConcentrationSegment,
} from '@/services/fieldActivityConcentration'
import { formatCurrency, formatPercent } from '@/services/formatters'

const props = withDefaults(
  defineProps<{
    salesmen: FieldActivitySalesmanOverviewRow[]
    loading?: boolean
  }>(),
  {
    loading: false,
  },
)

const NAMED_SEGMENT_COLORS = ['#0d9488', '#2563eb', '#7c3aed']
const OTHERS_SEGMENT_COLOR = '#94a3b8'

const concentration = computed(() => buildRevenueConcentration(props.salesmen))

function segmentColor(segment: ConcentrationSegment, index: number): string {
  return segment.key === CONCENTRATION_OTHERS_KEY
    ? OTHERS_SEGMENT_COLOR
    : NAMED_SEGMENT_COLORS[index % NAMED_SEGMENT_COLORS.length]
}

function segmentLabel(segment: ConcentrationSegment): string {
  return segment.code ? `${segment.code} · ${segment.label}` : segment.label
}

function segmentTitle(segment: ConcentrationSegment): string {
  return `${segmentLabel(segment)} — ${formatCurrency(segment.amount)} (${formatPercent(segment.percent)})`
}
</script>

<template>
  <Card class="revenue-concentration portal-chart-card">
    <template #title>
      <div class="revenue-concentration__title">
        <i class="pi pi-chart-pie" aria-hidden="true" />
        <span>Revenue Concentration</span>
      </div>
    </template>

    <template #content>
      <div v-if="loading" class="revenue-concentration__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>

      <p v-else-if="concentration.isEmpty" class="revenue-concentration__empty">
        No order value for this date.
      </p>

      <template v-else>
        <div class="revenue-concentration__summary">
          <span class="revenue-concentration__summary-item">
            <span class="revenue-concentration__summary-label">Top 1</span>
            <span class="revenue-concentration__summary-value">
              {{ formatPercent(concentration.top1Share) }}
            </span>
            <span class="revenue-concentration__summary-of">of order value</span>
          </span>
          <span class="revenue-concentration__summary-item">
            <span class="revenue-concentration__summary-label">Top 3</span>
            <span class="revenue-concentration__summary-value">
              {{ formatPercent(concentration.top3Share) }}
            </span>
            <span class="revenue-concentration__summary-of">of order value</span>
          </span>
        </div>

        <div class="revenue-concentration__bar" aria-hidden="true">
          <div
            v-for="(segment, index) in concentration.segments"
            :key="segment.key"
            class="revenue-concentration__segment"
            :style="{ width: `${segment.percent}%`, background: segmentColor(segment, index) }"
            :title="segmentTitle(segment)"
          />
        </div>

        <ul class="revenue-concentration__legend">
          <li
            v-for="(segment, index) in concentration.segments"
            :key="segment.key"
            class="revenue-concentration__legend-item"
          >
            <span
              class="revenue-concentration__legend-swatch"
              :style="{ background: segmentColor(segment, index) }"
              aria-hidden="true"
            />
            <span class="revenue-concentration__legend-label" :title="segmentLabel(segment)">
              {{ segmentLabel(segment) }}
            </span>
            <span class="revenue-concentration__legend-amount">
              {{ formatCurrency(segment.amount) }}
            </span>
            <span class="revenue-concentration__legend-percent">
              {{ formatPercent(segment.percent) }}
            </span>
          </li>
        </ul>
      </template>
    </template>
  </Card>
</template>

<style scoped>
.revenue-concentration__title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.revenue-concentration__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.revenue-concentration__empty {
  margin: 0;
  color: var(--p-text-muted-color, #64748b);
}

.revenue-concentration__summary {
  display: flex;
  flex-wrap: wrap;
  gap: 1.5rem;
  margin-bottom: 0.75rem;
}

.revenue-concentration__summary-item {
  display: flex;
  align-items: baseline;
  gap: 0.375rem;
  font-size: 0.8125rem;
}

.revenue-concentration__summary-label {
  font-size: 0.6875rem;
  font-weight: 700;
  letter-spacing: 0.08em;
  text-transform: uppercase;
  color: var(--p-text-muted-color, #94a3b8);
}

.revenue-concentration__summary-value {
  font-size: 1.125rem;
  font-weight: 800;
  letter-spacing: -0.03em;
  font-variant-numeric: tabular-nums;
  color: var(--domain-sales-color, #0d9488);
}

.revenue-concentration__summary-of {
  color: var(--p-text-muted-color, #64748b);
}

.revenue-concentration__bar {
  display: flex;
  width: 100%;
  height: 1.25rem;
  border-radius: 999px;
  overflow: hidden;
  background: var(--kpi-status-unknown-bg, #f1f5f9);
}

.revenue-concentration__segment {
  height: 100%;
  transition: width var(--dashboard-transition, 175ms ease);
}

.revenue-concentration__legend {
  margin: 0.75rem 0 0;
  padding: 0;
  list-style: none;
  display: flex;
  flex-direction: column;
  gap: 0.375rem;
}

.revenue-concentration__legend-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.8125rem;
}

.revenue-concentration__legend-swatch {
  flex: none;
  width: 0.625rem;
  height: 0.625rem;
  border-radius: 0.125rem;
}

.revenue-concentration__legend-label {
  flex: 1 1 auto;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  color: var(--p-text-color);
}

.revenue-concentration__legend-amount {
  flex: none;
  font-variant-numeric: tabular-nums;
  color: var(--p-text-muted-color, #64748b);
}

.revenue-concentration__legend-percent {
  flex: none;
  min-width: 3rem;
  text-align: right;
  font-weight: 700;
  font-variant-numeric: tabular-nums;
  color: var(--p-text-color);
}
</style>
