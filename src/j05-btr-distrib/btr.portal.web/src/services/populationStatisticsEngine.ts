import type { PopulationMapPoint } from '@/models/entityAnalytics'

export interface AxisPair {
  entityId: string
  x: number
  y: number
}

export interface VisualAxisPair {
  entityId: string
  rawX: number
  rawY: number
  visualX: number
  visualY: number
}

export interface RobustRegressionModel {
  slope: number
  intercept: number
  predictY(x: number): number
}

export type StatisticalClass = 'normal' | 'watch' | 'attention' | 'critical'

export type DeviationLabel = 'Within Expected' | 'Above Expected' | 'Below Expected'

export interface AnalyzedPoint {
  entityId: string
  rawX: number
  rawY: number
  statisticalClass: StatisticalClass
  deviationLabel: DeviationLabel
  /** Internal — not exposed in UI */
  visualExpectedY: number
  visualResidual: number
  absResidualMad: number
}

export interface BandPolyline {
  x: number
  y: number
}

export interface ConfidenceBandSet {
  center: BandPolyline[]
  upper1: BandPolyline[]
  lower1: BandPolyline[]
  upper2: BandPolyline[]
  lower2: BandPolyline[]
  xMin: number
  xMax: number
}

export interface PopulationMapAnalysis {
  model: RobustRegressionModel
  mad: number
  points: Map<string, AnalyzedPoint>
  bandGeometry: ConfidenceBandSet
}

const MAD_FLOOR = 1e-9
const BAND_SAMPLE_COUNT = 80
/** Exact Theil–Sen above this count uses a deterministic subsample for slope only. */
const THEIL_SEN_EXACT_MAX = 600
const THEIL_SEN_SUBSAMPLE_SIZE = 600

/** log10(max(value, 0) + 1) — safe for zero; clamps negatives before transform. */
export function businessToVisual(value: number): number {
  return Math.log10(Math.max(value, 0) + 1)
}

function median(values: number[]): number {
  if (!values.length) return 0
  const sorted = [...values].sort((a, b) => a - b)
  const mid = Math.floor(sorted.length / 2)
  return sorted.length % 2 === 0
    ? (sorted[mid - 1] + sorted[mid]) / 2
    : sorted[mid]
}

function mad(values: number[]): number {
  if (!values.length) return MAD_FLOOR
  const med = median(values)
  const deviations = values.map((v) => Math.abs(v - med))
  return Math.max(median(deviations), MAD_FLOOR)
}

export function extractVisualPairs(points: PopulationMapPoint[]): VisualAxisPair[] {
  return points
    .filter((p) => p.AxisX != null && p.AxisY != null)
    .map((p) => {
      const rawX = p.AxisX!
      const rawY = p.AxisY!
      return {
        entityId: p.EntityId,
        rawX,
        rawY,
        visualX: businessToVisual(rawX),
        visualY: businessToVisual(rawY),
      }
    })
}

function toRegressionPairs(pairs: VisualAxisPair[]): AxisPair[] {
  return pairs.map((p) => ({
    entityId: p.entityId,
    x: p.visualX,
    y: p.visualY,
  }))
}

function computeTheilSenSlopes(pairs: AxisPair[]): number[] {
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

function subsamplePairs(pairs: AxisPair[], targetSize: number): AxisPair[] {
  if (pairs.length <= targetSize) return pairs
  const sorted = [...pairs].sort((a, b) => a.x - b.x)
  const step = sorted.length / targetSize
  const sample: AxisPair[] = []
  for (let i = 0; i < targetSize; i++) {
    sample.push(sorted[Math.min(Math.floor(i * step), sorted.length - 1)])
  }
  return sample
}

export function fitTheilSenRegression(pairs: AxisPair[]): RobustRegressionModel {
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

export function classifyByResidualMagnitude(absResidualMad: number): StatisticalClass {
  if (absResidualMad <= 1) return 'normal'
  if (absResidualMad <= 2) return 'watch'
  if (absResidualMad <= 3) return 'attention'
  return 'critical'
}

export function resolveDeviationLabel(
  residual: number,
  statisticalClass: StatisticalClass,
): DeviationLabel {
  if (statisticalClass === 'normal') return 'Within Expected'
  return residual > 0 ? 'Above Expected' : 'Below Expected'
}

export function computeAnalyzedPoints(
  pairs: VisualAxisPair[],
  model: RobustRegressionModel,
): { analyzed: AnalyzedPoint[]; mad: number } {
  if (!pairs.length) {
    return { analyzed: [], mad: MAD_FLOOR }
  }

  const regressionPairs = toRegressionPairs(pairs)
  const residuals = regressionPairs.map((p) => p.y - model.predictY(p.x))
  const spreadMad = mad(residuals)

  const analyzed = pairs.map((p) => {
    const visualExpectedY = model.predictY(p.visualX)
    const visualResidual = p.visualY - visualExpectedY
    const absResidualMad = Math.abs(visualResidual) / spreadMad
    const statisticalClass = classifyByResidualMagnitude(absResidualMad)

    return {
      entityId: p.entityId,
      rawX: p.rawX,
      rawY: p.rawY,
      visualExpectedY,
      visualResidual,
      absResidualMad,
      statisticalClass,
      deviationLabel: resolveDeviationLabel(visualResidual, statisticalClass),
    }
  })

  return { analyzed, mad: spreadMad }
}

export function buildConfidenceBands(
  model: RobustRegressionModel,
  spreadMad: number,
  xMin: number,
  xMax: number,
): ConfidenceBandSet {
  const safeMin = xMin
  const safeMax = xMax === xMin ? xMin + 1 : xMax
  const step = (safeMax - safeMin) / Math.max(BAND_SAMPLE_COUNT - 1, 1)

  const center: BandPolyline[] = []
  const upper1: BandPolyline[] = []
  const lower1: BandPolyline[] = []
  const upper2: BandPolyline[] = []
  const lower2: BandPolyline[] = []

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

export function analyzePopulationMap(points: PopulationMapPoint[]): PopulationMapAnalysis | null {
  const pairs = extractVisualPairs(points)
  if (!pairs.length) return null

  const regressionPairs = toRegressionPairs(pairs)
  const model = fitTheilSenRegression(regressionPairs)
  const { analyzed, mad: spreadMad } = computeAnalyzedPoints(pairs, model)

  const xValues = pairs.map((p) => p.visualX)
  const yValues = pairs.map((p) => p.visualY)
  let minX = Math.min(...xValues)
  let maxX = Math.max(...xValues)
  let minY = Math.min(...yValues)
  let maxY = Math.max(...yValues)

  for (const ap of analyzed) {
    minY = Math.min(minY, ap.visualExpectedY - 2 * spreadMad, businessToVisual(ap.rawY))
    maxY = Math.max(maxY, ap.visualExpectedY + 2 * spreadMad, businessToVisual(ap.rawY))
  }

  const bandGeometry = buildConfidenceBands(model, spreadMad, minX, maxX)

  const pointMap = new Map<string, AnalyzedPoint>()
  for (const ap of analyzed) {
    pointMap.set(ap.entityId, ap)
  }

  return {
    model,
    mad: spreadMad,
    points: pointMap,
    bandGeometry,
  }
}

export function formatStatisticalClass(statisticalClass: StatisticalClass): string {
  return statisticalClass.charAt(0).toUpperCase() + statisticalClass.slice(1)
}
