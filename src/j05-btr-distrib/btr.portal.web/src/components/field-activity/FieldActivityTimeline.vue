<script setup lang="ts">
import Tag from 'primevue/tag'
import type { FieldActivityActualStop } from '@/models/fieldActivity'

const props = defineProps<{
  stops: FieldActivityActualStop[]
  selectedIndex: number
}>()

const emit = defineEmits<{
  select: [index: number]
}>()

function gpsSeverity(level: string): 'success' | 'warn' | 'danger' | 'secondary' {
  switch (level) {
    case 'Valid':
      return 'success'
    case 'Warning':
      return 'warn'
    case 'Suspicious':
      return 'danger'
    default:
      return 'secondary'
  }
}

function isSelected(index: number): boolean {
  return props.selectedIndex === index
}
</script>

<template>
  <section class="field-activity-timeline">
    <h3 class="field-activity-timeline__title">Visit Timeline</h3>
    <p v-if="stops.length === 0" class="field-activity-timeline__empty">No check-ins recorded.</p>
    <ul v-else class="field-activity-timeline__items">
      <li
        v-for="(stop, index) in stops"
        :key="`${stop.CustomerId}-${stop.CheckInTime}`"
        class="field-activity-timeline__item"
        :class="{ 'field-activity-timeline__item--selected': isSelected(index) }"
        @click="emit('select', index)"
      >
        <div class="field-activity-timeline__sequence">{{ stop.Sequence }}</div>
        <div class="field-activity-timeline__details">
          <div class="field-activity-timeline__header">
            <span class="field-activity-timeline__time">{{ stop.CheckInTime }}</span>
            <Tag
              v-if="stop.IsEffectiveCall"
              value="Effective"
              severity="success"
              class="field-activity-timeline__tag"
            />
            <Tag
              v-if="stop.VisitStatus === 'Unplanned'"
              value="Unplanned"
              severity="info"
              class="field-activity-timeline__tag"
            />
          </div>
          <div class="field-activity-timeline__name">{{ stop.CustomerName }}</div>
          <div class="field-activity-timeline__meta">
            <Tag
              :value="stop.GpsValidation"
              :severity="gpsSeverity(stop.GpsValidation)"
              class="field-activity-timeline__tag"
            />
            <span v-if="stop.DistanceMeters != null">
              {{ stop.DistanceMeters.toFixed(0) }} m
            </span>
          </div>
        </div>
      </li>
    </ul>
  </section>
</template>

<style scoped>
.field-activity-timeline__title {
  margin: 0 0 0.75rem;
  font-size: 1rem;
}

.field-activity-timeline__empty {
  margin: 0;
  color: var(--p-text-muted-color);
  font-size: 0.875rem;
}

.field-activity-timeline__items {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  max-height: 18rem;
  overflow-y: auto;
}

.field-activity-timeline__item {
  display: flex;
  gap: 0.625rem;
  padding: 0.5rem 0.625rem;
  border: 1px solid var(--p-content-border-color);
  border-radius: 0.5rem;
  cursor: pointer;
  background: var(--p-content-background);
}

.field-activity-timeline__item--selected {
  border-color: var(--p-primary-color);
  box-shadow: 0 0 0 1px var(--p-primary-color);
}

.field-activity-timeline__sequence {
  font-weight: 700;
  min-width: 1.5rem;
  color: var(--p-primary-color);
}

.field-activity-timeline__details {
  flex: 1;
  min-width: 0;
}

.field-activity-timeline__header {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 0.375rem;
}

.field-activity-timeline__time {
  font-weight: 600;
  font-size: 0.875rem;
}

.field-activity-timeline__name {
  font-size: 0.875rem;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.field-activity-timeline__meta {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-top: 0.25rem;
  font-size: 0.75rem;
  color: var(--p-text-muted-color);
}

.field-activity-timeline__tag {
  font-size: 0.6875rem;
}
</style>
