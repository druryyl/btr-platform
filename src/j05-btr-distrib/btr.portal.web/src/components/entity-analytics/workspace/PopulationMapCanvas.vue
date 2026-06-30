<script setup lang="ts">

import { computed, onMounted, onUnmounted, ref, watch } from 'vue'

import type { PopulationMapPoint, PopulationMapResponse } from '@/models/entityAnalytics'

import {

  buildEntityColorMap,

  WORKSPACE_ATTENTION_POINT,

  WORKSPACE_ATTENTION_POINT_OPACITY,

  WORKSPACE_ATTENTION_RADIUS,

  WORKSPACE_CRITICAL_POINT,

  WORKSPACE_CRITICAL_POINT_OPACITY,

  WORKSPACE_CRITICAL_RADIUS,

  WORKSPACE_HALO_PADDING,

  WORKSPACE_HOVER_ENLARGE,

  WORKSPACE_HOVER_POINT,

  WORKSPACE_HOVER_POINT_OPACITY,

  WORKSPACE_HOVER_RADIUS,

  WORKSPACE_MAP_AXIS_COLOR,

  WORKSPACE_MAP_PLOT_BG,

  WORKSPACE_MAP_PLOT_BORDER,

  WORKSPACE_MAP_QUADRANT_LABEL,

  WORKSPACE_MAP_QUADRANT_LINE,

  WORKSPACE_NEUTRAL_POINT,

  WORKSPACE_NEUTRAL_POINT_OPACITY,

  WORKSPACE_NORMAL_RADIUS,

  WORKSPACE_SELECTED_RADIUS,

  WORKSPACE_SELECTED_STROKE_WIDTH,

} from '@/composables/useComparisonColors'

import {

  buildTransform,

  computeBounds,

  findNearestPoint,

  measureLabel,

  plotPoints,

  resolveLabelPlacements,

  resolvePopulationPointTier,

  type LabelCandidate,

  type MapTransform,

  type PlottedPoint,

  type PopulationPointTier,

  type ResolvedLabel,

} from '@/services/populationMapLayout'



const props = defineProps<{

  population: PopulationMapResponse | null

  selectedEntityIds: string[]

  searchHighlightIds?: string[]

  investigationMode?: boolean

  loading?: boolean

}>()



const emit = defineEmits<{

  select: [point: PopulationMapPoint]

  hover: [point: PopulationMapPoint | null]

}>()



const canvasRef = ref<HTMLCanvasElement | null>(null)

const containerRef = ref<HTMLDivElement | null>(null)

const hovered = ref<PopulationMapPoint | null>(null)

const tooltipPos = ref({ x: 0, y: 0 })



const zoom = ref(1)

const panX = ref(0)

const panY = ref(0)

const isPanning = ref(false)

const panStart = ref({ x: 0, y: 0, panX: 0, panY: 0 })



const colorMap = computed(() => buildEntityColorMap(props.selectedEntityIds))

const selectedSet = computed(() => new Set(props.selectedEntityIds))



const visiblePoints = computed(() =>

  (props.population?.Points ?? []).filter((p) => p.AxisX != null && p.AxisY != null),

)



const searchSet = computed(() => new Set(props.searchHighlightIds ?? []))



let plottedCache: PlottedPoint[] = []

let resizeObserver: ResizeObserver | null = null



interface PointDrawFlags {

  isSelected: boolean

  isHovered: boolean

  isSearchMatch: boolean

  hasSearch: boolean

  dimPopulation: boolean

}



const TIER_STYLES: Record<PopulationPointTier, { color: string; opacity: number; radius: number }> = {

  normal: {

    color: WORKSPACE_NEUTRAL_POINT,

    opacity: WORKSPACE_NEUTRAL_POINT_OPACITY,

    radius: WORKSPACE_NORMAL_RADIUS,

  },

  attention: {

    color: WORKSPACE_ATTENTION_POINT,

    opacity: WORKSPACE_ATTENTION_POINT_OPACITY,

    radius: WORKSPACE_ATTENTION_RADIUS,

  },

  critical: {

    color: WORKSPACE_CRITICAL_POINT,

    opacity: WORKSPACE_CRITICAL_POINT_OPACITY,

    radius: WORKSPACE_CRITICAL_RADIUS,

  },

}



