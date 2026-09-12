import { describe, expect, it } from 'vitest'
import type { PopulationMapPoint } from '@/models/entityAnalytics'
import type { AnalyzedPoint } from '@/services/populationProjection/populationProjectionEngine'
import { populationProjectionEngine } from '@/services/populationProjection/populationProjectionEngine'
import {
  buildAutoLabelCandidates,
  buildDaysCalendarAxisTicks,
  buildIdrNiceAxisTicks,
  buildTransform,
  classifyPacingQuadrant,
  computeBounds,
  computeRawExtents,
  dataToScreen,
  DAYS_CALENDAR_EDGES,
  DAYS_YEAR_EDGE,
  ensureZeroTick,
  formatAxisTickValue,
  generateIdrNiceEdges,
  generateLinearAxisTicks,
  generateProjectionAxisGuides,
  generateProjectionGridTicks,
  isBusinessZeroWithinBounds,
  isDaysAxisUnit,
  isIdrAxisUnit,
  plotPoints,
  projectionToScreenX,
  resolveBusinessAttentionTier,
  resolveBusinessZeroProjection,
  resolveLabelPlacements,
  resolvePacingQuadrantBoundaries,
  resolvePopulationAxisTicks,
  resolveVisualTier,
  formatBinRangeLabel,
  CRITICAL_PERCENTILE_THRESHOLD,
} from '@/services/populationMapLayout'

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

function makePlotted(
  point: PopulationMapPoint,
  screenX: number,
  screenY: number,
  index = 0,
  analyzed?: AnalyzedPoint,
  labelPriority?: number,
) {
  return { point, screenX, screenY, index, analyzed, labelPriority }
}

function makeParetoPoints(count: number): PopulationMapPoint[] {
  return Array.from({ length: count }, (_, i) => {
    const axis = Math.pow(i + 1, 2.5)
    return makePoint({
      EntityId: `p${i}`,
      AxisX: axis,
      AxisY: axis * 0.8,
    })
  })
}

function project(points: PopulationMapPoint[]) {
  return populationProjectionEngine.project(points)!
}

describe('computeBounds', () => {
  it('returns projection bounds with padding', () => {
    const points = [
      makePoint({ EntityId: 'a', AxisX: 0, AxisY: 0 }),
      makePoint({ EntityId: 'b', AxisX: 100, AxisY: 200 }),
    ]
    const projection = project(points)
    const bounds = computeBounds(projection)

    expect(bounds.maxX).toBeGreaterThan(bounds.minX)
    expect(bounds.maxY).toBeGreaterThan(bounds.minY)
    expect(bounds.minX).toBeLessThanOrEqual(projection.bounds.minX)
    expect(bounds.maxX).toBeGreaterThanOrEqual(projection.bounds.maxX)
  })

  it('returns default bounds when projection is null', () => {
    expect(computeBounds(null)).toEqual({ minX: 0, maxX: 1, minY: 0, maxY: 1 })
  })

  it('yields balanced projection span for Pareto data', () => {
    const points = makeParetoPoints(100)
    const projection = project(points)
    const bounds = computeBounds(projection)
    const spanX = bounds.maxX - bounds.minX
    const spanY = bounds.maxY - bounds.minY
    const ratio = spanX / spanY
    expect(ratio).toBeGreaterThan(0.3)
    expect(ratio).toBeLessThan(3)
  })
})

describe('generateProjectionAxisGuides', () => {
  it('returns business-labeled ticks in projection space', () => {
    const points = Array.from({ length: 20 }, (_, i) =>
      makePoint({ EntityId: `e${i}`, AxisX: (i + 1) * 1000, AxisY: (i + 1) * 800 }),
    )
    const projection = project(points)
    const { xTicks, yTicks } = generateProjectionAxisGuides(projection)

    expect(xTicks.length).toBeGreaterThan(0)
    expect(yTicks.length).toBeGreaterThan(0)
    for (let i = 1; i < xTicks.length; i++) {
      expect(xTicks[i].projectionValue).toBeGreaterThanOrEqual(xTicks[i - 1].projectionValue)
    }
  })
})

