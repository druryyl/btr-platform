package com.elsasa.bgud.network

import com.elsasa.bgud.model.api.BarcodeDto
import com.elsasa.bgud.model.api.BarcodeRegistrationSubmitRequest
import com.elsasa.bgud.model.api.BrgDto
import com.elsasa.bgud.model.api.JSendEnvelope
import com.elsasa.bgud.model.api.LoginRequest
import com.elsasa.bgud.model.api.LoginResult
import com.elsasa.bgud.model.api.RegistrationStatusDto
import com.google.gson.JsonElement
import retrofit2.http.Body
import retrofit2.http.GET
import retrofit2.http.POST
import retrofit2.http.Path

/**
 * Cloud Barcode Registry HTTP contract consumed by BGud
 * (Architecture §8.4, §8.5, §9.1, §17.2).
 *
 * - I-07 `POST api/Auth/login` — token issuance; anonymous (no JWT yet).
 * - I-05 `GET /api/barcodes/sync` — mandated lowercase route (IR-07, TQ-5),
 *   JWT; bulk Active download for the JWT-resolved tenant.
 * - I-04 `POST /api/barcode-registration` — mandated lowercase route (IR-07,
 *   TQ-5), JWT; body carries no `ServerId` (ADR-007).
 * - I-09 `GET api/BarcodeRegistration/status` — JWT; the caller's own request
 *   outcomes (§8.3). No `clientRequestId` filter (matches S3.7 GO INFO-003:
 *   the Cloud returns all of the caller's own requests).
 * - I-06 `GET /api/Brg/{serverId}` — existing Barang download (ADR-005); the
 *   only call where the device supplies a tenant value in the path
 *   (ADR-007 §8, §8.4).
 *
 * No point-lookup endpoint exists for MVP (TQ-5); scan resolution is local
 * Room only (P-07). No `ServerId` appears on any command body.
 */
interface BtradeApiService {

    /** I-07 — authenticate; `serverId` is returned for display + I-06 only. */
    @POST("api/Auth/login")
    suspend fun login(@Body request: LoginRequest): JSendEnvelope<LoginResult>

    /** I-05 — bulk Active barcode download for the JWT-resolved tenant. */
    @GET("api/barcodes/sync")
    suspend fun barcodeSync(): JSendEnvelope<List<BarcodeDto>>

    /** I-04 — submit one queued registration request (one HTTP call each). */
    @POST("api/barcode-registration")
    suspend fun submitRegistration(
        @Body request: BarcodeRegistrationSubmitRequest
    ): JSendEnvelope<JsonElement>

    /** I-09 — the caller's own registration request outcomes. */
    @GET("api/BarcodeRegistration/status")
    suspend fun registrationStatus(): JSendEnvelope<List<RegistrationStatusDto>>

    /** I-06 — existing Barang download for the login-returned `serverId`. */
    @GET("api/Brg/{serverId}")
    suspend fun brgList(@Path("serverId") serverId: String): JSendEnvelope<List<BrgDto>>
}
