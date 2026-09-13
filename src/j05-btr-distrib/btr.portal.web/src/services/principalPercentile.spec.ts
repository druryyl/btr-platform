import { describe, expect, it } from 'vitest'
import { percentileRank } from './principalPercentile'

describe('percentileRank', () => {
  it('returns undefined with population size 0 for an empty population', () => {
    expect(percentileRank([], 10)).toEqual({ percentile: undefined, populationSize: 0 })
  })

  it('does not throw for a single-principal population and returns the defined value', () => {
    expect(percentileRank([50], 50)).toEqual({ percentile: 0, populationSize: 1 })
  })

  it('returns 100 when the target exceeds the lone population value', () => {
    expect(percentileRank([50], 60)).toEqual({ percentile: 100, populationSize: 1 })
  })

  it('excludes null and undefined values from the population', () => {
    const result = percentileRank([10, null, 20, undefined], 20)
    expect(result).toEqual({ percentile: 50, populationSize: 2 })
  })

  it('returns undefined (not 0) when the target metric is null', () => {
    expect(percentileRank([10, 20, 30], null)).toEqual({ percentile: undefined, populationSize: 3 })
  })

  it('returns undefined (not 0) when the target metric is undefined', () => {
    expect(percentileRank([10, 20, 30], undefined)).toEqual({
      percentile: undefined,
      populationSize: 3,
    })
  })

  it('counts only strictly lower values so ties do not inflate the rank', () => {
    expect(percentileRank([10, 20, 20, 30], 20)).toEqual({ percentile: 25, populationSize: 4 })
  })

  it('returns the population size alongside the percentile', () => {
    const result = percentileRank([10, 20, 30, 40], 30)
    expect(result.populationSize).toBe(4)
    expect(result.percentile).toBe(50)
  })
})
