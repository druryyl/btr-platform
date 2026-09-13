import { describe, expect, it } from 'vitest'
import type { PopulationMapPoint } from '@/models/entityAnalytics'
import { populationProjectionEngine } from '@/services/populationProjection/populationProjectionEngine'
import { businessToProjectedLog } from '@/services/populationProjection/robustStats'
import {
  buildTransform,
  classifyPacingQuadrant,
  computeBounds,
  ensureZeroTick,
  generateProjectionAxisGuides,
  isBusinessZeroWithinBounds,
  isLowConfidencePoint,
  PACING_QUADRANT_LABELS,
  PACING_QUADRANT_X_THRESHOLD,
  PACING_QUADRANT_Y_THRESHOLD,
  plotPoints,
  PRINCIPAL_SALES_OUT_MAP_PRESET_ID,
  projectionToScreenY,
  resolveBusinessZeroProjection,
  resolvePacingQuadrantBoundaries,
  resolvePacingQuadrantForPoint,
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

const PERCENT_AXES = { axisXUnit: 'Percent', axisYUnit: 'Percent' } as const

function projectPercent(points: PopulationMapPoint[]) {
  const projection = populationProjectionEngine.project(points, PERCENT_AXES)
  expect(projection).not.toBeNull()
  return projection!
}

/** Mixed-sign YoY MTD Growth dataset (includes an exact zero and both signs). */
function mixedSignPoints(): PopulationMapPoint[] {
  return [
    makePoint({ EntityId: 'declining', DisplayName: 'Declining', AxisX: 60, AxisY: -25 }),
    makePoint({ EntityId: 'steady', DisplayName: 'Steady', AxisX: 120, AxisY: -8 }),
    makePoint({ EntityId: 'flat', DisplayName: 'Flat', AxisX: 90, AxisY: 0 }),
    makePoint({ EntityId: 'growing', DisplayName: 'Growing', AxisX: 80, AxisY: 12 }),
    makePoint({ EntityId: 'star', DisplayName: 'Star', AxisX: 130, AxisY: 40 }),
  ]
}

/** Mixed-sign dataset whose percentiles never land exactly on business 0. */
function spreadPercentPoints(): PopulationMapPoint[] {
  return [
    makePoint({ EntityId: 'a', AxisX: 55, AxisY: -30 }),
    makePoint({ EntityId: 'b', AxisX: 70, AxisY: -20 }),
    makePoint({ EntityId: 'c', AxisX: 85, AxisY: -7 }),
    makePoint({ EntityId: 'd', AxisX: 95, AxisY: 5 }),
    makePoint({ EntityId: 'e', AxisX: 115, AxisY: 18 }),
    makePoint({ EntityId: 'f', AxisX: 135, AxisY: 45 }),
  ]
}

describe('Principal Sales-Out Map time-aware KPI verification (PSOM-16)', () => {
  it('targets the principal-sales-out-map preset', () => {
    expect(PRINCIPAL_SALES_OUT_MAP_PRESET_ID).toBe('principal-sales-out-map')
  })

  describe('signed Y-axis projection and auto bounds (GAP-005)', () => {
    it('keeps negative YoY MTD Growth and preserves business-Y ordering', () => {
      const projection = projectPercent(mixedSignPoints())
      const ordered = ['declining', 'steady', 'flat', 'growing', 'star']

      expect(ordered.map((id) => projection.entities.get(id)!.businessY)).toEqual([
        -25, -8, 0, 12, 40,
      ])

      for (let i = 1; i < ordered.length; i++) {
        const previous = projection.entities.get(ordered[i - 1])!
        const current = projection.entities.get(ordered[i])!
        expect(current.projectionY).toBeGreaterThan(previous.projectionY)
      }
    })

    it('auto-computes bounds that enclose business zero across signs', () => {
      const projection = projectPercent(mixedSignPoints())
      const zero = resolveBusinessZeroProjection(projection.metadata.normalizationY, 'Percent')

      expect(Number.isFinite(zero)).toBe(true)
      expect(isBusinessZeroWithinBounds(zero, projection.bounds)).toBe(true)
      expect(projection.bounds.minY).toBeLessThan(zero)
      expect(projection.bounds.maxY).toBeGreaterThan(zero)
    })

    it('retains non-negative clamping for non-percent axes (other presets)', () => {
      const projection = populationProjectionEngine.project(
        [
          makePoint({ EntityId: 'neg', AxisX: 1_000_000, AxisY: -500 }),
          makePoint({ EntityId: 'pos', AxisX: 2_000_000, AxisY: 5_000 }),
        ],
        { axisXUnit: 'IDR', axisYUnit: 'IDR' },
      )!

      expect(projection.entities.get('neg')!.businessY).toBe(0)
      expect(businessToProjectedLog(-500, 'IDR')).toBe(0)
    })
  })

  describe('Y = 0 reference line and zero tick (GAP-005)', () => {
    it('places the Y = 0 reference at the plotted zero position', () => {
      const points = mixedSignPoints()
      const projection = projectPercent(points)
      const bounds = computeBounds(projection)
      const transform = buildTransform(800, 600, bounds)
      const zeroProjection = resolveBusinessZeroProjection(
        projection.metadata.normalizationY,
        'Percent',
      )

      const plottedZero = plotPoints(points, transform, projection).find(
        (p) => p.point.EntityId === 'flat',
      )!

      expect(projectionToScreenY(zeroProjection, transform)).toBeCloseTo(plottedZero.screenY, 6)
    })

    it('inserts a 0 tick at the reference position when data spans both signs', () => {
      const projection = projectPercent(spreadPercentPoints())
      const bounds = computeBounds(projection)
      const transform = buildTransform(800, 600, bounds)
      const zeroProjection = resolveBusinessZeroProjection(
        projection.metadata.normalizationY,
        'Percent',
      )
      const { yTicks } = generateProjectionAxisGuides(projection)

      expect(yTicks.some((t) => t.businessValue === 0)).toBe(false)

      const withZero = ensureZeroTick(yTicks, zeroProjection, bounds)
      const zeroTick = withZero.find((t) => t.businessValue === 0)
      expect(zeroTick).toBeDefined()
      expect(zeroTick!.projectionValue).toBeCloseTo(zeroProjection, 9)
      expect(projectionToScreenY(zeroTick!.projectionValue, transform)).toBeCloseTo(
        projectionToScreenY(zeroProjection, transform),
        6,
      )

      const outside = {
        minX: 0,
        maxX: 1,
        minY: zeroProjection + 1,
        maxY: zeroProjection + 2,
      }
      expect(ensureZeroTick(yTicks, zeroProjection, outside)).toBe(yTicks)
    })
  })

  describe('fixed business quadrants (GAP-006)', () => {
    it('classifies all four quadrants with fixed thresholds and labels', () => {
      expect(classifyPacingQuadrant(120, 15)).toBe('star')
      expect(classifyPacingQuadrant(75, 12)).toBe('growing')
      expect(classifyPacingQuadrant(130, -18)).toBe('steady')
      expect(classifyPacingQuadrant(60, -22)).toBe('declining')
      expect(PACING_QUADRANT_X_THRESHOLD).toBe(100)
      expect(PACING_QUADRANT_Y_THRESHOLD).toBe(0)
      expect(PACING_QUADRANT_LABELS).toEqual({
        star: 'Star',
        growing: 'Growing',
        steady: 'Steady',
        declining: 'Declining',
      })
    })

    it('positions the quadrant boundaries on the plotted (100, 0) point', () => {
      const points = [
        ...spreadPercentPoints(),
        makePoint({ EntityId: 'threshold', AxisX: 100, AxisY: 0 }),
      ]
      const projection = projectPercent(points)
      const bounds = computeBounds(projection)
      const transform = buildTransform(800, 600, bounds)
      const threshold = plotPoints(points, transform, projection).find(
        (p) => p.point.EntityId === 'threshold',
      )!

      const { boundaryX, boundaryY } = resolvePacingQuadrantBoundaries(
        projection,
        transform,
        'Percent',
        'Percent',
      )

      expect(boundaryX).not.toBeNull()
      expect(boundaryY).not.toBeNull()
      expect(boundaryX!).toBeCloseTo(threshold.screenX, 6)
      expect(boundaryY!).toBeCloseTo(threshold.screenY, 6)
    })

    it('classifies plotted points by fixed thresholds rather than residuals', () => {
      const points = [
        makePoint({ EntityId: 'star', AxisX: 120, AxisY: 15 }),
        makePoint({ EntityId: 'growing', AxisX: 75, AxisY: 12 }),
        makePoint({ EntityId: 'steady', AxisX: 130, AxisY: -18 }),
        makePoint({ EntityId: 'declining', AxisX: 60, AxisY: -22 }),
      ]
      const projection = projectPercent(points)
      const bounds = computeBounds(projection)
      const transform = buildTransform(800, 600, bounds)
      const plotted = plotPoints(points, transform, projection)

      const quadrants = new Map(
        plotted.map((p) => [p.point.EntityId, resolvePacingQuadrantForPoint(p.point)]),
      )

      expect(quadrants.get('star')).toBe('star')
      expect(quadrants.get('growing')).toBe('growing')
      expect(quadrants.get('steady')).toBe('steady')
      expect(quadrants.get('declining')).toBe('declining')
    })
  })

  describe('low-confidence exclusion (GAP-007)', () => {
    it('excludes low-confidence entities from quadrants and classifies the rest', () => {
      const points = [
        makePoint({ EntityId: 'flaggedStar', AxisX: 120, AxisY: 20, IsLowConfidence: true }),
        makePoint({ EntityId: 'star', AxisX: 120, AxisY: 15 }),
        makePoint({ EntityId: 'declining', AxisX: 60, AxisY: -22 }),
      ]
      const projection = projectPercent(points)
      const bounds = computeBounds(projection)
      const transform = buildTransform(800, 600, bounds)

      for (const plotted of plotPoints(points, transform, projection)) {
        if (plotted.point.EntityId === 'flaggedStar') {
          expect(isLowConfidencePoint(plotted.point)).toBe(true)
          expect(resolvePacingQuadrantForPoint(plotted.point)).toBeNull()
        } else {
          expect(resolvePacingQuadrantForPoint(plotted.point)).not.toBeNull()
        }
      }

      expect(resolvePacingQuadrantForPoint(makePoint({ AxisX: 120, AxisY: 20 }))).toBe('star')
    })
  })
})
