<script setup lang="ts">
import Tag from 'primevue/tag'
import type { FieldActivityMissedVisit } from '@/models/fieldActivity'

defineProps<{
  items: FieldActivityMissedVisit[]
}>()
</script>

<template>
  <section class="field-activity-missed-list">
    <h3 class="field-activity-missed-list__title">Missed Visits</h3>
    <p v-if="items.length === 0" class="field-activity-missed-list__empty">No missed visits.</p>
    <ul v-else class="field-activity-missed-list__items">
      <li v-for="item in items" :key="item.CustomerId" class="field-activity-missed-list__item">
        <div class="field-activity-missed-list__sequence">#{{ item.NoUrut }}</div>
        <div class="field-activity-missed-list__details">
          <div class="field-activity-missed-list__name">{{ item.CustomerName }}</div>
          <div class="field-activity-missed-list__code">{{ item.CustomerCode }}</div>
        </div>
        <Tag
          v-if="!item.HasCoordinates"
          value="No Coordinates"
          severity="warn"
          class="field-activity-missed-list__badge"
        />
      </li>
    </ul>
  </section>
</template>

<style scoped>
.field-activity-missed-list__title {
  margin: 0 0 0.75rem;
  font-size: 1rem;
}

.field-activity-missed-list__empty {
  margin: 0;
  color: var(--p-text-muted-color);
  font-size: 0.875rem;
}

.field-activity-missed-list__items {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 0.625rem;
}

.field-activity-missed-list__item {
  display: flex;
  align-items: center;
  gap: 0.625rem;
  padding: 0.5rem 0.625rem;
  border: 1px solid var(--p-content-border-color);
  border-radius: 0.5rem;
  background: var(--p-content-background);
}

.field-activity-missed-list__sequence {
  font-weight: 700;
  color: var(--p-red-500);
  min-width: 2rem;
}

.field-activity-missed-list__details {
  flex: 1;
  min-width: 0;
}

.field-activity-missed-list__name {
  font-weight: 600;
  font-size: 0.875rem;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.field-activity-missed-list__code {
  font-size: 0.75rem;
  color: var(--p-text-muted-color);
}

.field-activity-missed-list__badge {
  flex-shrink: 0;
}
</style>
