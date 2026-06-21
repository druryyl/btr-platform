import { defineStore } from 'pinia'
import { ref } from 'vue'
import {
  fetchDashboardAlerts,
  fetchDashboardCustomer,
  fetchDashboardCollection,
  fetchDashboardLocation,
  fetchDashboardSalesman,
  fetchDashboardExecutive,
  fetchDashboardInventory,
  fetchDashboardInventoryRisk,
  fetchDashboardOverview,
  fetchDashboardPiutang,
  fetchDashboardPurchasing,
  fetchDashboardSales,
  fetchDashboardSalesForecast,
} from '@/api/dashboardApi'
import { getApiErrorMessage } from '@/api/httpClient'
import { isInfrastructureStoreError } from '@/services/platformDiagnostics'
import { usePresentationStore } from '@/stores/presentationStore'
import type {
  DashboardAlertCenterResponse,
  DashboardCollectionResponse,
  DashboardLocationResponse,
  DashboardCustomerResponse,
  DashboardSalesmanResponse,
  DashboardExecutiveResponse,
  DashboardInventoryResponse,
  DashboardInventoryRiskResponse,
  DashboardOverviewResponse,
  DashboardPiutangResponse,
  DashboardPurchasingResponse,
  DashboardSalesResponse,
  DashboardSalesForecastResponse,
} from '@/models/dashboard'

export const useDashboardStore = defineStore('dashboard', () => {
  const overview = ref<DashboardOverviewResponse | null>(null)
  const executive = ref<DashboardExecutiveResponse | null>(null)
  const alerts = ref<DashboardAlertCenterResponse | null>(null)
  const sales = ref<DashboardSalesResponse | null>(null)
  const salesForecast = ref<DashboardSalesForecastResponse | null>(null)
  const piutang = ref<DashboardPiutangResponse | null>(null)
  const inventory = ref<DashboardInventoryResponse | null>(null)
  const inventoryRisk = ref<DashboardInventoryRiskResponse | null>(null)
  const purchasing = ref<DashboardPurchasingResponse | null>(null)
  const customer = ref<DashboardCustomerResponse | null>(null)
  const collection = ref<DashboardCollectionResponse | null>(null)
  const location = ref<DashboardLocationResponse | null>(null)
  const salesman = ref<DashboardSalesmanResponse | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  function setInfrastructureError(message: string): void {
    const presentation = usePresentationStore()
    if (presentation.hidePlatformDiagnostics && isInfrastructureStoreError(message)) {
      return
    }

    error.value = message
  }

  async function loadExecutive(): Promise<void> {
    loading.value = true
    error.value = null

    try {
      executive.value = await fetchDashboardExecutive()

      if (executive.value.HasUnavailableDomain) {
        setInfrastructureError(
          'Some dashboard data is not yet available. Run the snapshot refresh worker.',
        )
      }
    } catch (err) {
      error.value = getApiErrorMessage(err, 'Failed to load executive dashboard.')
    } finally {
      loading.value = false
    }
  }

  async function loadAlerts(): Promise<void> {
    loading.value = true
    error.value = null

    try {
      alerts.value = await fetchDashboardAlerts()

      if (alerts.value.HasUnavailableDomain) {
        setInfrastructureError(
          'Some dashboard data is not yet available. Run the snapshot refresh worker.',
        )
      }
    } catch (err) {
      error.value = getApiErrorMessage(err, 'Failed to load alert center.')
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
        setInfrastructureError(
          'Some dashboard data is not yet available. Run the snapshot refresh worker.',
        )
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

  async function loadSalesForecast(): Promise<void> {
    loading.value = true
    error.value = null

    try {
      salesForecast.value = await fetchDashboardSalesForecast()
    } catch (err) {
      error.value = getApiErrorMessage(err, 'Failed to load sales forecast dashboard.')
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

  async function loadInventoryRisk(): Promise<void> {
    loading.value = true
    error.value = null

    try {
      inventoryRisk.value = await fetchDashboardInventoryRisk()

      if (!inventoryRisk.value.IsAvailable) {
        setInfrastructureError(
          'Inventory risk data is not yet available. Run the snapshot refresh worker.',
        )
      }
    } catch (err) {
      error.value = getApiErrorMessage(err, 'Failed to load inventory risk dashboard.')
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
        setInfrastructureError(
          'Customer analytics data is not yet available. Run the snapshot refresh worker.',
        )
      }
    } catch (err) {
      error.value = getApiErrorMessage(err, 'Failed to load customer analytics dashboard.')
    } finally {
      loading.value = false
    }
  }

  async function loadCollection(): Promise<void> {
    loading.value = true
    error.value = null

    try {
      collection.value = await fetchDashboardCollection()

      if (!collection.value.IsAvailable) {
        setInfrastructureError(
          'Collection dashboard data is not yet available. Run the snapshot refresh worker.',
        )
      }
    } catch (err) {
      error.value = getApiErrorMessage(err, 'Failed to load collection dashboard.')
    } finally {
      loading.value = false
    }
  }

  async function loadLocation(): Promise<void> {
    loading.value = true
    error.value = null

    try {
      location.value = await fetchDashboardLocation()

      if (!location.value.IsAvailable) {
        setInfrastructureError(
          'Location dashboard data is not yet available. Run the snapshot refresh worker.',
        )
      }
    } catch (err) {
      error.value = getApiErrorMessage(err, 'Failed to load location dashboard.')
    } finally {
      loading.value = false
    }
  }

  async function loadSalesman(): Promise<void> {
    loading.value = true
    error.value = null

    try {
      salesman.value = await fetchDashboardSalesman()

      if (!salesman.value.IsAvailable) {
        setInfrastructureError(
          'Salesman performance data is not yet available. Run the snapshot refresh worker.',
        )
      }
    } catch (err) {
      error.value = getApiErrorMessage(err, 'Failed to load salesman performance dashboard.')
    } finally {
      loading.value = false
    }
  }

  function reset(): void {
    overview.value = null
    executive.value = null
    alerts.value = null
    sales.value = null
    salesForecast.value = null
    piutang.value = null
    inventory.value = null
    inventoryRisk.value = null
    purchasing.value = null
    customer.value = null
    collection.value = null
    location.value = null
    salesman.value = null
    loading.value = false
    error.value = null
  }

  return {
    overview,
    executive,
    alerts,
    sales,
    salesForecast,
    piutang,
    inventory,
    inventoryRisk,
    purchasing,
    customer,
    collection,
    location,
    salesman,
    loading,
    error,
    loadDashboard,
    loadExecutive,
    loadAlerts,
    loadSales,
    loadSalesForecast,
    loadPiutang,
    loadInventory,
    loadInventoryRisk,
    loadPurchasing,
    loadCustomer,
    loadCollection,
    loadLocation,
    loadSalesman,
    reset,
  }
})
