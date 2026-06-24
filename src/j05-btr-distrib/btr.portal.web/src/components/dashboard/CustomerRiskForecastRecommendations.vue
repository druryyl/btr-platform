<script setup lang="ts">
import { computed } from 'vue'
import Card from 'primevue/card'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Tag from 'primevue/tag'
import ProgressSpinner from 'primevue/progressspinner'
import { RouterLink } from 'vue-router'
import type { DashboardCustomerRiskForecastRecommendationItem } from '@/models/dashboard'
import { categoryBadgeSeverity } from '@/services/customerRiskForecastSignals'

const props = defineProps<{
  recommendations: DashboardCustomerRiskForecastRecommendationItem[]
  loading: boolean
}>()

const rows = computed(() => (props.recommendations ?? []).slice(0, 15))
</script>

<template>
  <Card class="customer-risk-forecast-recommendations">
    <template #title>
      <div class="customer-risk-forecast-recommendations__title">
        <i class="pi pi-list-check" aria-hidden="true" />
        <span>Top Recommended Actions</span>
      </div>
    </template>

    <template #content>
      <div v-if="loading" class="customer-risk-forecast-recommendations__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>

      <template v-else>
        <DataTable
          v-if="rows.length > 0"
          :value="rows"
          striped-rows
          size="small"
          class="customer-risk-forecast-recommendations__grid"
        >
          <Column header="#" field="SortOrder" />
          <Column header="Category">
            <template #body="{ data }">
              <Tag :severity="categoryBadgeSeverity(data.Category)" :value="data.Category" />
            </template>
          </Column>
          <Column field="RecommendationLabel" header="Action" />
          <Column field="CustomerName" header="Customer" />
          <Column field="ReasonText" header="Reason" />
          <Column header="">
            <template #body="{ data }">
              <RouterLink
                v-if="data.DrillDownRoute"
                :to="data.DrillDownRoute"
                class="customer-risk-forecast-recommendations__link"
              >
                Drill down
              </RouterLink>
              <RouterLink
                v-else-if="data.ReportRoute"
                :to="data.ReportRoute"
                class="customer-risk-forecast-recommendations__link"
              >
                Report
              </RouterLink>
            </template>
          </Column>
        </DataTable>

        <p v-else class="customer-risk-forecast-recommendations__empty">
          No recommended actions for the current forecast refresh.
        </p>
      </template>
    </template>
  </Card>
</template>

<style scoped>
.customer-risk-forecast-recommendations__title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.customer-risk-forecast-recommendations__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.customer-risk-forecast-recommendations__empty {
  margin: 0;
  padding: 2rem 0;
  text-align: center;
  color: var(--p-text-muted-color);
}

.customer-risk-forecast-recommendations__link {
  color: var(--p-primary-color);
  font-weight: 600;
  text-decoration: none;
}

.customer-risk-forecast-recommendations__link:hover {
  text-decoration: underline;
}
</style>
