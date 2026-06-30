import type { PopulationMapPoint } from '@/models/entityAnalytics'



export interface MapBounds {

  minX: number

  maxX: number

  minY: number

  maxY: number

}



export interface MapTransform {

  padding: number

  scaleX: number

  scaleY: number

  offsetX: number

  offsetY: number

  plotWidth: number

  plotHeight: number

  bounds: MapBounds

}



export interface PlottedPoint {

  point: PopulationMapPoint

  screenX: number

  screenY: number

  index: number

}



/** Tunable by Product without changing classification logic. */

export const CRITICAL_PERCENTILE_THRESHOLD = 10



export type PopulationPointTier = 'normal' | 'attention' | 'critical'



export interface LabelCandidate {

  entityId: string

  text: string

  anchorX: number

  anchorY: number

  priority: number

}



export interface ResolvedLabel extends LabelCandidate {

  boxX: number

  boxY: number

  boxW: number

  boxH: number

  leader?: { fromX: number; fromY: number; toX: number; toY: number }

}



export interface LabelMetrics {

  boxW: number

  boxH: number

  padX: number

  padY: number

  font: string

}



const DEFAULT_PADDING = 48

const LABEL_FONT = '11px system-ui, sans-serif'

const LABEL_PAD_X = 4

const LABEL_PAD_Y = 2

const LABEL_TEXT_HEIGHT = 14

const LABEL_COLLISION_PAD = 2

const LABEL_LEADER_DISTANCE = 12



const LABEL_OFFSETS: Array<{ dx: number; dy: number }> = [

  { dx: 10, dy: -10 },

  { dx: -10, dy: -10 },

  { dx: 10, dy: 10 },

  { dx: -10, dy: 10 },

  { dx: 0, dy: -14 },

  { dx: 0, dy: 14 },

]



function isExtremePercentile(

  percentile: number | null | undefined,

  threshold = CRITICAL_PERCENTILE_THRESHOLD,

): boolean {

  if (percentile == null) return false

  const upper = 100 - threshold

  return percentile <= threshold || percentile >= upper

}



export function resolvePopulationPointTier(point: PopulationMapPoint): PopulationPointTier {

  const count = point.ActiveAttentionCount ?? 0

  if (count <= 0) return 'normal'



  const isCritical =

    count >= 2

    || (count >= 1 && (

      isExtremePercentile(point.AxisXPercentile)

      || isExtremePercentile(point.AxisYPercentile)

    ))



  return isCritical ? 'critical' : 'attention'

}



export function measureLabel(

  ctx: CanvasRenderingContext2D,

  text: string,

): LabelMetrics {

  ctx.font = LABEL_FONT

  const metrics = ctx.measureText(text)

  const boxW = metrics.width + LABEL_PAD_X * 2

  const boxH = LABEL_TEXT_HEIGHT + LABEL_PAD_Y * 2

  return {

    boxW,

    boxH,

    padX: LABEL_PAD_X,

    padY: LABEL_PAD_Y,

    font: LABEL_FONT,

  }

}



function boxesOverlap(

  a: { x: number; y: number; w: number; h: number },

  b: { x: number; y: number; w: number; h: number },

  pad: number,

): boolean {

  return !(

    a.x + a.w + pad <= b.x

    || b.x + b.w + pad <= a.x

    || a.y + a.h + pad <= b.y

    || b.y + b.h + pad <= a.y

  )

}



