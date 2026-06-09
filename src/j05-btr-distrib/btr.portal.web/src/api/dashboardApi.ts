import { httpClient } from '@/api/httpClient'
import { isApiSuccess, type ApiResponse } from '@/models/api'
import type {
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
} from '@/models/dashboard'

export async function fetchDashboardOverview(): Promise<DashboardOverviewResponse> {
  const { data } = await httpClient.get<ApiResponse<DashboardOverviewResponse>>('/api/dashboard/overview')

  if (!isApiSuccess(data) || !data.Data) {
    throw new Error(data.Message ?? 'Failed to load dashboard overview.')
  }

  return data.Data
}

export async function fetchDashboardExecutive(): Promise<DashboardExecutiveResponse> {
  const { data } = await httpClient.get<ApiResponse<DashboardExecutiveResponse>>('/api/dashboard/executive')

  if (!isApiSuccess(data) || !data.Data) {
    throw new Error(data.Message ?? 'Failed to load executive dashboard.')
  }

  return data.Data
}

export async function fetchDashboardSales(): Promise<DashboardSalesResponse> {
  const { data } = await httpClient.get<ApiResponse<DashboardSalesResponse>>('/api/dashboard/sales')

  if (!isApiSuccess(data) || !data.Data) {
    throw new Error(data.Message ?? 'Failed to load sales dashboard.')
  }

  return data.Data
}

export async function fetchDashboardPiutang(): Promise<DashboardPiutangResponse> {
  const { data } = await httpClient.get<ApiResponse<DashboardPiutangResponse>>('/api/dashboard/piutang')

  if (!isApiSuccess(data) || !data.Data) {
    throw new Error(data.Message ?? 'Failed to load piutang dashboard.')
  }

  return data.Data
}

export async function fetchDashboardInventory(): Promise<DashboardInventoryResponse> {
  const { data } = await httpClient.get<ApiResponse<DashboardInventoryResponse>>('/api/dashboard/inventory')

  if (!isApiSuccess(data) || !data.Data) {
    throw new Error(data.Message ?? 'Failed to load inventory dashboard.')
  }

  return data.Data
}

export async function fetchDashboardInventoryRisk(): Promise<DashboardInventoryRiskResponse> {
  const { data } = await httpClient.get<ApiResponse<DashboardInventoryRiskResponse>>('/api/dashboard/inventory-risk')

  if (!isApiSuccess(data) || !data.Data) {
    throw new Error(data.Message ?? 'Failed to load inventory risk dashboard.')
  }

  return data.Data
}

export async function fetchDashboardPurchasing(): Promise<DashboardPurchasingResponse> {
  const { data } = await httpClient.get<ApiResponse<DashboardPurchasingResponse>>('/api/dashboard/purchasing')

  if (!isApiSuccess(data) || !data.Data) {
    throw new Error(data.Message ?? 'Failed to load purchasing dashboard.')
  }

  return data.Data
}

export async function fetchDashboardCustomer(): Promise<DashboardCustomerResponse> {
  const { data } = await httpClient.get<ApiResponse<DashboardCustomerResponse>>('/api/dashboard/customers')

  if (!isApiSuccess(data) || !data.Data) {
    throw new Error(data.Message ?? 'Failed to load customer analytics dashboard.')
  }

  return data.Data
}

export async function fetchDashboardSalesman(): Promise<DashboardSalesmanResponse> {
  const { data } = await httpClient.get<ApiResponse<DashboardSalesmanResponse>>('/api/dashboard/salesmen')

  if (!isApiSuccess(data) || !data.Data) {
    throw new Error(data.Message ?? 'Failed to load salesman performance dashboard.')
  }

  return data.Data
}

export async function fetchDashboardCollection(): Promise<DashboardCollectionResponse> {
  const { data } = await httpClient.get<ApiResponse<DashboardCollectionResponse>>('/api/dashboard/collection')

  if (!isApiSuccess(data) || !data.Data) {
    throw new Error(data.Message ?? 'Failed to load collection dashboard.')
  }

  return data.Data
}

export async function fetchDashboardLocation(): Promise<DashboardLocationResponse> {
  const { data } = await httpClient.get<ApiResponse<DashboardLocationResponse>>('/api/dashboard/locations')

  if (!isApiSuccess(data) || !data.Data) {
    throw new Error(data.Message ?? 'Failed to load location dashboard.')
  }

  return data.Data
}
