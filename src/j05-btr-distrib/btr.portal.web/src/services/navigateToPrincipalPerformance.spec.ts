import { describe, expect, it, vi } from 'vitest'
import type { Router } from 'vue-router'
import {
  openPrincipalPerformance,
  PRINCIPAL_PERFORMANCE_ROUTE,
} from '@/services/navigateToPrincipalPerformance'

describe('navigateToPrincipalPerformance', () => {
  it('navigates to principal performance with trimmed supplierId', () => {
    const router = { push: vi.fn() } as unknown as Router

    openPrincipalPerformance(router, '  S-01  ')

    expect(router.push).toHaveBeenCalledWith({
      path: PRINCIPAL_PERFORMANCE_ROUTE,
      query: { supplierId: 'S-01' },
    })
  })

  it('does not navigate when supplierId is blank', () => {
    const router = { push: vi.fn() } as unknown as Router

    openPrincipalPerformance(router, '  ')

    expect(router.push).not.toHaveBeenCalled()
  })

  it('does not navigate when supplierId is missing', () => {
    const router = { push: vi.fn() } as unknown as Router

    openPrincipalPerformance(router, undefined)

    expect(router.push).not.toHaveBeenCalled()
  })
})