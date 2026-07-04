/** @deprecated Import from populationProjection/populationProjectionEngine instead. */
export {
  analyzePopulationMap,
  businessToVisual,
  classifyByResidualMagnitude,
  fitTheilSenRegression,
  formatStatisticalClass,
  populationProjectionEngine,
  projectedEntityToAnalyzed,
  projectionResultToLegacyAnalysis,
  resolveDeviationLabel,
  type AnalyzedPoint,
  type AxisGuide,
  type BandPolyline,
  type ConfidenceBandSet,
  type DeviationLabel,
  type PopulationMapAnalysis,
  type PopulationProjectionResult,
  type ProjectedEntity,
  type RobustRegressionModel,
  type StatisticalClass,
} from '@/services/populationProjection/populationProjectionEngine'

import type { PopulationMapPoint } from '@/models/entityAnalytics'
import type { AnalyzedPoint, RobustRegressionModel } from '@/services/populationProjection/types'
import {
  businessToLog,
  classifyByResidualMagnitude,
  mad,
  resolveDeviationLabel,
} from '@/services/populationProjection/robustStats'

export type { RegressionPair as AxisPair } from '@/services/populationProjection/types'

/** @deprecated */
export interface VisualAxisPair {
  entityId: string
  rawX: number
  rawY: number
  visualX: number
  visualY: number
}

export { buildConfidenceBands } from '@/services/populationProjection/robustStats'

/** @deprecated Use populationProjectionEngine.project */
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
        visualX: businessToLog(rawX),
        visualY: businessToLog(rawY),
      }
    })
}

/** @deprecated Use populationProjectionEngine.project */
export function computeAnalyzedPoints(
  pairs: VisualAxisPair[],
  model: RobustRegressionModel,
): { analyzed: AnalyzedPoint[]; mad: number } {
  const residuals = pairs.map((p) => p.visualY - model.predictY(p.visualX))
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
