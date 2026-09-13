import { describe, expect, it } from 'vitest'
import type { PrincipalPerformanceRankingItem } from '@/models/dashboard'
import type { PrincipalDependency } from './principalContribution'
import {
  dependencyRiskEntries,
  opportunityEntries,
  opportunityRiskBoard,
  returnRiskEntries,
  underperformingEntries,
} from './principalOpportunityRisk'

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

function makeDependency(overrides: Partial<PrincipalDependency> = {}): PrincipalDependency {
  return {
    supplierId: 'P1',
    principalName: 'Principal One',
    dependencyRatio: null,
    dependencyRank: null,
    contributingSalesmanCount: 1,
    topContributor: null,
    totalContributionAmount: 0,
    contributionCoverage: null,
    distribution: [],
    ...overrides,
  }
}

function rankingWith(
  coverage: ReadonlyArray<number | null>,
  achievement: ReadonlyArray<number | null>,
  returns: ReadonlyArray<number | null> = [],
): PrincipalPerformanceRankingItem[] {
  return coverage.map((value, index) =>
    makeRanking({
      SupplierId: `P${index}`,
      PrincipalName: `Principal ${index}`,
      CoveragePercentage: value,
      AchievementPercentage: achievement[index] ?? null,
      ReturnPercentage: returns[index] ?? null,
    }),
  )
}

const ASCENDING = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9]

describe('opportunityEntries', () => {
  it('flags a Principal at exactly Coverage P70 and Achievement P40', () => {
    const ranking = rankingWith(ASCENDING, [0, 1, 2, 3, 10, 11, 12, 4, 8, 9])

    const entries = opportunityEntries(ranking)

    expect(entries).toHaveLength(1)
    expect(entries[0].supplierId).toBe('P7')
  })

  it('does not flag a Principal below Coverage P70', () => {
    const ranking = rankingWith([0, 1, 2, 3, 4, 5, 6, 6, 8, 9], [
      0, 1, 2, 3, 10, 11, 12, 4, 8, 9,
    ])

    expect(opportunityEntries(ranking)).toEqual([])
  })

  it('does not flag a Principal above Achievement P40', () => {
    const ranking = rankingWith(ASCENDING, [0, 1, 2, 3, 4, 11, 12, 5, 8, 9])

    expect(opportunityEntries(ranking)).toEqual([])
  })

  it('excludes a Principal with a null Coverage metric', () => {
    const ranking = rankingWith([0, 1, 2, 3, 4, 5, 6, null, 8, 9], [
      0, 1, 2, 3, 10, 11, 12, 4, 8, 9,
    ])

    expect(opportunityEntries(ranking)).toEqual([])
  })

  it('excludes a Principal with a null Achievement metric', () => {
    const ranking = rankingWith(ASCENDING, [0, 1, 2, 3, 10, 11, 12, null, 8, 9])

    expect(opportunityEntries(ranking)).toEqual([])
  })

  it('returns no opportunities for an empty population', () => {
    expect(opportunityEntries([])).toEqual([])
  })

  it('reports the rule, supporting metric, percentile, secondary metric, and population size', () => {
    const ranking = rankingWith(ASCENDING, [0, 1, 2, 3, 10, 11, 12, 4, 8, 9])

    expect(opportunityEntries(ranking)[0]).toEqual({
      ruleId: 'Opportunity',
      supplierId: 'P7',
      principalName: 'Principal 7',
      supportingMetric: 7,
      percentile: 70,
      populationSize: 10,
      secondaryMetric: 4,
      secondaryPercentile: 40,
    })
  })
})

describe('returnRiskEntries', () => {
  it('flags a Principal at exactly Return P80', () => {
    const ranking = rankingWith(ASCENDING, ASCENDING, ASCENDING)

    const entries = returnRiskEntries(ranking)

    expect(entries.map((entry) => entry.supplierId)).toContain('P8')
  })

  it('does not flag a Principal below Return P80', () => {
    const ranking = rankingWith(ASCENDING, ASCENDING, ASCENDING)

    expect(returnRiskEntries(ranking).map((entry) => entry.supplierId)).not.toContain('P7')
  })

  it('excludes a Principal with a null Return metric', () => {
    const ranking = rankingWith(ASCENDING, ASCENDING, [0, 1, 2, 3, 4, 5, 6, 7, null, 9])

    const entries = returnRiskEntries(ranking)

    expect(entries.map((entry) => entry.supplierId)).not.toContain('P8')
    expect(entries.map((entry) => entry.supplierId)).toContain('P9')
  })

  it('reports the rule, supporting metric, percentile, and population size', () => {
    const ranking = rankingWith(ASCENDING, ASCENDING, ASCENDING)

    const entry = returnRiskEntries(ranking).find((item) => item.supplierId === 'P8')

    expect(entry).toEqual({
      ruleId: 'ReturnRisk',
      supplierId: 'P8',
      principalName: 'Principal 8',
      supportingMetric: 8,
      percentile: 80,
      populationSize: 10,
    })
  })
})

