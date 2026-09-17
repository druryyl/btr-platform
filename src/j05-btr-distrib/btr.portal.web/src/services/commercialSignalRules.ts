import { percentileRank } from './principalPercentile'

export const REVENUE_HIGH_PERCENTILE_MIN = 75
export const OVERDUE_HIGH_PERCENTILE_MIN = 75
export const OVERDUE_HEALTHY_PERCENTILE_MAX = 25
export const EFFECTIVE_CALL_RATE_HIGH_PERCENTILE_MIN = 75

export const COMMERCIAL_SIGNAL_KEYS = [
  'HighRevenue',
  'HighOverdue',
  'HealthyPortfolio',
  'NeedsCreditReview',
  'NeedsCollectionAction',
  'RecognitionCandidate',
] as const

export type CommercialSignalKey = (typeof COMMERCIAL_SIGNAL_KEYS)[number]

export const COMMERCIAL_SIGNAL_LABELS: Record<CommercialSignalKey, string> = {
  HighRevenue: 'High Revenue',
  HighOverdue: 'High Overdue',
  HealthyPortfolio: 'Healthy Portfolio',
  NeedsCreditReview: 'Needs Credit Review',
  NeedsCollectionAction: 'Needs Collection Action',
  RecognitionCandidate: 'Recognition Candidate',
}

export const COMMERCIAL_SIGNAL_RATIONALES: Record<CommercialSignalKey, string> = {
  HighRevenue: 'Revenue is in the top 25% of the active salesman population.',
  HighOverdue: 'Overdue exposure is in the top 25% of salesmen with overdue data.',
  HealthyPortfolio: 'Overdue exposure is in the bottom 25% of salesmen with overdue data.',
  NeedsCreditReview: 'High revenue combined with high overdue exposure.',
  NeedsCollectionAction: 'Overdue exposure is in the top 25% of salesmen with overdue data.',
  RecognitionCandidate: 'High revenue, high effective call rate, and a healthy portfolio.',
}

export interface CommercialSignalRow {
  SalesPersonId: string
  Revenue: number | null
  EffectiveCallRate: number | null
  OverdueExposure: number | null
}

export interface CommercialSignalClassification {
  key: CommercialSignalKey
  label: string
  rationale: string
}

function revenueValues(rows: ReadonlyArray<CommercialSignalRow>): Array<number | null> {
  return rows.map((row) => row.Revenue)
}

function overdueValues(rows: ReadonlyArray<CommercialSignalRow>): Array<number | null> {
  return rows.map((row) => row.OverdueExposure)
}

function effectiveCallRateValues(rows: ReadonlyArray<CommercialSignalRow>): Array<number | null> {
  return rows.map((row) => row.EffectiveCallRate)
}

function atOrAbovePercentile(
  value: number | null,
  values: ReadonlyArray<number | null | undefined>,
  threshold: number,
): boolean {
  const { percentile } = percentileRank(values, value)
  return percentile != null && percentile >= threshold
}

function atOrBelowPercentile(
  value: number | null,
  values: ReadonlyArray<number | null | undefined>,
  threshold: number,
): boolean {
  const { percentile } = percentileRank(values, value)
  return percentile != null && percentile <= threshold
}

export function isHighRevenue(
  row: CommercialSignalRow,
  rows: ReadonlyArray<CommercialSignalRow>,
): boolean {
  return atOrAbovePercentile(row.Revenue, revenueValues(rows), REVENUE_HIGH_PERCENTILE_MIN)
}

export function isHighOverdue(
  row: CommercialSignalRow,
  rows: ReadonlyArray<CommercialSignalRow>,
): boolean {
  return atOrAbovePercentile(row.OverdueExposure, overdueValues(rows), OVERDUE_HIGH_PERCENTILE_MIN)
}

export function isHealthyPortfolio(
  row: CommercialSignalRow,
  rows: ReadonlyArray<CommercialSignalRow>,
): boolean {
  return atOrBelowPercentile(
    row.OverdueExposure,
    overdueValues(rows),
    OVERDUE_HEALTHY_PERCENTILE_MAX,
  )
}

function isHighEffectiveCallRate(
  row: CommercialSignalRow,
  rows: ReadonlyArray<CommercialSignalRow>,
): boolean {
  return atOrAbovePercentile(
    row.EffectiveCallRate,
    effectiveCallRateValues(rows),
    EFFECTIVE_CALL_RATE_HIGH_PERCENTILE_MIN,
  )
}

export function needsCreditReview(
  row: CommercialSignalRow,
  rows: ReadonlyArray<CommercialSignalRow>,
): boolean {
  return isHighRevenue(row, rows) && isHighOverdue(row, rows)
}

export function needsCollectionAction(
  row: CommercialSignalRow,
  rows: ReadonlyArray<CommercialSignalRow>,
): boolean {
  return isHighOverdue(row, rows)
}

export function isRecognitionCandidate(
  row: CommercialSignalRow,
  rows: ReadonlyArray<CommercialSignalRow>,
): boolean {
  return (
    isHighRevenue(row, rows) &&
    isHighEffectiveCallRate(row, rows) &&
    isHealthyPortfolio(row, rows)
  )
}

export function recognitionCandidates(
  rows: ReadonlyArray<CommercialSignalRow>,
): CommercialSignalRow[] {
  return rows.filter((row) => isRecognitionCandidate(row, rows))
}

export function classifyCommercialSignals(
  row: CommercialSignalRow,
  rows: ReadonlyArray<CommercialSignalRow>,
): CommercialSignalClassification[] {
  const classifications: CommercialSignalClassification[] = []
  const add = (key: CommercialSignalKey): void => {
    classifications.push({
      key,
      label: COMMERCIAL_SIGNAL_LABELS[key],
      rationale: COMMERCIAL_SIGNAL_RATIONALES[key],
    })
  }

  const highRevenue = isHighRevenue(row, rows)
  const highOverdue = isHighOverdue(row, rows)

  if (highRevenue) add('HighRevenue')
  if (highOverdue) add('HighOverdue')
  if (isHealthyPortfolio(row, rows)) add('HealthyPortfolio')
  if (highRevenue && highOverdue) add('NeedsCreditReview')
  if (highOverdue) add('NeedsCollectionAction')
  if (isRecognitionCandidate(row, rows)) add('RecognitionCandidate')

  return classifications
}
