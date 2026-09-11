import type { KpiEnvelope, ProfileKpiCategoryGroup } from '@/models/entityAnalytics'
import { selectLensScopedKpis } from '@/services/lensScopedKpis'

/**
 * Matches EntityInvestigationDerivedMetricIds.PurchaseToSalesOutRatio on the backend.
 * Derived metrics are investigation-only and are never KPI IDs, registry entries,
 * persistence, or ranking contracts (IW-GAP-005, IW-TQ-005, IW-OQ-001).
 */
export const PURCHASE_TO_SALES_OUT_RATIO_ID = 'purchase-to-sales-out-ratio'

export const PRINCIPAL_PURCHASE_IN_KPI_ID = 'PRN-PUR-001'

export const PRINCIPAL_SALES_OUT_KPI_ID = 'PRN-SALES-001'

export const PURCHASE_TO_SALES_OUT_RATIO_LABEL = 'Purchase-to-Sales-Out Ratio'

export type DerivedMetricInterpretationCode =
  | 'over-buying'
  | 'balanced'
  | 'under-buying'
  | 'unavailable'

export interface DerivedMetricInterpretation {
  Code: DerivedMetricInterpretationCode
  Range: string
  Meaning: string
}

/** Approved interpretation bands (FEASIBILITY GAP-005 / Architecture §8.3). */
export const PURCHASE_TO_SALES_OUT_INTERPRETATIONS: DerivedMetricInterpretation[] = [
  { Code: 'over-buying', Range: '> 1.0', Meaning: 'Inventory Building / Potential Over-Buying' },
  { Code: 'balanced', Range: '≈ 1.0', Meaning: 'Balanced' },
  { Code: 'under-buying', Range: '< 1.0', Meaning: 'Inventory Drawdown / Potential Under-Buying' },
]

export interface DerivedMetricResult {
  MetricId: string
  DisplayName: string
  IsAvailable: boolean
  Value: number | null
  FormattedValue: string
  Interpretation: DerivedMetricInterpretation | null
  NumeratorKpiId: string
  NumeratorLabel: string
  NumeratorValue: number | null
  DenominatorKpiId: string
  DenominatorLabel: string
  DenominatorValue: number | null
}

function findKpi(kpis: KpiEnvelope[], kpiId: string): KpiEnvelope | undefined {
  return kpis.find((kpi) => kpi.KpiId === kpiId)
}

function resolveInterpretation(value: number): DerivedMetricInterpretation {
  if (value > 1) return PURCHASE_TO_SALES_OUT_INTERPRETATIONS[0]
  if (value < 1) return PURCHASE_TO_SALES_OUT_INTERPRETATIONS[2]
  return PURCHASE_TO_SALES_OUT_INTERPRETATIONS[1]
}

/**
 * Derives the investigation-only Purchase-to-Sales-Out Ratio from the already-stored
 * PRN-PUR-001 and PRN-SALES-001 values. No value is recomputed from source transactions.
 */
export function computePurchaseToSalesOutRatio(kpis: KpiEnvelope[]): DerivedMetricResult {
  const purchaseIn = findKpi(kpis, PRINCIPAL_PURCHASE_IN_KPI_ID)
  const salesOut = findKpi(kpis, PRINCIPAL_SALES_OUT_KPI_ID)
  const numerator = purchaseIn?.Value ?? null
  const denominator = salesOut?.Value ?? null

  const isAvailable = numerator != null && denominator != null && denominator !== 0
  const value = isAvailable ? (numerator as number) / (denominator as number) : null

  return {
    MetricId: PURCHASE_TO_SALES_OUT_RATIO_ID,
    DisplayName: PURCHASE_TO_SALES_OUT_RATIO_LABEL,
    IsAvailable: isAvailable,
    Value: value,
    FormattedValue: value == null ? '—' : value.toFixed(2),
    Interpretation: value == null ? null : resolveInterpretation(value),
    NumeratorKpiId: PRINCIPAL_PURCHASE_IN_KPI_ID,
    NumeratorLabel: purchaseIn?.DisplayName || 'Purchase-In',
    NumeratorValue: numerator,
    DenominatorKpiId: PRINCIPAL_SALES_OUT_KPI_ID,
    DenominatorLabel: salesOut?.DisplayName || 'Principal Sales-Out',
    DenominatorValue: denominator,
  }
}

/**
 * Selects the derived metrics declared by the active lens from the profile KPI summary.
 * Unknown derived-metric ids are ignored; a null/empty set yields no derived metrics,
 * preserving behavior for non-lensed entity types.
 */
export function selectLensDerivedMetrics(
  categories: ProfileKpiCategoryGroup[],
  metricIds: string[] | null | undefined,
): DerivedMetricResult[] {
  if (!metricIds?.length) return []

  const kpis = selectLensScopedKpis(categories, null)
  return metricIds
    .map((metricId) => (
      metricId === PURCHASE_TO_SALES_OUT_RATIO_ID
        ? computePurchaseToSalesOutRatio(kpis)
        : null
    ))
    .filter((result): result is DerivedMetricResult => result != null)
}
