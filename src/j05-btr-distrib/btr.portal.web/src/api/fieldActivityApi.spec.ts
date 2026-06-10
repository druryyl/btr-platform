import { describe, expect, it, vi, beforeEach } from 'vitest'
import { httpClient } from '@/api/httpClient'
import { getFieldActivity, listFieldActivitySalesmen } from '@/api/fieldActivityApi'

vi.mock('@/api/httpClient', () => ({
  httpClient: {
    get: vi.fn(),
  },
}))

describe('fieldActivityApi', () => {
  beforeEach(() => {
    vi.mocked(httpClient.get).mockReset()
  })

  it('getFieldActivity encodes salesman and visit date params', async () => {
    vi.mocked(httpClient.get).mockResolvedValue({
      data: {
        Status: 'success',
        Code: 200,
        Message: null,
        Data: {
          SalesPersonId: 'SP1',
          VisitDate: '2026-06-11',
        },
      },
    })

    await getFieldActivity('SP1', '2026-06-11')

    expect(httpClient.get).toHaveBeenCalledWith('/api/dashboard/field-activity', {
      params: {
        salesPersonId: 'SP1',
        visitDate: '2026-06-11',
      },
    })
  })

  it('listFieldActivitySalesmen calls salesmen endpoint', async () => {
    vi.mocked(httpClient.get).mockResolvedValue({
      data: {
        Status: 'success',
        Code: 200,
        Message: null,
        Data: { Items: [] },
      },
    })

    await listFieldActivitySalesmen()

    expect(httpClient.get).toHaveBeenCalledWith('/api/dashboard/field-activity/salesmen')
  })
})
