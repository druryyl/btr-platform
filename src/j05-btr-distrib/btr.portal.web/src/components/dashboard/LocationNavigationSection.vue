<script setup lang="ts">
import { RouterLink } from 'vue-router'
import Card from 'primevue/card'
import type { DashboardLocationNavigationLinks } from '@/models/dashboard'

defineProps<{
  navigation: DashboardLocationNavigationLinks | null
}>()
</script>

<template>
  <section class="location-navigation">
    <h2 class="location-navigation__heading">Navigation</h2>
    <Card>
      <template #content>
        <div class="location-navigation__links">
          <RouterLink
            v-if="navigation"
            :to="navigation.InventoryDashboardRoute"
            class="location-navigation__link"
          >
            Inventory Dashboard
          </RouterLink>
          <RouterLink
            v-if="navigation"
            :to="navigation.InventoryRiskDashboardRoute"
            class="location-navigation__link"
          >
            Inventory Risk Dashboard
          </RouterLink>
          <RouterLink
            v-if="navigation"
            :to="navigation.SalesDashboardRoute"
            class="location-navigation__link"
          >
            Sales Dashboard
          </RouterLink>
          <RouterLink
            v-if="navigation"
            :to="navigation.PurchasingDashboardRoute"
            class="location-navigation__link"
          >
            Purchasing Dashboard
          </RouterLink>
          <RouterLink
            v-if="navigation"
            :to="navigation.CollectionDashboardRoute"
            class="location-navigation__link"
          >
            Collection Dashboard
          </RouterLink>
          <RouterLink
            v-if="navigation"
            :to="navigation.CustomerDashboardRoute"
            class="location-navigation__link"
          >
            Customer Analytics
          </RouterLink>
          <RouterLink
            v-if="navigation"
            :to="navigation.SalesmanDashboardRoute"
            class="location-navigation__link"
          >
            Salesman Performance
          </RouterLink>
        </div>
      </template>
    </Card>
  </section>
</template>

<style scoped>
.location-navigation__heading {
  margin: 0 0 1rem;
  font-size: 1.25rem;
}

.location-navigation__links {
  display: flex;
  flex-wrap: wrap;
  gap: 1rem 2rem;
}

.location-navigation__link {
  color: var(--p-primary-color);
  text-decoration: none;
}

.location-navigation__link:hover {
  text-decoration: underline;
}
</style>
