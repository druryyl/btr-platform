import { computed, type ComputedRef, type Ref } from 'vue'
import { filterRowsByFreeText } from '@/services/reportFreeTextFilter'

export function useReportFreeTextFilter<T extends object>(
  rows: Ref<T[]> | ComputedRef<T[]>,
  fields: (keyof T & string)[],
  freeText: Ref<string>,
) {
  const filteredRows = computed(() =>
    filterRowsByFreeText(
      rows.value as Array<T & Record<string, unknown>>,
      freeText.value,
      fields,
    ) as T[],
  )

  const hasFreeTextFilter = computed(() => freeText.value.trim().length > 0)

  return {
    filteredRows,
    hasFreeTextFilter,
  }
}
