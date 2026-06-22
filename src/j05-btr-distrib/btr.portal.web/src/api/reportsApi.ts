import { httpClient } from '@/api/httpClient'
import { isApiSuccess, type ApiResponse } from '@/models/api'
import type {
  CustomerReportQuery,
  CustomerReportResponse,
  InventoryReportResponse,
  PiutangReportQuery,
  PiutangReportResponse,
  PurchasingReportResponse,
  ReportDateQuery,
  SalesReportResponse,
} from '@/models/reports'

export async function fetchSalesReport(query: ReportDateQuery): Promise<SalesReportResponse> {
  const { data } = await httpClient.get<ApiResponse<SalesReportResponse>>('/api/reports/sales', {
    params: { from: query.from, to: query.to },
  })

  if (!isApiSuccess(data) || !data.Data) {
    throw new Error(data.Message ?? 'Failed to load sales report.')
  }

  return data.Data
}

export async function fetchPiutangReport(query: PiutangReportQuery): Promise<PiutangReportResponse> {
  const { data } = await httpClient.get<ApiResponse<PiutangReportResponse>>('/api/reports/piutang', {
    params: {
      from: query.from,
      to: query.to,
      dateField: query.dateField,
      allOpenBalances: query.allOpenBalances ?? false,
    },
  })

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

export async function fetchPurchasingReport(query: ReportDateQuery): Promise<PurchasingReportResponse> {
  const { data } = await httpClient.get<ApiResponse<PurchasingReportResponse>>('/api/reports/purchasing', {
    params: { from: query.from, to: query.to },
  })

  if (!isApiSuccess(data) || !data.Data) {
    throw new Error(data.Message ?? 'Failed to load purchasing report.')
  }

  return data.Data
}

export async function fetchCustomerReport(
  query: CustomerReportQuery = {},
): Promise<CustomerReportResponse> {
  const { data } = await httpClient.get<ApiResponse<CustomerReportResponse>>('/api/reports/customers', {
    params: query.customerCode ? { customerCode: query.customerCode } : undefined,
  })

  if (!isApiSuccess(data) || !data.Data) {
    throw new Error(data.Message ?? 'Failed to load customer report.')
  }

  return data.Data
}
