import { defineStore } from 'pinia'
import { ref } from 'vue'
import { fetchSalesReport } from '@/api/reportsApi'
import { getApiErrorMessage } from '@/api/httpClient'
import type { SalesReportResponse } from '@/models/reports'
import { currentMonthRange } from '@/services/reportFilterDefaults'
import { usePresentationStore } from '@/stores/presentationStore'

export const useSalesReportStore = defineStore('salesReport', () => {
  const report = ref<SalesReportResponse | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)
  const from = ref(currentMonthRange().from)
  const to = ref(currentMonthRange().to)
  const freeText = ref('')
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

  async function loadReport(): Promise<void> {
    ensureDefaultPeriod()
    loading.value = true
    error.value = null

    try {
      report.value = await fetchSalesReport({ from: from.value, to: to.value })
    } catch (err) {
      error.value = getApiErrorMessage(err, 'Failed to load sales report.')
    } finally {
      loading.value = false
    }
  }

  function reset(): void {
    report.value = null
    loading.value = false
    error.value = null
    syncDefaultPeriod()
    freeText.value = ''
  }

  return {
    report,
    loading,
    error,
    from,
    to,
    freeText,
    loadReport,
    reset,
  }
})
