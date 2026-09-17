package com.elsasa.bgud.repository

import com.elsasa.bgud.dao.CustomerDao
import com.elsasa.bgud.dao.DriverDao
import com.elsasa.bgud.dao.SalesPersonDao
import com.elsasa.bgud.datastore.SessionPreferencesDataSource
import com.elsasa.bgud.model.CustomerEntity
import com.elsasa.bgud.model.DriverEntity
import com.elsasa.bgud.model.SalesPersonEntity
import com.elsasa.bgud.model.api.CustomerDto
import com.elsasa.bgud.model.api.DriverDto
import com.elsasa.bgud.model.api.SalesPersonDto
import com.elsasa.bgud.network.BtradeApiService
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext

/**
 * Return Order reference-cache synchronization repository (S4.3).
 *
 * Downloads the Customer / SalesPerson / Driver reference sets into Room,
 * mirroring the Barang/Barcode download pattern (`BarcodeSyncRepository`,
 * S5.3):
 *
 * ```text
 * download Customer (I-RO-03, replace local cache)
 *     ↓
 * download SalesPerson (I-RO-04, replace local cache)
 *     ↓
 * download Driver (I-RO-05, replace local cache)
 * ```
 *
 * Rules (no new decisions):
 * - Every call carries the JWT via [BtradeApiService] (S4.2
 *   `AuthInterceptor`); this class performs no authentication itself.
 * - No `ServerId` is stored or sent as a command input (P-06). The only
 *   tenant value the device ever supplies is the legacy read-route path
 *   `GET /api/Customer|SalesPerson|Driver/{serverId}` (the
 *   `GET /api/Brg/{serverId}` precedent), using the login-returned
 *   `serverId` passed per run for that route only (§8.1, §9.2). It is
 *   never persisted here; the session already holds both `WarehouseCode`
 *   and `ServerId` (resolved at login, IR-RO-04).
 * - Downloads replace the local caches (full set, `deleteAll` +
 *   `upsertAll`); timestamps advance only for the download that committed
 *   (`last_customer_sync`, `last_salesperson_sync`, `last_driver_sync`).
 * - Steps never abort each other; failures are collected in [errors].
 *   The Customer cache backs offline selection of the mandatory Customer
 *   (GAP-004); Salesman/Driver caches are optional data (ADR-RO-005).
 */
class ReturnOrderReferenceSyncRepository(
    private val api: BtradeApiService,
    private val customerDao: CustomerDao,
    private val salesPersonDao: SalesPersonDao,
    private val driverDao: DriverDao,
    private val session: SessionPreferencesDataSource
) {

    /**
     * Outcome of one ordered reference sync run. Steps never abort each
     * other; failures are collected in [errors] for the caller retry
     * surface.
     */
    data class SyncRunResult(
        val customerCount: Int = 0,
        val salesPersonCount: Int = 0,
        val driverCount: Int = 0,
        val errors: List<String> = emptyList()
    )

    /**
     * Full ordered run: customer → salesperson → driver.
     *
     * @param serverId login-returned Office id for the I-RO-03/04/05 read
     * routes only; blank fails each download (recorded in [errors]).
     */
    suspend fun sync(serverId: String): SyncRunResult = withContext(Dispatchers.IO) {
        val errors = mutableListOf<String>()

        val customers = try {
            downloadCustomer(serverId)
        } catch (e: Exception) {
            errors.add("customer: ${e.message}")
            0
        }

        val salesPersons = try {
            downloadSalesPerson(serverId)
        } catch (e: Exception) {
            errors.add("salesperson: ${e.message}")
            0
        }

        val drivers = try {
            downloadDriver(serverId)
        } catch (e: Exception) {
            errors.add("driver: ${e.message}")
            0
        }

        SyncRunResult(
            customerCount = customers,
            salesPersonCount = salesPersons,
            driverCount = drivers,
            errors = errors
        )
    }

    /**
     * Customer reference download (I-RO-03); replaces the local mandatory
     * Customer cache (GAP-004). [serverId] is the login-returned Office id,
     * used for this read route only (§8.1, §9.2).
     */
    suspend fun downloadCustomer(serverId: String): Int = withContext(Dispatchers.IO) {
        if (serverId.isBlank()) {
            throw IllegalArgumentException("serverId is required for Customer download (I-RO-03)")
        }
        val envelope = api.customerList(serverId)
        val dtos = envelope.data ?: emptyList()
        val entities = dtos.map { it.toEntity() }
        customerDao.deleteAll()
        if (entities.isNotEmpty()) {
            customerDao.upsertAll(entities)
        }
        session.setLastCustomerSync(System.currentTimeMillis())
        entities.size
    }

    /**
     * Sales Person reference download (I-RO-04); replaces the local
     * optional Salesman cache (ADR-RO-005). [serverId] is the
     * login-returned Office id, used for this read route only (§8.1, §9.2).
     */
    suspend fun downloadSalesPerson(serverId: String): Int = withContext(Dispatchers.IO) {
        if (serverId.isBlank()) {
            throw IllegalArgumentException("serverId is required for SalesPerson download (I-RO-04)")
        }
        val envelope = api.salesPersonList(serverId)
        val dtos = envelope.data ?: emptyList()
        val entities = dtos.map { it.toEntity() }
        salesPersonDao.deleteAll()
        if (entities.isNotEmpty()) {
            salesPersonDao.upsertAll(entities)
        }
        session.setLastSalesPersonSync(System.currentTimeMillis())
        entities.size
    }

    /**
     * Driver reference download (I-RO-05, S2.5); replaces the local
     * optional Driver cache (ADR-RO-005, GAP-013). [serverId] is the
     * login-returned Office id, used for this read route only (§8.1, §9.2).
     */
    suspend fun downloadDriver(serverId: String): Int = withContext(Dispatchers.IO) {
        if (serverId.isBlank()) {
            throw IllegalArgumentException("serverId is required for Driver download (I-RO-05)")
        }
        val envelope = api.driverList(serverId)
        val dtos = envelope.data ?: emptyList()
        val entities = dtos.map { it.toEntity() }
        driverDao.deleteAll()
        if (entities.isNotEmpty()) {
            driverDao.upsertAll(entities)
        }
        session.setLastDriverSync(System.currentTimeMillis())
        entities.size
    }

    companion object {
        /** I-RO-03 item → mandatory Customer cache row. */
        fun CustomerDto.toEntity(): CustomerEntity = CustomerEntity(
            customerId = customerId,
            customerCode = customerCode,
            customerName = customerName,
            address = alamat
        )

        /** I-RO-04 item → optional Salesman cache row. */
        fun SalesPersonDto.toEntity(): SalesPersonEntity = SalesPersonEntity(
            salesPersonId = salesPersonId,
            salesPersonName = salesPersonName
        )

        /** I-RO-05 item → optional Driver cache row. */
        fun DriverDto.toEntity(): DriverEntity = DriverEntity(
            driverId = driverId,
            driverName = driverName,
            isAktif = isAktif
        )
    }
}
