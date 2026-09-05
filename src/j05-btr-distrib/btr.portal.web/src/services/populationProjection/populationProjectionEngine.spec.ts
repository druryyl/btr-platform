import { describe, expect, it } from 'vitest'
import type { PopulationMapPoint } from '@/models/entityAnalytics'
import {
  analyzePopulationMap,
  businessToVisual,
  classifyByResidualMagnitude,
  fitTheilSenRegression,
  IDR_PROJECTION_FLOOR,
  DAYS_PROJECTION_CAP,
  populationProjectionEngine,
  projectedEntityToAnalyzed,
  resolveDeviationLabel,
} from '@/services/populationProjection/populationProjectionEngine'
import { businessToLog, robustNormalize } from '@/services/populationProjection/robustStats'
import { robustProjectionStrategy } from '@/services/populationProjection/strategies/robustProjectionStrategy'

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

describe('robustNormalize', () => {
  it('centers values near zero using MAD', () => {
    const values = [1, 2, 3, 4, 5]
    const result = robustNormalize(values)
    expect(result.method).toBe('mad')
    const mean = result.normalized.reduce((s, v) => s + v, 0) / result.normalized.length
    expect(Math.abs(mean)).toBeLessThan(0.5)
  })

  it('falls back to stddev when MAD is near zero', () => {
    const values = [5, 5, 5, 5]
    const result = robustNormalize(values)
    expect(result.method).toBe('stddev')
    expect(result.normalized.every((v) => v === 0)).toBe(true)
  })
})

