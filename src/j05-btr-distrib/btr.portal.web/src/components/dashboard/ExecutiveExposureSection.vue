<script setup lang="ts">
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Top10RankingTable from '@/components/dashboard/Top10RankingTable.vue'
import type { DashboardExecutiveRiskItem } from '@/models/dashboard'
import { resolveInvestigationSourceLabel } from '@/services/investigationSourceLabels'
import { navigateToInvestigation } from '@/services/navigateToInvestigation'

const props = defineProps<{
  title: string
  items: DashboardExecutiveRiskItem[]
  loading: boolean
  nameHeader?: string
  amountHeader?: string
}>()

const router = useRouter()
const route = useRoute()

const rows = computed(() => props.items as unknown as Record<string, unknown>[])

function onRowClick(row: Record<string, unknown>): void {
  const item = row as unknown as DashboardExecutiveRiskItem
  if (!item.Investigation) return

  navigateToInvestigation(
    router,
    item.Investigation,
    resolveInvestigationSourceLabel(route.path),
  )
}
</script>

<template>
  <Top10RankingTable
    :title="title"
    :columns="[
      { field: 'Rank', header: '#' },
      { field: 'Name', header: nameHeader ?? 'Name' },
      { field: 'Amount', header: amountHeader ?? 'Amount' },
    ]"
    :rows="rows"
    :loading="loading"
    value-field="Amount"
    clickable
    :empty-message="`No ${title.toLowerCase()} data available.`"
    @row-click="onRowClick"
  />
</template>
