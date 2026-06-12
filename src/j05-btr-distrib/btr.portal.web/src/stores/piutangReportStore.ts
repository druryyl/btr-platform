import { defineStore } from 'pinia'
import { ref } from 'vue'
import { fetchPiutangReport } from '@/api/reportsApi'
import { getApiErrorMessage } from '@/api/httpClient'
import type { PiutangReportResponse } from '@/models/reports'
import { currentMonthRange, type PiutangDateField } from '@/services/reportFilterDefaults'
import { usePresentationStore } from '@/stores/presentationStore'

export const usePiutangReportStore = defineStore('piutangReport', () => {
  const report = ref<PiutangReportResponse | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)
  const from = ref(currentMonthRange().from)
  const to = ref(currentMonthRange().to)
  const dateField = ref<PiutangDateField>('DueDate')
  const freeText = ref('')
  const allOpenBalances = ref(false)
  let defaultPeriodApplied = false

  function syncDefaultPeriod(): void {
    const presentation = usePresentationStore()
    const defaults = currentMonthRange(presentation.businessReferenceDate)
    from.value = defaults.from
    to.value = defaults.to
    defaultPeriodApplied = true
  }

  function ensureDefaultPeriod(): void {
    if (!defaultPeriodApplied) {
      syncDefaultPeriod()
    }
  }

  async function loadReport(options?: { allOpenBalances?: boolean }): Promise<void> {
    ensureDefaultPeriod()
    loading.value = true
    error.value = null

    if (options?.allOpenBalances !== undefined) {
      allOpenBalances.value = options.allOpenBalances
    }

    try {
      report.value = await fetchPiutangReport({
        from: from.value,
        to: to.value,
        dateField: dateField.value,
        allOpenBalances: allOpenBalances.value,
      })
    } catch (err) {
      error.value = getApiErrorMessage(err, 'Failed to load piutang report.')
    } finally {
      loading.value = false
    }
  }

  function reset(): void {
    report.value = null
    loading.value = false
    error.value = null
    syncDefaultPeriod()
    dateField.value = 'DueDate'
    freeText.value = ''
    allOpenBalances.value = false
  }

  return {
    report,
    loading,
    error,
    from,
    to,
    dateField,
    freeText,
    allOpenBalances,
    loadReport,
    reset,
  }
})
