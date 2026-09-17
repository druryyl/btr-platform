import { describe, expect, it } from 'vitest'
import type { KpiEnvelope, ProfileKpiCategoryGroup } from '@/models/entityAnalytics'
import {
  computePurchaseToSalesOutRatio,
  PRINCIPAL_PURCHASE_IN_KPI_ID,
  PRINCIPAL_SALES_OUT_KPI_ID,
  PURCHASE_TO_SALES_OUT_RATIO_ID,
  selectLensDerivedMetrics,
} from '@/services/investigationDerivedMetrics'

function kpi(kpiId: string, value: number | null, displayName = kpiId): KpiEnvelope {
  return {
    KpiId: kpiId,
    Category: 'Financial',
    DisplayName: displayName,
    Value: value,
    TextValue: null,
    FormattedValue: '',
    Unit: 'IDR',
    Direction: 'HigherIsBetter',
    PeriodLabel: 'MTD',
    EvidenceRoute: null,
    FilterDimension: null,
    ValueType: 'Numeric',
    DisplayPrecision: 0,
    TrendEligible: true,
    RankEligible: false,
    NullableBehavior: 'ShowEmpty',
  }
}

function categories(rows: KpiEnvelope[]): ProfileKpiCategoryGroup[] {
  return [{ Category: 'All', Kpis: rows }]
}

describe('computePurchaseToSalesOutRatio', () => {
  it('derives the ratio from stored PRN-PUR-001 and PRN-SALES-001 values', () => {
    const result = computePurchaseToSalesOutRatio([
      kpi(PRINCIPAL_PURCHASE_IN_KPI_ID, 150, 'Purchase-In'),
      kpi(PRINCIPAL_SALES_OUT_KPI_ID, 100, 'Principal Sales-Out'),
    ])

    expect(result.MetricId).toBe(PURCHASE_TO_SALES_OUT_RATIO_ID)
    expect(result.IsAvailable).toBe(true)
    expect(result.Value).toBeCloseTo(1.5)
    expect(result.FormattedValue).toBe('1.50')
    expect(result.Interpretation?.Code).toBe('over-buying')
    expect(result.Interpretation?.Meaning).toBe(
      'Inventory Building / Potential Over-Buying',
    )
    expect(result.NumeratorLabel).toBe('Purchase-In')
    expect(result.DenominatorLabel).toBe('Principal Sales-Out')
  })

  it('is balanced when the ratio equals 1.0', () => {
    const result = computePurchaseToSalesOutRatio([
      kpi(PRINCIPAL_PURCHASE_IN_KPI_ID, 80),
      kpi(PRINCIPAL_SALES_OUT_KPI_ID, 80),
    ])

    expect(result.Value).toBe(1)
    expect(result.Interpretation?.Code).toBe('balanced')
    expect(result.Interpretation?.Meaning).toBe('Balanced')
  })

  it('is under-buying when the ratio is below 1.0', () => {
    const result = computePurchaseToSalesOutRatio([
      kpi(PRINCIPAL_PURCHASE_IN_KPI_ID, 40),
      kpi(PRINCIPAL_SALES_OUT_KPI_ID, 100),
    ])

    expect(result.Value).toBeCloseTo(0.4)
    expect(result.Interpretation?.Code).toBe('under-buying')
  })

  it('is unavailable when Sales-Out is zero or missing, or Purchase-In is missing', () => {
    const zeroSales = computePurchaseToSalesOutRatio([
      kpi(PRINCIPAL_PURCHASE_IN_KPI_ID, 100),
      kpi(PRINCIPAL_SALES_OUT_KPI_ID, 0),
    ])
    const missingSales = computePurchaseToSalesOutRatio([
      kpi(PRINCIPAL_PURCHASE_IN_KPI_ID, 100),
    ])
    const missingPurchase = computePurchaseToSalesOutRatio([
      kpi(PRINCIPAL_SALES_OUT_KPI_ID, 100),
    ])

    expect(zeroSales.IsAvailable).toBe(false)
    expect(zeroSales.Value).toBeNull()
    expect(zeroSales.FormattedValue).toBe('—')
    expect(zeroSales.Interpretation).toBeNull()
    expect(missingSales.IsAvailable).toBe(false)
    expect(missingPurchase.IsAvailable).toBe(false)
  })
})

describe('selectLensDerivedMetrics', () => {
  it('returns no derived metrics when the lens declares none (non-lensed entities)', () => {
    const groups = categories([
      kpi(PRINCIPAL_PURCHASE_IN_KPI_ID, 150),
      kpi(PRINCIPAL_SALES_OUT_KPI_ID, 100),
    ])

    expect(selectLensDerivedMetrics(groups, null)).toEqual([])
    expect(selectLensDerivedMetrics(groups, undefined)).toEqual([])
    expect(selectLensDerivedMetrics(groups, [])).toEqual([])
  })

  it('returns the Purchase-to-Sales-Out Ratio when the lens declares it', () => {
    const groups = categories([
      kpi(PRINCIPAL_PURCHASE_IN_KPI_ID, 150),
      kpi(PRINCIPAL_SALES_OUT_KPI_ID, 100),
    ])

    const metrics = selectLensDerivedMetrics(groups, [PURCHASE_TO_SALES_OUT_RATIO_ID])

    expect(metrics).toHaveLength(1)
    expect(metrics[0].MetricId).toBe(PURCHASE_TO_SALES_OUT_RATIO_ID)
    expect(metrics[0].Value).toBeCloseTo(1.5)
  })

  it('ignores unknown derived-metric ids', () => {
    const groups = categories([
      kpi(PRINCIPAL_PURCHASE_IN_KPI_ID, 150),
      kpi(PRINCIPAL_SALES_OUT_KPI_ID, 100),
    ])

    expect(selectLensDerivedMetrics(groups, ['not-a-derived-metric'])).toEqual([])
  })

  it('never treats the ratio as a KPI row', () => {
    const groups = categories([
      kpi(PRINCIPAL_PURCHASE_IN_KPI_ID, 150),
      kpi(PRINCIPAL_SALES_OUT_KPI_ID, 100),
    ])

    const metrics = selectLensDerivedMetrics(groups, [PURCHASE_TO_SALES_OUT_RATIO_ID])

    expect(metrics.some((m) => m.MetricId.startsWith('PRN-'))).toBe(false)
  })
})
