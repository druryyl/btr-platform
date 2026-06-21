export const COLLECTION_OPTIMIZATION_ACTION_CATEGORY_KEYS = [
  'ImmediateCollection',
  'EscalateManagement',
  'PriorityFollowUp',
  'ProactiveReminder',
  'CreditReview',
  'SalesRecoveryVisit',
  'LegacyDebtReview',
  'RelationshipMonitor',
  'DeferCollection',
  'NoActionToday',
] as const

export type CollectionOptimizationActionCategoryKey =
  (typeof COLLECTION_OPTIMIZATION_ACTION_CATEGORY_KEYS)[number]

export const COLLECTION_OPTIMIZATION_ACTION_CATEGORY_LABELS: Record<
  CollectionOptimizationActionCategoryKey,
  string
> = {
  ImmediateCollection: 'Immediate Collection',
  EscalateManagement: 'Escalate to Management',
  PriorityFollowUp: 'Priority Follow-up',
  ProactiveReminder: 'Send Reminder',
  CreditReview: 'Credit Review',
  SalesRecoveryVisit: 'Schedule Sales Visit',
  LegacyDebtReview: 'Legacy Debt Review',
  RelationshipMonitor: 'Continue Monitoring',
  DeferCollection: 'Safe to Wait',
  NoActionToday: 'No Action Today',
}

export const COLLECTION_OPTIMIZATION_QUEUE_KEYS = [
  'ProactiveReminder',
  'CreditReview',
  'SalesRecovery',
  'EscalateManagement',
] as const

export type CollectionOptimizationQueueKey = (typeof COLLECTION_OPTIMIZATION_QUEUE_KEYS)[number]

export const COLLECTION_OPTIMIZATION_QUEUE_LABELS: Record<CollectionOptimizationQueueKey, string> = {
  ProactiveReminder: 'Proactive Reminders',
  CreditReview: 'Credit Review',
  SalesRecovery: 'Sales Recovery',
  EscalateManagement: 'Management Escalation',
}

export type CollectionOptimizationBadgeSeverity = 'success' | 'info' | 'warn' | 'danger' | 'secondary'

export function actionCategoryBadgeSeverity(
  actionCategoryKey: string,
): CollectionOptimizationBadgeSeverity {
  switch (actionCategoryKey) {
    case 'ImmediateCollection':
    case 'EscalateManagement':
      return 'danger'
    case 'PriorityFollowUp':
    case 'CreditReview':
    case 'LegacyDebtReview':
      return 'warn'
    case 'ProactiveReminder':
    case 'SalesRecoveryVisit':
    case 'RelationshipMonitor':
      return 'info'
    case 'DeferCollection':
      return 'success'
    default:
      return 'secondary'
  }
}

export function actionOwnerBadgeSeverity(owner: string): CollectionOptimizationBadgeSeverity {
  if (owner === 'Sales') return 'info'
  if (owner === 'Finance') return 'warn'
  if (owner === 'Management') return 'danger'
  return 'secondary'
}

export function filterQueuesByKey<T extends { QueueKey: string }>(
  items: T[],
  queueKey: string,
): T[] {
  if (!queueKey) return items
  return items.filter((item) => item.QueueKey === queueKey)
}
