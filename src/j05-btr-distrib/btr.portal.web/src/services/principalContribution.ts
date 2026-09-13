import type {
  PrincipalPerformanceRankingItem,
  PrincipalSalesmanContributionItem,
} from '@/models/dashboard'

export function contributionPercentage(
  amount: number | null | undefined,
  denominator: number | null | undefined,
): number | null {
  if (amount == null || denominator == null || denominator <= 0) {
    return null
  }
  return (amount / denominator) * 100
}

export function contributionShare(
  amount: number | null | undefined,
  denominator: number | null | undefined,
): number | null {
  return contributionPercentage(amount, denominator)
}

export function contributionCoverage(
  totalContributionAmount: number | null | undefined,
  principalSalesOutAmount: number | null | undefined,
): number | null {
  return contributionPercentage(totalContributionAmount, principalSalesOutAmount)
}

export interface ContributionDistributionEntry {
  salesPersonId: string
  salesPersonName: string
  contributionAmount: number
  share: number | null
}

export interface PrincipalDependency {
  supplierId: string
  principalName: string
  dependencyRatio: number | null
  dependencyRank: number | null
  contributingSalesmanCount: number
  topContributor: ContributionDistributionEntry | null
  totalContributionAmount: number
  contributionCoverage: number | null
  distribution: ContributionDistributionEntry[]
}

function compareName(a: string, b: string): number {
  if (a < b) return -1
  if (a > b) return 1
  return 0
}

function compareDistribution(
  a: ContributionDistributionEntry,
  b: ContributionDistributionEntry,
): number {
  if (a.share == null && b.share == null) return compareName(a.salesPersonName, b.salesPersonName)
  if (a.share == null) return 1
  if (b.share == null) return -1
  if (a.share !== b.share) return b.share - a.share
  return compareName(a.salesPersonName, b.salesPersonName)
}

function rankDependencies(dependencies: PrincipalDependency[]): void {
  dependencies.sort((a, b) => {
    if (a.dependencyRatio == null && b.dependencyRatio == null) {
      return compareName(a.principalName, b.principalName)
    }
    if (a.dependencyRatio == null) return 1
    if (b.dependencyRatio == null) return -1
    if (a.dependencyRatio !== b.dependencyRatio) return b.dependencyRatio - a.dependencyRatio
    return compareName(a.principalName, b.principalName)
  })

  let previousRatio: number | null = null
  let previousRank = 0
  let position = 0
  for (const dependency of dependencies) {
    if (dependency.dependencyRatio == null) {
      dependency.dependencyRank = null
      continue
    }
    position += 1
    if (previousRatio != null && dependency.dependencyRatio === previousRatio) {
      dependency.dependencyRank = previousRank
    } else {
      dependency.dependencyRank = position
      previousRank = position
      previousRatio = dependency.dependencyRatio
    }
  }
}

export function principalDependencies(
  contributions: ReadonlyArray<PrincipalSalesmanContributionItem>,
  ranking: ReadonlyArray<PrincipalPerformanceRankingItem>,
): PrincipalDependency[] {
  const salesOutBySupplier = new Map<string, number>()
  for (const item of ranking) {
    salesOutBySupplier.set(item.SupplierId, item.PrincipalSalesOutAmount)
  }

  const rowsBySupplier = new Map<string, PrincipalSalesmanContributionItem[]>()
  for (const contribution of contributions) {
    const rows = rowsBySupplier.get(contribution.SupplierId)
    if (rows) {
      rows.push(contribution)
    } else {
      rowsBySupplier.set(contribution.SupplierId, [contribution])
    }
  }

  const dependencies: PrincipalDependency[] = []
  for (const [supplierId, rows] of rowsBySupplier) {
    const totalContributionAmount = rows.reduce((sum, row) => sum + row.ContributionAmount, 0)
    const distribution = rows
      .map((row) => ({
        salesPersonId: row.SalesPersonId,
        salesPersonName: row.SalesPersonName,
        contributionAmount: row.ContributionAmount,
        share: contributionShare(row.ContributionAmount, totalContributionAmount),
      }))
      .sort(compareDistribution)
    const topContributor = distribution.find((entry) => entry.share != null) ?? null
    const distinctSalesmen = new Set(rows.map((row) => row.SalesPersonId))

    dependencies.push({
      supplierId,
      principalName: rows[0].PrincipalName,
      dependencyRatio: topContributor?.share ?? null,
      dependencyRank: null,
      contributingSalesmanCount: distinctSalesmen.size,
      topContributor,
      totalContributionAmount,
      contributionCoverage: contributionCoverage(
        totalContributionAmount,
        salesOutBySupplier.get(supplierId),
      ),
      distribution,
    })
  }

  rankDependencies(dependencies)
  return dependencies
}
