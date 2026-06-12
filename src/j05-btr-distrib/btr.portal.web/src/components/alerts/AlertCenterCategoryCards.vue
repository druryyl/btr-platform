<script setup lang="ts">
import { computed } from 'vue'
import KpiCard from '@/components/KpiCard.vue'
import type { DashboardAlertCenterCategorySummary } from '@/models/dashboard'
import {
  ALERT_CENTER_CATEGORY_ICONS,
  type AlertCenterCategory,
  sortCategorySummaries,
} from '@/services/alertCenterPrioritization'

const props = defineProps<{
  summaries: DashboardAlertCenterCategorySummary[]
  loading: boolean
}>()

const emit = defineEmits<{
  select: [category: string]
}>()

const sortedSummaries = computed(() => sortCategorySummaries(props.summaries))

function categoryIcon(category: string): string {
  return ALERT_CENTER_CATEGORY_ICONS[category as AlertCenterCategory] ?? 'pi pi-bell'
}

function subtitle(summary: DashboardAlertCenterCategorySummary): string {
  if (summary.TotalCount === 0) {
    return 'No alerts'
  }

  if (summary.HasMore) {
    return `Showing ${summary.DisplayedCount} of ${summary.TotalCount}`
  }

  return `${summary.TotalCount} alert${summary.TotalCount === 1 ? '' : 's'}`
}

function onSelect(category: string): void {
  emit('select', category)
}
</script>

<template>
  <section id="alert-category-summary" class="alert-center-category-cards">
    <h2 class="alert-center-category-cards__heading">Category Attention</h2>
    <div class="alert-center-category-cards__grid">
      <button
        v-for="summary in sortedSummaries"
        :key="summary.Category"
        type="button"
        class="alert-center-category-cards__card"
        :class="{
          'alert-center-category-cards__card--attention': summary.TotalCount > 0,
        }"
        :aria-label="`${summary.Category}: ${summary.TotalCount} alerts`"
        @click="onSelect(summary.Category)"
      >
        <KpiCard
          :title="summary.Category"
          :icon="categoryIcon(summary.Category)"
          :loading="loading"
          class="alert-center-category-cards__kpi"
        >
          <div class="alert-center-category-cards__count">
            {{ loading ? '—' : summary.TotalCount }}
          </div>
          <div class="alert-center-category-cards__subtitle">
            {{ loading ? 'Loading…' : subtitle(summary) }}
          </div>
        </KpiCard>
      </button>
    </div>
  </section>
</template>

<style scoped>
.alert-center-category-cards__heading {
  margin: 0 0 0.75rem;
  font-size: 1.125rem;
}

.alert-center-category-cards__grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(160px, 1fr));
  gap: 1rem;
}

.alert-center-category-cards__card {
  display: block;
  width: 100%;
  padding: 0;
  border: none;
  background: none;
  text-align: inherit;
  cursor: pointer;
  border-radius: var(--p-content-border-radius);
}

.alert-center-category-cards__card:focus-visible {
  outline: 2px solid var(--p-primary-color);
  outline-offset: 2px;
}

.alert-center-category-cards__card--attention :deep(.p-card) {
  border-left: 4px solid var(--p-orange-500);
}

.alert-center-category-cards__kpi {
  height: 100%;
  transition: box-shadow 0.15s ease;
}

.alert-center-category-cards__card:hover :deep(.p-card) {
  box-shadow: 0 4px 12px rgb(0 0 0 / 8%);
}

.alert-center-category-cards__count {
  font-size: 1.75rem;
  font-weight: 700;
  line-height: 1.1;
}

.alert-center-category-cards__subtitle {
  font-size: 0.8125rem;
  color: var(--p-text-muted-color);
}

@media (max-width: 767px) {
  .alert-center-category-cards__grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}
</style>
