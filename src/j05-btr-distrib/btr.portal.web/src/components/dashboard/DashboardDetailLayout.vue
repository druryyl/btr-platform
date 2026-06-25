<script setup lang="ts">
import { computed } from 'vue'
import Button from 'primevue/button'
import Message from 'primevue/message'
import { formatDateTime } from '@/services/formatters'
import { shouldShowInfrastructureError } from '@/services/platformDiagnostics'
import { usePresentationStore } from '@/stores/presentationStore'

const props = defineProps<{
  title: string
  subtitle?: string
  loading?: boolean
  error?: string | null
  generatedAt?: string | null
}>()

const emit = defineEmits<{
  refresh: []
}>()

const presentation = usePresentationStore()

const showGeneratedAt = computed(
  () => !presentation.hidePlatformDiagnostics && props.generatedAt,
)

const showError = computed(() =>
  shouldShowInfrastructureError(props.error, presentation.hidePlatformDiagnostics),
)

const visibleError = computed(() => (showError.value ? props.error : null))
</script>

<template>
  <div class="dashboard-detail">
    <div class="dashboard-detail__header">
      <div>
        <h1>{{ title }}</h1>
        <p v-if="subtitle">{{ subtitle }}</p>
        <p v-if="showGeneratedAt" class="dashboard-detail__meta">
          Data as of {{ formatDateTime(generatedAt!) }}
        </p>
      </div>
      <div class="dashboard-detail__header-actions">
        <slot name="header-actions" />
        <Button
          label="Refresh"
          icon="pi pi-refresh"
          outlined
          :loading="loading"
          @click="emit('refresh')"
        />
      </div>
    </div>

    <Message v-if="visibleError" severity="error" :closable="false">
      {{ visibleError }}
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

.dashboard-detail__header-actions {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  flex-shrink: 0;
}

.dashboard-detail__header h1 {
  margin: 0;
  font-size: 1.75rem;
}

.dashboard-detail__header p {
  margin: 0.375rem 0 0;
  color: var(--p-text-muted-color);
}

.dashboard-detail__meta {
  margin-top: 0.5rem !important;
  font-size: 0.875rem;
}
</style>
