import type { PopulationMapPoint } from '@/models/entityAnalytics'
import type { AnalyzedPoint, PopulationProjectionResult, StatisticalClass } from '@/services/populationProjection/populationProjectionEngine'
import { projectedEntityToAnalyzed } from '@/services/populationProjection/populationProjectionEngine'
import { formatNumber } from '@/services/formatters'

export interface MapBounds {
  minX: number
  maxX: number
  minY: number
  maxY: number
}

export interface RawExtents {
  minX: number
  maxX: number
  minY: number
  maxY: number
}

export interface MapPadding {
  left: number
  right: number
  top: number
  bottom: number
}

export const MAP_PADDING: MapPadding = {
  left: 52,
  right: 32,
  top: 24,
  bottom: 40,
}

export interface MapTransform {
  padding: MapPadding
  scaleX: number
  scaleY: number
  offsetX: number
  offsetY: number
  plotWidth: number
  plotHeight: number
  bounds: MapBounds
}

export interface PlottedPoint {
  point: PopulationMapPoint
  screenX: number
  screenY: number
  index: number
  analyzed?: AnalyzedPoint
  labelPriority?: number
}

/** Tunable by Product without changing classification logic. */
export const CRITICAL_PERCENTILE_THRESHOLD = 10

export type BusinessAttentionTier = 'normal' | 'attention' | 'critical'

export type VisualPointTier = 'normal' | 'watch' | 'attention' | 'critical'

/** @deprecated Use resolveBusinessAttentionTier or resolveVisualTier */
export type PopulationPointTier = BusinessAttentionTier

export interface LabelCandidate {
  entityId: string
  text: string
  anchorX: number
  anchorY: number
  priority: number
}

export interface ResolvedLabel extends LabelCandidate {
  boxX: number
  boxY: number
  boxW: number
  boxH: number
  leader?: { fromX: number; fromY: number; toX: number; toY: number }
}

export interface LabelMetrics {
  boxW: number
  boxH: number
  padX: number
  padY: number
  font: string
}

export interface AutoLabelOptions {
  selectedIds: Set<string>
  searchIds: Set<string>
  hoveredId: string | null
  topAxisXCount?: number
  topAxisYCount?: number
  criticalCap?: number
  attentionCap?: number
}

export interface SpatialIndex {
  cellSize: number
  cells: Map<string, PlottedPoint[]>
}

const LABEL_FONT = '11px system-ui, sans-serif'
const LABEL_PAD_X = 4
const LABEL_PAD_Y = 2
const LABEL_TEXT_HEIGHT = 14
const LABEL_COLLISION_PAD = 2
const LABEL_LEADER_DISTANCE = 12

const TIER_RANK: Record<VisualPointTier, number> = {
  normal: 0,
  watch: 1,
  attention: 2,
  critical: 3,
}

const BUSINESS_RANK: Record<BusinessAttentionTier, number> = {
  normal: 0,
  attention: 2,
  critical: 3,
}

const RANK_TO_TIER: VisualPointTier[] = ['normal', 'watch', 'attention', 'critical']

const LABEL_OFFSETS: Array<{ dx: number; dy: number }> = [
  { dx: 10, dy: -10 },
  { dx: -10, dy: -10 },
  { dx: 10, dy: 10 },
  { dx: -10, dy: 10 },
  { dx: 0, dy: -14 },
  { dx: 0, dy: 14 },
  { dx: 16, dy: 0 },
  { dx: -16, dy: 0 },
  { dx: 0, dy: -22 },
  { dx: 0, dy: 22 },
  { dx: 22, dy: -14 },
  { dx: -22, dy: -14 },
]

function isExtremePercentile(
  percentile: number | null | undefined,
  threshold = CRITICAL_PERCENTILE_THRESHOLD,
): boolean {
  if (percentile == null) return false
  const upper = 100 - threshold
  return percentile <= threshold || percentile >= upper
}

export function resolveBusinessAttentionTier(point: PopulationMapPoint): BusinessAttentionTier {
  const count = point.ActiveAttentionCount ?? 0
  if (count <= 0) return 'normal'

  const isCritical =
    count >= 2
    || (count >= 1
      && (isExtremePercentile(point.AxisXPercentile) || isExtremePercentile(point.AxisYPercentile)))

  return isCritical ? 'critical' : 'attention'
}

/** @deprecated Use resolveBusinessAttentionTier */
export function resolvePopulationPointTier(point: PopulationMapPoint): BusinessAttentionTier {
  return resolveBusinessAttentionTier(point)
}

export function resolveVisualTier(
  analyzed: AnalyzedPoint | undefined,
  businessTier: BusinessAttentionTier,
): VisualPointTier {
  const statisticalClass: StatisticalClass = analyzed?.statisticalClass ?? 'normal'
  const visualRank = Math.max(TIER_RANK[statisticalClass], BUSINESS_RANK[businessTier])
  return RANK_TO_TIER[visualRank]
}

