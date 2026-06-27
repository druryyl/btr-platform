export interface FieldActivityKpis {
  PlannedVisits: number
  ActualVisits: number
  EffectiveCalls: number
  MissedVisits: number
  UnplannedVisits: number
  VisitExecutionPercent: number | null
  EffectiveCallRate: number | null
  CoordinateCoveragePercent: number | null
  GpsValidCount: number
  GpsWarningCount: number
  GpsSuspiciousCount: number
}

export interface FieldActivityPlannedStop {
  CustomerId: string
  CustomerCode: string
  CustomerName: string
  NoUrut: number
  Latitude: number
  Longitude: number
  HasCoordinates: boolean
  VisitStatus: string
}

export interface FieldActivityActualStop {
  CustomerId: string
  CustomerCode: string
  CustomerName: string
  Sequence: number
  CheckInTime: string
  Latitude: number
  Longitude: number
  HasCoordinates: boolean
  VisitStatus: string
  IsEffectiveCall: boolean
  GpsValidation: string
  DistanceMeters: number | null
}

export interface FieldActivityMissedVisit {
  CustomerId: string
  CustomerCode: string
  CustomerName: string
  NoUrut: number
  HasCoordinates: boolean
}

export interface FieldActivityGeoJsonLine {
  Type: string
  Coordinates: number[][]
}

export interface FieldActivityRouteGeometry {
  Planned: FieldActivityGeoJsonLine
  Actual: FieldActivityGeoJsonLine
}

export interface FieldActivityMeta {
  PlanDataAvailable: boolean
  VisitPlanGoLiveDate: string
  QueriedAt: string
}

export interface FieldActivityResponse {
  SalesPersonId: string
  SalesPersonName: string
  VisitDate: string
  Kpis: FieldActivityKpis
  PlannedStops: FieldActivityPlannedStop[]
  ActualStops: FieldActivityActualStop[]
  MissedVisits: FieldActivityMissedVisit[]
  RouteGeometry: FieldActivityRouteGeometry
  Meta: FieldActivityMeta
}

export interface FieldActivitySalesmanItem {
  SalesPersonId: string
  SalesPersonName: string
  SalesPersonCode: string
  Email: string
  HasEmail: boolean
}

export interface FieldActivitySalesmenResponse {
  Items: FieldActivitySalesmanItem[]
}

export type GpsValidationLevel = 'Valid' | 'Warning' | 'Suspicious' | 'Invalid'

export interface FieldActivityTeamKpis {
  ActiveSalesmenCount: number
  PlannedVisits: number
  ActualVisits: number
  VisitExecutionPercent: number | null
  EffectiveCalls: number
  EffectiveCallRate: number | null
  MissedVisits: number
  UnplannedVisits: number
  GpsValidRate: number | null
  TotalOrders: number
  TotalOmzet: number
}

export interface FieldActivitySalesmanOverviewRow {
  SalesPersonId: string
  SalesPersonCode: string
  SalesPersonName: string
  WilayahName: string
  HasEmail: boolean
  Rank: number
  PlannedVisits: number
  ActualVisits: number
  VisitExecutionPercent: number | null
  EffectiveCalls: number
  EffectiveCallRate: number | null
  MissedVisits: number
  UnplannedVisits: number
  GpsValidPercent: number | null
  OrdersCount: number
  OmzetAmount: number
  StatusCode: string
}

export interface FieldActivityRankingEntry {
  Rank: number
  SalesPersonId: string
  SalesPersonCode: string
  SalesPersonName: string
  PrimaryValue: number | null
  PrimaryLabel: string
}

export interface FieldActivityRankingSection {
  TopVisitExecution: FieldActivityRankingEntry[]
  BottomVisitExecution: FieldActivityRankingEntry[]
  TopEffectiveCallRate: FieldActivityRankingEntry[]
  BottomEffectiveCallRate: FieldActivityRankingEntry[]
  TopOmzet: FieldActivityRankingEntry[]
  TopOrders: FieldActivityRankingEntry[]
  MostMissedVisits: FieldActivityRankingEntry[]
  MostUnplannedVisits: FieldActivityRankingEntry[]
}

export interface FieldActivityTrendPoint {
  TrendDate: string
  VisitExecutionPercent: number | null
  EffectiveCallRate: number | null
  OrdersCount: number
  OmzetAmount: number
}

export interface FieldActivityTrendSection {
  Last7Days: FieldActivityTrendPoint[]
  Last30Days: FieldActivityTrendPoint[]
}

export interface FieldActivityWilayahBreakdownRow {
  WilayahName: string
  ActualVisits: number
}

export interface FieldActivityOverviewMeta {
  PlanDataAvailable: boolean
  VisitPlanGoLiveDate: string
}

export interface FieldActivityOverviewResponse {
  VisitDate: string
  DataSource: string
  GeneratedAt: string | null
  QueriedAt: string | null
  TeamKpis: FieldActivityTeamKpis
  Salesmen: FieldActivitySalesmanOverviewRow[]
  Rankings: FieldActivityRankingSection
  Trends: FieldActivityTrendSection
  WilayahBreakdown: FieldActivityWilayahBreakdownRow[]
  Meta: FieldActivityOverviewMeta
}
