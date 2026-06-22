<script setup lang="ts">
import Card from 'primevue/card'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import type { DashboardCustomerPortfolioConcentrationRow } from '@/models/dashboard'
import { formatCurrency, formatPercent } from '@/services/formatters'

defineProps<{
  topOmzet: DashboardCustomerPortfolioConcentrationRow[]
  topPiutang: DashboardCustomerPortfolioConcentrationRow[]
  loading: boolean
}>()
</script>

<template>
  <div class="customer-portfolio-concentration-tables">
    <Card class="customer-portfolio-concentration-tables__card">
      <template #title>Top 10 MTD Omzet Concentration</template>
      <template #content>
        <DataTable
          :value="topOmzet"
          :loading="loading"
          size="small"
          striped-rows
        >
          <Column field="Rank" header="#" style="width: 3rem" />
          <Column field="CustomerCode" header="Code" />
          <Column field="CustomerName" header="Customer" />
          <Column header="Amount">
            <template #body="{ data }">{{ formatCurrency(data.Amount) }}</template>
          </Column>
          <Column header="% of Total">
            <template #body="{ data }">{{ formatPercent(data.PercentOfTotal) }}</template>
          </Column>
        </DataTable>
      </template>
    </Card>

    <Card class="customer-portfolio-concentration-tables__card">
      <template #title>Top 10 Open Piutang Concentration</template>
      <template #content>
        <DataTable
          :value="topPiutang"
          :loading="loading"
          size="small"
          striped-rows
        >
          <Column field="Rank" header="#" style="width: 3rem" />
          <Column field="CustomerCode" header="Code" />
          <Column field="CustomerName" header="Customer" />
          <Column header="Amount">
            <template #body="{ data }">{{ formatCurrency(data.Amount) }}</template>
          </Column>
          <Column header="% of Total">
            <template #body="{ data }">{{ formatPercent(data.PercentOfTotal) }}</template>
          </Column>
        </DataTable>
      </template>
    </Card>
  </div>
</template>

<style scoped>
.customer-portfolio-concentration-tables {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 1rem;
  margin-top: 1rem;
}

@media (max-width: 960px) {
  .customer-portfolio-concentration-tables {
    grid-template-columns: 1fr;
  }
}
</style>
