<script setup lang="ts">
import ProfileSectionCard from '@/components/entity-analytics/ProfileSectionCard.vue'
import type { CompareKpiSection } from '@/models/entityAnalytics'

defineProps<{
  section: CompareKpiSection | null | undefined
  loading?: boolean
}>()
</script>

<template>
  <ProfileSectionCard
    title="KPI Comparison"
    :is-available="section?.IsAvailable"
    :unavailable-reason="section?.UnavailableReason"
    :loading="loading"
  >
    <div v-if="section?.Rows?.length" class="entity-compare-table__wrap">
      <table class="entity-compare-table">
        <thead>
          <tr>
            <th>KPI</th>
            <th v-for="cell in section.Rows[0].Values" :key="cell.EntityCode">
              {{ cell.DisplayName }}
              <small>{{ cell.EntityCode }}</small>
            </th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="row in section.Rows" :key="row.KpiId">
            <td>
              <strong>{{ row.DisplayName }}</strong>
              <small>{{ row.Unit }}</small>
            </td>
            <td v-for="cell in row.Values" :key="`${row.KpiId}-${cell.EntityCode}`">
              {{ cell.FormattedValue }}
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </ProfileSectionCard>
</template>

<style scoped>
.entity-compare-table__wrap {
  overflow-x: auto;
}

.entity-compare-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.9rem;
}

.entity-compare-table th,
.entity-compare-table td {
  border-bottom: 1px solid var(--p-content-border-color, #e2e8f0);
  padding: 0.65rem 0.75rem;
  text-align: left;
  vertical-align: top;
}

.entity-compare-table th small,
.entity-compare-table td small {
  display: block;
  color: var(--p-text-muted-color, #64748b);
  font-weight: 400;
}
</style>