describe('generateProjectionGridTicks', () => {
  it('produces evenly spaced projection grid ticks', () => {
    const bounds = { minX: -2, maxX: 2, minY: -1, maxY: 1 }
    const { xTicks, yTicks } = generateProjectionGridTicks(bounds)
    expect(xTicks.length).toBeGreaterThan(2)
    expect(yTicks.length).toBeGreaterThan(2)
  })
})

describe('generateLinearAxisTicks', () => {
  it('produces evenly spaced ticks for a numeric range', () => {
    const ticks = generateLinearAxisTicks(0, 100)
    expect(ticks.length).toBeGreaterThan(2)
    expect(ticks[0]).toBeGreaterThanOrEqual(0)
    expect(ticks[ticks.length - 1]).toBeLessThanOrEqual(100)
  })
})

describe('buildDaysCalendarAxisTicks', () => {
  const norm = { center: 1, scale: 0.5 }

  it('uses week/month ladder clipped to data range', () => {
    const ticks = buildDaysCalendarAxisTicks(norm, 0, 100)
    const values = ticks.map((t) => t.businessValue)
    expect(values).toEqual([0, 7, 14, 21, 30, 60, 90])
    expect(values).not.toContain(365)
  })

  it('includes 365 when dataMax is above one year', () => {
    const ticks = buildDaysCalendarAxisTicks(norm, 5, 800)
    const values = ticks.map((t) => t.businessValue)
    expect(values[0]).toBe(0)
    expect(values).toContain(30)
    expect(values).toContain(365)
    expect(values.every((v) => DAYS_CALENDAR_EDGES.includes(v as (typeof DAYS_CALENDAR_EDGES)[number]))).toBe(true)
  })

  it('projects ticks with finite projection values', () => {
    const ticks = buildDaysCalendarAxisTicks(norm, 0, 400)
    expect(ticks.length).toBeGreaterThan(0)
    for (const tick of ticks) {
      expect(Number.isFinite(tick.projectionValue)).toBe(true)
    }
  })

  it('detects Days axis units', () => {
    expect(isDaysAxisUnit('Days')).toBe(true)
    expect(isDaysAxisUnit('days')).toBe(true)
    expect(isDaysAxisUnit('IDR')).toBe(false)
  })

  it('projects 365 tick with Days cap transform', () => {
    const ticks = buildDaysCalendarAxisTicks(norm, 0, 10_000)
    const tick365 = ticks.find((t) => t.businessValue === 365)!
    expect(tick365).toBeDefined()
    // Cap does not change 365 itself; ensures finite projection aligned with DAYS_YEAR_EDGE
    expect(DAYS_YEAR_EDGE).toBe(365)
    expect(Number.isFinite(tick365.projectionValue)).toBe(true)
  })
})

describe('formatAxisTickValue Days cap label', () => {
  it('returns 365+ when value is cap and dataMax exceeds one year', () => {
    expect(formatAxisTickValue(365, 'Days', { dataMax: 800 })).toBe('365+')
  })

  it('returns 365 when dataMax is at or below the cap', () => {
    expect(formatAxisTickValue(365, 'Days', { dataMax: 365 })).toBe('365')
    expect(formatAxisTickValue(365, 'Days')).toBe('365')
  })
})

