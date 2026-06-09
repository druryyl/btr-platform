import { ref } from 'vue'
import type { RouteLocationNormalizedLoaded } from 'vue-router'
import type { InvestigationBreadcrumbContext } from '@/models/investigation'
import { applyInvestigationQuery } from '@/services/applyInvestigationQuery'

export function useReportInvestigationHydration() {
  const breadcrumb = ref<InvestigationBreadcrumbContext>({})
  const customerId = ref('')
  const salesmanId = ref('')
  const brgId = ref('')
  const warehouseId = ref('')
  const supplierId = ref('')
  const postingFilter = ref('')

  function hydrateFromRoute(route: RouteLocationNormalizedLoaded): {
    freeText: string
    allOpenBalances: boolean
  } {
    const applied = applyInvestigationQuery(route)

    breadcrumb.value = applied.breadcrumb
    customerId.value = applied.customerId
    salesmanId.value = applied.salesmanId
    brgId.value = applied.brgId
    warehouseId.value = applied.warehouseId
    supplierId.value = applied.supplierId
    postingFilter.value = applied.postingFilter

    return {
      freeText: applied.freeText,
      allOpenBalances: applied.allOpenBalances,
    }
  }

  return {
    breadcrumb,
    customerId,
    salesmanId,
    brgId,
    warehouseId,
    supplierId,
    postingFilter,
    hydrateFromRoute,
  }
}
