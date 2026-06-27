type ChartOptionsConfig = Record<string, unknown>

const compactLayoutPadding = {
  top: 2,
  bottom: 0,
  left: 0,
  right: 0,
}

export const compactLegendLabels = {
  boxWidth: 10,
  boxHeight: 10,
  padding: 6,
  usePointStyle: true,
  pointStyle: 'circle' as const,
  font: { size: 11 },
}

export const chartLegend = {
  hidden: () => ({ display: false }),
  bottom: () => ({
    display: true,
    position: 'bottom' as const,
    align: 'center' as const,
    labels: compactLegendLabels,
  }),
  right: () => ({
    display: true,
    position: 'right' as const,
    align: 'center' as const,
    labels: {
      ...compactLegendLabels,
      padding: 8,
    },
  }),
}

export const compactAxisTitle = (text: string) => ({
  display: true,
  text,
  padding: { top: 0, bottom: 2 },
  font: { size: 11 },
})

export function createChartOptions(overrides: ChartOptionsConfig = {}): ChartOptionsConfig {
  const { layout, plugins, ...rest } = overrides
  const layoutObject =
    layout && typeof layout === 'object' ? (layout as Record<string, unknown>) : {}
  const layoutPadding =
    layoutObject.padding && typeof layoutObject.padding === 'object'
      ? { ...compactLayoutPadding, ...(layoutObject.padding as Record<string, unknown>) }
      : compactLayoutPadding

  return {
    responsive: true,
    maintainAspectRatio: false,
    layout: {
      ...layoutObject,
      padding: layoutPadding,
    },
    plugins: {
      legend: chartLegend.hidden(),
      ...(plugins && typeof plugins === 'object' ? (plugins as Record<string, unknown>) : {}),
    },
    ...rest,
  }
}
