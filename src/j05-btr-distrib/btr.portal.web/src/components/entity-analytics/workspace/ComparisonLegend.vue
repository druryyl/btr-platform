<script setup lang="ts">

import Button from 'primevue/button'

import type { WorkspaceSelectedEntity } from '@/models/entityAnalytics'

import {

  buildEntityColorMap,

  WORKSPACE_HALO_PADDING,

  WORKSPACE_LEGEND_SWATCH_DIAMETER,

  WORKSPACE_SELECTED_RADIUS,

  WORKSPACE_SELECTED_STROKE_WIDTH,

} from '@/composables/useComparisonColors'



const props = defineProps<{

  entities: WorkspaceSelectedEntity[]

}>()



const emit = defineEmits<{

  remove: [entityId: string]

}>()



const colors = () => buildEntityColorMap(props.entities.map((e) => e.EntityId))



function swatchStyle(entityId: string) {

  const palette = colors().get(entityId)

  if (!palette) return {}



  const coreDiameter = WORKSPACE_SELECTED_RADIUS * 2

  const haloSpread = WORKSPACE_HALO_PADDING



  return {

    width: `${WORKSPACE_LEGEND_SWATCH_DIAMETER}px`,

    height: `${WORKSPACE_LEGEND_SWATCH_DIAMETER}px`,

    background: palette.fill,

    border: `${WORKSPACE_SELECTED_STROKE_WIDTH}px solid ${palette.border}`,

    boxShadow: `0 0 0 ${haloSpread}px #ffffff`,

    borderRadius: '50%',

    flexShrink: '0',

    boxSizing: 'border-box' as const,

    // Core fill visible inside halo ring

    backgroundClip: 'padding-box' as const,

    minWidth: `${coreDiameter + haloSpread * 2}px`,

    minHeight: `${coreDiameter + haloSpread * 2}px`,

  }

}

</script>



<template>

  <div v-if="entities.length" class="iw-comparison-legend">

    <div

      v-for="entity in entities"

      :key="entity.EntityId"

      class="iw-comparison-chip"

    >

      <span

        class="iw-comparison-chip__swatch"

        :style="swatchStyle(entity.EntityId)"

      />

      <span>{{ entity.DisplayName }}</span>

      <Button

        icon="pi pi-times"

        text

        rounded

        size="small"

        severity="secondary"

        aria-label="Remove from comparison"

        @click="emit('remove', entity.EntityId)"

      />

    </div>

  </div>

</template>