export function measureLabel(ctx: CanvasRenderingContext2D, text: string): LabelMetrics {
  ctx.font = LABEL_FONT
  const metrics = ctx.measureText(text)
  const boxW = metrics.width + LABEL_PAD_X * 2
  const boxH = LABEL_TEXT_HEIGHT + LABEL_PAD_Y * 2
  return {
    boxW,
    boxH,
    padX: LABEL_PAD_X,
    padY: LABEL_PAD_Y,
    font: LABEL_FONT,
  }
}

function boxesOverlap(
  a: { x: number; y: number; w: number; h: number },
  b: { x: number; y: number; w: number; h: number },
  pad: number,
): boolean {
  return !(
    a.x + a.w + pad <= b.x
    || b.x + b.w + pad <= a.x
    || a.y + a.h + pad <= b.y
    || b.y + b.h + pad <= a.y
  )
}

export function resolveLabelPlacements(
  candidates: LabelCandidate[],
  ctx: CanvasRenderingContext2D,
): ResolvedLabel[] {
  const sorted = [...candidates].sort((a, b) => b.priority - a.priority)
  const placed: ResolvedLabel[] = []

  for (const candidate of sorted) {
    const { boxW, boxH } = measureLabel(ctx, candidate.text)
    let placedCandidate = false

    for (const { dx, dy } of LABEL_OFFSETS) {
      const boxX = candidate.anchorX + dx
      const boxY = candidate.anchorY + dy - boxH

      const overlaps = placed.some((existing) =>
        boxesOverlap(
          { x: boxX, y: boxY, w: boxW, h: boxH },
          { x: existing.boxX, y: existing.boxY, w: existing.boxW, h: existing.boxH },
          LABEL_COLLISION_PAD,
        ),
      )

      if (overlaps) continue

      const labelCenterX = boxX + boxW / 2
      const labelCenterY = boxY + boxH / 2
      const dist = Math.hypot(labelCenterX - candidate.anchorX, labelCenterY - candidate.anchorY)

      const resolved: ResolvedLabel = {
        ...candidate,
        boxX,
        boxY,
        boxW,
        boxH,
      }

      if (dist > LABEL_LEADER_DISTANCE) {
        resolved.leader = {
          fromX: candidate.anchorX,
          fromY: candidate.anchorY,
          toX: labelCenterX,
          toY: labelCenterY,
        }
      }

      placed.push(resolved)
      placedCandidate = true
      break
    }

    if (!placedCandidate) {
      // Drop lower-priority label when all offsets collide.
    }
  }

  return placed
}

export function computeRawExtents(points: PopulationMapPoint[]): RawExtents {
  const values = points.filter((p) => p.AxisX != null && p.AxisY != null)
  if (!values.length) {
    return { minX: 0, maxX: 1, minY: 0, maxY: 1 }
  }

  let minX = Infinity
  let maxX = -Infinity
  let minY = Infinity
  let maxY = -Infinity

  for (const p of values) {
    minX = Math.min(minX, p.AxisX!)
    maxX = Math.max(maxX, p.AxisX!)
    minY = Math.min(minY, p.AxisY!)
    maxY = Math.max(maxY, p.AxisY!)
  }

  return { minX, maxX, minY, maxY }
}

export function computeBounds(
  projection?: PopulationProjectionResult | null,
): MapBounds {
  if (!projection) {
    return { minX: 0, maxX: 1, minY: 0, maxY: 1 }
  }
  return projection.bounds
}

function niceStep(span: number, targetTicks: number): number {
  const rough = span / targetTicks
  const magnitude = Math.pow(10, Math.floor(Math.log10(rough)))
  const normalized = rough / magnitude

  let nice: number
  if (normalized <= 1.5) nice = 1
  else if (normalized <= 3.5) nice = 2
  else if (normalized <= 7.5) nice = 5
  else nice = 10

  return nice * magnitude
}

export function generateLinearAxisTicks(min: number, max: number, targetTicks = 6): number[] {
  if (min === max) {
    return [min, min + 1]
  }

  const span = max - min
  const step = niceStep(span, targetTicks)
  const start = Math.ceil(min / step) * step
  const ticks: number[] = []

  for (let tick = start; tick <= max + step * 0.001; tick += step) {
    ticks.push(tick)
  }

  if (!ticks.length) {
    ticks.push(min, max)
  }

  return ticks
}

export interface ProjectionAxisTick {
  businessValue: number
  projectionValue: number
}