function resizeCanvas() {

  const canvas = canvasRef.value

  const container = containerRef.value

  if (!canvas || !container) return



  const rect = container.getBoundingClientRect()

  const dpr = window.devicePixelRatio || 1

  canvas.width = Math.max(rect.width * dpr, 1)

  canvas.height = Math.max(rect.height * dpr, 1)

  canvas.style.width = `${rect.width}px`

  canvas.style.height = `${rect.height}px`

  draw()

}



function getPointFlags(point: PopulationMapPoint): PointDrawFlags {

  const isSelected = selectedSet.value.has(point.EntityId)

  const isHovered = hovered.value?.EntityId === point.EntityId

  const hasSearch = searchSet.value.size > 0

  const isSearchMatch = hasSearch && searchSet.value.has(point.EntityId)

  const dimPopulation = Boolean(props.investigationMode && props.selectedEntityIds.length > 0)



  return { isSelected, isHovered, isSearchMatch, hasSearch, dimPopulation }

}



function applyPointAlpha(

  ctx: CanvasRenderingContext2D,

  point: PopulationMapPoint,

  flags: PointDrawFlags,

  baseOpacity: number,

) {

  let alpha = baseOpacity



  if (flags.hasSearch && !flags.isSearchMatch && !flags.isSelected) {

    alpha = Math.min(alpha, 0.2)

  } else if (flags.dimPopulation && !flags.isSelected) {

    alpha = Math.min(alpha, 0.4)

  }



  if (!point.MatchesFilter && !flags.isSelected) {

    alpha = Math.min(alpha, 0.25)

  }



  ctx.globalAlpha = alpha

}



function drawFilledCircle(

  ctx: CanvasRenderingContext2D,

  x: number,

  y: number,

  radius: number,

  fill: string,

) {

  ctx.beginPath()

  ctx.arc(x, y, radius, 0, Math.PI * 2)

  ctx.fillStyle = fill

  ctx.fill()

}



function drawTieredPoint(

  ctx: CanvasRenderingContext2D,

  item: PlottedPoint,

  tier: PopulationPointTier,

  flags: PointDrawFlags,

) {

  const style = TIER_STYLES[tier]

  applyPointAlpha(ctx, item.point, flags, style.opacity)

  drawFilledCircle(ctx, item.screenX, item.screenY, style.radius, style.color)

  ctx.globalAlpha = 1

}



function drawSelectedPoint(

  ctx: CanvasRenderingContext2D,

  x: number,

  y: number,

  radius: number,

  fill: string,

  border: string,

) {

  drawFilledCircle(ctx, x, y, radius + WORKSPACE_HALO_PADDING, '#ffffff')

  drawFilledCircle(ctx, x, y, radius, fill)

  ctx.beginPath()

  ctx.arc(x, y, radius, 0, Math.PI * 2)

  ctx.strokeStyle = border

  ctx.lineWidth = WORKSPACE_SELECTED_STROKE_WIDTH

  ctx.stroke()

}



function drawBackgroundAndAxes(

  ctx: CanvasRenderingContext2D,

  transform: MapTransform,

  height: number,

) {

  const { offsetX, offsetY, plotWidth, plotHeight } = transform



  ctx.fillStyle = WORKSPACE_MAP_PLOT_BG

  ctx.fillRect(offsetX, offsetY, plotWidth, plotHeight)



  ctx.strokeStyle = WORKSPACE_MAP_PLOT_BORDER

  ctx.lineWidth = 1

  ctx.setLineDash([])

  ctx.strokeRect(offsetX, offsetY, plotWidth, plotHeight)



  ctx.fillStyle = WORKSPACE_MAP_AXIS_COLOR

  ctx.font = '600 12px system-ui, sans-serif'



  if (props.population?.AxisXLabel) {

    ctx.fillText(props.population.AxisXLabel, offsetX, height - 12)

  }

  if (props.population?.AxisYLabel) {

    ctx.save()

    ctx.translate(14, offsetY + plotHeight / 2)

    ctx.rotate(-Math.PI / 2)

    ctx.fillText(props.population.AxisYLabel, 0, 0)

    ctx.restore()

  }

}



