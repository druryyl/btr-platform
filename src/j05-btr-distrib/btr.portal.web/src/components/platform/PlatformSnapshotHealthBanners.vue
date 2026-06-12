<script setup lang="ts">
import Message from 'primevue/message'
import { computed } from 'vue'
import { usePresentationStore } from '@/stores/presentationStore'

const props = defineProps<{
  isDataFresh?: boolean
  overallHealthStatus?: string | null
  showWhenFresh?: boolean
}>()

const presentation = usePresentationStore()

const showBanners = computed(() => !presentation.hidePlatformDiagnostics)

const showStaleBanner = computed(
  () => showBanners.value && props.showWhenFresh !== false && props.isDataFresh === false,
)

const showDegradedBanner = computed(
  () => showBanners.value && props.overallHealthStatus === 'degraded',
)

const showRefreshingBanner = computed(
  () => showBanners.value && props.overallHealthStatus === 'refreshing',
)
</script>

<template>
  <Message
    v-if="showStaleBanner"
    severity="warn"
    :closable="false"
    class="platform-health-banners__banner"
  >
    ⚠ Dashboard Data Not Fresh
  </Message>

  <Message
    v-if="showDegradedBanner"
    severity="error"
    :closable="false"
    class="platform-health-banners__banner"
  >
    Dashboard snapshot refresh is degraded. Some analytics may be outdated.
  </Message>

  <Message
    v-if="showRefreshingBanner"
    severity="info"
    :closable="false"
    class="platform-health-banners__banner"
  >
    Dashboard snapshots are currently refreshing.
  </Message>
</template>

<style scoped>
.platform-health-banners__banner {
  margin-bottom: 1rem;
}
</style>
