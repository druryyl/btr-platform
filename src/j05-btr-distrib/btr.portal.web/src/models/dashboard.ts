export interface DashboardOverviewSalesSection {
  TotalOmzet: number
  TotalFaktur: number
  TotalCustomer: number
  GeneratedAt: string
}

export interface DashboardOverviewPiutangSection {
  TotalPiutang: number
  TotalCustomer: number
  GeneratedAt: string
}

export interface DashboardOverviewInventorySection {
  TotalInventoryValue: number
  TotalItem: number
  GeneratedAt: string
}

export interface DashboardOverviewPurchasingSection {
  GrandTotalPurchase: number
  TotalInvoice: number
  GeneratedAt: string
}

export interface DashboardOverviewResponse {
  Sales: DashboardOverviewSalesSection | null
  Piutang: DashboardOverviewPiutangSection | null
  Inventory: DashboardOverviewInventorySection | null
  Purchasing: DashboardOverviewPurchasingSection | null
  HasUnavailableDomain: boolean
}

export type AchievementBand = 'Healthy' | 'Warning' | 'Critical' | 'Unknown'

export interface DashboardExecutiveSalesAttention {
  AchievementPercent: number | null
  TotalAchievement: number
  AchievementBand: AchievementBand
  RequiresAttention: boolean
  IsAvailable: boolean
}

export interface DashboardExecutivePiutangAttention {
  TotalPiutang: number
  OverdueCustomer: number
  AgingOver90Amount: number
  AgingOver90Percent: number | null
  TopCustomerPercent: number | null
  RequiresAttention: boolean
  IsAvailable: boolean
}

export interface DashboardExecutivePurchasingAttention {
  PendingPostingInvoiceCount: number
  PendingPostingValue: number
  TopPrincipalPercent: number | null
  RequiresAttention: boolean
  IsAvailable: boolean
}

export interface DashboardExecutiveInventoryAttention {
  TotalInventoryValue: number
  TopCategoryPercent: number | null
  TopSupplierPercent: number | null
  RequiresAttention: boolean
  IsAvailable: boolean
}

export interface DashboardExecutiveRiskItem {
  Rank: number
  Name: string
  Amount: number
}

export interface DashboardExecutiveCriticalExposures {
  TopCustomers: DashboardExecutiveRiskItem[]
  TopCategories: DashboardExecutiveRiskItem[]
  TopSuppliers: DashboardExecutiveRiskItem[]
  TopPrincipals: DashboardExecutiveRiskItem[]
}

export interface DashboardExecutiveDomainSummary {
  Domain: string
  SummaryText: string
  DetailDashboardRoute: string
  IsAvailable: boolean
}

export interface DashboardExecutiveResponse {
  HasUnavailableDomain: boolean
  IsDataFresh: boolean
  LastRefreshed: string | null
  OverallHealthStatus: string
  Sales: DashboardExecutiveSalesAttention
  Piutang: DashboardExecutivePiutangAttention
  Purchasing: DashboardExecutivePurchasingAttention
  Inventory: DashboardExecutiveInventoryAttention
  CriticalExposures: DashboardExecutiveCriticalExposures
  DomainSummaries: DashboardExecutiveDomainSummary[]
}

export interface DashboardSalesWeekTrendItem {
  WeekStart: string
  WeekEnd: string
  WeekLabel: string
  RecognizedAmount: number
}

export interface DashboardSalesTargetVsAchievement {
  TargetAmount: number
  AchievementAmount: number
}

export interface DashboardSalesRankingItem {
  Rank: number
  SalesPersonName: string
  CompletedOmzet: number
}

export interface DashboardSalesResponse {
  TotalOmzet: number
  CompletedOmzet: number
  PipelineOmzet: number
  TotalFaktur: number
  TotalCustomer: number
  GeneratedAt: string
  WeeklyTrend: DashboardSalesWeekTrendItem[]
  TotalTarget: number
  TotalAchievement: number
  AchievementPercent: number | null
  TargetVsAchievement: DashboardSalesTargetVsAchievement
  TopSalesmanRanking: DashboardSalesRankingItem[]
}

export interface DashboardPiutangAgingBucket {
  BucketKey: string
  BucketLabel: string
  Amount: number
  SortOrder: number
}

