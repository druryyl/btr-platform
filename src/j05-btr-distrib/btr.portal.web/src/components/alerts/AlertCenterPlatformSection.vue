<script setup lang="ts">
import { useRouter } from 'vue-router'
import Message from 'primevue/message'
import Button from 'primevue/button'
import type { DashboardAlertCenterPlatformAlert } from '@/models/dashboard'

defineProps<{
  alerts: DashboardAlertCenterPlatformAlert[]
}>()

const router = useRouter()

function openDashboard(route: string): void {
  void router.push(route)
}
</script>

<template>
  <section v-if="alerts.length > 0" class="alert-center-platform">
    <h2 class="alert-center-platform__heading">Platform Alerts</h2>
    <Message
      v-for="alert in alerts"
      :key="alert.SignalKey"
      severity="error"
      :closable="false"
      class="alert-center-platform__message"
    >
      <div class="alert-center-platform__content">
        <div>
          <strong>{{ alert.SignalLabel }}</strong>
          <span v-if="alert.ValueText"> — {{ alert.ValueText }}</span>
        </div>
        <Button
          label="Executive Dashboard"
          icon="pi pi-arrow-right"
          text
          size="small"
          @click="openDashboard(alert.DashboardRoute)"
        />
      </div>
    </Message>
  </section>
</template>

<style scoped>
.alert-center-platform__heading {
  margin: 0 0 0.75rem;
  font-size: 1.125rem;
}

.alert-center-platform__message {
  margin-bottom: 0.5rem;
}

.alert-center-platform__content {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  flex-wrap: wrap;
}
</style>
