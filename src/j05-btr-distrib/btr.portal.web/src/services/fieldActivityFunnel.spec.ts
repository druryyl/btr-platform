import { describe, expect, it } from 'vitest'
import type { FieldActivityTeamKpis } from '@/models/fieldActivity'
import { buildFunnelStages, type FunnelStage } from './fieldActivityFunnel'

function teamKpis(overrides: Partial<FieldActivityTeamKpis> = {}): FieldActivityTeamKpis {
  return {
    ActiveSalesmenCount: 10,
    PlannedVisits: 100,
    ActualVisits: 88,
    VisitExecutionPercent: 88.3,
    EffectiveCalls: 50,
    EffectiveCallRate: 56.8,
    MissedVisits: 12,
    UnplannedVisits: 4,
    GpsValidRate: 95,
    TotalOrders: 25,
    TotalOmzet: 12_500_000,
    ...overrides,
  }
}

function stage(stages: FunnelStage[], key: FunnelStage['key']): FunnelStage {
  const found = stages.find((entry) => entry.key === key)
  if (!found) throw new Error(`stage ${key} not found`)
  return found
}

describe('fieldActivityFunnel', () => {
  describe('buildFunnelStages', () => {
    it('returns exactly five ordered stages', () => {
      const stages = buildFunnelStages(teamKpis())

      expect(stages.map((entry) => entry.key)).toEqual([
        'planned',
        'actual',
        'effective',
        'orders',
        'orderValue',
      ])
      expect(stages.map((entry) => entry.label)).toEqual([
        'Planned Visits',
        'Actual Visits',
        'Effective Calls',
        'Orders',
        'Order Value',
      ])
    })

    it('maps each primary value from TeamKpis', () => {
      const stages = buildFunnelStages(teamKpis())

      expect(stage(stages, 'planned').value).toBe(100)
      expect(stage(stages, 'actual').value).toBe(88)
      expect(stage(stages, 'effective').value).toBe(50)
      expect(stage(stages, 'orders').value).toBe(25)
      expect(stage(stages, 'orderValue').value).toBe(12_500_000)
    })

    it('returns an empty list for a null team', () => {
      expect(buildFunnelStages(null)).toEqual([])
    })

    it('reports the API execution rate on the Actual stage and no conversion on Planned', () => {
      const stages = buildFunnelStages(teamKpis())

      expect(stage(stages, 'planned').conversion).toBeNull()
      expect(stage(stages, 'actual').conversion).toEqual({
        label: 'Execution Rate',
        kind: 'percent',
        value: 88.3,
      })
    })

    it('reports the API effective call rate on the Effective Calls stage', () => {
      const stages = buildFunnelStages(teamKpis())

      expect(stage(stages, 'effective').conversion).toEqual({
        label: 'Effective Call Rate',
        kind: 'percent',
        value: 56.8,
      })
    })

    it('derives Order Conversion as Orders / Effective Calls', () => {
      const stages = buildFunnelStages(teamKpis({ TotalOrders: 25, EffectiveCalls: 50 }))

      expect(stage(stages, 'orders').conversion).toEqual({
        label: 'Order Conversion',
        kind: 'percent',
        value: 50,
      })
    })

    it('derives Average Order Value as Order Value / Orders', () => {
      const stages = buildFunnelStages(teamKpis({ TotalOmzet: 12_500_000, TotalOrders: 25 }))

      expect(stage(stages, 'orderValue').conversion).toEqual({
        label: 'Average Order Value',
        kind: 'currency',
        value: 500_000,
      })
    })

    it('exposes the two approved leaks and no others (Authority Alignment Note 1)', () => {
      const stages = buildFunnelStages(teamKpis({ ActualVisits: 88, EffectiveCalls: 50, MissedVisits: 12 }))

      expect(stage(stages, 'planned').leak).toBeNull()
      expect(stage(stages, 'actual').leak).toEqual({ label: 'Missed Visits', value: 12 })
      expect(stage(stages, 'effective').leak).toEqual({ label: 'No-Order Visits', value: 38 })
      expect(stage(stages, 'orders').leak).toBeNull()
      expect(stage(stages, 'orderValue').leak).toBeNull()
    })

    it('renders Execution Rate null when Planned is zero', () => {
      const stages = buildFunnelStages(
        teamKpis({ PlannedVisits: 0, ActualVisits: 0, VisitExecutionPercent: null }),
      )

      expect(stage(stages, 'actual').conversion?.value).toBeNull()
    })

    it('forces Execution Rate null when Planned is zero even if the API returns a value', () => {
      const stages = buildFunnelStages(
        teamKpis({ PlannedVisits: 0, VisitExecutionPercent: 88.3 }),
      )

      expect(stage(stages, 'actual').conversion?.value).toBeNull()
    })

    it('renders Effective Call Rate null when Actual is zero', () => {
      const stages = buildFunnelStages(
        teamKpis({ ActualVisits: 0, EffectiveCallRate: null }),
      )

      expect(stage(stages, 'effective').conversion?.value).toBeNull()
    })

    it('renders Order Conversion null when Effective Calls is zero', () => {
      const stages = buildFunnelStages(teamKpis({ EffectiveCalls: 0, TotalOrders: 0 }))

      expect(stage(stages, 'orders').conversion?.value).toBeNull()
    })

    it('renders Average Order Value null when Orders is zero', () => {
      const stages = buildFunnelStages(teamKpis({ TotalOrders: 0, TotalOmzet: 0 }))

      expect(stage(stages, 'orderValue').conversion?.value).toBeNull()
    })

    it('returns all five stages with null conversions on an all-zero team day', () => {
      const stages = buildFunnelStages(
        teamKpis({
          ActiveSalesmenCount: 0,
          PlannedVisits: 0,
          ActualVisits: 0,
          VisitExecutionPercent: null,
          EffectiveCalls: 0,
          EffectiveCallRate: null,
          MissedVisits: 0,
          UnplannedVisits: 0,
          GpsValidRate: null,
          TotalOrders: 0,
          TotalOmzet: 0,
        }),
      )

      expect(stages).toHaveLength(5)
      for (const entry of stages) {
        if (entry.conversion) {
          expect(entry.conversion.value).toBeNull()
        }
      }
      expect(stage(stages, 'actual').leak).toEqual({ label: 'Missed Visits', value: 0 })
      expect(stage(stages, 'effective').leak).toEqual({ label: 'No-Order Visits', value: 0 })
    })

    it('never produces NaN or Infinity across zero-denominator combinations', () => {
      const stages = buildFunnelStages(
        teamKpis({
          PlannedVisits: 0,
          ActualVisits: 0,
          VisitExecutionPercent: 0,
          EffectiveCalls: 0,
          EffectiveCallRate: 0,
          TotalOrders: 0,
          TotalOmzet: 0,
        }),
      )

      for (const entry of stages) {
        expect(Number.isFinite(entry.value)).toBe(true)
        if (entry.conversion) {
          expect(entry.conversion.value).toBeNull()
        }
      }
    })
  })
})
