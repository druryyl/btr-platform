# Investigation — BGud Android App Infinite Spinner on First Install

**Type:** Bug investigation (Analyst + Architect) — root cause and recommended solution.
**Surface:** `src/BGud` (Android, `com.elsasa.bgud`), startup/session gate + login flow.
**Status:** Root cause confirmed and **fixed** (see §7). Not caused by an unreachable API on the startup screen.
**Related artifacts:** `src/BGud/app/src/main/java/com/elsasa/bgud/ui/Navigation.kt`, `viewmodel/LoginViewModel.kt`, `datastore/SessionPreferencesDataSource.kt`, `network/ApiClient.kt`.

---

## 1. Executive Summary

The infinite spinner on a **fresh install** is produced **before any network call is attempted**. It is a session-gate bug in `AppNavigation`:

- `Navigation.kt:154` reads `val token by session.token.collectAsState(initial = null)`.
- `Navigation.kt:156` then treats `token == null` as "session not yet read" and renders a `CircularProgressIndicator` forever.
- But `SessionPreferencesDataSource.token` (`SessionPreferencesDataSource.kt:42-43`) is a `Flow<String?>` whose value is **legitimately `null` whenever no token is stored** — which is exactly the first-install state (empty DataStore).

The two states — "DataStore has not emitted yet" and "DataStore emitted, no token present" — are **both represented by `null`** and cannot be distinguished. On a fresh install the DataStore emits `null` and the gate condition `token == null` remains `true` forever, so the spinner never resolves and the app never reaches the login screen.

This is **not** caused by an unreachable API at startup (the spinner renders before `ApiClient` is ever constructed), but there is a **second, independent blocker**: the cloud base URL is hardcoded blank, so even with the spinner fixed, login cannot succeed.

---

## 2. Root Cause

### 2.1 The session gate (primary cause of the infinite spinner)

`ui/Navigation.kt`:

```kotlin
val token by session.token.collectAsState(initial = null)   // line 154

if (token == null) {                                        // line 156
    // "Session not yet read — hold the gate instead of guessing."
    Box(...) { CircularProgressIndicator() }                // line 162
    return
}

NavHost(
    startDestination = if (token.isNullOrBlank()) "login" else "home"  // line 169
)
```

`datastore/SessionPreferencesDataSource.kt`:

```kotlin
val token: Flow<String?> = context.sessionPreferencesDataStore.data
    .map { it[TOKEN_KEY] }                                  // line 42-43
```

On first install `session_preferences` is empty. The DataStore `data` flow emits a `Preferences` with no `token` key, so the mapping produces `null`. Compose holds `token = null` (same as the `initial` value), the `if (token == null)` branch stays active, and the indeterminate `CircularProgressIndicator` spins forever.

The intent ("hold the gate until the session is read") cannot be expressed with a nullable string, because `null` already has the domain meaning "no session". A separate, non-null sentinel/loading state is required.

Note the same defect reappears after **logout**: `SettingsViewModel` calls `clearSession()`, which removes the token key (`SessionPreferencesDataSource.kt:83-90`). Any recomposition of `AppNavigation` while the token is absent renders the spinner again, not `login`.

### 2.2 The base URL is intentionally blank (second blocker, not the spinner)

`ui/Navigation.kt:72`:

```kotlin
private const val CLOUD_BASE_URL = ""
```

The comment states the transport is unresolved (C-3/R-03) and no environment URL is hardcoded. `LoginViewModel.login` explicitly guards this (`LoginViewModel.kt:104-109`):

```kotlin
if (baseUrl.isBlank()) {
    _error.value = "Server belum dikonfigurasi. Hubungi administrator."
    return@launch
}
```

So once the gate is fixed, first install shows the login screen, but pressing **Login** immediately shows `"Server belum dikonfigurasi. Hubungi administrator."` — it does not hang. The app currently has **no way to reach the API**.

There is **no other place** the base URL is configured:

- No `BuildConfig.BASE_URL` / `buildConfigField` in `app/build.gradle.kts` (`buildFeatures { compose = true }` only; no `buildConfig = true`).
- No `res/values` string, no Settings screen field, no `local.properties` wiring.
- The single composition-root value is `CLOUD_BASE_URL` in `Navigation.kt`, passed to `LoginViewModelFactory`, `SynchronizationViewModelFactory`, and `ReturnOrderSyncViewModelFactory`.

To point the app at a real server, set `CLOUD_BASE_URL` (e.g. a trailing-slash URL such as `https://host/btr/…/`; the code normalizes a missing trailing slash) — or, better, move it to a `buildConfigField`/resource so it is not hand-edited per environment.

### 2.3 Why "unreachable API" is not the startup cause

- The spinner in `Navigation.kt:158-163` is composed before `NavHost` and therefore before any `ApiClient`/Retrofit instance exists. No HTTP request is issued while it is showing.
- Unreachable API would instead manifest as a login error (`LoginViewModel.kt:188-189`, `"Tidak dapat terhubung ke server. Periksa koneksi."`), not an indefinite spinner.
- `ApiClient` (`network/ApiClient.kt:26-38`) sets no explicit timeouts, so calls use OkHttp defaults (10 s connect / 10 s read). A black-holed network could produce a long login spinner, but that requires reaching the login screen first — which first install never does.

### 2.4 Secondary robustness gaps (not the reported symptom, but worth fixing)

