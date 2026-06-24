import type { PortalMenuCode, PortalMenuItem, PortalMenuLink } from './portalMenu.types'
import { allPortalMenuItems } from './portalMenuRegistry'

function normalizeRoute(route: string): string {
  if (!route) return route
  return route.endsWith('/') && route.length > 1 ? route.slice(0, -1) : route
}

export function formatMenuLabel(code: PortalMenuCode | string, label: string): string {
  return `${code} · ${label}`
}

export function findMenuItemByRoute(route: string | null | undefined): PortalMenuItem | undefined {
  if (!route) return undefined
  const normalized = normalizeRoute(route)
  return allPortalMenuItems.find((item) => normalizeRoute(item.route) === normalized)
}

export function findMenuItemByRouteName(routeName: string | null | undefined): PortalMenuItem | undefined {
  if (!routeName) return undefined
  return allPortalMenuItems.find((item) => item.routeName === routeName)
}

export function formatMenuLabelByRoute(route: string | null | undefined, fallbackLabel?: string): string {
  const item = findMenuItemByRoute(route)
  if (item) return formatMenuLabel(item.code, item.label)
  if (fallbackLabel) return fallbackLabel
  return route ?? ''
}

export function toPortalMenuLink(item: PortalMenuItem): PortalMenuLink {
  return {
    code: item.code,
    label: item.label,
    route: item.route,
  }
}

export function resolveInvestigationStepLabel(
  label: string,
  dashboardRoute?: string | null,
  reportRoute?: string | null,
): string {
  const route = dashboardRoute || reportRoute
  const menuLabel = formatMenuLabelByRoute(route)
  if (menuLabel && menuLabel !== route) return menuLabel
  return label
}
