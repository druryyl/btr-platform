<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Button from 'primevue/button'
import Select from 'primevue/select'
import Message from 'primevue/message'
import DashboardDetailLayout from '@/components/dashboard/DashboardDetailLayout.vue'
import WorkspaceBreadcrumb from '@/components/entity-analytics/workspace/WorkspaceBreadcrumb.vue'
import WorkspaceStageSection from '@/components/entity-analytics/workspace/WorkspaceStageSection.vue'
import MapPresetSelector from '@/components/entity-analytics/workspace/MapPresetSelector.vue'
import PopulationMapCanvas from '@/components/entity-analytics/workspace/PopulationMapCanvas.vue'
import PopulationFilterPanel from '@/components/entity-analytics/workspace/PopulationFilterPanel.vue'
import PopulationSearchField from '@/components/entity-analytics/workspace/PopulationSearchField.vue'
import ScopeIndicator from '@/components/entity-analytics/workspace/ScopeIndicator.vue'
import EntityIdentityPanel from '@/components/entity-analytics/workspace/EntityIdentityPanel.vue'
import WorkspaceKpiSummarySection from '@/components/entity-analytics/workspace/WorkspaceKpiSummarySection.vue'
import ComparisonLegend from '@/components/entity-analytics/workspace/ComparisonLegend.vue'
import PeerPositionPanel from '@/components/entity-analytics/workspace/PeerPositionPanel.vue'
import TrajectoryPanel from '@/components/entity-analytics/workspace/TrajectoryPanel.vue'
import SignalHistoryPanel from '@/components/entity-analytics/workspace/SignalHistoryPanel.vue'
import PositionHistoryPanel from '@/components/entity-analytics/workspace/PositionHistoryPanel.vue'
import BusinessDriversPanel from '@/components/entity-analytics/workspace/BusinessDriversPanel.vue'
import ValidationStagePanel from '@/components/entity-analytics/workspace/ValidationStagePanel.vue'
import ComparisonLimitDialog from '@/components/entity-analytics/workspace/ComparisonLimitDialog.vue'
import type { PopulationMapPoint } from '@/models/entityAnalytics'
import { buildWorkspaceQuery, parseWorkspaceUrlState } from '@/services/investigationWorkspaceUrl'
import { buildWorkspaceRoute } from '@/navigation/investigationWorkspaceNavigation'
import { useEntityAnalyticsStore } from '@/stores/entityAnalyticsStore'
import { useInvestigationWorkspaceStore } from '@/stores/investigationWorkspaceStore'

const ENTITY_TYPES = ['Customer', 'Salesman', 'Supplier', 'Item']

const route = useRoute()
const router = useRouter()
const workspace = useInvestigationWorkspaceStore()
const analyticsStore = useEntityAnalyticsStore()

const mapRef = ref<InstanceType<typeof PopulationMapCanvas> | null>(null)
const searchHighlightIds = ref<string[]>([])
const limitDialogVisible = ref(false)
const limitDialogPoint = ref<PopulationMapPoint | null>(null)

const entityTypeParam = computed(() => String(route.params.entityType ?? 'Customer'))

const entityTypeOptions = computed(() =>
  (analyticsStore.types.length ? analyticsStore.types : ENTITY_TYPES.map((t) => ({
    EntityType: t,
    DisplayName: t,
    IsEnabled: true,
    IsAvailable: true,
    ProfileRouteTemplate: '',
  }))).filter((t) => t.IsEnabled),
)

const isInvestigation = computed(() => workspace.mode === 'investigation')

const primaryKpiId = computed(
  () => workspace.activePreset?.AxisYKpiId ?? workspace.population?.AxisYKpiId ?? '',
)

const profilesActive = computed(() =>
  workspace.selectedEntityIds.map((id) => workspace.profiles[id]?.Overview?.IsActive ?? true),
)

function syncRoute() {
  router.replace({
    name: 'entity-analytics-workspace',
    params: { entityType: workspace.entityType },
    query: buildWorkspaceQuery({
      entityType: workspace.entityType,
      presetId: workspace.presetId,
      entityIds: workspace.selectedEntityIds,
      dimensionFilter: workspace.dimensionFilter,
      attentionOnly: workspace.attentionOnly,
    }),
  })
}

