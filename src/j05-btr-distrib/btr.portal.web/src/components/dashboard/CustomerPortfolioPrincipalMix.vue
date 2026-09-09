<script setup lang="ts">
import { computed } from 'vue'
import Card from 'primevue/card'
import ProgressSpinner from 'primevue/progressspinner'
import Tag from 'primevue/tag'
import type { DashboardCustomerPortfolioPrincipalMix } from '@/models/dashboard'
import { CU04_PRINCIPAL_MIX_NOTE } from '@/services/customerAnalyticsAttribution'
import { formatCurrency } from '@/services/formatters'

const props = defineProps<{
  mix: DashboardCustomerPortfolioPrincipalMix | null | undefined
  loading: boolean
}>()

const disclosures = computed(() => {
  const items = props.mix?.Disclosures ?? []
  return items.length > 0 ? items : [CU04_PRINCIPAL_MIX_NOTE]
})

const customers = computed(() => props.mix?.Customers ?? [])

function statusSeverity(status: string): string {
  if (status === 'Active') return 'success'
  if (status === 'Dormant') return 'warn'
  return 'info'
}
</script>

<template>
  <Card
    class="customer-portfolio-principal-mix"
    data-kpi="PRN-SALES-001"
    aria-label="Portfolio mix"
  >
    <template #title>
      <div class="customer-portfolio-principal-mix__title">
        <i class="pi pi-th-large" aria-hidden="true" />
        <span>Portfolio mix</span>
      </div>
      <p class="customer-portfolio-principal-mix__note">
        {{ mix?.Note || CU04_PRINCIPAL_MIX_NOTE }}
      </p>
      <ul class="customer-portfolio-principal-mix__disclosures">
        <li v-for="item in disclosures" :key="item">{{ item }}</li>
      </ul>
    </template>

    <template #content>
      <div v-if="loading" class="customer-portfolio-principal-mix__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>

      <template v-else>
        <p
          v-if="mix && !mix.IsAvailable"
          class="customer-portfolio-principal-mix__empty"
        >
          Portfolio mix is unavailable until the relationship projection is refreshed.
        </p>
        <p v-else-if="customers.length === 0" class="customer-portfolio-principal-mix__empty">
          No priority customers to show portfolio mix.
        </p>

        <template v-else>
          <div
            v-for="customer in customers"
            :key="customer.CustomerCode"
            class="customer-portfolio-principal-mix__customer"
          >
            <h3 class="customer-portfolio-principal-mix__customer-title">
              {{ customer.CustomerCode }} — {{ customer.CustomerName }}
            </h3>
            <p
              v-if="customer.Principals.length === 0"
              class="customer-portfolio-principal-mix__empty"
            >
              No Principals on this Customer's projection.
            </p>
            <table v-else class="customer-portfolio-principal-mix__table">
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
.customer-portfolio-principal-mix__title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.customer-portfolio-principal-mix__note {
  margin: 0.5rem 0 0;
  color: var(--p-text-muted-color);
  font-size: 0.875rem;
  font-weight: 400;
}

.customer-portfolio-principal-mix__disclosures {
  margin: 0.5rem 0 0;
  padding-left: 1.25rem;
  color: var(--p-text-muted-color);
  font-size: 0.875rem;
  font-weight: 400;
}

.customer-portfolio-principal-mix__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.customer-portfolio-principal-mix__empty {
  margin: 0;
  padding: 1rem 0;
  color: var(--p-text-muted-color);
}

.customer-portfolio-principal-mix__customer + .customer-portfolio-principal-mix__customer {
  margin-top: 1rem;
}

.customer-portfolio-principal-mix__customer-title {
  margin: 0 0 0.5rem;
  font-size: 1rem;
}

.customer-portfolio-principal-mix__table {
  width: 100%;
  border-collapse: collapse;
}

.customer-portfolio-principal-mix__table th,
.customer-portfolio-principal-mix__table td {
  padding: 0.5rem 0.75rem;
  text-align: left;
  border-bottom: 1px solid var(--p-content-border-color);
}

.customer-portfolio-principal-mix__table th:nth-child(n + 3),
.customer-portfolio-principal-mix__table td:nth-child(n + 3) {
  text-align: right;
}
</style>
