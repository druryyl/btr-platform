package com.elsasa.btrade3.util

import org.junit.Assert.assertFalse
import org.junit.Assert.assertTrue
import org.junit.Test

class LocationAcquisitionLogicTest {

    @Test
    fun shouldAcceptFix_whenAccuracyAndAgeWithinThreshold() {
        assertTrue(
            LocationAcquisitionLogic.shouldAcceptFix(
                accuracyMeters = 20f,
                locationAgeMs = 2_000L,
                targetAccuracyMeters = 30f,
                maxLocationAgeMs = 5_000L
            )
        )
    }

    @Test
    fun shouldRejectFix_whenAccuracyTooLow() {
        assertFalse(
            LocationAcquisitionLogic.shouldAcceptFix(
                accuracyMeters = 80f,
                locationAgeMs = 1_000L,
                targetAccuracyMeters = 30f,
                maxLocationAgeMs = 5_000L
            )
        )
    }

    @Test
    fun shouldRejectFix_whenFixIsStale() {
        assertFalse(
            LocationAcquisitionLogic.shouldAcceptFix(
                accuracyMeters = 10f,
                locationAgeMs = 10_000L,
                targetAccuracyMeters = 30f,
                maxLocationAgeMs = 5_000L
            )
        )
    }

    @Test
    fun pickBetterAccuracy_prefersMoreAccurateFix() {
        val result = LocationAcquisitionLogic.pickBetterAccuracy(50f, 15f)
        assertTrue(result == 15f)
    }
}
