export interface PopulationEntityInput {
  entityId: string
  label: string
  businessX: number | null
  businessY: number | null
}

export interface RegressionPair {
  entityId: string
  x: number
  y: number
}

export interface RobustRegressionModel {
  slope: number
  intercept: number
  predictY(x: number): number
}

export type StatisticalClass = 'normal' | 'watch' | 'attention' | 'critical'

export type DeviationLabel = 'Within Expected' | 'Above Expected' | 'Below Expected'

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

export interface MapBounds {
  minX: number
  maxX: number
  minY: number
  maxY: number
}

export interface AxisNormalization {
  center: number
  scale: number
  method: 'mad' | 'stddev'
}

export interface ProjectedEntity {
  entityId: string
  label: string
  businessX: number
  businessY: number
  projectionX: number
  projectionY: number
  statisticalClass: StatisticalClass
  deviationLabel: DeviationLabel
  priorityScore: number
  labelPriority: number
  /** Internal — not exposed in UI */
  projectionExpectedY: number
  projectionResidual: number
  absResidualMad: number
}

/** Bridge between business-readable axis labels and projection-space positions. */
export interface AxisGuide {
  axis: 'x' | 'y'
  businessValue: number
  projectionValue: number
}

export interface PopulationProjectionMetadata {
  strategyId: string
  entityCount: number
  excludedCount: number
  normalizationX: AxisNormalization
  normalizationY: AxisNormalization
}

export interface PopulationProjectionResult {
  strategyId: string
  entities: Map<string, ProjectedEntity>
  regression: RobustRegressionModel
  residualMad: number
  bandGeometry: ConfidenceBandSet
  bounds: MapBounds
  axisGuides: AxisGuide[]
  metadata: PopulationProjectionMetadata
}

export interface IPopulationProjectionStrategy {
  readonly id: string
  project(
    entities: PopulationEntityInput[],
    options?: PopulationProjectionOptions,
  ): PopulationProjectionResult | null
}

export interface PopulationProjectionOptions {
  axisXUnit?: string | null
  axisYUnit?: string | null
}

/** @deprecated Use ProjectedEntity — kept for tooltip/layout compatibility during transition */
export interface AnalyzedPoint {
  entityId: string
  rawX: number
  rawY: number
  statisticalClass: StatisticalClass
  deviationLabel: DeviationLabel
  visualExpectedY: number
  visualResidual: number
  absResidualMad: number
}

/** @deprecated Use PopulationProjectionResult */
export interface PopulationMapAnalysis {
  model: RobustRegressionModel
  mad: number
  points: Map<string, AnalyzedPoint>
  bandGeometry: ConfidenceBandSet
}

export function projectedEntityToAnalyzed(entity: ProjectedEntity): AnalyzedPoint {
  return {
    entityId: entity.entityId,
    rawX: entity.businessX,
    rawY: entity.businessY,
    statisticalClass: entity.statisticalClass,
    deviationLabel: entity.deviationLabel,
    visualExpectedY: entity.projectionExpectedY,
    visualResidual: entity.projectionResidual,
    absResidualMad: entity.absResidualMad,
  }
}

export function projectionResultToLegacyAnalysis(
  result: PopulationProjectionResult,
): PopulationMapAnalysis {
  const points = new Map<string, AnalyzedPoint>()
  for (const entity of result.entities.values()) {
    points.set(entity.entityId, projectedEntityToAnalyzed(entity))
  }
  return {
    model: result.regression,
    mad: result.residualMad,
    points,
    bandGeometry: result.bandGeometry,
  }
}
