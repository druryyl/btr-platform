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
  Portfolio?: DashboardExecutivePortfolioAttention | null
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

export interface DashboardSalesForecastVsTarget {
  TargetAmount: number
  CurrentAmount: number
  ForecastAmount: number
}

export interface DashboardSalesDailyPaceItem {
  PaceDate: string
  DayOfMonth: number
  IsElapsed: boolean
  ActualAmount: number
  ProjectedDailyAmount: number
}

export type ForecastConfidence = 'Low' | 'Medium' | 'High'

export type RequiredDailySeverity = 'Normal' | 'Warning' | 'Critical'

export interface DashboardSalesForecastResponse {
  GeneratedAt: string
  PeriodYear: number
  PeriodMonth: number
  BusinessDate: string
  DaysInMonth: number
  DaysElapsed: number
  DaysRemaining: number
  CurrentSales: number
  TotalTarget: number
  CurrentAchievementPercent: number | null
  DailyAverageSales: number
  ForecastSales: number
  ForecastAchievementPercent: number | null
  RequiredDailySales: number | null
  TargetGap: number
  ForecastVariance: number
  BestCaseSales: number
  WorstCaseSales: number
  ForecastConfidence: ForecastConfidence | string
  ForecastRiskBand: AchievementBand | string
  RequiredDailySeverity: RequiredDailySeverity | string
  ExecutiveSummary: string
  ForecastVsTarget: DashboardSalesForecastVsTarget
  DailyPace: DashboardSalesDailyPaceItem[]
  WeeklyTrend: DashboardSalesWeekTrendItem[]
}

export interface DashboardCashFlowDailyPaceItem {
  PaceDate: string
  DayOfMonth: number
  IsElapsed: boolean
  ActualCashAmount: number
  ActualCollectionAmount: number
  ProjectedDailyCashAmount: number
}

export interface DashboardCashFlowRecoveryTrendItem {
  TrendDate: string
  DayOfMonth: number
  IsElapsed: boolean
  CumulativeCollections: number
  CumulativeBilling: number
}

export interface DashboardCashFlowCollectionRiskItem {
  SortOrder: number
  RiskKey: string
  RiskLabel: string
  EntityType: string
  EntityId: string
  EntityName: string
  Amount: number
  DueOrAgingText: string
  RuleExplanation: string
  ReportRoute: string
}

export interface DashboardCashFlowForecastResponse {
  IsAvailable: boolean
  GeneratedAt: string
  PeriodYear: number
  PeriodMonth: number
  BusinessDate: string
  DaysInMonth: number
  DaysElapsed: number
  DaysRemaining: number
  CashCollectedMtd: number
  MonthCollections: number
  MonthFakturOmzet: number
  DailyCashCollectionAverage: number
  DailyCollectionAverage: number
  ExpectedCashCollection: number
  ProjectedMonthEndTotalCollections: number
  CollectionForecastPercent: number | null
  RecoveryVsBillingPercent: number | null
  RecoveryVsBillingForecastPercent: number | null
  RemainingCollectionTarget: number
  RequiredDailyCollection: number | null
  OutstandingDueRemaining: number
  OverdueOutstanding: number
  CollectionGap: number
  ForecastVarianceCash: number
  ExpectedCollectionRatePercent: number | null
  BestCaseCash: number
  WorstCaseCash: number
  ForecastConfidence: ForecastConfidence | string
  ForecastRiskBand: AchievementBand | string
  RequiredDailySeverity: RequiredDailySeverity | string
  ExecutiveSummary: string
  DailyPace: DashboardCashFlowDailyPaceItem[]
  RecoveryTrend: DashboardCashFlowRecoveryTrendItem[]
  CollectionRisks: DashboardCashFlowCollectionRiskItem[]
}

export interface DashboardInventoryForecastDailyConsumptionItem {
  ConsumptionDate: string
  DayIndex: number
  UnitsSold: number
  AdcReference: number
}

export interface DashboardInventoryForecastLevelItem {
  HorizonDay: number
  ProjectedInventoryValue: number
}

export interface DashboardInventoryForecastHeatCellItem {
  DosBand: string
  ValueBand: string
  ItemCount: number
}

export interface DashboardInventoryForecastRiskItem {
  SortOrder: number
  SignalKey: string
  SignalLabel: string
  BrgId: string
  BrgCode: string
  BrgName: string
  SupplierName: string
  DaysOfSupply: number | null
  StockOutDate: string | null
  ValueAmount: number
  Urgency: string
  DosSeverity: string
  RuleExplanation: string
  ReportRoute: string
  EntityCode: string
}

