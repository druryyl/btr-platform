<script setup lang="ts">
import Dialog from 'primevue/dialog'
import Button from 'primevue/button'
import type { PopulationMapPoint } from '@/models/entityAnalytics'

defineProps<{
  visible: boolean
  point: PopulationMapPoint | null
}>()

const emit = defineEmits<{
  confirm: []
  dismiss: []
}>()
</script>

<template>
  <Dialog
    :visible="visible"
    modal
    header="Comparison limit reached"
    :style="{ width: '24rem' }"
    @update:visible="(v: boolean) => !v && emit('dismiss')"
  >
    <p>
      Maximum 4 entities for comparison. Remove one to add
      <strong v-if="point">{{ point.DisplayName }}</strong>.
    </p>
    <template #footer>
      <Button label="OK" @click="emit('dismiss')" />
    </template>
  </Dialog>
</template>
