<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import Checkbox from 'primevue/checkbox'
import maplibregl, { type GeoJSONSource, type Map as MapLibreMap } from 'maplibre-gl'
import 'maplibre-gl/dist/maplibre-gl.css'
import type { FieldActivityResponse } from '@/models/fieldActivity'

const props = defineProps<{
  data: FieldActivityResponse | null
  selectedIndex: number
  loading?: boolean
}>()

const emit = defineEmits<{
  'stop-selected': [index: number]
}>()

const mapContainer = ref<HTMLDivElement | null>(null)
const mapInstance = ref<MapLibreMap | null>(null)

const showPlannedRoute = ref(true)
const showActualRoute = ref(true)
const showPlannedPins = ref(true)
const showActualPins = ref(true)
const showMissedPins = ref(true)
const showUnplannedPins = ref(true)

const hasData = computed(() => props.data != null)

const osmStyle = {
  version: 8 as const,
  sources: {
    osm: {
      type: 'raster' as const,
      tiles: ['https://tile.openstreetmap.org/{z}/{x}/{y}.png'],
      tileSize: 256,
      attribution: '© OpenStreetMap contributors',
    },
  },
  layers: [
    {
      id: 'osm',
      type: 'raster' as const,
      source: 'osm',
    },
  ],
}

function gpsColor(level: string): string {
  switch (level) {
    case 'Valid':
      return '#22c55e'
    case 'Warning':
      return '#eab308'
    case 'Suspicious':
      return '#ef4444'
    default:
      return '#64748b'
  }
}

function buildPointFeatures(
  stops: Array<{ Latitude: number; Longitude: number; label: string; color: string; kind: string; index?: number }>,
) {
  return {
    type: 'FeatureCollection' as const,
    features: stops.map((stop) => ({
      type: 'Feature' as const,
      geometry: {
        type: 'Point' as const,
        coordinates: [stop.Longitude, stop.Latitude],
      },
      properties: {
        label: stop.label,
        color: stop.color,
        kind: stop.kind,
        index: stop.index ?? -1,
      },
    })),
  }
}

function updateMapData(): void {
  const map = mapInstance.value
  const data = props.data
  if (!map || !data) return

  const plannedLine = {
    type: 'Feature' as const,
    geometry: {
      type: 'LineString' as const,
      coordinates: data.RouteGeometry.Planned.Coordinates ?? [],
    },
    properties: {},
  }

  const actualLine = {
    type: 'Feature' as const,
    geometry: {
      type: 'LineString' as const,
      coordinates: data.RouteGeometry.Actual.Coordinates ?? [],
    },
    properties: {},
  }

  const plannedPinStops = data.PlannedStops.filter(
    (stop) => stop.HasCoordinates && stop.VisitStatus === 'Visited',
  ).map((stop) => ({
    Latitude: stop.Latitude,
    Longitude: stop.Longitude,
    label: String(stop.NoUrut),
    color: '#0d9488',
    kind: 'planned',
  }))

  const missedPinStops = data.PlannedStops.filter(
    (stop) => stop.HasCoordinates && stop.VisitStatus === 'Missed',
  ).map((stop) => ({
    Latitude: stop.Latitude,
    Longitude: stop.Longitude,
    label: String(stop.NoUrut),
    color: '#ef4444',
    kind: 'missed',
  }))

  const actualPinStops = data.ActualStops.filter((stop) => stop.HasCoordinates).map((stop, index) => ({
    Latitude: stop.Latitude,
    Longitude: stop.Longitude,
    label: String(stop.Sequence),
    color: stop.VisitStatus === 'Unplanned' ? '#3b82f6' : gpsColor(stop.GpsValidation),
    kind: stop.VisitStatus === 'Unplanned' ? 'unplanned' : 'actual',
    index,
  }))

  ;(map.getSource('planned-route') as GeoJSONSource | undefined)?.setData(plannedLine)
  ;(map.getSource('actual-route') as GeoJSONSource | undefined)?.setData(actualLine)
  ;(map.getSource('planned-pins') as GeoJSONSource | undefined)?.setData(
    buildPointFeatures(plannedPinStops),
  )
  ;(map.getSource('missed-pins') as GeoJSONSource | undefined)?.setData(
    buildPointFeatures(missedPinStops),
  )
  ;(map.getSource('actual-pins') as GeoJSONSource | undefined)?.setData(
    buildPointFeatures(actualPinStops),
  )
  ;(map.getSource('unplanned-pins') as GeoJSONSource | undefined)?.setData(
    buildPointFeatures(actualPinStops.filter((stop) => stop.kind === 'unplanned')),
  )

  fitMapBounds(data)
  updateReplayMarker()
}