function drawQuadrantGuides(ctx: CanvasRenderingContext2D, transform: MapTransform) {

  const midX = transform.offsetX + transform.plotWidth / 2

  const midY = transform.offsetY + transform.plotHeight / 2



  ctx.strokeStyle = WORKSPACE_MAP_QUADRANT_LINE

  ctx.lineWidth = 1.5

  ctx.setLineDash([6, 4])



  ctx.beginPath()

  ctx.moveTo(midX, transform.offsetY)

  ctx.lineTo(midX, transform.offsetY + transform.plotHeight)

  ctx.stroke()



  ctx.beginPath()

  ctx.moveTo(transform.offsetX, midY)

  ctx.lineTo(transform.offsetX + transform.plotWidth, midY)

  ctx.stroke()



  ctx.setLineDash([])



  ctx.fillStyle = WORKSPACE_MAP_QUADRANT_LABEL

  ctx.font = '500 11px system-ui, sans-serif'

  const labels = [

    { text: 'High / High', x: transform.offsetX + transform.plotWidth * 0.72, y: transform.offsetY + 14 },

    { text: 'Low / High', x: transform.offsetX + 8, y: transform.offsetY + 14 },

    { text: 'High / Low', x: transform.offsetX + transform.plotWidth * 0.72, y: transform.offsetY + transform.plotHeight - 6 },

    { text: 'Low / Low', x: transform.offsetX + 8, y: transform.offsetY + transform.plotHeight - 6 },

  ]

  for (const label of labels) {

    ctx.fillText(label.text, label.x, label.y)

  }

}



function drawTierPass(

  ctx: CanvasRenderingContext2D,

  tier: PopulationPointTier,

) {

  for (const item of plottedCache) {

    const flags = getPointFlags(item.point)

    if (flags.isSelected) continue

    if (resolvePopulationPointTier(item.point) !== tier) continue

    drawTieredPoint(ctx, item, tier, flags)

  }

}



function drawSelectedEntities(ctx: CanvasRenderingContext2D) {

  for (const id of props.selectedEntityIds) {

    const plot = plottedCache.find((p) => p.point.EntityId === id)

    if (!plot) continue



    const palette = colorMap.value.get(id)

    const fill = palette?.fill ?? WORKSPACE_HOVER_POINT

    const border = palette?.border ?? '#334155'



    ctx.globalAlpha = 1

    drawSelectedPoint(ctx, plot.screenX, plot.screenY, WORKSPACE_SELECTED_RADIUS, fill, border)

  }

}



function collectLabelCandidates(): LabelCandidate[] {

  const candidates: LabelCandidate[] = []

  const seen = new Set<string>()



  for (const id of props.selectedEntityIds) {

    const plot = plottedCache.find((p) => p.point.EntityId === id)

    if (!plot) continue

    candidates.push({

      entityId: id,

      text: plot.point.DisplayName,

      anchorX: plot.screenX,

      anchorY: plot.screenY,

      priority: 100,

    })

    seen.add(id)

  }



  if (hovered.value && !seen.has(hovered.value.EntityId)) {

    const plot = plottedCache.find((p) => p.point.EntityId === hovered.value?.EntityId)

    if (plot) {

      candidates.push({

        entityId: hovered.value.EntityId,

        text: hovered.value.DisplayName,

        anchorX: plot.screenX,

        anchorY: plot.screenY,

        priority: 50,

      })

    }

  }



  return candidates

}



