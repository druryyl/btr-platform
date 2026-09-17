<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import Button from 'primevue/button'
import DatePicker from 'primevue/datepicker'
import Message from 'primevue/message'
import SelectButton from 'primevue/selectbutton'
import ExecutionFunnel from '@/components/field-activity/ExecutionFunnel.vue'
import FieldActivityCollectionHealthSection from '@/components/field-activity/FieldActivityCollectionHealthSection.vue'
import FieldActivityGroupedActionCenter, {
  type CommercialRiskClickPayload,
} from '@/components/field-activity/FieldActivityGroupedActionCenter.vue'
import FieldActivitySalesmanTable from '@/components/field-activity/FieldActivitySalesmanTable.vue'
import FieldActivityTeamKpiStrip from '@/components/field-activity/FieldActivityTeamKpiStrip.vue'
import FieldActivityTeamTrendChart from '@/components/field-activity/FieldActivityTeamTrendChart.vue'
import FieldActivityWilayahChart from '@/components/field-activity/FieldActivityWilayahChart.vue'
import RevenueConcentrationStrip from '@/components/field-activity/RevenueConcentrationStrip.vue'
import { getFieldActivityOverview } from '@/api/fieldActivityApi'
import { getApiErrorMessage } from '@/api/httpClient'
import type { FieldActivityOverviewResponse } from '@/models/fieldActivity'
import { navigateToDashboard, navigateToInvestigation } from '@/services/navigateToInvestigation'
import { useDashboardStore } from '@/stores/dashboardStore'
import { usePresentationStore } from '@/stores/presentationStore'

type DatePreset = 'today' | 'yesterday' | 'custom'

const INVESTIGATION_SOURCE_LABEL = 'Sales Force Overview'

const router = useRouter()
const presentation = usePresentationStore()
const dashboard = useDashboardStore()

const overview = ref<FieldActivityOverviewResponse | null>(null)
const loading = ref(false)
const loadError = ref<string | null>(null)
const selectedDate = ref<Date>(startOfDay(new Date()))
const datePreset = ref<DatePreset>('today')
const customDate = ref<Date>(startOfDay(new Date()))

const datePresetOptions = [
  { label: 'Today', value: 'today' },
  { label: 'Yesterday', value: 'yesterday' },
  { label: 'Custom', value: 'custom' },
]

function businessToday(): Date {
  return startOfDay(presentation.businessReferenceDate)
}

function startOfDay(date: Date): Date {
  const copy = new Date(date)
  copy.setHours(0, 0, 0, 0)
  return copy
}

function formatVisitDate(date: Date): string {
  const year = date.getFullYear()
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const day = String(date.getDate()).padStart(2, '0')
  return `${year}-${month}-${day}`
}

function applyDatePreset(preset: DatePreset): void {
  datePreset.value = preset
  const today = businessToday()

  if (preset === 'today') {
    selectedDate.value = today
    return
  }

  if (preset === 'yesterday') {
    const yesterday = new Date(today)
    yesterday.setDate(yesterday.getDate() - 1)
    selectedDate.value = yesterday
    return
  }

  selectedDate.value = startOfDay(customDate.value)
}

watch(customDate, () => {
  if (datePreset.value === 'custom') {
    selectedDate.value = startOfDay(customDate.value)
  }
})

watch(selectedDate, () => {
  void loadOverview()
})

const freshnessText = computed(() => {
  if (!overview.value) return null
  if (overview.value.DataSource === 'Snapshot' && overview.value.GeneratedAt) {
    return `Snapshot refreshed ${new Date(overview.value.GeneratedAt).toLocaleString()}`
  }
  if (overview.value.QueriedAt) {
    return `Live query at ${new Date(overview.value.QueriedAt).toLocaleString()}`
  }
  return null
})

const planBanner = computed(() => {
  if (!overview.value?.Meta) return null
  if (overview.value.Meta.PlanDataAvailable) {
    return `Visit plan data available from ${overview.value.Meta.VisitPlanGoLiveDate}.`
  }
  return `No visit plan data before ${overview.value.Meta.VisitPlanGoLiveDate}. Planned KPIs show zero for earlier dates.`
})

const piutangDashboardRoute = computed(
  () => dashboard.collection?.Navigation?.PiutangDashboardRoute ?? null,
)

async function loadOverview(): Promise<void> {
  loading.value = true
  loadError.value = null

  try {
    overview.value = await getFieldActivityOverview(formatVisitDate(selectedDate.value))
  } catch (error) {
    loadError.value = getApiErrorMessage(error, 'Failed to load sales force overview.')
  } finally {
    loading.value = false
  }
}

function refresh(): void {
  void loadOverview()
  void dashboard.loadCollection()
}

function navigateToDetail(salesPersonId: string): void {
  void router.push({
    name: 'field-activity-detail',
    query: {
      salesPersonId,
      visitDate: formatVisitDate(selectedDate.value),
    },
  })
}

