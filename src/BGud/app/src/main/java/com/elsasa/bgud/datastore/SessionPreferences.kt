package com.elsasa.bgud.datastore

import androidx.datastore.preferences.core.MutablePreferences
import androidx.datastore.preferences.core.Preferences
import androidx.datastore.preferences.core.stringPreferencesKey

/**
 * The local session snapshot (TD-10).
 *
 * The local session replaces the former JWT-based session. It carries the
 * signed-in Google email, the Cloud-resolved BTR identity, and the selected
 * Gudang together with its resolved `ServerId`:
 *
 * | Field | Meaning |
 * | ----- | ------- |
 * | `googleEmail` | signed-in Google email (session identity) |
 * | `userId` | resolved BTR `UserId` |
 * | `userName` | resolved BTR `UserName` |
 * | `roleId` | resolved BTR `RoleId` |
 * | `locationId` | selected Gudang (`GAMPING`/`CONCAT`/`MAGELANG`) |
 * | `serverId` | resolved `ServerId` for the selected Gudang |
 *
 * Session validity = `google_email` present (TD-10/TD-11). A legacy install
 * holding only the removed `token` key therefore has no Google email and is
 * treated as signed out (routed to sign-in). The `ServerId` is stored only to
 * supply the legacy `{serverId}` read routes (TD-08); it is never sent as a
 * command input on the four session-context routes.
 */
data class SessionState(
    val googleEmail: String,
    val userId: String,
    val userName: String,
    val roleId: String,
    val locationId: String,
    val serverId: String
) {
    /** TD-10/TD-11 — a session is valid only while a Google email is present. */
    val isValid: Boolean get() = googleEmail.isNotBlank()
}

/**
 * Pure preference mapping for the local session (TD-10).
 *
 * Kept separate from [SessionPreferencesDataSource] so the store/validity rule
 * is testable without an Android `Context`/DataStore. The removed `token` and
 * `office_code` keys are deliberately absent: no code path stores, reads, or
 * sends a JWT or password (ARCHITECTURE §10). Existing sync timestamps are
 * retained and owned by [SessionPreferencesDataSource].
 */
object SessionPreferences {

    val GoogleEmailKey = stringPreferencesKey("google_email")
    val UserIdKey = stringPreferencesKey("user_id")
    val UserNameKey = stringPreferencesKey("user_name")
    val RoleIdKey = stringPreferencesKey("role_id")
    val LocationIdKey = stringPreferencesKey("location_id")
    val ServerIdKey = stringPreferencesKey("server_id")

    /** Read the session snapshot; absent keys read back as empty strings. */
    fun snapshot(preferences: Preferences): SessionState = SessionState(
        googleEmail = preferences[GoogleEmailKey].orEmpty(),
        userId = preferences[UserIdKey].orEmpty(),
        userName = preferences[UserNameKey].orEmpty(),
        roleId = preferences[RoleIdKey].orEmpty(),
        locationId = preferences[LocationIdKey].orEmpty(),
        serverId = preferences[ServerIdKey].orEmpty()
    )

    /** Persist the six session keys; sync timestamps are untouched. */
    fun write(preferences: MutablePreferences, session: SessionState) {
        preferences[GoogleEmailKey] = session.googleEmail
        preferences[UserIdKey] = session.userId
        preferences[UserNameKey] = session.userName
        preferences[RoleIdKey] = session.roleId
        preferences[LocationIdKey] = session.locationId
        preferences[ServerIdKey] = session.serverId
    }

    /** Clear the session keys; existing sync timestamps are retained. */
    fun clear(preferences: MutablePreferences) {
        preferences.remove(GoogleEmailKey)
        preferences.remove(UserIdKey)
        preferences.remove(UserNameKey)
        preferences.remove(RoleIdKey)
        preferences.remove(LocationIdKey)
        preferences.remove(ServerIdKey)
    }
}
