<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Button from 'primevue/button'
import Card from 'primevue/card'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import Message from 'primevue/message'
import { fetchPrincipalReturnEvidence } from '@/api/dashboardApi'
import DashboardDetailLayout from '@/components/dashboard/DashboardDetailLayout.vue'
import { getApiErrorMessage } from '@/api/httpClient'
import { formatCurrency, formatDate } from '@/services/formatters'
import type { PrincipalReturnEvidenceResponse } from '@/models/dashboard'

const route = useRoute()
const router = useRouter()

const evidence = ref<PrincipalReturnEvidenceResponse | null>(null)
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
    error.value = 'Select a Principal from Principal Performance to open Return Item evidence.'
    return
  }

  loading.value = true
  error.value = null

  try {
    evidence.value = await fetchPrincipalReturnEvidence(supplierId.value)
  } catch (err) {
    evidence.value = null
    error.value = getApiErrorMessage(err, 'Failed to load Principal return evidence.')
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
    title="Principal return evidence"
    subtitle="Return Item evidence for PRN-RET-001 through PRN-RET-004."
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

    <p class="principal-return-evidence__identity">
      {{ principalLabel }}
      <span v-if="supplierId">· SupplierId {{ supplierId }}</span>
      <span v-if="periodLabel">· {{ periodLabel }}</span>
    </p>

    <section class="principal-return-evidence__disclosure" aria-label="Principal return disclosure">
      <h2>Principal return disclosure</h2>
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
          data-key="ReturJualItemId"
        >
          <template #empty>
            <p>No Return Item evidence for this Principal.</p>
          </template>

          <Column field="ReturJualDate" header="Tanggal">
            <template #body="{ data }">
              {{ formatDate(data.ReturJualDate) }}
            </template>
          </Column>
          <Column field="ReturJualCode" header="Retur" />
          <Column field="ReturJualItemId" header="Retur Item" />
          <Column field="BrgId" header="BrgId" />
          <Column field="JenisRetur" header="Jenis Retur" />
          <Column field="ReturnAmount" header="Return Amount">
            <template #body="{ data }">
              {{ formatCurrency(data.ReturnAmount) }}
            </template>
          </Column>
        </DataTable>

        <p v-if="evidence?.IsAvailable" class="principal-return-evidence__total">
          Total Return {{ formatCurrency(evidence.TotalReturnAmount) }}
        </p>
        <p v-if="evidence?.IsAvailable" class="principal-return-evidence__note">
          Returns are shown separately. Sales-Out is unchanged. Return Percentage is a quality
          ratio, not Net Sales.
        </p>
      </template>
    </Card>

    <Message
      v-if="!supplierId"
      severity="info"
      :closable="false"
    >
      Select a Principal from Principal Performance to open Return Item evidence.
    </Message>
  </DashboardDetailLayout>
</template>

<style scoped>
.principal-return-evidence__identity,
.principal-return-evidence__total {
  margin: 0 0 1rem;
}

.principal-return-evidence__note {
  margin: 0;
  color: var(--p-text-muted-color);
}

.principal-return-evidence__disclosure {
  margin-bottom: 1rem;
  padding: 1rem;
  background: var(--p-surface-0);
  border: 1px solid var(--p-surface-200);
  border-radius: var(--dashboard-radius-sm);
}

.principal-return-evidence__disclosure h2 {
  margin: 0 0 0.75rem;
  font-size: 1rem;
}

.principal-return-evidence__disclosure ul {
  margin: 0;
  padding-left: 1.25rem;
}

.principal-return-evidence__disclosure li + li {
  margin-top: 0.35rem;
}
</style>
