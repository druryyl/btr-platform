<script setup lang="ts">
import { computed, onMounted } from 'vue'
import Button from 'primevue/button'
import Message from 'primevue/message'
import Tag from 'primevue/tag'
import AlertCenterPlatformSection from '@/components/alerts/AlertCenterPlatformSection.vue'
import AlertCenterCategorySummary from '@/components/alerts/AlertCenterCategorySummary.vue'
import AlertCenterAlertTable from '@/components/alerts/AlertCenterAlertTable.vue'
import AlertCenterInventoryRiskSummary from '@/components/alerts/AlertCenterInventoryRiskSummary.vue'
import AlertCenterConcentrationsSection from '@/components/alerts/AlertCenterConcentrationsSection.vue'
import AlertCenterNavigationSection from '@/components/alerts/AlertCenterNavigationSection.vue'
import { formatDateTime } from '@/services/formatters'
import { useDashboardStore } from '@/stores/dashboardStore'

const dashboard = useDashboardStore()

const platformStatus = computed(() => {
  const alerts = dashboard.alerts
  if (!alerts) return 'Unknown'

  if (alerts.OverallHealthStatus === 'degraded') return 'Degraded'
  if (!alerts.IsDataFresh) return 'Stale'
  return 'OK'
})

const platformStatusSeverity = computed(() => {
  const status = platformStatus.value
  if (status === 'Degraded') return 'danger'
  if (status === 'Stale') return 'warn'
  return 'success'
})

onMounted(() => {
  void dashboard.loadAlerts()
})
</script>

<template>
  <div class="alert-center">
    <div class="alert-center__header">
      <div>
        <h1>Alert Center</h1>
        <p>What requires attention right now across the business?</p>
        <p v-if="dashboard.alerts?.LastRefreshed" class="alert-center__refreshed">
          Last Refreshed: {{ formatDateTime(dashboard.alerts.LastRefreshed) }}
        </p>
        <Tag
          v-if="dashboard.alerts"
          :value="`Platform: ${platformStatus}`"
          :severity="platformStatusSeverity"
          class="alert-center__status"
        />
      </div>
      <Button
        label="Refresh"
        icon="pi pi-refresh"
        outlined
        :loading="dashboard.loading"
        @click="dashboard.loadAlerts()"
      />
    </div>

    <Message
      v-if="dashboard.alerts && !dashboard.alerts.IsDataFresh"
      severity="warn"
      :closable="false"
      class="alert-center__banner"
    >
      ⚠ Dashboard Data Not Fresh
    </Message>

    <Message
      v-if="dashboard.alerts && dashboard.alerts.OverallHealthStatus === 'degraded'"
      severity="error"
      :closable="false"
      class="alert-center__banner"
    >
      Dashboard snapshot refresh is degraded. Some analytics may be outdated.
    </Message>

    <Message
      v-if="dashboard.alerts && dashboard.alerts.OverallHealthStatus === 'refreshing'"
      severity="info"
      :closable="false"
      class="alert-center__banner"
    >
      Dashboard snapshots are currently refreshing.
    </Message>

    <Message v-if="dashboard.error" severity="error" :closable="false">
      {{ dashboard.error }}
    </Message>

    <AlertCenterPlatformSection
      v-if="dashboard.alerts"
      :alerts="dashboard.alerts.PlatformAlerts"
    />

    <AlertCenterCategorySummary
      v-if="dashboard.alerts"
      :summaries="dashboard.alerts.CategorySummaries"
    />

    <AlertCenterAlertTable
      :groups="dashboard.alerts?.AlertGroups ?? []"
      :loading="dashboard.loading"
    />

    <AlertCenterInventoryRiskSummary
      :summary="dashboard.alerts?.InventoryRiskSummary ?? null"
    />

    <AlertCenterConcentrationsSection
      :items="dashboard.alerts?.Concentrations ?? []"
      :loading="dashboard.loading"
    />

    <AlertCenterNavigationSection
      :navigation="dashboard.alerts?.Navigation ?? null"
    />
  </div>
</template>

<style scoped>
.alert-center__header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
  margin-bottom: 1.5rem;
}

.alert-center__header h1 {
  margin: 0 0 0.25rem;
}

.alert-center__header p {
  margin: 0;
  color: var(--p-text-muted-color);
}

.alert-center__refreshed {
  margin-top: 0.5rem !important;
  font-size: 0.875rem;
}

.alert-center__status {
  margin-top: 0.5rem;
}

.alert-center__banner {
  margin-bottom: 1rem;
}

.alert-center > section {
  margin-bottom: 2rem;
}
</style>
