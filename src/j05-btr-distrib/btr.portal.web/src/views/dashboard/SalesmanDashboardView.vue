<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import Message from 'primevue/message'
import DashboardDetailLayout from '@/components/dashboard/DashboardDetailLayout.vue'
import SalesmanAttentionCardGroup from '@/components/dashboard/SalesmanAttentionCardGroup.vue'
import SalesmanAttentionList from '@/components/dashboard/SalesmanAttentionList.vue'
import SalesmanSegmentationSection from '@/components/dashboard/SalesmanSegmentationSection.vue'
import SalesmanNavigationSection from '@/components/dashboard/SalesmanNavigationSection.vue'
import Top10RankingTable from '@/components/dashboard/Top10RankingTable.vue'
import { formatNumber, formatPercent } from '@/services/formatters'
import { navigateToReport } from '@/services/navigateToReport'
import { useDashboardStore } from '@/stores/dashboardStore'

const dashboard = useDashboardStore()
const router = useRouter()

const cards = computed(() => dashboard.salesman?.AttentionCards)
const unavailable = computed(() => dashboard.salesman != null && !dashboard.salesman.IsAvailable)

const omzetRankingRows = computed(() =>
  (dashboard.salesman?.PerformanceRankings?.TopOmzet ?? []).map((row) => ({
    Rank: row.Rank,
    SalesPersonCode: row.SalesPersonCode,
    SalesPersonName: row.SalesPersonName,
    Amount: row.Amount,
    PercentOfTotal: row.PercentOfTotal,
    ReportRoute: row.ReportRoute,
  })),
)

const achievementRankingRows = computed(() =>
  (dashboard.salesman?.PerformanceRankings?.TopAchievement ?? []).map((row) => ({
    Rank: row.Rank,
    SalesPersonCode: row.SalesPersonCode,
    SalesPersonName: row.SalesPersonName,
    Amount: row.Amount,
    PercentOfTotal: row.PercentOfTotal,
    AchievementPercent: row.AchievementPercent,
    TargetAmount: row.TargetAmount,
    ReportRoute: row.ReportRoute,
  })),
)

const piutangRankingRows = computed(() =>
  (dashboard.salesman?.ExposureRankings?.TopPiutang ?? []).map((row) => ({
    Rank: row.Rank,
    SalesPersonCode: row.SalesPersonCode,
    SalesPersonName: row.SalesPersonName,
    Amount: row.Amount,
    PercentOfTotal: row.PercentOfTotal,
    ReportRoute: row.ReportRoute,
  })),
)

const omzetColumns = [
  { field: 'Rank', header: 'Rank' },
  { field: 'SalesPersonCode', header: 'Code' },
  { field: 'SalesPersonName', header: 'Salesman' },
  { field: 'Amount', header: 'Omzet' },
  { field: 'PercentOfTotal', header: '% of Total' },
]

const achievementColumns = [
  { field: 'Rank', header: 'Rank' },
  { field: 'SalesPersonCode', header: 'Code' },
  { field: 'SalesPersonName', header: 'Salesman' },
  { field: 'AchievementPercent', header: 'Achievement %' },
  { field: 'Amount', header: 'Omzet' },
]

const piutangColumns = [
  { field: 'Rank', header: 'Rank' },
  { field: 'SalesPersonCode', header: 'Code' },
  { field: 'SalesPersonName', header: 'Salesman' },
  { field: 'Amount', header: 'Outstanding' },
  { field: 'PercentOfTotal', header: '% of Total' },
]

function onRankingRowClick(row: Record<string, unknown>): void {
  const salesmanName = String(row.SalesPersonName ?? '')
  const reportRoute = String(row.ReportRoute ?? '')
  if (salesmanName && reportRoute) {
    navigateToReport(router, reportRoute, salesmanName)
  }
}

onMounted(() => {
  void dashboard.loadSalesman()
})
</script>

