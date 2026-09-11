import { computed, ref, shallowRef } from 'vue'
import { defineStore } from 'pinia'
import axios from 'axios'
import {
  fetchEntityCompare,
  fetchEntityProfile,
  fetchMapPresets,
  fetchPeerGroupRules,
  fetchPopulationMap,
  POPULATION_MAP_TIMEOUT_MS,
} from '@/api/entityAnalyticsApi'
import { getApiErrorMessage } from '@/api/httpClient'
import type {
  EntityCompareResponse,
  EntityPerformanceProfileResponse,
  MapPreset,
  PeerGroupRule,
  PopulationMapPoint,
  PopulationMapResponse,
  WorkspaceSelectedEntity,
} from '@/models/entityAnalytics'
import { MAX_WORKSPACE_COMPARISON as MAX_COMPARISON } from '@/models/entityAnalytics'
import { getEntityDisplayLabel } from '@/navigation/entityAnalyticsNavigation'

export type WorkspaceMode = 'discovery' | 'investigation'

export interface WorkspaceHistoryEntry {
  entityType: string
  presetId: string | null
  selectedEntityIds: string[]
  dimensionFilter: string | null
  attentionOnly: boolean
  peerGroupRuleId: string | null
}

export const useInvestigationWorkspaceStore = defineStore('investigationWorkspace', () => {
  const entityType = ref('Customer')
  const presetId = ref<string | null>(null)
  const dimensionFilter = ref<string | null>(null)
  const attentionOnly = ref(false)
  const selectedEntityIds = ref<string[]>([])
  const peerGroupRuleId = ref<string | null>(null)

  const presets = ref<MapPreset[]>([])
  const peerGroupRules = ref<PeerGroupRule[]>([])
  const population = shallowRef<PopulationMapResponse | null>(null)
  const profiles = ref<Record<string, EntityPerformanceProfileResponse>>({})
  const compareBundle = ref<EntityCompareResponse | null>(null)

  const loadingPresets = ref(false)
  const loadingPopulation = ref(false)
  const loadingProfiles = ref(false)
  const error = ref<string | null>(null)

  const historyStack = ref<WorkspaceHistoryEntry[]>([])
  const undoStack = ref<WorkspaceHistoryEntry[]>([])

  /** Monotonic id so stale population responses never overwrite newer state. */
  let populationRequestId = 0
  let populationAbort: AbortController | null = null

  const expandedPanels = ref({
    peerPosition: true,
    trajectory: false,
    signalHistory: false,
    positionHistory: false,
    businessDrivers: false,
    validation: true,
  })

  const mode = computed<WorkspaceMode>(() =>
    selectedEntityIds.value.length > 0 ? 'investigation' : 'discovery',
  )

  const isComparisonMode = computed(() => selectedEntityIds.value.length > 1)

  const activePreset = computed(() =>
    presets.value.find((p) => p.PresetId === presetId.value)
    ?? presets.value.find((p) => p.IsDefault)
    ?? presets.value[0]
    ?? null,
  )

  const selectedEntities = computed<WorkspaceSelectedEntity[]>(() => {
    const points = population.value?.Points ?? []
    return selectedEntityIds.value.map((id) => {
      const point = points.find((p) => p.EntityId === id)
      return {
        EntityId: id,
        EntityCode: point?.EntityCode ?? id,
        DisplayName: point?.DisplayName ?? id,
      }
    })
  })

  const scopeLabel = computed(() => {
    if (!population.value) return null
    if (population.value.ActiveFilterDescription) return population.value.ActiveFilterDescription
    const total = population.value.TotalPopulationCount
    const label = getEntityDisplayLabel(entityType.value)
    return `Showing ${total} active ${label.toLowerCase()}s`
  })

  const showPeerGroupSelector = computed(
    () => (entityType.value === 'Customer' || entityType.value === 'Item')
      && peerGroupRules.value.length > 1,
  )

  function snapshotState(): WorkspaceHistoryEntry {
    return {
      entityType: entityType.value,
      presetId: presetId.value,
      selectedEntityIds: [...selectedEntityIds.value],
      dimensionFilter: dimensionFilter.value,
      attentionOnly: attentionOnly.value,
      peerGroupRuleId: peerGroupRuleId.value,
    }
  }

  function pushUndo() {
    undoStack.value.push(snapshotState())
    if (undoStack.value.length > 10) undoStack.value.shift()
  }

  function applySnapshot(entry: WorkspaceHistoryEntry) {
    entityType.value = entry.entityType
    presetId.value = entry.presetId
    selectedEntityIds.value = [...entry.selectedEntityIds]
    dimensionFilter.value = entry.dimensionFilter
    attentionOnly.value = entry.attentionOnly
    peerGroupRuleId.value = entry.peerGroupRuleId ?? null
  }

  async function loadPresets(type = entityType.value) {
    loadingPresets.value = true
    error.value = null
    try {
      const response = await fetchMapPresets(type)
      presets.value = response.Presets ?? []
      if (!presetId.value || !presets.value.some((p) => p.PresetId === presetId.value)) {
        presetId.value = presets.value.find((p) => p.IsDefault)?.PresetId
          ?? presets.value[0]?.PresetId
          ?? null
      }
    } catch (err) {
      error.value = getApiErrorMessage(err, 'Failed to load map presets')
      presets.value = []
    } finally {
      loadingPresets.value = false
    }
  }

  async function loadPeerGroupRules(type = entityType.value) {
    try {
      const response = await fetchPeerGroupRules(type)
      peerGroupRules.value = response.Rules ?? []
      if (
        peerGroupRuleId.value
        && !peerGroupRules.value.some((r) => r.RuleId === peerGroupRuleId.value)
      ) {
        peerGroupRuleId.value = null
      }
      if (!peerGroupRuleId.value && peerGroupRules.value.length > 1) {
        peerGroupRuleId.value = response.DefaultRuleId
          ?? peerGroupRules.value.find((r) => r.IsDefault)?.RuleId
          ?? peerGroupRules.value[0]?.RuleId
          ?? null
      }
      if (peerGroupRules.value.length <= 1) {
        peerGroupRuleId.value = null
      }
    } catch {
      peerGroupRules.value = []
      peerGroupRuleId.value = null
    }
  }

  async function loadPopulation() {
    if (!entityType.value) return

    const requestId = ++populationRequestId
    populationAbort?.abort()
    populationAbort = new AbortController()
    const { signal } = populationAbort

    loadingPopulation.value = true
    error.value = null
    try {
      const result = await fetchPopulationMap({
        entityType: entityType.value,
        presetId: presetId.value ?? undefined,
        dimensionFilter: dimensionFilter.value ?? undefined,
        attentionOnly: attentionOnly.value || undefined,
        signal,
        timeoutMs: POPULATION_MAP_TIMEOUT_MS,
      })
      if (requestId !== populationRequestId) return
      population.value = result
      if (result?.PresetId) {
        presetId.value = result.PresetId
      }
    } catch (err) {
      if (requestId !== populationRequestId) return
      if (axios.isAxiosError(err) && err.code === 'ERR_CANCELED') {
        return
      }
      error.value = getApiErrorMessage(err, 'Failed to load population map')
      population.value = null
    } finally {
      if (requestId === populationRequestId) {
        loadingPopulation.value = false
      }
    }
  }

  async function loadProfilesForSelection() {
    const ids = selectedEntityIds.value
    if (!ids.length) {
      profiles.value = {}
      compareBundle.value = null
      return
    }

    loadingProfiles.value = true
    try {
      const profileResults = await Promise.all(
        ids.map(async (id) => {
          const profile = await fetchEntityProfile(entityType.value, id)
          return [id, profile] as const
        }),
      )
      profiles.value = Object.fromEntries(profileResults)

      if (ids.length > 1) {
        compareBundle.value = await fetchEntityCompare({
          entityType: entityType.value,
          entityIds: ids,
        })
      } else {
        compareBundle.value = null
      }
    } catch (err) {
      error.value = getApiErrorMessage(err, 'Failed to load entity profiles')
    } finally {
      loadingProfiles.value = false
    }
  }

  async function initializeWorkspace(type: string, initial?: Partial<WorkspaceHistoryEntry>) {
    entityType.value = type
    if (initial?.presetId !== undefined) presetId.value = initial.presetId
    if (initial?.dimensionFilter !== undefined) dimensionFilter.value = initial.dimensionFilter
    if (initial?.attentionOnly !== undefined) attentionOnly.value = initial.attentionOnly
    if (initial?.peerGroupRuleId !== undefined) peerGroupRuleId.value = initial.peerGroupRuleId
    if (initial?.selectedEntityIds) selectedEntityIds.value = [...initial.selectedEntityIds]

    await loadPresets(type)
    await loadPeerGroupRules(type)
    await loadPopulation()
    if (selectedEntityIds.value.length) {
      await loadProfilesForSelection()
    }
  }

  async function setEntityType(type: string) {
    pushUndo()
    entityType.value = type
    selectedEntityIds.value = []
    profiles.value = {}
    dimensionFilter.value = null
    attentionOnly.value = false
    peerGroupRuleId.value = null
    await loadPresets(type)
    await loadPeerGroupRules(type)
    await loadPopulation()
  }

  async function setPreset(id: string) {
    if (presetId.value === id) return
    pushUndo()
    presetId.value = id
    await loadPopulation()
  }

  async function setDimensionFilter(value: string | null) {
    pushUndo()
    dimensionFilter.value = value
    await loadPopulation()
  }

  async function setAttentionOnly(value: boolean) {
    pushUndo()
    attentionOnly.value = value
    await loadPopulation()
  }

  function setPeerGroupRuleId(value: string | null) {
    if (peerGroupRuleId.value === value) return
    pushUndo()
    peerGroupRuleId.value = value
  }

  async function clearFilters() {
    pushUndo()
    dimensionFilter.value = null
    attentionOnly.value = false
    await loadPopulation()
  }

  function selectEntity(point: PopulationMapPoint) {
    const id = point.EntityId
    const existingIndex = selectedEntityIds.value.indexOf(id)

    pushUndo()

    if (existingIndex >= 0) {
      selectedEntityIds.value = selectedEntityIds.value.filter((eid) => eid !== id)
    } else if (selectedEntityIds.value.length >= MAX_COMPARISON) {
      return { limitReached: true, point }
    } else {
      selectedEntityIds.value = [...selectedEntityIds.value, id]
    }

    void loadProfilesForSelection()
    return { limitReached: false, point }
  }

  function removeEntity(id: string) {
    pushUndo()
    selectedEntityIds.value = selectedEntityIds.value.filter((eid) => eid !== id)
    const next = { ...profiles.value }
    delete next[id]
    profiles.value = next
  }

  function clearSelection() {
    pushUndo()
    selectedEntityIds.value = []
    profiles.value = {}
  }

  function undo() {
    const previous = undoStack.value.pop()
    if (!previous) return
    applySnapshot(previous)
    void loadPeerGroupRules(entityType.value)
    void loadPopulation()
    if (selectedEntityIds.value.length) void loadProfilesForSelection()
    else profiles.value = {}
  }

  function pushNavigationHistory() {
    historyStack.value.push(snapshotState())
  }

  function popNavigationHistory(): WorkspaceHistoryEntry | undefined {
    return historyStack.value.pop()
  }

  async function refresh() {
    await loadPopulation()
    if (selectedEntityIds.value.length) await loadProfilesForSelection()
  }

  return {
    entityType,
    presetId,
    dimensionFilter,
    attentionOnly,
    peerGroupRuleId,
    selectedEntityIds,
    presets,
    peerGroupRules,
    population,
    profiles,
    compareBundle,
    loadingPresets,
    loadingPopulation,
    loadingProfiles,
    error,
    expandedPanels,
    mode,
    isComparisonMode,
    activePreset,
    selectedEntities,
    scopeLabel,
    showPeerGroupSelector,
    initializeWorkspace,
    loadPresets,
    loadPeerGroupRules,
    loadPopulation,
    loadProfilesForSelection,
    setEntityType,
    setPreset,
    setDimensionFilter,
    setAttentionOnly,
    setPeerGroupRuleId,
    clearFilters,
    selectEntity,
    removeEntity,
    clearSelection,
    undo,
    pushNavigationHistory,
    popNavigationHistory,
    refresh,
  }
})
