<script setup lang="ts">
import { ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import Button from 'primevue/button'
import type { DashboardAlertCenterAlertRow } from '@/models/dashboard'
import {
  canInvestigateAlert,
  canViewDashboardAlert,
  formatAlertValue,
} from '@/services/alertCenterAlertActions'
import { navigateToDashboard, navigateToInvestigation } from '@/services/navigateToInvestigation'

const props = defineProps<{
  alerts: DashboardAlertCenterAlertRow[]
}>()

const router = useRouter()
const sourceLabel = 'Alert Center'
const first = ref(0)
const rows = ref(10)

watch(
  () => props.alerts,
  () => {
    first.value = 0
  },
)

function investigate(row: DashboardAlertCenterAlertRow): void {
  if (!row.Investigation || !canInvestigateAlert(row)) return
  navigateToInvestigation(router, row.Investigation, sourceLabel)
}

function openDashboard(row: DashboardAlertCenterAlertRow): void {
  if (!row.DashboardRoute) return
  navigateToDashboard(router, row.DashboardRoute)
}
</script>

<template>
  <div class="alert-center-category-table">
    <DataTable
      v-model:first="first"
      :value="alerts"
      paginator
      :rows="rows"
      :rows-per-page-options="[5, 10, 20]"
      striped-rows
      class="alert-center-category-table__table"
    >
      <template #empty>
        <p class="alert-center-category-table__empty">No alerts in this category.</p>
      </template>

      <Column field="EntityName" header="Entity" />
      <Column field="SignalLabel" header="Signal" />
      <Column header="Value">
        <template #body="{ data }">
          {{ formatAlertValue(data) }}
        </template>
      </Column>
      <Column header="Actions">
        <template #body="{ data }">
          <div class="alert-center-category-table__actions">
            <Button
              v-if="canInvestigateAlert(data)"
              label="Investigate"
              text
              size="small"
              @click="investigate(data)"
            />
            <Button
              v-if="canViewDashboardAlert(data)"
              label="View Dashboard"
              text
              size="small"
              severity="secondary"
              @click="openDashboard(data)"
            />
          </div>
        </template>
      </Column>
    </DataTable>
  </div>
</template>

<style scoped>
.alert-center-category-table {
  max-height: 20rem;
  overflow: auto;
}

.alert-center-category-table__actions {
  display: flex;
  flex-wrap: wrap;
  gap: 0.25rem;
}

.alert-center-category-table__empty {
  margin: 0;
  padding: 1rem 0;
  text-align: center;
  color: var(--p-text-muted-color);
}

@media (max-width: 767px) {
  .alert-center-category-table__actions :deep(.p-button) {
    min-height: 2.75rem;
  }
}
</style>
