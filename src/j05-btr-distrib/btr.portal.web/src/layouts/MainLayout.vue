<script setup lang="ts">

import { onMounted } from 'vue'

import { useRoute, useRouter } from 'vue-router'

import Button from 'primevue/button'

import { useAuthStore } from '@/stores/authStore'

import { usePresentationStore } from '@/stores/presentationStore'

import { useUiStore } from '@/stores/uiStore'

import PortalMenuLabel from '@/components/navigation/PortalMenuLabel.vue'

import { portalMenuGroups } from '@/navigation/portalMenuRegistry'



const auth = useAuthStore()

const ui = useUiStore()

const presentation = usePresentationStore()



onMounted(() => {

  void presentation.load()

})

const router = useRouter()

const route = useRoute()



function isActive(routeName: string): boolean {

  if (routeName === 'entity-analytics-home') {

    return route.path.startsWith('/analytics')

  }

  if (routeName === 'principal-performance-dashboard') {
    return route.path.startsWith('/dashboard/principal-performance')
  }

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

      <button

        v-tooltip.bottom="ui.isSidebarCollapsed ? 'Expand menu' : 'Collapse menu'"

        type="button"

        class="layout__collapse-toggle"

        :aria-expanded="!ui.isSidebarCollapsed"

        aria-controls="portal-nav"

        aria-label="Toggle navigation menu"

        @click="ui.toggleSidebar()"

      >

        <i class="pi pi-bars" aria-hidden="true" />

      </button>



      <div class="layout__brand">

        <i class="pi pi-building layout__brand-icon" />

        <div>

          <div class="layout__brand-title">BTR Portal V2</div>

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

          class="layout__logout"

          @click="logout"

        />

      </div>

    </header>



    <div class="layout__body">

      <aside

        id="portal-nav"

        class="layout__sidebar"

        :class="{ 'layout__sidebar--collapsed': ui.isSidebarCollapsed }"

        aria-label="Main navigation"

      >

        <nav class="layout__nav" aria-label="Main navigation">

          <section

            v-for="group in portalMenuGroups"

            :key="group.id"

            class="layout__nav-section"

          >

            <h2 class="layout__nav-heading">{{ group.label }}</h2>

            <ul class="layout__nav-list">

              <li v-for="item in group.items" :key="item.routeName">

                <RouterLink

                  :to="{ name: item.routeName }"

                  v-tooltip="ui.isSidebarCollapsed ? item.label : null"

                  class="layout__nav-link"

                  :class="{ 'layout__nav-link--active': isActive(item.routeName) }"

                >

                  <i :class="['layout__nav-icon', item.icon]" aria-hidden="true" />

                  <PortalMenuLabel :code="item.code" :label="item.label" />

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

  background: var(--portal-canvas);

}



.layout__header {

  display: flex;

  align-items: center;

  justify-content: space-between;

  gap: 1rem;

  padding: 1rem 1.5rem;

  background: var(--portal-header-bg);

  color: var(--portal-nav-text);

  border-bottom: 1px solid var(--portal-nav-border);

}



.layout__collapse-toggle {

  display: inline-flex;

  align-items: center;

  justify-content: center;

  width: 2.5rem;

  height: 2.5rem;

  border: 1px solid transparent;

  border-radius: var(--p-content-border-radius);

  background: transparent;

  color: var(--portal-nav-text);

  cursor: pointer;

  flex-shrink: 0;

  transition: background-color 0.15s ease, color 0.15s ease;

}

.layout__collapse-toggle:hover {

  background: var(--portal-nav-bg-elevated);

  color: var(--portal-nav-text);

}

.layout__collapse-toggle:focus-visible {

  outline: 2px solid var(--portal-nav-active-accent);

  outline-offset: 2px;

}



.layout__brand {

  display: flex;

  align-items: center;

  gap: 0.75rem;

}



.layout__brand-icon {

  font-size: 1.75rem;

  color: var(--portal-nav-brand);

}



.layout__brand-title {

  font-size: 1.25rem;

  font-weight: 700;

  color: var(--portal-nav-text);

}



