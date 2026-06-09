<script setup lang="ts">
import { computed } from 'vue'
import AgingPieChart from '@/components/dashboard/AgingPieChart.vue'
import type { DashboardCollectionAgingBucket } from '@/models/dashboard'

const props = defineProps<{
  buckets: DashboardCollectionAgingBucket[]
  loading: boolean
}>()

const chartBuckets = computed(() =>
  props.buckets.map((b) => ({
    BucketKey: b.BucketKey,
    BucketLabel: b.BucketLabel,
    Amount: b.Amount,
    SortOrder: b.SortOrder,
  })),
)
</script>

<template>
  <section class="collection-aging">
    <AgingPieChart
      :buckets="chartBuckets"
      :loading="loading"
      title="Aging Risk Summary (Overdue Only)"
      empty-message="No overdue exposure."
    />
  </section>
</template>
