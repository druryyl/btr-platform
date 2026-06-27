import { describe, expect, it } from 'vitest'
import { resolveDashboardDomain } from '@/services/dashboardDomains'

describe('dashboardDomains', () => {
  it('maps API domain names to dashboard domain keys', () => {
    expect(resolveDashboardDomain('Sales')).toBe('sales')
    expect(resolveDashboardDomain('Piutang')).toBe('finance')
    expect(resolveDashboardDomain('Inventory')).toBe('inventory')
    expect(resolveDashboardDomain('Customer Portfolio')).toBe('portfolio')
  })

  it('falls back to alert for unknown domains', () => {
    expect(resolveDashboardDomain('Unknown Domain')).toBe('alert')
  })
})
