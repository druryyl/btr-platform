<script setup lang="ts">
import Card from 'primevue/card'
import ProgressSpinner from 'primevue/progressspinner'
import type { DashboardDomain } from '@/services/dashboardDomains'

withDefaults(
  defineProps<{
    title: string
    icon?: string
    loading?: boolean
    domain?: DashboardDomain
    hero?: boolean
  }>(),
  {
    hero: false,
  },
)
</script>

<template>
  <Card
    class="kpi-card"
    :class="{
      'kpi-card--domain': domain,
      'kpi-card--hero': hero,
    }"
    :data-domain="domain"
  >
    <template #title>
      <span class="kpi-card__title">
        <span v-if="icon" class="kpi-card__icon-wrap">
          <i :class="icon" class="kpi-card__icon" aria-hidden="true" />
        </span>
        {{ title }}
      </span>
    </template>
    <template #content>
      <div v-if="loading" class="kpi-card__loading">
        <ProgressSpinner style="width: 2rem; height: 2rem" stroke-width="4" />
      </div>
      <div v-else class="kpi-card__content">
        <slot />
      </div>
    </template>
  </Card>
</template>

<style scoped>
.kpi-card {
  height: 100%;
  border-radius: var(--dashboard-radius);
  box-shadow: var(--dashboard-shadow-idle);
  transition:
    box-shadow var(--dashboard-transition),
    transform var(--dashboard-transition);
  overflow: hidden;
}

.kpi-card:hover {
  box-shadow: var(--dashboard-shadow-hover);
  transform: translateY(-2px);
}

.kpi-card--domain {
  background: var(--dashboard-domain-tint, var(--p-surface-0));
  border-top: 3px solid var(--dashboard-domain-color, var(--p-primary-color));
}

.kpi-card--domain :deep(.p-card-body) {
  background: inherit;
}

.kpi-card--hero :deep(.p-card-title) {
  font-size: 1.15rem;
}

.kpi-card--hero :deep(.p-card-body) {
  padding-top: 0.25rem;
}

.kpi-card--hero .kpi-card__icon-wrap {
  width: 2.125rem;
  height: 2.125rem;
}

.kpi-card--hero .kpi-card__icon {
  font-size: 1.0625rem;
}

.kpi-card__title {
  display: inline-flex;
  align-items: center;
  gap: 0.625rem;
  font-size: 1.05rem;
  font-weight: 700;
}

.kpi-card__icon-wrap {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 1.875rem;
  height: 1.875rem;
  border-radius: var(--dashboard-radius-chip);
  background: color-mix(in srgb, var(--dashboard-domain-color, var(--p-primary-color)) 14%, white);
  flex-shrink: 0;
}

.kpi-card__icon {
  font-size: 0.9375rem;
  color: var(--dashboard-domain-color, var(--p-primary-color));
}

.kpi-card__loading {
  display: flex;
  justify-content: center;
  padding: 1.5rem 0;
}

.kpi-card__content {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}
</style>
