import { describe, expect, it } from 'vitest'
import type { KpiEnvelope, ProfileKpiCategoryGroup } from '@/models/entityAnalytics'
import { selectLensScopedKpis } from '@/services/lensScopedKpis'

function kpi(kpiId: string, category = 'Financial'): KpiEnvelope {
  return {
    KpiId: kpiId,
    Category: category,
    DisplayName: kpiId,
    Value: null,
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

const salesOutKpi = kpi('PRN-SALES-001')
const returnPercentageKpi = kpi('PRN-RET-004', 'Quality')
const achievementPercentageKpi = kpi('PRN-TGT-003')
const yoyGrowthKpi = kpi('PRN-GRW-002', 'Growth')
const purchaseInKpi = kpi('PRN-PUR-001')
const mtdPurchaseKpi = kpi('PU-KPI-001')
const inventoryValueKpi = kpi('PRN-INV-001', 'Portfolio')
const targetKpi = kpi('PRN-TGT-001')

function categories(rows: KpiEnvelope[]): ProfileKpiCategoryGroup[] {
  return [{ Category: 'All', Kpis: rows }]
}

describe('selectLensScopedKpis', () => {
  it('returns all KPIs when no lens KPI set is provided (non-lensed entity types)', () => {
    const groups = categories([salesOutKpi, purchaseInKpi, inventoryValueKpi])

    expect(selectLensScopedKpis(groups, null).map((k) => k.KpiId)).toEqual([
      'PRN-SALES-001',
      'PRN-PUR-001',
      'PRN-INV-001',
    ])
    expect(selectLensScopedKpis(groups, undefined).map((k) => k.KpiId)).toEqual([
      'PRN-SALES-001',
      'PRN-PUR-001',
      'PRN-INV-001',
    ])
    expect(selectLensScopedKpis(groups, []).map((k) => k.KpiId)).toEqual([
      'PRN-SALES-001',
      'PRN-PUR-001',
      'PRN-INV-001',
    ])
  })

  it('shows only the Sales-Out KPI set and never Purchase-In as Principal sales performance', () => {
    const groups = categories([
      salesOutKpi,
      returnPercentageKpi,
      achievementPercentageKpi,
      yoyGrowthKpi,
      purchaseInKpi,
      mtdPurchaseKpi,
      inventoryValueKpi,
      targetKpi,
    ])
    const salesOutSet = [
      'PRN-SALES-001',
      'PRN-RET-004',
      'PRN-TGT-003',
      'PRN-GRW-002',
    ]

    const selected = selectLensScopedKpis(groups, salesOutSet)

    expect(selected.map((k) => k.KpiId)).toEqual([
      'PRN-SALES-001',
      'PRN-RET-004',
      'PRN-TGT-003',
      'PRN-GRW-002',
    ])
    expect(selected.some((k) => k.KpiId === 'PRN-PUR-001')).toBe(false)
  })

  it('shows only the Purchasing KPI set for the Purchasing lens', () => {
    const groups = categories([
      salesOutKpi,
      purchaseInKpi,
      mtdPurchaseKpi,
      inventoryValueKpi,
      targetKpi,
    ])
    const purchasingSet = ['PRN-PUR-001', 'PU-KPI-001', 'PRN-INV-001', 'PRN-INV-002']

    const selected = selectLensScopedKpis(groups, purchasingSet)

    expect(selected.map((k) => k.KpiId)).toEqual(['PRN-PUR-001', 'PU-KPI-001', 'PRN-INV-001'])
    expect(selected.some((k) => k.KpiId === 'PRN-SALES-001')).toBe(false)
  })

  it('preserves the original category-group order when flattening', () => {
    const groups = [
      { Category: 'Financial', Kpis: [kpi('PRN-SALES-001')] },
      { Category: 'Portfolio', Kpis: [kpi('PRN-INV-001', 'Portfolio')] },
    ]

    expect(selectLensScopedKpis(groups, ['PRN-INV-001', 'PRN-SALES-001']).map((k) => k.KpiId))
      .toEqual(['PRN-SALES-001', 'PRN-INV-001'])
  })
})