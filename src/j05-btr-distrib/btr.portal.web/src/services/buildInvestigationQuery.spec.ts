import { describe, expect, it } from 'vitest'
import type { InvestigationMetadata } from '@/models/investigation'
import { buildInvestigationQuery } from '@/services/buildInvestigationQuery'

describe('buildInvestigationQuery', () => {
  it('maps piutang customer drill-down with all-open balances and breadcrumb fields', () => {
    const investigation: InvestigationMetadata = {
      SignalKey: 'ChronicOverdue',
      SignalLabel: 'Chronic Overdue',
      EntityType: 'Customer',
      EntityId: 'C001',
      EntityName: 'PT ABC',
      ReportRoute: '/reports/piutang',
      SuggestedQuery: {
        FreeText: 'PT ABC',
        CustomerId: 'C001',
        PeriodMode: 'allOpenBalances',
      },
    }

    const query = buildInvestigationQuery(investigation, 'Collection Dashboard')

    expect(query).toEqual({
      q: 'PT ABC',
      customerId: 'C001',
      periodMode: 'allOpenBalances',
      signalKey: 'ChronicOverdue',
      signalLabel: 'Chronic Overdue',
      source: 'Collection Dashboard',
      entityType: 'Customer',
    })
  })

  it('maps qualified backlog with posting=BELUM and principal name', () => {
    const investigation: InvestigationMetadata = {
      SignalKey: 'QualifiedBacklog',
      SignalLabel: 'Qualified Backlog',
      EntityType: 'Principal',
      EntityId: 'P9',
      EntityName: 'Principal XYZ',
      ReportRoute: '/reports/purchasing',
      SuggestedQuery: {
        FreeText: 'Principal XYZ',
        SupplierId: 'P9',
        PostingFilter: 'BELUM',
      },
    }

    const query = buildInvestigationQuery(investigation, 'Alert Center')

    expect(query).toEqual({
      q: 'Principal XYZ',
      supplierId: 'P9',
      posting: 'BELUM',
      signalKey: 'QualifiedBacklog',
      signalLabel: 'Qualified Backlog',
      source: 'Alert Center',
      entityType: 'Principal',
    })
  })

  it('maps minimal sales drill-down with q only when no entity id', () => {
    const investigation: InvestigationMetadata = {
      SignalKey: 'LegacyTopSalesman',
      SignalLabel: 'Top Salesman',
      EntityType: 'Salesman',
      EntityId: '',
      EntityName: 'Budi Santoso',
      ReportRoute: '/reports/sales',
      SuggestedQuery: {
        FreeText: 'Budi Santoso',
      },
    }

    const query = buildInvestigationQuery(investigation, 'Sales Dashboard')

    expect(query).toEqual({
      q: 'Budi Santoso',
      signalKey: 'LegacyTopSalesman',
      signalLabel: 'Top Salesman',
      source: 'Sales Dashboard',
      entityType: 'Salesman',
    })
  })

  it('maps customer report drill-down with customerCode query param', () => {
    const investigation: InvestigationMetadata = {
      SignalKey: 'PortfolioCollect',
      SignalLabel: 'Collect',
      EntityType: 'Customer',
      EntityId: 'C001',
      EntityName: 'PT ABC',
      ReportRoute: '/reports/customers',
      SuggestedQuery: {
        CustomerCode: 'C001',
        FreeText: 'PT ABC',
      },
    }

    const query = buildInvestigationQuery(investigation, '/dashboard/customer-portfolio')

    expect(query).toEqual({
      q: 'PT ABC',
      customerCode: 'C001',
      signalKey: 'PortfolioCollect',
      signalLabel: 'Collect',
      source: '/dashboard/customer-portfolio',
      entityType: 'Customer',
    })
  })
})
