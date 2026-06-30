<script setup lang="ts">
defineProps<{
  title?: string
}>()
</script>

<template>
  <section class="iw-stage">
    <h2 v-if="title" class="iw-section-title">{{ title }}</h2>
    <slot />
  </section>
</template>
