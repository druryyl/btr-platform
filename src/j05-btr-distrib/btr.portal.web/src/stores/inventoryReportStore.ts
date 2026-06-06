import { defineStore } from 'pinia'
import { ref } from 'vue'
import { fetchInventoryReport } from '@/api/reportsApi'
import { getApiErrorMessage } from '@/api/httpClient'
import type { InventoryReportResponse } from '@/models/reports'

export const useInventoryReportStore = defineStore('inventoryReport', () => {
  const report = ref<InventoryReportResponse | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function loadReport(): Promise<void> {
    loading.value = true
    error.value = null

    try {
      report.value = await fetchInventoryReport()
    } catch (err) {
      error.value = getApiErrorMessage(err, 'Failed to load inventory report.')
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
