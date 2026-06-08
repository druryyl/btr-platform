<script setup lang="ts">
import { RouterLink } from 'vue-router'
import KpiCard from '@/components/KpiCard.vue'

defineProps<{
  title: string
  icon: string
  loading?: boolean
  requiresAttention?: boolean
  unavailable?: boolean
  /** Vue Router path (plan §7.5 domain dashboard links). */
  to?: string
  /** In-page anchor, e.g. #customer-attention-list for Credit card. */
  href?: string
}>()
</script>

<template>
  <RouterLink
    v-if="to && !unavailable"
    :to="to"
    class="customer-attention-card__wrapper customer-attention-card__wrapper--link"
  >
    <KpiCard
      :title="title"
      :icon="icon"
      :loading="loading"
      class="customer-attention-card"
      :class="{
        'customer-attention-card--attention': requiresAttention,
        'customer-attention-card--unavailable': unavailable,
      }"
    >
      <slot />
    </KpiCard>
  </RouterLink>
  <a
    v-else-if="href && !unavailable"
    :href="href"
    class="customer-attention-card__wrapper customer-attention-card__wrapper--link"
  >
    <KpiCard
      :title="title"
      :icon="icon"
      :loading="loading"
      class="customer-attention-card"
      :class="{
        'customer-attention-card--attention': requiresAttention,
        'customer-attention-card--unavailable': unavailable,
      }"
    >
      <slot />
    </KpiCard>
  </a>
  <div v-else class="customer-attention-card__wrapper">
    <KpiCard
      :title="title"
      :icon="icon"
      :loading="loading"
      class="customer-attention-card"
      :class="{
        'customer-attention-card--attention': requiresAttention,
        'customer-attention-card--unavailable': unavailable,
      }"
    >
      <div v-if="unavailable" class="customer-attention-card__unavailable">
        Data unavailable
      </div>
      <slot v-else />
    </KpiCard>
  </div>
</template>

<style scoped>
.customer-attention-card__wrapper {
  display: block;
  height: 100%;
  text-decoration: none;
  color: inherit;
}

.customer-attention-card__wrapper--link {
  cursor: pointer;
}

.customer-attention-card__wrapper--link:hover .customer-attention-card {
  box-shadow: 0 2px 8px rgb(0 0 0 / 0.08);
}

.customer-attention-card {
  height: 100%;
}

.customer-attention-card--attention {
  border-left: 4px solid var(--p-orange-500);
}

.customer-attention-card--unavailable {
  opacity: 0.7;
}

.customer-attention-card__unavailable {
  color: var(--p-text-muted-color);
  font-size: 0.9rem;
}
</style>