describe('dependencyRiskEntries', () => {
  it('flags a Principal at exactly Dependency P90', () => {
    const dependencies = ASCENDING.map((value, index) =>
      makeDependency({
        supplierId: `P${index}`,
        principalName: `Principal ${index}`,
        dependencyRatio: value,
      }),
    )

    const entries = dependencyRiskEntries(dependencies)

    expect(entries).toHaveLength(1)
    expect(entries[0].supplierId).toBe('P9')
  })

  it('does not flag a Principal below Dependency P90', () => {
    const dependencies = ASCENDING.slice(0, 9).map((value, index) =>
      makeDependency({ supplierId: `P${index}`, dependencyRatio: value }),
    )

    const entries = dependencyRiskEntries(dependencies)

    expect(entries.map((entry) => entry.supplierId)).not.toContain('P8')
  })

  it('excludes a Principal with a null DependencyRatio', () => {
    const dependencies = [
      ...ASCENDING.slice(0, 9).map((value, index) =>
        makeDependency({ supplierId: `P${index}`, dependencyRatio: value }),
      ),
      makeDependency({ supplierId: 'P-NONE', dependencyRatio: null }),
    ]

    const entries = dependencyRiskEntries(dependencies)

    expect(entries.map((entry) => entry.supplierId)).not.toContain('P-NONE')
    expect(entries).toEqual([])
  })

  it('reports the rule, supporting metric, percentile, and population size', () => {
    const dependencies = ASCENDING.map((value, index) =>
      makeDependency({
        supplierId: `P${index}`,
        principalName: `Principal ${index}`,
        dependencyRatio: value,
      }),
    )

    expect(dependencyRiskEntries(dependencies)[0]).toEqual({
      ruleId: 'DependencyRisk',
      supplierId: 'P9',
      principalName: 'Principal 9',
      supportingMetric: 9,
      percentile: 90,
      populationSize: 10,
    })
  })
})

describe('underperformingEntries', () => {
  it('flags a Principal at exactly Achievement P25', () => {
    const ranking = rankingWith([0, 1, 2, 3], [0, 1, 2, 3])

    expect(underperformingEntries(ranking).map((entry) => entry.supplierId)).toContain('P1')
  })

  it('does not flag a Principal above Achievement P25', () => {
    const ranking = rankingWith([0, 1, 2, 3], [0, 1, 2, 3])

    expect(underperformingEntries(ranking).map((entry) => entry.supplierId)).not.toContain('P2')
  })

  it('excludes a Principal with a null Achievement metric', () => {
    const ranking = rankingWith([0, 1, 2, 3], [0, 1, null, 3])

    expect(underperformingEntries(ranking).map((entry) => entry.supplierId)).not.toContain('P2')
  })

  it('reports the rule, supporting metric, percentile, and population size', () => {
    const ranking = rankingWith([0, 1, 2, 3], [0, 1, 2, 3])

    const entry = underperformingEntries(ranking).find((item) => item.supplierId === 'P1')

    expect(entry).toEqual({
      ruleId: 'Underperforming',
      supplierId: 'P1',
      principalName: 'Principal 1',
      supportingMetric: 1,
      percentile: 25,
      populationSize: 4,
    })
  })
})

describe('opportunityRiskBoard', () => {
  it('partitions opportunities and risks and produces no alert records', () => {
    const ranking = rankingWith(ASCENDING, ASCENDING, ASCENDING)
    const dependencies = ASCENDING.map((value, index) =>
      makeDependency({ supplierId: `P${index}`, dependencyRatio: value }),
    )

    const board = opportunityRiskBoard(ranking, dependencies)

    expect(Object.keys(board).sort()).toEqual(['opportunities', 'risks'])
    expect(board.opportunities).toEqual(opportunityEntries(ranking))
    expect(board.risks.map((entry) => entry.ruleId)).toContain('ReturnRisk')
    expect(board.risks.map((entry) => entry.ruleId)).toContain('DependencyRisk')
    expect(board.risks.map((entry) => entry.ruleId)).toContain('Underperforming')
    expect(board.risks.map((entry) => entry.ruleId)).not.toContain('Opportunity')
  })

  it('allows the same Principal to coincide in multiple rules', () => {
    const ranking = rankingWith(
      [0, 1, 2, 3, 4, 5, 6, 9, 8, 10],
      [10, 11, 12, 13, 14, 15, 16, 0, 17, 18],
    )

    const board = opportunityRiskBoard(ranking, [])

    const opportunity = board.opportunities.find((entry) => entry.supplierId === 'P7')
    const underperforming = board.risks.find(
      (entry) => entry.ruleId === 'Underperforming' && entry.supplierId === 'P7',
    )

    expect(opportunity).toBeDefined()
    expect(underperforming).toBeDefined()
  })
})
