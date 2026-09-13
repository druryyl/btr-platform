<script setup lang="ts">
import { computed } from 'vue'
import ProgressSpinner from 'primevue/progressspinner'
import type { PrincipalPerformanceRankingItem } from '@/models/dashboard'
import { formatWithNoData } from '@/services/principalNoData'
import { formatCurrency, formatPercent } from '@/services/formatters'

const props = defineProps<{
  ranking: PrincipalPerformanceRankingItem[]
  loading: boolean
}>()

interface AchievementGapEntry {
  rank: number | null
  item: PrincipalPerformanceRankingItem
  gapAmount: number | null
  barWidthPercent: number
}

const entries = computed<AchievementGapEntry[]>(() => {
  const rows = props.ranking.map((item, index) => ({
    item,
    index,
    gapAmount: item.AchievementAmount ?? null,
  }))

  const withValue = rows.filter((row) => row.gapAmount != null)
  const withoutValue = rows.filter((row) => row.gapAmount == null)

  withValue.sort((a, b) => {
    const difference = (b.gapAmount as number) - (a.gapAmount as number)
    if (difference !== 0) return difference
    return a.index - b.index
  })

  const ordered = [...withValue, ...withoutValue]
  const maxAbsolute = withValue.reduce(
    (max, row) => Math.max(max, Math.abs(row.gapAmount as number)),
    0,
  )

  let nextRank = 0
  return ordered.map((row) => {
    if (row.gapAmount == null) {
      return { rank: null, item: row.item, gapAmount: null, barWidthPercent: 0 }
    }
    nextRank += 1
    return {
      rank: nextRank,
      item: row.item,
      gapAmount: row.gapAmount,
      barWidthPercent: maxAbsolute > 0 ? (Math.abs(row.gapAmount) / maxAbsolute) * 100 : 0,
    }
  })
})

function currencyDisplay(value: number | null | undefined): string {
  return formatWithNoData(value, (v) => formatCurrency(v))
}

function percentDisplay(ratio: number | null | undefined): string {
  return formatWithNoData(ratio, (v) => formatPercent(v * 100))
}

function rankDisplay(rank: number | null): string {
  return rank == null ? 'No Data' : String(rank)
}

function isTopEntry(entry: AchievementGapEntry): boolean {
  return entry.rank != null && entry.rank <= 3
}
</script>

<template>
  <section
    class="principal-achievement-gap"
    aria-label="Achievement Gap Leaderboard"
    data-testid="achievement-gap-leaderboard"
  >
    <h2 class="principal-achievement-gap__title">Achievement Gap Leaderboard</h2>
    <p class="principal-achievement-gap__note">
      Ranked by governed Achievement Amount (PRN-TGT-002), consumed as-is.
      Gap Amount is not recalculated as Target minus Sales.
    </p>

    <div v-if="loading" class="principal-achievement-gap__loading">
      <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
    </div>

    <p v-else-if="entries.length === 0" class="principal-achievement-gap__empty">
      No achievement gap data for the current period.
    </p>

    <ul v-else class="principal-achievement-gap__list">
      <li
        v-for="entry in entries"
        :key="entry.item.SupplierId"
        class="principal-achievement-gap__item"
        :class="{ 'principal-achievement-gap__item--top': isTopEntry(entry) }"
        :data-supplier-id="entry.item.SupplierId"
      >
        <span class="principal-achievement-gap__rank" :class="{ '--empty': entry.rank == null }">
          {{ rankDisplay(entry.rank) }}
        </span>
        <div class="principal-achievement-gap__body">
          <div class="principal-achievement-gap__headline">
            <span class="principal-achievement-gap__principal">
              {{ entry.item.PrincipalName || entry.item.SupplierId }}
            </span>
            <span class="principal-achievement-gap__gap">
              Gap Amount: {{ currencyDisplay(entry.gapAmount) }}
            </span>
          </div>
          <div class="principal-achievement-gap__track" aria-hidden="true">
            <div
              class="principal-achievement-gap__bar"
              :style="{ width: `${entry.barWidthPercent}%` }"
            />
          </div>
          <dl class="principal-achievement-gap__metrics">
            <div class="principal-achievement-gap__metric">
              <dt>Achievement %</dt>
              <dd>{{ percentDisplay(entry.item.AchievementPercentage) }}</dd>
            </div>
            <div class="principal-achievement-gap__metric">
              <dt>Sales</dt>
              <dd>{{ currencyDisplay(entry.item.PrincipalSalesOutAmount) }}</dd>
            </div>
            <div class="principal-achievement-gap__metric">
              <dt>Target</dt>
              <dd>{{ currencyDisplay(entry.item.PrincipalTargetAmount) }}</dd>
            </div>
          </dl>
        </div>
      </li>
    </ul>
  </section>
