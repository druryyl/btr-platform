import { defineStore } from 'pinia'
import { ref } from 'vue'
import { fetchSalesReport } from '@/api/reportsApi'
import { getApiErrorMessage } from '@/api/httpClient'
import type { SalesReportResponse } from '@/models/reports'
import { currentMonthRange } from '@/services/reportFilterDefaults'

export const useSalesReportStore = defineStore('salesReport', () => {
  const report = ref<SalesReportResponse | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)
  const from = ref(currentMonthRange().from)
  const to = ref(currentMonthRange().to)
  const freeText = ref('')

  async function loadReport(): Promise<void> {
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
    const defaults = currentMonthRange()
    from.value = defaults.from
    to.value = defaults.to
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
