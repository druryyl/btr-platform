import type { Router } from 'vue-router'

export const PRINCIPAL_PERFORMANCE_ROUTE = '/dashboard/principal-performance'

export function openPrincipalPerformance(
  router: Router,
  supplierId?: string | null,
): void {
  const id = (supplierId ?? '').trim()
  if (!id) {
    return
  }

  void router.push({
    path: PRINCIPAL_PERFORMANCE_ROUTE,
    query: { supplierId: id },
  })
}