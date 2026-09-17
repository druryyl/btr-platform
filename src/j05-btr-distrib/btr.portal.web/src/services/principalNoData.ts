export const NO_DATA_LABEL = 'No Data' as const

export function isMissingValue(value: unknown): value is null | undefined {
  return value == null
}

export function formatWithNoData<T>(
  value: T | null | undefined,
  format: (value: T) => string,
): string {
  if (value == null) {
    return NO_DATA_LABEL
  }
  return format(value)
}

export function orNoData(value: string | null | undefined): string {
  return value ?? NO_DATA_LABEL
}
