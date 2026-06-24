import { ref } from 'vue'
import { defineStore } from 'pinia'
import {
  fetchEntityAnalyticsTypes,
  fetchEntityCompare,
  fetchEntityProfile,
  searchEntities,
} from '@/api/entityAnalyticsApi'
import { getApiErrorMessage } from '@/api/httpClient'
import type {
  EntityAnalyticsType,
  EntityCompareResponse,
  EntityPerformanceProfileResponse,
  EntitySearchResult,
} from '@/models/entityAnalytics'

export const useEntityAnalyticsStore = defineStore('entityAnalytics', () => {
  const types = ref<EntityAnalyticsType[]>([])
  const profile = ref<EntityPerformanceProfileResponse | null>(null)
  const compare = ref<EntityCompareResponse | null>(null)
  const loading = ref(false)
  const compareLoading = ref(false)
  const error = ref<string | null>(null)
  const compareError = ref<string | null>(null)

  async function loadTypes() {
    loading.value = true
    error.value = null
    try {
      const response = await fetchEntityAnalyticsTypes()
      types.value = response.Types ?? []
    } catch (err) {
      error.value = getApiErrorMessage(err, 'Failed to load entity analytics types')
      types.value = []
    } finally {
      loading.value = false
    }
  }

  async function loadProfile(entityType: string, entityId: string) {
    loading.value = true
    error.value = null
    try {
      profile.value = await fetchEntityProfile(entityType, entityId)
    } catch (err) {
      error.value = getApiErrorMessage(err, 'Failed to load entity performance profile')
      profile.value = null
    } finally {
      loading.value = false
    }
  }

  async function loadCompare(entityType: string, entityIds: string[]) {
    compareLoading.value = true
    compareError.value = null
    try {
      compare.value = await fetchEntityCompare({ entityType, entityIds })
    } catch (err) {
      compareError.value = getApiErrorMessage(err, 'Failed to load entity comparison')
      compare.value = null
    } finally {
      compareLoading.value = false
    }
  }

  async function searchEntityPicker(
    entityType: string,
    q: string,
  ): Promise<EntitySearchResult[]> {
    if (!q || q.trim().length < 2) {
      return []
    }

    try {
      return await searchEntities(entityType, q.trim())
    } catch {
      return []
    }
  }

  function reset() {
    types.value = []
    profile.value = null
    compare.value = null
    loading.value = false
    compareLoading.value = false
    error.value = null
    compareError.value = null
  }

  return {
    types,
    profile,
    compare,
    loading,
    compareLoading,
    error,
    compareError,
    loadTypes,
    loadProfile,
    loadCompare,
    searchEntityPicker,
    reset,
  }
})