export interface DashboardPiutangTopCustomer {
  Rank: number
  CustomerName: string
  OutstandingBalance: number
}

export interface DashboardPiutangResponse {
  TotalPiutang: number
  TotalCustomer: number
  GeneratedAt: string
  OverdueCustomer: number
  AgingBuckets: DashboardPiutangAgingBucket[]
  TopCustomers: DashboardPiutangTopCustomer[]
}

export interface DashboardInventoryBreakdownItem {
  Name: string
  InventoryValue: number
}

export interface DashboardInventoryRankingItem {
  Rank: number
  Name: string
  InventoryValue: number
}

export interface DashboardInventoryResponse {
  TotalInventoryValue: number
  TotalItem: number
  GeneratedAt: string
  CategoryBreakdown: DashboardInventoryBreakdownItem[]
  SupplierBreakdown: DashboardInventoryBreakdownItem[]
  TopCategories: DashboardInventoryRankingItem[]
  TopSuppliers: DashboardInventoryRankingItem[]
}

export interface DashboardPurchasingWeekTrendItem {
  WeekStart: string
  WeekEnd: string
  WeekLabel: string
  PurchaseAmount: number
}

export interface DashboardPurchasingPostingStatusItem {
  StatusKey: string
  StatusLabel: string
  SortOrder: number
  PurchaseAmount: number
}

export interface DashboardPurchasingRankingItem {
  Rank: number
  PrincipalName: string
  PurchaseAmount: number
}

export interface DashboardPurchasingResponse {
  GrandTotalPurchase: number
  TotalInvoice: number
  PendingPostingInvoiceCount: number
  GeneratedAt: string
  WeeklyTrend: DashboardPurchasingWeekTrendItem[]
  PostingStatusBreakdown: DashboardPurchasingPostingStatusItem[]
  TopPrincipalRanking: DashboardPurchasingRankingItem[]
}

export interface DashboardCustomerAttentionCards {
  OverdueCustomerCount: number
  AgingOver90Amount: number
  CollectionRequiresAttention: boolean
  TopOmzetCustomerPercent: number | null
  TopPiutangCustomerPercent: number | null
  ActiveCustomerCount: number
  DormantCustomerCount: number
  InactivityRequiresAttention: boolean
  PlafondBreachCount: number
  SuspendedWithSalesCount: number
  CreditRequiresAttention: boolean
}

export interface DashboardCustomerAttentionItem {
  CustomerCode: string
  CustomerName: string
  SignalKey: string
  SignalLabel: string
  ValueAmount: number | null
  ValueText: string | null
  WilayahName: string
  ReportRoute: string
  RequiresAttention: boolean
}

export interface DashboardCustomerRankingRow {
  Rank: number
  CustomerCode: string
  CustomerName: string
  Amount: number
  PercentOfTotal: number | null
  ReportRoute: string
}

export interface DashboardCustomerRankings {
  TopOmzet: DashboardCustomerRankingRow[]
  TopPiutang: DashboardCustomerRankingRow[]
}

export interface DashboardCustomerSegmentRow {
  SegmentType: string
  SegmentLabel: string
  CustomerCount: number
  ActiveCount: number
  DormantCount: number
}

export interface DashboardCustomerSegmentationSummary {
  ByKlasifikasi: DashboardCustomerSegmentRow[]
  ByWilayah: DashboardCustomerSegmentRow[]
  ActiveSummary: DashboardCustomerSegmentRow | null
  DormantSummary: DashboardCustomerSegmentRow | null
}

export interface DashboardCustomerNavigationLinks {
  SalesDashboardRoute: string
  PiutangDashboardRoute: string
  SalesReportRoute: string
  PiutangReportRoute: string
}

export interface DashboardCustomerResponse {
  IsAvailable: boolean
  IsDataFresh: boolean
  GeneratedAt: string | null
  PeriodYear: number
  PeriodMonth: number
  AttentionCards: DashboardCustomerAttentionCards | null
  AttentionList: DashboardCustomerAttentionItem[]
  Rankings: DashboardCustomerRankings | null
  Segmentation: DashboardCustomerSegmentationSummary | null
  Navigation: DashboardCustomerNavigationLinks | null
}
