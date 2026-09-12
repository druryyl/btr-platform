/**
 * Standard Entity Analytics freshness disclosure (IW-OQ-002): the workspace
 * reflects the latest available analytics snapshot and may not include
 * transactions entered after the snapshot was generated.
 */
export const DATA_HEALTH_SNAPSHOT_DISCLOSURE =
  'Data shown in this workspace is based on the latest available analytics snapshot ' +
  'and may not reflect transactions entered after the snapshot was generated.'

/** Sale Report surfaces the Unknown Principal exception-lines disclosure (IW-OQ-003). */
export const UNKNOWN_PRINCIPAL_EVIDENCE_ROUTE = '/reports/sales'

/** Formats a snapshot period as an id-ID long month + year label, or null when unknown. */
export function formatSnapshotPeriod(
  periodYear: number | null | undefined,
  periodMonth: number | null | undefined,
): string | null {
  if (!periodYear || !periodMonth || periodMonth < 1 || periodMonth > 12) {
    return null
  }
  return new Intl.DateTimeFormat('id-ID', { month: 'long', year: 'numeric' }).format(
    new Date(periodYear, periodMonth - 1, 1),
  )
}