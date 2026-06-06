import { httpClient } from '@/api/httpClient'
import { isApiSuccess, type ApiResponse } from '@/models/api'
import type {
  InventoryReportResponse,
  PiutangReportResponse,
  PurchasingReportResponse,
  SalesReportResponse,
} from '@/models/reports'

export async function fetchSalesReport(): Promise<SalesReportResponse> {
  const { data } = await httpClient.get<ApiResponse<SalesReportResponse>>('/api/reports/sales')

  if (!isApiSuccess(data) || !data.Data) {
    throw new Error(data.Message ?? 'Failed to load sales report.')
  }

  return data.Data
}

export async function fetchPiutangReport(): Promise<PiutangReportResponse> {
  const { data } = await httpClient.get<ApiResponse<PiutangReportResponse>>('/api/reports/piutang')

  if (!isApiSuccess(data) || !data.Data) {
    throw new Error(data.Message ?? 'Failed to load piutang report.')
  }

  return data.Data
}

export async function fetchInventoryReport(): Promise<InventoryReportResponse> {
  const { data } = await httpClient.get<ApiResponse<InventoryReportResponse>>('/api/reports/inventory')

  if (!isApiSuccess(data) || !data.Data) {
    throw new Error(data.Message ?? 'Failed to load inventory report.')
  }

  return data.Data
}

export async function fetchPurchasingReport(): Promise<PurchasingReportResponse> {
  const { data } = await httpClient.get<ApiResponse<PurchasingReportResponse>>('/api/reports/purchasing')

  if (!isApiSuccess(data) || !data.Data) {
    throw new Error(data.Message ?? 'Failed to load purchasing report.')
  }

  return data.Data
}