.layout__brand-subtitle {

  font-size: 0.85rem;

  color: var(--portal-nav-muted);

}



.layout__presentation {

  margin-left: auto;

  padding: 0.375rem 0.75rem;

  border: 1px solid var(--portal-nav-border);

  border-radius: var(--p-content-border-radius);

  background: var(--portal-nav-bg-elevated);

  text-align: right;

}



.layout__presentation-title {

  font-size: 0.75rem;

  font-weight: 700;

  letter-spacing: 0.03em;

  text-transform: uppercase;

  color: var(--portal-nav-active-accent);

}



.layout__presentation-date {

  font-size: 0.85rem;

  color: var(--portal-nav-muted);

}



.layout__user {

  display: flex;

  align-items: center;

  gap: 1rem;

}



.layout__logout.layout__logout {

  color: var(--portal-nav-text);

  background: transparent;

  border-color: var(--portal-nav-muted);

}

.layout__logout.layout__logout:hover {

  color: var(--portal-nav-text);

  background: var(--portal-nav-bg-elevated);

  border-color: var(--portal-nav-text);

}



.layout__user-info {

  display: flex;

  flex-direction: column;

  align-items: flex-end;

  gap: 0.125rem;

}



.layout__user-name {

  font-weight: 600;

  color: var(--portal-nav-text);

}



.layout__user-role {

  font-size: 0.85rem;

  color: var(--portal-nav-muted);

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

  overflow-x: hidden;

  overflow-y: auto;

  background: var(--portal-nav-bg);

  border-right: 1px solid var(--portal-nav-border);

  transition: width 0.2s ease, padding 0.2s ease;

}

.layout__sidebar--collapsed {

  width: 64px;

  padding: 1rem 0.5rem;

}

.layout__sidebar--collapsed .layout__nav-heading {

  display: none;

}

.layout__sidebar--collapsed :deep(.portal-menu-label) {

  display: none;

}

.layout__sidebar--collapsed .layout__nav-link {

  justify-content: center;

  gap: 0;

  padding: 0.625rem 0;

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

  color: var(--portal-nav-muted);

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

  color: var(--portal-nav-text);

  text-decoration: none;

  transition: background-color 0.15s ease, color 0.15s ease;

}



.layout__nav-link:hover {

  background: var(--portal-nav-bg-elevated);

}



.layout__nav-link--active {

  background: var(--portal-nav-bg-active);

  color: var(--portal-nav-text-active);

  box-shadow: inset 3px 0 0 var(--portal-nav-active-accent);

}

.layout__nav-link--active:hover {

  background: var(--portal-nav-bg-active);

  color: var(--portal-nav-text-active);

}



.layout__nav-link--active :deep(.portal-menu-label__code) {

  color: var(--portal-nav-text-active);

}

.layout__nav-link--active :deep(.portal-menu-label__separator) {

  color: var(--portal-nav-text-active);

}

.layout__nav-link :deep(.portal-menu-label__code) {

  color: var(--portal-nav-muted);

}

.layout__nav-link :deep(.portal-menu-label__separator) {

  color: var(--portal-nav-muted);

}



.layout__nav-icon {

  width: 1rem;

  text-align: center;

  flex-shrink: 0;

  color: var(--portal-nav-icon);

}

.layout__nav-link--active .layout__nav-icon {

  color: var(--portal-nav-text-active);

}



.layout__content {

  flex: 1;

  padding: 1.5rem;

  overflow: auto;

}



@media (max-width: 768px) {

  .layout__collapse-toggle {

    display: none;

  }



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



  .layout__sidebar,
  .layout__sidebar--collapsed {

    width: 100%;

    padding: 1rem;

    border-right: none;

    border-bottom: 1px solid var(--portal-nav-border);

  }



  .layout__sidebar--collapsed .layout__nav-heading {

    display: block;

  }



  .layout__sidebar--collapsed :deep(.portal-menu-label) {

    display: inline-flex;

  }



  .layout__sidebar--collapsed .layout__nav-link {

    justify-content: flex-start;

    gap: 0.625rem;

    padding: 0.625rem 0.75rem;

  }

}

</style>

