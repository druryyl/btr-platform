import { describe, expect, it } from 'vitest'
import type { ProfileOverviewSection } from '@/models/entityAnalytics'
import {
  isLastInvoicingSalesmanRelationship,
  LAST_INVOICING_SALESMAN_LABEL,
  LAST_INVOICING_SALESMAN_NOTE,
} from '@/services/customerLastInvoicingSalesman'
import { buildProfileOverviewLayout } from '@/services/profileOverviewLayout'

function customerOverview(dimensions: Record<string, string>): ProfileOverviewSection {
  return {
    IsAvailable: true,
    UnavailableReason: null,
    EntityType: 'Customer',
    EntityId: 'C001',
    EntityCode: 'C001',
    DisplayName: 'Customer One',
    IsActive: true,
    GeneratedAt: null,
    Dimensions: dimensions,
  }
}

describe('customer last-invoicing salesman labels', () => {
  it('does not label the latest-Faktur Salesman as Assigned Salesman or Owner', () => {
    const layout = buildProfileOverviewLayout(customerOverview({
      'Last Invoicing Salesman': 'Budi',
    }))
    const fields = layout.sections.flatMap((section) => section.fields)
    const salesman = fields.find((field) => field.value === 'Budi')

    expect(salesman).toBeDefined()
    expect(salesman?.label).toBe(LAST_INVOICING_SALESMAN_LABEL)
    expect(salesman?.label).not.toMatch(/assigned salesman/i)
    expect(salesman?.label).not.toMatch(/\bowner\b/i)
    expect(salesman?.note).toBe(LAST_INVOICING_SALESMAN_NOTE)
    expect(salesman?.note?.toLowerCase()).toContain('last invoicing salesman')
    expect(salesman?.note?.toLowerCase()).toContain('not the customer owner')
  })

  it('relabels a legacy Salesman dimension the same way', () => {
    const layout = buildProfileOverviewLayout(customerOverview({
      Salesman: 'Budi',
    }))
    const salesman = layout.sections
      .flatMap((section) => section.fields)
      .find((field) => field.value === 'Budi')

    expect(salesman?.label).toBe(LAST_INVOICING_SALESMAN_LABEL)
    expect(salesman?.note).toBe(LAST_INVOICING_SALESMAN_NOTE)
  })

  it('keeps the stored relationship code as the last-invoicing recency indicator', () => {
    expect(isLastInvoicingSalesmanRelationship('AssignedSalesman')).toBe(true)
    expect(isLastInvoicingSalesmanRelationship('TopPrincipalsByOmzet')).toBe(false)
  })
})
