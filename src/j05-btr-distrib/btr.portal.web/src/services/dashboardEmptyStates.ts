export type DashboardEmptyKind = 'no-target' | 'no-data' | 'not-available' | 'unknown'

const EMPTY_LABELS: Record<DashboardEmptyKind, string> = {
  'no-target': 'No Target',
  'no-data': 'No Data',
  'not-available': 'Not Available',
  unknown: 'Unknown',
}

export function formatDashboardEmpty(kind: DashboardEmptyKind = 'unknown'): string {
  return EMPTY_LABELS[kind]
}

export function formatDashboardPercent(
  value: number | null | undefined,
  emptyKind: DashboardEmptyKind = 'unknown',
): string {
  if (value == null) return formatDashboardEmpty(emptyKind)
  return `${value.toFixed(1)}%`
}

export function formatDashboardCurrency(
  value: number | null | undefined,
  formatter: (value: number) => string,
  emptyKind: DashboardEmptyKind = 'no-data',
): string {
  if (value == null) return formatDashboardEmpty(emptyKind)
  return formatter(value)
}
