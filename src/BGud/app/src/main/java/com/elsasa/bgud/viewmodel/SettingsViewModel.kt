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
 * Settings view model (SCR-MOB-008, Architecture §19.3).
 *
 * Settings is a navigation leaf (traceability: navigation — no workflow,
 * no domain capability; UX Blueprint §4). It exposes the login-bound
 * session for display and the session-closing action:
 *
 * - `user` / `warehouse` / `office` re-expose the DataStore session
 *   (`userId`, `warehouseCode`, `officeCode`; IR-09 — `warehouseCode`,
 *   never a `ServerId`, is the stored tenant binding; `officeCode` is the
 *   login-returned `serverId` kept for display-only, S5.5 precedent).
 * - `logout()` clears the session (`clearSession`) so the Navigation
 *   start-destination gate returns to `login` (IR-M8: no valid JWT → all
 *   operational commands blocked; §13.2 `any → login` when no valid JWT,
 *   token expired, or warehouse change requested). Warehouse change is
 *   realized as logout + re-authentication (IR-09); the local cache is
 *   replaced for the new tenant by the login sync (S5.3/S5.4) — this
 *   screen clears no Room state and issues no network call.
 */
class SettingsViewModel(
    private val session: SessionPreferencesDataSource
) : ViewModel() {

    val user: StateFlow<String> = session.userId
        .map { it.orEmpty() }
        .stateIn(viewModelScope, SharingStarted.WhileSubscribed(5_000), "")

    val warehouse: StateFlow<String> = session.warehouseCode
        .map { it.orEmpty() }
        .stateIn(viewModelScope, SharingStarted.WhileSubscribed(5_000), "")

    val office: StateFlow<String> = session.officeCode
        .map { it.orEmpty() }
        .stateIn(viewModelScope, SharingStarted.WhileSubscribed(5_000), "")

    /**
     * Logout (§13.2 `any → login`, IR-M8, IR-09): clear the session and
     * report completion so Navigation can route to `login`. Room caches
     * are intentionally untouched — the next login sync replaces them
     * for the newly bound tenant (S5.3/S5.4).
     */
    fun logout(onLoggedOut: () -> Unit) {
        viewModelScope.launch {
            session.clearSession()
            onLoggedOut()
        }
    }
}