describe('buildIdrNiceAxisTicks', () => {
  const norm = { center: 6, scale: 1 }

  function isNiceIdr(value: number): boolean {
    if (value <= 0) return false
    const exp = Math.floor(Math.log10(value))
    const scale = 10 ** exp
    const mant = value / scale
    return [1, 2, 5].some((m) => Math.abs(mant - m) < 1e-9)
  }

  it('uses 1-2-5 Rupiah edges within range', () => {
    const edges = generateIdrNiceEdges(1_000_000, 100_000_000)
    expect(edges).toContain(1_000_000)
    expect(edges).toContain(2_000_000)
    expect(edges).toContain(5_000_000)
    expect(edges).toContain(10_000_000)
    expect(edges).toContain(50_000_000)
    expect(edges).toContain(100_000_000)
    expect(edges.every(isNiceIdr)).toBe(true)
  })

  it('clips to data range and keeps a readable tick count', () => {
    const ticks = buildIdrNiceAxisTicks(norm, 5_000_000, 800_000_000)
    const values = ticks.map((t) => t.businessValue)
    expect(values.every((v) => v >= 5_000_000 && v <= 800_000_000)).toBe(true)
    expect(values.every(isNiceIdr)).toBe(true)
    expect(ticks.length).toBeGreaterThan(2)
    expect(ticks.length).toBeLessThanOrEqual(12)
  })

  it('projects ticks with finite projection values', () => {
    const ticks = buildIdrNiceAxisTicks(norm, 100_000, 1_000_000_000)
    expect(ticks.length).toBeGreaterThan(0)
    for (const tick of ticks) {
      expect(Number.isFinite(tick.projectionValue)).toBe(true)
    }
  })

  it('anchors a 0 edge and starts the ladder at the floor when data starts at 0', () => {
    const edges = generateIdrNiceEdges(0, 100_000_000)
    expect(edges[0]).toBe(0)
    expect(edges).toContain(10_000)
    expect(edges).toContain(1_000_000)
    expect(edges.slice(1).every((e) => e >= 10_000)).toBe(true)
    expect(edges.slice(1).every(isNiceIdr)).toBe(true)
  })

  it('builds a 0 tick projected consistently with compressed IDR log', () => {
    const ticks = buildIdrNiceAxisTicks(norm, 0, 100_000_000)
    expect(ticks.length).toBeGreaterThan(1)
    expect(ticks[0].businessValue).toBe(0)
    expect(Number.isFinite(ticks[0].projectionValue)).toBe(true)
    const ladder = ticks.slice(1).map((t) => t.businessValue)
    expect(ladder.every((v) => v >= 10_000)).toBe(true)
    expect(ladder.every(isNiceIdr)).toBe(true)
  })

  it('detects IDR axis units', () => {
    expect(isIdrAxisUnit('IDR')).toBe(true)
    expect(isIdrAxisUnit('idr')).toBe(true)
    expect(isIdrAxisUnit('Days')).toBe(false)
  })
})

describe('resolvePopulationAxisTicks', () => {
  const norm = { center: 6, scale: 1 }
  const fallback = [{ businessValue: 42, projectionValue: 0 }]

  it('returns IDR nice ticks without throwing', () => {
    const ticks = resolvePopulationAxisTicks('IDR', norm, 1_000_000, 100_000_000, fallback)
    expect(ticks.length).toBeGreaterThan(0)
    expect(ticks.map((t) => t.businessValue)).toContain(1_000_000)
    expect(ticks.map((t) => t.businessValue)).toContain(10_000_000)
  })

  it('returns Days calendar ticks for Days unit', () => {
    const ticks = resolvePopulationAxisTicks('Days', norm, 0, 100, fallback)
    expect(ticks.map((t) => t.businessValue)).toEqual([0, 7, 14, 21, 30, 60, 90])
  })

  it('returns fallback for other units', () => {
    expect(resolvePopulationAxisTicks('Percent', norm, 0, 100, fallback)).toBe(fallback)
  })
})

describe('dataToScreen with projection bounds', () => {
  it('maps known coordinates to expected screen positions', () => {
    const points = [
      makePoint({ AxisX: 0, AxisY: 0 }),
      makePoint({ EntityId: 'E2', AxisX: 100, AxisY: 100 }),
    ]
    const projection = project(points)
    const bounds = computeBounds(projection)
    const transform = buildTransform(800, 600, bounds)

    const minScreen = dataToScreen(bounds.minX, bounds.minY, transform)
    const maxScreen = dataToScreen(bounds.maxX, bounds.maxY, transform)

    expect(minScreen.x).toBeCloseTo(transform.offsetX, 0)
    expect(maxScreen.x).toBeCloseTo(transform.offsetX + transform.plotWidth, 0)
    expect(minScreen.y).toBeCloseTo(transform.offsetY + transform.plotHeight, 0)
    expect(maxScreen.y).toBeCloseTo(transform.offsetY, 0)
  })
})