function onCollectionAlertClick(): void {
  if (piutangDashboardRoute.value) {
    navigateToDashboard(router, piutangDashboardRoute.value)
  }
}

function onCommercialRiskClick(payload: CommercialRiskClickPayload): void {
  if (payload.investigation) {
    navigateToInvestigation(router, payload.investigation, INVESTIGATION_SOURCE_LABEL)
    return
  }

  if (piutangDashboardRoute.value) {
    navigateToDashboard(router, piutangDashboardRoute.value)
  }
}

onMounted(() => {
  applyDatePreset('today')
  void dashboard.loadCollection()
})
</script>

<template>
  <div class="field-activity-overview portal-page">
    <header class="field-activity-overview__header portal-page__header">
      <div>
        <h1>Sales Force Overview</h1>
        <p class="field-activity-overview__subtitle">
          Compare field execution across the sales organization for
          {{ formatVisitDate(selectedDate) }}.
        </p>
        <p v-if="freshnessText" class="field-activity-overview__freshness">{{ freshnessText }}</p>
      </div>

      <div class="field-activity-overview__toolbar">
        <SelectButton
          :model-value="datePreset"
          :options="datePresetOptions"
          option-label="label"
          option-value="value"
          @update:model-value="(value: DatePreset) => applyDatePreset(value)"
        />
        <DatePicker
          v-if="datePreset === 'custom'"
          v-model="customDate"
          date-format="yy-mm-dd"
          show-icon
        />
        <Button
          icon="pi pi-refresh"
          label="Refresh"
          severity="secondary"
          :loading="loading"
          @click="refresh"
        />
      </div>
    </header>

    <Message v-if="loadError" severity="error" :closable="false">{{ loadError }}</Message>
    <Message v-else-if="planBanner" severity="info" :closable="false">{{ planBanner }}</Message>

    <!-- A. Status — sales KPIs + dedicated Collection Health (MTD) section (GAP-005) -->
    <section class="field-activity-overview__section" aria-label="Status">
      <FieldActivityTeamKpiStrip :kpis="overview?.TeamKpis ?? null" :loading="loading" />

      <FieldActivityCollectionHealthSection
        :collection="dashboard.collection"
        :business-date="presentation.businessReferenceDate"
        :loading="dashboard.loading"
        @alert-click="onCollectionAlertClick"
      />
    </section>

    <!-- B. Performance — execution funnel + salesman scoreboard + quality -->
    <section class="field-activity-overview__section" aria-label="Performance">
      <ExecutionFunnel :kpis="overview?.TeamKpis ?? null" :loading="loading" />

      <FieldActivitySalesmanTable
        :rows="overview?.Salesmen ?? []"
        :collection="dashboard.collection"
        :loading="loading"
        @row-click="(row) => navigateToDetail(row.SalesPersonId)"
      />
    </section>

    <!-- D. Action Center — grouped, action-first interventions -->
    <section class="field-activity-overview__section" aria-label="Action Center">
      <FieldActivityGroupedActionCenter
        :salesmen="overview?.Salesmen ?? []"
        :rankings="overview?.Rankings ?? null"
        :collection="dashboard.collection"
        :loading="loading"
        @salesman-click="navigateToDetail"
        @commercial-risk-click="onCommercialRiskClick"
        @recognition-click="navigateToDetail"
      />
    </section>

    <!-- C. Outcomes — revenue distribution + territory & financial health -->
    <section class="field-activity-overview__section" aria-label="Outcomes">
      <RevenueConcentrationStrip :salesmen="overview?.Salesmen ?? []" :loading="loading" />

      <FieldActivityWilayahChart
        :items="overview?.WilayahBreakdown ?? []"
        :salesman-rows="overview?.Salesmen ?? []"
        :top-overdue-wilayah="dashboard.collection?.TopOverdueWilayah ?? []"
        :collection-available="dashboard.collection?.IsAvailable === true"
        :business-date="presentation.businessReferenceDate"
        :loading="loading"
      />
    </section>

    <!-- E. Trends — sales trends only (collection trends deferred, GAP-001) -->
    <section class="field-activity-overview__section" aria-label="Trends">
      <FieldActivityTeamTrendChart
        :last7-days="overview?.Trends.Last7Days ?? []"
        :last30-days="overview?.Trends.Last30Days ?? []"
        :loading="loading"
      />
    </section>
  </div>
</template>

<style scoped>
.field-activity-overview {
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
}

.field-activity-overview__header {
  display: flex;
  flex-wrap: wrap;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
}

.field-activity-overview__subtitle,
.field-activity-overview__freshness {
  margin: 0.25rem 0 0;
  color: var(--p-text-muted-color, #64748b);
}

.field-activity-overview__toolbar {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 0.75rem;
}

.field-activity-overview__section {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}
</style>
