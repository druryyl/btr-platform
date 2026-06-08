<script setup lang="ts">
import Card from 'primevue/card'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import ProgressSpinner from 'primevue/progressspinner'
import type { DashboardSalesmanSegmentRow } from '@/models/dashboard'
import { formatNumber } from '@/services/formatters'

defineProps<{
  byWilayah: DashboardSalesmanSegmentRow[]
  bySegment: DashboardSalesmanSegmentRow[]
  activeSummary: DashboardSalesmanSegmentRow | null
  inactiveSummary: DashboardSalesmanSegmentRow | null
  loading: boolean
}>()
</script>

<template>
  <section class="salesman-segmentation">
    <h2 class="salesman-segmentation__heading">Segmentation Summary</h2>

    <div v-if="loading" class="salesman-segmentation__loading">
      <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
    </div>

    <template v-else>
      <div class="salesman-segmentation__summary">
        <Card>
          <template #title>Active vs Inactive</template>
          <template #content>
            <div class="salesman-segmentation__activity">
              <div class="metric">
                <span class="metric__label">Active (month)</span>
                <span class="metric__value">
                  {{ activeSummary ? formatNumber(activeSummary.SalesmanCount) : '—' }}
                </span>
              </div>
              <div class="metric">
                <span class="metric__label">Inactive</span>
                <span class="metric__value">
                  {{ inactiveSummary ? formatNumber(inactiveSummary.SalesmanCount) : '—' }}
                </span>
              </div>
            </div>
          </template>
        </Card>
      </div>

      <div class="salesman-segmentation__grid">
        <Card>
          <template #title>By Wilayah</template>
          <template #content>
            <DataTable :value="byWilayah" striped-rows>
              <template #empty>
                <p class="salesman-segmentation__empty">No segmentation data.</p>
              </template>
              <Column field="SegmentLabel" header="Wilayah" />
              <Column field="SalesmanCount" header="Total" />
              <Column field="ActiveCount" header="Active" />
              <Column field="InactiveCount" header="Inactive" />
            </DataTable>
          </template>
        </Card>

        <Card v-if="bySegment.length > 0">
          <template #title>By Segment</template>
          <template #content>
            <DataTable :value="bySegment" striped-rows>
              <template #empty>
                <p class="salesman-segmentation__empty">No segment data.</p>
              </template>
              <Column field="SegmentLabel" header="Segment" />
              <Column field="SalesmanCount" header="Total" />
              <Column field="ActiveCount" header="Active" />
              <Column field="InactiveCount" header="Inactive" />
            </DataTable>
          </template>
        </Card>
      </div>
    </template>
  </section>
</template>

<style scoped>
.salesman-segmentation__heading {
  margin: 0 0 1rem;
  font-size: 1.25rem;
}

.salesman-segmentation__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.salesman-segmentation__summary {
  margin-bottom: 1rem;
}

.salesman-segmentation__activity {
  display: flex;
  gap: 2rem;
}

.salesman-segmentation__grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: 1rem;
}

.salesman-segmentation__empty {
  margin: 0;
  padding: 1rem 0;
  text-align: center;
  color: var(--p-text-muted-color);
}

.metric__label {
  display: block;
  font-size: 0.875rem;
  color: var(--p-text-muted-color);
}

.metric__value {
  display: block;
  font-size: 1.5rem;
  font-weight: 700;
}
</style>