async function bootstrap() {
  const parsed = parseWorkspaceUrlState(entityTypeParam.value, route.query)
  await analyticsStore.loadTypes()
  await workspace.initializeWorkspace(parsed.entityType, {
    presetId: parsed.presetId,
    selectedEntityIds: parsed.entityIds,
    dimensionFilter: parsed.dimensionFilter,
    attentionOnly: parsed.attentionOnly,
  })
}

function onSelectPoint(point: PopulationMapPoint) {
  const result = workspace.selectEntity(point)
  if (result?.limitReached) {
    limitDialogPoint.value = result.point
    limitDialogVisible.value = true
    return
  }
  syncRoute()
}

function onSearchSelect(point: PopulationMapPoint) {
  const result = workspace.selectEntity(point)
  if (!result?.limitReached) syncRoute()
}

function onEntityTypeChange(type: string) {
  void workspace.setEntityType(type).then(() => {
    router.replace(buildWorkspaceRoute(type))
  })
}

function onPresetChange(presetId: string) {
  void workspace.setPreset(presetId).then(syncRoute)
}

function onFilterChange(filter: string | null) {
  void workspace.setDimensionFilter(filter).then(syncRoute)
}

function onAttentionChange(value: boolean) {
  void workspace.setAttentionOnly(value).then(syncRoute)
}

function clearFilters() {
  void workspace.clearFilters().then(syncRoute)
}

function clearSelection() {
  workspace.clearSelection()
  syncRoute()
}

function removeEntity(id: string) {
  workspace.removeEntity(id)
  syncRoute()
}

async function navigateRelated(payload: { entityType: string; entityId: string }) {
  workspace.pushNavigationHistory()
  await router.push(
    buildWorkspaceRoute(payload.entityType, {
      entityIds: [payload.entityId],
      presetId: workspace.presetId,
    }),
  )
}

function onKeydown(event: KeyboardEvent) {
  if (event.target instanceof HTMLInputElement || event.target instanceof HTMLTextAreaElement) return
  if (event.key === 'Escape') {
    clearSelection()
  } else if (event.key === 'f' || event.key === 'F') {
    // filter panel is always visible in toolbar
  } else if (event.key === 'z' || event.key === 'Z') {
    mapRef.value?.resetZoom()
  } else if (event.key === 'c' || event.key === 'C') {
    clearFilters()
  } else if (event.key === 'Backspace' && workspace.selectedEntityIds.length) {
    const last = workspace.selectedEntityIds[workspace.selectedEntityIds.length - 1]
    removeEntity(last)
  } else if ((event.ctrlKey || event.metaKey) && event.key === 'z') {
    workspace.undo()
    syncRoute()
  }
}

onMounted(() => {
  void bootstrap()
  window.addEventListener('keydown', onKeydown)
})

onUnmounted(() => {
  window.removeEventListener('keydown', onKeydown)
})

watch(
  () => route.params.entityType,
  (type) => {
    if (type && type !== workspace.entityType) {
      void workspace.setEntityType(String(type)).then(syncRoute)
    }
  },
)
</script>

