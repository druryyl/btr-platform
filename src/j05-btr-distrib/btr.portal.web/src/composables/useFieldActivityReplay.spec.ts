import { describe, expect, it } from 'vitest'
import { ref } from 'vue'
import { getReplaySegmentDurationMs, useFieldActivityReplay } from '@/composables/useFieldActivityReplay'
import type { FieldActivityActualStop } from '@/models/fieldActivity'

function createStop(sequence: number, time: string): FieldActivityActualStop {
  return {
    CustomerId: `C${sequence}`,
    CustomerCode: `C${sequence}`,
    CustomerName: `Customer ${sequence}`,
    Sequence: sequence,
    CheckInTime: time,
    Latitude: -6.2,
    Longitude: 106.8,
    HasCoordinates: true,
    VisitStatus: 'Visited',
    IsEffectiveCall: false,
    GpsValidation: 'Valid',
    DistanceMeters: 10,
  }
}

describe('useFieldActivityReplay', () => {
  it('reset selects first stop when data exists', () => {
    const stops = ref([createStop(1, '08:00:00'), createStop(2, '09:00:00')])
    const replay = useFieldActivityReplay(stops)

    replay.reset()

    expect(replay.currentIndex.value).toBe(0)
    expect(replay.isPlaying.value).toBe(false)
  })

  it('setSpeed scales segment duration', () => {
    expect(getReplaySegmentDurationMs(1)).toBe(900)
    expect(getReplaySegmentDurationMs(2)).toBe(450)
    expect(getReplaySegmentDurationMs(4)).toBe(225)
  })

  it('selectIndex pauses and moves current index', () => {
    const stops = ref([createStop(1, '08:00:00'), createStop(2, '09:00:00')])
    const replay = useFieldActivityReplay(stops)

    replay.play()
    replay.selectIndex(1)

    expect(replay.isPlaying.value).toBe(false)
    expect(replay.currentIndex.value).toBe(1)
  })
})
