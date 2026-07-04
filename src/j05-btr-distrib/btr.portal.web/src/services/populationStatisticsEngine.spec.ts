import { describe, expect, it } from 'vitest'
import type { PopulationMapPoint } from '@/models/entityAnalytics'
import {
  analyzePopulationMap,
  businessToVisual,
  populationProjectionEngine,
} from '@/services/populationStatisticsEngine'

function makePoint(overrides: Partial<PopulationMapPoint> = {}): PopulationMapPoint {
  return {
    EntityId: 'E1',
    EntityCode: 'E1',
    DisplayName: 'Entity One',
    AxisX: 1,
    AxisY: 1,
    FormattedAxisX: '1',
    FormattedAxisY: '1',
    AxisXPercentile: 50,
    AxisYPercentile: 50,
    DimensionValue: null,
    IsActive: true,
    ActiveAttentionCount: 0,
    MatchesFilter: true,
    ...overrides,
  }
}

/** @deprecated module — core tests live in populationProjectionEngine.spec.ts */
describe('populationStatisticsEngine (deprecated re-export)', () => {
  it('re-exports businessToVisual', () => {
    expect(businessToVisual(9)).toBeCloseTo(1, 10)
  })

  it('analyzePopulationMap delegates to projection engine', () => {
    const points = [makePoint({ EntityId: 'a', AxisX: 10, AxisY: 20 })]
    const legacy = analyzePopulationMap(points)
    const modern = populationProjectionEngine.project(points)
    expect(legacy).not.toBeNull()
    expect(legacy!.points.size).toBe(modern!.entities.size)
  })
})
