import { describe, expect, it } from 'vitest'
import type {
  PrincipalPerformanceRankingItem,
  PrincipalSalesmanContributionItem,
} from '@/models/dashboard'
import {
  contributionCoverage,
  contributionPercentage,
  contributionShare,
  principalDependencies,
} from './principalContribution'

function makeContribution(
  overrides: Partial<PrincipalSalesmanContributionItem> = {},
): PrincipalSalesmanContributionItem {
  return {
    SupplierId: 'P1',
    PrincipalName: 'Principal One',
    SalesPersonId: 'S1',
    SalesPersonCode: 'S01',
    SalesPersonName: 'Salesman One',
    SourceSalesOutKpiId: 'PRN-SALES-001',
    ContributionAmount: 100,
    LineCount: 1,
    HasTargetResponsibility: false,
    ...overrides,
  }
}

function makeRanking(
  overrides: Partial<PrincipalPerformanceRankingItem> = {},
): PrincipalPerformanceRankingItem {
  return {
    Rank: 1,
    PrincipalName: 'Principal One',
    SupplierId: 'P1',
    KpiId: 'PRN-SALES-001',
    PrincipalSalesOutAmount: 1_000,
    ...overrides,
  }
}

describe('contributionPercentage', () => {
  it('returns the ratio scaled to percent units', () => {
    expect(contributionPercentage(450_000_000, 1_000_000_000)).toBe(45)
  })

  it('returns null when amount is null', () => {
    expect(contributionPercentage(null, 1_000_000_000)).toBeNull()
  })

  it('returns null when denominator is null', () => {
    expect(contributionPercentage(100, null)).toBeNull()
  })

  it('returns null when denominator is zero', () => {
    expect(contributionPercentage(100, 0)).toBeNull()
  })

  it('returns null when denominator is negative', () => {
    expect(contributionPercentage(100, -1)).toBeNull()
  })

  it('returns zero when amount is zero', () => {
    expect(contributionPercentage(0, 1_000_000_000)).toBe(0)
  })
})

describe('contributionShare', () => {
  it('returns the normalized share in percent units', () => {
    expect(contributionShare(250, 1_000)).toBe(25)
  })

  it('returns null when the denominator is zero', () => {
    expect(contributionShare(250, 0)).toBeNull()
  })

  it('returns null when the amount is null', () => {
    expect(contributionShare(null, 1_000)).toBeNull()
  })
})

describe('contributionCoverage', () => {
  it('divides total contribution by Principal Sales-Out in percent units', () => {
    expect(contributionCoverage(1_000, 2_000)).toBe(50)
  })

  it('returns null when Sales-Out is missing or zero', () => {
    expect(contributionCoverage(1_000, null)).toBeNull()
    expect(contributionCoverage(1_000, 0)).toBeNull()
  })
})