<template>
  <DashboardDetailLayout
    title="Salesman Performance"
    subtitle="Which salesman requires management attention? Current month sales + open piutang by salesman."
    :loading="dashboard.loading"
    :error="dashboard.error"
    :generated-at="dashboard.salesman?.GeneratedAt"
    @refresh="dashboard.loadSalesman()"
  >
    <Message
      v-if="dashboard.salesman && !dashboard.salesman.IsDataFresh"
      severity="warn"
      :closable="false"
      class="salesman-dashboard__banner"
    >
      ⚠ Dashboard Data Not Fresh
    </Message>

    <section class="salesman-dashboard__section">
      <h2 class="salesman-dashboard__section-title">Attention Cards</h2>
      <div class="salesman-dashboard__cards">
        <SalesmanAttentionCardGroup
          title="Performance"
          icon="pi pi-chart-line"
          to="/dashboard/sales"
          :loading="dashboard.loading"
          :requires-attention="cards?.PerformanceRequiresAttention"
          :unavailable="unavailable"
        >
          <div class="metric">
            <span class="metric__label">Below Target</span>
            <span class="metric__value">
              {{ cards ? formatNumber(cards.BelowTargetCount) : '—' }}
            </span>
          </div>
          <div class="metric">
            <span class="metric__label">No Target</span>
            <span class="metric__value">
              {{ cards ? formatNumber(cards.NoTargetCount) : '—' }}
            </span>
          </div>
        </SalesmanAttentionCardGroup>

        <SalesmanAttentionCardGroup
          title="Collection Exposure"
          icon="pi pi-wallet"
          to="/dashboard/piutang"
          :loading="dashboard.loading"
          :requires-attention="cards?.CollectionRequiresAttention"
          :unavailable="unavailable"
        >
          <div class="metric">
            <span class="metric__label">High Overdue Exposure</span>
            <span class="metric__value">
              {{ cards ? formatNumber(cards.HighOverdueExposureCount) : '—' }}
            </span>
          </div>
          <div class="metric">
            <span class="metric__label">High Piutang Exposure</span>
            <span class="metric__value">
              {{ cards ? formatNumber(cards.HighPiutangExposureCount) : '—' }}
            </span>
          </div>
        </SalesmanAttentionCardGroup>

        <SalesmanAttentionCardGroup
          title="Portfolio"
          icon="pi pi-briefcase"
          href="#salesman-attention-list"
          :loading="dashboard.loading"
          :requires-attention="cards?.PortfolioRequiresAttention"
          :unavailable="unavailable"
        >
          <div class="metric">
            <span class="metric__label">Dormant Portfolio</span>
            <span class="metric__value">
              {{ cards ? formatNumber(cards.DormantPortfolioCount) : '—' }}
            </span>
          </div>
          <div class="metric">
            <span class="metric__label">Top Omzet Salesman %</span>
            <span class="metric__value">
              {{ cards?.TopOmzetSalesmanPercent != null ? formatPercent(cards.TopOmzetSalesmanPercent) : '—' }}
            </span>
          </div>
          <div class="metric">
            <span class="metric__label">Top Piutang Salesman %</span>
            <span class="metric__value">
              {{ cards?.TopPiutangSalesmanPercent != null ? formatPercent(cards.TopPiutangSalesmanPercent) : '—' }}
            </span>
          </div>
        </SalesmanAttentionCardGroup>
      </div>
    </section>

    <SalesmanAttentionList
      id="salesman-attention-list"
      class="salesman-dashboard__section"
      :items="dashboard.salesman?.AttentionList ?? []"
      :loading="dashboard.loading"
    />

    <section class="salesman-dashboard__section">
      <h2 class="salesman-dashboard__section-title">Performance Rankings</h2>
      <div class="salesman-dashboard__rankings">
        <Top10RankingTable
          title="Top 10 Omzet (current month)"
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
          title="Top 10 Achievement %"
          :columns="achievementColumns"
          :rows="achievementRankingRows"
          :loading="dashboard.loading"
          value-field="Amount"
          empty-message="No achievement ranking data."
          clickable
          @row-click="onRankingRowClick"
        />
      </div>
    </section>

    <section class="salesman-dashboard__section">
      <h2 class="salesman-dashboard__section-title">Exposure Rankings</h2>
      <Top10RankingTable
        title="Top 10 Piutang (all open)"
        :columns="piutangColumns"
        :rows="piutangRankingRows"
        :loading="dashboard.loading"
        value-field="Amount"
        percent-field="PercentOfTotal"
        empty-message="No piutang ranking data."
        clickable
        @row-click="onRankingRowClick"
      />
    </section>

    <SalesmanSegmentationSection
      class="salesman-dashboard__section"
      :by-wilayah="dashboard.salesman?.Segmentation?.ByWilayah ?? []"
      :by-segment="dashboard.salesman?.Segmentation?.BySegment ?? []"
      :active-summary="dashboard.salesman?.Segmentation?.ActiveSummary ?? null"
      :inactive-summary="dashboard.salesman?.Segmentation?.InactiveSummary ?? null"
      :loading="dashboard.loading"
    />

    <SalesmanNavigationSection
      class="salesman-dashboard__section"
      :navigation="dashboard.salesman?.Navigation ?? null"
    />
  </DashboardDetailLayout>
</template>

<style scoped>
.salesman-dashboard__banner {
  margin-bottom: 1rem;
}

.salesman-dashboard__section {
  margin-bottom: 1.5rem;
}

.salesman-dashboard__section-title {
  margin: 0 0 1rem;
  font-size: 1.25rem;
}

.salesman-dashboard__cards {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: 1rem;
}

.salesman-dashboard__rankings {
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
