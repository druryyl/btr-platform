import { defineStore } from 'pinia'
import { ref } from 'vue'
import { fetchPurchasingReport } from '@/api/reportsApi'
import { getApiErrorMessage } from '@/api/httpClient'
import type { PurchasingReportResponse } from '@/models/reports'

export const usePurchasingReportStore = defineStore('purchasingReport', () => {
  const report = ref<PurchasingReportResponse | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function loadReport(): Promise<void> {
    loading.value = true
    error.value = null

    try {
      report.value = await fetchPurchasingReport()
    } catch (err) {
      error.value = getApiErrorMessage(err, 'Failed to load purchasing report.')
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
