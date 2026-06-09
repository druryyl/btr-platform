<script setup lang="ts">

import { useRouter } from 'vue-router'

import Card from 'primevue/card'

import Column from 'primevue/column'

import DataTable from 'primevue/datatable'

import Button from 'primevue/button'

import type { DashboardAlertCenterAlertRow, DashboardAlertCenterCategoryGroup } from '@/models/dashboard'

import { formatCurrency } from '@/services/formatters'

import { navigateToDashboard, navigateToInvestigation } from '@/services/navigateToInvestigation'



defineProps<{

  groups: DashboardAlertCenterCategoryGroup[]

  loading: boolean

}>()



const router = useRouter()

const sourceLabel = 'Alert Center'



function formatValue(row: DashboardAlertCenterAlertRow): string {

  if (row.ValueText) {

    return row.ValueText

  }



  if (row.ValueAmount != null) {

    return formatCurrency(row.ValueAmount)

  }



  return '—'

}



function canInvestigate(row: DashboardAlertCenterAlertRow): boolean {

  return row.Investigation?.ReportRoute != null

    && row.EntityType !== 'Wilayah'

    && row.EntityType !== 'Company'

}



function canViewDashboard(row: DashboardAlertCenterAlertRow): boolean {

  return Boolean(row.DashboardRoute)

}



function investigate(row: DashboardAlertCenterAlertRow): void {

  if (!row.Investigation || !canInvestigate(row)) return

  navigateToInvestigation(router, row.Investigation, sourceLabel)

}



function openDashboard(row: DashboardAlertCenterAlertRow): void {

  if (!row.DashboardRoute) return

  navigateToDashboard(router, row.DashboardRoute)

}

</script>



<template>

  <section class="alert-center-alerts">

    <h2 class="alert-center-alerts__heading">Alerts</h2>



    <div v-if="loading" class="alert-center-alerts__loading">

      Loading alerts…

    </div>



    <template v-else>

      <Card

        v-for="group in groups"

        :key="group.Category"

        class="alert-center-alerts__group"

      >

        <template #title>{{ group.Category }}</template>

        <template #content>

          <DataTable :value="group.Alerts" striped-rows class="alert-center-alerts__table">

            <template #empty>

              <p class="alert-center-alerts__empty">No alerts in this category.</p>

            </template>



            <Column field="EntityType" header="Type" />

            <Column field="EntityName" header="Entity" />

            <Column field="SignalLabel" header="Signal" />

            <Column header="Value">

              <template #body="{ data }">

                {{ formatValue(data) }}

              </template>

            </Column>

            <Column header="Actions">

              <template #body="{ data }">

                <div class="alert-center-alerts__actions">

                  <Button

                    v-if="canInvestigate(data)"

                    label="Investigate"

                    text

                    size="small"

                    @click="investigate(data)"

                  />

                  <Button

                    v-if="canViewDashboard(data)"

                    label="View Dashboard"

                    text

                    size="small"

                    severity="secondary"

                    @click="openDashboard(data)"

                  />

                </div>

              </template>

            </Column>

          </DataTable>

        </template>

      </Card>



      <p v-if="groups.length === 0" class="alert-center-alerts__empty">

        No exception alerts require attention right now.

      </p>

    </template>

  </section>

</template>



<style scoped>

.alert-center-alerts__heading {

  margin: 0 0 0.75rem;

  font-size: 1.125rem;

}



.alert-center-alerts__group {

  margin-bottom: 1rem;

}



.alert-center-alerts__actions {

  display: flex;

  flex-wrap: wrap;

  gap: 0.25rem;

}



.alert-center-alerts__loading,

.alert-center-alerts__empty {

  margin: 0;

  padding: 1.5rem 0;

  text-align: center;

  color: var(--p-text-muted-color);

}

</style>

