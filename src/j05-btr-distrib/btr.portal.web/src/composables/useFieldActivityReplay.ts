import { onUnmounted, ref, watch, type Ref } from 'vue'
import type { FieldActivityActualStop } from '@/models/fieldActivity'

const MIN_SPEED = 0.5
const MAX_SPEED = 4
const BASE_SEGMENT_MS = 900

export function useFieldActivityReplay(stops: Ref<FieldActivityActualStop[]>) {
  const currentIndex = ref(-1)
  const isPlaying = ref(false)
  const speed = ref(1)
  let timer: ReturnType<typeof setTimeout> | null = null

  function clearTimer(): void {
    if (timer != null) {
      clearTimeout(timer)
      timer = null
    }
  }

  function reset(): void {
    clearTimer()
    isPlaying.value = false
    currentIndex.value = stops.value.length > 0 ? 0 : -1
  }

  function pause(): void {
    clearTimer()
    isPlaying.value = false
  }

  function play(): void {
    if (stops.value.length === 0) return

    if (currentIndex.value < 0) {
      currentIndex.value = 0
    }

    if (currentIndex.value >= stops.value.length - 1) {
      currentIndex.value = 0
    }

    isPlaying.value = true
    scheduleNext()
  }

  function scheduleNext(): void {
    clearTimer()

    if (!isPlaying.value) return

    const delay = BASE_SEGMENT_MS / speed.value
    timer = setTimeout(() => {
      if (currentIndex.value >= stops.value.length - 1) {
        pause()
        return
      }

      currentIndex.value += 1
      scheduleNext()
    }, delay)
  }

  function setSpeed(value: number): void {
    speed.value = Math.min(MAX_SPEED, Math.max(MIN_SPEED, value))
    if (isPlaying.value) {
      scheduleNext()
    }
  }

  function selectIndex(index: number): void {
    pause()
    currentIndex.value = index
  }

  watch(stops, () => {
    reset()
  })

  onUnmounted(() => {
    clearTimer()
  })

  return {
    currentIndex,
    isPlaying,
    speed,
    minSpeed: MIN_SPEED,
    maxSpeed: MAX_SPEED,
    play,
    pause,
    reset,
    setSpeed,
    selectIndex,
  }
}

export function getReplaySegmentDurationMs(speed: number): number {
  return BASE_SEGMENT_MS / speed
}
