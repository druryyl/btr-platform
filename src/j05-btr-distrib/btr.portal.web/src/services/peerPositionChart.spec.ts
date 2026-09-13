import { describe, expect, it } from 'vitest'
import type { PeerDistributionBin, PeerDistributionResponse } from '@/models/entityAnalytics'
import { buildEntityColorMap } from '@/composables/useComparisonColors'
import {
  buildPeerBarColors,
  buildPeerBinMarkers,
  formatPeerBinLabel,
  PEER_BIN_NEUTRAL_FILL,
  PEER_BIN_OVERFLOW_BORDER,
  PEER_BIN_OVERFLOW_FILL,
  resolvePeerBinIndex,
} from '@/services/peerPositionChart'

function bin(start: number, end: number, index: number): PeerDistributionBin {
  return {
    BinIndex: index,
    BinStart: start,
    BinEnd: end,
    Count: 1,
    Label: `${start}-${end}`,
  }
}

const bins: PeerDistributionBin[] = [
  bin(0, 100_000, 0),
  bin(100_000, 1_000_000, 1),
  bin(1_000_000, 50_000_000, 2),
  bin(50_000_000, 500_000_000, 3),
]

function dist(
  entityId: string,
  selectedValue: number | null,
): PeerDistributionResponse {
  return {
    EntityType: 'Customer',
    EntityId: entityId,
    KpiId: 'CU-KPI-010',
    KpiDisplayName: 'Open Balance',
    Unit: 'IDR',
    PeerGroupSize: 10,
    PeerGroupRuleId: 'customer-wilayah',
    PeerGroupDimensionValue: 'Jakarta Pusat',
    FormattedPeerGroupLabel: '10 customers in Wilayah: Jakarta Pusat',
    SelectedValue: selectedValue,
    FormattedSelectedValue: String(selectedValue ?? '—'),
    SelectedPercentile: 50,
    PeerMin: 0,
    PeerMax: 500_000_000,
    FormattedPeerRange: '0 – 500M',
    Bins: bins,
  }
}

describe('resolvePeerBinIndex', () => {
  it('resolves first, middle, and last bins', () => {
    expect(resolvePeerBinIndex(0, bins)).toBe(0)
    expect(resolvePeerBinIndex(50_000, bins)).toBe(0)
    expect(resolvePeerBinIndex(500_000, bins)).toBe(1)
    expect(resolvePeerBinIndex(10_000_000, bins)).toBe(2)
    expect(resolvePeerBinIndex(100_000_000, bins)).toBe(3)
  })

  it('excludes upper edge for non-last bins', () => {
    expect(resolvePeerBinIndex(100_000, bins)).toBe(1)
    expect(resolvePeerBinIndex(1_000_000, bins)).toBe(2)
    expect(resolvePeerBinIndex(50_000_000, bins)).toBe(3)
  })

  it('includes upper edge for last bin', () => {
    expect(resolvePeerBinIndex(500_000_000, bins)).toBe(3)
  })

  it('returns null for missing or out-of-range values', () => {
    expect(resolvePeerBinIndex(null, bins)).toBeNull()
    expect(resolvePeerBinIndex(undefined, bins)).toBeNull()
    expect(resolvePeerBinIndex(Number.NaN, bins)).toBeNull()
    expect(resolvePeerBinIndex(-1, bins)).toBeNull()
    expect(resolvePeerBinIndex(500_000_001, bins)).toBeNull()
    expect(resolvePeerBinIndex(0, [])).toBeNull()
  })
})

describe('buildPeerBinMarkers', () => {
  it('maps multiple entities to distinct colors and bins', () => {
    const entityIds = ['A', 'B']
    const colorMap = buildEntityColorMap(entityIds)
    const markers = buildPeerBinMarkers({
      bins,
      entityIds,
      distributions: {
        A: dist('A', 50_000),
        B: dist('B', 100_000_000),
      },
      colorMap,
    })

    expect(markers).toHaveLength(2)
    expect(markers[0]).toMatchObject({ entityId: 'A', binIndex: 0 })
    expect(markers[1]).toMatchObject({ entityId: 'B', binIndex: 3 })
    expect(markers[0].color).not.toBe(markers[1].color)
  })

  it('places multiple entities in the same bin', () => {
    const entityIds = ['A', 'B']
    const colorMap = buildEntityColorMap(entityIds)
    const markers = buildPeerBinMarkers({
      bins,
      entityIds,
      distributions: {
        A: dist('A', 200_000),
        B: dist('B', 800_000),
      },
      colorMap,
    })

    expect(markers).toHaveLength(2)
    expect(markers.every((m) => m.binIndex === 1)).toBe(true)
  })

  it('skips entities with null SelectedValue', () => {
    const entityIds = ['A', 'B']
    const colorMap = buildEntityColorMap(entityIds)
    const markers = buildPeerBinMarkers({
      bins,
      entityIds,
      distributions: {
        A: dist('A', null),
        B: dist('B', 50_000),
      },
      colorMap,
    })

    expect(markers).toHaveLength(1)
    expect(markers[0].entityId).toBe('B')
  })
})

