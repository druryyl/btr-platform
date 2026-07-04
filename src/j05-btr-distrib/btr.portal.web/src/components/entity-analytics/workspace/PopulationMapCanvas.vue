<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref, watch } from 'vue'
import type { PopulationMapPoint, PopulationMapResponse } from '@/models/entityAnalytics'
import {
  buildEntityColorMap,
  WORKSPACE_ATTENTION_POINT,
  WORKSPACE_ATTENTION_POINT_OPACITY,
  WORKSPACE_ATTENTION_RADIUS,
  WORKSPACE_BAND_1_FILL,
  WORKSPACE_BAND_2_FILL,
  WORKSPACE_BAND_STROKE,
  WORKSPACE_CRITICAL_POINT,
  WORKSPACE_CRITICAL_POINT_OPACITY,
  WORKSPACE_CRITICAL_RADIUS,
  WORKSPACE_HALO_PADDING,
  WORKSPACE_HOVER_ENLARGE,
  WORKSPACE_HOVER_POINT,
  WORKSPACE_HOVER_POINT_OPACITY,
  WORKSPACE_HOVER_RADIUS,
  WORKSPACE_MAP_AXIS_COLOR,
  WORKSPACE_MAP_GRID_LINE,
  WORKSPACE_MAP_PLOT_BG,
  WORKSPACE_MAP_PLOT_BORDER,
  WORKSPACE_MAP_QUADRANT_LABEL,
  WORKSPACE_MAP_TICK_COLOR,
  WORKSPACE_NEUTRAL_POINT,
  WORKSPACE_NEUTRAL_POINT_OPACITY,
  WORKSPACE_NORMAL_RADIUS,
  WORKSPACE_REFERENCE_LINE,
  WORKSPACE_SEARCH_RADIUS_BONUS,
  WORKSPACE_SELECTED_RADIUS,
  WORKSPACE_SELECTED_STROKE_WIDTH,
  WORKSPACE_WATCH_POINT,
  WORKSPACE_WATCH_POINT_OPACITY,
  WORKSPACE_WATCH_RADIUS,
} from '@/composables/useComparisonColors'
import {
  buildAutoLabelCandidates,
  buildSpatialIndex,
  buildTransform,
  computeBounds,
  dataToScreen,
  findNearestPoint,
  formatAxisTickValue,
  generateProjectionAxisGuides,
  generateProjectionGridTicks,
  measureLabel,
  plotPoints,
  projectionToScreenX,
  projectionToScreenY,
  resolveBusinessAttentionTier,
  resolveLabelPlacements,
  resolveVisualTier,
  type MapTransform,
  type PlottedPoint,
  type ResolvedLabel,
  type SpatialIndex,
  type VisualPointTier,
} from '@/services/populationMapLayout'
import {
  populationProjectionEngine,
  projectedEntityToAnalyzed,
  type BandPolyline,
} from '@/services/populationProjection/populationProjectionEngine'
import PopulationMapTooltip from '@/components/entity-analytics/workspace/PopulationMapTooltip.vue'

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

const colorMap = computed(() => buildEntityColorMap(props.selectedEntityIds))
const selectedSet = computed(() => new Set(props.selectedEntityIds))

const visiblePoints = computed(() =>
  (props.population?.Points ?? []).filter((p) => p.AxisX != null && p.AxisY != null),
)

const searchSet = computed(() => new Set(props.searchHighlightIds ?? []))

const mapProjection = computed(() =>
  visiblePoints.value.length ? populationProjectionEngine.project(visiblePoints.value) : null,
)

const hoveredAnalyzed = computed(() => {
  if (!hovered.value || !mapProjection.value) return null
  const projected = mapProjection.value.entities.get(hovered.value.EntityId)
  return projected ? projectedEntityToAnalyzed(projected) : null
})

let plottedCache: PlottedPoint[] = []
let spatialIndexCache: SpatialIndex | null = null
let resizeObserver: ResizeObserver | null = null

