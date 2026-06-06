import { defineStore } from 'pinia'
import { ref } from 'vue'
import { fetchPiutangReport } from '@/api/reportsApi'
import { getApiErrorMessage } from '@/api/httpClient'
import type { PiutangReportResponse } from '@/models/reports'

export const usePiutangReportStore = defineStore('piutangReport', () => {
  const report = ref<PiutangReportResponse | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function loadReport(): Promise<void> {
    loading.value = true
    error.value = null

    try {
      report.value = await fetchPiutangReport()
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
  }

  return {
    report,
    loading,
    error,
    loadReport,
    reset,
  }
})
