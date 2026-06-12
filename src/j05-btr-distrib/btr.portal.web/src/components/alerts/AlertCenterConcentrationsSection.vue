<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import Panel from 'primevue/panel'
import Button from 'primevue/button'
import ProgressSpinner from 'primevue/progressspinner'
import type { DashboardAlertCenterConcentrationItem } from '@/models/dashboard'

const VISIBLE_LIMIT = 5

const props = defineProps<{
  items: DashboardAlertCenterConcentrationItem[]
  loading: boolean
}>()

const router = useRouter()
const collapsed = ref(true)
const showAll = ref(false)

const panelHeader = computed(() => {
  if (props.loading) return 'Concentrations'
  return `Concentrations (${props.items.length} informational)`
})

const visibleItems = computed(() => {
  if (showAll.value || props.items.length <= VISIBLE_LIMIT) {
    return props.items
  }

  return props.items.slice(0, VISIBLE_LIMIT)
})

const showToggle = computed(() => !props.loading && props.items.length > VISIBLE_LIMIT)

watch(
  () => props.items,
  () => {
    showAll.value = false
  },
)

function openDashboard(route: string): void {
  void router.push(route)
}

function toggleShowAll(): void {
  showAll.value = !showAll.value
}
</script>

<template>
  <section id="alert-concentrations" class="alert-center-concentrations">
    <Panel v-model:collapsed="collapsed" toggleable class="alert-center-concentrations__panel">
      <template #header>
        <span class="alert-center-concentrations__panel-title">{{ panelHeader }}</span>
      </template>

      <p class="alert-center-concentrations__subtitle">Informational — not exceptions</p>

      <div v-if="loading" class="alert-center-concentrations__loading">
        <ProgressSpinner style="width: 2rem; height: 2rem" stroke-width="4" />
      </div>

      <template v-else-if="items.length > 0">
        <ul
          class="alert-center-concentrations__list"
          :class="{ 'alert-center-concentrations__list--expanded': showAll }"
        >
          <li
            v-for="(item, index) in visibleItems"
            :key="`${item.Label}-${item.DashboardRoute}-${index}`"
            class="alert-center-concentrations__row"
          >
            <span class="alert-center-concentrations__label" :title="item.Label">
              {{ item.Label }}
            </span>
            <span class="alert-center-concentrations__value">
              {{ item.ValueText ?? '—' }}
            </span>
            <Button
              icon="pi pi-external-link"
              text
              rounded
              severity="secondary"
              class="alert-center-concentrations__link"
              aria-label="Open dashboard"
              @click="openDashboard(item.DashboardRoute)"
            />
          </li>
        </ul>

        <Button
          v-if="showToggle"
          :label="showAll ? 'Show fewer metrics' : `Show all ${items.length} metrics`"
          text
          size="small"
          class="alert-center-concentrations__toggle"
          @click="toggleShowAll"
        />
      </template>

      <p v-else class="alert-center-concentrations__empty">
        No concentration metrics available.
      </p>
    </Panel>
  </section>
</template>

<style scoped>
.alert-center-concentrations__panel {
  scroll-margin-top: 3.5rem;
}

.alert-center-concentrations__panel-title {
  font-size: 1.125rem;
  font-weight: 600;
}

.alert-center-concentrations__subtitle {
  margin: 0 0 0.75rem;
  font-size: 0.8125rem;
  color: var(--p-text-muted-color);
}

.alert-center-concentrations__loading,
.alert-center-concentrations__empty {
  margin: 0;
  padding: 0.75rem 0;
  text-align: center;
  color: var(--p-text-muted-color);
}

.alert-center-concentrations__list {
  list-style: none;
  margin: 0;
  padding: 0;
}

.alert-center-concentrations__list--expanded {
  max-height: 16rem;
  overflow: auto;
}

.alert-center-concentrations__row {
  display: grid;
  grid-template-columns: minmax(0, 1fr) auto auto;
  gap: 0.5rem 0.75rem;
  align-items: center;
  min-height: 2.25rem;
  padding: 0.25rem 0;
  border-bottom: 1px solid var(--p-content-border-color);
}

.alert-center-concentrations__row:last-child {
  border-bottom: none;
}

.alert-center-concentrations__label {
  font-size: 0.875rem;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.alert-center-concentrations__value {
  font-size: 0.875rem;
  font-weight: 600;
  white-space: nowrap;
}

.alert-center-concentrations__link {
  flex-shrink: 0;
}

.alert-center-concentrations__toggle {
  margin-top: 0.5rem;
  padding-left: 0;
}

@media (max-width: 767px) {
  .alert-center-concentrations__link {
    min-width: 2.75rem;
    min-height: 2.75rem;
  }
}
</style>
