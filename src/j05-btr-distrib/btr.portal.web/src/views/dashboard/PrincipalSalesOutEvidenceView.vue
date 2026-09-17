<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Button from 'primevue/button'
import Card from 'primevue/card'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import Message from 'primevue/message'
import { fetchPrincipalSalesOutEvidence } from '@/api/dashboardApi'
import DashboardDetailLayout from '@/components/dashboard/DashboardDetailLayout.vue'
import { getApiErrorMessage } from '@/api/httpClient'
import { formatCurrency, formatDate } from '@/services/formatters'
import type { PrincipalSalesOutEvidenceResponse } from '@/models/dashboard'

const route = useRoute()
const router = useRouter()

const evidence = ref<PrincipalSalesOutEvidenceResponse | null>(null)
const loading = ref(false)
const error = ref<string | null>(null)

const supplierId = computed(() => {
  const value = route.query.supplierId
  return typeof value === 'string' ? value.trim() : ''
})

const periodLabel = computed(() => {
  if (!evidence.value?.PeriodYear || !evidence.value.PeriodMonth) {
    return ''
  }

  const month = new Date(evidence.value.PeriodYear, evidence.value.PeriodMonth - 1, 1)
  return new Intl.DateTimeFormat('id-ID', { month: 'long', year: 'numeric' }).format(month)
})

const principalLabel = computed(() => {
  if (evidence.value?.PrincipalName) {
    return evidence.value.PrincipalName
  }

  return supplierId.value || 'Principal'
})

async function loadEvidence(): Promise<void> {
  if (!supplierId.value) {
    evidence.value = null
    error.value = 'Select a Principal from Principal Performance to open Faktur Item evidence.'
    return
  }

  loading.value = true
  error.value = null

  try {
    evidence.value = await fetchPrincipalSalesOutEvidence(supplierId.value)
  } catch (err) {
    evidence.value = null
    error.value = getApiErrorMessage(err, 'Failed to load Principal Sales-Out evidence.')
  } finally {
    loading.value = false
  }
}

function backToRanking(): void {
  void router.push({ name: 'principal-performance-dashboard' })
}

onMounted(() => {
  void loadEvidence()
})

watch(supplierId, () => {
  void loadEvidence()
})
</script>

<template>
  <DashboardDetailLayout
    title="Principal Sales-Out evidence"
    subtitle="Faktur Item evidence for PRN-SALES-001."
    :loading="loading"
    :error="error"
    @refresh="loadEvidence()"
  >
    <template #header-actions>
      <Button
        label="Back to ranking"
        icon="pi pi-arrow-left"
        outlined
        @click="backToRanking"
      />
    </template>

    <p class="principal-sales-out-evidence__identity">
      {{ principalLabel }}
      <span v-if="supplierId">· SupplierId {{ supplierId }}</span>
      <span v-if="periodLabel">· {{ periodLabel }}</span>
    </p>

    <section class="principal-sales-out-evidence__disclosure" aria-label="Principal Sales-Out disclosure">
      <h2>Principal Sales-Out disclosure</h2>
      <ul>
        <li
          v-for="statement in evidence?.Disclosures ?? []"
          :key="statement"
        >
          {{ statement }}
        </li>
      </ul>
    </section>

    <Card>
      <template #content>
        <DataTable
          :value="evidence?.Lines ?? []"
          :loading="loading"
          paginator
          :rows="25"
          :rows-per-page-options="[10, 25, 50, 100]"
          striped-rows
          data-key="FakturItemId"
        >
          <template #empty>
            <p>No Faktur Item evidence for this Principal.</p>
          </template>

          <Column field="FakturDate" header="Tanggal">
            <template #body="{ data }">
              {{ formatDate(data.FakturDate) }}
            </template>
          </Column>
          <Column field="FakturCode" header="Faktur" />
          <Column field="FakturItemId" header="Faktur Item" />
          <Column field="BrgId" header="BrgId" />
          <Column field="PrincipalSalesOutAmount" header="Principal Sales-Out">
            <template #body="{ data }">
              {{ formatCurrency(data.PrincipalSalesOutAmount) }}
            </template>
          </Column>
        </DataTable>

        <p v-if="evidence?.IsAvailable" class="principal-sales-out-evidence__total">
          Principal Sales-Out {{ formatCurrency(evidence.PrincipalSalesOutAmount) }}
        </p>
      </template>
    </Card>

    <Message
      v-if="!supplierId"
      severity="info"
      :closable="false"
    >
      Select a Principal from Principal Performance to open Faktur Item evidence.
    </Message>
  </DashboardDetailLayout>
</template>

<style scoped>
.principal-sales-out-evidence__identity,
.principal-sales-out-evidence__total {
  margin: 0 0 1rem;
}

.principal-sales-out-evidence__disclosure {
  margin-bottom: 1rem;
  padding: 1rem;
  background: var(--p-surface-0);
  border: 1px solid var(--p-surface-200);
  border-radius: var(--dashboard-radius-sm);
}

.principal-sales-out-evidence__disclosure h2 {
  margin: 0 0 0.75rem;
  font-size: 1rem;
}

.principal-sales-out-evidence__disclosure ul {
  margin: 0;
  padding-left: 1.25rem;
}

.principal-sales-out-evidence__disclosure li + li {
  margin-top: 0.35rem;
}
</style>
