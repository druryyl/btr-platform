import type {
  PrincipalPerformanceRankingItem,
  PrincipalPerformanceResponse,
} from '@/models/dashboard'

export interface PortfolioCoverage {
  coveragePercentage: number | null
  principalsWithCoverageData: number
}

export interface PortfolioOverview {
  totalSales: number
  totalTarget: number | null
  achievementPercentage: number | null
  coveragePercentage: number | null
  principalsWithCoverageData: number
  portfolioReturnPercentage: number | null
}

export function portfolioCoverage(
  ranking: ReadonlyArray<PrincipalPerformanceRankingItem>,
): PortfolioCoverage {
  let activeSum = 0
  let totalSum = 0
  let principalsWithCoverageData = 0

  for (const item of ranking) {
    if (item.ActiveCustomerCount == null || item.TotalCustomerCount == null) {
      continue
    }
    activeSum += item.ActiveCustomerCount
    totalSum += item.TotalCustomerCount
    principalsWithCoverageData += 1
  }

  if (principalsWithCoverageData === 0 || totalSum <= 0) {
    return { coveragePercentage: null, principalsWithCoverageData }
  }

  return {
    coveragePercentage: activeSum / totalSum,
    principalsWithCoverageData,
  }
}

export function portfolioOverview(response: PrincipalPerformanceResponse): PortfolioOverview {
  const coverage = portfolioCoverage(response.Ranking ?? [])

  return {
    totalSales: response.PrincipalSalesOutAmount,
    totalTarget: response.PrincipalTargetAmount ?? null,
    achievementPercentage: response.AchievementPercentage ?? null,
    coveragePercentage: coverage.coveragePercentage,
    principalsWithCoverageData: coverage.principalsWithCoverageData,
    portfolioReturnPercentage: response.ReturnPercentage ?? null,
  }
}