describe('projectionToScreenX', () => {
  it('maps projection values to screen x', () => {
    const bounds = { minX: 0, maxX: 10, minY: 0, maxY: 10 }
    const transform = buildTransform(400, 300, bounds)
    const screen = projectionToScreenX(5, transform)
    expect(screen).toBeGreaterThan(transform.offsetX)
    expect(screen).toBeLessThan(transform.offsetX + transform.plotWidth)
  })
})

describe('plotPoints', () => {
  it('plots using projection coordinates', () => {
    const points = [
      makePoint({ AxisX: 0, AxisY: 0 }),
      makePoint({ EntityId: 'E2', AxisX: 100, AxisY: 200 }),
    ]
    const projection = project(points)
    const bounds = computeBounds(projection)
    const transform = buildTransform(400, 300, bounds)
    const plotted = plotPoints(points, transform, projection)

    expect(plotted).toHaveLength(2)
    expect(plotted[0].screenX).not.toBeCloseTo(plotted[1].screenX, 0)
    expect(plotted[0].screenY).not.toBeCloseTo(plotted[1].screenY, 0)
    expect(plotted[0].analyzed).toBeDefined()
    expect(plotted[0].labelPriority).toBeDefined()
  })

  it('separates low and high business values on screen for long-tail data', () => {
    const points = makeParetoPoints(200)
    const projection = project(points)
    const bounds = computeBounds(projection)
    const transform = buildTransform(800, 600, bounds)
    const plotted = plotPoints(points, transform, projection)

    const small = plotted.find((p) => p.point.EntityId === 'p0')!
    const large = plotted.find((p) => p.point.EntityId === 'p199')!

    const screenSpanX = Math.abs(large.screenX - small.screenX)
    const screenSpanY = Math.abs(large.screenY - small.screenY)
    expect(screenSpanX).toBeGreaterThan(transform.plotWidth * 0.3)
    expect(screenSpanY).toBeGreaterThan(transform.plotHeight * 0.3)
  })
})

describe('computeRawExtents', () => {
  it('returns raw min/max without transform', () => {
    const points = [
      makePoint({ AxisX: 5, AxisY: 10 }),
      makePoint({ EntityId: 'E2', AxisX: 500, AxisY: 200 }),
    ]
    expect(computeRawExtents(points)).toEqual({
      minX: 5,
      maxX: 500,
      minY: 10,
      maxY: 200,
    })
  })
})

describe('resolveBusinessAttentionTier', () => {
  it('returns normal when there are no active attention signals', () => {
    expect(resolveBusinessAttentionTier(makePoint())).toBe('normal')
  })

  it('returns attention for a single signal without extreme percentile', () => {
    expect(
      resolveBusinessAttentionTier(
        makePoint({ ActiveAttentionCount: 1, AxisXPercentile: 50, AxisYPercentile: 50 }),
      ),
    ).toBe('attention')
  })

  it('returns critical when active attention count is two or more', () => {
    expect(resolveBusinessAttentionTier(makePoint({ ActiveAttentionCount: 2 }))).toBe('critical')
  })

  it('returns critical for one signal with extreme low percentile', () => {
    expect(
      resolveBusinessAttentionTier(
        makePoint({
          ActiveAttentionCount: 1,
          AxisXPercentile: CRITICAL_PERCENTILE_THRESHOLD,
          AxisYPercentile: 50,
        }),
      ),
    ).toBe('critical')
  })
})