function fitMapBounds(data: FieldActivityResponse): void {
  const map = mapInstance.value
  if (!map) return

  const coords: [number, number][] = []

  for (const stop of data.PlannedStops) {
    if (stop.HasCoordinates) coords.push([stop.Longitude, stop.Latitude])
  }
  for (const stop of data.ActualStops) {
    if (stop.HasCoordinates) coords.push([stop.Longitude, stop.Latitude])
  }

  if (coords.length === 0) {
    map.setCenter([106.8, -6.2])
    map.setZoom(11)
    return
  }

  const bounds = coords.reduce(
    (acc, coord) => acc.extend(coord),
    new maplibregl.LngLatBounds(coords[0], coords[0]),
  )

  map.fitBounds(bounds, { padding: 48, maxZoom: 15 })
}

function updateReplayMarker(): void {
  const map = mapInstance.value
  const data = props.data
  if (!map || !data) return

  const stop = props.selectedIndex >= 0 ? data.ActualStops[props.selectedIndex] : null
  const feature = stop?.HasCoordinates
    ? {
        type: 'Feature' as const,
        geometry: {
          type: 'Point' as const,
          coordinates: [stop.Longitude, stop.Latitude],
        },
        properties: {},
      }
    : {
        type: 'Feature' as const,
        geometry: {
          type: 'Point' as const,
          coordinates: [0, 0],
        },
        properties: {},
      }

  ;(map.getSource('replay-marker') as GeoJSONSource | undefined)?.setData(feature)
  map.setLayoutProperty(
    'replay-marker-circle',
    'visibility',
    stop?.HasCoordinates ? 'visible' : 'none',
  )
}

function setLayerVisibility(layerId: string, visible: boolean): void {
  const map = mapInstance.value
  if (!map || !map.getLayer(layerId)) return
  map.setLayoutProperty(layerId, 'visibility', visible ? 'visible' : 'none')
}

function applyLayerToggles(): void {
  setLayerVisibility('planned-route-line', showPlannedRoute.value)
  setLayerVisibility('actual-route-line', showActualRoute.value)
  setLayerVisibility('planned-pins-circle', showPlannedPins.value)
  setLayerVisibility('planned-pins-label', showPlannedPins.value)
  setLayerVisibility('actual-pins-circle', showActualPins.value)
  setLayerVisibility('actual-pins-label', showActualPins.value)
  setLayerVisibility('missed-pins-circle', showMissedPins.value)
  setLayerVisibility('missed-pins-label', showMissedPins.value)
  setLayerVisibility('unplanned-pins-circle', showUnplannedPins.value)
  setLayerVisibility('unplanned-pins-label', showUnplannedPins.value)
}

function initializeMap(): void {
  if (!mapContainer.value || mapInstance.value) return

  const map = new maplibregl.Map({
    container: mapContainer.value,
    style: osmStyle,
    center: [106.8, -6.2],
    zoom: 11,
  })

  map.addControl(new maplibregl.NavigationControl(), 'top-right')

  map.on('load', () => {
    map.addSource('planned-route', {
      type: 'geojson',
      data: { type: 'Feature', geometry: { type: 'LineString', coordinates: [] }, properties: {} },
    })
    map.addSource('actual-route', {
      type: 'geojson',
      data: { type: 'Feature', geometry: { type: 'LineString', coordinates: [] }, properties: {} },
    })
    map.addSource('planned-pins', { type: 'geojson', data: { type: 'FeatureCollection', features: [] } })
    map.addSource('missed-pins', { type: 'geojson', data: { type: 'FeatureCollection', features: [] } })
    map.addSource('actual-pins', { type: 'geojson', data: { type: 'FeatureCollection', features: [] } })
    map.addSource('unplanned-pins', { type: 'geojson', data: { type: 'FeatureCollection', features: [] } })
    map.addSource('replay-marker', {
      type: 'geojson',
      data: { type: 'Feature', geometry: { type: 'Point', coordinates: [0, 0] }, properties: {} },
    })

    map.addLayer({
      id: 'planned-route-line',
      type: 'line',
      source: 'planned-route',
      paint: {
        'line-color': '#0d9488',
        'line-width': 3,
        'line-dasharray': [2, 2],
      },
    })

    map.addLayer({
      id: 'actual-route-line',
      type: 'line',
      source: 'actual-route',
      paint: {
        'line-color': '#ea580c',
        'line-width': 4,
      },
    })

    addPinLayer(map, 'planned-pins', 'planned-pins-circle', 'planned-pins-label', '#0d9488', false)
    addPinLayer(map, 'missed-pins', 'missed-pins-circle', 'missed-pins-label', '#ef4444', true)
    addPinLayer(map, 'actual-pins', 'actual-pins-circle', 'actual-pins-label', ['get', 'color'], false)
    addPinLayer(map, 'unplanned-pins', 'unplanned-pins-circle', 'unplanned-pins-label', '#3b82f6', false)

    map.addLayer({
      id: 'replay-marker-circle',
      type: 'circle',
      source: 'replay-marker',
      paint: {
        'circle-radius': 12,
        'circle-color': '#a855f7',
        'circle-stroke-width': 3,
        'circle-stroke-color': '#ffffff',
      },
    })

    map.on('click', 'actual-pins-circle', (event) => {
      const index = event.features?.[0]?.properties?.index
      if (typeof index === 'number' && index >= 0) {
        emit('stop-selected', index)
      }
    })

    applyLayerToggles()
    updateMapData()
  })

  mapInstance.value = map
}

