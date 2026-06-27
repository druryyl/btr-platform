<script lang="ts">
export type KpiGroup = 'execution' | 'productivity' | 'quality' | 'issues'
export type KpiChipStatus = 'healthy' | 'warning' | 'critical' | 'unknown' | 'stable'
export type ProgressStatus = 'healthy' | 'warning' | 'critical' | 'unknown'
</script>

<script setup lang="ts">
import ProgressSpinner from 'primevue/progressspinner'
import KpiChip from './KpiChip.vue'
import KpiProgress from './KpiProgress.vue'

withDefaults(
  defineProps<{
    title: string
    value: string
    icon?: string
    kpiGroup?: KpiGroup
    subtitle?: string
    progress?: number | null
    progressStatus?: ProgressStatus | null
    chipLabel?: string
    chipStatus?: KpiChipStatus
    loading?: boolean
    size?: 'normal' | 'large'
  }>(),
  {
    kpiGroup: 'execution',
    loading: false,
    size: 'normal',
    progress: undefined,
    progressStatus: null,
  },
)
</script>

<template>
  <div class="dm-card" :class="[`dm-card--${kpiGroup}`, { 'dm-card--large': size === 'large' }]">
    <div class="dm-card__header">
      <div class="dm-card__title-stack">
        <span class="dm-card__title">{{ title }}</span>
        <span v-if="subtitle" class="dm-card__subtitle">{{ subtitle }}</span>
      </div>
      <span v-if="icon" class="dm-card__icon-wrap" aria-hidden="true">
        <i :class="icon" class="dm-card__icon" />
      </span>
    </div>

    <div v-if="loading" class="dm-card__loading">
      <ProgressSpinner style="width: 1.75rem; height: 1.75rem" stroke-width="4" />
    </div>
    <template v-else>
      <div class="dm-card__value">{{ value }}</div>
      <KpiProgress v-if="progress != null" :value="progress" :status="progressStatus" />
      <KpiChip v-if="chipLabel && chipStatus" :label="chipLabel" :status="chipStatus" />
    </template>
  </div>
</template>

<style scoped>
/* ── Group color tokens ─────────────────────────── */
.dm-card--execution {
  --dm-color: #2563eb;
}

.dm-card--productivity {
  --dm-color: #7c3aed;
}

.dm-card--quality {
  --dm-color: #15803d;
}

.dm-card--issues {
  --dm-color: #ea580c;
}

/* ── Card shell ─────────────────────────────────── */
.dm-card {
  background: color-mix(in srgb, var(--dm-color, #6366f1) 3%, white);
  border-radius: var(--dashboard-radius);
  border-top: 3px solid var(--dm-color, #6366f1);
  box-shadow: var(--dashboard-shadow-idle);
  padding: 0.875rem;
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  transition:
    box-shadow var(--dashboard-transition),
    transform var(--dashboard-transition);
  min-width: 0;
}

.dm-card:hover {
  box-shadow: var(--dashboard-shadow-hover);
  transform: translateY(-2px);
}

/* ── Header (title + icon side-by-side) ─────────── */
.dm-card__header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 0.5rem;
}

.dm-card__title-stack {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 0.1rem;
}

.dm-card__title {
  font-size: 0.6875rem;
  font-weight: 600;
  color: var(--p-text-muted-color, #64748b);
  letter-spacing: 0.01em;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.dm-card__subtitle {
  font-size: 0.625rem;
  font-weight: 400;
  color: var(--p-text-muted-color, #94a3b8);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  opacity: 0.72;
}

/* ── Icon circle ────────────────────────────────── */
.dm-card__icon-wrap {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 2.5rem;
  height: 2.5rem;
  border-radius: 50%;
  background: color-mix(in srgb, var(--dm-color, #6366f1) 11%, white);
  flex-shrink: 0;
}

.dm-card__icon {
  font-size: 0.9375rem;
  color: var(--dm-color, #6366f1);
}

/* ── KPI value ──────────────────────────────────── */
.dm-card__value {
  font-size: 1.625rem;
  font-weight: 800;
  line-height: 1.15;
  letter-spacing: -0.03em;
  font-variant-numeric: tabular-nums;
  color: var(--p-text-color);
  margin-top: 0.125rem;
}

.dm-card--large .dm-card__value {
  font-size: 1.875rem;
}

/* ── Loading state ──────────────────────────────── */
.dm-card__loading {
  display: flex;
  justify-content: flex-start;
  align-items: center;
  padding: 0.25rem 0;
}
</style>
