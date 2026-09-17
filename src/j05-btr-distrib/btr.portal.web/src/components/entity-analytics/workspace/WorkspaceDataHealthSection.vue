<script setup lang="ts">
import { computed } from 'vue'
import { RouterLink } from 'vue-router'
import DashboardMetric from '@/components/dashboard/primitives/DashboardMetric.vue'
import type { EntityDataHealthResponse } from '@/models/entityAnalytics'
import {
  DATA_HEALTH_SNAPSHOT_DISCLOSURE,
  UNKNOWN_PRINCIPAL_EVIDENCE_ROUTE,
  formatSnapshotPeriod,
} from '@/services/workspaceDataHealth'
import { formatCurrency, formatDateTime, formatNumber, formatPercent } from '@/services/formatters'

const props = defineProps<{
  dataHealth: EntityDataHealthResponse | null
  loading?: boolean
}>()

const reportingPeriod = computed(() =>
  formatSnapshotPeriod(props.dataHealth?.PeriodYear, props.dataHealth?.PeriodMonth),
)

const dataSnapshotPeriod = computed(() =>
  formatSnapshotPeriod(props.dataHealth?.PeriodYear, props.dataHealth?.PeriodMonth),
)
</script>

<template>
  <div v-if="loading" class="iw-skeleton" style="min-height: 10rem" />
  <div v-else-if="dataHealth?.IsAvailable" class="iw-panel-card">
    <h3 class="iw-section-title">Data Health</h3>
    <p class="iw-meta">
      Data completeness and quality for the Principal population. Lens-independent and visible
      before conclusions are drawn.
    </p>

    <div class="iw-data-health__indicators">
      <DashboardMetric
        label="Target Coverage %"
        :value="formatPercent(dataHealth.TargetCoveragePercentage)"
        :empty="dataHealth.TargetCoveragePercentage == null"
      />
      <DashboardMetric
        label="Principals Missing Target Count"
        :value="formatNumber(dataHealth.PrincipalsMissingTargetCount)"
      />
      <DashboardMetric
        label="Unknown Principal Exception Count"
        :value="formatNumber(dataHealth.UnknownPrincipalExceptionCount)"
      />
      <DashboardMetric
        label="Unknown Principal Exception Amount"
        :value="formatCurrency(dataHealth.UnknownPrincipalExceptionAmount)"
      />
    </div>

    <div class="iw-data-health__unknown">
      <p class="iw-meta">
        Unknown Principal lines are excluded from Principal amounts and are not assigned to a
        Principal. They remain a visible exception and are never silently dropped.
      </p>
      <RouterLink :to="UNKNOWN_PRINCIPAL_EVIDENCE_ROUTE" class="iw-data-health__link">
        View supporting evidence
      </RouterLink>
    </div>

    <div class="iw-data-health__freshness">
      <h4 class="iw-meta">Snapshot freshness</h4>
      <dl class="iw-data-health__freshness-list">
        <div v-if="dataHealth.GeneratedAt">
          <dt>Generated At</dt>
          <dd>{{ formatDateTime(dataHealth.GeneratedAt) }}</dd>
        </div>
        <div v-if="reportingPeriod">
          <dt>Reporting Period</dt>
          <dd>{{ reportingPeriod }}</dd>
        </div>
        <div v-if="dataSnapshotPeriod">
          <dt>Data Snapshot Period</dt>
          <dd>{{ dataSnapshotPeriod }}</dd>
        </div>
      </dl>
      <p class="iw-meta">{{ DATA_HEALTH_SNAPSHOT_DISCLOSURE }}</p>
    </div>
  </div>
</template>

<style scoped>
.iw-data-health__indicators {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(12rem, 1fr));
  gap: 1rem;
  margin-top: 0.75rem;
}

.iw-data-health__unknown {
  margin-top: 1rem;
  padding-top: 0.75rem;
  border-top: 1px solid #e2e8f0;
}

.iw-data-health__unknown p {
  margin: 0 0 0.5rem;
}

.iw-data-health__link {
  color: var(--p-primary-color);
  font-weight: 600;
  text-decoration: none;
}

.iw-data-health__link:hover {
  text-decoration: underline;
}

.iw-data-health__freshness {
  margin-top: 1rem;
  padding-top: 0.75rem;
  border-top: 1px solid #e2e8f0;
}

.iw-data-health__freshness h4 {
  margin: 0 0 0.5rem;
}

.iw-data-health__freshness-list {
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
  margin: 0 0 0.75rem;
}

.iw-data-health__freshness-list div {
  display: flex;
  gap: 0.75rem;
}

.iw-data-health__freshness-list dt {
  font-weight: 600;
  color: var(--iw-text-secondary, #64748b);
}

.iw-data-health__freshness-list dd {
  margin: 0;
}

.iw-data-health__freshness-list dd::before {
  content: '·';
  margin-right: 0.75rem;
  color: var(--iw-text-meta, #94a3b8);
}
</style>