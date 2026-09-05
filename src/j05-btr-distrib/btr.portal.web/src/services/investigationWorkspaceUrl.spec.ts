import { describe, expect, it } from 'vitest'
import { buildWorkspaceQuery, parseWorkspaceUrlState } from '@/services/investigationWorkspaceUrl'

describe('investigationWorkspaceUrl peerGroup', () => {
  it('serializes peerGroup query param', () => {
    expect(buildWorkspaceQuery({
      entityType: 'Customer',
      peerGroupRuleId: 'customer-klasifikasi',
    })).toEqual({ peerGroup: 'customer-klasifikasi' })
  })

  it('parses peerGroup query param', () => {
    const state = parseWorkspaceUrlState('Customer', {
      peerGroup: 'customer-klasifikasi',
    })
    expect(state.peerGroupRuleId).toBe('customer-klasifikasi')
  })

  it('round-trips peer group selection', () => {
    const query = buildWorkspaceQuery({
      entityType: 'Customer',
      presetId: 'customer-risk-map',
      entityIds: ['C001'],
      peerGroupRuleId: 'customer-klasifikasi',
    })
    const state = parseWorkspaceUrlState('Customer', query)
    expect(state.peerGroupRuleId).toBe('customer-klasifikasi')
    expect(state.presetId).toBe('customer-risk-map')
    expect(state.entityIds).toEqual(['C001'])
  })
})
