<script setup lang="ts">
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Button from 'primevue/button'
import Menu from 'primevue/menu'
import { useAuthStore } from '@/stores/authStore'

const auth = useAuthStore()
const router = useRouter()
const route = useRoute()

const menuItems = computed(() => [
  {
    label: 'Dashboard',
    icon: 'pi pi-home',
    items: [
      {
        label: 'Executive',
        icon: 'pi pi-th-large',
        command: () => router.push('/dashboard'),
        class: route.path === '/dashboard' ? 'layout-menu-item--active' : '',
      },
      {
        label: 'Sales',
        icon: 'pi pi-chart-line',
        command: () => router.push('/dashboard/sales'),
        class: route.path === '/dashboard/sales' ? 'layout-menu-item--active' : '',
      },
      {
        label: 'Piutang',
        icon: 'pi pi-wallet',
        command: () => router.push('/dashboard/piutang'),
        class: route.path === '/dashboard/piutang' ? 'layout-menu-item--active' : '',
      },
      {
        label: 'Customers',
        icon: 'pi pi-users',
        command: () => router.push('/dashboard/customers'),
        class: route.path === '/dashboard/customers' ? 'layout-menu-item--active' : '',
      },
      {
        label: 'Salesmen',
        icon: 'pi pi-id-card',
        command: () => router.push('/dashboard/salesmen'),
        class: route.path === '/dashboard/salesmen' ? 'layout-menu-item--active' : '',
      },
      {
        label: 'Inventory',
        icon: 'pi pi-box',
        command: () => router.push('/dashboard/inventory'),
        class: route.path === '/dashboard/inventory' ? 'layout-menu-item--active' : '',
      },
      {
        label: 'Purchasing',
        icon: 'pi pi-shopping-cart',
        command: () => router.push('/dashboard/purchasing'),
        class: route.path === '/dashboard/purchasing' ? 'layout-menu-item--active' : '',
      },
    ],
  },
  {
    label: 'Reports',
    icon: 'pi pi-file',
    items: [
      {
        label: 'Sales Report',
        icon: 'pi pi-list',
        command: () => router.push('/reports/sales'),
        class: route.path === '/reports/sales' ? 'layout-menu-item--active' : '',
      },
      {
        label: 'Piutang Report',
        icon: 'pi pi-wallet',
        command: () => router.push('/reports/piutang'),
        class: route.path === '/reports/piutang' ? 'layout-menu-item--active' : '',
      },
      {
        label: 'Inventory Report',
        icon: 'pi pi-box',
        command: () => router.push('/reports/inventory'),
        class: route.path === '/reports/inventory' ? 'layout-menu-item--active' : '',
      },
      {
        label: 'Purchasing Report',
        icon: 'pi pi-shopping-cart',
        command: () => router.push('/reports/purchasing'),
        class: route.path === '/reports/purchasing' ? 'layout-menu-item--active' : '',
      },
    ],
  },
])

function logout(): void {
  auth.logout()
  router.push('/login')
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
        <Menu :model="menuItems" class="layout__menu" />
      </aside>

      <main class="layout__content">
        <RouterView />
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

<style>
.layout-menu-item--active > .p-menuitem-content {
  background: var(--p-primary-50);
  color: var(--p-primary-700);
}
</style>