export function resolveLabelPlacements(

  candidates: LabelCandidate[],

  ctx: CanvasRenderingContext2D,

): ResolvedLabel[] {

  const sorted = [...candidates].sort((a, b) => b.priority - a.priority)

  const placed: ResolvedLabel[] = []



  for (const candidate of sorted) {

    const { boxW, boxH } = measureLabel(ctx, candidate.text)



    for (const { dx, dy } of LABEL_OFFSETS) {

      const boxX = candidate.anchorX + dx

      const boxY = candidate.anchorY + dy - boxH



      const overlaps = placed.some((existing) =>

        boxesOverlap(

          { x: boxX, y: boxY, w: boxW, h: boxH },

          { x: existing.boxX, y: existing.boxY, w: existing.boxW, h: existing.boxH },

          LABEL_COLLISION_PAD,

        ),

      )



      if (overlaps) continue



      const labelCenterX = boxX + boxW / 2

      const labelCenterY = boxY + boxH / 2

      const dist = Math.hypot(labelCenterX - candidate.anchorX, labelCenterY - candidate.anchorY)



      const resolved: ResolvedLabel = {

        ...candidate,

        boxX,

        boxY,

        boxW,

        boxH,

      }



      if (dist > LABEL_LEADER_DISTANCE) {

        resolved.leader = {

          fromX: candidate.anchorX,

          fromY: candidate.anchorY,

          toX: labelCenterX,

          toY: labelCenterY,

        }

      }



      placed.push(resolved)

      break

    }

  }



  return placed

}



export function computeBounds(points: PopulationMapPoint[]): MapBounds {

  const values = points.filter((p) => p.AxisX != null && p.AxisY != null)

  if (!values.length) {

    return { minX: 0, maxX: 1, minY: 0, maxY: 1 }

  }



  let minX = Infinity

  let maxX = -Infinity

  let minY = Infinity

  let maxY = -Infinity



  for (const p of values) {

    minX = Math.min(minX, p.AxisX!)

    maxX = Math.max(maxX, p.AxisX!)

    minY = Math.min(minY, p.AxisY!)

    maxY = Math.max(maxY, p.AxisY!)

  }



  const padX = (maxX - minX) * 0.05 || 1

  const padY = (maxY - minY) * 0.05 || 1



  return {

    minX: minX - padX,

    maxX: maxX + padX,

    minY: minY - padY,

    maxY: maxY + padY,

  }

}



export function buildTransform(

  width: number,

  height: number,

  bounds: MapBounds,

  zoom = 1,

  panX = 0,

  panY = 0,

): MapTransform {

  const padding = DEFAULT_PADDING

  const plotWidth = Math.max(width - padding * 2, 1)

  const plotHeight = Math.max(height - padding * 2, 1)



  const spanX = bounds.maxX - bounds.minX || 1

  const spanY = bounds.maxY - bounds.minY || 1



  const scaleX = (plotWidth / spanX) * zoom

  const scaleY = (plotHeight / spanY) * zoom



  return {

    padding,

    scaleX,

    scaleY,

    offsetX: padding + panX,

    offsetY: padding + panY,

    plotWidth,

    plotHeight,

    bounds,

  }

}



export function dataToScreen(

  x: number,

  y: number,

  transform: MapTransform,

): { x: number; y: number } {

  const screenX = transform.offsetX + (x - transform.bounds.minX) * transform.scaleX

  const screenY =

    transform.offsetY

    + transform.plotHeight

    - (y - transform.bounds.minY) * transform.scaleY

  return { x: screenX, y: screenY }

}



export function plotPoints(

  points: PopulationMapPoint[],

  transform: MapTransform,

): PlottedPoint[] {

  return points

    .map((point, index) => {

      if (point.AxisX == null || point.AxisY == null) return null

      const { x, y } = dataToScreen(point.AxisX, point.AxisY, transform)

      return { point, screenX: x, screenY: y, index }

    })

    .filter((p): p is PlottedPoint => p != null)

}



export function findNearestPoint(

  plotted: PlottedPoint[],

  x: number,

  y: number,

  radius = 10,

): PlottedPoint | null {

  let best: PlottedPoint | null = null

  let bestDist = radius * radius



  for (const item of plotted) {

    const dx = item.screenX - x

    const dy = item.screenY - y

    const dist = dx * dx + dy * dy

    if (dist <= bestDist) {

      bestDist = dist

      best = item

    }

  }



  return best

}



export function formatPercentile(value: number | null | undefined): string {

  if (value == null || Number.isNaN(value)) return '—'

  return `Above ${value.toFixed(0)}% of peers`

}


