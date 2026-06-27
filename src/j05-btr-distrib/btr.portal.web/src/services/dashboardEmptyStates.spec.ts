import { describe, expect, it } from 'vitest'
import {
  formatDashboardCurrency,
  formatDashboardEmpty,
  formatDashboardPercent,
} from '@/services/dashboardEmptyStates'

describe('dashboardEmptyStates', () => {
  it('returns semantic empty labels', () => {
    expect(formatDashboardEmpty('no-target')).toBe('No Target')
    expect(formatDashboardEmpty('no-data')).toBe('No Data')
    expect(formatDashboardEmpty('not-available')).toBe('Not Available')
    expect(formatDashboardEmpty('unknown')).toBe('Unknown')
  })

  it('formats percent or empty label', () => {
    expect(formatDashboardPercent(82.4)).toBe('82.4%')
    expect(formatDashboardPercent(null, 'no-target')).toBe('No Target')
  })

  it('formats currency or empty label', () => {
    const idr = (value: number) => `Rp${value}`
    expect(formatDashboardCurrency(1000, idr)).toBe('Rp1000')
    expect(formatDashboardCurrency(null, idr, 'no-data')).toBe('No Data')
  })
})