interface PointDrawFlags {
  isSelected: boolean
  isHovered: boolean
  isSearchMatch: boolean
  hasSearch: boolean
  dimPopulation: boolean
}

const TIER_STYLES: Record<VisualPointTier, { color: string; opacity: number; radius: number }> = {
  normal: {
    color: WORKSPACE_NEUTRAL_POINT,
    opacity: WORKSPACE_NEUTRAL_POINT_OPACITY,
    radius: WORKSPACE_NORMAL_RADIUS,
  },
  watch: {
    color: WORKSPACE_WATCH_POINT,
    opacity: WORKSPACE_WATCH_POINT_OPACITY,
    radius: WORKSPACE_WATCH_RADIUS,
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

function getVisualTier(item: PlottedPoint): VisualPointTier {
  const businessTier = resolveBusinessAttentionTier(item.point)
  return resolveVisualTier(item.analyzed, businessTier)
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

function getEffectiveAlpha(
  point: PopulationMapPoint,
  flags: PointDrawFlags,
  baseOpacity: number,
): number {
  let alpha = baseOpacity

  if (flags.hasSearch && !flags.isSearchMatch && !flags.isSelected) {
    alpha = Math.min(alpha, 0.2)
  } else if (flags.dimPopulation && !flags.isSelected) {
    alpha = Math.min(alpha, 0.4)
  }

  if (!point.MatchesFilter && !flags.isSelected) {
    alpha = Math.min(alpha, 0.25)
  }

  return alpha
}

function applyPointAlpha(
  ctx: CanvasRenderingContext2D,
  point: PopulationMapPoint,
  flags: PointDrawFlags,
  baseOpacity: number,
) {
  ctx.globalAlpha = getEffectiveAlpha(point, flags, baseOpacity)
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

function drawBatchedTierPoints(ctx: CanvasRenderingContext2D, tier: VisualPointTier) {
  const style = TIER_STYLES[tier]
  const buckets = new Map<number, PlottedPoint[]>()

  for (const item of plottedCache) {
    const flags = getPointFlags(item.point)
    if (flags.isSelected || flags.isSearchMatch) continue
    if (getVisualTier(item) !== tier) continue

    const alphaKey = Math.round(getEffectiveAlpha(item.point, flags, style.opacity) * 1000)
    const bucket = buckets.get(alphaKey) ?? []
    bucket.push(item)
    buckets.set(alphaKey, bucket)
  }

  ctx.fillStyle = style.color
  for (const [alphaKey, items] of buckets) {
    ctx.globalAlpha = alphaKey / 1000
    ctx.beginPath()
    for (const item of items) {
      ctx.moveTo(item.screenX + style.radius, item.screenY)
      ctx.arc(item.screenX, item.screenY, style.radius, 0, Math.PI * 2)
    }
    ctx.fill()
  }
  ctx.globalAlpha = 1
}

function drawTieredPoint(
  ctx: CanvasRenderingContext2D,
  item: PlottedPoint,
  tier: VisualPointTier,
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

function drawPlotBackground(ctx: CanvasRenderingContext2D, transform: MapTransform) {
  const { offsetX, offsetY, plotWidth, plotHeight } = transform

  ctx.fillStyle = WORKSPACE_MAP_PLOT_BG
  ctx.fillRect(offsetX, offsetY, plotWidth, plotHeight)

  ctx.strokeStyle = WORKSPACE_MAP_PLOT_BORDER
  ctx.lineWidth = 1
  ctx.setLineDash([])
  ctx.strokeRect(offsetX, offsetY, plotWidth, plotHeight)
}

function polylineToScreen(
  points: BandPolyline[],
  transform: MapTransform,
): Array<{ x: number; y: number }> {
  return points.map((pt) => dataToScreen(pt.x, pt.y, transform))
}

function drawBandPolygon(
  ctx: CanvasRenderingContext2D,
  upper: BandPolyline[],
  lower: BandPolyline[],
  transform: MapTransform,
  fill: string,
) {
  const upperScreen = polylineToScreen(upper, transform)
  const lowerScreen = polylineToScreen(lower, transform)
  if (!upperScreen.length || !lowerScreen.length) return

  ctx.beginPath()
  ctx.moveTo(upperScreen[0].x, upperScreen[0].y)
  for (let i = 1; i < upperScreen.length; i++) {
    ctx.lineTo(upperScreen[i].x, upperScreen[i].y)
  }
  for (let i = lowerScreen.length - 1; i >= 0; i--) {
    ctx.lineTo(lowerScreen[i].x, lowerScreen[i].y)
  }
  ctx.closePath()
  ctx.fillStyle = fill
  ctx.fill()
}

function drawConfidenceBands(ctx: CanvasRenderingContext2D, transform: MapTransform) {
  const projection = mapProjection.value
  if (!projection) return

  const { bandGeometry } = projection
  const { offsetX, offsetY, plotWidth, plotHeight } = transform

  ctx.save()
  ctx.beginPath()
  ctx.rect(offsetX, offsetY, plotWidth, plotHeight)
  ctx.clip()

  drawBandPolygon(ctx, bandGeometry.upper2, bandGeometry.lower2, transform, WORKSPACE_BAND_2_FILL)
  drawBandPolygon(ctx, bandGeometry.upper1, bandGeometry.lower1, transform, WORKSPACE_BAND_1_FILL)

  ctx.strokeStyle = WORKSPACE_BAND_STROKE
  ctx.lineWidth = 1
  ctx.setLineDash([4, 4])

  for (const band of [bandGeometry.upper2, bandGeometry.lower2]) {
    const screen = polylineToScreen(band, transform)
    if (!screen.length) continue
    ctx.beginPath()
    ctx.moveTo(screen[0].x, screen[0].y)
    for (let i = 1; i < screen.length; i++) {
      ctx.lineTo(screen[i].x, screen[i].y)
    }
    ctx.stroke()
  }

  ctx.restore()
}

function drawRegressionLine(ctx: CanvasRenderingContext2D, transform: MapTransform) {
  const projection = mapProjection.value
  if (!projection) return

  const { offsetX, offsetY, plotWidth, plotHeight, bounds } = transform
  const { regression } = projection

  const xStart = bounds.minX
  const xEnd = bounds.maxX
  const yStart = regression.predictY(xStart)
  const yEnd = regression.predictY(xEnd)

  const start = dataToScreen(xStart, yStart, transform)
  const end = dataToScreen(xEnd, yEnd, transform)

  ctx.save()
  ctx.beginPath()
  ctx.rect(offsetX, offsetY, plotWidth, plotHeight)
  ctx.clip()

  ctx.strokeStyle = WORKSPACE_REFERENCE_LINE
  ctx.lineWidth = 1.5
  ctx.setLineDash([])
  ctx.beginPath()
  ctx.moveTo(start.x, start.y)
  ctx.lineTo(end.x, end.y)
  ctx.stroke()

  ctx.restore()
}

function drawAxes(
  ctx: CanvasRenderingContext2D,
  transform: MapTransform,
  height: number,
) {
  const projection = mapProjection.value
  if (!projection) return

  const { offsetX, offsetY, plotWidth, plotHeight, bounds } = transform
  const gridTicks = generateProjectionGridTicks(bounds)
  const { xTicks, yTicks } = generateProjectionAxisGuides(projection)

  ctx.strokeStyle = WORKSPACE_MAP_GRID_LINE
  ctx.lineWidth = 1
  ctx.setLineDash([])
  ctx.font = '500 10px system-ui, sans-serif'
  ctx.fillStyle = WORKSPACE_MAP_TICK_COLOR

  for (const tick of gridTicks.xTicks) {
    const x = projectionToScreenX(tick, transform)
    if (x < offsetX || x > offsetX + plotWidth) continue

    ctx.beginPath()
    ctx.moveTo(x, offsetY)
    ctx.lineTo(x, offsetY + plotHeight)
    ctx.stroke()
  }

  for (const tick of gridTicks.yTicks) {
    const y = projectionToScreenY(tick, transform)
    if (y < offsetY || y > offsetY + plotHeight) continue

    ctx.beginPath()
    ctx.moveTo(offsetX, y)
    ctx.lineTo(offsetX + plotWidth, y)
    ctx.stroke()
  }

  for (const tick of xTicks) {
    const x = projectionToScreenX(tick.projectionValue, transform)
    if (x < offsetX || x > offsetX + plotWidth) continue

    const label = formatAxisTickValue(tick.businessValue, props.population?.AxisXUnit)
    ctx.fillText(label, x + 3, offsetY + plotHeight + 14)
  }

  for (const tick of yTicks) {
    const y = projectionToScreenY(tick.projectionValue, transform)
    if (y < offsetY || y > offsetY + plotHeight) continue

    const label = formatAxisTickValue(tick.businessValue, props.population?.AxisYUnit)
    const labelWidth = ctx.measureText(label).width
    ctx.fillText(label, offsetX - labelWidth - 6, y + 3)
  }

  ctx.fillStyle = WORKSPACE_MAP_AXIS_COLOR
  ctx.font = '600 12px system-ui, sans-serif'

  if (props.population?.AxisXLabel) {
    ctx.fillText(props.population.AxisXLabel, offsetX, height - 4)
  }

  if (props.population?.AxisYLabel) {
    ctx.save()
    ctx.translate(12, offsetY + plotHeight / 2)
    ctx.rotate(-Math.PI / 2)
    ctx.fillText(props.population.AxisYLabel, 0, 0)
    ctx.restore()
  }
}

function drawRegionLabelPill(
  ctx: CanvasRenderingContext2D,
  text: string,
  x: number,
  y: number,
) {
  ctx.font = '600 10px system-ui, sans-serif'
  const metrics = ctx.measureText(text)
  const padX = 6
  const padY = 3
  const w = metrics.width + padX * 2
  const h = 14 + padY * 2

  ctx.fillStyle = 'rgba(255, 255, 255, 0.88)'
  ctx.fillRect(x, y, w, h)

  ctx.lineWidth = 3
  ctx.strokeStyle = '#ffffff'
  ctx.strokeText(text, x + padX, y + h - padY - 2)
  ctx.fillStyle = WORKSPACE_MAP_QUADRANT_LABEL
  ctx.fillText(text, x + padX, y + h - padY - 2)
}

function drawRegionLabels(ctx: CanvasRenderingContext2D, transform: MapTransform) {
  const projection = mapProjection.value
  if (!projection) return

  const { offsetX, offsetY, plotWidth, plotHeight } = transform
  const pad = 10
  const midX = offsetX + plotWidth * 0.72

  const centerMid = projection.bandGeometry.center[Math.floor(projection.bandGeometry.center.length / 2)]
  if (centerMid) {
    const screen = dataToScreen(centerMid.x, centerMid.y, transform)
    drawRegionLabelPill(ctx, 'Expected', screen.x - 30, screen.y - 10)
  }

  drawRegionLabelPill(ctx, 'Above Expected', midX, offsetY + pad)
  drawRegionLabelPill(ctx, 'Below Expected', midX, offsetY + plotHeight - 28)
}

function drawEmphasisTierPass(
  ctx: CanvasRenderingContext2D,
  tier: 'watch' | 'attention' | 'critical',
) {
  for (const item of plottedCache) {
    const flags = getPointFlags(item.point)
    if (flags.isSelected || flags.isSearchMatch) continue
    if (getVisualTier(item) !== tier) continue
    drawTieredPoint(ctx, item, tier, flags)
  }
}

function drawSearchHighlights(ctx: CanvasRenderingContext2D) {
  if (searchSet.value.size === 0) return

  for (const item of plottedCache) {
    const flags = getPointFlags(item.point)
    if (!flags.isSearchMatch || flags.isSelected) continue

    const tier = getVisualTier(item)
    const style = TIER_STYLES[tier]
    const radius = style.radius + WORKSPACE_SEARCH_RADIUS_BONUS

    ctx.globalAlpha = 1
    drawFilledCircle(ctx, item.screenX, item.screenY, radius + 1.5, '#ffffff')
    drawFilledCircle(ctx, item.screenX, item.screenY, radius, style.color)
    ctx.beginPath()
    ctx.arc(item.screenX, item.screenY, radius, 0, Math.PI * 2)
    ctx.strokeStyle = '#ffffff'
    ctx.lineWidth = 1.5
    ctx.stroke()
    ctx.globalAlpha = 1
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

function collectLabelCandidates() {
  return buildAutoLabelCandidates(plottedCache, {
    selectedIds: selectedSet.value,
    searchIds: searchSet.value,
    hoveredId: hovered.value?.EntityId ?? null,
  })
}

function drawResolvedLabel(ctx: CanvasRenderingContext2D, label: ResolvedLabel) {
  const metrics = measureLabel(ctx, label.text)
  const textX = label.boxX + metrics.padX
  const textY = label.boxY + label.boxH - metrics.padY - 2

  if (label.leader) {
    ctx.strokeStyle = 'rgba(148, 163, 184, 0.7)'
    ctx.lineWidth = 1
    ctx.beginPath()
    ctx.moveTo(label.leader.fromX, label.leader.fromY)
    ctx.lineTo(label.leader.toX, label.leader.toY)
    ctx.stroke()
  }

  ctx.font = metrics.font
  ctx.lineWidth = 3
  ctx.strokeStyle = '#ffffff'
  ctx.lineJoin = 'round'
  ctx.strokeText(label.text, textX, textY)
  ctx.fillStyle = '#1e293b'
  ctx.fillText(label.text, textX, textY)
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

  const tier = getVisualTier(plot)
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

  const projection = mapProjection.value
  const bounds = computeBounds(projection)
  const transform = buildTransform(width, height, bounds)
  plottedCache = plotPoints(visiblePoints.value, transform, projection)
  spatialIndexCache = buildSpatialIndex(plottedCache)

  drawPlotBackground(ctx, transform)
  drawConfidenceBands(ctx, transform)
  drawAxes(ctx, transform, height)
  drawRegressionLine(ctx, transform)
  drawRegionLabels(ctx, transform)
  drawBatchedTierPoints(ctx, 'normal')
  drawEmphasisTierPass(ctx, 'watch')
  drawEmphasisTierPass(ctx, 'attention')
  drawEmphasisTierPass(ctx, 'critical')
  drawSearchHighlights(ctx)
  drawSelectedEntities(ctx)
  drawLabels(ctx)
  drawHoverOverlay(ctx)
}

function onPointerMove(event: MouseEvent) {
  const canvas = canvasRef.value
  if (!canvas) return

  const rect = canvas.getBoundingClientRect()
  const x = event.clientX - rect.left
  const y = event.clientY - rect.top

  const nearest = findNearestPoint(plottedCache, x, y, 10, spatialIndexCache)
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
  if (event.button !== 0) return

  const canvas = canvasRef.value
  if (!canvas) return

  const rect = canvas.getBoundingClientRect()
  const x = event.clientX - rect.left
  const y = event.clientY - rect.top
  const nearest = findNearestPoint(plottedCache, x, y, 12, spatialIndexCache)

  if (nearest) {
    emit('select', nearest.point)
  }
}

function onPointerLeave() {
  if (hovered.value) {
    hovered.value = null
    emit('hover', null)
    draw()
  }
}

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
      @mouseleave="onPointerLeave"
    />
    <div
      v-if="hovered"
      class="iw-tooltip"
      :style="{ left: `${tooltipPos.x}px`, top: `${tooltipPos.y}px` }"
    >
      <PopulationMapTooltip
        :point="hovered"
        :population="population"
        :analyzed="hoveredAnalyzed"
      />
    </div>
  </div>
</template>
