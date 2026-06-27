export interface EntityAnalyticsTypesResponse {
  Types: EntityAnalyticsType[]
}

export interface EntityAnalyticsType {
  EntityType: string
  DisplayName: string
  IsEnabled: boolean
  IsAvailable: boolean
  ProfileRouteTemplate: string
}

export interface EntityPerformanceProfileResponse {
  IsAvailable: boolean
  EntityType: string
  EntityId: string
  GeneratedAt: string | null
  SnapshotVersion: string
  ContractVersion: string
  Overview: ProfileOverviewSection
  KpiSummary: ProfileKpiSummarySection
  Comparison: ProfileComparisonSection
  Trend: ProfileTrendSection
  Radar: ProfileRadarSection
  Ranking: ProfileRankingSection
  Attention: ProfileAttentionSection
  RelatedEntities: ProfileRelatedEntitiesSection
  Evidence: ProfileEvidenceSection
}

export interface ProfileSectionBase {
  IsAvailable: boolean
  UnavailableReason: string | null
}

export interface ProfileOverviewSection extends ProfileSectionBase {
  EntityType: string
  EntityId: string
  EntityCode: string
  DisplayName: string
  IsActive: boolean
  GeneratedAt: string | null
  Dimensions: Record<string, string>
}

export interface ProfileKpiSummarySection extends ProfileSectionBase {
  Categories: ProfileKpiCategoryGroup[]
}

export interface ProfileKpiCategoryGroup {
  Category: string
  Kpis: KpiEnvelope[]
}

export interface KpiEnvelope {
  KpiId: string
  Category: string
  DisplayName: string
  Value: number | null
  TextValue: string | null
  FormattedValue: string
  Unit: string
  Direction: string
  PeriodLabel: string
  EvidenceRoute: string | null
  FilterDimension: string | null
  ValueType: string
  DisplayPrecision: number
  TrendEligible: boolean
  RankEligible: boolean
  NullableBehavior: string | null
}

export interface ProfileComparisonSection extends ProfileSectionBase {
  Metrics: ComparisonMetric[]
}

export interface ComparisonMetric {
  KpiId: string
  DisplayName: string
  Unit: string
  Direction: string
  CurrentValue: number | null
  CurrentFormatted: string
  CurrentPeriodLabel: string
  PriorMonthValue: number | null
  PriorMonthFormatted: string | null
  PriorMonthPeriodLabel: string | null
  MomDelta: number | null
  MomGrowthPercent: number | null
  PriorYearValue: number | null
  PriorYearFormatted: string | null
  PriorYearPeriodLabel: string | null
  YoyDelta: number | null
  YoyGrowthPercent: number | null
}

export interface EntityCompareResponse {
  EntityType: string
  ContractVersion: string
  Entities: CompareEntityColumn[]
  KpiComparison: CompareKpiSection
  TrendComparison: CompareTrendSection
  RankingComparison: CompareRankingSection
  AttentionComparison: CompareAttentionSection
  RelationshipComparison: CompareRelationshipSection
  RadarComparison: CompareRadarSection
  PeerComparison: ComparePeerSection
  Warnings: string[]
}

export interface CompareEntityColumn {
  EntityType: string
  EntityId: string
  EntityCode: string
  DisplayName: string
  IsActive: boolean
  GeneratedAt: string | null
  ProfileRoute: string
}

export interface CompareKpiSection extends ProfileSectionBase {
  Rows: CompareKpiRow[]
}

export interface CompareKpiRow {
  KpiId: string
  DisplayName: string
  Unit: string
  Direction: string
  Values: CompareKpiCell[]
}

export interface CompareKpiCell {
  EntityCode: string
  DisplayName: string
  Value: number | null
  FormattedValue: string
  PeriodLabel: string
}

export interface CompareTrendSection extends ProfileSectionBase {
  Overlays: CompareTrendOverlay[]
}

export interface CompareTrendOverlay {
  KpiId: string
  DisplayName: string
  Unit: string
  EntitySeries: CompareTrendEntitySeries[]
}

export interface CompareTrendEntitySeries {
  EntityCode: string
  DisplayName: string
  Points: ProfileTrendPoint[]
}

export interface CompareRankingSection extends ProfileSectionBase {
  Entities: CompareRankingEntity[]
}

export interface CompareRankingEntity {
  EntityCode: string
  DisplayName: string
  Ranking: ProfileRankingSection
}

export interface CompareRankingOverlay {
  KpiId: string
  DisplayName: string
  Unit: string
  RankingDirection: string
  EntitySeries: CompareRankingEntitySeries[]
}

export interface CompareRankingEntitySeries {
  EntityCode: string
  DisplayName: string
  CurrentRank: number | null
  CurrentPercentile: number | null
  CurrentPopulationSize: number | null
  BestRank: number | null
  WorstRank: number | null
  Points: ProfileRankingPoint[]
}

export interface CompareAttentionSection extends ProfileSectionBase {
  Entities: CompareAttentionEntity[]
}

