<script setup lang="ts">
import { computed } from 'vue'
import Card from 'primevue/card'
import ProgressSpinner from 'primevue/progressspinner'
import Tag from 'primevue/tag'
import type { CustomerReportPrincipalPairEvidence } from '@/models/reports'
import { CU05_PRINCIPAL_PAIR_NOTE } from '@/services/customerAnalyticsAttribution'
import { formatCurrency } from '@/services/formatters'

const props = defineProps<{
  evidence: CustomerReportPrincipalPairEvidence | null | undefined
  loading: boolean
}>()

const disclosures = computed(() => {
  const items = props.evidence?.Disclosures ?? []
  return items.length > 0 ? items : [CU05_PRINCIPAL_PAIR_NOTE]
})

const customers = computed(() => props.evidence?.Customers ?? [])

function statusSeverity(status: string): string {
  if (status === 'Active') return 'success'
  if (status === 'Dormant') return 'warn'
  return 'info'
}
</script>

<template>
  <Card
    class="customer-report-principal-pairs"
    data-kpi="PRN-SALES-001"
    aria-label="Customer Principal pair evidence"
  >
    <template #title>
      <div class="customer-report-principal-pairs__title">
        <i class="pi pi-th-large" aria-hidden="true" />
        <span>Customer–Principal pair evidence</span>
      </div>
      <p class="customer-report-principal-pairs__note">
        {{ evidence?.Note || CU05_PRINCIPAL_PAIR_NOTE }}
      </p>
      <ul class="customer-report-principal-pairs__disclosures">
        <li v-for="item in disclosures" :key="item">{{ item }}</li>
      </ul>
    </template>

    <template #content>
      <div v-if="loading" class="customer-report-principal-pairs__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>

      <template v-else>
        <p
          v-if="evidence && !evidence.IsAvailable"
          class="customer-report-principal-pairs__empty"
        >
          Pair evidence is unavailable until the relationship projection is refreshed.
        </p>
        <p v-else-if="customers.length === 0" class="customer-report-principal-pairs__empty">
          No customers to show pair evidence.
        </p>

        <template v-else>
          <div
            v-for="customer in customers"
            :key="customer.CustomerCode"
            class="customer-report-principal-pairs__customer"
          >
            <h3 class="customer-report-principal-pairs__customer-title">
              {{ customer.CustomerCode }} — {{ customer.CustomerName }}
            </h3>
            <p
              v-if="customer.Principals.length === 0"
              class="customer-report-principal-pairs__empty"
            >
              No Principals on this Customer's projection.
            </p>
            <table v-else class="customer-report-principal-pairs__table">
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
.customer-report-principal-pairs__title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.customer-report-principal-pairs__note {
  margin: 0.5rem 0 0;
  color: var(--p-text-muted-color);
  font-size: 0.875rem;
  font-weight: 400;
}

.customer-report-principal-pairs__disclosures {
  margin: 0.5rem 0 0;
  padding-left: 1.25rem;
  color: var(--p-text-muted-color);
  font-size: 0.875rem;
  font-weight: 400;
}

.customer-report-principal-pairs__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.customer-report-principal-pairs__empty {
  margin: 0;
  padding: 1rem 0;
  color: var(--p-text-muted-color);
}

.customer-report-principal-pairs__customer + .customer-report-principal-pairs__customer {
  margin-top: 1rem;
}

.customer-report-principal-pairs__customer-title {
  margin: 0 0 0.5rem;
  font-size: 1rem;
}

.customer-report-principal-pairs__table {
  width: 100%;
  border-collapse: collapse;
}

.customer-report-principal-pairs__table th,
.customer-report-principal-pairs__table td {
  padding: 0.5rem 0.75rem;
  text-align: left;
  border-bottom: 1px solid var(--p-content-border-color);
}

.customer-report-principal-pairs__table th:nth-child(n + 3),
.customer-report-principal-pairs__table td:nth-child(n + 3) {
  text-align: right;
}
</style>
