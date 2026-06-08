import type { Router } from 'vue-router'

export function navigateToReport(
  router: Router,
  route: string,
  customerName: string,
): void {
  void router.push({
    path: route,
    query: { q: customerName },
  })
}
