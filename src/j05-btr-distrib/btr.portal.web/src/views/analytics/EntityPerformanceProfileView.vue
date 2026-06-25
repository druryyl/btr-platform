<script setup lang="ts">
import { computed, onMounted, watch } from 'vue'
import { useRoute } from 'vue-router'
import EntityPerformanceProfileShell from '@/components/entity-analytics/EntityPerformanceProfileShell.vue'
import { useEntityAnalyticsStore } from '@/stores/entityAnalyticsStore'

const route = useRoute()
const store = useEntityAnalyticsStore()

const entityType = computed(() => String(route.params.entityType ?? ''))
const entityId = computed(() => String(route.params.entityId ?? ''))

function loadCurrentProfile() {
  if (!entityType.value || !entityId.value) return
  void store.loadProfile(entityType.value, entityId.value)
}

onMounted(loadCurrentProfile)

watch([entityType, entityId], loadCurrentProfile)
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