<template>
  <DashboardDetailLayout
    title="Investigation Workspace"
    :subtitle="`${workspace.entityType} population investigation`"
    :loading="workspace.loadingPopulation && !workspace.population"
    :error="workspace.error"
    :generated-at="workspace.population?.GeneratedAt"
    @refresh="workspace.refresh()"
  >
    <div class="iw-workspace">
      <WorkspaceBreadcrumb
        :entity-type="workspace.entityType"
        :preset="workspace.activePreset"
        :selected-count="workspace.selectedEntityIds.length"
        :selected-label="workspace.selectedEntities[0]?.DisplayName"
      />

      <WorkspaceStageSection title="Population Map">
        <div class="iw-map-toolbar">
          <Select
            :model-value="workspace.entityType"
            :options="entityTypeOptions"
            option-label="DisplayName"
            option-value="EntityType"
            placeholder="Entity type"
            @update:model-value="onEntityTypeChange"
          />
          <MapPresetSelector
            v-if="workspace.presets.length"
            :presets="workspace.presets"
            :model-value="workspace.presetId"
            @update:model-value="onPresetChange"
          />
          <PopulationSearchField
            :points="workspace.population?.Points ?? []"
            @highlight="searchHighlightIds = $event"
            @select="onSearchSelect"
          />
          <PopulationFilterPanel
            :points="workspace.population?.Points ?? []"
            :dimension-filter="workspace.dimensionFilter"
            :attention-only="workspace.attentionOnly"
            @update:dimension-filter="onFilterChange"
            @update:attention-only="onAttentionChange"
            @clear="clearFilters"
          />
          <Button
            v-if="workspace.selectedEntityIds.length"
            label="Clear selection"
            text
            size="small"
            @click="clearSelection"
          />
          <Button
            v-if="workspace.selectedEntityIds.length"
            label="Locate selected"
            text
            size="small"
            @click="mapRef?.locateSelected()"
          />
          <Button label="Reset zoom" text size="small" @click="mapRef?.resetZoom()" />
        </div>

        <ScopeIndicator
          :label="workspace.scopeLabel"
          :generated-at="workspace.population?.GeneratedAt"
        />

        <div
          class="iw-map-shell"
          :class="isInvestigation ? 'iw-map-shell--investigation' : 'iw-map-shell--discovery'"
        >
          <PopulationMapCanvas
            ref="mapRef"
            :population="workspace.population"
            :selected-entity-ids="workspace.selectedEntityIds"
            :search-highlight-ids="searchHighlightIds"
            :investigation-mode="isInvestigation"
            :loading="workspace.loadingPopulation"
            @select="onSelectPoint"
          />
        </div>
      </WorkspaceStageSection>

      <template v-if="isInvestigation">
        <WorkspaceStageSection title="Current Facts">
          <EntityIdentityPanel
            :entities="workspace.selectedEntities"
            :profiles-active="profilesActive"
          />
          <ComparisonLegend
            v-if="workspace.isComparisonMode"
            :entities="workspace.selectedEntities"
            @remove="removeEntity"
          />
          <WorkspaceKpiSummarySection
            :profiles="workspace.profiles"
            :entity-ids="workspace.selectedEntityIds"
            :loading="workspace.loadingProfiles"
          />
        </WorkspaceStageSection>

        <WorkspaceStageSection title="Context">
          <PeerPositionPanel
            v-if="primaryKpiId"
            :entity-type="workspace.entityType"
            :entity-ids="workspace.selectedEntityIds"
            :kpi-id="primaryKpiId"
            :dimension-filter="workspace.dimensionFilter"
          />
          <TrajectoryPanel
            :entity-ids="workspace.selectedEntityIds"
            :profiles="workspace.profiles"
            :compare-bundle="workspace.compareBundle"
            :loading="workspace.loadingProfiles"
          />
          <SignalHistoryPanel
            :entity-ids="workspace.selectedEntityIds"
            :profiles="workspace.profiles"
            :compare-bundle="workspace.compareBundle"
            :loading="workspace.loadingProfiles"
          />
          <PositionHistoryPanel
            :entity-ids="workspace.selectedEntityIds"
            :profiles="workspace.profiles"
            :compare-bundle="workspace.compareBundle"
            :loading="workspace.loadingProfiles"
          />
        </WorkspaceStageSection>

        <WorkspaceStageSection title="Explanation">
          <BusinessDriversPanel
            :entity-ids="workspace.selectedEntityIds"
            :profiles="workspace.profiles"
            :loading="workspace.loadingProfiles"
            @navigate-entity="navigateRelated"
          />
        </WorkspaceStageSection>

        <WorkspaceStageSection title="Validation">
          <ValidationStagePanel
            :entity-ids="workspace.selectedEntityIds"
            :profiles="workspace.profiles"
            :loading="workspace.loadingProfiles"
          />
          <Message severity="info" :closable="false" class="iw-completeness">
            You have reviewed Population Map and Current Facts. Business Drivers and Evidence are
            available to complete the investigation.
          </Message>
        </WorkspaceStageSection>
      </template>
    </div>

    <ComparisonLimitDialog
      :visible="limitDialogVisible"
      :point="limitDialogPoint"
      @dismiss="limitDialogVisible = false"
    />
  </DashboardDetailLayout>
</template>

<style scoped>
.iw-completeness {
  margin-top: 0.75rem;
}
</style>
