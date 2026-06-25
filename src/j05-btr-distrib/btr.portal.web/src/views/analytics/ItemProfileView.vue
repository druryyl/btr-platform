<script setup lang="ts">
import { computed, onMounted, watch } from 'vue'
import { useRoute } from 'vue-router'
import EntityPerformanceProfileShell from '@/components/entity-analytics/EntityPerformanceProfileShell.vue'
import { useEntityAnalyticsStore } from '@/stores/entityAnalyticsStore'

const ENTITY_TYPE = 'Item'

const route = useRoute()
const store = useEntityAnalyticsStore()

const entityId = computed(() => String(route.params.brgCode ?? ''))

function loadCurrentProfile() {
  if (!entityId.value) return
  void store.loadProfile(ENTITY_TYPE, entityId.value)
}

onMounted(loadCurrentProfile)

watch(entityId, loadCurrentProfile)
</script>

<template>
  <EntityPerformanceProfileShell
    :profile="store.profile"
    :loading="store.loading"
    :error="store.error"
    :entity-code="entityId"
    @refresh="loadCurrentProfile()"
  />
</template>
