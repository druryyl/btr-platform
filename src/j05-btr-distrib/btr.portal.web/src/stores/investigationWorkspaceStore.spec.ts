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
  fetchInvestigationLenses: vi.fn(),
  fetchEntityProfile: vi.fn(),
  fetchEntityCompare: vi.fn(),
  fetchEntityDataHealth: vi.fn(),
}))

import {
  fetchEntityDataHealth,
  fetchInvestigationLenses,
  fetchMapPresets,
  fetchPeerGroupRules,
  fetchPopulationMap,
} from '@/api/entityAnalyticsApi'

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

const SUPPLIER_LENSES = {
  EntityType: 'Supplier',
  DefaultLensId: 'sales-out',
  Lenses: [
    {
      LensId: 'sales-out',
      DisplayName: 'Sales-Out',
      IsDefault: true,
      DefaultPresetId: 'principal-sales-out-map',
      KpiIds: [],
      DerivedMetricIds: [],
      AttentionCategories: [],
      RelationshipDrivers: [],
      EvidenceRoutes: [],
    },
    {
      LensId: 'purchasing',
      DisplayName: 'Purchasing',
      IsDefault: false,
      DefaultPresetId: 'purchase-exposure-map',
      KpiIds: [],
      DerivedMetricIds: ['purchase-to-sales-out-ratio'],
      AttentionCategories: [],
      RelationshipDrivers: [],
      EvidenceRoutes: [],
    },
  ],
}

describe('investigationWorkspaceStore investigation lenses', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.mocked(fetchPopulationMap).mockReset()
    vi.mocked(fetchPeerGroupRules).mockReset()
    vi.mocked(fetchMapPresets).mockReset()
    vi.mocked(fetchInvestigationLenses).mockReset()

    vi.mocked(fetchPopulationMap).mockImplementation(
      async (params: { presetId?: string }) =>
        makePopulationResponse({ PresetId: params.presetId ?? 'default' }) as never,
    )
    vi.mocked(fetchInvestigationLenses).mockResolvedValue(SUPPLIER_LENSES as never)
    vi.mocked(fetchPeerGroupRules).mockResolvedValue({
      EntityType: 'Supplier',
      DefaultRuleId: 'supplier-all-active',
      Rules: [
        {
          RuleId: 'supplier-all-active',
          DisplayLabel: 'All Active',
          DimensionLabel: null,
          IsDefault: true,
        },
        {
          RuleId: 'supplier-wilayah',
          DisplayLabel: 'Wilayah',
          DimensionLabel: 'Wilayah',
          IsDefault: false,
        },
      ],
    } as never)
    vi.mocked(fetchMapPresets).mockResolvedValue({
      EntityType: 'Supplier',
      Presets: [
        {
          PresetId: 'principal-sales-out-map',
          DisplayName: 'Principal Sales-Out Map',
          Description: '',
          AxisXKpiId: 'PRN-TGT-003',
          AxisYKpiId: 'PRN-GRW-002',
          AxisXLabel: 'Achievement %',
          AxisYLabel: 'YoY Growth %',
          IsDefault: false,
        },
        {
          PresetId: 'purchase-exposure-map',
          DisplayName: 'Purchase Exposure Map',
          Description: '',
          AxisXKpiId: 'PU-KPI-001',
          AxisYKpiId: 'PRN-INV-001',
          AxisXLabel: 'Purchase',
          AxisYLabel: 'Inventory',
          IsDefault: true,
        },
      ],
    } as never)
  })

  it('defaults the workspace to the Sales-Out lens and its default preset', async () => {
    const store = useInvestigationWorkspaceStore()
    await store.initializeWorkspace('Supplier')

    expect(store.lenses).toHaveLength(2)
    expect(store.hasLensSwitcher).toBe(true)
    expect(store.activeLensId).toBe('sales-out')
    expect(store.presetId).toBe('principal-sales-out-map')
    expect(store.activeLensDerivedMetricIds).toEqual([])
  })

  it('switching lens selects the lens default preset and reloads the population map', async () => {
    const store = useInvestigationWorkspaceStore()
    await store.initializeWorkspace('Supplier')
    vi.mocked(fetchPopulationMap).mockClear()

    await store.setLens('purchasing')

    expect(store.activeLensId).toBe('purchasing')
    expect(store.presetId).toBe('purchase-exposure-map')
    expect(store.activeLensDerivedMetricIds).toEqual(['purchase-to-sales-out-ratio'])
    expect(fetchPopulationMap).toHaveBeenCalledTimes(1)
    expect(fetchPopulationMap).toHaveBeenCalledWith(
      expect.objectContaining({ presetId: 'purchase-exposure-map' }),
    )
  })

  it('keeps the Peer Group Selector hidden for the Principal entity type', async () => {
    const store = useInvestigationWorkspaceStore()
    await store.initializeWorkspace('Supplier')

    expect(store.peerGroupRules.length).toBeGreaterThan(1)
    expect(store.showPeerGroupSelector).toBe(false)
  })
})

