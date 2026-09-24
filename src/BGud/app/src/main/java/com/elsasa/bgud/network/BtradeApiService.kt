package com.elsasa.bgud.network

import com.elsasa.bgud.model.api.BarcodeDto
import com.elsasa.bgud.model.api.BarcodeRegistrationSubmitRequest
import com.elsasa.bgud.model.api.BrgDto
import com.elsasa.bgud.model.api.CustomerDto
import com.elsasa.bgud.model.api.DriverDto
import com.elsasa.bgud.model.api.JSendEnvelope
import com.elsasa.bgud.model.api.RegistrationStatusDto
import com.elsasa.bgud.model.api.ReturnOrderSubmitRequest
import com.elsasa.bgud.model.api.SalesPersonDto
import com.elsasa.bgud.model.api.SessionResolveRequest
import com.elsasa.bgud.model.api.SessionResolveResult
import com.google.gson.JsonElement
import retrofit2.http.Body
import retrofit2.http.GET
import retrofit2.http.POST
import retrofit2.http.Path

/**
 * Cloud HTTP contract consumed by BGud.
 *
 * BGud operates anonymously (OQ-001/OQ-002): there is no `api/Auth/login`
 * call, no token, and no `Authorization` header. The session context travels
 * in the fixed `X-Session-Location` / `X-Session-Actor` headers, attached by
 * [SessionContextInterceptor] (TD-03/TD-12).
 *
 * - `POST api/session/resolve` — anonymous account resolution (TD-02); the
 *   request body carries only the Google email and returns the BTR identity
 *   plus the locationId→ServerId mapping.
 * - `GET api/barcodes/sync` — bulk Active download for the header-resolved
 *   tenant.
 * - `POST api/barcode-registration` — body carries no `ServerId`/actor (P-06).
 * - `GET api/BarcodeRegistration/status` — the caller's own request outcomes.
 * - `GET api/Brg/{serverId}` — existing Barang download; the legacy
 *   `{serverId}` read route keeps its contract, supplied from the session's
 *   resolved `serverId` (TD-08).
 *
 * Return Order contract:
 * - `POST api/return-order` — idempotent submit keyed by the ULID
 *   `ReturnOrderId` (IR-RO-01); body carries no `ServerId` (P-06).
 * - `GET api/Customer|SalesPerson|Driver/{serverId}` — reference downloads on
 *   the same legacy `{serverId}` path convention (TD-08).
 *
 * This interface declares no authentication itself; the interceptor attaches
 * the session context.
 */
interface BtradeApiService {

    /** TD-02 — anonymous account resolution; body carries only the email. */
    @POST("api/session/resolve")
    suspend fun resolveSession(
        @Body request: SessionResolveRequest
    ): JSendEnvelope<SessionResolveResult>

    /** I-05 — bulk Active barcode download for the header-resolved tenant. */
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

    /** I-06 — existing Barang download for the session-resolved `serverId`. */
    @GET("api/Brg/{serverId}")
    suspend fun brgList(@Path("serverId") serverId: String): JSendEnvelope<List<BrgDto>>

    /** I-RO-01 — submit one Return Order (idempotent by ULID `ReturnOrderId`). */
    @POST("api/return-order")
    suspend fun submitReturnOrder(
        @Body request: ReturnOrderSubmitRequest
    ): JSendEnvelope<JsonElement>

    /** I-RO-03 — existing Customer download for the session-resolved `serverId`. */
    @GET("api/Customer/{serverId}")
    suspend fun customerList(@Path("serverId") serverId: String): JSendEnvelope<List<CustomerDto>>

    /** I-RO-04 — existing Sales Person download for the session-resolved `serverId`. */
    @GET("api/SalesPerson/{serverId}")
    suspend fun salesPersonList(@Path("serverId") serverId: String): JSendEnvelope<List<SalesPersonDto>>

    /** I-RO-05 — Driver download for the session-resolved `serverId` (S2.5). */
    @GET("api/Driver/{serverId}")
    suspend fun driverList(@Path("serverId") serverId: String): JSendEnvelope<List<DriverDto>>
}
