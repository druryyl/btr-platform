<script setup lang="ts">
import { computed } from 'vue'
import Top10RankingTable from '@/components/dashboard/Top10RankingTable.vue'
import type { DashboardExecutiveRiskItem } from '@/models/dashboard'

const props = defineProps<{
  title: string
  items: DashboardExecutiveRiskItem[]
  loading: boolean
  nameHeader?: string
  amountHeader?: string
}>()

const rows = computed(() => props.items as unknown as Record<string, unknown>[])
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
    :empty-message="`No ${title.toLowerCase()} data available.`"
  />
</template>
