const INFRASTRUCTURE_ERROR_PATTERNS = [
  'snapshot refresh worker',
  'not yet available',
  'Some dashboard data is not yet available',
] as const

export function isInfrastructureStoreError(message: string): boolean {
  const normalized = message.trim().toLowerCase()

  return INFRASTRUCTURE_ERROR_PATTERNS.some((pattern) =>
    normalized.includes(pattern.toLowerCase()),
  )
}

export function shouldShowInfrastructureError(
  message: string | null | undefined,
  hidePlatformDiagnostics: boolean,
): boolean {
  if (!message) {
    return false
  }

  if (!hidePlatformDiagnostics) {
    return true
  }

  return !isInfrastructureStoreError(message)
}