describe('resolveVisualTier', () => {
  it('elevates to critical when business attention is critical', () => {
    const point = makePoint({ ActiveAttentionCount: 2 })
    const businessTier = resolveBusinessAttentionTier(point)
    expect(resolveVisualTier(undefined, businessTier)).toBe('critical')
  })

  it('uses statistical class when higher than business tier', () => {
    const analyzed: AnalyzedPoint = {
      entityId: 'E1',
      rawX: 1,
      rawY: 1,
      visualExpectedY: 0,
      visualResidual: 10,
      absResidualMad: 5,
      statisticalClass: 'critical',
      deviationLabel: 'Above Expected',
    }
    expect(resolveVisualTier(analyzed, 'normal')).toBe('critical')
  })

  it('uses watch when statistical class is watch and no business attention', () => {
    const analyzed: AnalyzedPoint = {
      entityId: 'E1',
      rawX: 1,
      rawY: 1,
      visualExpectedY: 0,
      visualResidual: 2,
      absResidualMad: 1.5,
      statisticalClass: 'watch',
      deviationLabel: 'Above Expected',
    }
    expect(resolveVisualTier(analyzed, 'normal')).toBe('watch')
  })
})

describe('resolveLabelPlacements', () => {
  function mockCtx(): CanvasRenderingContext2D {
    return {
      font: '',
      measureText: (text: string) => ({ width: text.length * 6.5 }),
    } as unknown as CanvasRenderingContext2D
  }

  it('places non-overlapping labels for separated anchors', () => {
    const ctx = mockCtx()

    const resolved = resolveLabelPlacements(
      [
        { entityId: 'A', text: 'Alpha Corp', anchorX: 100, anchorY: 100, priority: 100 },
        { entityId: 'B', text: 'Beta Corp', anchorX: 300, anchorY: 100, priority: 100 },
      ],
      ctx,
    )

    expect(resolved).toHaveLength(2)
    expect(resolved[0].entityId).toBe('A')
    expect(resolved[1].entityId).toBe('B')
  })

  it('prioritizes higher-priority labels when offsets collide', () => {
    const ctx = mockCtx()

    const resolved = resolveLabelPlacements(
      [
        { entityId: 'low', text: 'Low Priority', anchorX: 100, anchorY: 100, priority: 50 },
        { entityId: 'high', text: 'High Priority', anchorX: 102, anchorY: 102, priority: 100 },
      ],
      ctx,
    )

    expect(resolved.some((l) => l.entityId === 'high')).toBe(true)
    expect(resolved.find((l) => l.entityId === 'high')).toBeDefined()
  })
})

