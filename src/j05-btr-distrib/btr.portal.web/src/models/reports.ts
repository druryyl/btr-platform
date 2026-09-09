export interface SalesReportRow {
  FakturDate: string
  FakturCode: string
  CustomerName: string
  SalesPersonId: string
  SalesName: string
  FakturTotal: number
  Status: string
}

export interface SalesReportPrincipalRow {
  SupplierId: string
  PrincipalName: string
  KpiId: string
  PrincipalSalesOutAmount: number
  LineCount: number
}

export interface SalesReportPrincipalEvidenceLine {
  FakturId: string
  FakturCode: string
  FakturDate: string
  FakturItemId: string
  BrgId: string
  SupplierId: string
  KpiId: string
  PrincipalSalesOutAmount: number
}

export interface SalesReportResponse {
  PeriodFrom: string
  PeriodTo: string
  GeneratedAt: string
  Rows: SalesReportRow[]
  KpiId: string
  KpiName: string
  HeaderTotalLabel: string
  HeaderTotalAmount: number
  Disclosures: string[]
  Principals: SalesReportPrincipalRow[]
  UnknownPrincipalExceptionCount: number
  SelectedSupplierId: string
  SelectedPrincipalName: string
  SelectedPrincipalSalesOutAmount: number
  EvidenceLines: SalesReportPrincipalEvidenceLine[]
}

export interface SalesReportQuery extends ReportDateQuery {
  supplierId?: string
}

export interface PiutangReportRow {
  CustomerCode: string
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
  DateField: string
  AllOpenBalances: boolean
  GeneratedAt: string
  Summary: PiutangReportSummary
  Rows: PiutangReportRow[]
}

export interface ReportDateQuery {
  from: string
  to: string
}

export interface PiutangReportQuery extends ReportDateQuery {
  dateField: 'DueDate' | 'PiutangDate'
  allOpenBalances?: boolean
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

export interface CustomerReportQuery {
  customerCode?: string
}

export interface CustomerReportRow {
  CustomerCode: string
  CustomerName: string
  WilayahName: string
  Klasifikasi: string
  MtdOmzet: number
  OpenBalance: number
  OverdueBalance: number | null
  LastPurchaseDate: string | null
  FirstPurchaseDate: string | null
  LifecycleStage: string
  LifecycleLabel: string
  PortfolioTier: string
  TierLabel: string
  PrimaryActionKey: string
  PrimaryActionLabel: string
  ActionOwner: string
  ActionReasonText: string
  SalesPersonName: string
  SalesmanAchievementPercent: number | null
  M29Category: string
  ValueDisclaimer: string
}

export interface CustomerReportSummary {
  TotalCustomerCount: number
  TotalMtdOmzet: number
  TotalOpenBalance: number
}

export interface CustomerReportPrincipalPair {
  SupplierId: string
  PrincipalName: string
  RelationshipStatus: string
  KpiId: string
  PairSalesOutAmount: number
}

export interface CustomerReportPrincipalPairCustomer {
  CustomerCode: string
  CustomerName: string
  Principals: CustomerReportPrincipalPair[]
}

export interface CustomerReportPrincipalPairEvidence {
  IsAvailable: boolean
  KpiId: string
  Note: string
  Disclosures: string[]
  Customers: CustomerReportPrincipalPairCustomer[]
}

export interface CustomerReportResponse {
  GeneratedAt: string
  Summary: CustomerReportSummary
  Rows: CustomerReportRow[]
  PairEvidence?: CustomerReportPrincipalPairEvidence | null
}
