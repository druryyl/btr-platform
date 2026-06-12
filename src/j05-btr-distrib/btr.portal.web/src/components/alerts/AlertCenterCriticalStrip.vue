<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import Card from 'primevue/card'
import Tag from 'primevue/tag'
import Button from 'primevue/button'
import ProgressSpinner from 'primevue/progressspinner'
import type { DashboardAlertCenterCategoryGroup } from '@/models/dashboard'
import {
  canInvestigateAlert,
  canViewDashboardAlert,
  formatAlertValue,
} from '@/services/alertCenterAlertActions'
import { navigateToDashboard, navigateToInvestigation } from '@/services/navigateToInvestigation'
import {
  CRITICAL_ALERTS_LIMIT,
  getTopCriticalAlerts,
} from '@/services/alertCenterPrioritization'

const props = defineProps<{
  groups: DashboardAlertCenterCategoryGroup[]
  loading: boolean
}>()

const router = useRouter()
const sourceLabel = 'Alert Center'

const criticalAlerts = computed(() =>
  getTopCriticalAlerts(props.groups, CRITICAL_ALERTS_LIMIT),
)

const totalAlertCount = computed(() =>
  props.groups.reduce((count, group) => count + group.Alerts.length, 0),
)

function bandSeverity(band: string | null): 'danger' | 'warn' | 'secondary' {
  if (band === 'Critical') return 'danger'
  if (band === 'Warning') return 'warn'
  return 'secondary'
}

function investigate(row: (typeof criticalAlerts.value)[number]): void {
  if (!row.Investigation || !canInvestigateAlert(row)) return
  navigateToInvestigation(router, row.Investigation, sourceLabel)
}

function openDashboard(row: (typeof criticalAlerts.value)[number]): void {
  if (!row.DashboardRoute) return
  navigateToDashboard(router, row.DashboardRoute)
}
</script>

<template>
  <section id="alert-critical" class="alert-center-critical">
    <div class="alert-center-critical__header">
      <h2 class="alert-center-critical__heading">
        Top Critical Alerts
        <span v-if="!loading && criticalAlerts.length > 0" class="alert-center-critical__count">
          ({{ criticalAlerts.length }})
        </span>
      </h2>
      <a
        v-if="!loading && totalAlertCount > criticalAlerts.length"
        href="#alert-categories"
        class="alert-center-critical__view-all"
      >
        View all alerts ↓
      </a>
    </div>

    <Card>
      <template #content>
        <div v-if="loading" class="alert-center-critical__loading">
          <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
        </div>

        <p v-else-if="criticalAlerts.length === 0" class="alert-center-critical__empty">
          No exception alerts require attention right now.
        </p>

        <template v-else>
          <div class="alert-center-critical__table" role="table" aria-label="Top critical alerts">
            <div class="alert-center-critical__row alert-center-critical__row--head" role="row">
              <span role="columnheader">Category</span>
              <span role="columnheader">Entity</span>
              <span role="columnheader">Signal</span>
              <span role="columnheader">Value</span>
              <span role="columnheader">Actions</span>
            </div>
            <div
              v-for="(row, index) in criticalAlerts"
              :key="`${row.Category}-${row.EntityCode ?? row.EntityName}-${row.SignalKey}-${index}`"
              class="alert-center-critical__row"
              role="row"
            >
              <span class="alert-center-critical__cell" role="cell">
                <Tag
                  v-if="row.AchievementBand"
                  :value="row.AchievementBand"
                  :severity="bandSeverity(row.AchievementBand)"
                  class="alert-center-critical__band"
                />
                <Tag :value="row.Category" severity="secondary" />
              </span>
              <span class="alert-center-critical__cell alert-center-critical__entity" role="cell">
                {{ row.EntityName }}
              </span>
              <span class="alert-center-critical__cell" role="cell">
                {{ row.SignalLabel }}
              </span>
              <span class="alert-center-critical__cell alert-center-critical__value" role="cell">
                {{ formatAlertValue(row) }}
              </span>
              <span class="alert-center-critical__cell alert-center-critical__actions" role="cell">
                <Button
                  v-if="canInvestigateAlert(row)"
                  label="Investigate"
                  text
                  size="small"
                  @click="investigate(row)"
                />
                <Button
                  v-if="canViewDashboardAlert(row)"
                  label="View Dashboard"
                  text
                  size="small"
                  severity="secondary"
                  @click="openDashboard(row)"
                />
              </span>
            </div>
          </div>

          <ul class="alert-center-critical__cards" aria-label="Top critical alerts">
            <li
              v-for="(row, index) in criticalAlerts"
              :key="`mobile-${row.Category}-${row.EntityCode ?? row.EntityName}-${row.SignalKey}-${index}`"
              class="alert-center-critical__card-item"
            >
              <div class="alert-center-critical__card-tags">
                <Tag
                  v-if="row.AchievementBand"
                  :value="row.AchievementBand"
                  :severity="bandSeverity(row.AchievementBand)"
                />
                <Tag :value="row.Category" severity="secondary" />
              </div>
              <div class="alert-center-critical__card-entity">{{ row.EntityName }}</div>
              <div class="alert-center-critical__card-signal">{{ row.SignalLabel }}</div>
              <div class="alert-center-critical__card-value">{{ formatAlertValue(row) }}</div>
              <div class="alert-center-critical__card-actions">
                <Button
                  v-if="canInvestigateAlert(row)"
                  label="Investigate"
                  text
                  size="small"
                  @click="investigate(row)"
                />
                <Button
                  v-if="canViewDashboardAlert(row)"
                  label="View Dashboard"
                  text
                  size="small"
                  severity="secondary"
                  @click="openDashboard(row)"
                />
              </div>
            </li>
          </ul>
        </template>
      </template>
    </Card>
  </section>
