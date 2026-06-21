import { describe, expect, it } from 'vitest'
import {
  actionCategoryBadgeSeverity,
  COLLECTION_OPTIMIZATION_ACTION_CATEGORY_LABELS,
  COLLECTION_OPTIMIZATION_QUEUE_LABELS,
  filterQueuesByKey,
} from '@/services/collectionOptimizationSignals'

describe('collectionOptimizationSignals', () => {
  it('maps action category labels', () => {
    expect(COLLECTION_OPTIMIZATION_ACTION_CATEGORY_LABELS.ImmediateCollection).toBe(
      'Immediate Collection',
    )
    expect(COLLECTION_OPTIMIZATION_ACTION_CATEGORY_LABELS.ProactiveReminder).toBe('Send Reminder')
  })

  it('maps queue labels', () => {
    expect(COLLECTION_OPTIMIZATION_QUEUE_LABELS.CreditReview).toBe('Credit Review')
  })

  it('filters specialized queues by queue key', () => {
    const rows = [
      { QueueKey: 'CreditReview', CustomerName: 'A' },
      { QueueKey: 'SalesRecovery', CustomerName: 'B' },
    ]

    expect(filterQueuesByKey(rows, 'CreditReview')).toHaveLength(1)
    expect(filterQueuesByKey(rows, 'CreditReview')[0]?.CustomerName).toBe('A')
  })

  it('assigns severity for immediate collection', () => {
    expect(actionCategoryBadgeSeverity('ImmediateCollection')).toBe('danger')
    expect(actionCategoryBadgeSeverity('DeferCollection')).toBe('success')
  })
})
