import { defineStore } from 'pinia'
import { ref } from 'vue'
import {
  fetchDashboardCustomer,
  fetchDashboardExecutive,
  fetchDashboardInventory,
  fetchDashboardOverview,
  fetchDashboardPiutang,
  fetchDashboardPurchasing,
  fetchDashboardSales,
} from '@/api/dashboardApi'
import { getApiErrorMessage } from '@/api/httpClient'
import type {
  DashboardCustomerResponse,
  DashboardExecutiveResponse,
  DashboardInventoryResponse,
  DashboardOverviewResponse,
  DashboardPiutangResponse,
  DashboardPurchasingResponse,
  DashboardSalesResponse,
} from '@/models/dashboard'

export const useDashboardStore = defineStore('dashboard', () => {
  const overview = ref<DashboardOverviewResponse | null>(null)
  const executive = ref<DashboardExecutiveResponse | null>(null)
  const sales = ref<DashboardSalesResponse | null>(null)
  const piutang = ref<DashboardPiutangResponse | null>(null)
  const inventory = ref<DashboardInventoryResponse | null>(null)
  const purchasing = ref<DashboardPurchasingResponse | null>(null)
  const customer = ref<DashboardCustomerResponse | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function loadExecutive(): Promise<void> {
    loading.value = true
    error.value = null

    try {
      executive.value = await fetchDashboardExecutive()

      if (executive.value.HasUnavailableDomain) {
        error.value = 'Some dashboard data is not yet available. Run the snapshot refresh worker.'
      }
    } catch (err) {
      error.value = getApiErrorMessage(err, 'Failed to load executive dashboard.')
    } finally {
      loading.value = false
    }
  }

  async function loadDashboard(): Promise<void> {
    loading.value = true
    error.value = null

    try {
      overview.value = await fetchDashboardOverview()

      if (overview.value.HasUnavailableDomain) {
        error.value = 'Some dashboard data is not yet available. Run the snapshot refresh worker.'
      }
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

  async function loadPurchasing(): Promise<void> {
    loading.value = true
    error.value = null

    try {
      purchasing.value = await fetchDashboardPurchasing()
    } catch (err) {
      error.value = getApiErrorMessage(err, 'Failed to load purchasing dashboard.')
    } finally {
      loading.value = false
    }
  }

  async function loadCustomer(): Promise<void> {
    loading.value = true
    error.value = null

    try {
      customer.value = await fetchDashboardCustomer()

      if (!customer.value.IsAvailable) {
        error.value = 'Customer analytics data is not yet available. Run the snapshot refresh worker.'
      }
    } catch (err) {
      error.value = getApiErrorMessage(err, 'Failed to load customer analytics dashboard.')
    } finally {
      loading.value = false
    }
  }

  function reset(): void {
    overview.value = null
    executive.value = null
    sales.value = null
    piutang.value = null
    inventory.value = null
    purchasing.value = null
    customer.value = null
    loading.value = false
    error.value = null
  }

  return {
    overview,
    executive,
    sales,
    piutang,
    inventory,
    purchasing,
    customer,
    loading,
    error,
    loadDashboard,
    loadExecutive,
    loadSales,
    loadPiutang,
    loadInventory,
    loadPurchasing,
    loadCustomer,
    reset,
  }
})
