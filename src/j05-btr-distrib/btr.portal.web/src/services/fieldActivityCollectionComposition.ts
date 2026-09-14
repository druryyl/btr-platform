import type {
  DashboardCollectionAgingBucket,
  DashboardCollectionAttentionCards,
  DashboardCollectionRankingRow,
} from '@/models/dashboard'
import type { InvestigationMetadata } from '@/models/investigation'

export interface SalesmanCollectionJoin<T> {
  salesman: T
  overdue: DashboardCollectionRankingRow | null
}

export function joinSalesmenToCollection<T extends { SalesPersonId: string }>(
  salesmen: ReadonlyArray<T>,
  topOverdueSalesmen: ReadonlyArray<DashboardCollectionRankingRow>,
): SalesmanCollectionJoin<T>[] {
  const overdueBySalesPersonId = new Map<string, DashboardCollectionRankingRow>()

  for (const overdue of topOverdueSalesmen) {
    const entityId = overdue.Investigation?.EntityId
    if (entityId && !overdueBySalesPersonId.has(entityId)) {
      overdueBySalesPersonId.set(entityId, overdue)
    }
  }

  return salesmen.map((salesman) => ({
    salesman,
    overdue: overdueBySalesPersonId.get(salesman.SalesPersonId) ?? null,
  }))
}

export interface TerritoryFinancialHealth {
  wilayahName: string
  overdueExposure: number
  overdueConcentrationPercent: number | null
}

export interface AgingRiskSummary {
  agingOver90Exposure: number
  buckets: DashboardCollectionAgingBucket[]
}

export interface TerritoryFinancialHealthSummary {
  territories: TerritoryFinancialHealth[]
  agingRisk: AgingRiskSummary
}

export function territoryFinancialHealth(
  topOverdueWilayah: ReadonlyArray<DashboardCollectionRankingRow>,
  attentionCards: DashboardCollectionAttentionCards | null,
  agingRiskSummary: ReadonlyArray<DashboardCollectionAgingBucket> = [],
): TerritoryFinancialHealthSummary {
  return {
    territories: topOverdueWilayah.map((row) => ({
      wilayahName: row.EntityName,
      overdueExposure: row.Amount,
      overdueConcentrationPercent: row.PercentOfTotal,
    })),
    agingRisk: {
      agingOver90Exposure: attentionCards?.AgingOver90Exposure ?? 0,
      buckets: [...agingRiskSummary],
    },
  }
}

export interface OverdueDistributionItem {
  wilayahName: string
  amount: number
  percentOfTotal: number | null
}

export function overdueDistribution(
  topOverdueWilayah: ReadonlyArray<DashboardCollectionRankingRow>,
): OverdueDistributionItem[] {
  return topOverdueWilayah.map((row) => ({
    wilayahName: row.EntityName,
    amount: row.Amount,
    percentOfTotal: row.PercentOfTotal,
  }))
}

export interface CollectionActionSalesman {
  salesPersonId: string | null
  salesPersonName: string
  salesPersonCode: string
  overdueAmount: number
  percentOfTotal: number | null
  investigation: InvestigationMetadata | null
}

export interface NeedsEscalationInputs {
  agingOver90Exposure: number
  legacyDebtCount: number
}

export interface CollectionActionInputs {
  needsCollectionAction: CollectionActionSalesman[]
  needsEscalation: NeedsEscalationInputs
}

export function collectionActionInputs(
  topOverdueSalesmen: ReadonlyArray<DashboardCollectionRankingRow>,
  attentionCards: DashboardCollectionAttentionCards | null,
): CollectionActionInputs {
  const needsCollectionAction = topOverdueSalesmen
    .map((row) => ({
      salesPersonId: row.Investigation?.EntityId ?? null,
      salesPersonName: row.EntityName,
      salesPersonCode: row.EntityCode,
      overdueAmount: row.Amount,
      percentOfTotal: row.PercentOfTotal,
      investigation: row.Investigation ?? null,
    }))
    .sort((a, b) => b.overdueAmount - a.overdueAmount)

  return {
    needsCollectionAction,
    needsEscalation: {
      agingOver90Exposure: attentionCards?.AgingOver90Exposure ?? 0,
      legacyDebtCount: attentionCards?.LegacyDebtCount ?? 0,
    },
  }
}
