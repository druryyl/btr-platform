export const PU01_PURCHASE_IN_LABEL = 'Purchase-In'

export const PU01_MTD_PURCHASE_IN_LABEL = 'MTD Purchase-In'

export const PU01_PERCENT_OF_PURCHASE_IN_LABEL = '% of Purchase-In'

export const PU01_WEEKLY_PURCHASE_IN_TREND_TITLE = 'Weekly Purchase-In Trend'

export const PU01_WEEKLY_PURCHASE_IN_EMPTY = 'No weekly Purchase-In data for the current period.'

export const PU01_TOP_PRINCIPALS_TITLE = 'Top 10 Principals by Purchase-In'

export const PU01_TOP_PRINCIPALS_EMPTY = 'No Purchase-In ranking for the current period.'

export const PU02_PURCHASE_IN_LABEL = 'Purchase-In'

export const PU02_EVIDENCE_NOTE =
  'Purchase-invoice evidence for Purchase-In. This is not Principal Sales-Out.'

export const PU01_PURCHASE_IN_DISCLOSURES = [
  'Purchase amounts on this page are Purchase-In. They are not Principal Sales-Out and are not PRN-SALES-001.',
  'This page does not display PRN-SALES-001 as purchase value.',
  'Purchase growth is not sales growth. Purchase-In is not Principal sales performance.',
] as const

export const PU02_PURCHASE_IN_DISCLOSURES = [
  'This report is purchase-invoice evidence. Invoice totals are Purchase-In, not Principal Sales-Out.',
  'This report does not display PRN-SALES-001.',
] as const

export function isSalesOutPresentedAsPurchase(label: string | null | undefined): boolean {
  const normalized = (label ?? '').toLowerCase().replace(/[^a-z0-9]/g, '')
  return normalized.includes('prnsales001') || normalized === 'principalsalesout'
}

export function isPurchaseGrowthRenamedToSalesGrowth(label: string | null | undefined): boolean {
  const normalized = (label ?? '').toLowerCase().replace(/[^a-z0-9]/g, '')
  return normalized.includes('salesgrowth')
}