describe('buildPeerBarColors', () => {
  it('highlights selected bins and keeps others neutral', () => {
    const entityIds = ['A', 'B']
    const colorMap = buildEntityColorMap(entityIds)
    const markers = buildPeerBinMarkers({
      bins,
      entityIds,
      distributions: {
        A: dist('A', 50_000),
        B: dist('B', 100_000_000),
      },
      colorMap,
    })

    const colors = buildPeerBarColors(bins, markers)

    expect(colors.backgroundColor[0]).not.toBe(PEER_BIN_NEUTRAL_FILL)
    expect(colors.backgroundColor[1]).toBe(PEER_BIN_NEUTRAL_FILL)
    expect(colors.backgroundColor[3]).not.toBe(PEER_BIN_NEUTRAL_FILL)
    expect(colors.borderWidth[0]).toBe(2)
    expect(colors.borderWidth[1]).toBe(0)
  })

  it('tints backend-flagged overflow bins distinctly', () => {
    const flagged: PeerDistributionBin[] = [
      ...bins.slice(0, 3),
      { ...bins[3], IsOverflow: true },
    ]
    const colors = buildPeerBarColors(flagged, [])

    expect(colors.backgroundColor[3]).toBe(PEER_BIN_OVERFLOW_FILL)
    expect(colors.borderColor[3]).toBe(PEER_BIN_OVERFLOW_BORDER)
    expect(colors.borderWidth[3]).toBe(2)
    expect(colors.backgroundColor[0]).toBe(PEER_BIN_NEUTRAL_FILL)
  })

  it('lets a selected entity color win over the overflow tint', () => {
    const flagged: PeerDistributionBin[] = [
      ...bins.slice(0, 3),
      { ...bins[3], IsOverflow: true },
    ]
    const entityIds = ['A']
    const colorMap = buildEntityColorMap(entityIds)
    const markers = buildPeerBinMarkers({
      bins: flagged,
      entityIds,
      distributions: { A: dist('A', 100_000_000) },
      colorMap,
    })

    const colors = buildPeerBarColors(flagged, markers)
    expect(colors.borderColor[3]).toBe(markers[0].color)
    expect(colors.backgroundColor[3]).not.toBe(PEER_BIN_OVERFLOW_FILL)
  })

  it('uses first marker color when multiple share a bin', () => {
    const entityIds = ['A', 'B']
    const colorMap = buildEntityColorMap(entityIds)
    const markers = buildPeerBinMarkers({
      bins,
      entityIds,
      distributions: {
        A: dist('A', 200_000),
        B: dist('B', 800_000),
      },
      colorMap,
    })

    const colors = buildPeerBarColors(bins, markers)
    expect(colors.borderColor[1]).toBe(markers[0].color)
    expect(colors.borderWidth[1]).toBe(2)
  })
})

describe('formatPeerBinLabel', () => {
  it('renders normal bins as ranges', () => {
    expect(formatPeerBinLabel(bin(80, 100, 0), 'Percent', false)).toBe('80 – 100')
    expect(formatPeerBinLabel(bin(0, 100_000, 0), 'IDR', false)).toBe('0 – 100K')
  })

  it('renders a flagged last bin as an open-ended band', () => {
    const overflow: PeerDistributionBin = { ...bin(150, 663, 6), IsOverflow: true }
    expect(formatPeerBinLabel(overflow, 'Percent', true)).toBe('≥ 150')
  })

  it('ignores a stray flag on non-last bins', () => {
    const stray: PeerDistributionBin = { ...bin(80, 100, 0), IsOverflow: true }
    expect(formatPeerBinLabel(stray, 'Percent', false)).toBe('80 – 100')
  })
})
