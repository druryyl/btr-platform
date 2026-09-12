import type { PopulationMapPoint } from '@/models/entityAnalytics'
import type { AnalyzedPoint, PopulationProjectionResult, StatisticalClass } from '@/services/populationProjection/populationProjectionEngine'
import { projectedEntityToAnalyzed } from '@/services/populationProjection/populationProjectionEngine'
import {
  businessToProjectedLog,
  DAYS_PROJECTION_CAP,
  IDR_PROJECTION_FLOOR,
  isDaysAxisUnit,
  isIdrAxisUnit,
} from '@/services/populationProjection/robustStats'
import { formatNumber } from '@/services/formatters'

export { isDaysAxisUnit, isIdrAxisUnit }

/** Same week/month ladder as Peer Position Days bins (backend DaysCalendarEdges). */
export const DAYS_CALENDAR_EDGES = [0, 7, 14, 21, 30, 60, 90, 120, 180, 270, 365] as const

/** One-year threshold — same as DAYS_PROJECTION_CAP / Peer Position DaysYearEdge. */
export const DAYS_YEAR_EDGE = DAYS_PROJECTION_CAP

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

const IDR_NICE_MANTISSAS_FULL = [1, 2, 5] as const
const IDR_NICE_MANTISSAS_MEDIUM = [1, 5] as const
const IDR_NICE_MANTISSAS_SPARSE = [1] as const
const IDR_MAX_AXIS_TICKS = 12

function chooseIdrMantissas(decadeSpan: number): readonly number[] {
  if (decadeSpan > 6) return IDR_NICE_MANTISSAS_SPARSE
  if (decadeSpan > 4) return IDR_NICE_MANTISSAS_MEDIUM
  return IDR_NICE_MANTISSAS_FULL
}

/** Generate 1-2-5 × 10^n Rupiah edges within [min, max]; anchors 0 when data starts at 0. */
export function generateIdrNiceEdges(dataMin: number, dataMax: number): number[] {
  const min = Number.isFinite(dataMin) ? Math.max(0, Math.min(dataMin, dataMax)) : 0
  const max = Number.isFinite(dataMax) ? Math.max(dataMin, dataMax) : min
  if (max <= 0) return []

  const hasZeroEdge = min === 0
  // Sub-floor ladder steps collapse into the zero-anchored band; start the ladder
  // at the floor so labels stay readable. Fall back to `min` when the whole axis
  // sits below the floor.
  const ladderMin = max < IDR_PROJECTION_FLOOR ? min : Math.max(min, IDR_PROJECTION_FLOOR)
  const startExp = Math.floor(Math.log10(Math.max(ladderMin, 1)))
  const endExp = Math.ceil(Math.log10(Math.max(max, 1)))
  const decadeSpan = Math.max(1, endExp - startExp + 1)
  let mantissas = chooseIdrMantissas(decadeSpan)

  const collect = (ms: readonly number[]): number[] => {
    const edges: number[] = []
    for (let e = Math.max(0, startExp - 1); e <= endExp + 1; e++) {
      const scale = 10 ** e
      for (const m of ms) {
        const step = m * scale
        if (step >= ladderMin && step <= max) edges.push(step)
      }
    }
    return [...new Set(edges)].sort((a, b) => a - b)
  }

  let edges = collect(mantissas)
  if (edges.length > IDR_MAX_AXIS_TICKS && mantissas !== IDR_NICE_MANTISSAS_MEDIUM) {
    mantissas = IDR_NICE_MANTISSAS_MEDIUM
    edges = collect(mantissas)
  }
  if (edges.length > IDR_MAX_AXIS_TICKS) {
    edges = collect(IDR_NICE_MANTISSAS_SPARSE)
  }
  if (edges.length > IDR_MAX_AXIS_TICKS) {
    const stride = Math.ceil(edges.length / IDR_MAX_AXIS_TICKS)
    edges = edges.filter((_, i) => i % stride === 0 || i === edges.length - 1)
  }

  if (hasZeroEdge) edges.unshift(0)

  return edges
}

/**
 * Build IDR axis tick labels from a 1-2-5 Rupiah ladder, projected with the same
 * log + robust-norm transform used for scatter points (labels only; points unchanged).
 */
export function buildIdrNiceAxisTicks(
  norm: { center: number; scale: number },
  dataMin: number,
  dataMax: number,
): ProjectionAxisTick[] {
  if (!Number.isFinite(norm.center) || !Number.isFinite(norm.scale)) {
    return []
  }

  const edges = generateIdrNiceEdges(dataMin, dataMax)
  const scale = Math.abs(norm.scale) < 1e-12 ? 1e-12 : norm.scale

  return edges.map((businessValue) => {
    const logValue = businessToProjectedLog(businessValue, 'IDR')
    return {
      businessValue,
      projectionValue: (logValue - norm.center) / scale,
    }
  })
}

