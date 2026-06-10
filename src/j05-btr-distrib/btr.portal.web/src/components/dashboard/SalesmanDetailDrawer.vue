<script setup lang="ts">
import { ref, watch } from 'vue'
import Drawer from 'primevue/drawer'
import TabView from 'primevue/tabview'
import TabPanel from 'primevue/tabpanel'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import ProgressSpinner from 'primevue/progressspinner'
import Tag from 'primevue/tag'
import { fetchSalesmanPrincipals, fetchSalesmanTrend } from '@/api/dashboardApi'
import type {
  SalesmanAchievementTrendResponse,
  SalesmanPrincipalAchievementResponse,
} from '@/models/dashboard'
import { formatCurrency, formatPercent } from '@/services/formatters'
import SalesmanAchievementTrend from '@/components/dashboard/SalesmanAchievementTrend.vue'

const visible = defineModel<boolean>('visible', { default: false })

const props = defineProps<{
  salesPersonId: string | null
  salesPersonName: string | null
}>()

const loadingPrincipals = ref(false)
const loadingTrend = ref(false)
const principals = ref<SalesmanPrincipalAchievementResponse | null>(null)
const trend = ref<SalesmanAchievementTrendResponse | null>(null)
const error = ref<string | null>(null)

watch(
  () => [visible.value, props.salesPersonId] as const,
  async ([isVisible, salesPersonId]) => {
    if (!isVisible || !salesPersonId) {
      return
    }

    error.value = null
    loadingPrincipals.value = true
    loadingTrend.value = true
    principals.value = null
    trend.value = null

    try {
      const [principalData, trendData] = await Promise.all([
        fetchSalesmanPrincipals(salesPersonId),
        fetchSalesmanTrend(salesPersonId),
      ])
      principals.value = principalData
      trend.value = trendData
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to load salesman detail.'
    } finally {
      loadingPrincipals.value = false
      loadingTrend.value = false
    }
  },
)

function bandSeverity(band: string | null | undefined): 'success' | 'warn' | 'danger' | 'secondary' {
  switch (band?.toLowerCase()) {
    case 'healthy':
      return 'success'
    case 'warning':
      return 'warn'
    case 'critical':
      return 'danger'
    default:
      return 'secondary'
  }
}
</script>

<template>
  <Drawer
    v-model:visible="visible"
    position="right"
    :header="salesPersonName ? `${salesPersonName} — Performance Detail` : 'Salesman Detail'"
    class="salesman-detail-drawer"
    style="width: min(42rem, 100vw)"
  >
    <p v-if="error" class="salesman-detail-drawer__error">{{ error }}</p>

    <TabView v-else>
      <TabPanel header="Principal Achievement">
        <div v-if="loadingPrincipals" class="salesman-detail-drawer__loading">
          <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
        </div>

        <template v-else>
          <p
            v-if="!principals?.Principals?.length"
            class="salesman-detail-drawer__empty"
          >
            No principal targets or sales this month.
          </p>

          <DataTable
            v-else
            :value="principals?.Principals ?? []"
            striped-rows
            class="salesman-detail-drawer__table"
          >
            <Column field="SupplierName" header="Principal" />
            <Column header="Target">
              <template #body="{ data }">
                {{ data.TargetAmount != null ? formatCurrency(data.TargetAmount) : '—' }}
              </template>
            </Column>
            <Column header="Omzet">
              <template #body="{ data }">
                {{ formatCurrency(data.CompletedOmzet) }}
              </template>
            </Column>
            <Column header="Achievement %">
              <template #body="{ data }">
                <div class="salesman-detail-drawer__achievement">
                  <span>
                    {{ data.AchievementPercent != null ? formatPercent(data.AchievementPercent) : '—' }}
                  </span>
                  <Tag
                    v-if="data.AchievementBand"
                    :value="data.AchievementBand"
                    :severity="bandSeverity(data.AchievementBand)"
                  />
                </div>
              </template>
            </Column>
          </DataTable>
        </template>
      </TabPanel>

      <TabPanel header="Trend">
        <SalesmanAchievementTrend
          :points="trend?.Points ?? []"
          :loading="loadingTrend"
        />
      </TabPanel>
    </TabView>
  </Drawer>
</template>

<style scoped>
.salesman-detail-drawer__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.salesman-detail-drawer__empty,
.salesman-detail-drawer__error {
  margin: 0;
  padding: 1.5rem 0;
  text-align: center;
  color: var(--p-text-muted-color);
}

.salesman-detail-drawer__error {
  color: var(--p-red-500);
}

.salesman-detail-drawer__achievement {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}
</style>
