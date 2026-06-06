<script setup lang="ts">
import Button from 'primevue/button'
import Message from 'primevue/message'

defineProps<{
  title: string
  subtitle?: string
  loading?: boolean
  error?: string | null
}>()

const emit = defineEmits<{
  refresh: []
}>()
</script>

<template>
  <div class="dashboard-detail">
    <div class="dashboard-detail__header">
      <div>
        <h1>{{ title }}</h1>
        <p v-if="subtitle">{{ subtitle }}</p>
      </div>
      <Button
        label="Refresh"
        icon="pi pi-refresh"
        outlined
        :loading="loading"
        @click="emit('refresh')"
      />
    </div>

    <Message v-if="error" severity="error" :closable="false">
      {{ error }}
    </Message>

    <slot />
  </div>
</template>

<style scoped>
.dashboard-detail__header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
  margin-bottom: 1.5rem;
}

.dashboard-detail__header h1 {
  margin: 0;
  font-size: 1.75rem;
}

.dashboard-detail__header p {
  margin: 0.375rem 0 0;
  color: var(--p-text-muted-color);
}
</style>
