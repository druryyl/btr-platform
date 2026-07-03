import { describe, expect, it } from 'vitest'
import type { PopulationMapPoint } from '@/models/entityAnalytics'
import {
  analyzePopulationMap,
  businessToVisual,
  classifyByResidualMagnitude,
  computeAnalyzedPoints,
  extractVisualPairs,
  fitTheilSenRegression,
  resolveDeviationLabel,
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

describe('businessToVisual', () => {
  it('maps zero to zero', () => {
    expect(businessToVisual(0)).toBe(0)
  })

  it('maps nine to log10(10)', () => {
    expect(businessToVisual(9)).toBeCloseTo(1, 10)
  })

  it('clamps negatives before transform', () => {
    expect(businessToVisual(-100)).toBe(0)
    expect(Number.isFinite(businessToVisual(-100))).toBe(true)
  })
})

describe('extractVisualPairs', () => {
  it('skips points with null axes', () => {
    const pairs = extractVisualPairs([
      makePoint({ EntityId: 'a', AxisX: 10, AxisY: 20 }),
      makePoint({ EntityId: 'b', AxisX: null, AxisY: 5 }),
    ])
    expect(pairs).toHaveLength(1)
    expect(pairs[0].entityId).toBe('a')
    expect(pairs[0].rawX).toBe(10)
    expect(pairs[0].visualX).toBeCloseTo(businessToVisual(10), 10)
  })
})

describe('fitTheilSenRegression', () => {
  it('fits y = 2x + 1 for collinear data in visual space', () => {
    const pairs = [
      { entityId: 'a', x: 0, y: 1 },
      { entityId: 'b', x: 1, y: 3 },
      { entityId: 'c', x: 2, y: 5 },
      { entityId: 'd', x: 3, y: 7 },
    ]
    const model = fitTheilSenRegression(pairs)
    expect(model.slope).toBeCloseTo(2, 5)
    expect(model.intercept).toBeCloseTo(1, 5)
  })

  it('returns horizontal line for single point', () => {
    const model = fitTheilSenRegression([{ entityId: 'a', x: 5, y: 42 }])
    expect(model.slope).toBe(0)
    expect(model.intercept).toBe(42)
    expect(model.predictY(100)).toBe(42)
  })

  it('returns horizontal line when all x values are identical', () => {
    const model = fitTheilSenRegression([
      { entityId: 'a', x: 5, y: 10 },
      { entityId: 'b', x: 5, y: 20 },
      { entityId: 'c', x: 5, y: 30 },
    ])
    expect(model.slope).toBe(0)
    expect(model.intercept).toBeCloseTo(20, 5)
  })

  it('fits proportional business data with slope near one in log space', () => {
    const visualPairs = Array.from({ length: 20 }, (_, i) => {
      const raw = Math.pow(10, i * 0.3)
      return {
        entityId: `p${i}`,
        rawX: raw,
        rawY: raw * 2,
        visualX: businessToVisual(raw),
        visualY: businessToVisual(raw * 2),
      }
    })

    const regressionPairs = visualPairs.map((p) => ({
      entityId: p.entityId,
      x: p.visualX,
      y: p.visualY,
    }))
    const model = fitTheilSenRegression(regressionPairs)
    expect(model.slope).toBeGreaterThan(0.5)
    expect(model.slope).toBeLessThan(1.5)
  })
})

describe('classifyByResidualMagnitude', () => {
  it('classifies by MAD ratio thresholds', () => {
    expect(classifyByResidualMagnitude(0.5)).toBe('normal')
    expect(classifyByResidualMagnitude(1.5)).toBe('watch')
    expect(classifyByResidualMagnitude(2.5)).toBe('attention')
    expect(classifyByResidualMagnitude(4)).toBe('critical')
  })
})

describe('resolveDeviationLabel', () => {
  it('returns Within Expected for normal class', () => {
    expect(resolveDeviationLabel(5, 'normal')).toBe('Within Expected')
  })

  it('returns Above/Below Expected for non-normal classes', () => {
    expect(resolveDeviationLabel(3, 'attention')).toBe('Above Expected')
    expect(resolveDeviationLabel(-3, 'critical')).toBe('Below Expected')
  })
})

describe('computeAnalyzedPoints', () => {
  it('classifies collinear log-space data as all normal', () => {
    const pairs = Array.from({ length: 20 }, (_, i) => {
      const visualX = i
      const visualY = 2 * i + 1
      return {
        entityId: `e${i}`,
        rawX: Math.pow(10, visualX) - 1,
        rawY: Math.pow(10, visualY) - 1,
        visualX,
        visualY,
      }
    })
    const model = fitTheilSenRegression(
      pairs.map((p) => ({ entityId: p.entityId, x: p.visualX, y: p.visualY })),
    )
    const { analyzed } = computeAnalyzedPoints(pairs, model)
    expect(analyzed.every((a) => a.statisticalClass === 'normal')).toBe(true)
  })

  it('classifies a proportional outlier as critical', () => {
    const pairs = Array.from({ length: 30 }, (_, i) => {
      const raw = (i + 1) * 1000
      return {
        entityId: `e${i}`,
        rawX: raw,
        rawY: raw,
        visualX: businessToVisual(raw),
        visualY: businessToVisual(raw),
      }
    })
    const outlierRaw = 15_000
    pairs.push({
      entityId: 'outlier',
      rawX: outlierRaw,
      rawY: outlierRaw * 100,
      visualX: businessToVisual(outlierRaw),
      visualY: businessToVisual(outlierRaw * 100),
    })

    const model = fitTheilSenRegression(
      pairs.map((p) => ({ entityId: p.entityId, x: p.visualX, y: p.visualY })),
    )
    const { analyzed } = computeAnalyzedPoints(pairs, model)
    const outlier = analyzed.find((a) => a.entityId === 'outlier')
    expect(outlier?.statisticalClass).toBe('critical')
  })

  it('places most points within ±1 MAD for symmetric noise in log space', () => {
    const pairs = Array.from({ length: 200 }, (_, i) => {
      const visualX = i * 0.1
      const noise = (i % 3) - 1
      const visualY = visualX + noise * 0.05
      return {
        entityId: `e${i}`,
        rawX: Math.pow(10, visualX) - 1,
        rawY: Math.pow(10, visualY) - 1,
        visualX,
        visualY,
      }
    })
    const model = fitTheilSenRegression(
      pairs.map((p) => ({ entityId: p.entityId, x: p.visualX, y: p.visualY })),
    )
    const { analyzed } = computeAnalyzedPoints(pairs, model)
    const withinOne = analyzed.filter((a) => a.absResidualMad <= 1).length
    expect(withinOne / analyzed.length).toBeGreaterThanOrEqual(0.5)
  })

  it('includes zero-axis points without NaN', () => {
    const points = [
      makePoint({ EntityId: 'a', AxisX: 0, AxisY: 0 }),
      makePoint({ EntityId: 'b', AxisX: 100, AxisY: 200 }),
    ]
    const analysis = analyzePopulationMap(points)
    expect(analysis).not.toBeNull()
    for (const ap of analysis!.points.values()) {
      expect(Number.isFinite(ap.visualExpectedY)).toBe(true)
      expect(Number.isFinite(ap.visualResidual)).toBe(true)
      expect(Number.isFinite(ap.absResidualMad)).toBe(true)
    }
  })
})

describe('analyzePopulationMap', () => {
  it('returns null for empty input', () => {
    expect(analyzePopulationMap([])).toBeNull()
  })

  it('produces band geometry spanning visual x extent', () => {
    const points = [
      makePoint({ EntityId: 'a', AxisX: 0, AxisY: 0 }),
      makePoint({ EntityId: 'b', AxisX: 100, AxisY: 200 }),
    ]
    const analysis = analyzePopulationMap(points)
    expect(analysis).not.toBeNull()
    expect(analysis!.bandGeometry.center.length).toBeGreaterThan(10)
    expect(analysis!.points.size).toBe(2)
    expect(analysis!.bandGeometry.xMin).toBeLessThanOrEqual(businessToVisual(0))
    expect(analysis!.bandGeometry.xMax).toBeGreaterThanOrEqual(businessToVisual(100))
  })
})

describe('analyzePopulationMap performance', () => {
  it('completes within 500ms for 5000 Pareto-like points', () => {
    const points: PopulationMapPoint[] = Array.from({ length: 5000 }, (_, i) => {
      const x = Math.pow(i + 1, 1.5)
      const y = x * 1.2 + (Math.random() - 0.5) * x * 0.1
      return makePoint({
        EntityId: `e${i}`,
        AxisX: x,
        AxisY: y,
      })
    })

    const start = performance.now()
    const analysis = analyzePopulationMap(points)
    const elapsed = performance.now() - start

    expect(analysis).not.toBeNull()
    expect(analysis!.points.size).toBe(5000)
    expect(elapsed).toBeLessThan(500)
  })
})