function makeDataHealthResponse(overrides: Record<string, unknown> = {}) {
  return {
    IsAvailable: true,
    EntityType: 'Supplier',
    PeriodYear: 2026,
    PeriodMonth: 6,
    GeneratedAt: '2026-06-30T10:00:00Z',
    TargetCoveragePercentage: 66.67,
    PrincipalsMissingTargetCount: 9,
    UnknownPrincipalExceptionCount: 3,
    UnknownPrincipalExceptionAmount: 150_000_000,
    ...overrides,
  }
}

describe('investigationWorkspaceStore data health', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.mocked(fetchPopulationMap).mockReset()
    vi.mocked(fetchPeerGroupRules).mockReset()
    vi.mocked(fetchMapPresets).mockReset()
    vi.mocked(fetchInvestigationLenses).mockReset()
    vi.mocked(fetchEntityDataHealth).mockReset()

    vi.mocked(fetchPopulationMap).mockResolvedValue(makePopulationResponse() as never)
    vi.mocked(fetchInvestigationLenses).mockResolvedValue(SUPPLIER_LENSES as never)
    vi.mocked(fetchPeerGroupRules).mockResolvedValue({
      EntityType: 'Supplier',
      DefaultRuleId: 'supplier-all-active',
      Rules: [
        {
          RuleId: 'supplier-all-active',
          DisplayLabel: 'All Active',
          DimensionLabel: null,
          IsDefault: true,
        },
      ],
    } as never)
    vi.mocked(fetchMapPresets).mockResolvedValue({
      EntityType: 'Supplier',
      Presets: [
        {
          PresetId: 'principal-sales-out-map',
          DisplayName: 'Principal Sales-Out Map',
          Description: '',
          AxisXKpiId: 'PRN-TGT-003',
          AxisYKpiId: 'PRN-GRW-002',
          AxisXLabel: 'Achievement %',
          AxisYLabel: 'YoY Growth %',
          IsDefault: false,
        },
        {
          PresetId: 'purchase-exposure-map',
          DisplayName: 'Purchase Exposure Map',
          Description: '',
          AxisXKpiId: 'PU-KPI-001',
          AxisYKpiId: 'PRN-INV-001',
          AxisXLabel: 'Purchase',
          AxisYLabel: 'Inventory',
          IsDefault: true,
        },
      ],
    } as never)
  })

  it('loads the lens-independent Data Health disclosures for the Principal', async () => {
    vi.mocked(fetchEntityDataHealth).mockResolvedValue(makeDataHealthResponse() as never)
    const store = useInvestigationWorkspaceStore()

    await store.initializeWorkspace('Supplier')

    expect(fetchEntityDataHealth).toHaveBeenCalledWith('Supplier')
    expect(store.dataHealth?.IsAvailable).toBe(true)
    expect(store.dataHealth?.TargetCoveragePercentage).toBe(66.67)
    expect(store.dataHealth?.PrincipalsMissingTargetCount).toBe(9)
    expect(store.dataHealth?.UnknownPrincipalExceptionCount).toBe(3)
    expect(store.dataHealth?.UnknownPrincipalExceptionAmount).toBe(150_000_000)
  })

  it('clears Data Health state when the load fails (non-blocking disclosure)', async () => {
    vi.mocked(fetchEntityDataHealth).mockRejectedValue(new Error('network down'))
    const store = useInvestigationWorkspaceStore()

    await store.initializeWorkspace('Supplier')

    expect(store.dataHealth).toBeNull()
    expect(store.loadingDataHealth).toBe(false)
  })

  it('reloads Data Health when switching entity type', async () => {
    vi.mocked(fetchEntityDataHealth)
      .mockResolvedValueOnce(makeDataHealthResponse() as never)
      .mockResolvedValueOnce(
        makeDataHealthResponse({ EntityType: 'Customer', IsAvailable: false }) as never,
      )
    const store = useInvestigationWorkspaceStore()

    await store.initializeWorkspace('Supplier')
    await store.setEntityType('Customer')

    expect(fetchEntityDataHealth).toHaveBeenCalledTimes(2)
    expect(store.dataHealth?.IsAvailable).toBe(false)
  })

  it('never reloads Data Health on a lens switch (section is lens-independent)', async () => {
    vi.mocked(fetchEntityDataHealth).mockResolvedValue(makeDataHealthResponse() as never)
    const store = useInvestigationWorkspaceStore()

    await store.initializeWorkspace('Supplier')
    vi.mocked(fetchEntityDataHealth).mockClear()

    await store.setLens('purchasing')

    expect(fetchEntityDataHealth).not.toHaveBeenCalled()
  })
})
