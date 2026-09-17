<script setup lang="ts">
import { computed, ref } from 'vue'
import { RouterLink } from 'vue-router'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import InputText from 'primevue/inputtext'
import ProgressSpinner from 'primevue/progressspinner'
import type {
  PrincipalPerformanceRankingItem,
  PrincipalSalesmanContributionItem,
} from '@/models/dashboard'
import { principalDependencies } from '@/services/principalContribution'
import { formatWithNoData } from '@/services/principalNoData'
import { formatCurrency, formatNumber, formatPercent } from '@/services/formatters'

const props = defineProps<{
  ranking: PrincipalPerformanceRankingItem[]
  contributions: PrincipalSalesmanContributionItem[]
  loading: boolean
}>()

interface PrincipalDetailRow {
  supplierId: string
  principalName: string
  sales: number
  target: number | null
  achievementPercent: number | null
  achievementGap: number | null
  coveragePercent: number | null
  returnPercent: number | null
  momGrowthPercent: number | null
  yoyGrowthPercent: number | null
  dependencyRank: number | null
}

const searchQuery = ref('')

const dependencyRankBySupplier = computed(() => {
  const ranks = new Map<string, number | null>()
  for (const dependency of principalDependencies(props.contributions, props.ranking)) {
    ranks.set(dependency.supplierId, dependency.dependencyRank)
  }
  return ranks
})

const rows = computed<PrincipalDetailRow[]>(() =>
  props.ranking.map((item) => ({
    supplierId: item.SupplierId,
    principalName: item.PrincipalName || item.SupplierId,
    sales: item.PrincipalSalesOutAmount,
    target: item.PrincipalTargetAmount ?? null,
    achievementPercent: item.AchievementPercentage ?? null,
    achievementGap: item.AchievementAmount ?? null,
    coveragePercent: item.CoveragePercentage ?? null,
    returnPercent: item.ReturnPercentage ?? null,
    momGrowthPercent: item.MomGrowthPercentage ?? null,
    yoyGrowthPercent: item.YoyGrowthPercentage ?? null,
    dependencyRank: dependencyRankBySupplier.value.get(item.SupplierId) ?? null,
  })),
)

const filteredRows = computed(() => {
  const query = searchQuery.value.trim().toLowerCase()
  if (!query) return rows.value
  return rows.value.filter((row) =>
    `${row.principalName} ${row.supplierId}`.toLowerCase().includes(query),
  )
})

function currencyDisplay(value: number | null | undefined): string {
  return formatWithNoData(value, (v) => formatCurrency(v))
}

function percentDisplay(ratio: number | null | undefined): string {
  return formatWithNoData(ratio, (v) => formatPercent(v * 100))
}

function rankDisplay(rank: number | null): string {
  return formatWithNoData(rank, (v) => `#${formatNumber(v)}`)
}
</script>

