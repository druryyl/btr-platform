<script setup lang="ts">
import { ref, watch } from 'vue'
import InputText from 'primevue/inputtext'
import type { PopulationMapPoint } from '@/models/entityAnalytics'

const props = defineProps<{
  points: PopulationMapPoint[]
}>()

const emit = defineEmits<{
  highlight: [entityIds: string[]]
  select: [point: PopulationMapPoint]
}>()

const query = ref('')

watch(query, () => {
  const q = query.value.trim().toLowerCase()
  if (!q) {
    emit('highlight', [])
    return
  }
  const matches = props.points
    .filter((p) =>
      p.DisplayName.toLowerCase().includes(q)
      || p.EntityCode.toLowerCase().includes(q))
    .map((p) => p.EntityId)
  emit('highlight', matches)
})

function onEnter() {
  const q = query.value.trim().toLowerCase()
  if (!q) return
  const match = props.points.find(
    (p) => p.DisplayName.toLowerCase().includes(q) || p.EntityCode.toLowerCase().includes(q),
  )
  if (match) emit('select', match)
}
</script>

<template>
  <span class="p-input-icon-left iw-search">
    <i class="pi pi-search" />
    <InputText
      v-model="query"
      placeholder="Search population..."
      class="iw-search__input"
      @keydown.enter.prevent="onEnter"
    />
  </span>
</template>

<style scoped>
.iw-search__input {
  min-width: 14rem;
}
</style>
