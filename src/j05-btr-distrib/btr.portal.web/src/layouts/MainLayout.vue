<script setup lang="ts">
import { onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Button from 'primevue/button'
import { useAuthStore } from '@/stores/authStore'
import { usePresentationStore } from '@/stores/presentationStore'

const auth = useAuthStore()
const presentation = usePresentationStore()

onMounted(() => {
  void presentation.load()
})
const router = useRouter()
const route = useRoute()

type NavItem = {
  label: string
  icon: string
  routeName: string
}

type NavSection = {
  label: string
  items: NavItem[]
}

const navSections: NavSection[] = [
  {
    label: 'Dashboard',
    items: [
      { label: 'Executive', icon: 'pi pi-th-large', routeName: 'dashboard' },
      { label: 'Alert Center', icon: 'pi pi-bell', routeName: 'alert-center' },
      { label: 'Sales', icon: 'pi pi-chart-line', routeName: 'sales-dashboard' },
      { label: 'Sales Forecast', icon: 'pi pi-chart-bar', routeName: 'sales-forecast-dashboard' },
      { label: 'Piutang', icon: 'pi pi-wallet', routeName: 'piutang-dashboard' },
      { label: 'Customers', icon: 'pi pi-users', routeName: 'customers-dashboard' },
      { label: 'Salesmen', icon: 'pi pi-id-card', routeName: 'salesmen-dashboard' },
      { label: 'Field Activity', icon: 'pi pi-map', routeName: 'field-activity-dashboard' },
      { label: 'Collection', icon: 'pi pi-money-bill', routeName: 'collection-dashboard' },
      { label: 'Inventory', icon: 'pi pi-box', routeName: 'inventory-dashboard' },
      { label: 'Inventory Risk', icon: 'pi pi-exclamation-triangle', routeName: 'inventory-risk-dashboard' },
      { label: 'Purchasing', icon: 'pi pi-shopping-cart', routeName: 'purchasing-dashboard' },
      { label: 'Locations', icon: 'pi pi-map-marker', routeName: 'locations-dashboard' },
    ],
  },
  {
    label: 'Reports',
    items: [
      { label: 'Sales Report', icon: 'pi pi-list', routeName: 'sales-report' },
      { label: 'Piutang Report', icon: 'pi pi-wallet', routeName: 'piutang-report' },
      { label: 'Inventory Report', icon: 'pi pi-box', routeName: 'inventory-report' },
      { label: 'Purchasing Report', icon: 'pi pi-shopping-cart', routeName: 'purchasing-report' },
    ],
  },
]

function isActive(routeName: string): boolean {
  return route.name === routeName
}

function logout(): void {
  auth.logout()
  router.push({ name: 'login' })
}
</script>

<template>
  <div class="layout">
    <header class="layout__header">
      <div class="layout__brand">
        <i class="pi pi-building layout__brand-icon" />
        <div>
          <div class="layout__brand-title">BTR Portal</div>
          <div class="layout__brand-subtitle">Distributor Management</div>
        </div>
      </div>

      <div
        v-if="presentation.isPresentationActive"
        class="layout__presentation"
        role="status"
        aria-live="polite"
      >
        <div class="layout__presentation-title">Presentation Mode</div>
        <div class="layout__presentation-date">
          Business Date: {{ presentation.formattedBusinessDate }}
        </div>
      </div>

      <div class="layout__user">
        <div class="layout__user-info">
          <span class="layout__user-name">{{ auth.user?.UserName ?? auth.user?.UserId }}</span>
          <span class="layout__user-role">{{ auth.user?.RoleName }}</span>
        </div>
        <Button
          label="Logout"
          icon="pi pi-sign-out"
          severity="secondary"
          outlined
          @click="logout"
        />
      </div>
    </header>

    <div class="layout__body">
      <aside class="layout__sidebar">
        <nav class="layout__nav" aria-label="Main navigation">
          <section
            v-for="section in navSections"
            :key="section.label"
            class="layout__nav-section"
          >
            <h2 class="layout__nav-heading">{{ section.label }}</h2>
            <ul class="layout__nav-list">
              <li v-for="item in section.items" :key="item.routeName">
                <RouterLink
                  :to="{ name: item.routeName }"
                  class="layout__nav-link"
                  :class="{ 'layout__nav-link--active': isActive(item.routeName) }"
                >
                  <i :class="['layout__nav-icon', item.icon]" aria-hidden="true" />
                  <span>{{ item.label }}</span>
                </RouterLink>
              </li>
            </ul>
          </section>
        </nav>
      </aside>

      <main class="layout__content">
        <RouterView v-if="presentation.loaded" />
      </main>
    </div>
  </div>
</template>

<style scoped>
.layout {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  background: var(--p-surface-50);
}

.layout__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  padding: 1rem 1.5rem;
  background: var(--p-surface-0);
  border-bottom: 1px solid var(--p-surface-200);
}

