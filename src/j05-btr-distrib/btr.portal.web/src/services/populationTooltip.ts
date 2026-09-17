import type { PopulationMapPoint, PopulationMapResponse } from '@/models/entityAnalytics'

export interface TooltipDimensionRow {
  label: string
  value: string
}

export function resolveTooltipDimensionRow(
  point: PopulationMapPoint | null | undefined,
  population: PopulationMapResponse | null | undefined,
): TooltipDimensionRow | null {
  const label = population?.DimensionLabel?.trim()
  const value = point?.DimensionValue?.trim()
  if (!label || !value) return null
  return { label, value }
}