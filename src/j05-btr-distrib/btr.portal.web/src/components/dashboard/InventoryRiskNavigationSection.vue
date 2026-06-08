<script setup lang="ts">
import { RouterLink } from 'vue-router'
import Card from 'primevue/card'
import type { DashboardInventoryRiskNavigationLinks } from '@/models/dashboard'

defineProps<{
  navigation: DashboardInventoryRiskNavigationLinks | null
}>()
</script>

<template>
  <section class="inventory-risk-navigation">
    <h2 class="inventory-risk-navigation__heading">Navigation</h2>
    <Card>
      <template #content>
        <div class="inventory-risk-navigation__links">
          <RouterLink
            v-if="navigation"
            :to="navigation.InventoryDashboardRoute"
            class="inventory-risk-navigation__link"
          >
            Inventory Dashboard
          </RouterLink>
          <RouterLink
            v-if="navigation"
            :to="navigation.InventoryReportRoute"
            class="inventory-risk-navigation__link"
          >
            Inventory Report
          </RouterLink>
        </div>
      </template>
    </Card>
  </section>
</template>

<style scoped>
.inventory-risk-navigation__heading {
  margin: 0 0 1rem;
  font-size: 1.25rem;
}

.inventory-risk-navigation__links {
  display: flex;
  flex-wrap: wrap;
  gap: 1.5rem;
}

.inventory-risk-navigation__link {
  color: var(--p-primary-color);
  text-decoration: none;
}

.inventory-risk-navigation__link:hover {
  text-decoration: underline;
}
</style>
