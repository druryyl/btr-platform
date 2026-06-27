<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { RouterLink, useRoute } from 'vue-router'
import Button from 'primevue/button'
import DatePicker from 'primevue/datepicker'
import Message from 'primevue/message'
import Select from 'primevue/select'
import FieldActivityKpiStrip from '@/components/field-activity/FieldActivityKpiStrip.vue'
import FieldActivityMap from '@/components/field-activity/FieldActivityMap.vue'
import FieldActivityMissedList from '@/components/field-activity/FieldActivityMissedList.vue'
import FieldActivityReplayControls from '@/components/field-activity/FieldActivityReplayControls.vue'
import FieldActivityTimeline from '@/components/field-activity/FieldActivityTimeline.vue'
import { getFieldActivity, listFieldActivitySalesmen } from '@/api/fieldActivityApi'
import { getApiErrorMessage } from '@/api/httpClient'
import { useFieldActivityReplay } from '@/composables/useFieldActivityReplay'
import type {
  FieldActivityResponse,
  FieldActivitySalesmanItem,
} from '@/models/fieldActivity'
import { formatPercent } from '@/services/formatters'
import { usePresentationStore } from '@/stores/presentationStore'

type DatePreset = 'today' | 'yesterday' | 'custom'

const presentation = usePresentationStore()
const route = useRoute()
const salesmen = ref<FieldActivitySalesmanItem[]>([])
const selectedSalesPersonId = ref<string | null>(null)
const selectedDate = ref<Date>(startOfDay(new Date()))
const datePreset = ref<DatePreset>('today')
const customDate = ref<Date>(startOfDay(new Date()))
const loadingSalesmen = ref(false)
const loadingData = ref(false)
const loadError = ref<string | null>(null)
const hasLoadedOnce = ref(false)
const fieldActivity = ref<FieldActivityResponse | null>(null)

function businessToday(): Date {
  return startOfDay(presentation.businessReferenceDate)
}

const actualStops = computed(() => fieldActivity.value?.ActualStops ?? [])
const replay = useFieldActivityReplay(actualStops)

const salesmanOptions = computed(() =>
  salesmen.value.map((item) => ({
    label: item.HasEmail
      ? `${item.SalesPersonName} (${item.SalesPersonCode})`
      : `${item.SalesPersonName} (${item.SalesPersonCode}) — no email`,
    value: item.SalesPersonId,
    disabled: false,
    class: item.HasEmail ? undefined : 'field-activity-dashboard__salesman-no-email',
  })),
)

const dataHealthText = computed(() => {
  if (!fieldActivity.value) return null

  const coverage = formatPercent(fieldActivity.value.Kpis.CoordinateCoveragePercent)
  const planNote = fieldActivity.value.Meta.PlanDataAvailable
    ? `Plan data from ${fieldActivity.value.Meta.VisitPlanGoLiveDate}`
    : `No visit plan data before ${fieldActivity.value.Meta.VisitPlanGoLiveDate}`

  return `Coordinate coverage ${coverage} · ${planNote}`
})

const breadcrumbName = computed(
  () => fieldActivity.value?.SalesPersonName ?? 'Salesman Field Activity',
)

const overviewBackQuery = computed(() => ({
  name: 'field-activity-overview' as const,
  query: { visitDate: formatVisitDate(selectedDate.value) },
}))

function parseVisitDate(value: unknown): Date | null {
  if (typeof value !== 'string' || !/^\d{4}-\d{2}-\d{2}$/.test(value)) return null
  const [year, month, day] = value.split('-').map(Number)
  return startOfDay(new Date(year, month - 1, day))
}