describe('buildAutoLabelCandidates', () => {
  it('orders selected above search and statistical labels', () => {
    const plotted = [
      makePlotted(makePoint({ EntityId: 'sel', DisplayName: 'Selected' }), 10, 10),
      makePlotted(makePoint({ EntityId: 'search', DisplayName: 'Search Hit' }), 20, 20),
      makePlotted(
        makePoint({
          EntityId: 'critical',
          DisplayName: 'Critical Outlier',
          AxisX: 50,
          AxisY: 500,
        }),
        30,
        30,
      ),
    ]

    const projection = populationProjectionEngine.project([
      makePoint({ EntityId: 'sel', AxisX: 10, AxisY: 10 }),
      makePoint({ EntityId: 'search', AxisX: 20, AxisY: 20 }),
      makePoint({ EntityId: 'critical', AxisX: 50, AxisY: 500 }),
      ...Array.from({ length: 20 }, (_, i) =>
        makePoint({ EntityId: `n${i}`, AxisX: i + 1, AxisY: i + 1 }),
      ),
    ])

    for (let i = 0; i < plotted.length; i++) {
      const p = plotted[i]
      const entity = projection?.entities.get(p.point.EntityId)
      if (entity) {
        plotted[i] = {
          ...p,
          analyzed: {
            entityId: entity.entityId,
            rawX: entity.businessX,
            rawY: entity.businessY,
            visualExpectedY: entity.projectionExpectedY,
            visualResidual: entity.projectionResidual,
            absResidualMad: entity.absResidualMad,
            statisticalClass: entity.statisticalClass,
            deviationLabel: entity.deviationLabel,
          },
          labelPriority: entity.labelPriority,
        }
      }
    }

    const candidates = buildAutoLabelCandidates(plotted, {
      selectedIds: new Set(['sel']),
      searchIds: new Set(['search']),
      hoveredId: null,
      topAxisXCount: 0,
      topAxisYCount: 0,
      criticalCap: 8,
      attentionCap: 0,
    })

    const selected = candidates.find((c) => c.entityId === 'sel')
    const search = candidates.find((c) => c.entityId === 'search')
    const critical = candidates.find((c) => c.entityId === 'critical')

    expect(selected?.priority).toBe(100)
    expect(search?.priority).toBe(90)
    if (critical) {
      expect(critical.priority).toBeGreaterThanOrEqual(70)
    }
  })

  it('places hovered entity at lowest auto-label priority', () => {
    const plotted = [
      makePlotted(makePoint({ EntityId: 'hover', DisplayName: 'Hovered' }), 10, 10),
      makePlotted(makePoint({ EntityId: 'top', DisplayName: 'Top', AxisX: 999 }), 20, 20),
    ]

    const candidates = buildAutoLabelCandidates(plotted, {
      selectedIds: new Set(),
      searchIds: new Set(),
      hoveredId: 'hover',
      topAxisXCount: 1,
      topAxisYCount: 0,
      criticalCap: 0,
      attentionCap: 0,
    })

    const hover = candidates.find((c) => c.entityId === 'hover')
    const top = candidates.find((c) => c.entityId === 'top')
    expect(hover?.priority).toBe(40)
    expect(top?.priority).toBe(50)
  })

  it('sorts largest entities by raw business AxisX values', () => {
    const points = [
      makePoint({ EntityId: 'small', AxisX: 1, AxisY: 1 }),
      makePoint({ EntityId: 'large', AxisX: 1_000_000, AxisY: 1 }),
    ]
    const projection = project(points)
    const bounds = computeBounds(projection)
    const transform = buildTransform(400, 300, bounds)
    const plotted = plotPoints(points, transform, projection)

    const candidates = buildAutoLabelCandidates(plotted, {
      selectedIds: new Set(),
      searchIds: new Set(),
      hoveredId: null,
      topAxisXCount: 1,
      topAxisYCount: 0,
      criticalCap: 0,
      attentionCap: 0,
    })

    expect(candidates.some((c) => c.entityId === 'large')).toBe(true)
    expect(candidates.some((c) => c.entityId === 'small')).toBe(false)
  })
})

describe('formatBinRangeLabel', () => {
  it('formats IDR ranges with K/J/M/T suffixes', () => {
    expect(formatBinRangeLabel(0, 100_000, 'IDR')).toBe('0 – 100K')
    expect(formatBinRangeLabel(100_000, 50_000_000, 'IDR')).toBe('100K – 50J')
    expect(formatBinRangeLabel(50_000_000, 1_400_000_000, 'IDR')).toBe('50J – 1.4M')
    expect(formatBinRangeLabel(500_000_000, 1_400_000_000, 'IDR', true)).toBe('500J – ~')
    expect(formatBinRangeLabel(1_000_000_000, 2_500_000_000_000, 'IDR')).toBe('1M – 2.5T')
  })
})

