<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import Button from 'primevue/button'
import DatePicker from 'primevue/datepicker'
import Message from 'primevue/message'
import SelectButton from 'primevue/selectbutton'
import FieldActivityComparisonChart, {
  type FieldActivityComparisonItem,
} from '@/components/field-activity/FieldActivityComparisonChart.vue'
import FieldActivityRankingGrid from '@/components/field-activity/FieldActivityRankingGrid.vue'
import FieldActivitySalesmanTable from '@/components/field-activity/FieldActivitySalesmanTable.vue'
import FieldActivityTeamKpiStrip from '@/components/field-activity/FieldActivityTeamKpiStrip.vue'
import FieldActivityTeamTrendChart from '@/components/field-activity/FieldActivityTeamTrendChart.vue'
import FieldActivityWilayahChart from '@/components/field-activity/FieldActivityWilayahChart.vue'
import { getFieldActivityOverview } from '@/api/fieldActivityApi'
import { getApiErrorMessage } from '@/api/httpClient'
import type {
  FieldActivityOverviewResponse,
  FieldActivityRankingEntry,
  FieldActivitySalesmanOverviewRow,
} from '@/models/fieldActivity'
import { usePresentationStore } from '@/stores/presentationStore'

type DatePreset = 'today' | 'yesterday' | 'custom'

const router = useRouter()
const presentation = usePresentationStore()

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

function chartLabel(row: FieldActivitySalesmanOverviewRow): string {
  return `${row.SalesPersonCode} · ${row.SalesPersonName}`
}

function toComparisonItems(
  rows: FieldActivitySalesmanOverviewRow[],
  valueSelector: (row: FieldActivitySalesmanOverviewRow) => number,
): FieldActivityComparisonItem[] {
  return rows
    .filter((row) => row.HasEmail)
    .map((row) => ({
      label: chartLabel(row),
      value: valueSelector(row),
      salesPersonId: row.SalesPersonId,
    }))
    .sort((a, b) => b.value - a.value)
}

const executionChartItems = computed(() =>
  toComparisonItems(overview.value?.Salesmen ?? [], (row) => row.VisitExecutionPercent ?? 0),
)

const effectiveChartItems = computed(() =>
  toComparisonItems(overview.value?.Salesmen ?? [], (row) => row.EffectiveCallRate ?? 0),
)

const ordersChartItems = computed(() =>
  toComparisonItems(overview.value?.Salesmen ?? [], (row) => row.OrdersCount),
)

const omzetChartItems = computed(() =>
  toComparisonItems(overview.value?.Salesmen ?? [], (row) => Number(row.OmzetAmount)),
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

function navigateToDetail(salesPersonId: string): void {
  void router.push({
    name: 'field-activity-detail',
    query: {
      salesPersonId,
      visitDate: formatVisitDate(selectedDate.value),
    },
  })
}

function onRankingClick(entry: FieldActivityRankingEntry): void {
  navigateToDetail(entry.SalesPersonId)
}

onMounted(() => {
  applyDatePreset('today')
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
          @click="loadOverview"
        />
      </div>
    </header>

    <Message v-if="loadError" severity="error" :closable="false">{{ loadError }}</Message>
    <Message v-else-if="planBanner" severity="info" :closable="false">{{ planBanner }}</Message>

    <FieldActivityTeamKpiStrip :kpis="overview?.TeamKpis ?? null" :loading="loading" />

    <FieldActivitySalesmanTable
      :rows="overview?.Salesmen ?? []"
      :loading="loading"
      @row-click="(row) => navigateToDetail(row.SalesPersonId)"
    />

    <section class="field-activity-overview__charts">
      <FieldActivityComparisonChart
        title="Visit Execution %"
        :items="executionChartItems"
        :loading="loading"
        value-kind="percent"
        @bar-click="navigateToDetail"
      />
      <FieldActivityComparisonChart
        title="Effective Call Rate"
        :items="effectiveChartItems"
        :loading="loading"
        value-kind="percent"
        @bar-click="navigateToDetail"
      />
      <FieldActivityComparisonChart
        title="Orders Generated"
        :items="ordersChartItems"
        :loading="loading"
        value-kind="number"
        @bar-click="navigateToDetail"
      />
      <FieldActivityComparisonChart
        title="Order Value"
        :items="omzetChartItems"
        :loading="loading"
        value-kind="currency"
        @bar-click="navigateToDetail"
      />
    </section>

    <FieldActivityRankingGrid
      :rankings="overview?.Rankings ?? null"
      :loading="loading"
      @row-click="onRankingClick"
    />

    <FieldActivityTeamTrendChart
      :last7-days="overview?.Trends.Last7Days ?? []"
      :last30-days="overview?.Trends.Last30Days ?? []"
      :loading="loading"
    />

    <FieldActivityWilayahChart
      :items="overview?.WilayahBreakdown ?? []"
      :loading="loading"
    />
  </div>
</template>

<style scoped>
.field-activity-overview {
  display: flex;
  flex-direction: column;
  gap: 1rem;
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

.field-activity-overview__charts {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(20rem, 1fr));
  gap: 0.75rem;
}
</style>
