<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import Message from 'primevue/message'
import DashboardDetailLayout from '@/components/dashboard/DashboardDetailLayout.vue'
import CustomerAttentionCardGroup from '@/components/dashboard/CustomerAttentionCardGroup.vue'
import CustomerAttentionList from '@/components/dashboard/CustomerAttentionList.vue'
import CustomerSegmentationSection from '@/components/dashboard/CustomerSegmentationSection.vue'
import CustomerNavigationSection from '@/components/dashboard/CustomerNavigationSection.vue'
import Top10RankingTable from '@/components/dashboard/Top10RankingTable.vue'
import type { DashboardCustomerRankingRow } from '@/models/dashboard'
import { formatCurrency, formatNumber, formatPercent } from '@/services/formatters'
import { CUSTOMER_ATTENTION_SIGNAL_ALL } from '@/services/customerAttentionSignals'
import { resolveInvestigationSourceLabel } from '@/services/investigationSourceLabels'
import { navigateToInvestigation } from '@/services/navigateToInvestigation'
import { useDashboardStore } from '@/stores/dashboardStore'

const dashboard = useDashboardStore()
const router = useRouter()
const sourceLabel = resolveInvestigationSourceLabel('/dashboard/customers')
const attentionSignalFilter = ref(CUSTOMER_ATTENTION_SIGNAL_ALL)

const cards = computed(() => dashboard.customer?.AttentionCards)
const unavailable = computed(() => dashboard.customer != null && !dashboard.customer.IsAvailable)

const omzetRankingRows = computed(
  () => (dashboard.customer?.Rankings?.TopOmzet ?? []) as Record<string, unknown>[],
)

const piutangRankingRows = computed(
  () => (dashboard.customer?.Rankings?.TopPiutang ?? []) as Record<string, unknown>[],
)

const omzetColumns = [
  { field: 'Rank', header: 'Rank' },
  { field: 'CustomerCode', header: 'Code' },
  { field: 'CustomerName', header: 'Customer' },
  { field: 'Amount', header: 'Omzet' },
  { field: 'PercentOfTotal', header: '% of Total' },
]

const piutangColumns = [
  { field: 'Rank', header: 'Rank' },
  { field: 'CustomerCode', header: 'Code' },
  { field: 'CustomerName', header: 'Customer' },
  { field: 'Amount', header: 'Outstanding' },
  { field: 'PercentOfTotal', header: '% of Total' },
]

const sectionNavItems = [
  { id: 'customer-attention-cards', label: 'Attention Cards' },
  { id: 'customer-attention-list', label: 'Attention List' },
  { id: 'customer-rankings', label: 'Rankings' },
  { id: 'customer-segmentation', label: 'Segmentation' },
]

function onRankingRowClick(row: Record<string, unknown>): void {
  const item = row as unknown as DashboardCustomerRankingRow
  if (!item.Investigation) return
  navigateToInvestigation(router, item.Investigation, sourceLabel)
}

function setAttentionSignalFilter(signalKey: string): void {
  attentionSignalFilter.value = signalKey
}

function onRefresh(): void {
  attentionSignalFilter.value = CUSTOMER_ATTENTION_SIGNAL_ALL
  void dashboard.loadCustomer()
}

onMounted(() => {
  void dashboard.loadCustomer()
})
</script>

