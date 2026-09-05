import { describe, expect, it } from 'vitest'
import {
  formatPeerGroupLabel,
  peerGroupDimensionHeading,
  resolvePeerGroupLabel,
} from '@/services/peerGroupLabel'

describe('formatPeerGroupLabel', () => {
  it('formats customer wilayah with dimension', () => {
    expect(formatPeerGroupLabel({
      entityType: 'Customer',
      peerGroupRuleId: 'customer-wilayah',
      peerGroupSize: 47,
      dimensionValue: 'Jakarta Pusat',
    })).toBe('47 customers in Wilayah: Jakarta Pusat')
  })

  it('formats customer klasifikasi with dimension', () => {
    expect(formatPeerGroupLabel({
      entityType: 'Customer',
      peerGroupRuleId: 'customer-klasifikasi',
      peerGroupSize: 47,
      dimensionValue: 'A',
    })).toBe('47 customers in Klasifikasi: A')
  })

  it('formats customer wilayah without dimension', () => {
    expect(formatPeerGroupLabel({
      entityType: 'Customer',
      peerGroupRuleId: 'customer-wilayah',
      peerGroupSize: 12,
    })).toBe('12 customers in the same Wilayah')
  })

  it('formats item principal with dimension', () => {
    expect(formatPeerGroupLabel({
      entityType: 'Item',
      peerGroupRuleId: 'item-principal',
      peerGroupSize: 32,
      dimensionValue: 'Alpha Principal',
    })).toBe('32 items in Principal: Alpha Principal')
  })

  it('formats item category with dimension', () => {
    expect(formatPeerGroupLabel({
      entityType: 'Item',
      peerGroupRuleId: 'item-category',
      peerGroupSize: 18,
      dimensionValue: 'OTC',
    })).toBe('18 items in Category: OTC')
  })

  it('returns null when peer group size is zero', () => {
    expect(formatPeerGroupLabel({
      entityType: 'Customer',
      peerGroupRuleId: 'customer-wilayah',
      peerGroupSize: 0,
      dimensionValue: 'Jakarta Pusat',
    })).toBeNull()
  })
})

describe('peerGroupDimensionHeading', () => {
  it('labels klasifikasi dimension', () => {
    expect(peerGroupDimensionHeading({
      peerGroupRuleId: 'customer-klasifikasi',
      entityType: 'Customer',
      dimensionValue: 'A',
    })).toBe('Klasifikasi: A')
  })

  it('labels wilayah dimension', () => {
    expect(peerGroupDimensionHeading({
      peerGroupRuleId: 'customer-wilayah',
      entityType: 'Customer',
      dimensionValue: 'Jakarta',
    })).toBe('Wilayah: Jakarta')
  })

  it('labels item principal dimension', () => {
    expect(peerGroupDimensionHeading({
      peerGroupRuleId: 'item-principal',
      entityType: 'Item',
      dimensionValue: 'Alpha Principal',
    })).toBe('Principal: Alpha Principal')
  })

  it('labels item category dimension', () => {
    expect(peerGroupDimensionHeading({
      peerGroupRuleId: 'item-category',
      entityType: 'Item',
      dimensionValue: 'OTC',
    })).toBe('Category: OTC')
  })
})

describe('resolvePeerGroupLabel', () => {
  it('prefers API formatted label', () => {
    expect(resolvePeerGroupLabel({
      entityType: 'Customer',
      distribution: {
        FormattedPeerGroupLabel: '47 customers in Wilayah: Jakarta Pusat',
        PeerGroupRuleId: 'customer-wilayah',
        PeerGroupSize: 47,
      },
    })).toBe('47 customers in Wilayah: Jakarta Pusat')
  })

  it('falls back to population dimension when API label is missing', () => {
    expect(resolvePeerGroupLabel({
      entityType: 'Customer',
      distribution: {
        PeerGroupRuleId: 'customer-wilayah',
        PeerGroupSize: 47,
      },
      populationDimensionValue: 'Jakarta Pusat',
    })).toBe('47 customers in Wilayah: Jakarta Pusat')
  })
})
