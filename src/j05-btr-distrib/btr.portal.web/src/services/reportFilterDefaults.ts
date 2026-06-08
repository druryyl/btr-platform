export const REPORT_MAX_DAYS = 31

export type PiutangDateField = 'DueDate' | 'PiutangDate'

export interface ReportDateRange {
  from: string
  to: string
}

export function formatDateParam(date: Date): string {
  const year = date.getFullYear()
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const day = String(date.getDate()).padStart(2, '0')
  return `${year}-${month}-${day}`
}

export function parseDateParam(value: string): Date {
  const [year, month, day] = value.split('-').map(Number)
  return new Date(year, month - 1, day)
}

export function currentMonthRange(referenceDate = new Date()): ReportDateRange {
  const year = referenceDate.getFullYear()
  const month = referenceDate.getMonth()
  const from = new Date(year, month, 1)
  const to = new Date(year, month + 1, 0)
  return {
    from: formatDateParam(from),
    to: formatDateParam(to),
  }
}

export function validateDateRange(from: Date, to: Date): string | null {
  if (from > to) {
    return "Period 'from' must not be after 'to'."
  }

  const dayCount = Math.floor((to.getTime() - from.getTime()) / 86_400_000) + 1
  if (dayCount > REPORT_MAX_DAYS) {
    return `Report period must not exceed ${REPORT_MAX_DAYS} days.`
  }

  return null
}

export function piutangDateFieldLabel(field: PiutangDateField): string {
  return field === 'DueDate' ? 'Jatuh Tempo' : 'Piutang Date'
}
