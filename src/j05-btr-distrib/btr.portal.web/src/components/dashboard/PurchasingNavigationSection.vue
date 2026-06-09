<script setup lang="ts">
import { RouterLink } from 'vue-router'
import Card from 'primevue/card'
import type { DashboardPurchasingNavigationLinks } from '@/models/dashboard'

defineProps<{
  navigation: DashboardPurchasingNavigationLinks | null
}>()
</script>

<template>
  <section class="purchasing-navigation">
    <h2 class="purchasing-navigation__heading">Navigation</h2>
    <Card>
      <template #content>
        <div class="purchasing-navigation__links">
          <RouterLink
            v-if="navigation"
            :to="navigation.PurchasingReportRoute"
            class="purchasing-navigation__link"
          >
            Purchasing Report
          </RouterLink>
          <RouterLink
            v-if="navigation"
            :to="navigation.InventoryDashboardRoute"
            class="purchasing-navigation__link"
          >
            Inventory Dashboard
          </RouterLink>
          <RouterLink
            v-if="navigation"
            :to="navigation.InventoryRiskDashboardRoute"
            class="purchasing-navigation__link"
          >
            Inventory Risk Dashboard
          </RouterLink>
        </div>
      </template>
    </Card>
  </section>
</template>

<style scoped>
.purchasing-navigation__heading {
  margin: 0 0 1rem;
  font-size: 1.25rem;
}

.purchasing-navigation__links {
  display: flex;
  flex-wrap: wrap;
  gap: 1rem 2rem;
}

.purchasing-navigation__link {
  color: var(--p-primary-color);
  text-decoration: none;
}

.purchasing-navigation__link:hover {
  text-decoration: underline;
}
</style>
