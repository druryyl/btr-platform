import type { FieldActivityTeamKpis } from '@/models/fieldActivity'

export type FunnelStageKey = 'planned' | 'actual' | 'effective' | 'orders' | 'orderValue'

export type FunnelConversionKind = 'percent' | 'currency'

export interface FunnelConversion {
  label: string
  kind: FunnelConversionKind
  value: number | null
}

export interface FunnelLeak {
  label: string
  value: number
}

export interface FunnelStage {
  key: FunnelStageKey
  label: string
  value: number
  conversion: FunnelConversion | null
  leak: FunnelLeak | null
}

function safeDivide(numerator: number, denominator: number): number | null {
  if (denominator === 0) return null
  const result = numerator / denominator
  return Number.isFinite(result) ? result : null
}

function safePercent(numerator: number, denominator: number): number | null {
  const ratio = safeDivide(numerator, denominator)
  return ratio == null ? null : ratio * 100
}

function guardedApiPercent(value: number | null, denominator: number): number | null {
  if (denominator === 0) return null
  return value
}

export function buildFunnelStages(kpis: FieldActivityTeamKpis | null): FunnelStage[] {
  if (!kpis) return []

  return [
    {
      key: 'planned',
      label: 'Planned Visits',
      value: kpis.PlannedVisits,
      conversion: null,
      leak: null,
    },
    {
      key: 'actual',
      label: 'Actual Visits',
      value: kpis.ActualVisits,
      conversion: {
        label: 'Execution Rate',
        kind: 'percent',
        value: guardedApiPercent(kpis.VisitExecutionPercent, kpis.PlannedVisits),
      },
      leak: { label: 'Missed Visits', value: kpis.MissedVisits },
    },
    {
      key: 'effective',
      label: 'Effective Calls',
      value: kpis.EffectiveCalls,
      conversion: {
        label: 'Effective Call Rate',
        kind: 'percent',
        value: guardedApiPercent(kpis.EffectiveCallRate, kpis.ActualVisits),
      },
      leak: { label: 'No-Order Visits', value: kpis.ActualVisits - kpis.EffectiveCalls },
    },
    {
      key: 'orders',
      label: 'Orders',
      value: kpis.TotalOrders,
      conversion: {
        label: 'Order Conversion',
        kind: 'percent',
        value: safePercent(kpis.TotalOrders, kpis.EffectiveCalls),
      },
      leak: null,
    },
    {
      key: 'orderValue',
      label: 'Order Value',
      value: kpis.TotalOmzet,
      conversion: {
        label: 'Average Order Value',
        kind: 'currency',
        value: safeDivide(kpis.TotalOmzet, kpis.TotalOrders),
      },
      leak: null,
    },
  ]
}