function drawResolvedLabel(ctx: CanvasRenderingContext2D, label: ResolvedLabel) {

  const metrics = measureLabel(ctx, label.text)



  if (label.leader) {

    ctx.strokeStyle = '#cbd5e1'

    ctx.lineWidth = 1

    ctx.beginPath()

    ctx.moveTo(label.leader.fromX, label.leader.fromY)

    ctx.lineTo(label.leader.toX, label.leader.toY)

    ctx.stroke()

  }



  ctx.fillStyle = 'rgba(255, 255, 255, 0.92)'

  ctx.strokeStyle = '#cbd5e1'

  ctx.lineWidth = 1

  ctx.fillRect(label.boxX, label.boxY, label.boxW, label.boxH)

  ctx.strokeRect(label.boxX, label.boxY, label.boxW, label.boxH)



  ctx.fillStyle = '#1e293b'

  ctx.font = metrics.font

  ctx.fillText(label.text, label.boxX + metrics.padX, label.boxY + label.boxH - metrics.padY - 2)

}



function drawLabels(ctx: CanvasRenderingContext2D) {

  const candidates = collectLabelCandidates()

  if (!candidates.length) return



  const resolved = resolveLabelPlacements(candidates, ctx)

  for (const label of resolved) {

    drawResolvedLabel(ctx, label)

  }

}



function drawHoverOverlay(ctx: CanvasRenderingContext2D) {

  if (!hovered.value) return



  const plot = plottedCache.find((p) => p.point.EntityId === hovered.value?.EntityId)

  if (!plot) return



  const flags = getPointFlags(plot.point)

  ctx.globalAlpha = 1



  if (flags.isSelected) {

    const palette = colorMap.value.get(plot.point.EntityId)

    const fill = palette?.fill ?? WORKSPACE_HOVER_POINT

    const border = palette?.border ?? '#334155'

    drawSelectedPoint(

      ctx,

      plot.screenX,

      plot.screenY,

      WORKSPACE_SELECTED_RADIUS + WORKSPACE_HOVER_ENLARGE,

      fill,

      border,

    )

    return

  }



  const tier = resolvePopulationPointTier(plot.point)

  const style = TIER_STYLES[tier]

  const radius = tier === 'normal' ? WORKSPACE_HOVER_RADIUS : style.radius + WORKSPACE_HOVER_ENLARGE



  ctx.globalAlpha = WORKSPACE_HOVER_POINT_OPACITY

  drawFilledCircle(ctx, plot.screenX, plot.screenY, radius, WORKSPACE_HOVER_POINT)

  ctx.globalAlpha = 1

}



function draw() {

  const canvas = canvasRef.value

  if (!canvas) return

  const ctx = canvas.getContext('2d')

  if (!ctx) return



  const dpr = window.devicePixelRatio || 1

  ctx.setTransform(dpr, 0, 0, dpr, 0, 0)



  const width = canvas.width / dpr

  const height = canvas.height / dpr

  ctx.clearRect(0, 0, width, height)



  const bounds = computeBounds(visiblePoints.value)

  const transform = buildTransform(width, height, bounds, zoom.value, panX.value, panY.value)

  plottedCache = plotPoints(visiblePoints.value, transform)



  // 1. Grid and background

  drawBackgroundAndAxes(ctx, transform, height)

  // 2. Quadrant guides

  drawQuadrantGuides(ctx, transform)

  // 3. Population (normal)

  drawTierPass(ctx, 'normal')

  // 4. Attention entities

  drawTierPass(ctx, 'attention')

  // 5. Critical entities

  drawTierPass(ctx, 'critical')

  // 6. Selected entities

  drawSelectedEntities(ctx)

  // 7. Labels

  drawLabels(ctx)

  // 8. Hover overlay

  drawHoverOverlay(ctx)

}