describe('business zero reference (PSOM-12)', () => {
  it('projects business 0 with the same transform as plotted points', () => {
    const points = [
      makePoint({ EntityId: 'neg', AxisX: 50, AxisY: -30 }),
      makePoint({ EntityId: 'pos', AxisX: 150, AxisY: 40 }),
    ]
    const projection = populationProjectionEngine.project(points, {
      axisXUnit: 'Percent',
      axisYUnit: 'Percent',
    })!
    const zero = resolveBusinessZeroProjection(
      projection.metadata.normalizationY,
      'Percent',
    )
    expect(Number.isFinite(zero)).toBe(true)
    expect(isBusinessZeroWithinBounds(zero, projection.bounds)).toBe(true)
    expect(projection.bounds.minY).toBeLessThan(zero)
    expect(projection.bounds.maxY).toBeGreaterThan(zero)
  })

  it('inserts a 0 tick when missing and zero is within bounds', () => {
    const bounds = { minX: -2, maxX: 2, minY: -1, maxY: 1 }
    const ticks = [
      { businessValue: -20, projectionValue: -0.8 },
      { businessValue: 30, projectionValue: 0.7 },
    ]
    const result = ensureZeroTick(ticks, 0, bounds)
    expect(result.map((t) => t.businessValue)).toContain(0)
    const zeroTick = result.find((t) => t.businessValue === 0)!
    expect(zeroTick.projectionValue).toBe(0)
    for (let i = 1; i < result.length; i++) {
      expect(result[i].projectionValue).toBeGreaterThanOrEqual(result[i - 1].projectionValue)
    }
  })

  it('keeps ticks unchanged when zero is outside bounds or already present', () => {
    const bounds = { minX: -2, maxX: 2, minY: 0.5, maxY: 2 }
    const ticks = [{ businessValue: 10, projectionValue: 1 }]
    expect(ensureZeroTick(ticks, 0, bounds)).toBe(ticks)

    const inBounds = { minX: -2, maxX: 2, minY: -1, maxY: 1 }
    const withZero = [
      { businessValue: 0, projectionValue: 0 },
      { businessValue: 10, projectionValue: 0.5 },
    ]
    expect(ensureZeroTick(withZero, 0, inBounds)).toBe(withZero)
  })
})

describe('fixed business quadrants (PSOM-13)', () => {
  it('classifies the four fixed business quadrants', () => {
    expect(classifyPacingQuadrant(120, 10)).toBe('star')
    expect(classifyPacingQuadrant(80, 10)).toBe('growing')
    expect(classifyPacingQuadrant(120, -10)).toBe('steady')
    expect(classifyPacingQuadrant(80, -10)).toBe('declining')
  })

  it('treats the exact boundaries as Star (X≥100, Y≥0)', () => {
    expect(classifyPacingQuadrant(100, 0)).toBe('star')
    expect(classifyPacingQuadrant(100, -1)).toBe('steady')
    expect(classifyPacingQuadrant(99, 0)).toBe('growing')
    expect(classifyPacingQuadrant(99, -1)).toBe('declining')
  })

  it('returns null when a business value is missing or non-finite', () => {
    expect(classifyPacingQuadrant(null, 10)).toBeNull()
    expect(classifyPacingQuadrant(120, null)).toBeNull()
    expect(classifyPacingQuadrant(undefined, undefined)).toBeNull()
    expect(classifyPacingQuadrant(Number.NaN, 10)).toBeNull()
  })

  it('aligns quadrant boundaries with the plotted point transform', () => {
    const points = [
      makePoint({ EntityId: 'low', AxisX: 50, AxisY: -20 }),
      makePoint({ EntityId: 'threshold', AxisX: 100, AxisY: 0 }),
      makePoint({ EntityId: 'high', AxisX: 160, AxisY: 30 }),
    ]
    const projection = project(points)
    const bounds = computeBounds(projection)
    const transform = buildTransform(800, 600, bounds)

    const { boundaryX, boundaryY } = resolvePacingQuadrantBoundaries(
      projection,
      transform,
      'Percent',
      'Percent',
    )

    const threshold = projection.entities.get('threshold')!
    const screen = dataToScreen(threshold.projectionX, threshold.projectionY, transform)

    expect(boundaryX).not.toBeNull()
    expect(boundaryY).not.toBeNull()
    expect(boundaryX!).toBeCloseTo(screen.x, 6)
    expect(boundaryY!).toBeCloseTo(screen.y, 6)
  })

  it('returns null boundaries when they fall outside the visible bounds', () => {
    const points = [
      makePoint({ EntityId: 'a', AxisX: 0, AxisY: 0 }),
      makePoint({ EntityId: 'b', AxisX: 200, AxisY: 50 }),
    ]
    const projection = project(points)
    const transform = buildTransform(800, 600, { minX: 5, maxX: 10, minY: 0.5, maxY: 2 })

    const { boundaryX, boundaryY } = resolvePacingQuadrantBoundaries(
      projection,
      transform,
      'Percent',
      'Percent',
    )

    expect(boundaryX).toBeNull()
    expect(boundaryY).toBeNull()
  })
})
