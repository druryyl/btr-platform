import { describe, expect, it } from 'vitest'
import {
  DATA_HEALTH_SNAPSHOT_DISCLOSURE,
  UNKNOWN_PRINCIPAL_EVIDENCE_ROUTE,
  formatSnapshotPeriod,
} from '@/services/workspaceDataHealth'

describe('formatSnapshotPeriod', () => {
  it('returns null when the period is missing', () => {
    expect(formatSnapshotPeriod(null, null)).toBeNull()
    expect(formatSnapshotPeriod(undefined, undefined)).toBeNull()
    expect(formatSnapshotPeriod(2026, undefined)).toBeNull()
    expect(formatSnapshotPeriod(undefined, 6)).toBeNull()
    expect(formatSnapshotPeriod(null, 6)).toBeNull()
    expect(formatSnapshotPeriod(2026, null)).toBeNull()
  })

  it('returns null for out-of-range months', () => {
    expect(formatSnapshotPeriod(2026, 0)).toBeNull()
    expect(formatSnapshotPeriod(2026, 13)).toBeNull()
  })

  it('formats an id-ID long month + year label', () => {
    expect(formatSnapshotPeriod(2026, 6)).toBe('Juni 2026')
    expect(formatSnapshotPeriod(2026, 1)).toBe('Januari 2026')
  })
})

describe('Data Health constants', () => {
  it('carries the standard snapshot disclosure', () => {
    expect(DATA_HEALTH_SNAPSHOT_DISCLOSURE).toContain('latest available analytics snapshot')
  })

  it('points Unknown Principal exceptions to the Sales Report evidence', () => {
    expect(UNKNOWN_PRINCIPAL_EVIDENCE_ROUTE).toBe('/reports/sales')
  })
})