function onPointerMove(event: MouseEvent) {

  const canvas = canvasRef.value

  if (!canvas) return

  const rect = canvas.getBoundingClientRect()

  const x = event.clientX - rect.left

  const y = event.clientY - rect.top



  if (isPanning.value) {

    panX.value = panStart.value.panX + (x - panStart.value.x)

    panY.value = panStart.value.panY + (y - panStart.value.y)

    draw()

    return

  }



  const nearest = findNearestPoint(plottedCache, x, y)

  const next = nearest?.point ?? null

  if (hovered.value?.EntityId !== next?.EntityId) {

    hovered.value = next

    emit('hover', next)

    draw()

  }



  if (next) {

    tooltipPos.value = { x: x + 12, y: y + 12 }

  }

}



function onPointerDown(event: MouseEvent) {

  if (event.button === 1 || event.altKey) {

    isPanning.value = true

    const rect = canvasRef.value?.getBoundingClientRect()

    if (!rect) return

    panStart.value = {

      x: event.clientX - rect.left,

      y: event.clientY - rect.top,

      panX: panX.value,

      panY: panY.value,

    }

    return

  }



  const canvas = canvasRef.value

  if (!canvas) return

  const rect = canvas.getBoundingClientRect()

  const x = event.clientX - rect.left

  const y = event.clientY - rect.top

  const nearest = findNearestPoint(plottedCache, x, y, 12)

  if (nearest) {

    emit('select', nearest.point)

  }

}



function onPointerUp() {

  isPanning.value = false

}



function onWheel(event: WheelEvent) {

  event.preventDefault()

  const factor = event.deltaY > 0 ? 0.9 : 1.1

  zoom.value = Math.min(4, Math.max(0.5, zoom.value * factor))

  draw()

}



function resetZoom() {

  zoom.value = 1

  panX.value = 0

  panY.value = 0

  draw()

}



function locateSelected() {

  const firstId = props.selectedEntityIds[0]

  if (!firstId) return

  const plot = plottedCache.find((p) => p.point.EntityId === firstId)

  if (!plot || !containerRef.value || !canvasRef.value) return

  const width = canvasRef.value.width / (window.devicePixelRatio || 1)

  const height = canvasRef.value.height / (window.devicePixelRatio || 1)

  panX.value = width / 2 - plot.screenX

  panY.value = height / 2 - plot.screenY

  zoom.value = 1.5

  draw()

}



defineExpose({ resetZoom, locateSelected })



watch(

  () => [props.population, props.selectedEntityIds, props.investigationMode, props.searchHighlightIds],

  () => draw(),

  { deep: true },

)



onMounted(() => {

  resizeObserver = new ResizeObserver(() => resizeCanvas())

  if (containerRef.value) resizeObserver.observe(containerRef.value)

  resizeCanvas()

})



onUnmounted(() => {

  resizeObserver?.disconnect()

})

</script>



<template>

  <div ref="containerRef" class="iw-map-canvas-wrap">

    <div v-if="loading" class="iw-skeleton" />

    <canvas

      v-show="!loading"

      ref="canvasRef"

      class="iw-map-canvas"

      @mousemove="onPointerMove"

      @mousedown="onPointerDown"

      @mouseup="onPointerUp"

      @mouseleave="onPointerUp"

      @wheel.prevent="onWheel"

    />

    <div

      v-if="hovered"

      class="iw-tooltip"

      :style="{ left: `${tooltipPos.x}px`, top: `${tooltipPos.y}px` }"

    >

      <strong>{{ hovered.DisplayName }}</strong>

      <div>{{ population?.AxisXLabel }}: {{ hovered.FormattedAxisX }}</div>

      <div>{{ population?.AxisYLabel }}: {{ hovered.FormattedAxisY }}</div>

      <div v-if="hovered.AxisXPercentile != null">

        {{ population?.AxisXLabel }}: above {{ hovered.AxisXPercentile.toFixed(0) }}% of peers

      </div>

      <div v-if="hovered.ActiveAttentionCount">

        {{ hovered.ActiveAttentionCount }} active signal(s)

      </div>

      <div v-else>No active signals</div>

    </div>

  </div>

</template>


