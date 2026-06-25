export type PortalMenuCode =
  | 'EX01'
  | 'EX02'
  | 'EX03'
  | 'SA01'
  | 'SA02'
  | 'SA03'
  | 'CU01'
  | 'CU02'
  | 'CU03'
  | 'CU04'
  | 'CU05'
  | 'FI01'
  | 'FI02'
  | 'FI03'
  | 'FI04'
  | 'SF01'
  | 'SF02'
  | 'IN01'
  | 'IN02'
  | 'IN03'
  | 'IN04'
  | 'IN05'
  | 'PU01'
  | 'PU02'
  | 'OP01'

export type PortalMenuGroupId =
  | 'executive'
  | 'sales'
  | 'customers'
  | 'finance'
  | 'sales-force'
  | 'inventory'
  | 'purchasing'
  | 'operations'

export interface PortalMenuItem {
  code: PortalMenuCode
  label: string
  icon: string
  routeName: string
  route: string
  order: number
  groupId: PortalMenuGroupId
  isDashboard: boolean
}

export interface PortalMenuGroup {
  id: PortalMenuGroupId
  label: string
  order: number
  items: PortalMenuItem[]
}

export interface PortalMenuLink {
  code: PortalMenuCode
  label: string
  route: string
}
