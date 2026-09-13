import { describe, expect, it } from 'vitest'
import type {
  PrincipalPerformanceRankingItem,
  PrincipalPerformanceResponse,
} from '@/models/dashboard'
import { portfolioCoverage, portfolioOverview } from './principalPortfolio'

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

function makeResponse(
  overrides: Partial<PrincipalPerformanceResponse> = {},
): PrincipalPerformanceResponse {
  return {
    IsAvailable: true,
    KpiId: 'PRN-SALES-001',
    KpiName: 'Principal Sales Out',
    PeriodYear: 2026,
    PeriodMonth: 9,
    GeneratedAt: null,
    PrincipalSalesOutAmount: 5_000,
    UnknownPrincipalExceptionCount: 0,
    TargetKpiId: 'PRN-TGT-001',
    PrincipalTargetAmount: 4_000,
    AchievementAmountKpiId: 'PRN-TGT-002',
    AchievementPercentageKpiId: 'PRN-TGT-003',
    AchievementPercentage: 1.25,
    TargetAchievementIsAvailable: true,
    MissingTargetExceptionCount: 0,
    GoodReturnAmountKpiId: 'PRN-RET-001',
    BrokenReturnAmountKpiId: 'PRN-RET-002',
    TotalReturnAmountKpiId: 'PRN-RET-003',
    ReturnPercentageKpiId: 'PRN-RET-004',
    ReturnPercentage: 0.05,
    ReturnIsAvailable: true,
    MomGrowthKpiId: 'PRN-GRW-001',
    YoyGrowthKpiId: 'PRN-GRW-002',
    GrowthIsAvailable: true,
    ContributionIsAvailable: true,
    ActiveCustomerCountKpiId: 'PRN-CUS-001',
    CustomerCoverageKpiId: 'PRN-CUS-002',
    CustomerReachIsAvailable: true,
    SalesmanContributions: [],
    Disclosures: [],
    Ranking: [],
    SupportingRankingOptions: [],
    ...overrides,
  }
}

describe('portfolioCoverage', () => {
  it('returns null with count 0 for an empty coverage population', () => {
    expect(portfolioCoverage([])).toEqual({
      coveragePercentage: null,
      principalsWithCoverageData: 0,
    })
  })

  it('returns null with count 0 when no Principal has both counts', () => {
    const result = portfolioCoverage([
      makeRanking({ ActiveCustomerCount: 10, TotalCustomerCount: null }),
      makeRanking({ ActiveCustomerCount: null, TotalCustomerCount: 100 }),
      makeRanking({ ActiveCustomerCount: null, TotalCustomerCount: null }),
    ])

    expect(result).toEqual({ coveragePercentage: null, principalsWithCoverageData: 0 })
  })

  it('computes the weighted ratio over Principals with both counts', () => {
    const result = portfolioCoverage([
      makeRanking({ SupplierId: 'P1', ActiveCustomerCount: 30, TotalCustomerCount: 100 }),
      makeRanking({ SupplierId: 'P2', ActiveCustomerCount: 20, TotalCustomerCount: 100 }),
    ])

    expect(result.principalsWithCoverageData).toBe(2)
    expect(result.coveragePercentage).toBeCloseTo(50 / 200, 10)
  })

  it('uses weighted aggregation, not a simple average of per-Principal rates', () => {
    const result = portfolioCoverage([
      makeRanking({ SupplierId: 'P-BIG', ActiveCustomerCount: 90, TotalCustomerCount: 100 }),
      makeRanking({ SupplierId: 'P-SMALL', ActiveCustomerCount: 0, TotalCustomerCount: 900 }),
    ])

    expect(result.coveragePercentage).toBeCloseTo(90 / 1000, 10)
    expect(result.coveragePercentage).not.toBeCloseTo((0.9 + 0) / 2, 10)
  })

  it('excludes partially missing rows from both sums', () => {
    const result = portfolioCoverage([
      makeRanking({ SupplierId: 'P1', ActiveCustomerCount: 40, TotalCustomerCount: 80 }),
      makeRanking({ SupplierId: 'P2', ActiveCustomerCount: 1_000, TotalCustomerCount: null }),
      makeRanking({ SupplierId: 'P3', ActiveCustomerCount: null, TotalCustomerCount: 1_000 }),
    ])

    expect(result).toEqual({ coveragePercentage: 0.5, principalsWithCoverageData: 1 })
  })

  it('never zero-fills missing values', () => {
    const withMissing = portfolioCoverage([
      makeRanking({ SupplierId: 'P1', ActiveCustomerCount: 40, TotalCustomerCount: 80 }),
      makeRanking({ SupplierId: 'P2', ActiveCustomerCount: null, TotalCustomerCount: null }),
    ])
    const zeroFilled = portfolioCoverage([
      makeRanking({ SupplierId: 'P1', ActiveCustomerCount: 40, TotalCustomerCount: 80 }),
      makeRanking({ SupplierId: 'P2', ActiveCustomerCount: 0, TotalCustomerCount: 0 }),
    ])

    expect(withMissing.coveragePercentage).toBe(0.5)
    expect(zeroFilled.coveragePercentage).toBe(0.5)
    expect(withMissing.principalsWithCoverageData).toBe(1)
  })

  it('treats zero active customers as valid data, not missing data', () => {
    const result = portfolioCoverage([
      makeRanking({ SupplierId: 'P1', ActiveCustomerCount: 0, TotalCustomerCount: 100 }),
    ])

    expect(result).toEqual({ coveragePercentage: 0, principalsWithCoverageData: 1 })
  })

  it('returns null when the in-scope total is zero or negative', () => {
    expect(
      portfolioCoverage([
        makeRanking({ SupplierId: 'P1', ActiveCustomerCount: 0, TotalCustomerCount: 0 }),
      ]).coveragePercentage,
    ).toBeNull()
  })
})

