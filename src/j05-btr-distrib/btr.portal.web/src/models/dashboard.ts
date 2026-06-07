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
