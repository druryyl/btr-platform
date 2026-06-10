<script setup lang="ts">
import Button from 'primevue/button'
import Slider from 'primevue/slider'

defineProps<{
  isPlaying: boolean
  speed: number
  minSpeed: number
  maxSpeed: number
  disabled?: boolean
}>()

const emit = defineEmits<{
  play: []
  pause: []
  reset: []
  'update:speed': [value: number]
}>()
</script>

<template>
  <section class="field-activity-replay">
    <div class="field-activity-replay__controls">
      <Button
        v-if="!isPlaying"
        icon="pi pi-play"
        label="Play"
        size="small"
        :disabled="disabled"
        @click="emit('play')"
      />
      <Button
        v-else
        icon="pi pi-pause"
        label="Pause"
        size="small"
        severity="secondary"
        @click="emit('pause')"
      />
      <Button
        icon="pi pi-refresh"
        label="Reset"
        size="small"
        outlined
        :disabled="disabled"
        @click="emit('reset')"
      />
    </div>

    <div class="field-activity-replay__speed">
      <label class="field-activity-replay__speed-label">Speed {{ speed.toFixed(1) }}×</label>
      <Slider
        :model-value="speed"
        :min="minSpeed"
        :max="maxSpeed"
        :step="0.1"
        :disabled="disabled"
        @update:model-value="emit('update:speed', $event as number)"
      />
    </div>

    <p class="field-activity-replay__note" title="Replay shows visit-to-visit segments, not driven road paths.">
      Replay shows visit-to-visit segments, not driven road paths.
    </p>
  </section>
</template>

<style scoped>
.field-activity-replay {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.field-activity-replay__controls {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.field-activity-replay__speed-label {
  display: block;
  margin-bottom: 0.375rem;
  font-size: 0.8125rem;
  color: var(--p-text-muted-color);
}

.field-activity-replay__note {
  margin: 0;
  font-size: 0.75rem;
  color: var(--p-text-muted-color);
}
</style>
