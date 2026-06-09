<script setup lang="ts">
import type { DashboardAlertCenterCategorySummary } from '@/models/dashboard'

defineProps<{
  summaries: DashboardAlertCenterCategorySummary[]
}>()
</script>

<template>
  <section v-if="summaries.length > 0" class="alert-center-summary">
    <h2 class="alert-center-summary__heading">Alert Summary by Category</h2>
    <div class="alert-center-summary__badges">
      <span
        v-for="summary in summaries"
        :key="summary.Category"
        class="alert-center-summary__badge"
      >
        {{ summary.Category }} ({{ summary.TotalCount }}<template v-if="summary.HasMore"> · showing {{ summary.DisplayedCount }}</template>)
      </span>
    </div>
  </section>
</template>

<style scoped>
.alert-center-summary__heading {
  margin: 0 0 0.75rem;
  font-size: 1.125rem;
}

.alert-center-summary__badges {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.alert-center-summary__badge {
  display: inline-flex;
  align-items: center;
  padding: 0.35rem 0.75rem;
  border-radius: 999px;
  background: var(--p-content-hover-background);
  border: 1px solid var(--p-content-border-color);
  font-size: 0.875rem;
}
</style>
