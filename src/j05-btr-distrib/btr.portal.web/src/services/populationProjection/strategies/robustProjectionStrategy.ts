import type {
  AxisGuide,
  IPopulationProjectionStrategy,
  MapBounds,
  PopulationEntityInput,
  PopulationProjectionOptions,
  PopulationProjectionResult,
  ProjectedEntity,
  RegressionPair,
} from '@/services/populationProjection/types'
import {
  buildConfidenceBands,
  businessToProjectedLog,
  classifyByResidualMagnitude,
  computeLabelPriority,
  fitTheilSenRegression,
  mad,
  percentileValue,
  resolveDeviationLabel,
  robustNormalize,
} from '@/services/populationProjection/robustStats'

const AXIS_GUIDE_PERCENTILES = [10, 25, 50, 75, 90]

interface ValidatedEntity {
  entityId: string
  label: string
  businessX: number
  businessY: number
  logX: number
  logY: number
}

function isValidNumber(value: number | null): value is number {
  return value != null && Number.isFinite(value)
}

function validateEntities(
  entities: PopulationEntityInput[],
  options?: PopulationProjectionOptions,
): {
  valid: ValidatedEntity[]
  excludedCount: number
} {
  const valid: ValidatedEntity[] = []
  let excludedCount = 0

  for (const entity of entities) {
    if (!isValidNumber(entity.businessX) || !isValidNumber(entity.businessY)) {
      excludedCount++
      continue
    }

    const businessX = Math.max(entity.businessX, 0)
    const businessY = Math.max(entity.businessY, 0)

    valid.push({
      entityId: entity.entityId,
      label: entity.label,
      businessX,
      businessY,
      logX: businessToProjectedLog(businessX, options?.axisXUnit),
      logY: businessToProjectedLog(businessY, options?.axisYUnit),
    })
  }

  return { valid, excludedCount }
}

function buildAxisGuides(
  businessValues: number[],
  logValues: number[],
  normX: ReturnType<typeof robustNormalize>,
  normY: ReturnType<typeof robustNormalize>,
  axis: 'x' | 'y',
  axisUnit?: string | null,
): AxisGuide[] {
  const sorted = [...businessValues].sort((a, b) => a - b)
  const guides: AxisGuide[] = []
  const seen = new Set<number>()

  for (const percentile of AXIS_GUIDE_PERCENTILES) {
    const businessValue = percentileValue(sorted, percentile)
    const rounded = Math.round(businessValue * 1e6) / 1e6
    if (seen.has(rounded)) continue
    seen.add(rounded)

    const index = businessValues.findIndex((v) => v === businessValue)
    let logValue: number
    if (index >= 0) {
      logValue = logValues[index]
    } else {
      logValue = businessToProjectedLog(businessValue, axisUnit)
    }

    const norm = axis === 'x' ? normX : normY
    const projectionValue = (logValue - norm.center) / norm.scale

    guides.push({ axis, businessValue: rounded, projectionValue })
  }

  return guides
}

function computeProjectionBounds(
  entities: ProjectedEntity[],
  spreadMad: number,
  regression: ReturnType<typeof fitTheilSenRegression>,
): MapBounds {
  let minX = Infinity
  let maxX = -Infinity
  let minY = Infinity
  let maxY = -Infinity

  for (const entity of entities) {
    minX = Math.min(minX, entity.projectionX)
    maxX = Math.max(maxX, entity.projectionX)
    minY = Math.min(minY, entity.projectionY)
    maxY = Math.max(maxY, entity.projectionY)
  }

  for (const entity of entities) {
    minY = Math.min(minY, entity.projectionExpectedY - 2 * spreadMad)
    maxY = Math.max(maxY, entity.projectionExpectedY + 2 * spreadMad)
  }

  minY = Math.min(minY, regression.predictY(minX) - 2 * spreadMad)
  maxY = Math.max(maxY, regression.predictY(maxX) + 2 * spreadMad)

  const padX = (maxX - minX) * 0.05 || 0.1
  const padY = (maxY - minY) * 0.05 || 0.1

  return {
    minX: minX - padX,
    maxX: maxX + padX,
    minY: minY - padY,
    maxY: maxY + padY,
  }
}

export class RobustProjectionStrategy implements IPopulationProjectionStrategy {
  readonly id = 'robust'

  project(
    entities: PopulationEntityInput[],
    options?: PopulationProjectionOptions,
  ): PopulationProjectionResult | null {
    const { valid, excludedCount } = validateEntities(entities, options)
    if (!valid.length) return null

    const logXValues = valid.map((v) => v.logX)
    const logYValues = valid.map((v) => v.logY)

    const normX = robustNormalize(logXValues)
    const normY = robustNormalize(logYValues)

    const regressionPairs: RegressionPair[] = valid.map((v, i) => ({
      entityId: v.entityId,
      x: normX.normalized[i],
      y: normY.normalized[i],
    }))

    const regression = fitTheilSenRegression(regressionPairs)
    const residuals = regressionPairs.map((p) => p.y - regression.predictY(p.x))
    const residualMad = mad(residuals)

    const projectedEntities: ProjectedEntity[] = valid.map((v, i) => {
      const projectionX = normX.normalized[i]
      const projectionY = normY.normalized[i]
      const projectionExpectedY = regression.predictY(projectionX)
      const projectionResidual = projectionY - projectionExpectedY
      const absResidualMad = Math.abs(projectionResidual) / residualMad
      const statisticalClass = classifyByResidualMagnitude(absResidualMad)
      const { priorityScore, labelPriority } = computeLabelPriority(
        statisticalClass,
        absResidualMad,
      )

      return {
        entityId: v.entityId,
        label: v.label,
        businessX: v.businessX,
        businessY: v.businessY,
        projectionX,
        projectionY,
        statisticalClass,
        deviationLabel: resolveDeviationLabel(projectionResidual, statisticalClass),
        priorityScore,
        labelPriority,
        projectionExpectedY,
        projectionResidual,
        absResidualMad,
      }
    })

    const xValues = projectedEntities.map((e) => e.projectionX)
    const minX = Math.min(...xValues)
    const maxX = Math.max(...xValues)

    const bandGeometry = buildConfidenceBands(regression, residualMad, minX, maxX)
    const bounds = computeProjectionBounds(projectedEntities, residualMad, regression)

    const businessXValues = valid.map((v) => v.businessX)
    const businessYValues = valid.map((v) => v.businessY)

    const axisGuides: AxisGuide[] = [
      ...buildAxisGuides(businessXValues, logXValues, normX, normY, 'x', options?.axisXUnit),
      ...buildAxisGuides(businessYValues, logYValues, normX, normY, 'y', options?.axisYUnit),
    ]

    const entityMap = new Map<string, ProjectedEntity>()
    for (const entity of projectedEntities) {
      entityMap.set(entity.entityId, entity)
    }

    return {
      strategyId: this.id,
      entities: entityMap,
      regression,
      residualMad,
      bandGeometry,
      bounds,
      axisGuides,
      metadata: {
        strategyId: this.id,
        entityCount: valid.length,
        excludedCount,
        normalizationX: {
          center: normX.center,
          scale: normX.scale,
          method: normX.method,
        },
        normalizationY: {
          center: normY.center,
          scale: normY.scale,
          method: normY.method,
        },
      },
    }
  }
}

export const robustProjectionStrategy = new RobustProjectionStrategy()
