package com.elsasa.bgud.datastore

import org.junit.Assert.assertFalse
import org.junit.Assert.assertTrue
import org.junit.Test

/**
 * IR-09 session binding rule: a queued record is submittable only while the
 * session is bound to the same `locationId` under which it was captured.
 * Queued records are never re-homed.
 */
class SessionBindingTest {

    private val binding = SessionBinding(userId = "U-1", locationId = "GAMPING")

    @Test
    fun canSubmit_whenQueuedLocationMatchesTheSession() {
        assertTrue(binding.canSubmit("GAMPING"))
    }

    @Test
    fun cannotSubmit_whenQueuedLocationDiffersFromTheSession() {
        // GAMPING and CONCAT both resolve to ServerId JOGJA, but they are
        // distinct locations and must never be treated as interchangeable.
        assertFalse(binding.canSubmit("CONCAT"))
        assertFalse(binding.canSubmit("MAGELANG"))
    }

    @Test
    fun cannotSubmit_whenEitherLocationIsBlank() {
        assertFalse(binding.canSubmit(""))
        assertFalse(binding.canSubmit("   "))
        assertFalse(
            SessionBinding(userId = "U-1", locationId = "").canSubmit("GAMPING")
        )
    }
}
