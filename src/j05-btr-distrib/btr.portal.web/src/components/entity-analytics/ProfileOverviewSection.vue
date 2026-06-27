<script setup lang="ts">
import { computed, ref } from 'vue'
import Panel from 'primevue/panel'
import Tag from 'primevue/tag'
import ProfileSectionCard from '@/components/entity-analytics/ProfileSectionCard.vue'
import type { ProfileOverviewSection } from '@/models/entityAnalytics'
import { buildProfileOverviewLayout } from '@/services/profileOverviewLayout'
import { formatDateTime } from '@/services/formatters'

const props = defineProps<{
  section: ProfileOverviewSection | null | undefined
  loading?: boolean
}>()

const detailsCollapsed = ref(true)

const layout = computed(() =>
  props.section ? buildProfileOverviewLayout(props.section) : null,
)

function formatFieldValue(key: string, value: string): string {
  if (key === 'snapshotTime') {
    return formatDateTime(value)
  }

  return value
}
</script>

<template>
  <ProfileSectionCard
    title="Overview"
    :is-available="section?.IsAvailable !== false"
    :loading="loading"
  >
    <div v-if="layout" class="profile-overview">
      <header class="profile-overview__hero">
        <div class="profile-overview__hero-text">
          <h3 class="profile-overview__name">{{ layout.displayName }}</h3>
        </div>
        <Tag
          class="profile-overview__status"
          :value="layout.statusLabel"
          :severity="layout.statusSeverity"
        />
      </header>

      <div v-if="layout.sections.length" class="profile-overview__grid">
        <section
          v-for="group in layout.sections"
          :key="group.id"
          class="profile-overview__section"
        >
          <h4 class="profile-overview__section-title">{{ group.title }}</h4>
          <dl class="profile-overview__fields">
            <div
              v-for="field in group.fields"
              :key="field.key"
              class="profile-overview__field"
            >
              <dt>{{ field.label }}</dt>
              <dd>
                <Tag
                  v-if="field.isBadge"
                  :value="field.value"
                  :severity="field.badgeSeverity"
                />
                <span v-else>{{ formatFieldValue(field.key, field.value) }}</span>
              </dd>
            </div>
          </dl>
        </section>
      </div>

      <Panel
        v-if="layout.details.length"
        v-model:collapsed="detailsCollapsed"
        toggleable
        class="profile-overview__details"
      >
        <template #header>
          <span class="profile-overview__details-title">Details</span>
        </template>
        <dl class="profile-overview__fields profile-overview__fields--details">
          <div
            v-for="field in layout.details"
            :key="field.key"
            class="profile-overview__field"
          >
            <dt>{{ field.label }}</dt>
            <dd>{{ formatFieldValue(field.key, field.value) }}</dd>
          </div>
        </dl>
      </Panel>
    </div>
  </ProfileSectionCard>
</template>

<style scoped>
.profile-overview {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.profile-overview__hero {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 0.75rem;
  padding-bottom: 0.75rem;
  border-bottom: 1px solid var(--p-content-border-color, #e2e8f0);
}

.profile-overview__hero-text {
  min-width: 0;
}

.profile-overview__name {
  margin: 0;
  font-size: 1.125rem;
  font-weight: 600;
  line-height: 1.3;
}

.profile-overview__status {
  flex-shrink: 0;
}

.profile-overview__grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: 1rem 1.5rem;
}

@media (min-width: 48rem) {
  .profile-overview__grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

.profile-overview__section-title {
  margin: 0 0 0.5rem;
  font-size: 0.8125rem;
  font-weight: 600;
  letter-spacing: 0.02em;
  text-transform: uppercase;
  color: var(--p-text-muted-color, #64748b);
}

.profile-overview__fields {
  display: grid;
  gap: 0.5rem;
  margin: 0;
}

.profile-overview__field {
  display: grid;
  grid-template-columns: minmax(6.5rem, 42%) minmax(0, 1fr);
  gap: 0.5rem 0.75rem;
  align-items: start;
}

.profile-overview__field dt {
  margin: 0;
  color: var(--p-text-muted-color, #64748b);
  font-size: 0.875rem;
  font-weight: 500;
}

.profile-overview__field dd {
  margin: 0;
  font-size: 0.9375rem;
  line-height: 1.4;
  word-break: break-word;
}

.profile-overview__details :deep(.p-panel-header) {
  padding: 0.5rem 0.75rem;
}

.profile-overview__details :deep(.p-panel-content) {
  padding: 0 0.75rem 0.75rem;
}

.profile-overview__details-title {
  font-size: 0.8125rem;
  font-weight: 600;
  letter-spacing: 0.02em;
  text-transform: uppercase;
  color: var(--p-text-muted-color, #64748b);
}

.profile-overview__fields--details {
  padding-top: 0.25rem;
}
</style>