</template>

<style scoped>
.principal-achievement-gap {
  margin-top: 1rem;
  padding: 1rem;
  background: var(--p-surface-0);
  border: 1px solid var(--p-surface-200);
  border-radius: var(--dashboard-radius-sm);
  box-shadow: var(--dashboard-shadow-idle);
}

.principal-achievement-gap__title {
  margin: 0 0 0.5rem;
  font-size: 1rem;
}

.principal-achievement-gap__note {
  margin: 0 0 1rem;
  color: var(--p-text-muted-color);
  font-size: 0.875rem;
}

.principal-achievement-gap__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.principal-achievement-gap__empty {
  margin: 0;
  padding: 1rem 0;
  color: var(--p-text-muted-color);
}

.principal-achievement-gap__list {
  margin: 0;
  padding: 0;
  list-style: none;
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.principal-achievement-gap__item {
  display: flex;
  align-items: flex-start;
  gap: 0.75rem;
  padding: 0.75rem;
  border: 1px solid var(--p-surface-200);
  border-radius: var(--dashboard-radius-sm);
}

.principal-achievement-gap__item--top {
  border-left: 4px solid var(--p-primary-color, #2563eb);
  background: color-mix(in srgb, var(--p-primary-color, #2563eb) 4%, white);
}

.principal-achievement-gap__rank {
  flex: 0 0 auto;
  min-width: 2rem;
  height: 2rem;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border-radius: 999px;
  background: var(--p-surface-100, #f1f5f9);
  color: var(--p-text-color);
  font-weight: 700;
  font-variant-numeric: tabular-nums;
}

.principal-achievement-gap__rank.--empty {
  border-radius: var(--dashboard-radius-sm);
  background: transparent;
  color: var(--p-text-muted-color);
  font-size: 0.75rem;
  font-weight: 400;
}

.principal-achievement-gap__body {
  flex: 1 1 auto;
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
}

.principal-achievement-gap__headline {
  display: flex;
  align-items: baseline;
  justify-content: space-between;
  gap: 0.75rem;
  flex-wrap: wrap;
}

.principal-achievement-gap__principal {
  font-weight: 600;
  color: var(--p-text-color);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.principal-achievement-gap__gap {
  font-variant-numeric: tabular-nums;
  color: var(--p-text-color);
  white-space: nowrap;
}

.principal-achievement-gap__track {
  width: 100%;
  height: 0.5rem;
  border-radius: 999px;
  background: var(--p-surface-100, #f1f5f9);
  overflow: hidden;
}

.principal-achievement-gap__bar {
  height: 100%;
  border-radius: 999px;
  background: var(--p-primary-color, #2563eb);
}

.principal-achievement-gap__metrics {
  margin: 0;
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 0.5rem 1rem;
}

.principal-achievement-gap__metric {
  display: flex;
  flex-direction: column;
  gap: 0.1rem;
  min-width: 0;
}

.principal-achievement-gap__metric dt {
  font-size: 0.6875rem;
  font-weight: 600;
  color: var(--p-text-muted-color, #64748b);
}

.principal-achievement-gap__metric dd {
  margin: 0;
  font-variant-numeric: tabular-nums;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

@media (max-width: 600px) {
  .principal-achievement-gap__metrics {
    grid-template-columns: 1fr;
  }
}
</style>
