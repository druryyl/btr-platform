<script setup lang="ts">

import { onMounted } from 'vue'

import { useRoute, useRouter } from 'vue-router'

import Button from 'primevue/button'

import { useAuthStore } from '@/stores/authStore'

import { usePresentationStore } from '@/stores/presentationStore'

import PortalMenuLabel from '@/components/navigation/PortalMenuLabel.vue'

import { portalMenuGroups } from '@/navigation/portalMenuRegistry'



const auth = useAuthStore()

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

            v-for="group in portalMenuGroups"

            :key="group.id"

            class="layout__nav-section"

          >

            <h2 class="layout__nav-heading">{{ group.label }}</h2>

            <ul class="layout__nav-list">

              <li v-for="item in group.items" :key="item.routeName">

                <RouterLink

                  :to="{ name: item.routeName }"

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

  overflow-y: auto;

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



.layout__nav-link--active :deep(.portal-menu-label__code) {

  color: var(--p-primary-700);

}



.layout__nav-icon {

  width: 1rem;

  text-align: center;

  flex-shrink: 0;

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

