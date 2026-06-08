<script setup lang="ts">
import Card from 'primevue/card'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import ProgressSpinner from 'primevue/progressspinner'
import type { DashboardCustomerSegmentRow } from '@/models/dashboard'
import { formatNumber } from '@/services/formatters'

defineProps<{
  byKlasifikasi: DashboardCustomerSegmentRow[]
  byWilayah: DashboardCustomerSegmentRow[]
  activeSummary: DashboardCustomerSegmentRow | null
  dormantSummary: DashboardCustomerSegmentRow | null
  loading: boolean
}>()
</script>

<template>
  <section class="customer-segmentation">
    <h2 class="customer-segmentation__heading">Segmentation Summary</h2>

    <div v-if="loading" class="customer-segmentation__loading">
      <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
    </div>

    <template v-else>
      <div class="customer-segmentation__summary">
        <Card>
          <template #title>Active vs Dormant</template>
          <template #content>
            <div class="customer-segmentation__activity">
              <div class="metric">
                <span class="metric__label">Active (month)</span>
                <span class="metric__value">
                  {{ activeSummary ? formatNumber(activeSummary.CustomerCount) : '—' }}
                </span>
              </div>
              <div class="metric">
                <span class="metric__label">Dormant (90-day)</span>
                <span class="metric__value">
                  {{ dormantSummary ? formatNumber(dormantSummary.CustomerCount) : '—' }}
                </span>
              </div>
            </div>
          </template>
        </Card>
      </div>

      <div class="customer-segmentation__grid">
        <Card>
          <template #title>By Klasifikasi</template>
          <template #content>
            <DataTable :value="byKlasifikasi" striped-rows>
              <template #empty>
                <p class="customer-segmentation__empty">No segmentation data.</p>
              </template>
              <Column field="SegmentLabel" header="Klasifikasi" />
              <Column field="CustomerCount" header="Total" />
              <Column field="ActiveCount" header="Active" />
              <Column field="DormantCount" header="Dormant" />
            </DataTable>
          </template>
        </Card>

        <Card>
          <template #title>By Wilayah</template>
          <template #content>
            <DataTable :value="byWilayah" striped-rows>
              <template #empty>
                <p class="customer-segmentation__empty">No segmentation data.</p>
              </template>
              <Column field="SegmentLabel" header="Wilayah" />
              <Column field="CustomerCount" header="Total" />
              <Column field="ActiveCount" header="Active" />
              <Column field="DormantCount" header="Dormant" />
            </DataTable>
          </template>
        </Card>
      </div>
    </template>
  </section>
</template>

<style scoped>
.customer-segmentation__heading {
  margin: 0 0 1rem;
  font-size: 1.25rem;
}

.customer-segmentation__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.customer-segmentation__summary {
  margin-bottom: 1rem;
}

.customer-segmentation__activity {
  display: flex;
  gap: 2rem;
}

.customer-segmentation__grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: 1rem;
}

.customer-segmentation__empty {
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