function applyRouteQuery(): void {
  const salesPersonId = route.query.salesPersonId
  const visitDate = route.query.visitDate

  if (typeof visitDate === 'string') {
    const parsed = parseVisitDate(visitDate)
    if (parsed) {
      selectedDate.value = parsed
      customDate.value = parsed
      const today = businessToday()
      const yesterday = new Date(today)
      yesterday.setDate(yesterday.getDate() - 1)

      if (parsed.getTime() === today.getTime()) {
        datePreset.value = 'today'
      } else if (parsed.getTime() === yesterday.getTime()) {
        datePreset.value = 'yesterday'
      } else {
        datePreset.value = 'custom'
      }
    }
  }

  if (typeof salesPersonId === 'string' && salesPersonId.trim()) {
    selectedSalesPersonId.value = salesPersonId
  }
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

async function loadSalesmen(): Promise<void> {
  loadingSalesmen.value = true
  try {
    const response = await listFieldActivitySalesmen()
    salesmen.value = response.Items ?? []
  } catch (error) {
    loadError.value = getApiErrorMessage(error, 'Failed to load salesmen.')
  } finally {
    loadingSalesmen.value = false
  }
}

async function loadFieldActivity(): Promise<void> {
  if (!selectedSalesPersonId.value) {
    loadError.value = 'Select a salesman before loading field activity.'
    return
  }

  loadingData.value = true
  loadError.value = null

  try {
    fieldActivity.value = await getFieldActivity(
      selectedSalesPersonId.value,
      formatVisitDate(selectedDate.value),
    )
    hasLoadedOnce.value = true
    replay.reset()
  } catch (error) {
    fieldActivity.value = null
    loadError.value = getApiErrorMessage(error, 'Failed to load field activity.')
  } finally {
    loadingData.value = false
  }
}

function onTimelineSelect(index: number): void {
  replay.selectIndex(index)
}

function onMapStopSelected(index: number): void {
  replay.selectIndex(index)
}

onMounted(async () => {
  await presentation.load()
  customDate.value = businessToday()
  applyDatePreset('today')
  await loadSalesmen()
  applyRouteQuery()

  if (route.query.salesPersonId && route.query.visitDate) {
    await loadFieldActivity()
  }
})

watch(
  () => [route.query.salesPersonId, route.query.visitDate],
  async () => {
    applyRouteQuery()
    if (route.query.salesPersonId && route.query.visitDate) {
      await loadFieldActivity()
    }
  },
)
</script>

<template>
  <div class="field-activity-dashboard">
    <header class="field-activity-dashboard__header">
      <div>
        <nav class="field-activity-dashboard__breadcrumb" aria-label="Breadcrumb">
          <RouterLink :to="overviewBackQuery">Sales Force Overview</RouterLink>
          <span aria-hidden="true">→</span>
          <span>{{ breadcrumbName }}</span>
        </nav>
        <h1>Salesman Field Activity</h1>
        <p>Route execution, GPS check-in visibility, and daily visit replay.</p>
      </div>
    </header>

    <section class="field-activity-dashboard__toolbar">
      <Select
        v-model="selectedSalesPersonId"
        :options="salesmanOptions"
        option-label="label"
        option-value="value"
        placeholder="Select salesman"
        filter
        class="field-activity-dashboard__salesman-select"
        :loading="loadingSalesmen"
      />

      <div class="field-activity-dashboard__date-buttons">
        <Button
          label="Today"
          :outlined="datePreset !== 'today'"
          size="small"
          @click="applyDatePreset('today')"
        />
        <Button
          label="Yesterday"
          :outlined="datePreset !== 'yesterday'"
          size="small"
          @click="applyDatePreset('yesterday')"
        />
        <Button
          label="Custom Date"
          :outlined="datePreset !== 'custom'"
          size="small"
          @click="applyDatePreset('custom')"
        />
      </div>

      <DatePicker
        v-if="datePreset === 'custom'"
        v-model="customDate"
        date-format="dd M yy"
        show-icon
        class="field-activity-dashboard__date-picker"
      />

      <Button
        label="Load"
        icon="pi pi-search"
        :loading="loadingData"
        @click="loadFieldActivity"
      />
    </section>

    <p
      v-if="dataHealthText && !presentation.hidePlatformDiagnostics"
      class="field-activity-dashboard__health"
    >
      {{ dataHealthText }}
    </p>

    <Message v-if="loadError" severity="error" :closable="false">
      {{ loadError }}
    </Message>

    <div class="field-activity-dashboard__tower">
      <aside class="field-activity-dashboard__left">
        <FieldActivityKpiStrip
          :kpis="fieldActivity?.Kpis ?? null"
          :loading="loadingData"
        />
        <FieldActivityMissedList :items="fieldActivity?.MissedVisits ?? []" />
        <div class="field-activity-dashboard__cross-link">
          View sales performance →
          <RouterLink :to="{ name: 'salesmen-dashboard' }">Salesmen dashboard</RouterLink>
        </div>
      </aside>

      <main class="field-activity-dashboard__center">
        <FieldActivityMap
          :data="fieldActivity"
          :selected-index="replay.currentIndex.value"
          :loading="loadingData"
          @stop-selected="onMapStopSelected"
        />
      </main>

      <aside class="field-activity-dashboard__right">
        <FieldActivityTimeline
          :stops="actualStops"
          :selected-index="replay.currentIndex.value"
          @select="onTimelineSelect"
        />
        <FieldActivityReplayControls
          :is-playing="replay.isPlaying.value"
          :speed="replay.speed.value"
          :min-speed="replay.minSpeed"
          :max-speed="replay.maxSpeed"
          :disabled="!hasLoadedOnce || actualStops.length === 0"
          @play="replay.play"
          @pause="replay.pause"
          @reset="replay.reset"
          @update:speed="replay.setSpeed"
        />
      </aside>
    </div>
  </div>
</template>

<style scoped>
.field-activity-dashboard {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.field-activity-dashboard__header h1 {
  margin: 0;
  font-size: 1.75rem;
}

.field-activity-dashboard__header p {
  margin: 0.375rem 0 0;
  color: var(--p-text-muted-color);
}

.field-activity-dashboard__breadcrumb {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 0.375rem;
  margin-bottom: 0.375rem;
  font-size: 0.875rem;
  color: var(--p-text-muted-color);
}

.field-activity-dashboard__breadcrumb a {
  color: var(--p-primary-color);
  text-decoration: none;
  font-weight: 600;
}

.field-activity-dashboard__toolbar {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 0.75rem;
}

.field-activity-dashboard__salesman-select {
  min-width: 16rem;
}

.field-activity-dashboard__date-buttons {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.field-activity-dashboard__date-picker {
  min-width: 10rem;
}

.field-activity-dashboard__health {
  margin: 0;
  font-size: 0.875rem;
  color: var(--p-text-muted-color);
}

.field-activity-dashboard__tower {
  display: grid;
  grid-template-columns: minmax(14rem, 20%) minmax(0, 1fr) minmax(14rem, 22%);
  gap: 1rem;
  min-height: 32rem;
}

.field-activity-dashboard__left,
.field-activity-dashboard__right {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  min-width: 0;
}

.field-activity-dashboard__center {
  min-width: 0;
  min-height: 24rem;
}

.field-activity-dashboard__cross-link {
  margin-top: auto;
  font-size: 0.875rem;
  color: var(--p-text-muted-color);
}

.field-activity-dashboard__cross-link a {
  color: var(--p-primary-color);
  text-decoration: none;
  font-weight: 600;
}

@media (max-width: 1100px) {
  .field-activity-dashboard__tower {
    grid-template-columns: 1fr;
  }

  .field-activity-dashboard__center {
    order: -1;
    min-height: 28rem;
  }
}
</style>