describe('principalDependencies', () => {
  it('groups contribution rows by Principal', () => {
    const result = principalDependencies(
      [
        makeContribution({ SupplierId: 'P1', SalesPersonName: 'A' }),
        makeContribution({ SupplierId: 'P2', PrincipalName: 'Principal Two' }),
      ],
      [makeRanking({ SupplierId: 'P1' }), makeRanking({ SupplierId: 'P2' })],
    )

    expect(result.map((dependency) => dependency.supplierId).sort()).toEqual(['P1', 'P2'])
  })

  it('normalizes each salesman share against SUM(ContributionAmount), not Principal Sales-Out', () => {
    const result = principalDependencies(
      [
        makeContribution({ SupplierId: 'P1', SalesPersonId: 'S1', SalesPersonName: 'Big', ContributionAmount: 750 }),
        makeContribution({ SupplierId: 'P1', SalesPersonId: 'S2', SalesPersonName: 'Small', ContributionAmount: 250 }),
      ],
      [makeRanking({ SupplierId: 'P1', PrincipalSalesOutAmount: 5_000 })],
    )

    const dependency = result[0]
    expect(dependency.dependencyRatio).toBe(75)
    expect(dependency.distribution.map((entry) => entry.share)).toEqual([75, 25])
  })

  it('derives DependencyRatio as the maximum salesman share', () => {
    const result = principalDependencies(
      [
        makeContribution({ SupplierId: 'P1', SalesPersonId: 'S1', SalesPersonName: 'Big', ContributionAmount: 600 }),
        makeContribution({ SupplierId: 'P1', SalesPersonId: 'S2', SalesPersonName: 'Mid', ContributionAmount: 300 }),
        makeContribution({ SupplierId: 'P1', SalesPersonId: 'S3', SalesPersonName: 'Small', ContributionAmount: 100 }),
      ],
      [makeRanking({ SupplierId: 'P1' })],
    )

    expect(result[0].dependencyRatio).toBe(60)
    expect(result[0].topContributor).toEqual({
      salesPersonId: 'S1',
      salesPersonName: 'Big',
      contributionAmount: 600,
      share: 60,
    })
  })

  it('orders the distribution by share descending', () => {
    const result = principalDependencies(
      [
        makeContribution({ SupplierId: 'P1', SalesPersonId: 'S1', SalesPersonName: 'Low', ContributionAmount: 100 }),
        makeContribution({ SupplierId: 'P1', SalesPersonId: 'S2', SalesPersonName: 'High', ContributionAmount: 700 }),
        makeContribution({ SupplierId: 'P1', SalesPersonId: 'S3', SalesPersonName: 'Mid', ContributionAmount: 200 }),
      ],
      [makeRanking({ SupplierId: 'P1' })],
    )

    expect(result[0].distribution.map((entry) => entry.salesPersonName)).toEqual([
      'High',
      'Mid',
      'Low',
    ])
  })

  it('counts distinct SalesPersonId as supplementary metadata', () => {
    const result = principalDependencies(
      [
        makeContribution({ SupplierId: 'P1', SalesPersonId: 'S1', ContributionAmount: 300 }),
        makeContribution({ SupplierId: 'P1', SalesPersonId: 'S1', ContributionAmount: 200 }),
        makeContribution({ SupplierId: 'P1', SalesPersonId: 'S2', ContributionAmount: 500 }),
      ],
      [makeRanking({ SupplierId: 'P1' })],
    )

    expect(result[0].contributingSalesmanCount).toBe(2)
  })

  it('computes ContributionCoverage against Principal Sales-Out', () => {
    const result = principalDependencies(
      [makeContribution({ SupplierId: 'P1', ContributionAmount: 1_500 })],
      [makeRanking({ SupplierId: 'P1', PrincipalSalesOutAmount: 2_000 })],
    )

    expect(result[0].contributionCoverage).toBe(75)
  })

  it('ranks Principals by DependencyRatio descending', () => {
    const result = principalDependencies(
      [
        makeContribution({ SupplierId: 'P-LOW', PrincipalName: 'Low', SalesPersonId: 'A', ContributionAmount: 300 }),
        makeContribution({ SupplierId: 'P-LOW', PrincipalName: 'Low', SalesPersonId: 'B', ContributionAmount: 700 }),
        makeContribution({ SupplierId: 'P-HIGH', PrincipalName: 'High', SalesPersonId: 'C', ContributionAmount: 900 }),
        makeContribution({ SupplierId: 'P-HIGH', PrincipalName: 'High', SalesPersonId: 'D', ContributionAmount: 100 }),
        makeContribution({ SupplierId: 'P-MID', PrincipalName: 'Mid', SalesPersonId: 'E', ContributionAmount: 600 }),
        makeContribution({ SupplierId: 'P-MID', PrincipalName: 'Mid', SalesPersonId: 'F', ContributionAmount: 400 }),
      ],
      [makeRanking({ SupplierId: 'P-LOW' }), makeRanking({ SupplierId: 'P-HIGH' }), makeRanking({ SupplierId: 'P-MID' })],
    )

    expect(result.map((dependency) => [dependency.principalName, dependency.dependencyRank])).toEqual([
      ['High', 1],
      ['Low', 2],
      ['Mid', 3],
    ])
  })

  it('assigns tied DependencyRatio the same rank', () => {
    const result = principalDependencies(
      [
        makeContribution({ SupplierId: 'P1', PrincipalName: 'A', SalesPersonId: 'A1', ContributionAmount: 500 }),
        makeContribution({ SupplierId: 'P1', PrincipalName: 'A', SalesPersonId: 'A2', ContributionAmount: 500 }),
        makeContribution({ SupplierId: 'P2', PrincipalName: 'B', SalesPersonId: 'B1', ContributionAmount: 500 }),
        makeContribution({ SupplierId: 'P2', PrincipalName: 'B', SalesPersonId: 'B2', ContributionAmount: 500 }),
        makeContribution({ SupplierId: 'P3', PrincipalName: 'C', SalesPersonId: 'C1', ContributionAmount: 400 }),
        makeContribution({ SupplierId: 'P3', PrincipalName: 'C', SalesPersonId: 'C2', ContributionAmount: 600 }),
      ],
      [makeRanking({ SupplierId: 'P1' }), makeRanking({ SupplierId: 'P2' }), makeRanking({ SupplierId: 'P3' })],
    )

    expect(result.map((dependency) => [dependency.principalName, dependency.dependencyRank])).toEqual([
      ['C', 1],
      ['A', 2],
      ['B', 2],
    ])
  })

  it('does not use ContributingSalesmanCount to affect ranking (GAP-008)', () => {
    const result = principalDependencies(
      [
        makeContribution({ SupplierId: 'P-WIDE', PrincipalName: 'Wide', SalesPersonId: 'W1', ContributionAmount: 200 }),
        makeContribution({ SupplierId: 'P-WIDE', PrincipalName: 'Wide', SalesPersonId: 'W2', ContributionAmount: 200 }),
        makeContribution({ SupplierId: 'P-WIDE', PrincipalName: 'Wide', SalesPersonId: 'W3', ContributionAmount: 200 }),
        makeContribution({ SupplierId: 'P-WIDE', PrincipalName: 'Wide', SalesPersonId: 'W4', ContributionAmount: 200 }),
        makeContribution({ SupplierId: 'P-WIDE', PrincipalName: 'Wide', SalesPersonId: 'W5', ContributionAmount: 200 }),
        makeContribution({ SupplierId: 'P-NARROW', PrincipalName: 'Narrow', SalesPersonId: 'N1', ContributionAmount: 900 }),
        makeContribution({ SupplierId: 'P-NARROW', PrincipalName: 'Narrow', SalesPersonId: 'N2', ContributionAmount: 100 }),
      ],
      [makeRanking({ SupplierId: 'P-WIDE' }), makeRanking({ SupplierId: 'P-NARROW' })],
    )

    const wide = result.find((dependency) => dependency.supplierId === 'P-WIDE')!
    const narrow = result.find((dependency) => dependency.supplierId === 'P-NARROW')!
    expect(wide.contributingSalesmanCount).toBe(5)
    expect(narrow.contributingSalesmanCount).toBe(2)
    expect(narrow.dependencyRank).toBe(1)
    expect(wide.dependencyRank).toBe(2)
  })

  it('produces no dependency data for a Principal with no contribution rows', () => {
    const result = principalDependencies(
      [makeContribution({ SupplierId: 'P1' })],
      [makeRanking({ SupplierId: 'P1' }), makeRanking({ SupplierId: 'P-NONE', PrincipalName: 'No Contribution' })],
    )

    expect(result).toHaveLength(1)
    expect(result[0].supplierId).toBe('P1')
    expect(result.find((dependency) => dependency.supplierId === 'P-NONE')).toBeUndefined()
  })

  it('returns an empty list when there are no contribution rows', () => {
    expect(principalDependencies([], [makeRanking()])).toEqual([])
  })

  it('leaves ContributionCoverage null when Principal Sales-Out is unavailable', () => {
    const result = principalDependencies(
      [makeContribution({ SupplierId: 'P1' })],
      [makeRanking({ SupplierId: 'P1', PrincipalSalesOutAmount: 0 })],
    )

    expect(result[0].contributionCoverage).toBeNull()
  })
})