export interface CompareAttentionEntity {
  EntityCode: string
  DisplayName: string
  Attention: ProfileAttentionSection
}

export interface CompareRelationshipSection extends ProfileSectionBase {
  Entities: CompareRelationshipEntity[]
}

export interface CompareRelationshipEntity {
  EntityCode: string
  DisplayName: string
  RelatedEntities: ProfileRelatedEntitiesSection
}

export interface CompareRadarSection extends ProfileSectionBase {
  PeerGroupRuleId?: string | null
  PeerGroupSize?: number | null
  PeriodYear?: number
  PeriodMonth?: number
  PeriodLabel?: string | null
  Axes: CompareRadarAxis[]
  Overlays: CompareRadarOverlay[]
  PeerAverageScores?: Array<number | null> | null
}

export interface CompareRadarAxis {
  KpiId: string
  SignatureDimensionKey?: string | null
  DisplayName: string
  Direction?: string | null
}

export interface CompareRadarOverlay {
  EntityCode: string
  DisplayName: string
  Scores: Array<number | null>
}

export interface ComparePeerSection extends ProfileSectionBase {
  EntityCode?: string | null
  DisplayName?: string | null
  PeerGroupRuleId?: string | null
  PeerGroupSize?: number | null
  Radar?: ProfileRadarSection | null
}

export interface EntityAnalyticsSearchResponse {
  EntityType: string
  Results: EntitySearchResult[]
}

export interface EntitySearchResult {
  EntityType: string
  EntityId: string
  EntityCode: string
  DisplayName: string
  IsActive: boolean
  ProfileRoute: string
}

export interface EntityCompareQuery {
  entityType: string
  entityIds: string[]
  kpiIds?: string[]
  periodYear?: number
  periodMonth?: number
}

export interface ProfileTrendSection extends ProfileSectionBase {
  Series: ProfileTrendSeries[]
}

export interface ProfileTrendSeries {
  KpiId: string
  DisplayName: string
  PeriodSemantics: string
  Unit: string
  Points: ProfileTrendPoint[]
}

export interface ProfileTrendPoint {
  PeriodYear: number
  PeriodMonth: number
  Value: number | null
  IsClosed: boolean
  PeriodLabel: string
}

export interface ProfileRadarSection extends ProfileSectionBase {
  PeerGroupRuleId?: string | null
  PeerGroupSize?: number | null
  PeriodYear?: number
  PeriodMonth?: number
  PeriodLabel?: string | null
  UnavailableExplanation?: string | null
  Axes: ProfileRadarAxis[]
  PeerAverageScores?: Array<number | null> | null
}

export interface ProfileRadarAxis {
  KpiId: string
  SignatureDimensionKey?: string | null
  DisplayName: string
  Score: number | null
  Direction?: string | null
}

export interface ProfileRankingSection extends ProfileSectionBase {
  Series: ProfileRankingSeries[]
}

export interface ProfileRankingSeries {
  KpiId: string
  DisplayName: string
  RankingDirection: string
  Unit: string
  CurrentRank: number | null
  BestRank: number | null
  WorstRank: number | null
  CurrentPercentile: number | null
  CurrentPopulationSize: number | null
  Points: ProfileRankingPoint[]
}

export interface ProfileRankingPoint {
  PeriodYear: number
  PeriodMonth: number
  PeriodLabel: string
  RankPosition: number
  PopulationSize: number
  Percentile: number
}

export interface ProfileAttentionSection extends ProfileSectionBase {
  ActiveSignalCount: number
  HistoricalSignalCount: number
  Events: ProfileAttentionEvent[]
}

export interface ProfileAttentionEvent {
  SignalCode: string
  SignalLabel: string
  SignalCategory: string
  IsActive: boolean
  FirstSeenPeriodYear: number | null
  FirstSeenPeriodMonth: number | null
  LastSeenPeriodYear: number | null
  LastSeenPeriodMonth: number | null
  FirstSeen: string | null
  LastSeen: string | null
  ConsecutivePeriods: number
  TotalOccurrences: number
}

export interface ProfileRelatedEntitiesSection extends ProfileSectionBase {
  Blocks: ProfileRelationshipBlock[]
}

export interface ProfileRelationshipBlock {
  RelationshipCode: string
  RelationshipLabel: string
  DisplayName: string
  TargetEntityType: string
  Rows: ProfileRelatedEntityRow[]
}

export interface ProfileRelatedEntityRow {
  Rank: number
  EntityId: string
  EntityCode: string
  DisplayName: string
  TargetEntityType: string
  TargetEntityCode: string
  TargetEntityName: string
  MetricValue: number | null
  ProfileRoute: string
}

export interface ProfileEvidenceSection extends ProfileSectionBase {
  Links: ProfileEvidenceLink[]
}

export interface ProfileEvidenceLink {
  Category: string
  Label: string
  ReportRoute: string
  FilterDimension: string
}
