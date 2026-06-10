<script setup lang="ts">
import { RouterLink } from 'vue-router'
import KpiCard from '@/components/KpiCard.vue'

defineProps<{
  title: string
  icon: string
  loading?: boolean
  requiresAttention?: boolean
  unavailable?: boolean
  /** Vue Router path for cross-dashboard links. */
  to?: string
  /** In-page anchor, e.g. #purchasing-attention-list. */
  href?: string
}>()

const emit = defineEmits<{
  anchorNavigate: []
}>()
</script>

<template>
  <RouterLink
    v-if="to && !unavailable"
    :to="to"
    class="purchasing-attention-card__wrapper purchasing-attention-card__wrapper--link"
  >
    <KpiCard
      :title="title"
      :icon="icon"
      :loading="loading"
      class="purchasing-attention-card"
      :class="{
        'purchasing-attention-card--attention': requiresAttention,
        'purchasing-attention-card--unavailable': unavailable,
      }"
    >
      <slot />
    </KpiCard>
  </RouterLink>
  <a
    v-else-if="href && !unavailable"
    :href="href"
    class="purchasing-attention-card__wrapper purchasing-attention-card__wrapper--link"
    @click="emit('anchorNavigate')"
  >
    <KpiCard
      :title="title"
      :icon="icon"
      :loading="loading"
      class="purchasing-attention-card"
      :class="{
        'purchasing-attention-card--attention': requiresAttention,
        'purchasing-attention-card--unavailable': unavailable,
      }"
    >
      <slot />
    </KpiCard>
  </a>
  <div v-else class="purchasing-attention-card__wrapper">
    <KpiCard
      :title="title"
      :icon="icon"
      :loading="loading"
      class="purchasing-attention-card"
      :class="{
        'purchasing-attention-card--attention': requiresAttention,
        'purchasing-attention-card--unavailable': unavailable,
      }"
    >
      <div v-if="unavailable" class="purchasing-attention-card__unavailable">
        Data unavailable
      </div>
      <slot v-else />
    </KpiCard>
  </div>
</template>

<style scoped>
.purchasing-attention-card__wrapper {
  display: block;
  height: 100%;
  text-decoration: none;
  color: inherit;
}

.purchasing-attention-card__wrapper--link {
  cursor: pointer;
}

.purchasing-attention-card__wrapper--link:hover .purchasing-attention-card {
  box-shadow: 0 2px 8px rgb(0 0 0 / 0.08);
}

.purchasing-attention-card {
  height: 100%;
}

.purchasing-attention-card--attention {
  border-left: 4px solid var(--p-orange-500);
}

.purchasing-attention-card--unavailable {
  opacity: 0.7;
}

.purchasing-attention-card__unavailable {
  color: var(--p-text-muted-color);
  font-size: 0.9rem;
}
</style>
