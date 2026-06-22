<script setup lang="ts">
import { RouterLink } from 'vue-router'
import Card from 'primevue/card'
import PortalMenuLabel from '@/components/navigation/PortalMenuLabel.vue'
import type { DashboardCustomerNavigationLinks } from '@/models/dashboard'

defineProps<{
  navigation: DashboardCustomerNavigationLinks | null
}>()
</script>

<template>
  <section class="customer-navigation">
    <h2 class="customer-navigation__heading">Navigation</h2>
    <Card>
      <template #content>
        <div class="customer-navigation__links">
          <div class="customer-navigation__group">
            <h3>Sales</h3>
            <RouterLink
              v-if="navigation"
              :to="navigation.SalesDashboardRoute"
              class="customer-navigation__link"
            >
              <PortalMenuLabel :route="navigation.SalesDashboardRoute" />
            </RouterLink>
            <RouterLink
              v-if="navigation"
              :to="navigation.SalesReportRoute"
              class="customer-navigation__link"
            >
              <PortalMenuLabel :route="navigation.SalesReportRoute" />
            </RouterLink>
          </div>
          <div class="customer-navigation__group">
            <h3>Piutang</h3>
            <RouterLink
              v-if="navigation"
              :to="navigation.PiutangDashboardRoute"
              class="customer-navigation__link"
            >
              <PortalMenuLabel :route="navigation.PiutangDashboardRoute" />
            </RouterLink>
            <RouterLink
              v-if="navigation"
              :to="navigation.PiutangReportRoute"
              class="customer-navigation__link"
            >
              <PortalMenuLabel :route="navigation.PiutangReportRoute" />
            </RouterLink>
          </div>
        </div>
      </template>
    </Card>
  </section>
</template>

<style scoped>
.customer-navigation__heading {
  margin: 0 0 1rem;
  font-size: 1.25rem;
}

.customer-navigation__links {
  display: flex;
  flex-wrap: wrap;
  gap: 2rem;
}

.customer-navigation__group h3 {
  margin: 0 0 0.5rem;
  font-size: 1rem;
}

.customer-navigation__link {
  display: block;
  margin-bottom: 0.375rem;
  color: var(--p-primary-color);
  text-decoration: none;
}

.customer-navigation__link:hover {
  text-decoration: underline;
}
</style>
