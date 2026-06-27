import type { ProfileOverviewSection } from '@/models/entityAnalytics'
import { actionBadgeSeverity } from '@/services/customerPortfolioSignals'
import { categoryBadgeSeverity } from '@/services/customerRiskForecastSignals'

export type OverviewSectionId = 'identity' | 'business' | 'performance' | 'activity' | 'details'

export type OverviewBadgeSeverity = 'success' | 'info' | 'warn' | 'danger' | 'secondary'

export interface OverviewField {
  key: string
  label: string
  value: string
  isBadge: boolean
  badgeSeverity?: OverviewBadgeSeverity
}

export interface OverviewSectionGroup {
  id: OverviewSectionId
  title: string
  fields: OverviewField[]
}

export interface ProfileOverviewLayout {
  displayName: string
  statusLabel: string
  statusSeverity: OverviewBadgeSeverity
  sections: OverviewSectionGroup[]
  details: OverviewField[]
}

const SECTION_TITLES: Record<OverviewSectionId, string> = {
  identity: 'Identity',
  business: 'Business Profile',
  performance: 'Performance',
  activity: 'Activity',
  details: 'Details',
}

const ENTITY_TYPE_LABELS: Record<string, string> = {
  Customer: 'Customer',
  Salesman: 'Sales Representative',
  Item: 'Product Item',
  Supplier: 'Supplier',
}

const FIELD_LABEL_OVERRIDES: Record<string, string> = {
  Wilayah: 'Region',
  Klasifikasi: 'Classification',
  Salesman: 'Assigned Salesman',
  'Faktur Count (6 Mo)': 'Invoices (6 Months)',
  'Active MTD': 'Active This Month',
  Active: 'Currently Active',
  'Portfolio Priority Score': 'Priority Score',
  'Days Since Last Faktur': 'Days Since Last Sale',
  'Movement Class': 'Inventory Movement',
  'At-Risk Value': 'At-Risk Inventory',
  'Customer Count': 'Active Customers',
  'Dormant Customer Count': 'Dormant Customers',
  'Active SKU Count': 'Active SKUs',
  'Catalog Penetration': 'Catalog Coverage',
  'Purchase Share': 'Purchase Share',
  'Overdue Balance': 'Overdue Receivables',
  'Primary Risk Signal': 'Primary Risk Signal',
  'Attention Signals': 'Attention Signals',
  'Achievement Band': 'Achievement Band',
  'Portfolio Action': 'Recommended Action',
  'Risk Category': 'Risk Category',
  Lifecycle: 'Lifecycle Stage',
  Tier: 'Portfolio Tier',
  Category: 'Product Category',
  Supplier: 'Primary Supplier',
  Segment: 'Market Segment',
  'Inventory Value': 'Inventory Value',
  'Last Purchase': 'Last Purchase',
}

const SECTION_BY_NORMALIZED_LABEL: Record<string, OverviewSectionId> = {
  wilayah: 'business',
  klasifikasi: 'business',
  salesman: 'business',
  segment: 'business',
  category: 'business',
  supplier: 'business',
  lifecycle: 'performance',
  tier: 'performance',
  portfolioaction: 'performance',
  riskcategory: 'performance',
  achievementband: 'performance',
  movementclass: 'performance',
  primaryrisksignal: 'performance',
  purchaseshare: 'performance',
  inventoryvalue: 'performance',
  atriskvalue: 'performance',
  overduebalance: 'performance',
  catalogpenetration: 'performance',
  lastpurchase: 'activity',
  fakturcount6mo: 'activity',
  activemtd: 'activity',
  active: 'activity',
  dayssincelastfaktur: 'activity',
  attentionsignals: 'activity',
  customercount: 'activity',
  dormantcustomercount: 'activity',
  activeskucount: 'activity',
  portfoliopriorityscore: 'details',
}

const BADGE_LABELS = new Set([
  'lifecycle',
  'tier',
  'portfolioaction',
  'riskcategory',
  'achievementband',
  'movementclass',
])

function normalizeLabel(label: string): string {
  return label.toLowerCase().replace(/[^a-z0-9]/g, '')
}

function friendlyLabel(label: string): string {
  return FIELD_LABEL_OVERRIDES[label] ?? label
}

