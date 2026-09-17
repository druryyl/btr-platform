<script setup lang="ts">
import { computed } from 'vue'
import Card from 'primevue/card'
import ProgressSpinner from 'primevue/progressspinner'
import type { FieldActivityTeamKpis } from '@/models/fieldActivity'
import { buildFunnelStages, type FunnelStage } from '@/services/fieldActivityFunnel'
import { formatCurrency, formatNumber, formatPercent } from '@/services/formatters'

const props = withDefaults(
  defineProps<{
    kpis: FieldActivityTeamKpis | null
    loading?: boolean
  }>(),
  {
    loading: false,
  },
)

const stages = computed(() => buildFunnelStages(props.kpis))

const hasData = computed(() => stages.value.length > 0)

function formatStageValue(stage: FunnelStage): string {
  return stage.key === 'orderValue' ? formatCurrency(stage.value) : formatNumber(stage.value)
}

function formatConversion(stage: FunnelStage): string {
  const conversion = stage.conversion
  if (!conversion || conversion.value == null) return '—'
  return conversion.kind === 'currency'
    ? formatCurrency(conversion.value)
    : formatPercent(conversion.value)
}
</script>

<template>
  <Card class="execution-funnel portal-chart-card">
    <template #title>
      <div class="execution-funnel__title">
        <i class="pi pi-filter" aria-hidden="true" />
        <span>Execution Funnel</span>
      </div>
    </template>

    <template #content>
      <div v-if="loading" class="execution-funnel__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>

      <p v-else-if="!hasData" class="execution-funnel__empty">No data for this date.</p>

      <div v-else class="execution-funnel__strip">
        <template v-for="(stage, index) in stages" :key="stage.key">
          <div class="execution-funnel__stage">
            <span class="execution-funnel__stage-label">{{ stage.label }}</span>
            <span class="execution-funnel__stage-value">{{ formatStageValue(stage) }}</span>
            <span v-if="stage.conversion" class="execution-funnel__stage-conversion">
              <span class="execution-funnel__stage-conversion-label">
                {{ stage.conversion.label }}
              </span>
              <span class="execution-funnel__stage-conversion-value">
                {{ formatConversion(stage) }}
              </span>
            </span>
            <span v-if="stage.leak" class="execution-funnel__stage-leak">
              <i class="pi pi-exclamation-circle" aria-hidden="true" />
              <span class="execution-funnel__stage-leak-label">{{ stage.leak.label }}</span>
              <span class="execution-funnel__stage-leak-value">
                {{ formatNumber(stage.leak.value) }}
              </span>
            </span>
          </div>

          <div
            v-if="index < stages.length - 1"
            class="execution-funnel__arrow"
            aria-hidden="true"
          >
            <i class="pi pi-arrow-right" />
          </div>
        </template>
      </div>
    </template>
  </Card>
</template>

<style scoped>
.execution-funnel__title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.execution-funnel__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.execution-funnel__empty {
  margin: 0;
  color: var(--p-text-muted-color, #64748b);
}

.execution-funnel__strip {
  display: flex;
  align-items: stretch;
  gap: 0.5rem;
}

.execution-funnel__stage {
  display: flex;
  flex: 1 1 0;
  min-width: 0;
  flex-direction: column;
  gap: 0.25rem;
  padding: 0.75rem;
  border-radius: var(--dashboard-radius, 0.5rem);
  border-top: 3px solid var(--domain-sales-color, #0d9488);
  background: var(--p-surface-50, #f8fafc);
}

.execution-funnel__stage-label {
  font-size: 0.625rem;
  font-weight: 700;
  letter-spacing: 0.08em;
  text-transform: uppercase;
  color: var(--p-text-muted-color, #94a3b8);
}

.execution-funnel__stage-value {
  font-size: 1.375rem;
  font-weight: 800;
  letter-spacing: -0.03em;
  font-variant-numeric: tabular-nums;
  color: var(--p-text-color);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.execution-funnel__stage-conversion {
  display: flex;
  flex-direction: column;
  gap: 0.0625rem;
  font-size: 0.6875rem;
  line-height: 1.2;
}

.execution-funnel__stage-conversion-label {
  color: var(--p-text-muted-color, #64748b);
}

.execution-funnel__stage-conversion-value {
  font-weight: 700;
  font-variant-numeric: tabular-nums;
  color: var(--p-text-color);
}

.execution-funnel__stage-leak {
  display: flex;
  align-items: center;
  gap: 0.25rem;
  margin-top: auto;
  padding-top: 0.25rem;
  font-size: 0.6875rem;
  line-height: 1.2;
  color: var(--kpi-status-warning-color, #b45309);
}

.execution-funnel__stage-leak-label {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.execution-funnel__stage-leak-value {
  flex: none;
  font-weight: 700;
  font-variant-numeric: tabular-nums;
}

.execution-funnel__arrow {
  display: flex;
  flex: none;
  align-items: center;
  color: var(--p-text-muted-color, #94a3b8);
  font-size: 0.875rem;
}

@media (max-width: 768px) {
  .execution-funnel__strip {
    flex-direction: column;
  }

  .execution-funnel__arrow {
    justify-content: center;
  }

  .execution-funnel__arrow i {
    transform: rotate(90deg);
  }
}
</style>