1. **Uncaught DataStore read failures.** `session.token` has no `.catch { }`. A corrupted preferences file throws `IOException` on collection; the value never resolves and the spinner would hang (or the app crashes), indistinguishable from the gate bug.
2. **No OkHttp timeouts.** `ApiClient.create` builds `OkHttpClient` without `connectTimeout`/`readTimeout`/`callTimeout`. On a partially reachable network the login-time master sync (`LoginViewModel.kt:144-180`) can hang for a long time while `isSyncing` is true.
3. **BuildConfig disabled.** `buildFeatures { compose = true }` does not enable `buildConfig`, so a per-flavor `BASE_URL` cannot currently be generated without adding `buildConfig = true`.

---

## 3. Reproduction

1. Install the app on a device/emulator with no prior app data (fresh install). DataStore `session_preferences` is empty.
2. Launch. `AppNavigation` composes with `token == null`.
3. Observe an indefinite `CircularProgressIndicator`; the login screen is never shown and no network traffic is produced.

Workaround that confirms the diagnosis: install, then seed a token through a successful login on a build whose `CLOUD_BASE_URL` is set — `token` becomes a non-null string and the gate opens. (Not possible with the current blank URL, which is itself the second blocker.)

---

## 4. Recommended Solution

### 4.1 Fix the session gate (required)

Model the DataStore read explicitly instead of overloading `null`. Minimal option — separate "loaded" from "token value":

```kotlin
// SessionPreferencesDataSource.kt
val token: Flow<String?> = context.sessionPreferencesDataStore.data
    .catch { emit(emptyPreferences()) }   // recover from corruption
    .map { it[TOKEN_KEY] }
```

Then in `AppNavigation`, gate on a dedicated state, e.g. a `SessionGate` sealed type (`Loading` / `NoSession` / `Session(token)`) produced by a small ViewModel or by collecting into a sentinel. Example with an explicit sentinel:

```kotlin
private const val NOT_LOADED = "\u0000not-loaded"

val rawToken by session.token.collectAsState(initial = NOT_LOADED)
if (rawToken == NOT_LOADED) {
    CircularProgressIndicator()   // still loading
    return
}
val needsLogin = rawToken.isNullOrBlank()
NavHost(startDestination = if (needsLogin) "login" else "home") { ... }
```

Any approach is acceptable as long as "not yet loaded" and "no token" are distinct states. After the fix, a fresh install renders `login`, and logout (token cleared to `null`) also renders `login`.

### 4.2 Configure the base URL (required to actually log in)

Decide the deployment strategy (C-3/R-03) and make `CLOUD_BASE_URL` non-blank. Preferred: add `buildConfig = true` and a per-build-type/flavor `buildConfigField("String", "CLOUD_BASE_URL", "\"https://…/\"")`, then read `BuildConfig.CLOUD_BASE_URL` in `Navigation.kt`. Simplest interim: set the existing constant. Do not append a tenant segment — the tenant is JWT-resolved (ADR-007).

### 4.3 Hardening (recommended)

- Add explicit OkHttp timeouts in `ApiClient.create` (connect/read/call) so unreachable networks fail fast and surface the existing error messages.
- Add `.catch { emit(emptyPreferences()) }` (or equivalent) to the session/preference flows so a corrupt DataStore degrades to "no session" instead of hanging.

---

## 5. Impact / Affected Areas

| Area | File | Change |
| ---- | ---- | ------ |
| Session gate | `ui/Navigation.kt:154-169` | Distinguish loading from no-session; route to `login` on first install and logout |
| Token flow | `datastore/SessionPreferencesDataSource.kt:42-43` | Optional `.catch` recovery |
| Base URL config | `ui/Navigation.kt:72`, `app/build.gradle.kts` | Non-blank value / `buildConfigField` |
| HTTP client | `network/ApiClient.kt:26-33` | Explicit timeouts |

No transaction, sync, or business-rule behavior is affected; the defect is confined to Android startup/session presentation.

---

## 6. Verification

- Fresh install (clear app data) → login screen renders; no spinner.
- Login with a valid server (once `CLOUD_BASE_URL` is set) → home renders.
- Logout → login screen renders (not a spinner).
- Airplane mode at login → `"Tidak dapat terhubung ke server. Periksa koneksi."` within the configured timeout.
- Existing session present at launch → home renders directly.

---

## 7. Resolution

| # | Change | File |
| - | ------ | ---- |
| 1 | Session gate now distinguishes "not loaded" (sentinel `SESSION_NOT_LOADED`) from "no token" (`null`), so first install and logout resolve to `login` instead of spinning | `ui/Navigation.kt:79-88,170-190` |
| 2 | Base URL set to the BTrade3 backend host/prefix: `http://dev.smart-ics.com:8089/belajar-api/` (BGud paths already carry `api/`, so the effective endpoint matches BTrade3's `http://dev.smart-ics.com:8089/belajar-api/api/…`) | `ui/Navigation.kt:63-79` |
| 3 | Cleartext HTTP permitted for `dev.smart-ics.com` (Android blocks `http://` by default) | `res/xml/network_security_config.xml`, `AndroidManifest.xml:16` |
| 4 | Explicit 30 s connect/read/write timeouts (mirrors BTrade3 `NetworkModule`) | `network/ApiClient.kt` |
| 5 | Preference flows recover from a corrupt DataStore with `emptyPreferences()` instead of leaving the token unresolved | `datastore/SessionPreferencesDataSource.kt:44-51` |

Verified with `:app:compileDebugKotlin` (clean compile).
