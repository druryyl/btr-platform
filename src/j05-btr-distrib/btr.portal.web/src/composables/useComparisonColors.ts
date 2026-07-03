export const WORKSPACE_COMPARISON_COLORS = [
  { border: 'rgb(230, 126, 34)', fill: 'rgb(230, 126, 34)', label: 'Entity 1' },
  { border: 'rgb(52, 152, 219)', fill: 'rgb(52, 152, 219)', label: 'Entity 2' },
  { border: 'rgb(155, 89, 182)', fill: 'rgb(155, 89, 182)', label: 'Entity 3' },
  { border: 'rgb(26, 188, 156)', fill: 'rgb(26, 188, 156)', label: 'Entity 4' },
] as const

export const WORKSPACE_MAP_PLOT_BG = '#e8ebf0'
export const WORKSPACE_MAP_PLOT_BORDER = 'rgba(148, 163, 184, 0.25)'

export const WORKSPACE_NEUTRAL_POINT = '#94a3b8'
export const WORKSPACE_NEUTRAL_POINT_OPACITY = 0.52

export const WORKSPACE_ATTENTION_POINT = '#d97706'
export const WORKSPACE_ATTENTION_POINT_OPACITY = 0.88

export const WORKSPACE_CRITICAL_POINT = '#dc2626'
export const WORKSPACE_CRITICAL_POINT_OPACITY = 0.94

export const WORKSPACE_HOVER_POINT = '#475569'
export const WORKSPACE_HOVER_POINT_OPACITY = 0.9

export const WORKSPACE_REFERENCE_LINE = 'rgba(71, 85, 105, 0.55)'

export const WORKSPACE_BAND_2_FILL = 'rgba(148, 163, 184, 0.05)'
export const WORKSPACE_BAND_1_FILL = 'rgba(148, 163, 184, 0.07)'
export const WORKSPACE_BAND_STROKE = 'rgba(148, 163, 184, 0.18)'

export const WORKSPACE_WATCH_POINT = '#b45309'
export const WORKSPACE_WATCH_POINT_OPACITY = 0.65

export const WORKSPACE_NORMAL_RADIUS = 3
export const WORKSPACE_WATCH_RADIUS = 3.5
export const WORKSPACE_SEARCH_RADIUS_BONUS = 2
export const WORKSPACE_ATTENTION_RADIUS = 4
export const WORKSPACE_CRITICAL_RADIUS = 4.75
export const WORKSPACE_SELECTED_RADIUS = 6.5
export const WORKSPACE_HOVER_RADIUS = 5
export const WORKSPACE_HOVER_ENLARGE = 1
export const WORKSPACE_HALO_PADDING = 1.5
export const WORKSPACE_SELECTED_STROKE_WIDTH = 2

export const WORKSPACE_MAP_AXIS_COLOR = '#475569'
export const WORKSPACE_MAP_GRID_LINE = 'rgba(148, 163, 184, 0.35)'
export const WORKSPACE_MAP_TICK_COLOR = '#64748b'
export const WORKSPACE_MAP_QUADRANT_LINE = 'rgba(71, 85, 105, 0.65)'
export const WORKSPACE_MAP_QUADRANT_LABEL = '#334155'
export const WORKSPACE_MAP_QUADRANT_TINT_A = 'rgba(148, 163, 184, 0.04)'
export const WORKSPACE_MAP_QUADRANT_TINT_B = 'rgba(100, 116, 139, 0.035)'

export const WORKSPACE_LEGEND_SWATCH_DIAMETER =
  (WORKSPACE_SELECTED_RADIUS + WORKSPACE_HALO_PADDING) * 2

export function resolveComparisonColor(index: number) {
  return WORKSPACE_COMPARISON_COLORS[index % WORKSPACE_COMPARISON_COLORS.length]
}

export function buildEntityColorMap(entityIds: string[]): Map<string, (typeof WORKSPACE_COMPARISON_COLORS)[number]> {
  const map = new Map<string, (typeof WORKSPACE_COMPARISON_COLORS)[number]>()
  entityIds.forEach((id, index) => {
    map.set(id, resolveComparisonColor(index))
  })
  return map
}
