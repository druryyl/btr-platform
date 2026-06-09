<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import Message from 'primevue/message'
import Button from 'primevue/button'
import InvestigationStepsList from '@/components/reports/InvestigationStepsList.vue'
import type { InvestigationBreadcrumbContext } from '@/models/investigation'

const props = defineProps<{
  context: InvestigationBreadcrumbContext
}>()

const router = useRouter()

const visible = computed(
  () =>
    Boolean(props.context.signalLabel)
    || Boolean(props.context.source)
    || Boolean(props.context.entityName),
)

const summary = computed(() => {
  const parts: string[] = []

  if (props.context.entityName) {
    parts.push(props.context.entityName)
  }

  if (props.context.signalLabel) {
    parts.push(`Signal: ${props.context.signalLabel}`)
  }

  if (props.context.source) {
    parts.push(`Source: ${props.context.source}`)
  }

  return parts.join(' · ')
})

function openDashboard(): void {
  if (props.context.dashboardRoute) {
    void router.push(props.context.dashboardRoute)
  }
}
</script>

<template>
  <div v-if="visible" class="investigation-breadcrumb">
    <Message severity="info" :closable="false" class="investigation-breadcrumb__message">
      <div class="investigation-breadcrumb__content">
        <p class="investigation-breadcrumb__summary">
          <strong>Investigating:</strong> {{ summary }}
        </p>
        <p v-if="context.desktopNextStep" class="investigation-breadcrumb__desktop">
          {{ context.desktopNextStep }}
        </p>
        <InvestigationStepsList
          v-if="context.investigationSteps && context.investigationSteps.length > 0"
          :steps="context.investigationSteps"
        />
        <Button
          v-if="context.dashboardRoute"
          label="View dashboard context →"
          link
          class="investigation-breadcrumb__dashboard-link"
          @click="openDashboard"
        />
      </div>
    </Message>
  </div>
</template>

<style scoped>
.investigation-breadcrumb {
  margin-bottom: 1rem;
}

.investigation-breadcrumb__message {
  width: 100%;
}

.investigation-breadcrumb__summary {
  margin: 0;
}

.investigation-breadcrumb__desktop {
  margin: 0.5rem 0 0;
  color: var(--p-text-muted-color);
  font-size: 0.9rem;
}

.investigation-breadcrumb__dashboard-link {
  margin-top: 0.5rem;
  padding-left: 0;
}
</style>
