import { defineStore } from 'pinia'
import { ref } from 'vue'
import {
  fetchDashboardInventory,
  fetchDashboardPiutang,
  fetchDashboardSales,
} from '@/api/dashboardApi'
import { getApiErrorMessage } from '@/api/httpClient'
import type {
  DashboardInventoryResponse,
  DashboardPiutangResponse,
  DashboardSalesResponse,
} from '@/models/dashboard'

export const useDashboardStore = defineStore('dashboard', () => {
  const sales = ref<DashboardSalesResponse | null>(null)
  const piutang = ref<DashboardPiutangResponse | null>(null)
  const inventory = ref<DashboardInventoryResponse | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function loadDashboard(): Promise<void> {
    loading.value = true
    error.value = null

    try {
      const [salesData, piutangData, inventoryData] = await Promise.all([
        fetchDashboardSales(),
        fetchDashboardPiutang(),
        fetchDashboardInventory(),
      ])

      sales.value = salesData
      piutang.value = piutangData
      inventory.value = inventoryData
    } catch (err) {
      error.value = getApiErrorMessage(err, 'Failed to load dashboard data.')
    } finally {
      loading.value = false
    }
  }

  async function loadSales(): Promise<void> {
    loading.value = true
    error.value = null

    try {
      sales.value = await fetchDashboardSales()
    } catch (err) {
      error.value = getApiErrorMessage(err, 'Failed to load sales dashboard.')
    } finally {
      loading.value = false
    }
  }

  async function loadPiutang(): Promise<void> {
    loading.value = true
    error.value = null

    try {
      piutang.value = await fetchDashboardPiutang()
    } catch (err) {
      error.value = getApiErrorMessage(err, 'Failed to load piutang dashboard.')
    } finally {
      loading.value = false
    }
  }

  async function loadInventory(): Promise<void> {
    loading.value = true
    error.value = null

    try {
      inventory.value = await fetchDashboardInventory()
    } catch (err) {
      error.value = getApiErrorMessage(err, 'Failed to load inventory dashboard.')
    } finally {
      loading.value = false
    }
  }

  function reset(): void {
    sales.value = null
    piutang.value = null
    inventory.value = null
    loading.value = false
    error.value = null
  }

  return {
    sales,
    piutang,
    inventory,
    loading,
    error,
    loadDashboard,
    loadSales,
    loadPiutang,
    loadInventory,
    reset,
  }
})
