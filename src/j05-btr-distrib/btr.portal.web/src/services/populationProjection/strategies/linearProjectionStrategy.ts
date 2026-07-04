import type {
  IPopulationProjectionStrategy,
  PopulationEntityInput,
  PopulationProjectionResult,
} from '@/services/populationProjection/types'

/** Placeholder for future linear projection without log/normalize transforms. */
export class LinearProjectionStrategy implements IPopulationProjectionStrategy {
  readonly id = 'linear'

  project(_entities: PopulationEntityInput[]): PopulationProjectionResult | null {
    throw new Error('LinearProjectionStrategy is not yet implemented')
  }
}

export const linearProjectionStrategy = new LinearProjectionStrategy()
