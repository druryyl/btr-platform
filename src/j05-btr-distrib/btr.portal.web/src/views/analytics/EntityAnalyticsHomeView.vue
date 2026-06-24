<script setup lang="ts">
import { onMounted } from 'vue'
import { RouterLink } from 'vue-router'
import Card from 'primevue/card'
import DashboardDetailLayout from '@/components/dashboard/DashboardDetailLayout.vue'
import { useEntityAnalyticsStore } from '@/stores/entityAnalyticsStore'

const store = useEntityAnalyticsStore()

onMounted(() => {
  void store.loadTypes()
})
</script>

<template>
  <DashboardDetailLayout
    title="Entity Analytics"
    subtitle="Performance profiles for customers, salesmen, items, and suppliers"
    :loading="store.loading"
    :error="store.error"
    @refresh="store.loadTypes()"
  >
    <p class="entity-analytics-home__intro">
      Select an entity type to explore performance profiles. Entity producers and snapshot data will
      be populated in upcoming milestones.
    </p>

    <div class="entity-analytics-home__grid">
      <Card
        v-for="entityType in store.types"
        :key="entityType.EntityType"
        class="entity-analytics-home__card"
      >
        <template #title>{{ entityType.DisplayName }}</template>
        <template #content>
          <p class="entity-analytics-home__status">
            {{ entityType.IsEnabled ? 'Enabled' : 'Not enabled yet' }}
          </p>
          <p class="entity-analytics-home__hint">
            <RouterLink :to="{ name: 'customer-compare' }">Compare customers</RouterLink>
            or open a profile via
            <code>/analytics/Customer/&lt;customerCode&gt;</code>
          </p>
        </template>
      </Card>
    </div>
  </DashboardDetailLayout>
</template>

<style scoped>
.entity-analytics-home__intro {
  margin: 0 0 1.5rem;
  color: var(--p-text-muted-color);
}

.entity-analytics-home__grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(16rem, 1fr));
  gap: 1rem;
}

.entity-analytics-home__status {
  margin: 0 0 0.5rem;
  font-weight: 600;
}

.entity-analytics-home__hint {
  margin: 0;
  color: var(--p-text-muted-color);
  font-size: 0.875rem;
}

.entity-analytics-home__hint code {
  font-size: 0.8125rem;
}
</style>
