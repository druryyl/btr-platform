export interface InvestigationSuggestedQuery {
  FreeText?: string
  CustomerId?: string
  CustomerCode?: string
  SalesmanId?: string
  BrgId?: string
  WarehouseId?: string
  SupplierId?: string
  PeriodMode?: 'currentMonth' | 'allOpenBalances'
  PostingFilter?: string
}

export interface InvestigationStep {
  Order: number
  Label: string
  ReportRoute?: string | null
  DashboardRoute?: string | null
  SuggestedQuery?: InvestigationSuggestedQuery | null
}

export interface InvestigationMetadata {
  SignalKey: string
  SignalLabel: string
  EntityType: string
  EntityId: string
  EntityName: string
  DashboardRoute?: string | null
  ReportRoute?: string | null
  SuggestedQuery?: InvestigationSuggestedQuery | null
  InvestigationSteps?: InvestigationStep[] | null
  DesktopNextStep?: string | null
}

export interface InvestigationBreadcrumbContext {
  signalKey?: string
  signalLabel?: string
  source?: string
  entityType?: string
  entityName?: string
  dashboardRoute?: string
  desktopNextStep?: string
  investigationSteps?: InvestigationStep[]
}

export interface InvestigationQueryParams {
  [key: string]: string | undefined
  q?: string
  customerId?: string
  customerCode?: string
  salesmanId?: string
  brgId?: string
  warehouseId?: string
  supplierId?: string
  periodMode?: string
  posting?: string
  signalKey?: string
  signalLabel?: string
  source?: string
  entityType?: string
}
