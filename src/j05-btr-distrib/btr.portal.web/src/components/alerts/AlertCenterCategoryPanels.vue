<script setup lang="ts">
import { computed, watch } from 'vue'
import { useRouter } from 'vue-router'
import Panel from 'primevue/panel'
import Button from 'primevue/button'
import AlertCenterCategoryTable from '@/components/alerts/AlertCenterCategoryTable.vue'
import type {
  DashboardAlertCenterCategoryGroup,
  DashboardAlertCenterCategorySummary,
  DashboardAlertCenterNavigationLinks,
} from '@/models/dashboard'
import {
  ALERT_CENTER_CATEGORY_ORDER,
  categoryPanelId,
  getCategoryDashboardRoute,
} from '@/services/alertCenterPrioritization'

const props = defineProps<{
  groups: DashboardAlertCenterCategoryGroup[]
  summaries: DashboardAlertCenterCategorySummary[]
  navigation: DashboardAlertCenterNavigationLinks | null
  loading: boolean
  expandedCategory: string | null
}>()

const emit = defineEmits<{
  'update:expandedCategory': [category: string | null]
}>()

const router = useRouter()

const summaryByCategory = computed(() => {
  const map = new Map<string, DashboardAlertCenterCategorySummary>()
  for (const summary of props.summaries) {
    map.set(summary.Category, summary)
  }
  return map
})

const alertsByCategory = computed(() => {
  const map = new Map<string, DashboardAlertCenterCategoryGroup['Alerts']>()
  for (const group of props.groups) {
    map.set(group.Category, group.Alerts)
  }
  return map
})

const hasAnyAlerts = computed(() =>
  props.groups.some((group) => group.Alerts.length > 0),
)

const panelCategories = computed(() =>
  ALERT_CENTER_CATEGORY_ORDER.map((category) => ({
    category,
    summary: summaryByCategory.value.get(category),
    alerts: alertsByCategory.value.get(category) ?? [],
    panelId: categoryPanelId(category),
    dashboardRoute: getCategoryDashboardRoute(category, props.navigation),
  })),
)

function isExpanded(category: string): boolean {
  return props.expandedCategory === category
}

function setExpanded(category: string, expanded: boolean): void {
  emit('update:expandedCategory', expanded ? category : null)
}

function panelHeader(category: string, totalCount: number, displayedCount: number, hasMore: boolean): string {
  if (totalCount === 0) {
    return `${category} (0)`
  }

  if (hasMore) {
    return `${category} (${totalCount} · showing ${displayedCount})`
  }

  return `${category} (${totalCount})`
}

function panelCaption(hasMore: boolean): string | null {
  if (!hasMore) return null
  return 'Top 20 shown — open domain dashboard for the full list'
}

function openCategoryDashboard(route: string | null): void {
  if (!route) return
  void router.push(route)
}

watch(
  () => props.expandedCategory,
  (category) => {
    if (!category) return
    const element = document.getElementById(categoryPanelId(category))
    element?.scrollIntoView({ behavior: 'smooth', block: 'nearest' })
  },
)
</script>

<template>
  <section id="alert-categories" class="alert-center-panels">
    <h2 class="alert-center-panels__heading">Alerts by Category</h2>

    <div v-if="loading" class="alert-center-panels__loading">
      Loading alerts…
    </div>

    <template v-else>
      <Panel
        v-for="panel in panelCategories"
        :key="panel.category"
        :id="panel.panelId"
        toggleable
        :collapsed="!isExpanded(panel.category)"
        class="alert-center-panels__panel"
        @update:collapsed="setExpanded(panel.category, !$event)"
      >
        <template #header>
          <div class="alert-center-panels__header">
            <span class="alert-center-panels__title">
              {{ panelHeader(
                panel.category,
                panel.summary?.TotalCount ?? panel.alerts.length,
                panel.summary?.DisplayedCount ?? panel.alerts.length,
                panel.summary?.HasMore ?? false,
              ) }}
            </span>
            <span
              v-if="panelCaption(panel.summary?.HasMore ?? false)"
              class="alert-center-panels__caption"
            >
              {{ panelCaption(panel.summary?.HasMore ?? false) }}
            </span>
          </div>
        </template>

        <template #icons>
          <Button
            v-if="panel.dashboardRoute"
            label="View Dashboard"
            text
            size="small"
            severity="secondary"
            class="alert-center-panels__dashboard-btn"
            @click.stop="openCategoryDashboard(panel.dashboardRoute)"
          />
        </template>

        <AlertCenterCategoryTable :alerts="panel.alerts" />
      </Panel>

      <p v-if="!hasAnyAlerts" class="alert-center-panels__empty">
        No exception alerts require attention right now.
      </p>
    </template>
  </section>
</template>

<style scoped>
.alert-center-panels__heading {
  margin: 0 0 0.75rem;
  font-size: 1.125rem;
}

.alert-center-panels__panel {
  margin-bottom: 0.75rem;
  scroll-margin-top: 3.5rem;
}

.alert-center-panels__header {
  display: flex;
  flex-direction: column;
  gap: 0.15rem;
}

.alert-center-panels__title {
  font-weight: 600;
}

.alert-center-panels__caption {
  font-size: 0.8125rem;
  font-weight: 400;
  color: var(--p-text-muted-color);
}

.alert-center-panels__dashboard-btn {
  margin-right: 0.25rem;
}

.alert-center-panels__loading,
.alert-center-panels__empty {
  margin: 0;
  padding: 1.5rem 0;
  text-align: center;
  color: var(--p-text-muted-color);
}

@media (max-width: 767px) {
  .alert-center-panels__panel :deep(.p-panel-header) {
    min-height: 2.75rem;
  }
}
</style>
