<script setup lang="ts">
import { computed } from 'vue'
import Card from 'primevue/card'
import ProgressSpinner from 'primevue/progressspinner'
import Tag from 'primevue/tag'
import type { DashboardCustomerRiskForecastPrincipalDecline } from '@/models/dashboard'
import { CU02_PRINCIPAL_DECLINE_NOTE } from '@/services/customerAnalyticsAttribution'
import { formatCurrency } from '@/services/formatters'

const props = defineProps<{
  decline: DashboardCustomerRiskForecastPrincipalDecline | null | undefined
  loading: boolean
}>()

const disclosures = computed(() => {
  const items = props.decline?.Disclosures ?? []
  return items.length > 0 ? items : [CU02_PRINCIPAL_DECLINE_NOTE]
})

const customers = computed(() => props.decline?.Customers ?? [])

function statusSeverity(status: string): string {
  if (status === 'Active') return 'success'
  if (status === 'Dormant') return 'warn'
  return 'info'
}
</script>

<template>
  <Card
    class="customer-risk-forecast-principal-decline"
    data-kpi="PRN-SALES-001"
    aria-label="Principal decline or inactivity"
  >
    <template #title>
      <div class="customer-risk-forecast-principal-decline__title">
        <i class="pi pi-th-large" aria-hidden="true" />
        <span>Principal decline or inactivity</span>
      </div>
      <p class="customer-risk-forecast-principal-decline__note">
        {{ decline?.Note || CU02_PRINCIPAL_DECLINE_NOTE }}
      </p>
      <ul class="customer-risk-forecast-principal-decline__disclosures">
        <li v-for="item in disclosures" :key="item">{{ item }}</li>
      </ul>
    </template>

    <template #content>
      <div v-if="loading" class="customer-risk-forecast-principal-decline__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>

      <template v-else>
        <p
          v-if="decline && !decline.IsAvailable"
          class="customer-risk-forecast-principal-decline__empty"
        >
          Principal decline is unavailable until the relationship projection is refreshed.
        </p>
        <p v-else-if="customers.length === 0" class="customer-risk-forecast-principal-decline__empty">
          No at-risk customers to show Principal decline.
        </p>

        <template v-else>
          <div
            v-for="customer in customers"
            :key="customer.CustomerCode"
            class="customer-risk-forecast-principal-decline__customer"
          >
            <h3 class="customer-risk-forecast-principal-decline__customer-title">
              {{ customer.CustomerCode }} — {{ customer.CustomerName }}
            </h3>
            <p
              v-if="customer.Principals.length === 0"
              class="customer-risk-forecast-principal-decline__empty"
            >
              No Principals on this Customer's projection.
            </p>
            <table v-else class="customer-risk-forecast-principal-decline__table">
              <thead>
                <tr>
                  <th>Principal</th>
                  <th>Status</th>
                  <th>Pair Sales-Out</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="principal in customer.Principals" :key="principal.SupplierId">
                  <td>{{ principal.PrincipalName || principal.SupplierId }}</td>
                  <td>
                    <Tag :severity="statusSeverity(principal.RelationshipStatus)" :value="principal.RelationshipStatus || '—'" />
                  </td>
                  <td>{{ formatCurrency(principal.PairSalesOutAmount) }}</td>
                </tr>
              </tbody>
            </table>
          </div>
        </template>
      </template>
    </template>
  </Card>
</template>

<style scoped>
.customer-risk-forecast-principal-decline__title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.customer-risk-forecast-principal-decline__note {
  margin: 0.5rem 0 0;
  color: var(--p-text-muted-color);
  font-size: 0.875rem;
  font-weight: 400;
}

.customer-risk-forecast-principal-decline__disclosures {
  margin: 0.5rem 0 0;
  padding-left: 1.25rem;
  color: var(--p-text-muted-color);
  font-size: 0.875rem;
  font-weight: 400;
}

.customer-risk-forecast-principal-decline__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.customer-risk-forecast-principal-decline__empty {
  margin: 0;
  padding: 1rem 0;
  color: var(--p-text-muted-color);
}

.customer-risk-forecast-principal-decline__customer + .customer-risk-forecast-principal-decline__customer {
  margin-top: 1rem;
}

.customer-risk-forecast-principal-decline__customer-title {
  margin: 0 0 0.5rem;
  font-size: 1rem;
}

.customer-risk-forecast-principal-decline__table {
  width: 100%;
  border-collapse: collapse;
}

.customer-risk-forecast-principal-decline__table th,
.customer-risk-forecast-principal-decline__table td {
  padding: 0.5rem 0.75rem;
  text-align: left;
  border-bottom: 1px solid var(--p-content-border-color);
}

.customer-risk-forecast-principal-decline__table th:nth-child(n + 3),
.customer-risk-forecast-principal-decline__table td:nth-child(n + 3) {
  text-align: right;
}
</style>
