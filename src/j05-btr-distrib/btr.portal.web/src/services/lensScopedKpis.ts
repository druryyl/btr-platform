import type { KpiEnvelope, ProfileKpiCategoryGroup } from '@/models/entityAnalytics'

export function selectLensScopedKpis(
  categories: ProfileKpiCategoryGroup[],
  kpiIds: string[] | null | undefined,
): KpiEnvelope[] {
  const allowed = kpiIds?.length ? new Set(kpiIds) : null
  return categories.flatMap((g) => g.Kpis).filter((kpi) => !allowed || allowed.has(kpi.KpiId))
}