export function generateProjectionAxisGuides(
  projection: PopulationProjectionResult,
): { xTicks: ProjectionAxisTick[]; yTicks: ProjectionAxisTick[] } {
  const xTicks = projection.axisGuides
    .filter((g) => g.axis === 'x')
    .map((g) => ({ businessValue: g.businessValue, projectionValue: g.projectionValue }))
    .sort((a, b) => a.projectionValue - b.projectionValue)

  const yTicks = projection.axisGuides
    .filter((g) => g.axis === 'y')
    .map((g) => ({ businessValue: g.businessValue, projectionValue: g.projectionValue }))
    .sort((a, b) => a.projectionValue - b.projectionValue)

  return { xTicks, yTicks }
}

export function generateProjectionGridTicks(
  bounds: MapBounds,
  targetTicks = 6,
): { xTicks: number[]; yTicks: number[] } {
  return {
    xTicks: generateLinearAxisTicks(bounds.minX, bounds.maxX, targetTicks),
    yTicks: generateLinearAxisTicks(bounds.minY, bounds.maxY, targetTicks),
  }
}

/** @deprecated Use generateProjectionAxisGuides */
export interface LogMappedAxisTick {
  businessValue: number
  visualValue: number
}

/** @deprecated Use generateProjectionAxisGuides */
export function generateLogMappedAxisTicks(
  rawMin: number,
  rawMax: number,
  targetTicks = 6,
): LogMappedAxisTick[] {
  const min = Math.max(rawMin, 0)
  const max = Math.max(rawMax, min)
  if (min === max) {
    return [{ businessValue: min, visualValue: min }]
  }
  const ticks = generateLinearAxisTicks(min, max, targetTicks)
  return ticks.map((businessValue) => ({ businessValue, visualValue: businessValue }))
}

export function formatAxisTickValue(value: number, unit: string | null | undefined): string {
  const normalizedUnit = unit?.trim().toLowerCase() ?? ''

  if (normalizedUnit.includes('idr') || normalizedUnit === 'rp') {
    if (value >= 1_000_000) return `${value / 1_000_000}M`
    if (value >= 1_000) return `${value / 1_000}K`
    return formatNumber(value)
  }

  if (normalizedUnit.includes('day')) {
    if (value === 0) return '0'
    if (value === 1) return '1'
    if (value >= 1_000) return `${value / 1_000}K`
    return formatNumber(value)
  }

  if (value >= 1_000_000) return `${value / 1_000_000}M`
  if (value >= 1_000) return `${value / 1_000}K`
  return formatNumber(value)
}

export function buildTransform(width: number, height: number, bounds: MapBounds): MapTransform {
  const padding = MAP_PADDING
  const plotWidth = Math.max(width - padding.left - padding.right, 1)
  const plotHeight = Math.max(height - padding.top - padding.bottom, 1)

  const spanX = bounds.maxX - bounds.minX || 1
  const spanY = bounds.maxY - bounds.minY || 1

  const scaleX = plotWidth / spanX
  const scaleY = plotHeight / spanY

  return {
    padding,
    scaleX,
    scaleY,
    offsetX: padding.left,
    offsetY: padding.top,
    plotWidth,
    plotHeight,
    bounds,
  }
}

export function dataToScreen(x: number, y: number, transform: MapTransform): { x: number; y: number } {
  const screenX = transform.offsetX + (x - transform.bounds.minX) * transform.scaleX
  const screenY =
    transform.offsetY
    + transform.plotHeight
    - (y - transform.bounds.minY) * transform.scaleY
  return { x: screenX, y: screenY }
}

export function rawToScreenX(rawX: number, transform: MapTransform): number {
  return dataToScreen(rawX, transform.bounds.minY, transform).x
}

export function rawToScreenY(rawY: number, transform: MapTransform): number {
  return dataToScreen(transform.bounds.minX, rawY, transform).y
}

export function projectionToScreenX(projectionValue: number, transform: MapTransform): number {
  return dataToScreen(projectionValue, transform.bounds.minY, transform).x
}

export function projectionToScreenY(projectionValue: number, transform: MapTransform): number {
  return dataToScreen(transform.bounds.minX, projectionValue, transform).y
}

/** @deprecated Use projectionToScreenX */
export function rawToScreenXFromBusiness(rawTick: number, transform: MapTransform): number {
  return projectionToScreenX(rawTick, transform)
}

/** @deprecated Use projectionToScreenY */
export function rawToScreenYFromBusiness(rawTick: number, transform: MapTransform): number {
  return projectionToScreenY(rawTick, transform)
}

