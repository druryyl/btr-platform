import { describe, expect, it } from 'vitest'
import type { PopulationMapPoint } from '@/models/entityAnalytics'
import type { AnalyzedPoint } from '@/services/populationStatisticsEngine'
import { analyzePopulationMap, businessToVisual } from '@/services/populationStatisticsEngine'
import {
  buildAutoLabelCandidates,
  buildTransform,
  computeBounds,
  computeRawExtents,
  dataToScreen,
  generateLinearAxisTicks,
  generateLogMappedAxisTicks,
  plotPoints,
  resolveBusinessAttentionTier,
  resolveLabelPlacements,
  resolveVisualTier,
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
) {
  return { point, screenX, screenY, index, analyzed }
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

describe('computeBounds', () => {
  it('uses visual log-space coordinates with padding', () => {
    const points = [
      makePoint({ EntityId: 'a', AxisX: 0, AxisY: 0 }),
      makePoint({ EntityId: 'b', AxisX: 100, AxisY: 200 }),
    ]

    const bounds = computeBounds(points)
    expect(bounds.minX).toBeLessThan(businessToVisual(0))
    expect(bounds.maxX).toBeGreaterThan(businessToVisual(100))
    expect(bounds.minY).toBeLessThan(businessToVisual(0))
    expect(bounds.maxY).toBeGreaterThan(businessToVisual(200))
  })

  it('expands bounds to include confidence bands when analysis is provided', () => {
    const points = [
      makePoint({ EntityId: 'a', AxisX: 0, AxisY: 0 }),
      makePoint({ EntityId: 'b', AxisX: 100, AxisY: 100 }),
      makePoint({ EntityId: 'c', AxisX: 50, AxisY: 50 }),
    ]
    const analysis = analyzePopulationMap(points)!
    const boundsWithout = computeBounds(points)
    const boundsWith = computeBounds(points, analysis)
    expect(boundsWith.maxY - boundsWith.minY).toBeGreaterThanOrEqual(
      boundsWithout.maxY - boundsWithout.minY,
    )
  })

  it('yields wider visual span than raw linear extent for Pareto data', () => {
    const points = makeParetoPoints(100)
    const rawExtents = computeRawExtents(points)
    const bounds = computeBounds(points)
    const rawSpan = rawExtents.maxX - rawExtents.minX
    const visualSpan = bounds.maxX - bounds.minX
    expect(visualSpan).toBeLessThan(rawSpan)
    expect(visualSpan).toBeGreaterThan(1)
  })
})

describe('generateLogMappedAxisTicks', () => {
  it('returns business values with monotonic visual positions', () => {
    const ticks = generateLogMappedAxisTicks(0, 1_000_000)
    expect(ticks.length).toBeGreaterThan(2)

    for (let i = 1; i < ticks.length; i++) {
      expect(ticks[i].businessValue).toBeGreaterThan(ticks[i - 1].businessValue)
      expect(ticks[i].visualValue).toBeGreaterThan(ticks[i - 1].visualValue)
      expect(ticks[i].visualValue).toBeCloseTo(businessToVisual(ticks[i].businessValue), 10)
    }
  })

  it('includes zero when range starts at zero', () => {
    const ticks = generateLogMappedAxisTicks(0, 10_000)
    expect(ticks.some((t) => t.businessValue === 0)).toBe(true)
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

describe('dataToScreen with visual bounds', () => {
  it('maps known coordinates to expected screen positions', () => {
    const points = [
      makePoint({ AxisX: 0, AxisY: 0 }),
      makePoint({ EntityId: 'E2', AxisX: 100, AxisY: 100 }),
    ]
    const bounds = computeBounds(points)
    const transform = buildTransform(800, 600, bounds)

    const minScreen = dataToScreen(bounds.minX, bounds.minY, transform)
    const maxScreen = dataToScreen(bounds.maxX, bounds.maxY, transform)

    expect(minScreen.x).toBeCloseTo(transform.offsetX, 0)
    expect(maxScreen.x).toBeCloseTo(transform.offsetX + transform.plotWidth, 0)
    expect(minScreen.y).toBeCloseTo(transform.offsetY + transform.plotHeight, 0)
    expect(maxScreen.y).toBeCloseTo(transform.offsetY, 0)
  })
})

describe('plotPoints', () => {
  it('plots using log-transformed visual coordinates', () => {
    const points = [
      makePoint({ AxisX: 0, AxisY: 0 }),
      makePoint({ EntityId: 'E2', AxisX: 100, AxisY: 200 }),
    ]
    const analysis = analyzePopulationMap(points)
    const bounds = computeBounds(points, analysis)
    const transform = buildTransform(400, 300, bounds)
    const plotted = plotPoints(points, transform, analysis)

    expect(plotted).toHaveLength(2)
    expect(plotted[0].screenX).not.toBeCloseTo(plotted[1].screenX, 0)
    expect(plotted[0].screenY).not.toBeCloseTo(plotted[1].screenY, 0)
    expect(plotted[0].analyzed).toBeDefined()
  })

  it('separates low and high business values on screen for long-tail data', () => {
    const points = makeParetoPoints(200)
    const analysis = analyzePopulationMap(points)
    const bounds = computeBounds(points, analysis)
    const transform = buildTransform(800, 600, bounds)
    const plotted = plotPoints(points, transform, analysis)

    const small = plotted.find((p) => p.point.EntityId === 'p0')!
    const large = plotted.find((p) => p.point.EntityId === 'p199')!

    const screenSpanX = Math.abs(large.screenX - small.screenX)
    const screenSpanY = Math.abs(large.screenY - small.screenY)
    expect(screenSpanX).toBeGreaterThan(transform.plotWidth * 0.5)
    expect(screenSpanY).toBeGreaterThan(transform.plotHeight * 0.5)
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

    const analysis = analyzePopulationMap([
      makePoint({ EntityId: 'sel', AxisX: 10, AxisY: 10 }),
      makePoint({ EntityId: 'search', AxisX: 20, AxisY: 20 }),
      makePoint({ EntityId: 'critical', AxisX: 50, AxisY: 500 }),
      ...Array.from({ length: 20 }, (_, i) =>
        makePoint({ EntityId: `n${i}`, AxisX: i, AxisY: i }),
      ),
    ])

    for (const p of plotted) {
      p.analyzed = analysis?.points.get(p.point.EntityId)
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
      expect(critical.priority).toBe(70)
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
    const analysis = analyzePopulationMap(points)
    const bounds = computeBounds(points, analysis)
    const transform = buildTransform(400, 300, bounds)
    const plotted = plotPoints(points, transform, analysis)

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
