<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import Message from 'primevue/message'
import DashboardDetailLayout from '@/components/dashboard/DashboardDetailLayout.vue'
import CustomerAttentionCardGroup from '@/components/dashboard/CustomerAttentionCardGroup.vue'
import CustomerAttentionList from '@/components/dashboard/CustomerAttentionList.vue'
import CustomerSegmentationSection from '@/components/dashboard/CustomerSegmentationSection.vue'
import CustomerNavigationSection from '@/components/dashboard/CustomerNavigationSection.vue'
import Top10RankingTable from '@/components/dashboard/Top10RankingTable.vue'
import { formatCurrency, formatNumber, formatPercent } from '@/services/formatters'
import { navigateToReport } from '@/services/navigateToReport'
import { useDashboardStore } from '@/stores/dashboardStore'
const dashboard = useDashboardStore()
const router = useRouter()

const cards = computed(() => dashboard.customer?.AttentionCards)
const unavailable = computed(() => dashboard.customer != null && !dashboard.customer.IsAvailable)

const omzetRankingRows = computed(() =>
  (dashboard.customer?.Rankings?.TopOmzet ?? []).map((row) => ({
    Rank: row.Rank,
    CustomerCode: row.CustomerCode,
    CustomerName: row.CustomerName,
    Amount: row.Amount,
    PercentOfTotal: row.PercentOfTotal,
    ReportRoute: row.ReportRoute,
  })),
)

const piutangRankingRows = computed(() =>
  (dashboard.customer?.Rankings?.TopPiutang ?? []).map((row) => ({
    Rank: row.Rank,
    CustomerCode: row.CustomerCode,
    CustomerName: row.CustomerName,
    Amount: row.Amount,
    PercentOfTotal: row.PercentOfTotal,
    ReportRoute: row.ReportRoute,
  })),
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

function onRankingRowClick(row: Record<string, unknown>): void {
  const customerName = String(row.CustomerName ?? '')
  const reportRoute = String(row.ReportRoute ?? '')
  if (customerName && reportRoute) {
    navigateToReport(router, reportRoute, customerName)
  }
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
    @refresh="dashboard.loadCustomer()"
  >
    <Message
      v-if="dashboard.customer && !dashboard.customer.IsDataFresh"
      severity="warn"
      :closable="false"
      class="customer-dashboard__banner"
    >
      ⚠ Dashboard Data Not Fresh
    </Message>

    <section class="customer-dashboard__section">
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
          :loading="dashboard.loading"
          :requires-attention="cards?.InactivityRequiresAttention"
          :unavailable="unavailable"
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

    <CustomerAttentionList
      id="customer-attention-list"
      class="customer-dashboard__section"
      :items="dashboard.customer?.AttentionList ?? []"
      :loading="dashboard.loading"
    />

    <section class="customer-dashboard__section">
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

.customer-dashboard__section {
  margin-bottom: 1.5rem;
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