/**
 * Build Days axis tick labels from the calendar ladder, projected with the same
 * log + robust-norm transform used for scatter points (labels only; points unchanged).
 *
 * Includes ladder steps in [dataMin, dataMax]. Includes 0 when dataMin is near 0.
 * Includes 365 when it falls within the data range (dataMax >= 365).
 */
export function buildDaysCalendarAxisTicks(
  norm: { center: number; scale: number },
  dataMin: number,
  dataMax: number,
): ProjectionAxisTick[] {
  const min = Number.isFinite(dataMin) ? Math.max(0, Math.min(dataMin, dataMax)) : 0
  const max = Number.isFinite(dataMax) ? Math.max(dataMin, dataMax) : min
  if (!Number.isFinite(norm.center) || !Number.isFinite(norm.scale)) {
    return []
  }

  const edges: number[] = []
  for (const step of DAYS_CALENDAR_EDGES) {
    if (step > max) break
    if (step < min) {
      if (step === 0 && min < 7) edges.push(0)
      continue
    }
    edges.push(step)
  }

  const unique = [...new Set(edges)].sort((a, b) => a - b)
  const scale = Math.abs(norm.scale) < 1e-12 ? 1e-12 : norm.scale

  return unique.map((businessValue) => {
    const logValue = businessToProjectedLog(businessValue, 'Days')
    return {
      businessValue,
      projectionValue: (logValue - norm.center) / scale,
    }
  })
}

/** Resolve axis ticks: Days calendar, IDR 1-2-5, otherwise percentile fallback. */
export function resolvePopulationAxisTicks(
  unit: string | null | undefined,
  norm: { center: number; scale: number },
  dataMin: number,
  dataMax: number,
  fallback: ProjectionAxisTick[],
): ProjectionAxisTick[] {
  if (isDaysAxisUnit(unit)) {
    return buildDaysCalendarAxisTicks(norm, dataMin, dataMax)
  }
  if (isIdrAxisUnit(unit)) {
    return buildIdrNiceAxisTicks(norm, dataMin, dataMax)
  }
  return fallback
}

/**
 * PSOM-12/PSOM-13 — project an arbitrary business value with the same
 * log + robust-norm transform used for scatter points, so overlays (zero line,
 * quadrant boundaries) sit exactly on the plotted value.
 */
export function resolveBusinessProjection(
  norm: { center: number; scale: number },
  unit: string | null | undefined,
  businessValue: number,
): number {
  if (!Number.isFinite(norm.center) || !Number.isFinite(norm.scale)) return NaN
  const scale = Math.abs(norm.scale) < 1e-12 ? 1e-12 : norm.scale
  const logValue = businessToProjectedLog(businessValue, unit)
  return (logValue - norm.center) / scale
}

/**
 * PSOM-12 — business-zero helper (GAP-005).
 *
 * Business `Y = 0` is projected with the same log + robust-norm transform used
 * for scatter points, so the reference line and its tick sit exactly on the
 * plotted zero. For Percent (signed symlog) axes 0 anchors at 0; for other
 * units businessToProjectedLog(0, unit) is likewise the origin.
 */
export function resolveBusinessZeroProjection(
  norm: { center: number; scale: number },
  unit: string | null | undefined,
): number {
  return resolveBusinessProjection(norm, unit, 0)
}

export function isBusinessZeroWithinBounds(
  zeroProjection: number,
  bounds: MapBounds,
): boolean {
  if (!Number.isFinite(zeroProjection)) return false
  return zeroProjection >= bounds.minY && zeroProjection <= bounds.maxY
}

/**
 * Ensure a `0` business tick exists at the projected zero position.
 * Returns the input unchanged when zero is outside the visible Y bounds or a
 * zero tick is already present; otherwise inserts it sorted by projection value.
 */
export function ensureZeroTick(
  ticks: ProjectionAxisTick[],
  zeroProjection: number,
  bounds: MapBounds,
): ProjectionAxisTick[] {
  if (!isBusinessZeroWithinBounds(zeroProjection, bounds)) return ticks
  const hasZero = ticks.some(
    (t) => t.businessValue === 0 || Math.abs(t.projectionValue - zeroProjection) < 1e-9,
  )
  if (hasZero) return ticks
  return [...ticks, { businessValue: 0, projectionValue: zeroProjection }].sort(
    (a, b) => a.projectionValue - b.projectionValue,
  )
}