export interface DashboardInventoryForecastRecommendationItem {
  SortOrder: number
  BrgId: string
  BrgCode: string
  BrgName: string
  SupplierName: string
  ReorderDate: string | null
  RecommendedPurchaseQty: number
  AverageDailyConsumption: number
  CurrentQty: number
  DaysOfSupply: number | null
  Urgency: string
  ReportRoute: string
  EntityCode: string
}

export interface DashboardInventoryForecastTraceability {
  InventoryDashboardRoute: string
  InventoryRiskDashboardRoute: string
  InventoryReportRoute: string
  PurchasingManagementRoute: string
  Disclaimer: string
}

export interface DashboardInventoryForecastResponse {
  IsAvailable: boolean
  GeneratedAt: string
  BusinessDate: string
  PlanningHorizonDays: number
  CurrentInventoryValue: number
  ProjectedInventoryValue: number
  BestCaseProjectedValue: number
  WorstCaseProjectedValue: number
  AverageDailyConsumptionUnits: number
  WeightedAverageDaysOfSupply: number | null
  UnderstockValue: number
  OverstockValue: number
  StockOutRiskItemCount: number
  InventoryCoveragePercent: number | null
  InventoryTurnoverForecast: number | null
  InventoryHealthScore: number
  ForecastConfidence: ForecastConfidence | string
  AtRiskInventoryPercent: number | null
  ForecastConsumptionUnits: number
  ExecutiveSummary: string
  Traceability: DashboardInventoryForecastTraceability
  DailyConsumption: DashboardInventoryForecastDailyConsumptionItem[]
  ProjectedLevel: DashboardInventoryForecastLevelItem[]
  HeatSummary: DashboardInventoryForecastHeatCellItem[]
  TopRisks: DashboardInventoryForecastRiskItem[]
  PurchaseRecommendations: DashboardInventoryForecastRecommendationItem[]
}

export interface DashboardInventoryOptimizationTraceability {
  InventoryForecastRoute: string
  InventoryRiskRoute: string
  PurchasingManagementRoute: string
  InventoryReportRoute: string
  PurchasingReportRoute: string
  Disclaimer: string
}

export interface DashboardInventoryOptimizationPriorityDistItem {
  Category: string
  ActionCount: number
  SortOrder: number
}

export interface DashboardInventoryOptimizationActionHeatItem {
  ActionType: string
  ActionLabel: string
  Category: string
  ActionCount: number
}

export interface DashboardInventoryOptimizationActionItem {
  SortOrder: number
  PriorityScore: number
  Category: string
  ActionType: string
  ActionLabel: string
  BrgId: string
  BrgName: string
  SupplierName: string
  WarehouseFromName?: string | null
  WarehouseToName?: string | null
  Quantity?: number | null
  ImpactValueIdr: number
  DaysOfSupply?: number | null
  ReasonText: string
  RuleId: string
  ReportRoute: string
  DrillDownRoute: string
}

export interface DashboardInventoryOptimizationReorderItem {
  SortOrder: number
  PriorityScore: number
  Category: string
  BrgId: string
  BrgCode: string
  BrgName: string
  SupplierName: string
  RecommendedPurchaseQty: number
  EstimatedCostIdr: number
  DaysOfSupply?: number | null
  ReorderDate?: string | null
  ReasonText: string
  RuleId: string
  ReportRoute: string
  DrillDownRoute: string
}

export interface DashboardInventoryOptimizationTransferItem {
  SortOrder: number
  PriorityScore: number
  Category: string
  BrgId: string
  BrgName: string
  WarehouseFromName: string
  WarehouseToName: string
  TransferQty: number
  DestDaysOfSupply?: number | null
  ReasonText: string
  RuleId: string
  ReportRoute: string
  DrillDownRoute: string
}

export interface DashboardInventoryOptimizationDelayItem {
  SortOrder: number
  PriorityScore: number
  Category: string
  ActionType: string
  ActionLabel: string
  BrgId: string
  BrgName: string
  SupplierName: string
  DaysOfSupply?: number | null
  MovementClass: string
  SuggestedQty?: number | null
  ReasonText: string
  RuleId: string
  ReportRoute: string
  DrillDownRoute: string
}

export interface DashboardInventoryOptimizationClearanceItem {
  SortOrder: number
  PriorityScore: number
  Category: string
  BrgId: string
  BrgName: string
  InventoryValueIdr: number
  IdleDays?: number | null
  RecommendedAction: string
  ReasonText: string
  RuleId: string
  ReportRoute: string
  DrillDownRoute: string
}

