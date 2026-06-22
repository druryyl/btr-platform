<script setup lang="ts">
import { RouterLink } from 'vue-router'
import Card from 'primevue/card'
import PortalMenuLabel from '@/components/navigation/PortalMenuLabel.vue'
import type { DashboardCollectionNavigationLinks } from '@/models/dashboard'

defineProps<{
  navigation: DashboardCollectionNavigationLinks | null
}>()
</script>

<template>
  <section class="collection-navigation">
    <h2 class="collection-navigation__heading">Navigation</h2>
    <Card>
      <template #content>
        <div class="collection-navigation__links">
          <RouterLink
            v-if="navigation"
            :to="navigation.PiutangDashboardRoute"
            class="collection-navigation__link"
          >
            <PortalMenuLabel :route="navigation.PiutangDashboardRoute" />
          </RouterLink>
          <RouterLink
            v-if="navigation"
            :to="navigation.CustomerDashboardRoute"
            class="collection-navigation__link"
          >
            <PortalMenuLabel :route="navigation.CustomerDashboardRoute" />
          </RouterLink>
          <RouterLink
            v-if="navigation"
            :to="navigation.SalesmanDashboardRoute"
            class="collection-navigation__link"
          >
            <PortalMenuLabel :route="navigation.SalesmanDashboardRoute" />
          </RouterLink>
          <RouterLink
            v-if="navigation"
            :to="navigation.PiutangReportRoute"
            class="collection-navigation__link"
          >
            <PortalMenuLabel :route="navigation.PiutangReportRoute" />
          </RouterLink>
        </div>
      </template>
    </Card>
  </section>
</template>

<style scoped>
.collection-navigation__heading {
  margin: 0 0 1rem;
  font-size: 1.25rem;
}

.collection-navigation__links {
  display: flex;
  flex-wrap: wrap;
  gap: 1rem 2rem;
}

.collection-navigation__link {
  color: var(--p-primary-color);
  text-decoration: none;
}

.collection-navigation__link:hover {
  text-decoration: underline;
}
</style>
