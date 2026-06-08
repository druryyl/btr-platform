<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import Button from 'primevue/button'
import DatePicker from 'primevue/datepicker'
import IconField from 'primevue/iconfield'
import InputIcon from 'primevue/inputicon'
import InputText from 'primevue/inputtext'
import Message from 'primevue/message'
import SelectButton from 'primevue/selectbutton'
import {
  parseDateParam,
  type PiutangDateField,
  validateDateRange,
} from '@/services/reportFilterDefaults'

const props = withDefaults(
  defineProps<{
    from?: string
    to?: string
    freeText: string
    loading?: boolean
    showDateRange?: boolean
    showDateField?: boolean
    dateField?: PiutangDateField
  }>(),
  {
    from: '',
    to: '',
    loading: false,
    showDateRange: true,
    showDateField: false,
    dateField: 'DueDate',
  },
)

const emit = defineEmits<{
  apply: []
  'update:from': [value: string]
  'update:to': [value: string]
  'update:freeText': [value: string]
  'update:dateField': [value: PiutangDateField]
}>()

const dateFieldOptions = [
  { label: 'Jatuh Tempo', value: 'DueDate' as PiutangDateField },
  { label: 'Piutang Date', value: 'PiutangDate' as PiutangDateField },
]

const localDateRange = ref<[Date, Date] | null>(
  props.showDateRange && props.from && props.to
    ? [parseDateParam(props.from), parseDateParam(props.to)]
    : null,
)

const periodError = ref<string | null>(null)

const localDateField = computed({
  get: () => props.dateField,
  set: (value: PiutangDateField) => emit('update:dateField', value),
})

const localFreeText = computed({
  get: () => props.freeText,
  set: (value: string) => emit('update:freeText', value),
})

watch(
  () => [props.from, props.to, props.showDateRange] as const,
  ([from, to, showDateRange]) => {
    if (!showDateRange || !from || !to) {
      return
    }

    localDateRange.value = [parseDateParam(from), parseDateParam(to)]
  },
)

function applyFilters(): void {
  if (!props.showDateRange) {
    emit('apply')
    return
  }

  const range = localDateRange.value
  if (!range || !range[0] || !range[1]) {
    periodError.value = 'Select a start and end date.'
    return
  }

  const error = validateDateRange(range[0], range[1])
  periodError.value = error
  if (error) {
    return
  }

  const format = (date: Date) => {
    const year = date.getFullYear()
    const month = String(date.getMonth() + 1).padStart(2, '0')
    const day = String(date.getDate()).padStart(2, '0')
    return `${year}-${month}-${day}`
  }

  emit('update:from', format(range[0]))
  emit('update:to', format(range[1]))
  emit('apply')
}

function clearFreeText(): void {
  emit('update:freeText', '')
}
</script>

<template>
  <div class="report-filter-bar">
    <div class="report-filter-bar__row">
      <div v-if="showDateRange" class="report-filter-bar__period">
        <label class="report-filter-bar__label" for="report-period-range">Period</label>
        <DatePicker
          id="report-period-range"
          v-model="localDateRange"
          selection-mode="range"
          date-format="dd M yy"
          show-icon
          :manual-input="false"
          class="report-filter-bar__datepicker"
        />
      </div>

      <div v-if="showDateField" class="report-filter-bar__date-field">
        <label class="report-filter-bar__label">Filter by</label>
        <SelectButton
          v-model="localDateField"
          :options="dateFieldOptions"
          option-label="label"
          option-value="value"
          :allow-empty="false"
        />
      </div>

      <div class="report-filter-bar__search">
        <label class="report-filter-bar__label" for="report-free-text">Search</label>
        <div class="report-filter-bar__search-controls">
          <IconField class="report-filter-bar__search-field">
            <InputIcon class="pi pi-search" />
            <InputText
              id="report-free-text"
              v-model="localFreeText"
              placeholder="Filter rows..."
              class="report-filter-bar__search-input"
            />
          </IconField>
          <Button
            v-if="localFreeText"
            icon="pi pi-times"
            severity="secondary"
            text
            rounded
            aria-label="Clear search"
            @click="clearFreeText"
          />
        </div>
      </div>

      <Button
        v-if="showDateRange"
        label="Apply"
        icon="pi pi-filter"
        :loading="loading"
        :disabled="loading"
        @click="applyFilters"
      />
    </div>

    <Message v-if="periodError" severity="warn" :closable="false" class="report-filter-bar__error">
      {{ periodError }}
    </Message>
  </div>
</template>

<style scoped>
.report-filter-bar {
  margin-bottom: 1rem;
}

.report-filter-bar__row {
  display: flex;
  flex-wrap: wrap;
  align-items: flex-end;
  gap: 1rem;
}

.report-filter-bar__label {
  display: block;
  margin-bottom: 0.35rem;
  font-size: 0.85rem;
  font-weight: 600;
  color: var(--p-text-muted-color);
}

.report-filter-bar__period,
.report-filter-bar__date-field,
.report-filter-bar__search {
  display: flex;
  flex-direction: column;
}

.report-filter-bar__search {
  flex: 1;
  min-width: 12rem;
}

.report-filter-bar__search-controls {
  display: flex;
  align-items: center;
  gap: 0.25rem;
}

.report-filter-bar__search-field {
  flex: 1;
}

.report-filter-bar__search-input {
  width: 100%;
  min-width: 14rem;
}

.report-filter-bar__datepicker {
  min-width: 16rem;
}

.report-filter-bar__error {
  margin-top: 0.75rem;
}
</style>
