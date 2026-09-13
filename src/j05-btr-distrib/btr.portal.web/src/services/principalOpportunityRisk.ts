import type { PrincipalPerformanceRankingItem } from '@/models/dashboard'
import type { PrincipalDependency } from './principalContribution'
import { percentileRank } from './principalPercentile'

export const OPPORTUNITY_COVERAGE_PERCENTILE_MIN = 70
export const OPPORTUNITY_ACHIEVEMENT_PERCENTILE_MAX = 40
export const RETURN_RISK_PERCENTILE_MIN = 80
export const DEPENDENCY_RISK_PERCENTILE_MIN = 90
export const UNDERPERFORMING_ACHIEVEMENT_PERCENTILE_MAX = 25

export const OPPORTUNITY_RISK_RULE_IDS = [
  'Opportunity',
  'ReturnRisk',
  'DependencyRisk',
  'Underperforming',
] as const

export type OpportunityRiskRuleId = (typeof OPPORTUNITY_RISK_RULE_IDS)[number]

export interface OpportunityRiskEntry {
  ruleId: OpportunityRiskRuleId
  supplierId: string
  principalName: string
  supportingMetric: number
  percentile: number
  populationSize: number
  secondaryMetric?: number
  secondaryPercentile?: number
}

export interface OpportunityRiskBoard {
  opportunities: OpportunityRiskEntry[]
  risks: OpportunityRiskEntry[]
}

function rankingEntry(
  item: PrincipalPerformanceRankingItem,
  ruleId: OpportunityRiskRuleId,
  metric: number,
  values: ReadonlyArray<number | null | undefined>,
): OpportunityRiskEntry | null {
  const { percentile, populationSize } = percentileRank(values, metric)
  if (percentile == null) {
    return null
  }
  return {
    ruleId,
    supplierId: item.SupplierId,
    principalName: item.PrincipalName,
    supportingMetric: metric,
    percentile,
    populationSize,
  }
}

export function opportunityEntries(
  ranking: ReadonlyArray<PrincipalPerformanceRankingItem>,
): OpportunityRiskEntry[] {
  const coverageValues = ranking.map((item) => item.CoveragePercentage)
  const achievementValues = ranking.map((item) => item.AchievementPercentage)
  const entries: OpportunityRiskEntry[] = []

  for (const item of ranking) {
    if (item.CoveragePercentage == null || item.AchievementPercentage == null) {
      continue
    }
    const coverageRank = percentileRank(coverageValues, item.CoveragePercentage)
    const achievementRank = percentileRank(achievementValues, item.AchievementPercentage)
    if (coverageRank.percentile == null || achievementRank.percentile == null) {
      continue
    }
    if (
      coverageRank.percentile >= OPPORTUNITY_COVERAGE_PERCENTILE_MIN &&
      achievementRank.percentile <= OPPORTUNITY_ACHIEVEMENT_PERCENTILE_MAX
    ) {
      entries.push({
        ruleId: 'Opportunity',
        supplierId: item.SupplierId,
        principalName: item.PrincipalName,
        supportingMetric: item.CoveragePercentage,
        percentile: coverageRank.percentile,
        populationSize: coverageRank.populationSize,
        secondaryMetric: item.AchievementPercentage,
        secondaryPercentile: achievementRank.percentile,
      })
    }
  }

  return entries
}

export function returnRiskEntries(
  ranking: ReadonlyArray<PrincipalPerformanceRankingItem>,
): OpportunityRiskEntry[] {
  const returnValues = ranking.map((item) => item.ReturnPercentage)
  const entries: OpportunityRiskEntry[] = []

  for (const item of ranking) {
    if (item.ReturnPercentage == null) {
      continue
    }
    const entry = rankingEntry(item, 'ReturnRisk', item.ReturnPercentage, returnValues)
    if (entry != null && entry.percentile >= RETURN_RISK_PERCENTILE_MIN) {
      entries.push(entry)
    }
  }

  return entries
}

export function dependencyRiskEntries(
  dependencies: ReadonlyArray<PrincipalDependency>,
): OpportunityRiskEntry[] {
  const ratioValues = dependencies.map((dependency) => dependency.dependencyRatio)
  const entries: OpportunityRiskEntry[] = []

  for (const dependency of dependencies) {
    if (dependency.dependencyRatio == null) {
      continue
    }
    const { percentile, populationSize } = percentileRank(ratioValues, dependency.dependencyRatio)
    if (percentile == null || percentile < DEPENDENCY_RISK_PERCENTILE_MIN) {
      continue
    }
    entries.push({
      ruleId: 'DependencyRisk',
      supplierId: dependency.supplierId,
      principalName: dependency.principalName,
      supportingMetric: dependency.dependencyRatio,
      percentile,
      populationSize,
    })
  }

  return entries
}

export function underperformingEntries(
  ranking: ReadonlyArray<PrincipalPerformanceRankingItem>,
): OpportunityRiskEntry[] {
  const achievementValues = ranking.map((item) => item.AchievementPercentage)
  const entries: OpportunityRiskEntry[] = []

  for (const item of ranking) {
    if (item.AchievementPercentage == null) {
      continue
    }
    const { percentile, populationSize } = percentileRank(
      achievementValues,
      item.AchievementPercentage,
    )
    if (percentile == null || percentile > UNDERPERFORMING_ACHIEVEMENT_PERCENTILE_MAX) {
      continue
    }
    entries.push({
      ruleId: 'Underperforming',
      supplierId: item.SupplierId,
      principalName: item.PrincipalName,
      supportingMetric: item.AchievementPercentage,
      percentile,
      populationSize,
    })
  }

  return entries
}

export function opportunityRiskBoard(
  ranking: ReadonlyArray<PrincipalPerformanceRankingItem>,
  dependencies: ReadonlyArray<PrincipalDependency>,
): OpportunityRiskBoard {
  return {
    opportunities: opportunityEntries(ranking),
    risks: [
      ...returnRiskEntries(ranking),
      ...dependencyRiskEntries(dependencies),
      ...underperformingEntries(ranking),
    ],
  }
}
