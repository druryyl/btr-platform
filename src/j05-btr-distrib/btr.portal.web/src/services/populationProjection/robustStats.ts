import type { RegressionPair, RobustRegressionModel } from '@/services/populationProjection/types'

export const MAD_FLOOR = 1e-9
export const STDDEV_FLOOR = 1e-9
export const BAND_SAMPLE_COUNT = 80
/** Exact Theil–Sen above this count uses a deterministic subsample for slope only. */
export const THEIL_SEN_EXACT_MAX = 600
export const THEIL_SEN_SUBSAMPLE_SIZE = 600

/**
 * IDR amounts below this are treated as the floor for Population Map log projection.
 * The 0–10K band is compressed into a tiny zero-anchored space so tiny/no-meaning
 * piutang do not stretch the scale, while 0 itself still projects at the origin.
 */
export const IDR_PROJECTION_FLOOR = 10_000

/**
 * Days values above this are treated as the ceiling for Population Map log projection
 * so extreme DoS (e.g. 10k days) do not stretch the scale; tooltips still show the true value.
 */
export const DAYS_PROJECTION_CAP = 365

export function isIdrAxisUnit(unit: string | null | undefined): boolean {
  const normalized = unit?.trim().toLowerCase() ?? ''
  return normalized === 'idr' || normalized === 'rp' || normalized.includes('idr')
}

export function isDaysAxisUnit(unit: string | null | undefined): boolean {
  const normalized = unit?.trim().toLowerCase() ?? ''
  return normalized === 'days' || normalized.includes('day')
}

export function isPercentAxisUnit(unit: string | null | undefined): boolean {
  const normalized = unit?.trim().toLowerCase() ?? ''
  return normalized === '%' || normalized === 'pct' || normalized.includes('percent')
}

/**
 * Symmetric log (symlog): sign-preserving and monotonic, anchoring 0 at 0.
 * Positive and negative magnitudes compress logarithmically while preserving ordering.
 */
export function businessToSignedLog(value: number): number {
  if (value >= 0) return Math.log10(value + 1)
  return -Math.log10(-value + 1)
}

/** log10(max(value, 0) + 1) — safe for zero; clamps negatives before transform. */
export function businessToLog(value: number): number {
  return Math.log10(Math.max(value, 0) + 1)
}

/**
 * Log-space projection offset for IDR axes. Subtracting it anchors the origin at 0
 * while the floor (Rp 10K) lands 1 log-unit above the origin — the compressed
 * "no business value" band (0–10K) occupies just one small unit on the axis.
 */
export const IDR_LOG_COMPRESSION = businessToLog(IDR_PROJECTION_FLOOR) - 1

/**
 * Final log-domain projection position for a business value after unit-specific clamps:
 * - Percent: signed symmetric-log (symlog) so negative ratios keep their sign and
 *   ordering (0 anchors at 0; magnitude compresses logarithmically on each side).
 * - IDR: 0 → 0 (origin); values below the floor collapse into the zero-anchored band;
 *   above the floor, ordinary log minus a constant offset.
 * - Days: ceiling at DAYS_PROJECTION_CAP.
 * Otherwise pass through (still clamp negatives at 0).
 */
export function businessToProjectedLog(value: number, unit: string | null | undefined): number {
  if (isPercentAxisUnit(unit)) {
    return businessToSignedLog(value)
  }
  const clamped = Math.max(value, 0)
  if (isIdrAxisUnit(unit)) {
    return Math.max(businessToLog(clamped) - IDR_LOG_COMPRESSION, 0)
  }
  if (isDaysAxisUnit(unit)) {
    return businessToLog(Math.min(clamped, DAYS_PROJECTION_CAP))
  }
  return businessToLog(clamped)
}

export function median(values: number[]): number {
  if (!values.length) return 0
  const sorted = [...values].sort((a, b) => a - b)
  const mid = Math.floor(sorted.length / 2)
  return sorted.length % 2 === 0
    ? (sorted[mid - 1] + sorted[mid]) / 2
    : sorted[mid]
}

export function mad(values: number[]): number {
  if (!values.length) return MAD_FLOOR
  const med = median(values)
  const deviations = values.map((v) => Math.abs(v - med))
  return Math.max(median(deviations), MAD_FLOOR)
}

function mean(values: number[]): number {
  if (!values.length) return 0
  return values.reduce((sum, v) => sum + v, 0) / values.length
}

function stddev(values: number[]): number {
  if (values.length < 2) return STDDEV_FLOOR
  const avg = mean(values)
  const variance = values.reduce((sum, v) => sum + (v - avg) ** 2, 0) / values.length
  return Math.max(Math.sqrt(variance), STDDEV_FLOOR)
}

export interface RobustNormalizeResult {
  normalized: number[]
  center: number
  scale: number
  method: 'mad' | 'stddev'
}