function addPinLayer(
  map: MapLibreMap,
  sourceId: string,
  circleLayerId: string,
  labelLayerId: string,
  color: string | ['get', string],
  outlineOnly: boolean,
): void {
  map.addLayer({
    id: circleLayerId,
    type: 'circle',
    source: sourceId,
    paint: {
      'circle-radius': 14,
      'circle-color': outlineOnly ? 'rgba(0,0,0,0)' : color,
      'circle-stroke-width': outlineOnly ? 3 : 2,
      'circle-stroke-color': typeof color === 'string' ? color : '#64748b',
    },
  })

  map.addLayer({
    id: labelLayerId,
    type: 'symbol',
    source: sourceId,
    layout: {
      'text-field': ['get', 'label'],
      'text-size': 11,
      'text-font': ['Open Sans Bold', 'Arial Unicode MS Bold'],
    },
    paint: {
      'text-color': '#ffffff',
    },
  })
}

watch(
  () => props.data,
  () => {
    updateMapData()
  },
  { deep: true },
)

watch(
  () => props.selectedIndex,
  () => {
    updateReplayMarker()
  },
)

watch([showPlannedRoute, showActualRoute, showPlannedPins, showActualPins, showMissedPins, showUnplannedPins], () => {
  applyLayerToggles()
})

onMounted(() => {
  initializeMap()
})

onBeforeUnmount(() => {
  mapInstance.value?.remove()
  mapInstance.value = null
})
</script>

<template>
  <section class="field-activity-map">
    <div class="field-activity-map__legend">
      <span><i class="legend-line legend-line--planned" /> Planned route</span>
      <span><i class="legend-line legend-line--actual" /> Actual route</span>
      <span><i class="legend-dot legend-dot--missed" /> Missed</span>
      <span><i class="legend-dot legend-dot--unplanned" /> Unplanned</span>
    </div>

    <div class="field-activity-map__toggles">
      <label><Checkbox v-model="showPlannedRoute" binary /> Planned route</label>
      <label><Checkbox v-model="showActualRoute" binary /> Actual route</label>
      <label><Checkbox v-model="showPlannedPins" binary /> Planned pins</label>
      <label><Checkbox v-model="showActualPins" binary /> Actual pins</label>
      <label><Checkbox v-model="showMissedPins" binary /> Missed pins</label>
      <label><Checkbox v-model="showUnplannedPins" binary /> Unplanned pins</label>
    </div>

    <div class="field-activity-map__canvas-wrap">
      <div ref="mapContainer" class="field-activity-map__canvas" />
      <div v-if="loading" class="field-activity-map__overlay">
        <i class="pi pi-spin pi-spinner" />
        Loading map data...
      </div>
      <div v-else-if="!hasData" class="field-activity-map__overlay field-activity-map__overlay--muted">
        Select a salesman and date, then click Load.
      </div>
    </div>
  </section>
</template>

<style scoped>
.field-activity-map {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  height: 100%;
  min-height: 24rem;
}

.field-activity-map__legend,
.field-activity-map__toggles {
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem 1rem;
  font-size: 0.8125rem;
}

.field-activity-map__toggles label {
  display: inline-flex;
  align-items: center;
  gap: 0.375rem;
}

.field-activity-map__canvas-wrap {
  position: relative;
  flex: 1;
  min-height: 20rem;
  border: 1px solid var(--p-content-border-color);
  border-radius: 0.75rem;
  overflow: hidden;
}

.field-activity-map__canvas {
  width: 100%;
  height: 100%;
  min-height: 20rem;
}

.field-activity-map__overlay {
  position: absolute;
  inset: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  background: rgba(255, 255, 255, 0.82);
  font-weight: 600;
}

.field-activity-map__overlay--muted {
  color: var(--p-text-muted-color);
  font-weight: 500;
}

.legend-line {
  display: inline-block;
  width: 1.25rem;
  height: 0;
  border-top-width: 3px;
  border-top-style: solid;
  margin-right: 0.25rem;
  vertical-align: middle;
}

.legend-line--planned {
  border-top-color: #0d9488;
  border-top-style: dashed;
}

.legend-line--actual {
  border-top-color: #ea580c;
}

.legend-dot {
  display: inline-block;
  width: 0.75rem;
  height: 0.75rem;
  border-radius: 999px;
  margin-right: 0.25rem;
  vertical-align: middle;
}

.legend-dot--missed {
  border: 2px solid #ef4444;
  background: transparent;
}

.legend-dot--unplanned {
  background: #3b82f6;
}
</style>
