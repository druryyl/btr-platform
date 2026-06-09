<script setup lang="ts">
import { RouterLink } from 'vue-router'
import Card from 'primevue/card'
import type { DashboardAlertCenterNavigationLinks } from '@/models/dashboard'

defineProps<{
  navigation: DashboardAlertCenterNavigationLinks | null
}>()
</script>

<template>
  <section class="alert-center-navigation">
    <h2 class="alert-center-navigation__heading">Domain Dashboards</h2>
    <Card>
      <template #content>
        <div v-if="navigation" class="alert-center-navigation__links">
          <RouterLink :to="navigation.ExecutiveDashboardRoute" class="alert-center-navigation__link">
            Executive
          </RouterLink>
          <RouterLink :to="navigation.SalesDashboardRoute" class="alert-center-navigation__link">
            Sales
          </RouterLink>
          <RouterLink :to="navigation.PiutangDashboardRoute" class="alert-center-navigation__link">
            Piutang
          </RouterLink>
          <RouterLink :to="navigation.CustomerDashboardRoute" class="alert-center-navigation__link">
            Customers
          </RouterLink>
          <RouterLink :to="navigation.SalesmanDashboardRoute" class="alert-center-navigation__link">
            Salesmen
          </RouterLink>
          <RouterLink :to="navigation.CollectionDashboardRoute" class="alert-center-navigation__link">
            Collection
          </RouterLink>
          <RouterLink :to="navigation.InventoryDashboardRoute" class="alert-center-navigation__link">
            Inventory
          </RouterLink>
          <RouterLink :to="navigation.InventoryRiskDashboardRoute" class="alert-center-navigation__link">
            Inventory Risk
          </RouterLink>
          <RouterLink :to="navigation.PurchasingDashboardRoute" class="alert-center-navigation__link">
            Purchasing
          </RouterLink>
          <RouterLink :to="navigation.LocationDashboardRoute" class="alert-center-navigation__link">
            Locations
          </RouterLink>
        </div>
      </template>
    </Card>
  </section>
</template>

<style scoped>
.alert-center-navigation__heading {
  margin: 0 0 0.75rem;
  font-size: 1.125rem;
}

.alert-center-navigation__links {
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem 1.25rem;
}

.alert-center-navigation__link {
  font-weight: 600;
  text-decoration: none;
}
</style>
