package com.elsasa.bgud.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.elsasa.bgud.datastore.SessionPreferencesDataSource
import kotlinx.coroutines.flow.SharingStarted
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.map
import kotlinx.coroutines.flow.stateIn
import kotlinx.coroutines.launch

/**
 * Settings view model (SCR-MOB-008).
 *
 * Settings is a navigation leaf. It exposes the local session for display and
 * the two session-closing actions:
 *
 * - `user` / `warehouse` / `office` re-expose the DataStore session
 *   (`user_id`, `location_id`, `server_id`; TD-10). `location_id` — never a
 *   `ServerId` — is the stored tenant binding (IR-09); `server_id` is the
 *   Cloud-resolved value kept for the legacy `{serverId}` read routes (TD-08).
 * - `logout()` and `changeWarehouse()` both clear the local session so the
 *   Navigation gate returns to `login` (TD-10/TD-11). Gudang change is a new
 *   session: the operator ends the current session and selects another Gudang
 *   (FEATURE §6.8). Room caches and queued records are intentionally
 *   untouched — they keep their original `locationId` binding and are never
 *   re-homed (IR-09).
 */
class SettingsViewModel(
    private val session: SessionPreferencesDataSource
) : ViewModel() {

    val user: StateFlow<String> = session.userId
        .map { it.orEmpty() }
        .stateIn(viewModelScope, SharingStarted.WhileSubscribed(5_000), "")

    val warehouse: StateFlow<String> = session.locationId
        .map { it.orEmpty() }
        .stateIn(viewModelScope, SharingStarted.WhileSubscribed(5_000), "")

    val office: StateFlow<String> = session.serverId
        .map { it.orEmpty() }
        .stateIn(viewModelScope, SharingStarted.WhileSubscribed(5_000), "")

    /**
     * Logout (TD-10/TD-11): clear the local session and report completion so
     * Navigation can route to `login`. Room caches and queued records are
     * intentionally untouched (IR-09).
     */
    fun logout(onLoggedOut: () -> Unit) {
        clearSession(onLoggedOut)
    }

    /**
     * Gudang change (FEATURE §6.8, IR-09): ending the session is exactly what
     * logout does; the operator then selects another Gudang at sign-in. Queued
     * records keep their original `locationId` and are never re-homed.
     */
    fun changeWarehouse(onChanged: () -> Unit) {
        clearSession(onChanged)
    }

    private fun clearSession(onCleared: () -> Unit) {
        viewModelScope.launch {
            session.clearSession()
            onCleared()
        }
    }
}
