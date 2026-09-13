import type { PrincipalPerformanceResponse } from '@/models/dashboard'

export interface CompletenessMetric {
  isAvailable: boolean
  availableCount: number
  populationSize: number
  completenessPercentage: number | null
}

export interface PrincipalDataCompleteness {
  populationSize: number
  target: CompletenessMetric
  return: CompletenessMetric
  growth: CompletenessMetric
  coverage: CompletenessMetric
  contribution: CompletenessMetric
  missingTargetExceptionCount: number
  unknownPrincipalExceptionCount: number
}

function toMetric(
  isAvailable: boolean,
  availableCount: number,
  populationSize: number,
): CompletenessMetric {
  return {
    isAvailable,
    availableCount,
    populationSize,
    completenessPercentage: populationSize === 0 ? null : (availableCount / populationSize) * 100,
  }
}

export function principalDataCompleteness(
  response: PrincipalPerformanceResponse,
): PrincipalDataCompleteness {
  const ranking = response.Ranking ?? []
  const populationSize = ranking.length

  let targetCount = 0
  let returnCount = 0
  let growthCount = 0
  let coverageCount = 0

  for (const item of ranking) {
    if (item.PrincipalTargetAmount != null) {
      targetCount += 1
    }
    if (item.ReturnPercentage != null || item.TotalReturnAmount != null) {
      returnCount += 1
    }
    if (item.MomGrowthPercentage != null || item.YoyGrowthPercentage != null) {
      growthCount += 1
    }
    if (item.ActiveCustomerCount != null && item.TotalCustomerCount != null) {
      coverageCount += 1
    }
  }

  const contributingSuppliers = new Set<string>()
  for (const contribution of response.SalesmanContributions ?? []) {
    contributingSuppliers.add(contribution.SupplierId)
  }

  return {
    populationSize,
    target: toMetric(response.TargetAchievementIsAvailable, targetCount, populationSize),
    return: toMetric(response.ReturnIsAvailable, returnCount, populationSize),
    growth: toMetric(response.GrowthIsAvailable, growthCount, populationSize),
    coverage: toMetric(response.CustomerReachIsAvailable, coverageCount, populationSize),
    contribution: toMetric(
      response.ContributionIsAvailable,
      contributingSuppliers.size,
      populationSize,
    ),
    missingTargetExceptionCount: response.MissingTargetExceptionCount,
    unknownPrincipalExceptionCount: response.UnknownPrincipalExceptionCount,
  }
}
