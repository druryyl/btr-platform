export const SIGNATURE_DIMENSIONS = [
  'Performance',
  'Growth',
  'Quality',
  'Stability',
  'Reach',
  'Risk',
] as const

export const ENTITY_SERIES_COLORS = [
  { border: 'rgb(59, 130, 246)', fill: 'rgba(59, 130, 246, 0.04)' },
  { border: 'rgb(249, 115, 22)', fill: 'rgba(249, 115, 22, 0.04)' },
  { border: 'rgb(168, 85, 247)', fill: 'rgba(168, 85, 247, 0.04)' },
  { border: 'rgb(20, 184, 166)', fill: 'rgba(20, 184, 166, 0.04)' },
  { border: 'rgb(234, 179, 8)', fill: 'rgba(234, 179, 8, 0.04)' },
] as const

export const PEER_AVERAGE_LABEL = 'Peer Average'

export const PEER_AVERAGE_STYLE = {
  border: 'rgb(156, 163, 175)',
  fill: 'rgba(156, 163, 175, 0.02)',
  borderDash: [6, 4] as number[],
}

export function resolveSeriesColor(index: number) {
  return ENTITY_SERIES_COLORS[index % ENTITY_SERIES_COLORS.length]
}

export function formatSignatureScore(score: number | null | undefined): string {
  if (score == null || Number.isNaN(score)) return '—'
  return String(Math.round(score))
}
