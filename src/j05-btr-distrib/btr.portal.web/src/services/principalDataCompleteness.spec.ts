import { describe, expect, it } from 'vitest'
import type {
  PrincipalPerformanceRankingItem,
  PrincipalPerformanceResponse,
  PrincipalSalesmanContributionItem,
} from '@/models/dashboard'
import { principalDataCompleteness } from './principalDataCompleteness'

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

function makeContribution(
  overrides: Partial<PrincipalSalesmanContributionItem> = {},
): PrincipalSalesmanContributionItem {
  return {
    SupplierId: 'P1',
    PrincipalName: 'Principal One',
    SalesPersonId: 'S1',
    SalesPersonCode: 'S1',
    SalesPersonName: 'Salesman One',
    SourceSalesOutKpiId: 'PRN-SALES-001',
    ContributionAmount: 500,
    LineCount: 1,
    HasTargetResponsibility: false,
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
    TargetAchievementIsAvailable: true,
    MissingTargetExceptionCount: 0,
    GoodReturnAmountKpiId: 'PRN-RET-001',
    BrokenReturnAmountKpiId: 'PRN-RET-002',
    TotalReturnAmountKpiId: 'PRN-RET-003',
    ReturnPercentageKpiId: 'PRN-RET-004',
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

function fullRanking(): PrincipalPerformanceRankingItem[] {
  return [
    makeRanking({
      SupplierId: 'P1',
      PrincipalTargetAmount: 1_000,
      ReturnPercentage: 0.05,
      TotalReturnAmount: 50,
      MomGrowthPercentage: 0.1,
      ActiveCustomerCount: 10,
      TotalCustomerCount: 100,
    }),
    makeRanking({
      SupplierId: 'P2',
      PrincipalTargetAmount: 2_000,
      ReturnPercentage: 0.08,
      TotalReturnAmount: 80,
      YoyGrowthPercentage: 0.2,
      ActiveCustomerCount: 20,
      TotalCustomerCount: 100,
    }),
  ]
}

describe('principalDataCompleteness', () => {
  it('reports full completeness for a fully available population', () => {
    const result = principalDataCompleteness(
      makeResponse({
        Ranking: fullRanking(),
        SalesmanContributions: [
          makeContribution({ SupplierId: 'P1', SalesPersonId: 'S1' }),
          makeContribution({ SupplierId: 'P2', SalesPersonId: 'S2' }),
        ],
      }),
    )

    expect(result.populationSize).toBe(2)
    expect(result.target).toEqual({
      isAvailable: true,
      availableCount: 2,
      populationSize: 2,
      completenessPercentage: 100,
    })
    expect(result.return.completenessPercentage).toBe(100)
    expect(result.growth.completenessPercentage).toBe(100)
    expect(result.coverage.completenessPercentage).toBe(100)
    expect(result.contribution).toEqual({
      isAvailable: true,
      availableCount: 2,
      populationSize: 2,
      completenessPercentage: 100,
    })
  })

  it('reports partial completeness for a partially available population', () => {
    const result = principalDataCompleteness(
      makeResponse({
        Ranking: [
          makeRanking({
            SupplierId: 'P1',
            PrincipalTargetAmount: 1_000,
            ReturnPercentage: 0.05,
            MomGrowthPercentage: 0.1,
            ActiveCustomerCount: 10,
            TotalCustomerCount: 100,
          }),
          makeRanking({
            SupplierId: 'P2',
            PrincipalTargetAmount: null,
            ReturnPercentage: null,
            TotalReturnAmount: null,
            MomGrowthPercentage: null,
            YoyGrowthPercentage: null,
            ActiveCustomerCount: null,
            TotalCustomerCount: 100,
          }),
          makeRanking({
            SupplierId: 'P3',
            PrincipalTargetAmount: null,
            ReturnPercentage: null,
            TotalReturnAmount: null,
            MomGrowthPercentage: null,
            YoyGrowthPercentage: null,
            ActiveCustomerCount: 5,
            TotalCustomerCount: null,
          }),
          makeRanking({
            SupplierId: 'P4',
            PrincipalTargetAmount: 4_000,
            ReturnPercentage: null,
            TotalReturnAmount: 40,
            YoyGrowthPercentage: 0.05,
            ActiveCustomerCount: 0,
            TotalCustomerCount: 50,
          }),
        ],
        SalesmanContributions: [
          makeContribution({ SupplierId: 'P1', SalesPersonId: 'S1' }),
          makeContribution({ SupplierId: 'P1', SalesPersonId: 'S2' }),
        ],
      }),
    )

    expect(result.populationSize).toBe(4)
    expect(result.target.availableCount).toBe(2)
    expect(result.target.completenessPercentage).toBe(50)
    expect(result.return.availableCount).toBe(2)
    expect(result.return.completenessPercentage).toBe(50)
    expect(result.growth.availableCount).toBe(2)
    expect(result.growth.completenessPercentage).toBe(50)
    // P4 has zero active customers: valid data, not missing (OQ-8).
    expect(result.coverage.availableCount).toBe(2)
    expect(result.coverage.completenessPercentage).toBe(50)
    // Distinct suppliers only; duplicate salesmen for P1 count once.
    expect(result.contribution.availableCount).toBe(1)
    expect(result.contribution.completenessPercentage).toBe(25)
  })

  it('reports absent (null) percentages for an unavailable/empty population', () => {
    const result = principalDataCompleteness(
      makeResponse({
        Ranking: [],
        SalesmanContributions: [],
        TargetAchievementIsAvailable: false,
        ReturnIsAvailable: false,
        GrowthIsAvailable: false,
        CustomerReachIsAvailable: false,
        ContributionIsAvailable: false,
      }),
    )

    expect(result.populationSize).toBe(0)
    for (const metric of [
      result.target,
      result.return,
      result.growth,
      result.coverage,
      result.contribution,
    ]) {
      expect(metric.availableCount).toBe(0)
      expect(metric.completenessPercentage).toBeNull()
      expect(metric.isAvailable).toBe(false)
    }
  })

  it('preserves response availability flags and exception counts', () => {
    const result = principalDataCompleteness(
      makeResponse({
        Ranking: fullRanking(),
        SalesmanContributions: [],
        TargetAchievementIsAvailable: false,
        ContributionIsAvailable: false,
        MissingTargetExceptionCount: 3,
        UnknownPrincipalExceptionCount: 2,
      }),
    )

    expect(result.target.isAvailable).toBe(false)
    expect(result.contribution.isAvailable).toBe(false)
    expect(result.return.isAvailable).toBe(true)
    expect(result.missingTargetExceptionCount).toBe(3)
    expect(result.unknownPrincipalExceptionCount).toBe(2)
    // Row counts still derived from rows; flags are preserved, not zero-filled.
    expect(result.target.availableCount).toBe(2)
    expect(result.contribution.availableCount).toBe(0)
    expect(result.contribution.completenessPercentage).toBe(0)
  })

  it('never treats unknown as zero', () => {
    const result = principalDataCompleteness(
      makeResponse({
        Ranking: [
          makeRanking({
            SupplierId: 'P1',
            PrincipalTargetAmount: null,
            ReturnPercentage: null,
            TotalReturnAmount: null,
            MomGrowthPercentage: null,
            YoyGrowthPercentage: null,
            ActiveCustomerCount: null,
            TotalCustomerCount: null,
          }),
        ],
        SalesmanContributions: [],
      }),
    )

    expect(result.target.completenessPercentage).toBe(0)
    expect(result.target.availableCount).toBe(0)
    expect(result.coverage.completenessPercentage).toBe(0)
    // Absent is 0% of a non-empty population, never a fabricated value;
    // an empty population is null, not 0 (covered above).
    expect(result.populationSize).toBe(1)
  })
})
