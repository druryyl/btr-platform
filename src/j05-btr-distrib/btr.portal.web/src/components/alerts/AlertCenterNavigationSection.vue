<script setup lang="ts">
import { RouterLink } from 'vue-router'
import Card from 'primevue/card'
import PortalMenuLabel from '@/components/navigation/PortalMenuLabel.vue'
import type { DashboardAlertCenterNavigationLinks } from '@/models/dashboard'

defineProps<{
  navigation: DashboardAlertCenterNavigationLinks | null
}>()
</script>

<template>
  <section class="alert-center-navigation alert-center-navigation-section">
    <h2 class="alert-center-navigation__heading">Domain Dashboards</h2>
    <Card>
      <template #content>
        <div v-if="navigation?.DomainDashboards?.length" class="alert-center-navigation__links">
          <RouterLink
            v-for="link in navigation.DomainDashboards"
            :key="link.Code"
            :to="link.Route"
            class="alert-center-navigation__link"
          >
            <PortalMenuLabel :code="link.Code" :label="link.Label" />
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
