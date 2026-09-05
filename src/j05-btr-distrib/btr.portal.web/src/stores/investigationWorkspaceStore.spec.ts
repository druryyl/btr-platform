import { createPinia, setActivePinia } from 'pinia'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import axios from 'axios'
import { useInvestigationWorkspaceStore } from '@/stores/investigationWorkspaceStore'

const POPULATION_MAP_TIMEOUT_MS = 30_000

vi.mock('@/api/httpClient', () => ({
  getApiErrorMessage: (error: unknown, fallback = 'An unexpected error occurred.') => {
    const code = (error as { code?: string } | null)?.code
    if (code === 'ECONNABORTED' || code === 'ETIMEDOUT') {
      return 'Request timed out. Please try again.'
    }
    return fallback
  },
}))

vi.mock('@/api/entityAnalyticsApi', () => ({
  POPULATION_MAP_TIMEOUT_MS: 30_000,
  fetchMapPresets: vi.fn(),
  fetchPopulationMap: vi.fn(),
  fetchPeerGroupRules: vi.fn(),
  fetchEntityProfile: vi.fn(),
  fetchEntityCompare: vi.fn(),
}))

import { fetchMapPresets, fetchPeerGroupRules, fetchPopulationMap } from '@/api/entityAnalyticsApi'

function makePopulationResponse(overrides: Record<string, unknown> = {}) {
  return {
    EntityType: 'Customer',
    PresetId: 'default',
    PresetDisplayName: 'Default',
    AxisXKpiId: 'X',
    AxisYKpiId: 'Y',
    AxisXLabel: 'X',
    AxisYLabel: 'Y',
    AxisXUnit: 'IDR',
    AxisYUnit: 'IDR',
    TotalPopulationCount: 1,
    FilteredPopulationCount: 1,
    ActiveFilterDescription: null,
    GeneratedAt: null,
    Points: [],
    ...overrides,
  }
}

describe('investigationWorkspaceStore loadPopulation', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.mocked(fetchPopulationMap).mockReset()
    vi.mocked(fetchPeerGroupRules).mockReset()
    vi.mocked(fetchPeerGroupRules).mockResolvedValue({
      EntityType: 'Customer',
      DefaultRuleId: 'customer-wilayah',
      Rules: [
        {
          RuleId: 'customer-wilayah',
          DisplayLabel: 'Wilayah',
          DimensionLabel: 'Wilayah',
          IsDefault: true,
        },
        {
          RuleId: 'customer-klasifikasi',
          DisplayLabel: 'Klasifikasi',
          DimensionLabel: 'Klasifikasi',
          IsDefault: false,
        },
      ],
    })
  })

  it('clears loadingPopulation after a successful load', async () => {
    vi.mocked(fetchPopulationMap).mockResolvedValue(makePopulationResponse() as never)
    const store = useInvestigationWorkspaceStore()

    await store.loadPopulation()

    expect(store.loadingPopulation).toBe(false)
    expect(store.population).not.toBeNull()
    expect(store.error).toBeNull()
  })

  it('clears loadingPopulation and sets error when fetch fails', async () => {
    vi.mocked(fetchPopulationMap).mockRejectedValue(new Error('network down'))
    const store = useInvestigationWorkspaceStore()

    await store.loadPopulation()

    expect(store.loadingPopulation).toBe(false)
    expect(store.population).toBeNull()
    expect(store.error).toBe('Failed to load population map')
  })

  it('clears loadingPopulation on axios timeout', async () => {
    const err = new axios.AxiosError('timeout of 30000ms exceeded', 'ECONNABORTED')
    vi.mocked(fetchPopulationMap).mockRejectedValue(err)

    const store = useInvestigationWorkspaceStore()
    await store.loadPopulation()

    expect(store.loadingPopulation).toBe(false)
    expect(store.population).toBeNull()
    expect(store.error).toBe('Request timed out. Please try again.')
  })

  it('passes timeout and abort signal to fetchPopulationMap', async () => {
    vi.mocked(fetchPopulationMap).mockResolvedValue(makePopulationResponse() as never)
    const store = useInvestigationWorkspaceStore()

    await store.loadPopulation()

    expect(fetchPopulationMap).toHaveBeenCalledWith(
      expect.objectContaining({
        entityType: 'Customer',
        timeoutMs: POPULATION_MAP_TIMEOUT_MS,
        signal: expect.any(AbortSignal),
      }),
    )
  })

  it('ignores stale response when a newer loadPopulation starts', async () => {
    let resolveFirst!: (value: ReturnType<typeof makePopulationResponse>) => void
    const first = new Promise<ReturnType<typeof makePopulationResponse>>((resolve) => {
      resolveFirst = resolve
    })
    vi.mocked(fetchPopulationMap)
      .mockImplementationOnce(() => first as never)
      .mockResolvedValueOnce(makePopulationResponse({ PresetId: 'second' }) as never)

    const store = useInvestigationWorkspaceStore()
    const firstLoad = store.loadPopulation()
    const secondLoad = store.loadPopulation()

    resolveFirst(makePopulationResponse({ PresetId: 'first' }))
    await Promise.all([firstLoad, secondLoad])

    expect(store.loadingPopulation).toBe(false)
    expect(store.population?.PresetId).toBe('second')
  })
})

