import { defineStore } from 'pinia'
import { ref } from 'vue'
import { fetchCustomerReport } from '@/api/reportsApi'
import { getApiErrorMessage } from '@/api/httpClient'
import type { CustomerReportResponse } from '@/models/reports'

export const useCustomerReportStore = defineStore('customerReport', () => {
  const report = ref<CustomerReportResponse | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)
  const customerCode = ref('')
  const freeText = ref('')

  async function loadReport(options?: { customerCode?: string }): Promise<void> {
    loading.value = true
    error.value = null

    if (options?.customerCode !== undefined) {
      customerCode.value = options.customerCode
    }

    try {
      report.value = await fetchCustomerReport({
        customerCode: customerCode.value || undefined,
      })
    } catch (err) {
      error.value = getApiErrorMessage(err, 'Failed to load customer report.')
    } finally {
      loading.value = false
    }
  }

  function reset(): void {
    report.value = null
    loading.value = false
    error.value = null
    customerCode.value = ''
    freeText.value = ''
  }

  return {
    report,
    loading,
    error,
    customerCode,
    freeText,
    loadReport,
    reset,
  }
})
