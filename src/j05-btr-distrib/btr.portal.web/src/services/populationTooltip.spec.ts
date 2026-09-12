import { describe, expect, it } from 'vitest'
import type { PopulationMapPoint, PopulationMapResponse } from '@/models/entityAnalytics'
import { resolveTooltipDimensionRow } from '@/services/populationTooltip'

function point(overrides: Partial<PopulationMapPoint> = {}): PopulationMapPoint {
  return {
    EntityId: 'S001',
    EntityCode: 'SUP001',
    DisplayName: 'Alpha Principal',
    AxisX: 0,
    AxisY: 0,
    FormattedAxisX: '',
    FormattedAxisY: '',
    AxisXPercentile: 0,
    AxisYPercentile: 0,
    DimensionValue: null,
    MatchesFilter: true,
    IsActive: true,
    ActiveAttentionCount: 0,
    ...overrides,
  }
}

function population(overrides: Partial<PopulationMapResponse> = {}): PopulationMapResponse {
  return {
    EntityType: 'Supplier',
    PresetId: 'principal-sales-out-map',
    PresetDisplayName: 'Principal Sales-Out Map',
    AxisXKpiId: 'PRN-TGT-003',
    AxisYKpiId: 'PRN-GRW-002',
    AxisXLabel: 'Achievement %',
    AxisYLabel: 'YoY Growth %',
    AxisXUnit: null,
    AxisYUnit: null,
    DimensionLabel: null,
    TotalPopulationCount: 1,
    FilteredPopulationCount: 1,
    ActiveFilterDescription: null,
    GeneratedAt: '2026-06-24T08:00:00',
    Points: [],
    ...overrides,
  }
}

describe('resolveTooltipDimensionRow', () => {
  it('hides the dimension row when the population has no meaningful dimension (Principal)', () => {
    const row = resolveTooltipDimensionRow(point({ DimensionValue: 'All Active' }), population())

    expect(row).toBeNull()
  })

  it('hides the dimension row when the point has no dimension value', () => {
    const row = resolveTooltipDimensionRow(
      point({ DimensionValue: null }),
      population({ DimensionLabel: 'Wilayah' }),
    )

    expect(row).toBeNull()
  })

  it('hides the dimension row when a population-level label is absent but the point carries a value', () => {
    const row = resolveTooltipDimensionRow(
      point({ DimensionValue: 'Jakarta' }),
      population({ DimensionLabel: null }),
    )

    expect(row).toBeNull()
  })

  it('hides the dimension row when point or population is missing', () => {
    expect(resolveTooltipDimensionRow(null, population())).toBeNull()
    expect(resolveTooltipDimensionRow(undefined, population())).toBeNull()
    expect(resolveTooltipDimensionRow(point(), null)).toBeNull()
    expect(resolveTooltipDimensionRow(point(), undefined)).toBeNull()
  })

  it('renders the dimension row with the registry label for a dimensioned population', () => {
    const row = resolveTooltipDimensionRow(
      point({ DimensionValue: 'Jakarta' }),
      population({ EntityType: 'Customer', DimensionLabel: 'Wilayah' }),
    )

    expect(row).toEqual({ label: 'Wilayah', value: 'Jakarta' })
  })

  it('treats blank labels and values as missing', () => {
    expect(
      resolveTooltipDimensionRow(
        point({ DimensionValue: '  ' }),
        population({ DimensionLabel: 'Wilayah' }),
      ),
    ).toBeNull()
    expect(
      resolveTooltipDimensionRow(
        point({ DimensionValue: 'Jakarta' }),
        population({ DimensionLabel: '  ' }),
      ),
    ).toBeNull()
  })
})