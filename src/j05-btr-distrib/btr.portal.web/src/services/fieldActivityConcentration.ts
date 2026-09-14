import type { FieldActivitySalesmanOverviewRow } from '@/models/fieldActivity'

export const CONCENTRATION_TOP_LIMIT = 3
export const CONCENTRATION_OTHERS_KEY = 'others'
export const CONCENTRATION_OTHERS_LABEL = 'Others'

export interface ConcentrationSegment {
  key: string
  label: string
  code: string | null
  amount: number
  percent: number
}

export interface RevenueConcentration {
  total: number
  top1Share: number | null
  top3Share: number | null
  segments: ConcentrationSegment[]
  isEmpty: boolean
}

function roundToOneDecimal(value: number): number {
  return Math.round(value * 10) / 10
}

function sharePercent(amount: number, total: number): number | null {
  if (total === 0) return null
  const percent = (amount / total) * 100
  return Number.isFinite(percent) ? roundToOneDecimal(percent) : null
}

function compareSalesmen(
  a: FieldActivitySalesmanOverviewRow,
  b: FieldActivitySalesmanOverviewRow,
): number {
  if (a.OmzetAmount !== b.OmzetAmount) return b.OmzetAmount - a.OmzetAmount
  if (a.SalesPersonCode === b.SalesPersonCode) return 0
  return a.SalesPersonCode < b.SalesPersonCode ? -1 : 1
}

function toSegment(
  row: FieldActivitySalesmanOverviewRow,
  total: number,
): ConcentrationSegment {
  return {
    key: row.SalesPersonId,
    label: row.SalesPersonName,
    code: row.SalesPersonCode,
    amount: row.OmzetAmount,
    percent: sharePercent(row.OmzetAmount, total) ?? 0,
  }
}

export function buildRevenueConcentration(
  salesmen: ReadonlyArray<FieldActivitySalesmanOverviewRow>,
): RevenueConcentration {
  const total = salesmen.reduce((sum, row) => sum + row.OmzetAmount, 0)

  if (salesmen.length === 0 || total === 0) {
    return {
      total,
      top1Share: null,
      top3Share: null,
      segments: [],
      isEmpty: true,
    }
  }

  const ordered = [...salesmen].sort(compareSalesmen)
  const top = ordered.slice(0, CONCENTRATION_TOP_LIMIT)
  const topAmount = top.reduce((sum, row) => sum + row.OmzetAmount, 0)

  const segments = top
    .filter((row) => row.OmzetAmount > 0)
    .map((row) => toSegment(row, total))

  const othersAmount = total - topAmount
  if (othersAmount > 0) {
    segments.push({
      key: CONCENTRATION_OTHERS_KEY,
      label: CONCENTRATION_OTHERS_LABEL,
      code: null,
      amount: othersAmount,
      percent: sharePercent(othersAmount, total) ?? 0,
    })
  }

  return {
    total,
    top1Share: sharePercent(ordered[0].OmzetAmount, total),
    top3Share: sharePercent(topAmount, total),
    segments,
    isEmpty: false,
  }
}