/**
 * PSOM-13 — fixed business quadrants (GAP-006 / OQ-002).
 *
 * The `principal-sales-out-map` preset replaces statistical regions with fixed
 * business quadrants at X = 100% Pacing Achievement and Y = 0% YoY MTD Growth.
 */
export const PRINCIPAL_SALES_OUT_MAP_PRESET_ID = 'principal-sales-out-map'

export const PACING_QUADRANT_X_THRESHOLD = 100
export const PACING_QUADRANT_Y_THRESHOLD = 0

export type PacingQuadrant = 'star' | 'growing' | 'steady' | 'declining'

export const PACING_QUADRANT_LABELS: Record<PacingQuadrant, string> = {
  star: 'Star',
  growing: 'Growing',
  steady: 'Steady',
  declining: 'Declining',
}

/**
 * Classify a Principal into a fixed business quadrant using thresholds — never
 * regression residuals:
 * Star (X≥100, Y≥0), Growing (X<100, Y≥0), Steady (X≥100, Y<0), Declining (X<100, Y<0).
 * Returns `null` when either business value is missing/non-finite.
 */
export function classifyPacingQuadrant(
  businessX: number | null | undefined,
  businessY: number | null | undefined,
): PacingQuadrant | null {
  if (
    businessX == null
    || businessY == null
    || !Number.isFinite(businessX)
    || !Number.isFinite(businessY)
  ) {
    return null
  }

  const strongX = businessX >= PACING_QUADRANT_X_THRESHOLD
  const positiveY = businessY >= PACING_QUADRANT_Y_THRESHOLD

  if (strongX && positiveY) return 'star'
  if (!strongX && positiveY) return 'growing'
  if (strongX && !positiveY) return 'steady'
  return 'declining'
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

export function formatAxisTickValue(
  value: number,
  unit: string | null | undefined,
  options?: { dataMax?: number },
): string {
  const normalizedUnit = unit?.trim().toLowerCase() ?? ''

  if (normalizedUnit.includes('idr') || normalizedUnit === 'rp') {
    if (value >= 1_000_000_000_000) return `${value / 1_000_000_000_000}T`
    if (value >= 1_000_000_000) return `${value / 1_000_000_000}M`
    if (value >= 1_000_000) return `${value / 1_000_000}J`
    if (value >= 1_000) return `${value / 1_000}K`
    return formatNumber(value)
  }

  if (normalizedUnit.includes('day')) {
    const rounded = Math.round(value)
    if (rounded === 0) return '0'
    if (
      rounded === DAYS_PROJECTION_CAP &&
      options?.dataMax != null &&
      options.dataMax > DAYS_PROJECTION_CAP
    ) {
      return `${DAYS_PROJECTION_CAP}+`
    }
    if (rounded >= 1_000) return `${rounded / 1_000}K`
    return String(rounded)
  }

  if (value >= 1_000_000) return `${value / 1_000_000}M`
  if (value >= 1_000) return `${value / 1_000}K`
  return formatNumber(value)
}

export function formatBinRangeLabel(
  start: number,
  end: number,
  unit: string | null | undefined,
  openEnd = false,
): string {
  const endLabel = openEnd ? '~' : formatAxisTickValue(end, unit)
  return `${formatAxisTickValue(start, unit)} – ${endLabel}`
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

export interface PacingQuadrantBoundaries {
  /** Screen X of the X = 100% boundary; null when outside the visible X bounds. */
  boundaryX: number | null
  /** Screen Y of the Y = 0% boundary; null when outside the visible Y bounds. */
  boundaryY: number | null
}

/**
 * PSOM-13 — convert the fixed business quadrant boundaries (X = 100%,
 * Y = 0%) to screen coordinates using the same transform as the plotted points.
 */
export function resolvePacingQuadrantBoundaries(
  projection: PopulationProjectionResult,
  transform: MapTransform,
  axisXUnit: string | null | undefined,
  axisYUnit: string | null | undefined,
): PacingQuadrantBoundaries {
  const xProjection = resolveBusinessProjection(
    projection.metadata.normalizationX,
    axisXUnit,
    PACING_QUADRANT_X_THRESHOLD,
  )
  const yProjection = resolveBusinessProjection(
    projection.metadata.normalizationY,
    axisYUnit,
    PACING_QUADRANT_Y_THRESHOLD,
  )

  const { bounds } = transform
  const xVisible = Number.isFinite(xProjection)
    && xProjection >= bounds.minX
    && xProjection <= bounds.maxX
  const yVisible = Number.isFinite(yProjection)
    && yProjection >= bounds.minY
    && yProjection <= bounds.maxY

  return {
    boundaryX: xVisible ? projectionToScreenX(xProjection, transform) : null,
    boundaryY: yVisible ? projectionToScreenY(yProjection, transform) : null,
  }
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
