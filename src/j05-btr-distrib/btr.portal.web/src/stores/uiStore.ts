import { defineStore } from 'pinia'
import { computed, ref } from 'vue'

const STORAGE_KEY = 'btr.portal.sidebar-collapsed'

function loadCollapsed(): boolean {
  try {
    return window.localStorage.getItem(STORAGE_KEY) === 'true'
  } catch {
    return false
  }
}

function persistCollapsed(value: boolean): void {
  try {
    window.localStorage.setItem(STORAGE_KEY, String(value))
  } catch {
    // localStorage unavailable (e.g. privacy mode) — collapse still applies for the session
  }
}

export const useUiStore = defineStore('ui', () => {
  const sidebarCollapsed = ref(loadCollapsed())

  const isSidebarCollapsed = computed(() => sidebarCollapsed.value)

  function setSidebarCollapsed(collapsed: boolean): void {
    sidebarCollapsed.value = collapsed
    persistCollapsed(collapsed)
  }

  function toggleSidebar(): void {
    setSidebarCollapsed(!sidebarCollapsed.value)
  }

  return {
    sidebarCollapsed,
    isSidebarCollapsed,
    setSidebarCollapsed,
    toggleSidebar,
  }
})