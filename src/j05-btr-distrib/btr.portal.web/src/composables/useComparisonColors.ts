export const WORKSPACE_COMPARISON_COLORS = [
  { border: 'rgb(230, 126, 34)', fill: 'rgb(230, 126, 34)', label: 'Entity 1' },
  { border: 'rgb(52, 152, 219)', fill: 'rgb(52, 152, 219)', label: 'Entity 2' },
  { border: 'rgb(155, 89, 182)', fill: 'rgb(155, 89, 182)', label: 'Entity 3' },
  { border: 'rgb(26, 188, 156)', fill: 'rgb(26, 188, 156)', label: 'Entity 4' },
] as const

export const WORKSPACE_MAP_PLOT_BG = '#e8ebf0'
export const WORKSPACE_MAP_PLOT_BORDER = 'rgba(148, 163, 184, 0.25)'

export const WORKSPACE_NEUTRAL_POINT = '#94a3b8'
export const WORKSPACE_NEUTRAL_POINT_OPACITY = 0.38

export const WORKSPACE_ATTENTION_POINT = '#d97706'
export const WORKSPACE_ATTENTION_POINT_OPACITY = 0.85

export const WORKSPACE_CRITICAL_POINT = '#dc2626'
export const WORKSPACE_CRITICAL_POINT_OPACITY = 0.92

export const WORKSPACE_HOVER_POINT = '#475569'
export const WORKSPACE_HOVER_POINT_OPACITY = 0.9

export const WORKSPACE_REFERENCE_LINE = 'rgba(156, 163, 175, 0.6)'

export const WORKSPACE_NORMAL_RADIUS = 2.75
export const WORKSPACE_ATTENTION_RADIUS = 4
export const WORKSPACE_CRITICAL_RADIUS = 4.75
export const WORKSPACE_SELECTED_RADIUS = 6.5
export const WORKSPACE_HOVER_RADIUS = 5
export const WORKSPACE_HOVER_ENLARGE = 1
export const WORKSPACE_HALO_PADDING = 1.5
export const WORKSPACE_SELECTED_STROKE_WIDTH = 2

export const WORKSPACE_MAP_AXIS_COLOR = '#475569'
export const WORKSPACE_MAP_QUADRANT_LINE = 'rgba(100, 116, 139, 0.55)'
export const WORKSPACE_MAP_QUADRANT_LABEL = 'rgba(71, 85, 105, 0.72)'

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
