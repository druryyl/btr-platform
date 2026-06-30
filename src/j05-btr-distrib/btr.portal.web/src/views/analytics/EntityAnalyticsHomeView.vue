<script setup lang="ts">
import { computed, onMounted, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { RouterLink } from 'vue-router'
import Button from 'primevue/button'
import Card from 'primevue/card'
import Tag from 'primevue/tag'
import DashboardDetailLayout from '@/components/dashboard/DashboardDetailLayout.vue'
import EntityPicker from '@/components/entity-analytics/EntityPicker.vue'
import type { EntitySearchResult } from '@/models/entityAnalytics'
import { buildCompareRoute, getEntityAnalyticsNav } from '@/navigation/entityAnalyticsNavigation'
import { buildWorkspaceRoute } from '@/navigation/investigationWorkspaceNavigation'
import { useEntityAnalyticsStore } from '@/stores/entityAnalyticsStore'

const router = useRouter()
const store = useEntityAnalyticsStore()

const selectedByType = reactive<Record<string, EntitySearchResult | null>>({})

const enabledTypes = computed(() =>
  store.types.filter((type) => type.IsEnabled && getEntityAnalyticsNav(type.EntityType)),
)

function selectionFor(entityType: string): EntitySearchResult | null {
  return selectedByType[entityType] ?? null
}

function setSelection(entityType: string, value: EntitySearchResult | null): void {
  selectedByType[entityType] = value
}

function openProfile(entityType: string): void {
  const selected = selectionFor(entityType)
  if (!selected?.ProfileRoute) return
  void router.push(selected.ProfileRoute)
}

function compareRoute(entityType: string) {
  const selected = selectionFor(entityType)
  return buildCompareRoute(entityType, selected?.EntityId)
}

function navFor(entityType: string) {
  return getEntityAnalyticsNav(entityType)
}

onMounted(() => {
  void store.loadTypes()
})
</script>

<template>
  <DashboardDetailLayout
    title="Entity Analytics"
    subtitle="Performance profiles, peer comparison, and cross-domain KPI investigation"
    :loading="store.loading"
    :error="store.error"
    @refresh="store.loadTypes()"
  >
    <p class="entity-analytics-home__intro">
      Start with the Investigation Workspace to explore the full population, identify outliers, and
      investigate entities in context. Legacy Performance Profile and Compare remain available below.
    </p>

    <div v-if="enabledTypes.length" class="entity-analytics-home__workspace-row">
      <Card
        v-for="entityType in enabledTypes"
        :key="`ws-${entityType.EntityType}`"
        class="entity-analytics-home__workspace-card"
      >
        <template #title>{{ entityType.DisplayName }} Investigation</template>
        <template #content>
          <p class="entity-analytics-home__workspace-copy">
            Open the {{ entityType.DisplayName }} Population Map to discover and investigate entities.
          </p>
          <RouterLink
            v-slot="{ navigate }"
            :to="buildWorkspaceRoute(entityType.EntityType)"
            custom
          >
            <Button
              label="Open Investigation Workspace"
              icon="pi pi-map"
              @click="navigate"
            />
          </RouterLink>
        </template>
      </Card>
    </div>

    <h2 v-if="enabledTypes.length" class="entity-analytics-home__legacy-title">Legacy entry points</h2>
    <div v-if="enabledTypes.length" class="entity-analytics-home__grid entity-analytics-home__grid--legacy">
      <Card
        v-for="entityType in enabledTypes"
        :key="entityType.EntityType"
        class="entity-analytics-home__card"
      >
        <template #title>
          <div class="entity-analytics-home__card-title">
            <span>{{ entityType.DisplayName }}</span>
            <Tag
              v-if="entityType.IsAvailable"
              value="Data available"
              severity="success"
              class="entity-analytics-home__tag"
            />
            <Tag
              v-else
              value="Awaiting snapshot"
              severity="secondary"
              class="entity-analytics-home__tag"
            />
          </div>
        </template>

        <template #content>
          <span class="entity-analytics-home__label">
            Search {{ navFor(entityType.EntityType)?.pluralLabel ?? entityType.DisplayName }}
          </span>
          <EntityPicker
            :model-value="selectionFor(entityType.EntityType)"
            :entity-type="entityType.EntityType"
            :placeholder="`Search by code or name`"
            @update:model-value="setSelection(entityType.EntityType, $event)"
          />

          <div class="entity-analytics-home__actions">
            <Button
              label="Open Profile"
              icon="pi pi-id-card"
              :disabled="!selectionFor(entityType.EntityType)"
              @click="openProfile(entityType.EntityType)"
            />
            <RouterLink
              v-if="navFor(entityType.EntityType)"
              v-slot="{ navigate }"
              :to="compareRoute(entityType.EntityType)"
              custom
            >
              <Button
                :label="`Compare ${navFor(entityType.EntityType)!.pluralLabel}`"
                icon="pi pi-chart-bar"
                outlined
                severity="secondary"
                @click="navigate"
              />
            </RouterLink>
          </div>
        </template>
      </Card>
    </div>

    <p v-else-if="!store.loading && !store.error" class="entity-analytics-home__empty">
      No entity types are available. Ensure the portal API is running with Entity Analytics
      registrars loaded and set
      <code>EntityAnalytics:EnabledEntityTypes</code>
      in
      <code>btr.portal.api/appsettings.json</code>
      (or your machine override). Snapshot data is populated by the dashboard worker.
    </p>
  </DashboardDetailLayout>
</template>

<style scoped>
.entity-analytics-home__intro {
  margin: 0 0 1.5rem;
  color: var(--p-text-muted-color);
  max-width: 48rem;
}

.entity-analytics-home__workspace-row {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(16rem, 1fr));
  gap: 1rem;
  margin-bottom: 2rem;
}

.entity-analytics-home__workspace-copy {
  margin: 0 0 1rem;
  color: var(--p-text-muted-color);
  font-size: 0.875rem;
}

.entity-analytics-home__legacy-title {
  margin: 0 0 1rem;
  font-size: 1rem;
  font-weight: 600;
}

.entity-analytics-home__grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(18rem, 1fr));
  gap: 1rem;
}

.entity-analytics-home__card-title {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 0.5rem;
}

.entity-analytics-home__tag {
  font-size: 0.75rem;
}

.entity-analytics-home__label {
  display: block;
  margin-bottom: 0.375rem;
  font-size: 0.875rem;
  color: var(--p-text-muted-color);
}

.entity-analytics-home__actions {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
  margin-top: 1rem;
}

.entity-analytics-home__empty {
  margin: 0;
  color: var(--p-text-muted-color);
}
</style>
