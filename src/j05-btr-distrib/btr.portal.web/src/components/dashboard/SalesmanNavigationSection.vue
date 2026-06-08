<script setup lang="ts">
import { RouterLink } from 'vue-router'
import Card from 'primevue/card'
import type { DashboardSalesmanNavigationLinks } from '@/models/dashboard'

defineProps<{
  navigation: DashboardSalesmanNavigationLinks | null
}>()
</script>

<template>
  <section class="salesman-navigation">
    <h2 class="salesman-navigation__heading">Navigation</h2>
    <Card>
      <template #content>
        <div class="salesman-navigation__links">
          <div class="salesman-navigation__group">
            <h3>Sales</h3>
            <RouterLink
              v-if="navigation"
              :to="navigation.SalesDashboardRoute"
              class="salesman-navigation__link"
            >
              Sales Dashboard
            </RouterLink>
            <RouterLink
              v-if="navigation"
              :to="navigation.SalesReportRoute"
              class="salesman-navigation__link"
            >
              Sales Report
            </RouterLink>
          </div>
          <div class="salesman-navigation__group">
            <h3>Piutang</h3>
            <RouterLink
              v-if="navigation"
              :to="navigation.PiutangDashboardRoute"
              class="salesman-navigation__link"
            >
              Piutang Dashboard
            </RouterLink>
            <RouterLink
              v-if="navigation"
              :to="navigation.PiutangReportRoute"
              class="salesman-navigation__link"
            >
              Piutang Report
            </RouterLink>
          </div>
        </div>
      </template>
    </Card>
  </section>
</template>

<style scoped>
.salesman-navigation__heading {
  margin: 0 0 1rem;
  font-size: 1.25rem;
}

.salesman-navigation__links {
  display: flex;
  flex-wrap: wrap;
  gap: 2rem;
}

.salesman-navigation__group h3 {
  margin: 0 0 0.5rem;
  font-size: 1rem;
}

.salesman-navigation__link {
  display: block;
  margin-bottom: 0.375rem;
  color: var(--p-primary-color);
  text-decoration: none;
}

.salesman-navigation__link:hover {
  text-decoration: underline;
}
</style>
