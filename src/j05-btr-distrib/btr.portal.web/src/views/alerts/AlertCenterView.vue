<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import Button from 'primevue/button'
import Message from 'primevue/message'
import Tag from 'primevue/tag'
import PlatformSnapshotHealthBanners from '@/components/platform/PlatformSnapshotHealthBanners.vue'
import AlertCenterPlatformSection from '@/components/alerts/AlertCenterPlatformSection.vue'
import AlertCenterCategoryCards from '@/components/alerts/AlertCenterCategoryCards.vue'
import AlertCenterCriticalStrip from '@/components/alerts/AlertCenterCriticalStrip.vue'
import AlertCenterCategoryPanels from '@/components/alerts/AlertCenterCategoryPanels.vue'
import AlertCenterInventoryRiskSummary from '@/components/alerts/AlertCenterInventoryRiskSummary.vue'
import AlertCenterConcentrationsSection from '@/components/alerts/AlertCenterConcentrationsSection.vue'
import AlertCenterNavigationSection from '@/components/alerts/AlertCenterNavigationSection.vue'
import { formatDateTime } from '@/services/formatters'
import { shouldShowInfrastructureError } from '@/services/platformDiagnostics'
import { categoryPanelId, findHighestCountCategory } from '@/services/alertCenterPrioritization'
import { useDashboardStore } from '@/stores/dashboardStore'
import { usePresentationStore } from '@/stores/presentationStore'

const dashboard = useDashboardStore()
const presentation = usePresentationStore()
const expandedCategory = ref<string | null>(null)

const sectionNavItems = [
  { id: 'alert-category-summary', label: 'Summary' },
  { id: 'alert-critical', label: 'Critical' },
  { id: 'alert-context', label: 'Inventory' },
  { id: 'alert-concentrations', label: 'Concentrations' },
  { id: 'alert-categories', label: 'Alerts' },
  { id: 'alert-dashboards', label: 'Dashboards' },
]

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

const visibleError = computed(() =>
  shouldShowInfrastructureError(dashboard.error, presentation.hidePlatformDiagnostics)
    ? dashboard.error
    : null,
)

function onCategorySelect(category: string): void {
  expandedCategory.value = category
  const element = document.getElementById(categoryPanelId(category))
  element?.scrollIntoView({ behavior: 'smooth', block: 'start' })
}

watch(
  () => dashboard.alerts,
  (alerts) => {
    if (!alerts) return
    expandedCategory.value = findHighestCountCategory(alerts.CategorySummaries)
  },
  { immediate: true },
)

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
        <p
          v-if="dashboard.alerts?.LastRefreshed && !presentation.hidePlatformDiagnostics"
          class="alert-center__refreshed"
        >
          Last Refreshed: {{ formatDateTime(dashboard.alerts.LastRefreshed) }}
        </p>
        <Tag
          v-if="dashboard.alerts && !presentation.hidePlatformDiagnostics"
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

    <PlatformSnapshotHealthBanners
      v-if="dashboard.alerts"
      :is-data-fresh="dashboard.alerts.IsDataFresh"
      :overall-health-status="dashboard.alerts.OverallHealthStatus"
    />

    <Message v-if="visibleError" severity="error" :closable="false">
      {{ visibleError }}
    </Message>

    <AlertCenterPlatformSection
      v-if="dashboard.alerts && !presentation.hidePlatformDiagnostics"
      :alerts="dashboard.alerts.PlatformAlerts"
    />

    <nav class="alert-center__section-nav" aria-label="Alert Center sections">
      <a
        v-for="item in sectionNavItems"
        :key="item.id"
        :href="`#${item.id}`"
        class="alert-center__section-nav-link"
      >
        {{ item.label }}
      </a>
    </nav>

    <AlertCenterCategoryCards
      v-if="dashboard.alerts"
      class="alert-center__section"
      :summaries="dashboard.alerts.CategorySummaries"
      :loading="dashboard.loading"
      @select="onCategorySelect"
    />

    <AlertCenterCriticalStrip
      class="alert-center__section"
      :groups="dashboard.alerts?.AlertGroups ?? []"
      :loading="dashboard.loading"
    />

    <div id="alert-context" class="alert-center__context-row alert-center__section">
      <AlertCenterInventoryRiskSummary
        class="alert-center__context-col"
        :summary="dashboard.alerts?.InventoryRiskSummary ?? null"
      />
      <AlertCenterConcentrationsSection
        class="alert-center__context-col"
        :items="dashboard.alerts?.Concentrations ?? []"
        :loading="dashboard.loading"
      />
    </div>

    <AlertCenterCategoryPanels
      v-model:expanded-category="expandedCategory"
      class="alert-center__section"
      :groups="dashboard.alerts?.AlertGroups ?? []"
      :summaries="dashboard.alerts?.CategorySummaries ?? []"
      :navigation="dashboard.alerts?.Navigation ?? null"
      :loading="dashboard.loading"
    />

    <AlertCenterNavigationSection
      id="alert-dashboards"
      class="alert-center__section"
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

.alert-center__section-nav {
  display: flex;
  flex-wrap: nowrap;
  gap: 0.5rem 1rem;
  overflow-x: auto;
  position: sticky;
  top: 0;
  z-index: 2;
  margin-bottom: 1.5rem;
  padding: 0.75rem 0;
  background: var(--p-content-background);
  border-bottom: 1px solid var(--p-content-border-color);
}

.alert-center__section-nav-link {
  flex-shrink: 0;
  font-size: 0.875rem;
  font-weight: 600;
  color: var(--p-primary-color);
  text-decoration: none;
}

.alert-center__section-nav-link:hover {
  text-decoration: underline;
}

.alert-center__section {
  margin-bottom: 2rem;
  scroll-margin-top: 3.5rem;
}

.alert-center__context-row {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 1.5rem;
  align-items: start;
}

.alert-center__context-col {
  min-width: 0;
}

@media (max-width: 767px) {
  .alert-center__context-row {
    grid-template-columns: 1fr;
  }
}
</style>
