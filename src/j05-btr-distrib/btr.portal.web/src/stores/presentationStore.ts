import { defineStore } from 'pinia'

import { computed, ref } from 'vue'

import { fetchPresentationConfig } from '@/api/presentationApi'

import { formatDate } from '@/services/formatters'

import { businessDateFromIso } from '@/services/reportFilterDefaults'



export const usePresentationStore = defineStore('presentation', () => {

  const enabled = ref(false)

  const businessDate = ref<string | null>(null)

  const loaded = ref(false)

  let loadPromise: Promise<void> | null = null



  const hidePlatformDiagnostics = computed(() => enabled.value)



  const isPresentationActive = computed(() => enabled.value)



  const businessReferenceDate = computed(() => {

    if (businessDate.value) {

      return businessDateFromIso(businessDate.value)

    }



    return startOfDay(new Date())

  })



  const formattedBusinessDate = computed(() => {

    if (!businessDate.value) {

      return ''

    }



    return formatDate(businessDate.value)

  })



  async function load(): Promise<void> {

    if (loaded.value) {

      return

    }



    if (loadPromise) {

      return loadPromise

    }



    loadPromise = (async () => {

      try {

        const config = await fetchPresentationConfig()

        enabled.value = config.enabled

        businessDate.value = config.businessDate

      } catch {

        enabled.value = false

        businessDate.value = null

      } finally {

        loaded.value = true

        loadPromise = null

      }

    })()



    return loadPromise

  }



  return {

    enabled,

    businessDate,

    loaded,

    hidePlatformDiagnostics,

    isPresentationActive,

    businessReferenceDate,

    formattedBusinessDate,

    load,

  }

})



function startOfDay(date: Date): Date {

  const copy = new Date(date)

  copy.setHours(0, 0, 0, 0)

  return copy

}