<template>
  <section
    class="principal-detail-table"
    aria-label="Principal Detail Table"
    data-testid="principal-detail-table"
  >
    <div class="principal-detail-table__header">
      <div class="principal-detail-table__heading">
        <h2 class="principal-detail-table__title">Principal Detail Table</h2>
        <p class="principal-detail-table__note">
          Full per-Principal projection for validation. Columns are sortable and the
          search box filters the table by Principal. Missing values render
          "No Data" and are never shown as 0 or blank. Percent columns scale the
          stored ratio to percent; currency columns use stored amounts. Clicking a
          Principal opens its Performance Profile; the action links open the
          existing Sales-Out and Return evidence routes.
        </p>
      </div>
      <span class="p-input-icon-left principal-detail-table__search">
        <i class="pi pi-search" />
        <InputText
          v-model="searchQuery"
          placeholder="Search principal"
          data-testid="principal-detail-search"
        />
      </span>
    </div>

    <div v-if="loading" class="principal-detail-table__loading">
      <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
    </div>

    <DataTable
      v-else
      :value="filteredRows"
      striped-rows
      size="small"
      paginator
      :rows="10"
      removable-sort
      class="principal-detail-table__grid"
      data-testid="principal-detail-grid"
    >
      <template #empty>
        <p class="principal-detail-table__empty">
          No Principal detail for the current period.
        </p>
      </template>

      <Column field="principalName" header="Principal" sortable>
        <template #body="{ data }">
          <RouterLink
            class="principal-detail-table__principal-link"
            :to="{ name: 'supplier-performance-profile', params: { supplierId: data.supplierId } }"
          >
            {{ data.principalName }}
          </RouterLink>
        </template>
      </Column>
      <Column field="sales" header="Sales" sortable body-class="dash-numeric" header-class="dash-numeric">
        <template #body="{ data }">
          {{ currencyDisplay(data.sales) }}
        </template>
      </Column>
      <Column field="target" header="Target" sortable body-class="dash-numeric" header-class="dash-numeric">
        <template #body="{ data }">
          {{ currencyDisplay(data.target) }}
        </template>
      </Column>
      <Column
        field="achievementPercent"
        header="Achievement %"
        sortable
        body-class="dash-numeric"
        header-class="dash-numeric"
      >
        <template #body="{ data }">
          {{ percentDisplay(data.achievementPercent) }}
        </template>
      </Column>
      <Column
        field="achievementGap"
        header="Achievement Gap"
        sortable
        body-class="dash-numeric"
        header-class="dash-numeric"
      >
        <template #body="{ data }">
          {{ currencyDisplay(data.achievementGap) }}
        </template>
      </Column>
      <Column
        field="coveragePercent"
        header="Coverage %"
        sortable
        body-class="dash-numeric"
        header-class="dash-numeric"
      >
        <template #body="{ data }">
          {{ percentDisplay(data.coveragePercent) }}
        </template>
      </Column>
      <Column
        field="returnPercent"
        header="Return %"
        sortable
        body-class="dash-numeric"
        header-class="dash-numeric"
      >
        <template #body="{ data }">
          {{ percentDisplay(data.returnPercent) }}
        </template>
      </Column>
      <Column
        field="momGrowthPercent"
        header="MoM Growth"
        sortable
        body-class="dash-numeric"
        header-class="dash-numeric"
      >
        <template #body="{ data }">
          {{ percentDisplay(data.momGrowthPercent) }}
        </template>
      </Column>
      <Column
        field="yoyGrowthPercent"
        header="YoY Growth"
        sortable
        body-class="dash-numeric"
        header-class="dash-numeric"
      >
        <template #body="{ data }">
          {{ percentDisplay(data.yoyGrowthPercent) }}
        </template>
      </Column>
      <Column
        field="dependencyRank"
        header="Dependency Rank"
        sortable
        body-class="dash-numeric"
        header-class="dash-numeric"
      >
        <template #body="{ data }">
          {{ rankDisplay(data.dependencyRank) }}
        </template>
      </Column>
      <Column header="Actions">
        <template #body="{ data }">
          <div class="principal-detail-table__actions">
            <RouterLink
              class="principal-detail-table__action"
              :to="{ name: 'principal-performance-evidence', query: { supplierId: data.supplierId } }"
            >
              Sales-Out Evidence
            </RouterLink>
            <RouterLink
              class="principal-detail-table__action"
              :to="{
                name: 'principal-performance-return-evidence',
                query: { supplierId: data.supplierId },
              }"
            >
              Return Evidence
            </RouterLink>
          </div>
        </template>
      </Column>
    </DataTable>
  </section>
</template>

<style scoped>
.principal-detail-table {
  margin-top: 1rem;
  padding: 1rem;
  background: var(--p-surface-0);
  border: 1px solid var(--p-surface-200);
  border-radius: var(--dashboard-radius-sm);
  box-shadow: var(--dashboard-shadow-idle);
}

.principal-detail-table__header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
  flex-wrap: wrap;
}

.principal-detail-table__heading {
  flex: 1 1 auto;
  min-width: 0;
}

.principal-detail-table__title {
  margin: 0 0 0.5rem;
  font-size: 1rem;
}

.principal-detail-table__note {
  margin: 0 0 1rem;
  color: var(--p-text-muted-color);
  font-size: 0.875rem;
}

.principal-detail-table__search {
  min-width: 14rem;
}

.principal-detail-table__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.principal-detail-table__empty {
  margin: 0;
  padding: 1.5rem 0;
  text-align: center;
  color: var(--p-text-muted-color);
}

.principal-detail-table__grid :deep(.p-datatable-thead > tr > th) {
  background: var(--dashboard-table-header-bg);
  font-size: 0.8125rem;
  font-weight: 700;
  color: var(--p-text-muted-color);
  border-bottom: 1px solid var(--p-surface-200);
}

.principal-detail-table__grid :deep(.p-datatable-tbody > tr:hover) {
  background: var(--dashboard-table-row-hover);
}

.principal-detail-table__grid :deep(.dash-numeric) {
  text-align: right;
  font-variant-numeric: tabular-nums;
}

.principal-detail-table__principal-link {
  color: var(--p-primary-color, #2563eb);
  font-weight: 600;
  text-decoration: none;
}

.principal-detail-table__principal-link:hover {
  text-decoration: underline;
}

.principal-detail-table__actions {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.principal-detail-table__action {
  color: var(--p-primary-color, #2563eb);
  font-size: 0.8125rem;
  text-decoration: none;
  white-space: nowrap;
}

.principal-detail-table__action:hover {
  text-decoration: underline;
}
</style>
