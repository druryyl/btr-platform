<script setup lang="ts">
import { RouterLink } from 'vue-router'
import type { ProfileRelationshipBlock } from '@/models/entityAnalytics'

defineProps<{
  blocks: ProfileRelationshipBlock[]
}>()

function formatMetric(value: number | null | undefined): string {
  if (value == null)
    return '—'

  return new Intl.NumberFormat('id-ID', {
    style: 'currency',
    currency: 'IDR',
    maximumFractionDigits: 0,
  }).format(value)
}
</script>

<template>
  <div class="related-entities-blocks">
    <section
      v-for="block in blocks"
      :key="block.RelationshipCode"
      class="related-entities-blocks__block"
    >
      <h3 class="related-entities-blocks__heading">
        {{ block.RelationshipLabel || block.DisplayName }}
      </h3>

      <div class="related-entities-blocks__table-wrap">
        <table class="related-entities-blocks__table">
          <thead>
            <tr>
              <th v-if="block.Rows.length > 1">#</th>
              <th>{{ block.TargetEntityType }}</th>
              <th v-if="block.Rows.some((row) => row.MetricValue != null)">MTD Omzet</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="row in block.Rows"
              :key="`${block.RelationshipCode}-${row.Rank}-${row.TargetEntityCode}`"
            >
              <td v-if="block.Rows.length > 1">{{ row.Rank }}</td>
              <td>
                <RouterLink
                  v-if="row.ProfileRoute"
                  :to="row.ProfileRoute"
                  class="related-entities-blocks__link"
                >
                  {{ row.TargetEntityName || row.DisplayName || row.TargetEntityCode }}
                </RouterLink>
                <span v-else>{{ row.TargetEntityName || row.DisplayName || row.TargetEntityCode }}</span>
              </td>
              <td v-if="block.Rows.some((r) => r.MetricValue != null)">
                {{ formatMetric(row.MetricValue) }}
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>
  </div>
</template>

<style scoped>
.related-entities-blocks {
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
}

.related-entities-blocks__heading {
  margin: 0 0 0.75rem;
  font-size: 0.95rem;
  font-weight: 600;
}

.related-entities-blocks__table-wrap {
  overflow-x: auto;
}

.related-entities-blocks__table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.875rem;
}

.related-entities-blocks__table th,
.related-entities-blocks__table td {
  padding: 0.5rem 0.75rem;
  border-bottom: 1px solid var(--p-content-border-color);
  text-align: left;
}

.related-entities-blocks__table th {
  color: var(--p-text-muted-color);
  font-weight: 600;
}

.related-entities-blocks__link {
  color: var(--p-primary-color);
  text-decoration: none;
}

.related-entities-blocks__link:hover {
  text-decoration: underline;
}
</style>
