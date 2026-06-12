<script setup lang="ts">
import { computed } from 'vue'
import { RouterLink } from 'vue-router'
import KpiCard from '@/components/KpiCard.vue'
import { usePresentationStore } from '@/stores/presentationStore'

const props = defineProps<{
  title: string
  icon: string
  loading?: boolean
  requiresAttention?: boolean
  unavailable?: boolean
  to?: string
  href?: string
}>()

const emit = defineEmits<{
  anchorNavigate: []
}>()

const presentation = usePresentationStore()

const showUnavailableLabel = computed(
  () => Boolean(props.unavailable) && !presentation.hidePlatformDiagnostics,
)

const canNavigate = computed(
  () => !props.unavailable || presentation.hidePlatformDiagnostics,
)
</script>

<template>
  <RouterLink
    v-if="to && canNavigate"
    :to="to"
    class="salesman-attention-card__wrapper salesman-attention-card__wrapper--link"
  >
    <KpiCard
      :title="title"
      :icon="icon"
      :loading="loading"
      class="salesman-attention-card"
      :class="{
        'salesman-attention-card--attention': requiresAttention,
        'salesman-attention-card--unavailable': showUnavailableLabel,
      }"
    >
      <slot />
    </KpiCard>
  </RouterLink>
  <a
    v-else-if="href && canNavigate"
    :href="href"
    class="salesman-attention-card__wrapper salesman-attention-card__wrapper--link"
    @click="emit('anchorNavigate')"
  >
    <KpiCard
      :title="title"
      :icon="icon"
      :loading="loading"
      class="salesman-attention-card"
      :class="{
        'salesman-attention-card--attention': requiresAttention,
        'salesman-attention-card--unavailable': showUnavailableLabel,
      }"
    >
      <slot />
    </KpiCard>
  </a>
  <div v-else class="salesman-attention-card__wrapper">
    <KpiCard
      :title="title"
      :icon="icon"
      :loading="loading"
      class="salesman-attention-card"
      :class="{
        'salesman-attention-card--attention': requiresAttention,
        'salesman-attention-card--unavailable': showUnavailableLabel,
      }"
    >
      <div v-if="showUnavailableLabel" class="salesman-attention-card__unavailable">
        Data unavailable
      </div>
      <slot v-else-if="canNavigate" />
    </KpiCard>
  </div>
</template>

<style scoped>
.salesman-attention-card__wrapper {
  display: block;
  height: 100%;
  text-decoration: none;
  color: inherit;
}

.salesman-attention-card__wrapper--link {
  cursor: pointer;
}

.salesman-attention-card__wrapper--link:hover .salesman-attention-card {
  box-shadow: 0 2px 8px rgb(0 0 0 / 0.08);
}

.salesman-attention-card {
  height: 100%;
}

.salesman-attention-card--attention {
  border-left: 4px solid var(--p-orange-500);
}

.salesman-attention-card--unavailable {
  opacity: 0.7;
}

.salesman-attention-card__unavailable {
  color: var(--p-text-muted-color);
  font-size: 0.9rem;
}
</style>