export interface DashboardInventoryOptimizationResponse {
  IsAvailable: boolean
  GeneratedAt: string
  BusinessDate: string
  PlanningHorizonDays: number
  BudgetCapIdr?: number | null
  InventoryHealthScore: number
  CriticalActionCount: number
  HighActionCount: number
  MediumActionCount: number
  LowActionCount: number
  PurchaseNowCount: number
  DelayCount: number
  TransferCount: number
  ClearanceCount: number
  PostFirstCount: number
  DeferCount: number
  RequiredPurchaseBudgetIdr: number
  RecommendedPurchaseBudgetIdr: number
  DeferrableSpendIdr: number
  RecoverableCapitalIdr: number
  PurchaseImpactIdr: number
  DelayImpactIdr: number
  TransferSavingsIdr: number
  ExecutiveSummary: string
  Traceability: DashboardInventoryOptimizationTraceability
  PriorityDistribution: DashboardInventoryOptimizationPriorityDistItem[]
  ActionHeatSummary: DashboardInventoryOptimizationActionHeatItem[]
  TopActions: DashboardInventoryOptimizationActionItem[]
  ReorderList: DashboardInventoryOptimizationReorderItem[]
  TransferList: DashboardInventoryOptimizationTransferItem[]
  DelayList: DashboardInventoryOptimizationDelayItem[]
  ClearanceList: DashboardInventoryOptimizationClearanceItem[]
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
  ProfileRoute?: string | null
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
  ProfileRoute?: string | null
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
  ProfileRoute?: string | null
  Investigation?: InvestigationMetadata | null
}

