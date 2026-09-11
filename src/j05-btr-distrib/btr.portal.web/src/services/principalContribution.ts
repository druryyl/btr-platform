export function contributionPercentage(
  amount: number | null | undefined,
  denominator: number | null | undefined,
): number | null {
  if (amount == null || denominator == null || denominator <= 0) {
    return null
  }
  return (amount / denominator) * 100
}
