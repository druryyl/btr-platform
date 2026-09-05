import type { PopulationMapPoint } from '@/models/entityAnalytics'
import type {
  IPopulationProjectionStrategy,
  PopulationEntityInput,
  PopulationMapAnalysis,
  PopulationProjectionOptions,
  PopulationProjectionResult,
  StatisticalClass,
} from '@/services/populationProjection/types'
import { projectionResultToLegacyAnalysis } from '@/services/populationProjection/types'
import { robustProjectionStrategy } from '@/services/populationProjection/strategies/robustProjectionStrategy'
import { classifyByResidualMagnitude, fitTheilSenRegression, resolveDeviationLabel } from '@/services/populationProjection/robustStats'

export function formatStatisticalClass(statisticalClass: StatisticalClass): string {
  return statisticalClass.charAt(0).toUpperCase() + statisticalClass.slice(1)
}

function toEntityInput(point: PopulationMapPoint): PopulationEntityInput {
  return {
    entityId: point.EntityId,
    label: point.DisplayName,
    businessX: point.AxisX,
    businessY: point.AxisY,
  }
}

export class PopulationProjectionEngine {
  private readonly defaultStrategy: IPopulationProjectionStrategy

  constructor(defaultStrategy: IPopulationProjectionStrategy = robustProjectionStrategy) {
    this.defaultStrategy = defaultStrategy
  }

  project(
    points: PopulationMapPoint[],
    options?: PopulationProjectionOptions,
    strategy?: IPopulationProjectionStrategy,
  ): PopulationProjectionResult | null {
    const inputs = points.map(toEntityInput)
    const activeStrategy = strategy ?? this.defaultStrategy
    return activeStrategy.project(inputs, options)
  }
}

export const populationProjectionEngine = new PopulationProjectionEngine()

export {
  type AnalyzedPoint,
  type AxisGuide,
  type BandPolyline,
  type ConfidenceBandSet,
  type DeviationLabel,
  type MapBounds,
  type PopulationEntityInput,
  type PopulationMapAnalysis,
  type PopulationProjectionMetadata,
  type PopulationProjectionOptions,
  type PopulationProjectionResult,
  type ProjectedEntity,
  type RobustRegressionModel,
  type StatisticalClass,
  projectedEntityToAnalyzed,
  projectionResultToLegacyAnalysis,
} from '@/services/populationProjection/types'

export {
  businessToLog as businessToVisual,
  DAYS_PROJECTION_CAP,
  IDR_PROJECTION_FLOOR,
  isDaysAxisUnit,
  isIdrAxisUnit,
} from '@/services/populationProjection/robustStats'

export { classifyByResidualMagnitude, fitTheilSenRegression, resolveDeviationLabel }

/** @deprecated Use populationProjectionEngine.project */
export function analyzePopulationMap(points: PopulationMapPoint[]): PopulationMapAnalysis | null {
  const result = populationProjectionEngine.project(points)
  if (!result) return null
  return projectionResultToLegacyAnalysis(result)
}
