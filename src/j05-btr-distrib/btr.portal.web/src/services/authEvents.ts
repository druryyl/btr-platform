let unauthorizedHandler: (() => void) | null = null

export function setUnauthorizedHandler(handler: () => void): void {
  unauthorizedHandler = handler
}

export function notifyUnauthorized(): void {
  unauthorizedHandler?.()
}
