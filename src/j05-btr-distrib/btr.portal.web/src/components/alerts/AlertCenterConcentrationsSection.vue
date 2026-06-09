<script setup lang="ts">
import { useRouter } from 'vue-router'
import Card from 'primevue/card'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import Button from 'primevue/button'
import type { DashboardAlertCenterConcentrationItem } from '@/models/dashboard'

defineProps<{
  items: DashboardAlertCenterConcentrationItem[]
  loading: boolean
}>()

const router = useRouter()

function openDashboard(route: string): void {
  void router.push(route)
}
</script>

<template>
  <section class="alert-center-concentrations">
    <h2 class="alert-center-concentrations__heading">Concentrations</h2>
    <Card>
      <template #content>
        <DataTable
          v-if="!loading && items.length > 0"
          :value="items"
          striped-rows
          class="alert-center-concentrations__table"
        >
          <Column field="Label" header="Metric" />
          <Column field="ValueText" header="Value" />
          <Column field="SourceDomain" header="Source" />
          <Column header="">
            <template #body="{ data }">
              <Button
                icon="pi pi-external-link"
                text
                rounded
                severity="secondary"
                aria-label="Open dashboard"
                @click="openDashboard(data.DashboardRoute)"
              />
            </template>
          </Column>
        </DataTable>
        <p v-else-if="!loading" class="alert-center-concentrations__empty">
          No concentration metrics available.
        </p>
        <p v-else class="alert-center-concentrations__empty">Loading concentrations…</p>
      </template>
    </Card>
  </section>
</template>

<style scoped>
.alert-center-concentrations__heading {
  margin: 0 0 0.75rem;
  font-size: 1.125rem;
}

.alert-center-concentrations__empty {
  margin: 0;
  padding: 1.5rem 0;
  text-align: center;
  color: var(--p-text-muted-color);
}
</style>
