<script setup lang="ts">
import type { InvestigationStep } from '@/models/investigation'
import { resolveInvestigationStepLabel } from '@/navigation/portalMenuHelpers'

defineProps<{
  steps: InvestigationStep[]
}>()
</script>

<template>
  <ol class="investigation-steps">
    <li
      v-for="step in steps"
      :key="step.Order"
      class="investigation-steps__item"
    >
      <span class="investigation-steps__order">{{ step.Order }}.</span>
      <span>{{
        resolveInvestigationStepLabel(step.Label, step.DashboardRoute, step.ReportRoute)
      }}</span>
    </li>
  </ol>
</template>

<style scoped>
.investigation-steps {
  margin: 0.5rem 0 0;
  padding-left: 1.25rem;
  font-size: 0.9rem;
  color: var(--p-text-muted-color);
}

.investigation-steps__item {
  margin-bottom: 0.25rem;
}

.investigation-steps__order {
  font-weight: 600;
  margin-right: 0.25rem;
}
</style>
