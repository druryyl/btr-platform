package com.elsasa.bgud.model.api

import com.google.gson.annotations.SerializedName

/**
 * Cloud HTTP contract DTOs (Architecture §8.4, §8.5, §9.1, §17.2, §19.2).
 *
 * Conventions (do not deviate):
 * - Every response is wrapped in the `JSendOk` envelope (`status`/`code`/`data`,
 *   lowercase — `Nuna.Lib.ActionResultHelper.JSendOk`); payload fields are
 *   PascalCase (ASP.NET Core default serialization of the C# records).
 * - No request ever carries `ServerId` as a command input (ADR-007, IR-09,
 *   P-06). The Cloud resolves the tenant server-side from the JWT. The only
 *   place the device supplies a tenant value is the legacy read route
 *   `GET /api/Brg/{serverId}` (ADR-007 §8), using the `ServerId` returned by
 *   the login response for display and for that route only (§8.4).
 */

/** `JSendOk` envelope: `status`/`code`/`data` (all lowercase). */
data class JSendEnvelope<T>(
    @SerializedName("status") val status: String? = null,
    @SerializedName("code") val code: String? = null,
    @SerializedName("data") val data: T? = null
)

/**
 * I-07 request — `POST api/Auth/login` (`IssueTokenCommand`).
 *
 * `locationId` is the selected operational location. The warehouse selector
 * (Gudang Gamping / Gudang Concat / Gudang Magelang) maps to a `locationId`
 * in S5.4; this layer only carries the value, it never invents the mapping.
 */
data class LoginRequest(
    @SerializedName("UserId") val userId: String,
    @SerializedName("Password") val password: String,
    @SerializedName("LocationId") val locationId: String
)

/**
 * I-07 response data — `IssueTokenResult` (§9.1).
 *
 * `serverId` is received for display and for the legacy I-06 read route only;
 * it is never sent back as a command input. `expiresAt` stays a String so no
 * date parsing can fail the login path.
 */
data class LoginResult(
    @SerializedName("Token") val token: String = "",
    @SerializedName("ExpiresAt") val expiresAt: String = "",
    @SerializedName("UserId") val userId: String = "",
    @SerializedName("UserName") val userName: String = "",
    @SerializedName("RoleId") val roleId: String = "",
    @SerializedName("LocationId") val locationId: String = "",
    @SerializedName("ServerId") val serverId: String = ""
)

/**
 * I-05 response item — `BarcodeType` (§5.2).
 *
 * `serverId` is deserialized (the Cloud returns it) but never sent and never
 * used for scoping; the local cache is scoped by the session binding (IR-09).
 */
data class BarcodeDto(
    @SerializedName("BrgBarcodeId") val brgBarcodeId: String = "",
    @SerializedName("BarcodeValue") val barcodeValue: String = "",
    @SerializedName("BrgId") val brgId: String = "",
    @SerializedName("BrgCode") val brgCode: String = "",
    @SerializedName("BrgName") val brgName: String = "",
    @SerializedName("Satuan") val satuan: String = "",
    @SerializedName("ServerId") val serverId: String = ""
)

/**
 * I-04 request — `POST /api/barcode-registration`
 * (`BarcodeRegistrationSubmitRequest`, §8.5, §19.2).
 *
 * No `ServerId` (ADR-007); `RequestedBy` is resolved server-side from the JWT.
 * `satuan` is '' when absent (BR-005).
 */
data class BarcodeRegistrationSubmitRequest(
    @SerializedName("ClientRequestId") val clientRequestId: String,
    @SerializedName("BarcodeValue") val barcodeValue: String,
    @SerializedName("BrgId") val brgId: String,
    @SerializedName("Satuan") val satuan: String = ""
)

/**
 * I-09 response item — status projection over
 * `BTRADE_BarcodeRegistrationRequest` for the caller's own requests
 * (`BarcodeRegistrationStatusQuery`, §8.3, §19.2).
 *
 * Key fields: `ClientRequestId`, `Status`, `ProcessedNote`. The remaining
 * fields are carried for diagnostics only.
 */
data class RegistrationStatusDto(
    @SerializedName("BarcodeRegistrationId") val barcodeRegistrationId: String = "",
    @SerializedName("ClientRequestId") val clientRequestId: String = "",
    @SerializedName("BarcodeValue") val barcodeValue: String = "",
    @SerializedName("BrgId") val brgId: String = "",
    @SerializedName("Satuan") val satuan: String = "",
    @SerializedName("Status") val status: String = "",
    @SerializedName("ProcessedNote") val processedNote: String = ""
)

/**
 * I-06 response item — `BrgType` (existing Barang download, ADR-005).
 *
 * Served by the legacy route `GET /api/Brg/{serverId}` whose path value is
 * the only tenant value the device ever supplies (ADR-007 §8). Full sync
 * mapping of these fields into `barang_entity` is owned by S5.3.
 */
data class BrgDto(
    @SerializedName("BrgId") val brgId: String = "",
    @SerializedName("BrgCode") val brgCode: String = "",
    @SerializedName("BrgName") val brgName: String = "",
    @SerializedName("KategoriName") val kategoriName: String = "",
    @SerializedName("SatBesar") val satBesar: String = "",
    @SerializedName("SatKecil") val satKecil: String = "",
    @SerializedName("Konversi") val konversi: Int = 0,
    @SerializedName("HrgSat") val hrgSat: Double = 0.0,
    @SerializedName("Stok") val stok: Int = 0,
    @SerializedName("ServerId") val serverId: String = ""
)
