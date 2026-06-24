<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Button from 'primevue/button'
import Message from 'primevue/message'
import DashboardDetailLayout from '@/components/dashboard/DashboardDetailLayout.vue'
import EntityPicker from '@/components/entity-analytics/EntityPicker.vue'
import EntityCompareTable from '@/components/entity-analytics/EntityCompareTable.vue'
import CompareTrendSection from '@/components/entity-analytics/CompareTrendSection.vue'
import CompareRankingSection from '@/components/entity-analytics/CompareRankingSection.vue'
import CompareAttentionSection from '@/components/entity-analytics/CompareAttentionSection.vue'
import CompareRelationshipsSection from '@/components/entity-analytics/CompareRelationshipsSection.vue'
import RadarCompareSection from '@/components/entity-analytics/RadarCompareSection.vue'
import type { EntitySearchResult } from '@/models/entityAnalytics'
import { useEntityAnalyticsStore } from '@/stores/entityAnalyticsStore'

const ENTITY_TYPE = 'Customer'
const MAX_ENTITIES = 5
const MIN_ENTITIES = 2

const route = useRoute()
const router = useRouter()
const store = useEntityAnalyticsStore()

const slots = ref<Array<EntitySearchResult | null>>([null, null])

const selectedCodes = computed(() =>
  slots.value
    .filter((slot): slot is EntitySearchResult => slot != null)
    .map((slot) => slot.EntityCode),
)

const canCompare = computed(
  () => selectedCodes.value.length >= MIN_ENTITIES && selectedCodes.value.length <= MAX_ENTITIES,
)

function addSlot() {
  if (slots.value.length < MAX_ENTITIES) {
    slots.value.push(null)
  }
}

function removeSlot(index: number) {
  if (slots.value.length > MIN_ENTITIES) {
    slots.value.splice(index, 1)
  }
}

function syncRouteQuery() {
  const entities = selectedCodes.value.join(',')
  router.replace({
    query: entities ? { entities } : {},
  })
}

async function runCompare() {
  if (!canCompare.value) return
  syncRouteQuery()
  await store.loadCompare(ENTITY_TYPE, selectedCodes.value)
}

function applyEntitiesFromQuery() {
  const raw = route.query.entities
  const codes = typeof raw === 'string'
    ? raw.split(',').map((code) => code.trim()).filter(Boolean)
    : []

  if (codes.length === 0) return

  slots.value = codes.slice(0, MAX_ENTITIES).map((code) => ({
    EntityType: ENTITY_TYPE,
    EntityId: code,
    EntityCode: code,
    DisplayName: code,
    IsActive: true,
    ProfileRoute: `/analytics/customers/${code}`,
  }))

  while (slots.value.length < MIN_ENTITIES) {
    slots.value.push(null)
  }
}

onMounted(async () => {
  applyEntitiesFromQuery()
  if (canCompare.value) {
    await runCompare()
  }
})

watch(
  () => route.query.entities,
  async () => {
    applyEntitiesFromQuery()
    if (canCompare.value) {
      await runCompare()
    }
  },
)
</script>

<template>
  <DashboardDetailLayout
    title="Compare Customers"
    subtitle="Side-by-side KPI, trend, ranking, attention, and relationship comparison"
    :loading="store.compareLoading"
    :error="store.compareError"
    @refresh="runCompare()"
  >
    <section class="customer-compare-view__pickers">
      <div
        v-for="(_slot, index) in slots"
        :key="index"
        class="customer-compare-view__picker-row"
      >
        <label>Entity {{ index + 1 }}</label>
        <EntityPicker
          v-model="slots[index]"
          :entity-type="ENTITY_TYPE"
          :placeholder="`Customer ${index + 1}`"
        />
        <Button
          v-if="slots.length > MIN_ENTITIES"
          icon="pi pi-times"
          text
          rounded
          severity="secondary"
          aria-label="Remove entity slot"
          @click="removeSlot(index)"
        />
      </div>

      <div class="customer-compare-view__actions">
        <Button
          v-if="slots.length < MAX_ENTITIES"
          label="Add entity"
          icon="pi pi-plus"
          outlined
          @click="addSlot"
        />
        <Button
          label="Compare"
          icon="pi pi-chart-bar"
          :disabled="!canCompare"
          :loading="store.compareLoading"
          @click="runCompare()"
        />
      </div>
    </section>

    <Message
      v-if="store.compare?.Warnings?.includes('GeneratedAtMismatch')"
      severity="warn"
      :closable="false"
      class="customer-compare-view__warning"
    >
      Compared entities were refreshed at different times. Snapshot timestamps may not align.
    </Message>

    <div v-if="store.compare" class="customer-compare-view__sections">
      <EntityCompareTable :section="store.compare.KpiComparison" :loading="store.compareLoading" />
      <CompareTrendSection :section="store.compare.TrendComparison" :loading="store.compareLoading" />
      <CompareRankingSection
        :section="store.compare.RankingComparison"
        :loading="store.compareLoading"
      />
      <CompareAttentionSection
        :section="store.compare.AttentionComparison"
        :loading="store.compareLoading"
      />
      <CompareRelationshipsSection
        :section="store.compare.RelationshipComparison"
        :loading="store.compareLoading"
      />
      <RadarCompareSection :section="store.compare.RadarComparison" />
    </div>
  </DashboardDetailLayout>
</template>

<style scoped>
.customer-compare-view__pickers {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  margin-bottom: 1.5rem;
}

.customer-compare-view__picker-row {
  display: grid;
  grid-template-columns: 6rem 1fr auto;
  gap: 0.75rem;
  align-items: center;
}

.customer-compare-view__picker-row label {
  font-size: 0.875rem;
  color: var(--p-text-muted-color, #64748b);
}

.customer-compare-view__actions {
  display: flex;
  gap: 0.75rem;
  margin-top: 0.5rem;
}

.customer-compare-view__warning {
  margin-bottom: 1rem;
}

.customer-compare-view__sections {
  display: flex;
  flex-direction: column;
  gap: 0;
}
</style>
