import { describe, expect, it } from 'vitest'
import { contributionPercentage } from './principalContribution'

describe('contributionPercentage', () => {
  it('returns the ratio scaled to percent units', () => {
    expect(contributionPercentage(450_000_000, 1_000_000_000)).toBe(45)
  })

  it('returns null when amount is null', () => {
    expect(contributionPercentage(null, 1_000_000_000)).toBeNull()
  })

  it('returns null when denominator is null', () => {
    expect(contributionPercentage(100, null)).toBeNull()
  })

  it('returns null when denominator is zero', () => {
    expect(contributionPercentage(100, 0)).toBeNull()
  })

  it('returns null when denominator is negative', () => {
    expect(contributionPercentage(100, -1)).toBeNull()
  })

  it('returns zero when amount is zero', () => {
    expect(contributionPercentage(0, 1_000_000_000)).toBe(0)
  })
})