describe('fitTheilSenRegression', () => {
  it('fits y = 2x + 1 for collinear data in projection space', () => {
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

describe('populationProjectionEngine', () => {
  it('returns null for empty input', () => {
    expect(populationProjectionEngine.project([])).toBeNull()
  })

  it('skips points with null axes', () => {
    const result = populationProjectionEngine.project([
      makePoint({ EntityId: 'a', AxisX: 10, AxisY: 20 }),
      makePoint({ EntityId: 'b', AxisX: null, AxisY: 5 }),
    ])
    expect(result).not.toBeNull()
    expect(result!.entities.size).toBe(1)
    expect(result!.metadata.excludedCount).toBe(1)
  })

  it('uses robust strategy by default', () => {
    const result = populationProjectionEngine.project([
      makePoint({ EntityId: 'a', AxisX: 100, AxisY: 200 }),
    ])
    expect(result!.strategyId).toBe('robust')
  })

  it('produces band geometry spanning projection x extent', () => {
    const points = [
      makePoint({ EntityId: 'a', AxisX: 0, AxisY: 0 }),
      makePoint({ EntityId: 'b', AxisX: 100, AxisY: 200 }),
    ]
    const result = populationProjectionEngine.project(points)
    expect(result).not.toBeNull()
    expect(result!.bandGeometry.center.length).toBeGreaterThan(10)
    expect(result!.entities.size).toBe(2)
  })

  it('classifies proportional outlier as critical', () => {
    const points = Array.from({ length: 30 }, (_, i) => {
      const raw = (i + 1) * 1000
      return makePoint({ EntityId: `e${i}`, AxisX: raw, AxisY: raw })
    })
    points.push(makePoint({ EntityId: 'outlier', AxisX: 15_000, AxisY: 15_000 * 100 }))

    const result = populationProjectionEngine.project(points)
    const outlier = result!.entities.get('outlier')
    expect(outlier?.statisticalClass).toBe('critical')
  })

  it('fits proportional business data with slope near one in projection space', () => {
    const points = Array.from({ length: 40 }, (_, i) => {
      const raw = Math.pow(10, i * 0.15)
      return makePoint({
        EntityId: `p${i}`,
        AxisX: raw,
        AxisY: raw * 2,
      })
    })

    const result = populationProjectionEngine.project(points)
    expect(result!.regression.slope).toBeGreaterThan(0.7)
    expect(result!.regression.slope).toBeLessThan(1.3)
  })

  it('includes zero-axis points without NaN', () => {
    const points = [
      makePoint({ EntityId: 'a', AxisX: 0, AxisY: 0 }),
      makePoint({ EntityId: 'b', AxisX: 100, AxisY: 200 }),
    ]
    const result = populationProjectionEngine.project(points)
    for (const entity of result!.entities.values()) {
      expect(Number.isFinite(entity.projectionExpectedY)).toBe(true)
      expect(Number.isFinite(entity.projectionResidual)).toBe(true)
      expect(Number.isFinite(entity.absResidualMad)).toBe(true)
    }
  })

  it('provides axis guides with business values', () => {
    const points = Array.from({ length: 20 }, (_, i) =>
      makePoint({ EntityId: `e${i}`, AxisX: (i + 1) * 1000, AxisY: (i + 1) * 800 }),
    )
    const result = populationProjectionEngine.project(points)
    const xGuides = result!.axisGuides.filter((g) => g.axis === 'x')
    expect(xGuides.length).toBeGreaterThan(0)
    expect(xGuides.every((g) => g.businessValue > 0)).toBe(true)
    expect(xGuides.every((g) => Number.isFinite(g.projectionValue))).toBe(true)
  })

  it('maps projectedEntityToAnalyzed for tooltip compatibility', () => {
    const result = populationProjectionEngine.project([
      makePoint({ EntityId: 'a', AxisX: 50, AxisY: 100 }),
    ])
    const entity = result!.entities.get('a')!
    const analyzed = projectedEntityToAnalyzed(entity)
    expect(analyzed.rawX).toBe(50)
    expect(analyzed.rawY).toBe(100)
    expect(analyzed.statisticalClass).toBe(entity.statisticalClass)
  })

  it('applies IDR projection floor so tiny Y values share floor log projection', () => {
    const points = [
      ...Array.from({ length: 20 }, (_, i) =>
        makePoint({
          EntityId: `zero${i}`,
          AxisX: 50_000_000 + i * 1_000_000,
          AxisY: i % 2 === 0 ? 0 : 500,
        }),
      ),
      makePoint({ EntityId: 'atFloor', AxisX: 70_000_000, AxisY: IDR_PROJECTION_FLOOR }),
      makePoint({ EntityId: 'mid', AxisX: 80_000_000, AxisY: 50_000_000 }),
      makePoint({ EntityId: 'high', AxisX: 120_000_000, AxisY: 500_000_000 }),
    ]

    const withFloor = populationProjectionEngine.project(points, {
      axisXUnit: 'IDR',
      axisYUnit: 'IDR',
    })!
    const withoutFloor = populationProjectionEngine.project(points)!

    const zeroEntity = withFloor.entities.get('zero0')!
    const floorEntity = withFloor.entities.get('atFloor')!
    expect(zeroEntity.businessY).toBe(0)
    expect(floorEntity.businessY).toBe(IDR_PROJECTION_FLOOR)

    // All sub-floor Y values project like the explicit floor peer in the same population
    for (const [id, entity] of withFloor.entities) {
      if (id.startsWith('zero')) {
        expect(entity.projectionY).toBeCloseTo(floorEntity.projectionY, 8)
      }
    }

    // Without floor, zeros sit lower in projection space than the floor peer
    const unflooredZero = withoutFloor.entities.get('zero0')!
    const unflooredFloor = withoutFloor.entities.get('atFloor')!
    expect(unflooredZero.projectionY).toBeLessThan(unflooredFloor.projectionY)
  })

  it('does not apply IDR floor when axis unit is Days', () => {
    const points = [
      makePoint({ EntityId: 'a', AxisX: 1_000_000, AxisY: 0 }),
      makePoint({ EntityId: 'b', AxisX: 2_000_000, AxisY: 30 }),
      makePoint({ EntityId: 'c', AxisX: 3_000_000, AxisY: 400 }),
    ]
    const daysY = populationProjectionEngine.project(points, {
      axisXUnit: 'IDR',
      axisYUnit: 'Days',
    })!
    const idrY = populationProjectionEngine.project(points, {
      axisXUnit: 'IDR',
      axisYUnit: 'IDR',
    })!

    expect(daysY.entities.get('a')!.businessY).toBe(0)
    expect(daysY.entities.get('a')!.projectionY).not.toBeCloseTo(
      idrY.entities.get('a')!.projectionY,
      2,
    )
  })

  it('applies Days projection cap so values above 365 share cap projection', () => {
    const points = [
      ...Array.from({ length: 18 }, (_, i) =>
        makePoint({
          EntityId: `mid${i}`,
          AxisX: 10_000_000 + i * 500_000,
          AxisY: 20 + i * 2,
        }),
      ),
      makePoint({ EntityId: 'low', AxisX: 20_000_000, AxisY: 30 }),
      makePoint({ EntityId: 'atCap', AxisX: 25_000_000, AxisY: DAYS_PROJECTION_CAP }),
      makePoint({ EntityId: 'over', AxisX: 30_000_000, AxisY: 400 }),
      makePoint({ EntityId: 'extreme', AxisX: 40_000_000, AxisY: 10_000 }),
    ]

    const result = populationProjectionEngine.project(points, {
      axisXUnit: 'IDR',
      axisYUnit: 'Days',
    })!

    const low = result.entities.get('low')!
    const atCap = result.entities.get('atCap')!
    const over = result.entities.get('over')!
    const extreme = result.entities.get('extreme')!

    expect(over.businessY).toBe(400)
    expect(extreme.businessY).toBe(10_000)
    expect(atCap.businessY).toBe(DAYS_PROJECTION_CAP)

    expect(over.projectionY).toBeCloseTo(atCap.projectionY, 8)
    expect(extreme.projectionY).toBeCloseTo(atCap.projectionY, 8)
    expect(low.projectionY).toBeLessThan(atCap.projectionY)
  })
})

describe('robustProjectionStrategy', () => {
  it('never throws on invalid data', () => {
    expect(() =>
      robustProjectionStrategy.project([
        { entityId: 'a', label: 'A', businessX: NaN, businessY: 1 },
        { entityId: 'b', label: 'B', businessX: 1, businessY: Infinity },
      ]),
    ).not.toThrow()
  })
})

describe('analyzePopulationMap (deprecated)', () => {
  it('delegates to projection engine', () => {
    const points = [makePoint({ EntityId: 'a', AxisX: 10, AxisY: 20 })]
    const legacy = analyzePopulationMap(points)
    const modern = populationProjectionEngine.project(points)
    expect(legacy).not.toBeNull()
    expect(legacy!.points.size).toBe(modern!.entities.size)
  })
})

describe('populationProjectionEngine performance', () => {
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
    const result = populationProjectionEngine.project(points)
    const elapsed = performance.now() - start

    expect(result).not.toBeNull()
    expect(result!.entities.size).toBe(5000)
    expect(elapsed).toBeLessThan(500)
  })
})

describe('businessToLog consistency', () => {
  it('matches businessToVisual alias', () => {
    expect(businessToLog(100)).toBe(businessToVisual(100))
  })
})
