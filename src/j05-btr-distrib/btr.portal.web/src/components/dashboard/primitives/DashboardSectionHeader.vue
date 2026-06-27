<script setup lang="ts">
import type { DashboardDomain } from '@/services/dashboardDomains'

defineProps<{
  title: string
  icon?: string
  domain?: DashboardDomain
}>()
</script>

<template>
  <div
    class="dashboard-section-header"
    :data-domain="domain"
  >
    <span v-if="icon" class="dashboard-section-header__icon-wrap">
      <i :class="icon" class="dashboard-section-header__icon" aria-hidden="true" />
    </span>
    <h2 class="dashboard-section-header__title">{{ title }}</h2>
  </div>
</template>

<style scoped>
.dashboard-section-header {
  display: flex;
  align-items: center;
  gap: 0.625rem;
  margin: 0 0 1.125rem;
  padding-left: 0.75rem;
  border-left: 3px solid var(--dashboard-domain-color, var(--p-primary-color));
}

.dashboard-section-header__icon-wrap {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 1.75rem;
  height: 1.75rem;
  border-radius: var(--dashboard-radius-sm);
  background: color-mix(in srgb, var(--dashboard-domain-color, var(--p-primary-color)) 12%, white);
  flex-shrink: 0;
}

.dashboard-section-header__icon {
  font-size: 0.875rem;
  color: var(--dashboard-domain-color, var(--p-primary-color));
}

.dashboard-section-header__title {
  margin: 0;
  font-size: 1.125rem;
  font-weight: 700;
  letter-spacing: -0.01em;
  color: var(--p-text-color);
}
</style>
