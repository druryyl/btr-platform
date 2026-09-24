package com.elsasa.bgud.repository

import com.elsasa.bgud.model.api.SessionResolveRequest
import com.elsasa.bgud.network.BtradeApiService
import retrofit2.HttpException
import java.io.IOException

/** One Gudang from the Cloud's TD-02 warehouse mapping. */
data class ResolvedWarehouse(
    val locationId: String,
    val serverId: String
)

/** The authoritative BTR identity resolved for a Google email (TD-02). */
data class ResolvedSessionAccount(
    val email: String,
    val userId: String,
    val userName: String,
    val roleId: String,
    val warehouses: List<ResolvedWarehouse>
)

/** Outcome of `POST api/session/resolve` (TD-02, §9 Error Semantics). */
sealed interface SessionResolveOutcome {
    /** The account resolved to an active BTR user with a warehouse mapping. */
    data class Resolved(val account: ResolvedSessionAccount) : SessionResolveOutcome

    /** HTTP 400 — unmapped/invalid Google account; sign-in is refused. */
    data class Refused(val message: String) : SessionResolveOutcome

    /** Transport/other failure; no session is created. */
    data class Failed(val message: String) : SessionResolveOutcome
}

/**
 * Cloud account-resolution repository (TD-02).
 *
 * Calls the anonymous `POST api/session/resolve` with only the Google email in
 * the body (TD-12) and returns the resolved BTR identity plus the
 * locationId→ServerId mapping, or a refusal. It stores no session itself —
 * session persistence is owned by the sign-in flow.
 */
class SessionResolverRepository(private val api: BtradeApiService) {

    suspend fun resolve(email: String): SessionResolveOutcome {
        val normalizedEmail = email.trim()
        if (normalizedEmail.isBlank()) {
            return SessionResolveOutcome.Refused(ACCOUNT_NOT_REGISTERED)
        }
        return try {
            val envelope = api.resolveSession(SessionResolveRequest(normalizedEmail))
            val data = envelope.data
            if (data == null || data.userId.isBlank()) {
                SessionResolveOutcome.Refused(ACCOUNT_NOT_REGISTERED)
            } else {
                SessionResolveOutcome.Resolved(
                    ResolvedSessionAccount(
                        email = normalizedEmail,
                        userId = data.userId,
                        userName = data.userName,
                        roleId = data.roleId,
                        warehouses = data.warehouses.map {
                            ResolvedWarehouse(
                                locationId = it.locationId,
                                serverId = it.serverId
                            )
                        }
                    )
                )
            }
        } catch (e: HttpException) {
            when (e.code()) {
                HTTP_BAD_REQUEST -> SessionResolveOutcome.Refused(ACCOUNT_NOT_REGISTERED)
                else -> SessionResolveOutcome.Failed("Login gagal (kode ${e.code()}).")
            }
        } catch (e: IOException) {
            SessionResolveOutcome.Failed("Tidak dapat terhubung ke server. Periksa koneksi.")
        } catch (e: Exception) {
            SessionResolveOutcome.Failed(
                "Login gagal: ${e.message?.take(200) ?: "kesalahan tidak diketahui."}"
            )
        }
    }

    companion object {
        /** §9 — unmapped/invalid account at `POST api/session/resolve` → 400. */
        const val HTTP_BAD_REQUEST = 400

        /** Operator-facing refusal (FEATURE §9: contact an administrator). */
        const val ACCOUNT_NOT_REGISTERED =
            "Akun Google tidak terdaftar atau tidak aktif. Hubungi administrator."
    }
}
