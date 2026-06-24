<script setup lang="ts">
import { computed } from 'vue'
import Accordion from 'primevue/accordion'
import AccordionPanel from 'primevue/accordionpanel'
import AccordionHeader from 'primevue/accordionheader'
import AccordionContent from 'primevue/accordioncontent'
import Card from 'primevue/card'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Tag from 'primevue/tag'
import { RouterLink } from 'vue-router'
import type { DashboardCustomerPortfolioPriorityRow } from '@/models/dashboard'
import { formatCurrency } from '@/services/formatters'
import {
  CUSTOMER_PORTFOLIO_ACTION_KEYS,
  CUSTOMER_PORTFOLIO_ACTION_LABELS,
  actionBadgeSeverity,
  groupPriorityRowsByAction,
} from '@/services/customerPortfolioSignals'

const props = defineProps<{
  rows: DashboardCustomerPortfolioPriorityRow[]
  loading: boolean
}>()

const groupedRows = computed(() => groupPriorityRowsByAction(props.rows ?? []))

const actionSections = computed(() =>
  CUSTOMER_PORTFOLIO_ACTION_KEYS.map((key) => ({
    key,
    label: CUSTOMER_PORTFOLIO_ACTION_LABELS[key],
    rows: groupedRows.value[key] ?? [],
  })).filter((section) => section.rows.length > 0),
)
</script>

<template>
  <Card class="customer-portfolio-action-segments">
    <template #title>Customers by Portfolio Action</template>
    <template #content>
      <p v-if="loading" class="customer-portfolio-action-segments__muted">Loading action segments…</p>
      <p v-else-if="actionSections.length === 0" class="customer-portfolio-action-segments__muted">
        No portfolio actions in the current priority queue.
      </p>
      <Accordion v-else multiple>
        <AccordionPanel
          v-for="section in actionSections"
          :key="section.key"
          :value="section.key"
        >
          <AccordionHeader>
            <div class="customer-portfolio-action-segments__header">
              <Tag
                :value="section.label"
                :severity="actionBadgeSeverity(section.key)"
              />
              <span>{{ section.rows.length }} customers</span>
            </div>
          </AccordionHeader>
          <AccordionContent>
            <DataTable :value="section.rows" size="small" striped-rows>
              <Column field="CustomerCode" header="Code" />
              <Column field="CustomerName" header="Customer" />
              <Column field="PortfolioPriorityScore" header="Priority" />
              <Column header="Open Balance">
                <template #body="{ data }">{{ formatCurrency(data.OpenBalance) }}</template>
              </Column>
              <Column field="ActionReasonText" header="Reason" />
              <Column header="">
                <template #body="{ data }">
                  <RouterLink
                    v-if="data.CustomerReportRoute"
                    :to="data.CustomerReportRoute"
                    class="customer-portfolio-action-segments__link"
                  >
                    Report
                  </RouterLink>
                </template>
              </Column>
            </DataTable>
          </AccordionContent>
        </AccordionPanel>
      </Accordion>
    </template>
  </Card>
</template>

<style scoped>
.customer-portfolio-action-segments__header {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.customer-portfolio-action-segments__muted {
  margin: 0;
  color: var(--p-text-muted-color);
}

.customer-portfolio-action-segments__link {
  color: var(--p-primary-color);
  font-weight: 600;
  text-decoration: none;
}

.customer-portfolio-action-segments__link:hover {
  text-decoration: underline;
}
</style>
