export interface SalesReportRow {
  FakturDate: string
  FakturCode: string
  CustomerName: string
  SalesName: string
  FakturTotal: number
  Status: string
}

export interface SalesReportResponse {
  PeriodFrom: string
  PeriodTo: string
  GeneratedAt: string
  Rows: SalesReportRow[]
}

export interface PiutangReportRow {
  CustomerName: string
  SalesName: string
  FakturCode: string
  FakturDate: string
  JatuhTempo: string
  TotalJual: number
  KurangBayar: number
}

export interface PiutangReportSummary {
  TotalPiutang: number
  TotalCustomer: number
}

export interface PiutangReportResponse {
  PeriodFrom: string
  PeriodTo: string
  GeneratedAt: string
  Summary: PiutangReportSummary
  Rows: PiutangReportRow[]
}

export interface InventoryReportRow {
  BrgId: string
  ItemDisplay: string
  WarehouseName: string
  Qty: number
  Hpp: number
  NilaiSediaan: number
}

export interface InventoryReportSummary {
  TotalInventoryValue: number
  TotalItem: number
}

export interface InventoryReportResponse {
  GeneratedAt: string
  Summary: InventoryReportSummary
  Rows: InventoryReportRow[]
}

export interface PurchasingReportRow {
  InvoiceCode: string
  InvoiceDate: string
  SupplierName: string
  WarehouseName: string
  Total: number
  Disc: number
  Tax: number
  GrandTotal: number
  PostingStok: string
}

export interface PurchasingReportSummary {
  GrandTotalPurchase: number
  TotalInvoice: number
}

export interface PurchasingReportResponse {
  PeriodFrom: string
  PeriodTo: string
  GeneratedAt: string
  Summary: PurchasingReportSummary
  Rows: PurchasingReportRow[]
}
