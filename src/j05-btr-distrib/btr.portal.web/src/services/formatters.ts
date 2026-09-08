export function formatCurrency(value: number): string {
  return new Intl.NumberFormat('id-ID', {
    style: 'currency',
    currency: 'IDR',
    minimumFractionDigits: 0,
    maximumFractionDigits: 0,
  }).format(value)
}

const RUPIAH_JUTA = 1_000_000
const RUPIAH_MILIAR = 1_000_000_000
const RUPIAH_TRILIUN = 1_000_000_000_000

const compactFractionMantissa = new Intl.NumberFormat('id-ID', {
  minimumFractionDigits: 0,
  maximumFractionDigits: 2,
})

/**
 * Compact IDR for dashboard KPI cards.
 *
 * - Below 1 juta: full id-ID number (detail matters; short values don't wrap).
 * - 1 juta .. < 1 miliar: "J" (juta), up to 2 fraction digits with trailing
 *   zeros trimmed (e.g. 50J, 1,25J, 512J).
 * - 1 miliar .. < 1 triliun: "M" (miliar), up to 2 fraction digits with trailing
 *   zeros trimmed (e.g. 7,45M, 50M, 2,5M).
 * - 1 triliun+: "T" (triliun), up to 2 fraction digits (e.g. 2T, 1,23T).
 *
 * A value just under 1 triliun (999.999.999.999) renders as 1.000M; this is an
 * accepted boundary and not realistic for this domain.
 */
export function formatCurrencyCompact(value: number): string {
  const abs = Math.abs(value)

  if (abs >= RUPIAH_TRILIUN) {
    const mantissa = value / RUPIAH_TRILIUN
    return `${compactFractionMantissa.format(mantissa)}T`
  }

  if (abs >= RUPIAH_MILIAR) {
    const mantissa = value / RUPIAH_MILIAR
    return `${compactFractionMantissa.format(mantissa)}M`
  }

  if (abs >= RUPIAH_JUTA) {
    const mantissa = value / RUPIAH_JUTA
    return `${compactFractionMantissa.format(mantissa)}J`
  }

  return formatNumber(value)
}

/**
 * Splits a compact currency string into its numeric mantissa and scale suffix
 * (J/M/T). Rendering layers use this to display the suffix as a superscript.
 *
 * Anything that does not end in a scaled suffix (full numbers, percents,
 * counts, matches, em dashes) returns the input untouched with a null suffix.
 */
export function splitCompactSuffix(value: string): {
  mantissa: string
  suffix: string | null
} {
  const match = /^(.+)([JMT])$/.exec(value)
  if (match) {
    return { mantissa: match[1], suffix: match[2] }
  }
  return { mantissa: value, suffix: null }
}

export function formatNumber(value: number): string {
  return new Intl.NumberFormat('id-ID').format(value)
}

export function formatDateTime(value: string): string {
  return new Intl.DateTimeFormat('id-ID', {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(value))
}

export function formatDate(value: string): string {
  return new Intl.DateTimeFormat('id-ID', {
    dateStyle: 'medium',
  }).format(new Date(value))
}

export function formatPercent(value: number | null | undefined): string {
  if (value == null) return '—'
  return `${value.toFixed(1)}%`
}
