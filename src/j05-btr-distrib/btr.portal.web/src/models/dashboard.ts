import type { InvestigationMetadata } from '@/models/investigation'

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
  Investigation?: InvestigationMetadata | null
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
  SalesPersonId?: string
  CompletedOmzet: number
  Investigation?: InvestigationMetadata | null
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

export interface DashboardPiutangTopCustomerRiskRow {
  Rank: number
  CustomerName: string
  CustomerCode?: string
  TotalPiutang: number
  CurrentAmount: number
  Aging30Amount: number
  Aging60Amount: number
  Aging90Amount: number
  AgingOver90Amount: number
  Investigation?: InvestigationMetadata | null
}

export interface DashboardPiutangResponse {
  TotalPiutang: number
  TotalCustomer: number
  GeneratedAt: string
  OverdueCustomer: number
  OverduePiutang: number
  AgingOver90Amount: number
  AgingOver90Percent: number | null
  Top10CustomerConcentrationPercent: number | null
  Top20CustomerConcentrationPercent: number | null
  AgingBuckets: DashboardPiutangAgingBucket[]
  TopCustomerRisk: DashboardPiutangTopCustomerRiskRow[]
}

export interface DashboardInventoryBreakdownItem {
  Name: string
  InventoryValue: number
}

export interface DashboardInventoryRankingItem {
  Rank: number
  Name: string
  InventoryValue: number
  Investigation?: InvestigationMetadata | null
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

export interface DashboardInventoryRiskAttentionCards {
  TotalInventoryValue: number
  DeadStockItemCount: number
  DeadStockValue: number
  SlowMovingItemCount: number
  SlowMovingValue: number
  AtRiskInventoryPercent: number
  RequiresAttention: boolean
}

export interface DashboardInventoryRiskAgingBucket {
  BucketKey: string
  BucketLabel: string
  Amount: number
  ItemCount: number
  SortOrder: number
}

export interface DashboardInventoryRiskBreakdownItem {
  Name: string
  AtRiskValue: number
  ItemCount: number
  PercentOfAtRisk: number | null
}

export interface DashboardInventoryRiskAttentionItem {
  BrgCode: string
  BrgName: string
  KategoriName: string
  SupplierName: string
  Qty: number
  InventoryValue: number
  DaysSinceLastFaktur: number | null
  SignalKey: string
  SignalLabel: string
  ReportRoute: string
  Investigation?: InvestigationMetadata | null
}

export interface DashboardInventoryRiskRankingRow {
  Rank: number
  BrgCode: string
  BrgName: string
  KategoriName: string
  SupplierName: string
  Qty: number
  InventoryValue: number
  DaysSinceLastFaktur: number
  PercentOfAtRisk: number | null
  ReportRoute: string
  Investigation?: InvestigationMetadata | null
}

export interface DashboardInventoryRiskRankings {
  TopDead: DashboardInventoryRiskRankingRow[]
  TopSlow: DashboardInventoryRiskRankingRow[]
}

export interface DashboardInventoryRiskNavigationLinks {
  InventoryDashboardRoute: string
  InventoryReportRoute: string
}

export interface DashboardInventoryRiskResponse {
  IsAvailable: boolean
  IsDataFresh: boolean
  GeneratedAt: string | null
  AttentionCards: DashboardInventoryRiskAttentionCards | null
  AgingBuckets: DashboardInventoryRiskAgingBucket[]
  CategoryRiskExposure: DashboardInventoryRiskBreakdownItem[]
  SupplierRiskExposure: DashboardInventoryRiskBreakdownItem[]
  AttentionList: DashboardInventoryRiskAttentionItem[]
  Rankings: DashboardInventoryRiskRankings | null
  Navigation: DashboardInventoryRiskNavigationLinks | null
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
  Investigation?: InvestigationMetadata | null
}

export interface DashboardPurchasingAttentionCardGroup {
  RequiresAttention: boolean
  Metrics: Record<string, string>
}

export interface DashboardPurchasingAttentionCards {
  PostingExposure: DashboardPurchasingAttentionCardGroup | null
  PrincipalDependency: DashboardPurchasingAttentionCardGroup | null
  PurchasingPace: DashboardPurchasingAttentionCardGroup | null
  InventoryCrossRisk: DashboardPurchasingAttentionCardGroup | null
}

export interface DashboardPurchasingSummaryRow {
  GrandTotalPurchase: number
  TotalInvoice: number
  PostedPercent: number | null
  PendingPostingValue: number
  QualifiedBacklogCount: number
  QualifiedBacklogValue: number
}

export interface DashboardPurchasingAttentionItem {
  EntityType: string
  EntityName: string
  SignalKey: string
  SignalLabel: string
  ValueAmount: number | null
  ValueText: string | null
  ReportRoute: string | null
  Investigation?: InvestigationMetadata | null
}

export interface DashboardPurchasingPrincipalExposureItem {
  Rank: number
  PrincipalName: string
  MtdPurchaseAmount: number
  PercentOfPurchase: number | null
  InventoryValue: number | null
  PercentOfInventory: number | null
  AtRiskValue: number | null
  PercentOfAtRisk: number | null
  IsCompoundDependency: boolean
  IsInventoryNoPurchase: boolean
  ReportRoute: string | null
  Investigation?: InvestigationMetadata | null
}

export interface DashboardPurchasingNavigationLinks {
  PurchasingReportRoute: string
  InventoryDashboardRoute: string
  InventoryRiskDashboardRoute: string
}

export interface DashboardPurchasingResponse {
  GrandTotalPurchase: number
  TotalInvoice: number
  PendingPostingInvoiceCount: number
  GeneratedAt: string
  WeeklyTrend: DashboardPurchasingWeekTrendItem[]
  PostingStatusBreakdown: DashboardPurchasingPostingStatusItem[]
  TopPrincipalRanking: DashboardPurchasingRankingItem[]
  IsManagementAvailable: boolean
  IsDataFresh: boolean
  AttentionCards: DashboardPurchasingAttentionCards | null
  Summary: DashboardPurchasingSummaryRow | null
  AttentionList: DashboardPurchasingAttentionItem[]
  PrincipalExposure: DashboardPurchasingPrincipalExposureItem[]
  Navigation: DashboardPurchasingNavigationLinks | null
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
  Investigation?: InvestigationMetadata | null
}

export interface DashboardCustomerRankingRow {
  Rank: number
  CustomerCode: string
  CustomerName: string
  Amount: number
  PercentOfTotal: number | null
  ReportRoute: string
  Investigation?: InvestigationMetadata | null
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

export interface DashboardSalesmanFilterDefaults {
  DefaultActiveOnly: boolean
  ExposureTopPercent: number
}

export interface DashboardSalesmanAttentionCards {
  BelowTargetCount: number
  MissingTargetSetupCount: number
  HighOverdueExposureCount: number
  HighPiutangExposureCount: number
  CustomerConcentrationCount: number
  DormantPortfolioCount: number
  TopOmzetSalesmanPercent: number | null
  TopPiutangSalesmanPercent: number | null
  PerformanceRequiresAttention: boolean
  CollectionRequiresAttention: boolean
  PortfolioRequiresAttention: boolean
}

export interface DashboardSalesmanAttentionItem {
  SalesPersonId: string
  SalesPersonCode: string
  SalesPersonName: string
  SignalKey: string
  SignalLabel: string
  ValueAmount: number | null
  ValueText: string | null
  WilayahName: string
  ReportRoute: string
  RequiresAttention: boolean
  IsActive: boolean
  Investigation?: InvestigationMetadata | null
}

export interface DashboardSalesmanRankingRow {
  Rank: number
  SalesPersonId: string
  SalesPersonCode: string
  SalesPersonName: string
  Amount: number
  PercentOfTotal: number | null
  AchievementPercent?: number | null
  TargetAmount?: number | null
  ReportRoute: string
  IsActive: boolean
  Investigation?: InvestigationMetadata | null
}

export interface SalesmanPrincipalAchievementRow {
  SupplierId: string
  SupplierName: string
  TargetAmount: number | null
  CompletedOmzet: number
  AchievementPercent: number | null
  AchievementBand: string | null
}

export interface SalesmanPrincipalAchievementResponse {
  SalesPersonId: string
  SalesPersonName: string
  PeriodYear: number
  PeriodMonth: number
  Principals: SalesmanPrincipalAchievementRow[]
}

export interface SalesmanAchievementTrendPoint {
  PeriodYear: number
  PeriodMonth: number
  PeriodLabel: string
  TargetAmount: number | null
  CompletedOmzet: number
  AchievementPercent: number | null
  AchievementBand: string | null
}

export interface SalesmanAchievementTrendResponse {
  SalesPersonId: string
  SalesPersonName: string
  Points: SalesmanAchievementTrendPoint[]
}

export interface DashboardSalesmanPerformanceRankings {
  TopOmzet: DashboardSalesmanRankingRow[]
  TopAchievement: DashboardSalesmanRankingRow[]
}

export interface DashboardSalesmanExposureRankings {
  TopPiutang: DashboardSalesmanRankingRow[]
}

export interface DashboardSalesmanSegmentRow {
  SegmentType: string
  SegmentLabel: string
  SalesmanCount: number
  ActiveCount: number
  InactiveCount: number
}

export interface DashboardSalesmanSegmentationSummary {
  ByWilayah: DashboardSalesmanSegmentRow[]
  BySegment: DashboardSalesmanSegmentRow[]
  ActiveSummary: DashboardSalesmanSegmentRow | null
  InactiveSummary: DashboardSalesmanSegmentRow | null
}

export interface DashboardSalesmanNavigationLinks {
  SalesDashboardRoute: string
  PiutangDashboardRoute: string
  SalesReportRoute: string
  PiutangReportRoute: string
}

export interface DashboardSalesmanResponse {
  IsAvailable: boolean
  IsDataFresh: boolean
  GeneratedAt: string | null
  PeriodYear: number
  PeriodMonth: number
  AttentionCards: DashboardSalesmanAttentionCards | null
  AttentionList: DashboardSalesmanAttentionItem[]
  PerformanceRankings: DashboardSalesmanPerformanceRankings | null
  ExposureRankings: DashboardSalesmanExposureRankings | null
  Segmentation: DashboardSalesmanSegmentationSummary | null
  Navigation: DashboardSalesmanNavigationLinks | null
  Filters: DashboardSalesmanFilterDefaults | null
}

export interface DashboardCollectionAttentionCards {
  OverdueExposure: number
  AgingOver90Exposure: number
  OverdueConcentrationPercent: number | null
  ExposureRequiresAttention: boolean
  CashCollectedMtd: number
  RecoveryVsBillingPercent: number | null
  RecoveryRequiresAttention: boolean
  LegacyDebtCount: number
  PortfolioRequiresAttention: boolean
}

export interface DashboardCollectionRecoverySummary {
  CashCollectedMtd: number
  RecoveryVsBillingPercent: number | null
  PaymentMixCashAmount: number
  PaymentMixGiroAmount: number
  PaymentMixAdjustmentAmount: number
  PaymentMixCashPercent: number | null
  PaymentMixGiroPercent: number | null
  PaymentMixAdjustmentPercent: number | null
}

export interface DashboardCollectionAgingBucket {
  BucketKey: string
  BucketLabel: string
  Amount: number
  SortOrder: number
}

export interface DashboardCollectionAttentionItem {
  EntityType: string
  EntityCode: string
  EntityName: string
  SignalKey: string
  SignalLabel: string
  ValueAmount: number | null
  ValueText: string | null
  WilayahName: string
  ReportRoute: string | null
  Investigation?: InvestigationMetadata | null
}

export interface DashboardCollectionRankingRow {
  Rank: number
  EntityCode: string
  EntityName: string
  Amount: number
  PercentOfTotal: number | null
  ReportRoute: string | null
  Investigation?: InvestigationMetadata | null
}

export interface DashboardCollectionNavigationLinks {
  PiutangDashboardRoute: string
  CustomerDashboardRoute: string
  SalesmanDashboardRoute: string
  PiutangReportRoute: string
}

export interface DashboardCollectionResponse {
  IsAvailable: boolean
  IsDataFresh: boolean
  GeneratedAt: string | null
  AttentionCards: DashboardCollectionAttentionCards | null
  RecoverySummary: DashboardCollectionRecoverySummary | null
  AgingRiskSummary: DashboardCollectionAgingBucket[]
  AttentionList: DashboardCollectionAttentionItem[]
  TopOverdueCustomers: DashboardCollectionRankingRow[]
  TopOverdueSalesmen: DashboardCollectionRankingRow[]
  TopOverdueWilayah: DashboardCollectionRankingRow[]
  Navigation: DashboardCollectionNavigationLinks | null
}

export interface DashboardLocationAttentionCards {
  Top1WarehouseInventoryPercent: number | null
  Top3WarehouseInventoryPercent: number | null
  Top1WarehouseAtRiskPercent: number | null
  Top1WarehouseSalesPercent: number | null
  Top1WilayahSalesPercent: number | null
  InactiveWarehouseWithStockCount: number
  WarehouseNoSalesWithInventoryCount: number
}

export interface DashboardLocationRankingRow {
  Rank: number
  EntityCode: string
  EntityName: string
  Amount: number
  PercentOfTotal: number | null
  ReportRoute: string | null
  Investigation?: InvestigationMetadata | null
}

export interface DashboardLocationWilayahRankingRow {
  Rank: number
  EntityCode: string | null
  EntityName: string
  Amount: number
  PercentOfTotal: number | null
  DashboardRoute: string | null
}

export interface DashboardLocationAttentionItem {
  EntityType: string
  EntityCode: string | null
  EntityName: string
  SignalKey: string
  SignalLabel: string
  ValueAmount: number | null
  ValueText: string | null
  ReportRoute: string | null
  Investigation?: InvestigationMetadata | null
}

export interface DashboardLocationNavigationLinks {
  InventoryDashboardRoute: string
  InventoryRiskDashboardRoute: string
  SalesDashboardRoute: string
  PurchasingDashboardRoute: string
  CollectionDashboardRoute: string
  CustomerDashboardRoute: string
  SalesmanDashboardRoute: string
}

export interface DashboardLocationResponse {
  IsAvailable: boolean
  IsDataFresh: boolean
  GeneratedAt: string | null
  AttentionCards: DashboardLocationAttentionCards | null
  TopWarehouseInventory: DashboardLocationRankingRow[]
  TopWarehouseAtRisk: DashboardLocationRankingRow[]
  TopWarehouseSales: DashboardLocationRankingRow[]
  TopWarehousePurchasing: DashboardLocationRankingRow[]
  TopWilayahSales: DashboardLocationWilayahRankingRow[]
  AttentionList: DashboardLocationAttentionItem[]
  Navigation: DashboardLocationNavigationLinks | null
}

export interface DashboardAlertCenterPlatformAlert {
  SignalKey: string
  SignalLabel: string
  ValueText: string
  DashboardRoute: string
}

export interface DashboardAlertCenterCategorySummary {
  Category: string
  TotalCount: number
  DisplayedCount: number
  HasMore: boolean
}

export interface DashboardAlertCenterAlertRow {
  Category: string
  EntityType: string
  EntityCode: string | null
  EntityName: string
  SignalKey: string
  SignalLabel: string
  ValueAmount: number | null
  ValueText: string | null
  AchievementBand: AchievementBand | null
  DashboardRoute: string
  ReportRoute: string | null
  EntityFilterQuery: string | null
  SortOrder: number
  Investigation?: InvestigationMetadata | null
}

export interface DashboardAlertCenterCategoryGroup {
  Category: string
  Alerts: DashboardAlertCenterAlertRow[]
}

export interface DashboardAlertCenterInventoryRiskSummary {
  IsAvailable: boolean
  DeadStockItemCount: number
  DeadStockValue: number
  SlowMovingItemCount: number
  SlowMovingValue: number
  NeverSoldItemCount: number
  NeverSoldValue: number
  AtRiskInventoryPercent: number | null
  DashboardRoute: string
}

export interface DashboardAlertCenterConcentrationItem {
  Label: string
  ValueText: string | null
  ValuePercent: number | null
  DashboardRoute: string
  SourceDomain: string
  SortOrder: number
}

export interface DashboardAlertCenterNavigationLinks {
  ExecutiveDashboardRoute: string
  SalesDashboardRoute: string
  PiutangDashboardRoute: string
  CustomerDashboardRoute: string
  SalesmanDashboardRoute: string
  CollectionDashboardRoute: string
  InventoryDashboardRoute: string
  InventoryRiskDashboardRoute: string
  PurchasingDashboardRoute: string
  LocationDashboardRoute: string
}

export interface DashboardAlertCenterResponse {
  IsAvailable: boolean
  IsDataFresh: boolean
  OverallHealthStatus: string
  HasUnavailableDomain: boolean
  LastRefreshed: string | null
  PlatformAlerts: DashboardAlertCenterPlatformAlert[]
  CategorySummaries: DashboardAlertCenterCategorySummary[]
  AlertGroups: DashboardAlertCenterCategoryGroup[]
  InventoryRiskSummary: DashboardAlertCenterInventoryRiskSummary
  Concentrations: DashboardAlertCenterConcentrationItem[]
  Navigation: DashboardAlertCenterNavigationLinks
}
