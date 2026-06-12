<script setup lang="ts">
import { computed } from 'vue'
import { RouterLink } from 'vue-router'
import Card from 'primevue/card'
import type { DashboardAlertCenterInventoryRiskSummary } from '@/models/dashboard'
import { formatCurrency, formatPercent } from '@/services/formatters'
import { usePresentationStore } from '@/stores/presentationStore'

const props = defineProps<{
  summary: DashboardAlertCenterInventoryRiskSummary | null
}>()

const presentation = usePresentationStore()

const showUnavailableMessage = computed(
  () => props.summary != null && !props.summary.IsAvailable && !presentation.hidePlatformDiagnostics,
)
</script>

<template>
  <section id="alert-inventory" class="alert-center-inventory-risk">
    <h2 class="alert-center-inventory-risk__heading">Inventory Risk Summary</h2>
    <Card>
      <template #content>
        <div v-if="summary?.IsAvailable" class="alert-center-inventory-risk__metrics">
          <div class="alert-center-inventory-risk__metric">
            <span class="alert-center-inventory-risk__label">Dead Stock</span>
            <span class="alert-center-inventory-risk__value">
              {{ summary.DeadStockItemCount }} items · {{ formatCurrency(summary.DeadStockValue) }}
            </span>
          </div>
          <div class="alert-center-inventory-risk__metric">
            <span class="alert-center-inventory-risk__label">Slow Moving</span>
            <span class="alert-center-inventory-risk__value">
              {{ summary.SlowMovingItemCount }} items · {{ formatCurrency(summary.SlowMovingValue) }}
            </span>
          </div>
          <div class="alert-center-inventory-risk__metric">
            <span class="alert-center-inventory-risk__label">Never Sold</span>
            <span class="alert-center-inventory-risk__value">
              {{ summary.NeverSoldItemCount }} items · {{ formatCurrency(summary.NeverSoldValue) }}
            </span>
          </div>
          <div class="alert-center-inventory-risk__metric">
            <span class="alert-center-inventory-risk__label">At-Risk Inventory</span>
            <span class="alert-center-inventory-risk__value">
              {{ formatPercent(summary.AtRiskInventoryPercent) }}
            </span>
          </div>
        </div>
        <p v-else-if="showUnavailableMessage" class="alert-center-inventory-risk__unavailable">
          Inventory risk snapshot unavailable.
        </p>
        <RouterLink
          v-if="summary?.DashboardRoute"
          :to="summary.DashboardRoute"
          class="alert-center-inventory-risk__link"
        >
          View Inventory Risk →
        </RouterLink>
      </template>
    </Card>
  </section>
</template>

<style scoped>
.alert-center-inventory-risk__heading {
  margin: 0 0 0.75rem;
  font-size: 1.125rem;
}

.alert-center-inventory-risk__metrics {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1rem;
  margin-bottom: 1rem;
}

.alert-center-inventory-risk__metric {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.alert-center-inventory-risk__label {
  font-size: 0.875rem;
  color: var(--p-text-muted-color);
}

.alert-center-inventory-risk__value {
  font-weight: 600;
}

.alert-center-inventory-risk__link {
  display: inline-block;
  font-weight: 600;
  text-decoration: none;
}

.alert-center-inventory-risk__unavailable {
  margin: 0 0 1rem;
  color: var(--p-text-muted-color);
}
</style>
