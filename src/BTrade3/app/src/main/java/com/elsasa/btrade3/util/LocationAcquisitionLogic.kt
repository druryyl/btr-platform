package com.elsasa.btrade3.util

import android.location.Location

object LocationAcquisitionLogic {

    fun shouldAcceptLocation(
        location: Location,
        targetAccuracyMeters: Float,
        maxLocationAgeMs: Long,
        nowMs: Long = System.currentTimeMillis()
    ): Boolean {
        if (!location.hasAccuracy()) return false
        val ageMs = nowMs - location.time
        return shouldAcceptFix(
            accuracyMeters = location.accuracy,
            locationAgeMs = ageMs,
            targetAccuracyMeters = targetAccuracyMeters,
            maxLocationAgeMs = maxLocationAgeMs
        )
    }

    fun shouldAcceptFix(
        accuracyMeters: Float,
        locationAgeMs: Long,
        targetAccuracyMeters: Float,
        maxLocationAgeMs: Long
    ): Boolean {
        if (accuracyMeters > targetAccuracyMeters) return false
        return locationAgeMs in 0..maxLocationAgeMs
    }

    fun pickBetterLocation(current: Location?, candidate: Location): Location {
        if (current == null) return candidate
        if (!candidate.hasAccuracy()) return current
        if (!current.hasAccuracy()) return candidate
        return if (candidate.accuracy < current.accuracy) candidate else current
    }

    fun pickBetterAccuracy(currentAccuracy: Float?, candidateAccuracy: Float): Float {
        if (currentAccuracy == null) return candidateAccuracy
        return if (candidateAccuracy < currentAccuracy) candidateAccuracy else currentAccuracy
    }
}