</template>

<style scoped>
.alert-center-critical__header {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;
  gap: 0.5rem;
  margin-bottom: 0.75rem;
}

.alert-center-critical__heading {
  margin: 0;
  font-size: 1.125rem;
}

.alert-center-critical__count {
  font-weight: 500;
  color: var(--p-text-muted-color);
}

.alert-center-critical__view-all {
  font-size: 0.875rem;
  font-weight: 600;
  color: var(--p-primary-color);
  text-decoration: none;
}

.alert-center-critical__view-all:hover {
  text-decoration: underline;
}

.alert-center-critical__loading,
.alert-center-critical__empty {
  margin: 0;
  padding: 1.5rem 0;
  text-align: center;
  color: var(--p-text-muted-color);
}

.alert-center-critical__table {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.alert-center-critical__row {
  display: grid;
  grid-template-columns: minmax(7rem, 1.1fr) minmax(8rem, 1.4fr) minmax(8rem, 1.4fr) minmax(5rem, 1fr) minmax(9rem, 1.2fr);
  gap: 0.75rem;
  align-items: center;
  padding: 0.5rem 0;
  border-bottom: 1px solid var(--p-content-border-color);
}

.alert-center-critical__row:last-child {
  border-bottom: none;
}

.alert-center-critical__row--head {
  font-size: 0.75rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.03em;
  color: var(--p-text-muted-color);
  border-bottom-width: 2px;
}

.alert-center-critical__cell {
  min-width: 0;
}

.alert-center-critical__entity {
  font-weight: 600;
}

.alert-center-critical__value {
  font-weight: 600;
}

.alert-center-critical__actions {
  display: flex;
  flex-wrap: wrap;
  gap: 0.25rem;
}

.alert-center-critical__band {
  margin-right: 0.25rem;
}

.alert-center-critical__cards {
  display: none;
  list-style: none;
  margin: 0;
  padding: 0;
  gap: 0.75rem;
}

.alert-center-critical__card-item {
  padding: 0.75rem;
  border: 1px solid var(--p-content-border-color);
  border-radius: var(--p-content-border-radius);
}

.alert-center-critical__card-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 0.35rem;
  margin-bottom: 0.5rem;
}

.alert-center-critical__card-entity {
  font-weight: 700;
  margin-bottom: 0.25rem;
}

.alert-center-critical__card-signal {
  font-size: 0.875rem;
  color: var(--p-text-muted-color);
  margin-bottom: 0.25rem;
}

.alert-center-critical__card-value {
  font-weight: 600;
  margin-bottom: 0.5rem;
}

.alert-center-critical__card-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 0.25rem;
}

@media (max-width: 767px) {
  .alert-center-critical__table {
    display: none;
  }

  .alert-center-critical__cards {
    display: flex;
    flex-direction: column;
  }

  .alert-center-critical__card-actions :deep(.p-button) {
    min-height: 2.75rem;
  }
}
</style>