export interface DashboardPurchasingPrincipalExposureItem {
  Rank: number
  SupplierCode?: string | null
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
  ProfileRoute?: string | null
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
  ProfileRoute?: string | null
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
  ProfileRoute?: string | null
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
  ProfileRoute?: string | null
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
  ProfileRoute?: string | null
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

export interface PortalMenuLinkDto {
  Code: string
  Label: string
  Route: string
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
  DomainDashboards: PortalMenuLinkDto[]
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

export type CustomerRiskForecastCategory =
  | 'Healthy'
  | 'Watch'
  | 'Attention'
  | 'HighRisk'
  | 'Critical'

export interface DashboardCustomerRiskForecastKpi {
  HorizonDays: number
  CustomersForecastedAtRisk: number
  HighRiskCustomerCount: number
  CriticalCustomerCount: number
  ElevatedRiskReceivable: number
  ElevatedRiskReceivablePercent: number | null
  PortfolioHealthScore: number
  TotalPiutang: number
  ForecastConfidence: ForecastConfidence | string
  PaymentDelaySignalCount: number
  CreditLimitSignalCount: number
  InactivitySignalCount: number
  PurchaseDeclineSignalCount: number
  CollectionRiskSignalCount: number
  HealthyCount: number
  WatchCount: number
  AttentionCount: number
  HighRiskCount: number
  CriticalCount: number
  ExecutiveSummaryText: string
}

export interface DashboardCustomerRiskForecastDistItem {
  Category: string
  CategoryLabel: string
  CustomerCount: number
  SortOrder: number
}

export interface DashboardCustomerRiskForecastWilayahItem {
  WilayahName: string
  ElevatedRiskCustomerCount: number
  SortOrder: number
}

export interface DashboardCustomerRiskForecastSignalMixItem {
  SignalFamilyKey: string
  SignalFamilyLabel: string
  CustomerCount: number
  SortOrder: number
}

export interface DashboardCustomerRiskForecastCustomerItem {
  SortOrder: number
  RiskPriorityScore: number
  Category: string
  CategoryLabel: string
  CustomerCode: string
  CustomerName: string
  WilayahName: string
  SalesPersonName: string
  OpenBalance: number
  OverdueBalance: number
  DueWithinHorizon: number
  Plafond: number
  ProjectedOpenBalance: number
  MtdOmzet: number
  PriorMonthOmzet: number
  DeclineRatio: number | null
  DaysSinceLastFaktur: number | null
  AvgPaymentLagDays: number | null
  PrimarySignalKey: string
  PrimarySignalLabel: string
  ReasonText: string
  RecommendationKey: string
  RecommendationLabel: string
  ReportRoute: string
  DrillDownRoute: string
}

export interface DashboardCustomerRiskForecastAttentionItem {
  SortOrder: number
  CustomerCode: string
  CustomerName: string
  SignalKey: string
  SignalLabel: string
  Severity: string
  Amount: number | null
  HorizonText: string
  RuleId: string
  Explanation: string
  ReportRoute: string
}

export interface DashboardCustomerRiskForecastRecommendationItem {
  SortOrder: number
  RecommendationKey: string
  RecommendationLabel: string
  CustomerCode: string
  CustomerName: string
  Category: string
  ReasonText: string
  RuleId: string
  ReportRoute: string
  DrillDownRoute: string
}

export interface DashboardCustomerRiskForecastResponse {
  IsAvailable: boolean
  GeneratedAt: string
  BusinessDate: string
  Kpi: DashboardCustomerRiskForecastKpi | null
  CategoryDistribution: DashboardCustomerRiskForecastDistItem[]
  TopWilayah: DashboardCustomerRiskForecastWilayahItem[]
  SignalMix: DashboardCustomerRiskForecastSignalMixItem[]
  TopCustomers: DashboardCustomerRiskForecastCustomerItem[]
  AttentionList: DashboardCustomerRiskForecastAttentionItem[]
  Recommendations: DashboardCustomerRiskForecastRecommendationItem[]
}

export interface DashboardCollectionOptimizationKpi {
  ActionsTodayCount: number
  ImmediateCollectionCount: number
  ProactiveReminderCount: number
  CreditReviewCount: number
  SalesRecoveryCount: number
  EscalateManagementCount: number
  CollectionImpactTotal: number
  ImmediateImpactTotal: number
  OverdueExposure: number
  DueWithin7Days: number
  RecoveryVsBillingPercent: number | null
  DeferNoActionCount: number
  PlanningConfidence: string
  ExecutiveSummaryText: string
}

export interface DashboardCollectionOptimizationActionDistItem {
  ActionCategoryKey: string
  ActionCategoryLabel: string
  CustomerCount: number
  ImpactTotal: number
  SortOrder: number
}

export interface DashboardCollectionOptimizationWorkloadItem {
  WorkloadType: string
  EntityKey: string
  EntityLabel: string
  ActionCount: number
  ImmediateCount: number
  ImpactTotal: number
  OverdueExposure: number
  IsHotspot: boolean
  SortOrder: number
}

export interface DashboardCollectionOptimizationPriorityItem {
  SortOrder: number
  CollectionPriorityScore: number
  CustomerCode: string
  CustomerName: string
  WilayahName: string
  SalesPersonName: string
  Klasifikasi: string
  ActionCategoryKey: string
  ActionCategoryLabel: string
  RecommendedActionKey: string
  RecommendedActionLabel: string
  ActionOwner: string
  OpenBalance: number
  OverdueBalance: number
  DueWithin7Days: number
  CollectionImpactAmount: number
  M29Category: string
  M29RecommendationKey: string
  M29PrimarySignalKey: string
  MinDaysUntilDue: number | null
  CreditUtilizationPercent: number | null
  SelectionReasonText: string
  PriorityReasonText: string
  ActionReasonText: string
  TriggeredRuleIds: string
  ReportRoute: string
  DrillDownRoute: string
}

export interface DashboardCollectionOptimizationQueueItem {
  QueueKey: string
  SortOrder: number
  CollectionPriorityScore: number
  CustomerCode: string
  CustomerName: string
  WilayahName: string
  SalesPersonName: string
  ActionCategoryKey: string
  ActionCategoryLabel: string
  RecommendedActionKey: string
  RecommendedActionLabel: string
  ActionOwner: string
  OverdueBalance: number
  DueWithin7Days: number
  CollectionImpactAmount: number
  M29Category: string
  QueueReasonText: string
  ReportRoute: string
  DrillDownRoute: string
}

export interface DashboardCollectionOptimizationImpactItem {
  SortOrder: number
  CustomerCode: string
  CustomerName: string
  WilayahName: string
  SalesPersonName: string
  ActionCategoryKey: string
  ActionCategoryLabel: string
  CollectionImpactAmount: number
  OverdueBalance: number
  DueWithin7Days: number
  ReportRoute: string
  DrillDownRoute: string
}

export interface DashboardCollectionOptimizationResponse {
  IsAvailable: boolean
  GeneratedAt: string
  BusinessDate: string
  Kpi: DashboardCollectionOptimizationKpi | null
  ActionDistribution: DashboardCollectionOptimizationActionDistItem[]
  Workload: DashboardCollectionOptimizationWorkloadItem[]
  PriorityQueue: DashboardCollectionOptimizationPriorityItem[]
  SpecializedQueues: DashboardCollectionOptimizationQueueItem[]
  TopImpactOpportunities: DashboardCollectionOptimizationImpactItem[]
}

export interface DashboardExecutivePortfolioAttention {
  IsAvailable: boolean
  PortfolioHealthyPercent: number | null
  CustomersAtRiskCount: number
  StrategicCustomersAtRiskCount: number
  DashboardRoute: string
}

export interface DashboardCustomerPortfolioKpiSnapshot {
  PortfolioHealthScore: number
  PortfolioHealthyPercent: number
  TotalCustomerCount: number
  AttentionCustomerCount: number
  StrategicCustomerCount: number
  StrategicAtRiskCount: number
  CustomersAtRiskCount: number
  WorkingCapitalTiedAmount: number
  TotalMtdOmzet: number
  TotalOpenBalance: number
  NeverPurchasedCount: number
  DormantCount: number
  DecliningCount: number
  ExecutiveSummaryText: string
  ValueDisclaimerText: string
}

export interface DashboardCustomerPortfolioLifecycleDistItem {
  LifecycleStage: string
  LifecycleLabel: string
  CustomerCount: number
  SortOrder: number
}

export interface DashboardCustomerPortfolioTierDistItem {
  PortfolioTier: string
  TierLabel: string
  CustomerCount: number
  SortOrder: number
}

export interface DashboardCustomerPortfolioActionDistItem {
  PrimaryActionKey: string
  PrimaryActionLabel: string
  CustomerCount: number
  SortOrder: number
}

export interface DashboardCustomerPortfolioPriorityRow {
  SortOrder: number
  PortfolioPriorityScore: number
  CustomerKey: string
  CustomerCode: string
  CustomerName: string
  WilayahName: string
  Klasifikasi: string
  LifecycleStage: string
  LifecycleLabel: string
  PortfolioTier: string
  TierLabel: string
  PrimaryActionKey: string
  PrimaryActionLabel: string
  ActionOwner: string
  ActionReasonText: string
  TriggeredRuleIds: string
  MtdOmzet: number
  OpenBalance: number
  OverdueBalance: number | null
  M29Category: string
  SalesPersonName: string
  SalesmanAchievementPercent: number | null
  SalesmanHighPiutangExposure: boolean
  IsAttention: boolean
  M30LinkRoute: string
  CustomerReportRoute: string
  DrillDownRouteM17: string
  DrillDownRouteM29: string
  ProfileRoute?: string | null
}

export interface DashboardCustomerPortfolioCustomerRow {
  SortOrder: number
  CustomerKey: string
  CustomerCode: string
  CustomerName: string
  WilayahName: string
  Klasifikasi: string
  LifecycleStage: string
  LifecycleLabel: string
  PortfolioTier: string
  TierLabel: string
  PrimaryActionKey: string
  PrimaryActionLabel: string
  ActionOwner: string
  ActionReasonText: string
  TriggeredRuleIds: string
  MtdOmzet: number
  OpenBalance: number
  OverdueBalance: number | null
  FakturCount6Mo: number
  IsActiveMtd: boolean
  LastPurchaseDate: string | null
  FirstPurchaseDate: string | null
  M29Category: string
  M29PrimarySignalKey: string
  SalesPersonName: string
  SalesmanAchievementPercent: number | null
  SalesmanHighPiutangExposure: boolean
  IsAttention: boolean
  PortfolioPriorityScore: number
  M30LinkRoute: string
  CustomerReportRoute: string
  DrillDownRouteM17: string
  DrillDownRouteM29: string
  ValueDisclaimer: string
}

export interface DashboardCustomerPortfolioConcentrationRow {
  SortOrder: number
  Rank: number
  CustomerCode: string
  CustomerName: string
  Amount: number
  PercentOfTotal: number | null
}

export interface DashboardCustomerPortfolioWilayahRow {
  SortOrder: number
  WilayahName: string
  CustomerCount: number
  AttentionCustomerCount: number
}

export interface DashboardCustomerPortfolioResponse {
  IsAvailable: boolean
  GeneratedAt: string
  BusinessDate: string
  Kpi: DashboardCustomerPortfolioKpiSnapshot | null
  LifecycleDistribution: DashboardCustomerPortfolioLifecycleDistItem[]
  TierDistribution: DashboardCustomerPortfolioTierDistItem[]
  ActionDistribution: DashboardCustomerPortfolioActionDistItem[]
  PriorityQueue: DashboardCustomerPortfolioPriorityRow[]
  Customers: DashboardCustomerPortfolioCustomerRow[]
  TopOmzet: DashboardCustomerPortfolioConcentrationRow[]
  TopPiutang: DashboardCustomerPortfolioConcentrationRow[]
  WilayahBreakdown: DashboardCustomerPortfolioWilayahRow[]
}
