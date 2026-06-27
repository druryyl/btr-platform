<script setup lang="ts">
import { computed, ref } from 'vue'
import Checkbox from 'primevue/checkbox'
import ProfileSectionCard from '@/components/entity-analytics/ProfileSectionCard.vue'
import PerformanceSignatureChart from '@/components/entity-analytics/PerformanceSignatureChart.vue'
import PerformanceSignatureScoreTable from '@/components/entity-analytics/PerformanceSignatureScoreTable.vue'
import type { CompareRadarSection, ProfileRadarSection } from '@/models/entityAnalytics'
import type { SignatureSeries } from '@/components/entity-analytics/PerformanceSignatureChart.vue'

const props = defineProps<{
  section: ProfileRadarSection | CompareRadarSection | null | undefined
  loading?: boolean
  mode?: 'profile' | 'compare'
  entityLabel?: string | null
}>()

const showPeerAverage = ref(false)

const axes = computed(() => props.section?.Axes ?? [])

const series = computed<SignatureSeries[]>(() => {
  if (props.mode === 'compare') {
    const compareSection = props.section as CompareRadarSection | null | undefined
    return (compareSection?.Overlays ?? []).map((overlay, index) => ({
      label: overlay.DisplayName || overlay.EntityCode,
      scores: overlay.Scores,
      colorIndex: index,
    }))
  }

  const profileSection = props.section as ProfileRadarSection | null | undefined
  if (!profileSection?.Axes?.length) return []

  return [
    {
      label: props.entityLabel?.trim() || 'Score',
      scores: profileSection.Axes.map((axis) => axis.Score),
      colorIndex: 0,
    },
  ]
})

const peerAverage = computed(() => props.section?.PeerAverageScores ?? null)

const scoreColumnCount = computed(() => {
  let count = series.value.length
  if (showPeerAverage.value) count += 1
  return Math.max(count, 1)
})

const hasPeerAverage = computed(
  () => peerAverage.value != null && peerAverage.value.some((score) => score != null),
)
</script>

<template>
  <ProfileSectionCard
    title="Performance Signature"
    :is-available="section?.IsAvailable"
    :unavailable-reason="section?.UnavailableReason"
    :loading="loading"
  >
    <template v-if="section?.IsAvailable">
      <p v-if="section.PeriodLabel || section.PeerGroupSize" class="performance-signature-section__meta">
        <span v-if="section.PeriodLabel">{{ section.PeriodLabel }}</span>
        <span v-if="section.PeerGroupSize != null"> · Peer group: {{ section.PeerGroupSize }}</span>
      </p>

      <div class="performance-signature-section__main">
        <div class="performance-signature-section__chart-column">
          <PerformanceSignatureChart
            :axes="axes"
            :series="series"
            :peer-average="peerAverage"
            :show-peer-average="showPeerAverage"
          />

          <label v-if="hasPeerAverage" class="performance-signature-section__toggle">
            <Checkbox v-model="showPeerAverage" :binary="true" input-id="show-peer-average" />
            <span>Show Peer Average</span>
          </label>
        </div>

        <div
          class="performance-signature-section__table-column"
          :style="{ '--score-columns': scoreColumnCount }"
        >
          <PerformanceSignatureScoreTable
            :axes="axes"
            :series="series"
            :peer-average="peerAverage"
            :show-peer-average="showPeerAverage"
          />
        </div>
      </div>
    </template>

    <p
      v-else-if="(section as ProfileRadarSection | undefined)?.UnavailableExplanation"
      class="performance-signature-section__explanation"
    >
      {{ (section as ProfileRadarSection).UnavailableExplanation }}
    </p>
  </ProfileSectionCard>
</template>

<style scoped>
.performance-signature-section__meta {
  margin: 0 0 0.75rem;
  color: var(--p-text-muted-color);
  font-size: 0.875rem;
}

.performance-signature-section__main {
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
}

.performance-signature-section__chart-column {
  display: flex;
  flex-direction: column;
  align-items: center;
  width: 100%;
}

.performance-signature-section__table-column {
  --score-columns: 1;
  width: 100%;
}

.performance-signature-section__toggle {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  margin-top: 0.5rem;
  font-size: 0.875rem;
  color: var(--p-text-color);
  cursor: pointer;
  user-select: none;
}

@media (min-width: 56rem) {
  .performance-signature-section__main {
    display: grid;
    grid-template-columns: minmax(0, 1.35fr) minmax(0, 1fr);
    gap: 1.5rem 2rem;
    align-items: start;
  }

  .performance-signature-section__chart-column {
    align-items: center;
  }

  .performance-signature-section__table-column {
    padding-top: 0.25rem;
  }
}

.performance-signature-section__explanation {
  margin: 0;
  color: var(--p-text-muted-color);
}
</style>
