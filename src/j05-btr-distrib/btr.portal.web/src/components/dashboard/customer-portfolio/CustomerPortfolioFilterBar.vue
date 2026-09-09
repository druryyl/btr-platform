<script setup lang="ts">
import Select from 'primevue/select'
import SelectButton from 'primevue/selectbutton'
import {
  CU04_LAST_INVOICING_SALESMAN_FILTER_ALL,
  CU04_LAST_INVOICING_SALESMAN_LABEL,
  CU04_LAST_INVOICING_SALESMAN_NOTE,
} from '@/services/customerAnalyticsAttribution'
import type { CustomerPortfolioFilterState } from '@/services/customerPortfolioSignals'
import {
  CUSTOMER_PORTFOLIO_ACTION_LABELS,
  CUSTOMER_PORTFOLIO_LIFECYCLE_LABELS,
  CUSTOMER_PORTFOLIO_TIER_LABELS,
  CUSTOMER_PORTFOLIO_VIEW_ALL,
  CUSTOMER_PORTFOLIO_VIEW_ATTENTION,
} from '@/services/customerPortfolioSignals'

const filters = defineModel<CustomerPortfolioFilterState>('filters', { required: true })

defineProps<{
  options: {
    wilayah: string[]
    klasifikasi: string[]
    tier: string[]
    lifecycle: string[]
    action: string[]
    salesman: string[]
  }
}>()

const viewOptions = [
  { label: 'Attention Only', value: CUSTOMER_PORTFOLIO_VIEW_ATTENTION },
  { label: 'All Customers', value: CUSTOMER_PORTFOLIO_VIEW_ALL },
]

function mapOptions(values: string[], allLabel: string, labelMap?: Record<string, string>) {
  return [
    { label: allLabel, value: '' },
    ...values.map((value) => ({
      label: labelMap?.[value] ?? value,
      value,
    })),
  ]
}
</script>

<template>
  <section class="customer-portfolio-filter-bar">
    <div class="customer-portfolio-filter-bar__view">
      <span class="customer-portfolio-filter-bar__label">View</span>
      <SelectButton v-model="filters.view" :options="viewOptions" option-label="label" option-value="value" />
    </div>

    <div class="customer-portfolio-filter-bar__filters">
      <Select
        v-model="filters.wilayah"
        :options="mapOptions(options.wilayah, 'All Wilayah')"
        option-label="label"
        option-value="value"
        placeholder="Wilayah"
        class="customer-portfolio-filter-bar__select"
      />
      <Select
        v-model="filters.klasifikasi"
        :options="mapOptions(options.klasifikasi, 'All Klasifikasi')"
        option-label="label"
        option-value="value"
        placeholder="Klasifikasi"
        class="customer-portfolio-filter-bar__select"
      />
      <Select
        v-model="filters.tier"
        :options="mapOptions(options.tier, 'All Tiers', CUSTOMER_PORTFOLIO_TIER_LABELS)"
        option-label="label"
        option-value="value"
        placeholder="Tier"
        class="customer-portfolio-filter-bar__select"
      />
      <Select
        v-model="filters.lifecycle"
        :options="mapOptions(options.lifecycle, 'All Lifecycles', CUSTOMER_PORTFOLIO_LIFECYCLE_LABELS)"
        option-label="label"
        option-value="value"
        placeholder="Lifecycle"
        class="customer-portfolio-filter-bar__select"
      />
      <Select
        v-model="filters.action"
        :options="mapOptions(options.action, 'All Actions', CUSTOMER_PORTFOLIO_ACTION_LABELS)"
        option-label="label"
        option-value="value"
        placeholder="Action"
        class="customer-portfolio-filter-bar__select"
      />
      <Select
        v-model="filters.salesman"
        :options="mapOptions(options.salesman, CU04_LAST_INVOICING_SALESMAN_FILTER_ALL)"
        option-label="label"
        option-value="value"
        :placeholder="CU04_LAST_INVOICING_SALESMAN_LABEL"
        class="customer-portfolio-filter-bar__select"
      />
    </div>
    <p class="customer-portfolio-filter-bar__note">{{ CU04_LAST_INVOICING_SALESMAN_NOTE }}</p>
  </section>
</template>

<style scoped>
.customer-portfolio-filter-bar {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  margin-bottom: 1rem;
  padding: 1rem;
  border: 1px solid var(--p-surface-200);
  border-radius: 0.75rem;
  background: var(--p-surface-0);
}

.customer-portfolio-filter-bar__view {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  flex-wrap: wrap;
}

.customer-portfolio-filter-bar__label {
  font-size: 0.875rem;
  font-weight: 600;
  color: var(--p-text-muted-color);
}

.customer-portfolio-filter-bar__filters {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 0.75rem;
}

.customer-portfolio-filter-bar__note {
  margin: 0;
  font-size: 0.8125rem;
  line-height: 1.4;
  color: var(--p-text-muted-color);
}

.customer-portfolio-filter-bar__select {
  width: 100%;
}

@media (max-width: 960px) {
  .customer-portfolio-filter-bar__filters {
    grid-template-columns: 1fr;
  }
}
</style>
