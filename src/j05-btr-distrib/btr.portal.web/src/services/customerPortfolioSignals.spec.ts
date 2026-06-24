import { describe, expect, it } from 'vitest'
import type { DashboardCustomerPortfolioCustomerRow } from '@/models/dashboard'
import {
  CUSTOMER_PORTFOLIO_VIEW_ATTENTION,
  actionBadgeSeverity,
  buildCustomerReportLink,
  buildM30CollectLink,
  collectDistinctFilterValues,
  filterPortfolioCustomers,
  groupPriorityRowsByAction,
} from '@/services/customerPortfolioSignals'

function customerRow(
  overrides: Partial<DashboardCustomerPortfolioCustomerRow> = {},
): DashboardCustomerPortfolioCustomerRow {
  return {
    SortOrder: 1,
    CustomerKey: 'K1',
    CustomerCode: 'C001',
    CustomerName: 'PT ABC',
    WilayahName: 'Jakarta',
    Klasifikasi: 'A',
    LifecycleStage: 'Mature',
    LifecycleLabel: 'Mature',
    PortfolioTier: 'Strategic',
    TierLabel: 'Strategic',
    PrimaryActionKey: 'Collect',
    PrimaryActionLabel: 'Collect',
    ActionOwner: 'Finance',
    ActionReasonText: 'Overdue balance',
    TriggeredRuleIds: 'M31-001',
    MtdOmzet: 1_000_000,
    OpenBalance: 500_000,
    OverdueBalance: 100_000,
    FakturCount6Mo: 4,
    IsActiveMtd: true,
    LastPurchaseDate: '2026-06-01',
    FirstPurchaseDate: '2024-01-01',
    M29Category: 'Attention',
    M29PrimarySignalKey: 'PaymentDelay',
    SalesPersonName: 'Budi',
    SalesmanAchievementPercent: 80,
    SalesmanHighPiutangExposure: false,
    IsAttention: true,
    PortfolioPriorityScore: 90,
    M30LinkRoute: '/dashboard/collection-optimization?customerKey=K1',
    CustomerReportRoute: '/reports/customers?customerCode=C001',
    DrillDownRouteM17: '/dashboard/customers',
    DrillDownRouteM29: '/dashboard/customer-risk-forecast',
    ValueDisclaimer: 'Customer Value = Omzet Proxy, NOT profitability.',
    ...overrides,
  }
}

describe('actionBadgeSeverity', () => {
  it('maps portfolio actions to badge severities', () => {
    expect(actionBadgeSeverity('Collect')).toBe('danger')
    expect(actionBadgeSeverity('Grow')).toBe('success')
    expect(actionBadgeSeverity('Monitor')).toBe('secondary')
  })
})

describe('buildM30CollectLink', () => {
  it('builds collection optimization link with customer key', () => {
    expect(buildM30CollectLink('K1')).toBe('/dashboard/collection-optimization?customerKey=K1')
  })

  it('falls back to dashboard route without customer key', () => {
    expect(buildM30CollectLink('')).toBe('/dashboard/collection-optimization')
  })
})

describe('buildCustomerReportLink', () => {
  it('builds customer report link with customer code', () => {
    expect(buildCustomerReportLink('C001')).toBe('/reports/customers?customerCode=C001')
  })
})

describe('filterPortfolioCustomers', () => {
  const rows = [
    customerRow({ CustomerCode: 'C001', IsAttention: true, PrimaryActionKey: 'Collect' }),
    customerRow({
      CustomerCode: 'C002',
      IsAttention: false,
      PrimaryActionKey: 'Monitor',
      WilayahName: 'Bandung',
    }),
  ]

  it('defaults to attention-only view', () => {
    const filtered = filterPortfolioCustomers(rows, {
      view: CUSTOMER_PORTFOLIO_VIEW_ATTENTION,
      wilayah: '',
      klasifikasi: '',
      tier: '',
      lifecycle: '',
      action: '',
      salesman: '',
    })

    expect(filtered).toHaveLength(1)
    expect(filtered[0]?.CustomerCode).toBe('C001')
  })

  it('filters by wilayah and action', () => {
    const filtered = filterPortfolioCustomers(rows, {
      view: 'all',
      wilayah: 'Bandung',
      klasifikasi: '',
      tier: '',
      lifecycle: '',
      action: 'Monitor',
      salesman: '',
    })

    expect(filtered).toHaveLength(1)
    expect(filtered[0]?.CustomerCode).toBe('C002')
  })
})

describe('groupPriorityRowsByAction', () => {
  it('groups rows by primary action key', () => {
    const grouped = groupPriorityRowsByAction([
      {
        SortOrder: 1,
        PortfolioPriorityScore: 10,
        CustomerKey: 'K1',
        CustomerCode: 'C001',
        CustomerName: 'A',
        WilayahName: '',
        Klasifikasi: '',
        LifecycleStage: 'Mature',
        LifecycleLabel: 'Mature',
        PortfolioTier: 'Strategic',
        TierLabel: 'Strategic',
        PrimaryActionKey: 'Collect',
        PrimaryActionLabel: 'Collect',
        ActionOwner: 'Finance',
        ActionReasonText: '',
        TriggeredRuleIds: '',
        MtdOmzet: 0,
        OpenBalance: 0,
        OverdueBalance: null,
        M29Category: 'Attention',
        SalesPersonName: '',
        SalesmanAchievementPercent: null,
        SalesmanHighPiutangExposure: false,
        IsAttention: true,
        M30LinkRoute: '',
        CustomerReportRoute: '',
        DrillDownRouteM17: '',
        DrillDownRouteM29: '',
      },
      {
        SortOrder: 2,
        PortfolioPriorityScore: 5,
        CustomerKey: 'K2',
        CustomerCode: 'C002',
        CustomerName: 'B',
        WilayahName: '',
        Klasifikasi: '',
        LifecycleStage: 'Growing',
        LifecycleLabel: 'Growing',
        PortfolioTier: 'HighValue',
        TierLabel: 'High Value',
        PrimaryActionKey: 'Grow',
        PrimaryActionLabel: 'Grow',
        ActionOwner: 'Sales',
        ActionReasonText: '',
        TriggeredRuleIds: '',
        MtdOmzet: 0,
        OpenBalance: 0,
        OverdueBalance: null,
        M29Category: 'Healthy',
        SalesPersonName: '',
        SalesmanAchievementPercent: null,
        SalesmanHighPiutangExposure: false,
        IsAttention: true,
        M30LinkRoute: '',
        CustomerReportRoute: '',
        DrillDownRouteM17: '',
        DrillDownRouteM29: '',
      },
    ])

    expect(grouped.Collect).toHaveLength(1)
    expect(grouped.Grow).toHaveLength(1)
  })
})

describe('collectDistinctFilterValues', () => {
  it('collects sorted distinct filter values', () => {
    const values = collectDistinctFilterValues([
      customerRow({ WilayahName: 'Jakarta', PrimaryActionKey: 'Collect' }),
      customerRow({ WilayahName: 'Bandung', PrimaryActionKey: 'Monitor', SalesPersonName: 'Ani' }),
    ])

    expect(values.wilayah).toEqual(['Bandung', 'Jakarta'])
    expect(values.action).toEqual(['Collect', 'Monitor'])
    expect(values.salesman).toEqual(['Ani', 'Budi'])
  })
})