.layout__brand {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.layout__brand-icon {
  font-size: 1.75rem;
  color: var(--p-primary-color);
}

.layout__brand-title {
  font-size: 1.25rem;
  font-weight: 700;
  color: var(--p-text-color);
}

.layout__brand-subtitle {
  font-size: 0.85rem;
  color: var(--p-text-muted-color);
}

.layout__presentation {
  margin-left: auto;
  padding: 0.375rem 0.75rem;
  border: 1px solid var(--p-primary-200);
  border-radius: var(--p-content-border-radius);
  background: var(--p-surface-100);
  text-align: right;
}

.layout__presentation-title {
  font-size: 0.75rem;
  font-weight: 700;
  letter-spacing: 0.03em;
  text-transform: uppercase;
  color: var(--p-primary-700);
}

.layout__presentation-date {
  font-size: 0.85rem;
  color: var(--p-text-muted-color);
}

.layout__user {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.layout__user-info {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 0.125rem;
}

.layout__user-name {
  font-weight: 600;
}

.layout__user-role {
  font-size: 0.85rem;
  color: var(--p-text-muted-color);
}

.layout__body {
  display: flex;
  flex: 1;
  min-height: 0;
}

.layout__sidebar {
  width: 240px;
  flex-shrink: 0;
  padding: 1rem;
  background: var(--p-surface-0);
  border-right: 1px solid var(--p-surface-200);
}

.layout__nav-section + .layout__nav-section {
  margin-top: 1rem;
}

.layout__nav-heading {
  margin: 0 0 0.5rem;
  padding: 0 0.75rem;
  font-size: 0.75rem;
  font-weight: 700;
  letter-spacing: 0.04em;
  text-transform: uppercase;
  color: var(--p-text-muted-color);
}

.layout__nav-list {
  margin: 0;
  padding: 0;
  list-style: none;
}

.layout__nav-link {
  display: flex;
  align-items: center;
  gap: 0.625rem;
  padding: 0.625rem 0.75rem;
  border-radius: var(--p-content-border-radius);
  color: var(--p-text-color);
  text-decoration: none;
  transition: background-color 0.15s ease, color 0.15s ease;
}

.layout__nav-link:hover {
  background: var(--p-surface-100);
}

.layout__nav-link--active {
  background: var(--p-primary-50);
  color: var(--p-primary-700);
}

.layout__nav-icon {
  width: 1rem;
  text-align: center;
}

.layout__content {
  flex: 1;
  padding: 1.5rem;
  overflow: auto;
}

@media (max-width: 768px) {
  .layout__header {
    flex-direction: column;
    align-items: flex-start;
  }

  .layout__user {
    width: 100%;
    justify-content: space-between;
  }

  .layout__body {
    flex-direction: column;
  }

  .layout__sidebar {
    width: 100%;
    border-right: none;
    border-bottom: 1px solid var(--p-surface-200);
  }
}
</style>
