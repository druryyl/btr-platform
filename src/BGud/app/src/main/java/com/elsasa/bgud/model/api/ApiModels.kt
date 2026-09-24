package com.elsasa.bgud.model.api

import com.google.gson.annotations.SerializedName

/**
 * Cloud HTTP contract DTOs (Architecture §8.4, §8.5, §9.1, §17.2, §19.2).
 *
 * Conventions (do not deviate):
 * - Every response is wrapped in the `JSendOk` envelope (`status`/`code`/`data`,
 *   lowercase — `Nuna.Lib.ActionResultHelper.JSendOk`); payload fields are
 *   PascalCase (ASP.NET Core default serialization of the C# records).
 * - No request ever carries `ServerId` as a command input on the four
 *   session-context routes (IR-09, P-06). The Cloud resolves the tenant
 *   server-side from `X-Session-Location`. The only place the device supplies
 *   a tenant value is the legacy read route `GET /api/Brg/{serverId}` (TD-08),
 *   using the session's resolved `ServerId` for that route only.
 */

/** `JSendOk` envelope: `status`/`code`/`data` (all lowercase). */
data class JSendEnvelope<T>(
    @SerializedName("status") val status: String? = null,
    @SerializedName("code") val code: String? = null,
    @SerializedName("data") val data: T? = null
)

/**
 * TD-02 request — `POST api/session/resolve`.
 *
 * Carries only the signed-in Google email (TD-12). There is no password, no
 * location, and no `ServerId`: the Cloud resolves the authoritative BTR
 * identity and returns the locationId→ServerId mapping.
 */
data class SessionResolveRequest(
    @SerializedName("Email") val email: String
)

/**
 * TD-02 response data — the resolved BTR identity plus the warehouse mapping.
 *
 * `warehouses` supplies the Gudang selector and the `ServerId` for the legacy
 * `{serverId}` read routes (TD-08); `serverId` is never sent back as a command
 * input on the four session-context routes.
 */
data class SessionResolveResult(
    @SerializedName("UserId") val userId: String = "",
    @SerializedName("UserName") val userName: String = "",
    @SerializedName("RoleId") val roleId: String = "",
    @SerializedName("Warehouses") val warehouses: List<WarehouseDto> = emptyList()
)

/**
 * TD-02 warehouse mapping item — one Gudang (`locationId`) and its resolved
 * `ServerId`.
 */
data class WarehouseDto(
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
 * No `ServerId`/actor (P-06); `RequestedBy` is resolved server-side from
 * `X-Session-Actor`. `satuan` is '' when absent (BR-005).
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
 * I-RO-01 request — `POST /api/return-order`
 * (`ReturnOrderUploadCommand`, Return Order Architecture §8.1, §19.3).
 *
 * `ReturnOrderId` is the device-generated ULID idempotency key (IR-RO-01);
 * resubmission replaces the staged copy, never duplicates (Arch §10.6).
 * `ReturnOrderDate` is a `yyyy-MM-dd` string, mirroring the Cloud
 * `ReturnOrderType` and the `OrderModel` string-date convention.
 * No `ServerId`/actor — the Cloud resolves the tenant and actor server-side
 * from `X-Session-Location`/`X-Session-Actor` (P-06). Items are carried in
 * `ListItem` (singular), matching the Cloud `ReturnOrderUploadCommand`
 * property name.
 */
data class ReturnOrderSubmitRequest(
    @SerializedName("ReturnOrderId") val returnOrderId: String,
    @SerializedName("ReturnOrderDate") val returnOrderDate: String,
    @SerializedName("WarehouseCode") val warehouseCode: String,
    @SerializedName("CustomerId") val customerId: String,
    @SerializedName("CustomerName") val customerName: String = "",
    @SerializedName("SalesPersonId") val salesPersonId: String = "",
    @SerializedName("SalesPersonName") val salesPersonName: String = "",
    @SerializedName("DriverId") val driverId: String = "",
    @SerializedName("DriverName") val driverName: String = "",
    @SerializedName("Note") val note: String = "",
    @SerializedName("ListItem") val listItem: List<ReturnOrderItemDto> = emptyList()
)

/**
 * I-RO-01 request item — `ReturnOrderItemType` (Return Order Architecture
 * §5.2, §8.1).
 *
 * `Qty` is the physical-unit quantity (IR-RO-03); no small-unit
 * normalization anywhere (P-09). `JenisRetur` is exactly `BAGUS` | `RUSAK`
 * (ADR-RO-004).
 */
data class ReturnOrderItemDto(
    @SerializedName("ReturnOrderId") val returnOrderId: String = "",
    @SerializedName("NoUrut") val noUrut: Int = 0,
    @SerializedName("BrgId") val brgId: String = "",
    @SerializedName("BrgCode") val brgCode: String = "",
    @SerializedName("BrgName") val brgName: String = "",
    @SerializedName("Qty") val qty: Double = 0.0,
    @SerializedName("SatId") val satId: String = "",
    @SerializedName("JenisRetur") val jenisRetur: String = ""
)

/**
 * I-RO-03 response item — `CustomerType` (existing Customer download,
 * GAP-004, Return Order Architecture §8.3).
 *
 * Served by the existing route `GET /api/Customer/{serverId}` whose path
 * value is the session-resolved `serverId` (the `GET api/Brg/{serverId}`
 * precedent). `serverId` is deserialized (the Cloud returns it) but never
 * sent and never used for scoping; the local cache is scoped by the session
 * binding. Full sync mapping of these fields into `customer_entity` is
 * owned by S4.3.
 */
data class CustomerDto(
    @SerializedName("CustomerId") val customerId: String = "",
    @SerializedName("CustomerCode") val customerCode: String = "",
    @SerializedName("CustomerName") val customerName: String = "",
    @SerializedName("Alamat") val alamat: String = "",
    @SerializedName("ServerId") val serverId: String = ""
)

/**
 * I-RO-04 response item — `SalesPersonType` (existing Sales Person download,
 * GAP-005, Return Order Architecture §8.3).
 *
 * Served by the existing route `GET /api/SalesPerson/{serverId}` whose path
 * value is the session-resolved `serverId` (the `GET api/Brg/{serverId}`
 * precedent). `serverId` is deserialized (the Cloud returns it) but never
 * sent and never used for scoping; the local cache is scoped by the session
 * binding. Full sync mapping of these fields into `salesperson_entity` is
 * owned by S4.3.
 */
data class SalesPersonDto(
    @SerializedName("SalesPersonId") val salesPersonId: String = "",
    @SerializedName("SalesPersonCode") val salesPersonCode: String = "",
    @SerializedName("SalesPersonName") val salesPersonName: String = "",
    @SerializedName("Email") val email: String = "",
    @SerializedName("ServerId") val serverId: String = ""
)

/**
 * I-RO-05 response item — `DriverType` (new Driver download, GAP-013,
 * Return Order Architecture §8.3).
 *
 * Served by `GET /api/Driver/{serverId}` (S2.5 `DriverController`) whose
 * path value is the session-resolved `serverId` (the `GET api/Brg/{serverId}`
 * precedent). `serverId` is deserialized (the Cloud returns it) but never
 * sent and never used for scoping; the local cache is scoped by the session
 * binding. Full sync mapping of these fields into `driver_entity` is owned
 * by S4.3.
 */
data class DriverDto(
    @SerializedName("DriverId") val driverId: String = "",
    @SerializedName("DriverName") val driverName: String = "",
    @SerializedName("IsAktif") val isAktif: Boolean = true,
    @SerializedName("ServerId") val serverId: String = ""
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
