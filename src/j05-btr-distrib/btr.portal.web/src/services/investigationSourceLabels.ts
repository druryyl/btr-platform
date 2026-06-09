const SOURCE_LABELS: Record<string, string> = {
  '/dashboard': 'Executive Dashboard',
  '/alerts': 'Alert Center',
  '/dashboard/customers': 'Customer Analytics',
  '/dashboard/collection': 'Collection Dashboard',
  '/dashboard/salesmen': 'Salesman Performance',
  '/dashboard/inventory-risk': 'Inventory Risk',
  '/dashboard/purchasing': 'Purchasing Dashboard',
  '/dashboard/locations': 'Location Performance',
  '/dashboard/sales': 'Sales Dashboard',
  '/dashboard/piutang': 'Piutang Dashboard',
  '/dashboard/inventory': 'Inventory Dashboard',
}

export function resolveInvestigationSourceLabel(routePath: string): string {
  return SOURCE_LABELS[routePath] ?? routePath
}