export function robustNormalize(values: number[]): RobustNormalizeResult {
  if (!values.length) {
    return { normalized: [], center: 0, scale: MAD_FLOOR, method: 'mad' }
  }

  const med = median(values)
  const spreadMad = mad(values)

  if (spreadMad > MAD_FLOOR * 10) {
    return {
      normalized: values.map((v) => (v - med) / spreadMad),
      center: med,
      scale: spreadMad,
      method: 'mad',
    }
  }

  const avg = mean(values)
  const spreadStd = stddev(values)
  return {
    normalized: values.map((v) => (v - avg) / spreadStd),
    center: avg,
    scale: spreadStd,
    method: 'stddev',
  }
}

function computeTheilSenSlopes(pairs: RegressionPair[]): number[] {
  const slopes: number[] = []
  for (let i = 0; i < pairs.length; i++) {
    for (let j = i + 1; j < pairs.length; j++) {
      const dx = pairs[j].x - pairs[i].x
      if (dx !== 0) {
        slopes.push((pairs[j].y - pairs[i].y) / dx)
      }
    }
  }
  return slopes
}

function subsamplePairs(pairs: RegressionPair[], targetSize: number): RegressionPair[] {
  if (pairs.length <= targetSize) return pairs
  const sorted = [...pairs].sort((a, b) => a.x - b.x)
  const step = sorted.length / targetSize
  const sample: RegressionPair[] = []
  for (let i = 0; i < targetSize; i++) {
    sample.push(sorted[Math.min(Math.floor(i * step), sorted.length - 1)])
  }
  return sample
}

export function fitTheilSenRegression(pairs: RegressionPair[]): RobustRegressionModel {
  if (!pairs.length) {
    return { slope: 0, intercept: 0, predictY: () => 0 }
  }

  if (pairs.length === 1) {
    const { y } = pairs[0]
    return { slope: 0, intercept: y, predictY: () => y }
  }

  const slopePairs =
    pairs.length > THEIL_SEN_EXACT_MAX
      ? subsamplePairs(pairs, THEIL_SEN_SUBSAMPLE_SIZE)
      : pairs

  const slopes = computeTheilSenSlopes(slopePairs)

  let slope: number
  if (!slopes.length) {
    slope = 0
  } else {
    slope = median(slopes)
  }

  const intercepts = pairs.map((p) => p.y - slope * p.x)
  const intercept = median(intercepts)

  return {
    slope,
    intercept,
    predictY(x: number) {
      return slope * x + intercept
    },
  }
}

export function classifyByResidualMagnitude(
  absResidualMad: number,
): 'normal' | 'watch' | 'attention' | 'critical' {
  if (absResidualMad <= 1) return 'normal'
  if (absResidualMad <= 2) return 'watch'
  if (absResidualMad <= 3) return 'attention'
  return 'critical'
}

export function resolveDeviationLabel(
  residual: number,
  statisticalClass: 'normal' | 'watch' | 'attention' | 'critical',
): 'Within Expected' | 'Above Expected' | 'Below Expected' {
  if (statisticalClass === 'normal') return 'Within Expected'
  return residual > 0 ? 'Above Expected' : 'Below Expected'
}

export function buildConfidenceBands(
  model: RobustRegressionModel,
  spreadMad: number,
  xMin: number,
  xMax: number,
) {
  const safeMin = xMin
  const safeMax = xMax === xMin ? xMin + 1 : xMax
  const step = (safeMax - safeMin) / Math.max(BAND_SAMPLE_COUNT - 1, 1)

  const center: Array<{ x: number; y: number }> = []
  const upper1: Array<{ x: number; y: number }> = []
  const lower1: Array<{ x: number; y: number }> = []
  const upper2: Array<{ x: number; y: number }> = []
  const lower2: Array<{ x: number; y: number }> = []

  for (let i = 0; i < BAND_SAMPLE_COUNT; i++) {
    const x = safeMin + step * i
    const y = model.predictY(x)
    center.push({ x, y })
    upper1.push({ x, y: y + spreadMad })
    lower1.push({ x, y: y - spreadMad })
    upper2.push({ x, y: y + 2 * spreadMad })
    lower2.push({ x, y: y - 2 * spreadMad })
  }

  return { center, upper1, lower1, upper2, lower2, xMin: safeMin, xMax: safeMax }
}

const TIER_PRIORITY: Record<'normal' | 'watch' | 'attention' | 'critical', number> = {
  normal: 10,
  watch: 30,
  attention: 50,
  critical: 70,
}

export function computeLabelPriority(
  statisticalClass: 'normal' | 'watch' | 'attention' | 'critical',
  absResidualMad: number,
): { priorityScore: number; labelPriority: number } {
  const tierBase = TIER_PRIORITY[statisticalClass]
  const priorityScore = tierBase + Math.min(absResidualMad, 5)
  const labelPriority = tierBase
  return { priorityScore, labelPriority }
}

export function percentileValue(sortedValues: number[], percentile: number): number {
  if (!sortedValues.length) return 0
  if (sortedValues.length === 1) return sortedValues[0]
  const index = (percentile / 100) * (sortedValues.length - 1)
  const lower = Math.floor(index)
  const upper = Math.ceil(index)
  if (lower === upper) return sortedValues[lower]
  const weight = index - lower
  return sortedValues[lower] * (1 - weight) + sortedValues[upper] * weight
}
