import { computed, type ComputedRef, type Ref } from 'vue'
import { filterRowsByFreeText } from '@/services/reportFreeTextFilter'

export interface ReportInvestigationIdFilter {
  customerId?: Ref<string>
  salesmanId?: Ref<string>
  brgId?: Ref<string>
  warehouseId?: Ref<string>
  supplierId?: Ref<string>
}

function matchesId(row: Record<string, unknown>, field: string, id: string): boolean {
  const value = row[field]
  if (value == null) return false
  return String(value).trim().toLowerCase() === id.trim().toLowerCase()
}

function matchesWarehouse(row: Record<string, unknown>, warehouseId: string): boolean {
  if (matchesId(row, 'WarehouseId', warehouseId)) return true
  return matchesId(row, 'WarehouseName', warehouseId)
}

function matchesSupplier(row: Record<string, unknown>, supplierId: string): boolean {
  if (matchesId(row, 'SupplierId', supplierId)) return true
  return matchesId(row, 'SupplierName', supplierId)
}

export function useReportInvestigationFilter<T extends object>(
  rows: Ref<T[]> | ComputedRef<T[]>,
  fields: (keyof T & string)[],
  freeText: Ref<string>,
  idFilter: ReportInvestigationIdFilter = {},
) {
  const idFilteredRows = computed(() => {
    const source = rows.value as Array<T & Record<string, unknown>>

    if (idFilter.customerId?.value.trim()) {
      const id = idFilter.customerId.value.trim()
      return source.filter(
        (row) => matchesId(row, 'CustomerCode', id) || matchesId(row, 'CustomerName', id),
      )
    }

    if (idFilter.salesmanId?.value.trim()) {
      const id = idFilter.salesmanId.value.trim()
      return source.filter(
        (row) => matchesId(row, 'SalesPersonId', id) || matchesId(row, 'SalesName', id),
      )
    }

    if (idFilter.brgId?.value.trim()) {
      const id = idFilter.brgId.value.trim()
      return source.filter((row) => matchesId(row, 'BrgId', id))
    }

    if (idFilter.warehouseId?.value.trim()) {
      const id = idFilter.warehouseId.value.trim()
      return source.filter((row) => matchesWarehouse(row, id))
    }

    if (idFilter.supplierId?.value.trim()) {
      const id = idFilter.supplierId.value.trim()
      return source.filter((row) => matchesSupplier(row, id))
    }

    return source
  })

  const filteredRows = computed(() =>
    filterRowsByFreeText(
      idFilteredRows.value as Array<T & Record<string, unknown>>,
      freeText.value,
      fields,
    ) as T[],
  )

  const hasIdFilter = computed(() =>
  Boolean(
    idFilter.customerId?.value.trim()
      || idFilter.salesmanId?.value.trim()
      || idFilter.brgId?.value.trim()
      || idFilter.warehouseId?.value.trim()
      || idFilter.supplierId?.value.trim(),
  ))

  const hasFreeTextFilter = computed(() => freeText.value.trim().length > 0)
  const hasActiveFilter = computed(() => hasIdFilter.value || hasFreeTextFilter.value)

  return {
    filteredRows,
    hasIdFilter,
    hasFreeTextFilter,
    hasActiveFilter,
  }
}
