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
