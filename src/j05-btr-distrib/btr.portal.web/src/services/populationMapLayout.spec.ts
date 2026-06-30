import { describe, expect, it } from 'vitest'
import type { PopulationMapPoint } from '@/models/entityAnalytics'
import {
  CRITICAL_PERCENTILE_THRESHOLD,
  resolveLabelPlacements,
  resolvePopulationPointTier,
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

describe('resolvePopulationPointTier', () => {
  it('returns normal when there are no active attention signals', () => {
    expect(resolvePopulationPointTier(makePoint())).toBe('normal')
  })

  it('returns attention for a single signal without extreme percentile', () => {
    expect(
      resolvePopulationPointTier(
        makePoint({ ActiveAttentionCount: 1, AxisXPercentile: 50, AxisYPercentile: 50 }),
      ),
    ).toBe('attention')
  })

  it('returns critical when active attention count is two or more', () => {
    expect(resolvePopulationPointTier(makePoint({ ActiveAttentionCount: 2 }))).toBe('critical')
  })

  it('returns critical for one signal with extreme low percentile', () => {
    expect(
      resolvePopulationPointTier(
        makePoint({
          ActiveAttentionCount: 1,
          AxisXPercentile: CRITICAL_PERCENTILE_THRESHOLD,
          AxisYPercentile: 50,
        }),
      ),
    ).toBe('critical')
  })

  it('returns critical for one signal with extreme high percentile', () => {
    const upper = 100 - CRITICAL_PERCENTILE_THRESHOLD
    expect(
      resolvePopulationPointTier(
        makePoint({
          ActiveAttentionCount: 1,
          AxisXPercentile: 50,
          AxisYPercentile: upper,
        }),
      ),
    ).toBe('critical')
  })

  it('returns attention just inside the percentile boundary', () => {
    expect(
      resolvePopulationPointTier(
        makePoint({
          ActiveAttentionCount: 1,
          AxisXPercentile: CRITICAL_PERCENTILE_THRESHOLD + 1,
          AxisYPercentile: 50,
        }),
      ),
    ).toBe('attention')
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