function friendlyEntityType(entityType: string): string {
  return ENTITY_TYPE_LABELS[entityType] ?? entityType
}

function lifecycleBadgeSeverity(value: string): OverviewBadgeSeverity {
  const normalized = normalizeLabel(value)

  if (normalized === 'dormant' || normalized === 'declining' || normalized === 'neverpurchased') {
    return 'danger'
  }
  if (normalized === 'growing' || normalized === 'new') {
    return 'success'
  }
  if (normalized === 'mature') {
    return 'info'
  }

  return 'secondary'
}

function tierBadgeSeverity(value: string): OverviewBadgeSeverity {
  const normalized = normalizeLabel(value)

  if (normalized === 'strategic') return 'success'
  if (normalized === 'highvalue') return 'info'
  if (normalized === 'mediumvalue') return 'secondary'
  if (normalized === 'lowvalue') return 'warn'

  return 'secondary'
}

function achievementBandBadgeSeverity(value: string): OverviewBadgeSeverity {
  switch (value.toLowerCase()) {
    case 'healthy':
      return 'success'
    case 'warning':
      return 'warn'
    case 'critical':
      return 'danger'
    default:
      return 'secondary'
  }
}

function movementClassBadgeSeverity(value: string): OverviewBadgeSeverity {
  const normalized = normalizeLabel(value)

  if (normalized === 'deadstock' || normalized === 'neversold') return 'danger'
  if (normalized === 'slowmoving') return 'warn'
  if (normalized === 'active') return 'success'

  return 'secondary'
}

function resolveBadgeSeverity(normalizedLabel: string, value: string): OverviewBadgeSeverity {
  switch (normalizedLabel) {
    case 'lifecycle':
      return lifecycleBadgeSeverity(value)
    case 'tier':
      return tierBadgeSeverity(value)
    case 'portfolioaction':
      return actionBadgeSeverity(value.replace(/\s+/g, ''))
    case 'riskcategory':
      return categoryBadgeSeverity(value)
    case 'achievementband':
      return achievementBandBadgeSeverity(value)
    case 'movementclass':
      return movementClassBadgeSeverity(value)
    default:
      return 'secondary'
  }
}

function createField(key: string, label: string, value: string): OverviewField {
  const normalized = normalizeLabel(label)
  const isBadge = BADGE_LABELS.has(normalized)

  return {
    key,
    label: friendlyLabel(label),
    value,
    isBadge,
    badgeSeverity: isBadge ? resolveBadgeSeverity(normalized, value) : undefined,
  }
}

function pushField(
  buckets: Record<OverviewSectionId, OverviewField[]>,
  sectionId: OverviewSectionId,
  field: OverviewField,
) {
  buckets[sectionId].push(field)
}

export function buildProfileOverviewLayout(
  section: ProfileOverviewSection,
): ProfileOverviewLayout {
  const buckets: Record<OverviewSectionId, OverviewField[]> = {
    identity: [],
    business: [],
    performance: [],
    activity: [],
    details: [],
  }

  const displayName = section.DisplayName || section.EntityCode || section.EntityId

  pushField(buckets, 'identity', createField('entityType', 'Entity Type', friendlyEntityType(section.EntityType)))
  pushField(buckets, 'identity', createField('entityCode', 'Business Code', section.EntityCode))

  for (const [label, value] of Object.entries(section.Dimensions ?? {})) {
    if (!value?.trim()) continue

    const normalized = normalizeLabel(label)
    const sectionId = SECTION_BY_NORMALIZED_LABEL[normalized] ?? 'activity'
    pushField(buckets, sectionId, createField(normalized, label, value))
  }

  const sections: OverviewSectionGroup[] = (['identity', 'business', 'performance', 'activity'] as const)
    .map((id) => ({
      id,
      title: SECTION_TITLES[id],
      fields: buckets[id],
    }))
    .filter((group) => group.fields.length > 0)

  const details: OverviewField[] = [...buckets.details]

  if (section.GeneratedAt) {
    details.unshift(
      createField('snapshotTime', 'Snapshot Time', section.GeneratedAt),
    )
  }

  return {
    displayName,
    statusLabel: section.IsActive ? 'Active' : 'Inactive',
    statusSeverity: section.IsActive ? 'success' : 'secondary',
    sections,
    details,
  }
}
