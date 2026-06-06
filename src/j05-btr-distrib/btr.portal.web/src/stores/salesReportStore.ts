import { defineStore } from 'pinia'
import { ref } from 'vue'
import { fetchSalesReport } from '@/api/reportsApi'
import { getApiErrorMessage } from '@/api/httpClient'
import type { SalesReportResponse } from '@/models/reports'

export const useSalesReportStore = defineStore('salesReport', () => {
  const report = ref<SalesReportResponse | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function loadReport(): Promise<void> {
    loading.value = true
    error.value = null

    try {
      report.value = await fetchSalesReport()
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
  }

  return {
    report,
    loading,
    error,
    loadReport,
    reset,
  }
})
