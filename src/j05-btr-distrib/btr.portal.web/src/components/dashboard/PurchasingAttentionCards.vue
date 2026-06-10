<script setup lang="ts">
import type { DashboardPurchasingAttentionCardGroup } from '@/models/dashboard'
import PurchasingAttentionCardGroup from '@/components/dashboard/PurchasingAttentionCardGroup.vue'

defineProps<{
  cards: {
    PostingExposure: DashboardPurchasingAttentionCardGroup | null
    PrincipalDependency: DashboardPurchasingAttentionCardGroup | null
    PurchasingPace: DashboardPurchasingAttentionCardGroup | null
    InventoryCrossRisk: DashboardPurchasingAttentionCardGroup | null
  } | null
  loading: boolean
  unavailable: boolean
}>()

const emit = defineEmits<{
  filterBySignal: [signalKey: string]
}>()
</script>

<template>
  <div class="purchasing-attention-cards">
    <PurchasingAttentionCardGroup
      title="Posting Exposure"
      icon="pi pi-inbox"
      href="#purchasing-attention-list"
      :loading="loading"
      :requires-attention="cards?.PostingExposure?.RequiresAttention"
      :unavailable="unavailable"
      @anchor-navigate="emit('filterBySignal', 'QualifiedBacklog')"
    >
      <div
        v-for="(value, label) in cards?.PostingExposure?.Metrics ?? {}"
        :key="label"
        class="metric"
      >
        <span class="metric__label">{{ label }}</span>
        <span class="metric__value">{{ value }}</span>
      </div>
    </PurchasingAttentionCardGroup>

    <PurchasingAttentionCardGroup
      title="Principal Dependency"
      icon="pi pi-sitemap"
      href="#purchasing-attention-list"
      :loading="loading"
      :requires-attention="cards?.PrincipalDependency?.RequiresAttention"
      :unavailable="unavailable"
      @anchor-navigate="emit('filterBySignal', 'CompoundDependency')"
    >
      <div
        v-for="(value, label) in cards?.PrincipalDependency?.Metrics ?? {}"
        :key="label"
        class="metric"
      >
        <span class="metric__label">{{ label }}</span>
        <span class="metric__value">{{ value }}</span>
      </div>
    </PurchasingAttentionCardGroup>

    <PurchasingAttentionCardGroup
      title="Purchasing Pace"
      icon="pi pi-calendar"
      href="#purchasing-attention-list"
      :loading="loading"
      :requires-attention="cards?.PurchasingPace?.RequiresAttention"
      :unavailable="unavailable"
      @anchor-navigate="emit('filterBySignal', 'PurchasingInactivity')"
    >
      <div
        v-for="(value, label) in cards?.PurchasingPace?.Metrics ?? {}"
        :key="label"
        class="metric"
      >
        <span class="metric__label">{{ label }}</span>
        <span class="metric__value">{{ value }}</span>
      </div>
    </PurchasingAttentionCardGroup>

    <PurchasingAttentionCardGroup
      title="Inventory Cross-Risk"
      icon="pi pi-exclamation-triangle"
      href="#purchasing-attention-list"
      :loading="loading"
      :requires-attention="cards?.InventoryCrossRisk?.RequiresAttention"
      :unavailable="unavailable"
      @anchor-navigate="emit('filterBySignal', 'PrincipalInventoryNoPurchase')"
    >
      <div
        v-for="(value, label) in cards?.InventoryCrossRisk?.Metrics ?? {}"
        :key="label"
        class="metric"
      >
        <span class="metric__label">{{ label }}</span>
        <span class="metric__value">{{ value }}</span>
      </div>
    </PurchasingAttentionCardGroup>
  </div>
</template>

<style scoped>
.purchasing-attention-cards {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 1rem;
}

.metric {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  margin-bottom: 0.75rem;
}

.metric:last-child {
  margin-bottom: 0;
}

.metric__label {
  font-size: 0.875rem;
  color: var(--p-text-muted-color);
}

.metric__value {
  font-size: 1.1rem;
  font-weight: 600;
  color: var(--p-text-color);
}

@media (max-width: 1200px) {
  .purchasing-attention-cards {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (max-width: 640px) {
  .purchasing-attention-cards {
    grid-template-columns: 1fr;
  }
}
</style>