export function plotPoints(
  points: PopulationMapPoint[],
  transform: MapTransform,
  projection?: PopulationProjectionResult | null,
): PlottedPoint[] {
  const result: PlottedPoint[] = []
  for (let index = 0; index < points.length; index++) {
    const point = points[index]
    if (point.AxisX == null || point.AxisY == null) continue

    const projected = projection?.entities.get(point.EntityId)
    if (!projected) continue

    const { x, y } = dataToScreen(projected.projectionX, projected.projectionY, transform)
    const analyzed: AnalyzedPoint = projectedEntityToAnalyzed(projected)
    result.push({
      point,
      screenX: x,
      screenY: y,
      index,
      analyzed,
      labelPriority: projected.labelPriority,
    })
  }
  return result
}

export function buildSpatialIndex(plotted: PlottedPoint[], cellSize = 24): SpatialIndex {
  const cells = new Map<string, PlottedPoint[]>()
  for (const item of plotted) {
    const cx = Math.floor(item.screenX / cellSize)
    const cy = Math.floor(item.screenY / cellSize)
    const key = `${cx},${cy}`
    const bucket = cells.get(key)
    if (bucket) bucket.push(item)
    else cells.set(key, [item])
  }
  return { cellSize, cells }
}

export function findNearestPoint(
  plotted: PlottedPoint[],
  x: number,
  y: number,
  radius = 10,
  index?: SpatialIndex | null,
): PlottedPoint | null {
  if (index && index.cells.size > 0) {
    return findNearestPointIndexed(index, x, y, radius)
  }

  let best: PlottedPoint | null = null
  let bestDist = radius * radius

  for (const item of plotted) {
    const dx = item.screenX - x
    const dy = item.screenY - y
    const dist = dx * dx + dy * dy
    if (dist <= bestDist) {
      bestDist = dist
      best = item
    }
  }

  return best
}

function findNearestPointIndexed(
  index: SpatialIndex,
  x: number,
  y: number,
  radius: number,
): PlottedPoint | null {
  const cx = Math.floor(x / index.cellSize)
  const cy = Math.floor(y / index.cellSize)
  let best: PlottedPoint | null = null
  let bestDist = radius * radius

  for (let dx = -1; dx <= 1; dx++) {
    for (let dy = -1; dy <= 1; dy++) {
      const bucket = index.cells.get(`${cx + dx},${cy + dy}`)
      if (!bucket) continue
      for (const item of bucket) {
        const ddx = item.screenX - x
        const ddy = item.screenY - y
        const dist = ddx * ddx + ddy * ddy
        if (dist <= bestDist) {
          bestDist = dist
          best = item
        }
      }
    }
  }

  return best
}

export function buildAutoLabelCandidates(
  plotted: PlottedPoint[],
  options: AutoLabelOptions,
): LabelCandidate[] {
  const candidates: LabelCandidate[] = []
  const seen = new Set<string>()

  const {
    topAxisXCount = 5,
    topAxisYCount = 5,
    criticalCap = 8,
    attentionCap = 8,
  } = options

  function add(plot: PlottedPoint, priority: number) {
    if (seen.has(plot.point.EntityId)) return
    seen.add(plot.point.EntityId)
    candidates.push({
      entityId: plot.point.EntityId,
      text: plot.point.DisplayName,
      anchorX: plot.screenX,
      anchorY: plot.screenY,
      priority,
    })
  }

  for (const id of options.selectedIds) {
    const plot = plotted.find((p) => p.point.EntityId === id)
    if (plot) add(plot, 100)
  }

  for (const id of options.searchIds) {
    const plot = plotted.find((p) => p.point.EntityId === id)
    if (plot) add(plot, 90)
  }

  const critical = plotted.filter((p) => p.analyzed?.statisticalClass === 'critical')
  const attention = plotted.filter((p) => p.analyzed?.statisticalClass === 'attention')

  for (const plot of critical.slice(0, criticalCap)) {
    add(plot, Math.max(plot.labelPriority ?? 0, 70))
  }

  for (const plot of attention.slice(0, attentionCap)) {
    add(plot, Math.max(plot.labelPriority ?? 0, 60))
  }

  const byX = [...plotted]
    .filter((p) => p.point.AxisX != null)
    .sort((a, b) => b.point.AxisX! - a.point.AxisX!)
  for (const plot of byX.slice(0, topAxisXCount)) {
    add(plot, 50)
  }

  const byY = [...plotted]
    .filter((p) => p.point.AxisY != null)
    .sort((a, b) => b.point.AxisY! - a.point.AxisY!)
  for (const plot of byY.slice(0, topAxisYCount)) {
    add(plot, 45)
  }

  if (options.hoveredId) {
    const plot = plotted.find((p) => p.point.EntityId === options.hoveredId)
    if (plot) add(plot, 40)
  }

  return candidates
}

export function formatPercentile(value: number | null | undefined): string {
  if (value == null || Number.isNaN(value)) return '—'
  return `Above ${value.toFixed(0)}% of peers`
}
