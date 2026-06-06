<script setup lang="ts">
import Card from 'primevue/card'
import ProgressSpinner from 'primevue/progressspinner'

defineProps<{
  title: string
  icon?: string
  loading?: boolean
}>()
</script>

<template>
  <Card class="kpi-card">
    <template #title>
      <span class="kpi-card__title">
        <i v-if="icon" :class="icon" class="kpi-card__icon" />
        {{ title }}
      </span>
    </template>
    <template #content>
      <div v-if="loading" class="kpi-card__loading">
        <ProgressSpinner style="width: 2rem; height: 2rem" stroke-width="4" />
      </div>
      <div v-else class="kpi-card__content">
        <slot />
      </div>
    </template>
  </Card>
</template>

<style scoped>
.kpi-card {
  height: 100%;
}

.kpi-card__title {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 1.1rem;
}

.kpi-card__icon {
  color: var(--p-primary-color);
}

.kpi-card__loading {
  display: flex;
  justify-content: center;
  padding: 1.5rem 0;
}

.kpi-card__content {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}
</style>
