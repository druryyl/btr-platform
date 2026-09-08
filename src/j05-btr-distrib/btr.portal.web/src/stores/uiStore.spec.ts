import { createPinia, setActivePinia } from 'pinia'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { useUiStore } from '@/stores/uiStore'

function createLocalStorageMock(initial: Record<string, string> = {}) {
  const data = new Map(Object.entries(initial))
  return {
    getItem: vi.fn((key: string) => data.get(key) ?? null),
    setItem: vi.fn((key: string, value: string) => {
      data.set(key, value)
    }),
  }
}

describe('uiStore sidebar collapse', () => {
  beforeEach(() => {
    vi.unstubAllGlobals()
    const localStorage = createLocalStorageMock()
    vi.stubGlobal('window', { localStorage })
    setActivePinia(createPinia())
  })

  it('defaults to expanded when nothing is stored', () => {
    const store = useUiStore()
    expect(store.sidebarCollapsed).toBe(false)
    expect(store.isSidebarCollapsed).toBe(false)
  })

  it('reads an existing collapsed preference from localStorage', () => {
    vi.stubGlobal('window', { localStorage: createLocalStorageMock({ 'btr.portal.sidebar-collapsed': 'true' }) })
    const store = useUiStore()
    expect(store.sidebarCollapsed).toBe(true)
  })

  it('toggles the collapsed state and persists it', () => {
    const localStorage = createLocalStorageMock()
    vi.stubGlobal('window', { localStorage })
    const store = useUiStore()

    store.toggleSidebar()

    expect(store.sidebarCollapsed).toBe(true)
    expect(localStorage.setItem).toHaveBeenCalledWith('btr.portal.sidebar-collapsed', 'true')

    store.toggleSidebar()

    expect(store.sidebarCollapsed).toBe(false)
    expect(localStorage.setItem).toHaveBeenCalledWith('btr.portal.sidebar-collapsed', 'false')
  })

  it('sets the collapsed state directly', () => {
    const store = useUiStore()

    store.setSidebarCollapsed(true)

    expect(store.sidebarCollapsed).toBe(true)
  })

  it('falls back to expanded when localStorage is unavailable', () => {
    vi.stubGlobal('window', {} as never)
    const store = useUiStore()
    expect(store.sidebarCollapsed).toBe(false)

    expect(() => store.toggleSidebar()).not.toThrow()
    expect(store.sidebarCollapsed).toBe(true)
  })
})