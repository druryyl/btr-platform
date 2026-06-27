export type FieldActivityKpiBand = 'good' | 'warn' | 'bad' | 'neutral'

export function executionBand(value: number | null | undefined): FieldActivityKpiBand {
  if (value == null) return 'neutral'
  if (value >= 80) return 'good'
  if (value >= 50) return 'warn'
  return 'bad'
}

export function effectiveCallBand(value: number | null | undefined): FieldActivityKpiBand {
  if (value == null) return 'neutral'
  if (value >= 50) return 'good'
  if (value >= 30) return 'warn'
  return 'bad'
}

export function gpsValidBand(value: number | null | undefined): FieldActivityKpiBand {
  if (value == null) return 'neutral'
  if (value >= 85) return 'good'
  if (value >= 70) return 'warn'
  return 'bad'
}

export function bandClass(band: FieldActivityKpiBand): string {
  return `field-activity-kpi-band field-activity-kpi-band--${band}`
}

export function statusSeverity(statusCode: string): 'success' | 'warn' | 'danger' | 'secondary' | 'info' {
  switch (statusCode) {
    case 'OnTrack':
      return 'success'
    case 'NeedsAttention':
      return 'warn'
    case 'Critical':
      return 'danger'
    case 'NoPlan':
      return 'info'
    default:
      return 'secondary'
  }
}

export function statusLabel(statusCode: string): string {
  switch (statusCode) {
    case 'OnTrack':
      return 'On Track'
    case 'NeedsAttention':
      return 'Needs Attention'
    case 'Critical':
      return 'Critical'
    case 'NoPlan':
      return 'No Plan'
    case 'NoFieldData':
      return 'No Field Data'
    default:
      return statusCode
  }
}