describe('investigationWorkspaceStore peer group', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.mocked(fetchPopulationMap).mockReset()
    vi.mocked(fetchPeerGroupRules).mockReset()
    vi.mocked(fetchMapPresets).mockReset()
    vi.mocked(fetchPopulationMap).mockResolvedValue(makePopulationResponse() as never)
    vi.mocked(fetchMapPresets).mockResolvedValue({
      EntityType: 'Customer',
      Presets: [{ PresetId: 'default', DisplayName: 'Default', Description: '', AxisXKpiId: 'X', AxisYKpiId: 'Y', AxisXLabel: 'X', AxisYLabel: 'Y', IsDefault: true }],
    } as never)
    vi.mocked(fetchPeerGroupRules).mockResolvedValue({
      EntityType: 'Customer',
      DefaultRuleId: 'customer-wilayah',
      Rules: [
        {
          RuleId: 'customer-wilayah',
          DisplayLabel: 'Wilayah',
          DimensionLabel: 'Wilayah',
          IsDefault: true,
        },
        {
          RuleId: 'customer-klasifikasi',
          DisplayLabel: 'Klasifikasi',
          DimensionLabel: 'Klasifikasi',
          IsDefault: false,
        },
      ],
    })
  })

  it('loads peer group rules and defaults to wilayah for Customer', async () => {
    const store = useInvestigationWorkspaceStore()
    await store.initializeWorkspace('Customer')

    expect(store.peerGroupRules).toHaveLength(2)
    expect(store.peerGroupRuleId).toBe('customer-wilayah')
    expect(store.showPeerGroupSelector).toBe(true)
  })

  it('accepts peerGroupRuleId from initial URL state', async () => {
    const store = useInvestigationWorkspaceStore()
    await store.initializeWorkspace('Customer', {
      peerGroupRuleId: 'customer-klasifikasi',
    })

    expect(store.peerGroupRuleId).toBe('customer-klasifikasi')
  })

  it('loads peer group rules and defaults to principal for Item', async () => {
    vi.mocked(fetchPeerGroupRules).mockResolvedValue({
      EntityType: 'Item',
      DefaultRuleId: 'item-principal',
      Rules: [
        {
          RuleId: 'item-principal',
          DisplayLabel: 'Principal',
          DimensionLabel: 'Principal',
          IsDefault: true,
        },
        {
          RuleId: 'item-category',
          DisplayLabel: 'Category',
          DimensionLabel: 'Category',
          IsDefault: false,
        },
      ],
    })
    vi.mocked(fetchMapPresets).mockResolvedValue({
      EntityType: 'Item',
      Presets: [{ PresetId: 'default', DisplayName: 'Default', Description: '', AxisXKpiId: 'X', AxisYKpiId: 'Y', AxisXLabel: 'X', AxisYLabel: 'Y', IsDefault: true }],
    } as never)

    const store = useInvestigationWorkspaceStore()
    await store.initializeWorkspace('Item')

    expect(store.peerGroupRules).toHaveLength(2)
    expect(store.peerGroupRuleId).toBe('item-principal')
    expect(store.showPeerGroupSelector).toBe(true)
  })

  it('clears peer group selection when switching entity type', async () => {
    vi.mocked(fetchPeerGroupRules).mockResolvedValueOnce({
      EntityType: 'Customer',
      DefaultRuleId: 'customer-wilayah',
      Rules: [
        {
          RuleId: 'customer-wilayah',
          DisplayLabel: 'Wilayah',
          DimensionLabel: 'Wilayah',
          IsDefault: true,
        },
        {
          RuleId: 'customer-klasifikasi',
          DisplayLabel: 'Klasifikasi',
          DimensionLabel: 'Klasifikasi',
          IsDefault: false,
        },
      ],
    }).mockResolvedValueOnce({
      EntityType: 'Item',
      DefaultRuleId: 'item-principal',
      Rules: [
        {
          RuleId: 'item-principal',
          DisplayLabel: 'Principal',
          DimensionLabel: 'Principal',
          IsDefault: true,
        },
        {
          RuleId: 'item-category',
          DisplayLabel: 'Category',
          DimensionLabel: 'Category',
          IsDefault: false,
        },
      ],
    })

    const store = useInvestigationWorkspaceStore()
    await store.initializeWorkspace('Customer', {
      peerGroupRuleId: 'customer-klasifikasi',
    })
    await store.setEntityType('Item')

    expect(store.peerGroupRuleId).toBe('item-principal')
    expect(store.showPeerGroupSelector).toBe(true)
  })
})