<template>
  <DashboardDetailLayout
    title="Customer Analytics"
    subtitle="Which customers require management attention?"
    :loading="dashboard.loading"
    :error="dashboard.error"
    :generated-at="dashboard.customer?.GeneratedAt"
    @refresh="onRefresh"
  >
    <Message
      v-if="dashboard.customer && !dashboard.customer.IsDataFresh"
      severity="warn"
      :closable="false"
      class="customer-dashboard__banner"
    >
      ⚠ Dashboard Data Not Fresh
    </Message>

    <nav class="customer-dashboard__section-nav" aria-label="Dashboard sections">
      <a
        v-for="item in sectionNavItems"
        :key="item.id"
        :href="`#${item.id}`"
        class="customer-dashboard__section-nav-link"
      >
        {{ item.label }}
      </a>
    </nav>

    <section id="customer-attention-cards" class="customer-dashboard__section">
      <h2 class="customer-dashboard__section-title">Attention Cards</h2>
      <div class="customer-dashboard__cards">
        <CustomerAttentionCardGroup
          title="Collection"
          icon="pi pi-wallet"
          to="/dashboard/piutang"
          :loading="dashboard.loading"
          :requires-attention="cards?.CollectionRequiresAttention"
          :unavailable="unavailable"
        >
          <div class="metric">
            <span class="metric__label">Overdue Customers</span>
            <span class="metric__value">
              {{ cards ? formatNumber(cards.OverdueCustomerCount) : '—' }}
            </span>
          </div>
          <div class="metric">
            <span class="metric__label">&gt;90 Day Exposure</span>
            <span class="metric__value">
              {{ cards ? formatCurrency(cards.AgingOver90Amount) : '—' }}
            </span>
          </div>
        </CustomerAttentionCardGroup>

        <CustomerAttentionCardGroup
          title="Concentration"
          icon="pi pi-chart-pie"
          :loading="dashboard.loading"
          :unavailable="unavailable"
        >
          <div class="metric">
            <span class="metric__label">Top Omzet Customer %</span>
            <span class="metric__value">
              {{ cards?.TopOmzetCustomerPercent != null ? formatPercent(cards.TopOmzetCustomerPercent) : '—' }}
            </span>
          </div>
          <div class="metric">
            <span class="metric__label">Top Piutang Customer %</span>
            <span class="metric__value">
              {{ cards?.TopPiutangCustomerPercent != null ? formatPercent(cards.TopPiutangCustomerPercent) : '—' }}
            </span>
          </div>
        </CustomerAttentionCardGroup>

        <CustomerAttentionCardGroup
          title="Activity"
          icon="pi pi-check-circle"
          to="/dashboard/sales"
          :loading="dashboard.loading"
          :unavailable="unavailable"
        >
          <div class="metric">
            <span class="metric__label">Active Customers (month)</span>
            <span class="metric__value">
              {{ cards ? formatNumber(cards.ActiveCustomerCount) : '—' }}
            </span>
          </div>
        </CustomerAttentionCardGroup>

        <CustomerAttentionCardGroup
          title="Inactivity"
          icon="pi pi-clock"
          href="#customer-attention-list"
          :loading="dashboard.loading"
          :requires-attention="cards?.InactivityRequiresAttention"
          :unavailable="unavailable"
          @anchor-navigate="setAttentionSignalFilter('Dormant')"
        >
          <div class="metric">
            <span class="metric__label">Dormant Customers (90-day)</span>
            <span class="metric__value">
              {{ cards ? formatNumber(cards.DormantCustomerCount) : '—' }}
            </span>
          </div>
        </CustomerAttentionCardGroup>

        <CustomerAttentionCardGroup
          title="Credit"
          icon="pi pi-shield"
          href="#customer-attention-list"
          :loading="dashboard.loading"
          :requires-attention="cards?.CreditRequiresAttention"
          :unavailable="unavailable"
          @anchor-navigate="setAttentionSignalFilter('PlafondBreach')"
        >
          <div class="metric">
            <span class="metric__label">Plafond Breach</span>
            <span class="metric__value">
              {{ cards ? formatNumber(cards.PlafondBreachCount) : '—' }}
            </span>
          </div>
          <div class="metric">
            <span class="metric__label">Suspended + Sales</span>
            <span class="metric__value">
              {{ cards ? formatNumber(cards.SuspendedWithSalesCount) : '—' }}
            </span>
          </div>
        </CustomerAttentionCardGroup>
      </div>
    </section>

    <section id="customer-attention-list" class="customer-dashboard__section">
      <CustomerAttentionList
        v-model:signal-filter="attentionSignalFilter"
        :items="dashboard.customer?.AttentionList ?? []"
        :loading="dashboard.loading"
      />
    </section>

    <section id="customer-rankings" class="customer-dashboard__section">
      <h2 class="customer-dashboard__section-title">Top Customer Rankings</h2>
      <div class="customer-dashboard__rankings">
        <Top10RankingTable
          title="Top 10 by Omzet (current month)"
          :columns="omzetColumns"
          :rows="omzetRankingRows"
          :loading="dashboard.loading"
          value-field="Amount"
          percent-field="PercentOfTotal"
          empty-message="No omzet ranking data."
          clickable
          @row-click="onRankingRowClick"
        />
        <Top10RankingTable
          title="Top 10 by Piutang (all open)"
          :columns="piutangColumns"
          :rows="piutangRankingRows"
          :loading="dashboard.loading"
          value-field="Amount"
          percent-field="PercentOfTotal"
          empty-message="No piutang ranking data."
          clickable
          @row-click="onRankingRowClick"
        />
      </div>
    </section>

    <CustomerSegmentationSection
      id="customer-segmentation"
      class="customer-dashboard__section"
      :by-klasifikasi="dashboard.customer?.Segmentation?.ByKlasifikasi ?? []"
      :by-wilayah="dashboard.customer?.Segmentation?.ByWilayah ?? []"
      :active-summary="dashboard.customer?.Segmentation?.ActiveSummary ?? null"
      :dormant-summary="dashboard.customer?.Segmentation?.DormantSummary ?? null"
      :loading="dashboard.loading"
    />

    <CustomerNavigationSection
      class="customer-dashboard__section"
      :navigation="dashboard.customer?.Navigation ?? null"
    />
  </DashboardDetailLayout>
</template>

<style scoped>
.customer-dashboard__banner {
  margin-bottom: 1rem;
}

.customer-dashboard__section-nav {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem 1rem;
  position: sticky;
  top: 0;
  z-index: 2;
  margin-bottom: 1.5rem;
  padding: 0.75rem 0;
  background: var(--p-content-background);
  border-bottom: 1px solid var(--p-content-border-color);
}

.customer-dashboard__section-nav-link {
  font-size: 0.875rem;
  font-weight: 600;
  color: var(--p-primary-color);
  text-decoration: none;
}

.customer-dashboard__section-nav-link:hover {
  text-decoration: underline;
}

.customer-dashboard__section {
  margin-bottom: 1.5rem;
  scroll-margin-top: 3.5rem;
}

.customer-dashboard__section-title {
  margin: 0 0 1rem;
  font-size: 1.25rem;
}

.customer-dashboard__cards {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: 1rem;
}

.customer-dashboard__rankings {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(320px, 1fr));
  gap: 1rem;
}

.metric__label {
  display: block;
  font-size: 0.875rem;
  color: var(--p-text-muted-color);
}

.metric__value {
  display: block;
  font-size: 1.25rem;
  font-weight: 700;
  margin-top: 0.25rem;
}
</style>
