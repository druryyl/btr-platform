import { Chart, type Chart as ChartType, type Plugin } from 'chart.js'
import type { PeerDistributionBin, PeerDistributionResponse } from '@/models/entityAnalytics'
import type { WORKSPACE_COMPARISON_COLORS } from '@/composables/useComparisonColors'

export const PEER_BIN_NEUTRAL_FILL = 'rgba(148, 163, 184, 0.45)'
export const PEER_BIN_HIGHLIGHT_OPACITY = 0.35

export interface PeerBinMarker {
  entityId: string
  binIndex: number
  color: string
  fill: string
}

type ComparisonColor = (typeof WORKSPACE_COMPARISON_COLORS)[number]

/**
 * Resolves which histogram bin a value belongs to.
 * Mirrors backend CountInRange: non-last bins are [start, end); last bin is [start, end].
 */
export function resolvePeerBinIndex(
  value: number | null | undefined,
  bins: PeerDistributionBin[],
): number | null {
  if (value == null || !Number.isFinite(value) || bins.length === 0) return null

  for (let i = 0; i < bins.length; i++) {
    const bin = bins[i]
    const isLast = i === bins.length - 1
    if (isLast) {
      if (value >= bin.BinStart && value <= bin.BinEnd) return i
    } else if (value >= bin.BinStart && value < bin.BinEnd) {
      return i
    }
  }

  return null
}

export function buildPeerBinMarkers(args: {
  bins: PeerDistributionBin[]
  entityIds: string[]
  distributions: Record<string, PeerDistributionResponse>
  colorMap: Map<string, ComparisonColor>
}): PeerBinMarker[] {
  const { bins, entityIds, distributions, colorMap } = args
  const markers: PeerBinMarker[] = []

  for (const entityId of entityIds) {
    const dist = distributions[entityId]
    if (!dist) continue

    const binIndex = resolvePeerBinIndex(dist.SelectedValue, bins)
    if (binIndex == null) continue

    const colors = colorMap.get(entityId)
    if (!colors) continue

    markers.push({
      entityId,
      binIndex,
      color: colors.border,
      fill: colors.fill,
    })
  }

  return markers
}

function withOpacity(rgb: string, opacity: number): string {
  const match = rgb.match(/rgb\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*\)/i)
  if (!match) return rgb
  return `rgba(${match[1]}, ${match[2]}, ${match[3]}, ${opacity})`
}

export function buildPeerBarColors(
  bins: PeerDistributionBin[],
  markers: PeerBinMarker[],
  neutralColor = PEER_BIN_NEUTRAL_FILL,
): {
  backgroundColor: string[]
  borderColor: string[]
  borderWidth: number[]
} {
  const backgroundColor = bins.map(() => neutralColor)
  const borderColor = bins.map(() => 'transparent')
  const borderWidth = bins.map(() => 0)

  // One highlight color per bin: first marker wins when multiple share a bin.
  const firstByBin = new Map<number, PeerBinMarker>()
  for (const marker of markers) {
    if (!firstByBin.has(marker.binIndex)) firstByBin.set(marker.binIndex, marker)
  }

  for (const [binIndex, marker] of firstByBin) {
    if (binIndex < 0 || binIndex >= bins.length) continue
    backgroundColor[binIndex] = withOpacity(marker.fill, PEER_BIN_HIGHLIGHT_OPACITY)
    borderColor[binIndex] = marker.color
    borderWidth[binIndex] = 2
  }

  return { backgroundColor, borderColor, borderWidth }
}

function caretOffsets(count: number): number[] {
  if (count <= 1) return [0]
  if (count === 2) return [-8, 8]
  if (count === 3) return [-12, 0, 12]
  const spread = 10
  const start = -((count - 1) * spread) / 2
  return Array.from({ length: count }, (_, i) => start + i * spread)
}

function drawDownCaret(
  ctx: CanvasRenderingContext2D,
  tipX: number,
  tipY: number,
  color: string,
  size = 7,
) {
  ctx.save()
  ctx.fillStyle = color
  ctx.beginPath()
  ctx.moveTo(tipX, tipY)
  ctx.lineTo(tipX - size, tipY - size * 1.4)
  ctx.lineTo(tipX + size, tipY - size * 1.4)
  ctx.closePath()
  ctx.fill()
  ctx.restore()
}

export const peerBinMarkerPlugin: Plugin<'bar'> = {
  id: 'peerBinMarkers',
  afterDatasetsDraw(chart: ChartType) {
    const pluginOpts = (
      chart.options.plugins as
        | { peerBinMarkers?: { markers?: PeerBinMarker[] } }
        | undefined
    )?.peerBinMarkers
    const markers = pluginOpts?.markers
    if (!markers?.length) return

    const meta = chart.getDatasetMeta(0)
    if (!meta?.data?.length) return

    const byBin = new Map<number, PeerBinMarker[]>()
    for (const marker of markers) {
      const list = byBin.get(marker.binIndex) ?? []
      list.push(marker)
      byBin.set(marker.binIndex, list)
    }

    const ctx = chart.ctx
    for (const [binIndex, binMarkers] of byBin) {
      const bar = meta.data[binIndex]
      if (!bar) continue

      const props = bar.getProps(['x', 'y'], true) as { x: number; y: number }
      const offsets = caretOffsets(binMarkers.length)
      binMarkers.forEach((marker, i) => {
        drawDownCaret(ctx, props.x + (offsets[i] ?? 0), props.y - 2, marker.color)
      })
    }
  },
}

let registered = false

export function ensurePeerBinMarkerPluginRegistered() {
  if (registered) return
  Chart.register(peerBinMarkerPlugin)
  registered = true
}