describe('portfolioOverview', () => {
  it('passes Total Sales, Total Target, and Overall Achievement % through as-is', () => {
    const result = portfolioOverview(
      makeResponse({
        PrincipalSalesOutAmount: 7_000,
        PrincipalTargetAmount: 5_000,
        AchievementPercentage: 1.4,
      }),
    )

    expect(result.totalSales).toBe(7_000)
    expect(result.totalTarget).toBe(5_000)
    expect(result.achievementPercentage).toBe(1.4)
  })

  it('keeps nullable Target and Achievement % as null when unavailable', () => {
    const result = portfolioOverview(
      makeResponse({ PrincipalTargetAmount: null, AchievementPercentage: null }),
    )

    expect(result.totalTarget).toBeNull()
    expect(result.achievementPercentage).toBeNull()
  })

  it('passes Portfolio Return % through with no calculation', () => {
    const result = portfolioOverview(makeResponse({ ReturnPercentage: 0.08 }))

    expect(result.portfolioReturnPercentage).toBe(0.08)
  })

  it('keeps Portfolio Return % null when unavailable', () => {
    const result = portfolioOverview(makeResponse({ ReturnPercentage: null }))

    expect(result.portfolioReturnPercentage).toBeNull()
  })

  it('derives coverage from the same both-counts population', () => {
    const result = portfolioOverview(
      makeResponse({
        Ranking: [
          makeRanking({ SupplierId: 'P1', ActiveCustomerCount: 25, TotalCustomerCount: 50 }),
          makeRanking({ SupplierId: 'P2', ActiveCustomerCount: null, TotalCustomerCount: 50 }),
        ],
      }),
    )

    expect(result.coveragePercentage).toBe(0.5)
    expect(result.principalsWithCoverageData).toBe(1)
  })

  it('does not compute Total Active Customers (GAP-001)', () => {
    const result = portfolioOverview(
      makeResponse({
        Ranking: [
          makeRanking({ SupplierId: 'P1', ActiveCustomerCount: 25, TotalCustomerCount: 50 }),
        ],
      }),
    )

    expect(result).not.toHaveProperty('totalActiveCustomers')
    expect(result).not.toHaveProperty('totalActiveCustomerCount')
    expect(Object.keys(result).sort()).toEqual(
      [
        'achievementPercentage',
        'coveragePercentage',
        'portfolioReturnPercentage',
        'principalsWithCoverageData',
        'totalSales',
        'totalTarget',
      ].sort(),
    )
  })
})
