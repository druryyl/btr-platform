export interface PercentileRankResult {
  percentile: number | undefined
  populationSize: number
}

export function percentileRank(
  values: ReadonlyArray<number | null | undefined>,
  target: number | null | undefined,
): PercentileRankResult {
  const population = values.filter((value): value is number => value != null)
  const populationSize = population.length
  if (target == null || populationSize === 0) {
    return { percentile: undefined, populationSize }
  }
  const lowerCount = population.filter((value) => value < target).length
  return { percentile: (lowerCount / populationSize) * 100, populationSize }
}
