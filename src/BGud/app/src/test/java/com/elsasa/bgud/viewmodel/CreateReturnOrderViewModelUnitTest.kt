package com.elsasa.bgud.viewmodel

import android.content.Context
import android.content.ContextWrapper
import com.elsasa.bgud.dao.BarangDao
import com.elsasa.bgud.dao.BarcodeDao
import com.elsasa.bgud.dao.CustomerDao
import com.elsasa.bgud.dao.DriverDao
import com.elsasa.bgud.dao.ReturnOrderDao
import com.elsasa.bgud.dao.ReturnOrderItemDao
import com.elsasa.bgud.dao.SalesPersonDao
import com.elsasa.bgud.database.AppDatabase
import com.elsasa.bgud.datastore.SessionPreferencesDataSource
import com.elsasa.bgud.model.BarangEntity
import com.elsasa.bgud.model.BarcodeEntity
import com.elsasa.bgud.model.CustomerEntity
import com.elsasa.bgud.model.DriverEntity
import com.elsasa.bgud.model.ReturnOrderEntity
import com.elsasa.bgud.model.ReturnOrderItemEntity
import com.elsasa.bgud.model.SalesPersonEntity
import com.elsasa.bgud.repository.ReturnOrderCaptureRepository
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.ExperimentalCoroutinesApi
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.emptyFlow
import kotlinx.coroutines.test.StandardTestDispatcher
import kotlinx.coroutines.test.advanceUntilIdle
import kotlinx.coroutines.test.resetMain
import kotlinx.coroutines.test.runTest
import kotlinx.coroutines.test.setMain
import org.junit.After
import org.junit.Assert.assertEquals
import org.junit.Assert.assertNull
import org.junit.Assert.assertTrue
import org.junit.Before
import org.junit.Test
import java.io.File
import java.nio.file.Files

@OptIn(ExperimentalCoroutinesApi::class)
class CreateReturnOrderViewModelUnitTest {

    private val testDispatcher = StandardTestDispatcher()

    @Before
    fun setUp() {
        Dispatchers.setMain(testDispatcher)
    }

    @After
    fun tearDown() {
        Dispatchers.resetMain()
    }

    private fun createTestContext(): Context {
        val tempDir: File = Files.createTempDirectory("create_ro_vm_test").toFile()
        return object : ContextWrapper(null) {
            override fun getApplicationContext(): Context = this
            override fun getFilesDir(): File = tempDir
            override fun getDataDir(): File = tempDir
        }
    }

    private class FakeBarcodeDao(
        private val barcodes: Map<String, BarcodeEntity> = emptyMap()
    ) : BarcodeDao {
        var queryCount: Int = 0

        override suspend fun getByKey(barcodeValueKey: String): BarcodeEntity? {
            queryCount++
            return barcodes[barcodeValueKey]
        }
        override suspend fun getById(brgBarcodeId: String): BarcodeEntity? = barcodes.values.firstOrNull { it.brgBarcodeId == brgBarcodeId }
        override fun listByBrg(brgId: String): Flow<List<BarcodeEntity>> = emptyFlow()
        override suspend fun getUnitsByBrg(brgId: String): List<String> =
            barcodes.values.filter { it.brgId == brgId && it.satuan.isNotBlank() }.map { it.satuan }.distinct()
        override suspend fun paged(limit: Int, offset: Int): List<BarcodeEntity> = emptyList()
        override suspend fun search(query: String, limit: Int, offset: Int): List<BarcodeEntity> = emptyList()
        override suspend fun upsert(barcode: BarcodeEntity) {}
        override suspend fun upsertAll(barcodes: List<BarcodeEntity>) {}
        override suspend fun deleteById(brgBarcodeId: String) {}
        override suspend fun deleteAll() {}
    }

    private class FakeBarangDao(
        private val items: Map<String, BarangEntity> = emptyMap()
    ) : BarangDao {
        override suspend fun getById(brgId: String): BarangEntity? = items[brgId]
        override suspend fun search(query: String): List<BarangEntity> = items.values.filter { it.brgName.contains(query, ignoreCase = true) }
        override fun getAll(): Flow<List<BarangEntity>> = emptyFlow()
        override suspend fun upsert(barang: BarangEntity) {}
        override suspend fun upsertAll(barangs: List<BarangEntity>) {}
        override suspend fun deleteAll() {}
    }

    private class FakeCustomerDao : CustomerDao {
        override suspend fun getById(customerId: String): CustomerEntity? = null
        override suspend fun search(query: String): List<CustomerEntity> = emptyList()
        override fun getAll(): Flow<List<CustomerEntity>> = emptyFlow()
        override suspend fun listAll(): List<CustomerEntity> = emptyList()
        override suspend fun upsert(customer: CustomerEntity) {}
        override suspend fun upsertAll(customers: List<CustomerEntity>) {}
        override suspend fun deleteAll() {}
    }

    private class FakeSalesPersonDao : SalesPersonDao {
        override suspend fun getById(salesPersonId: String): SalesPersonEntity? = null
        override suspend fun search(query: String): List<SalesPersonEntity> = emptyList()
        override fun getAll(): Flow<List<SalesPersonEntity>> = emptyFlow()
        override suspend fun listAll(): List<SalesPersonEntity> = emptyList()
        override suspend fun upsert(salesPerson: SalesPersonEntity) {}
        override suspend fun upsertAll(salesPersons: List<SalesPersonEntity>) {}
        override suspend fun deleteAll() {}
    }

    private class FakeDriverDao : DriverDao {
        override suspend fun getById(driverId: String): DriverEntity? = null
        override suspend fun search(query: String): List<DriverEntity> = emptyList()
        override fun getAll(): Flow<List<DriverEntity>> = emptyFlow()
        override suspend fun listAll(): List<DriverEntity> = emptyList()
        override suspend fun upsert(driver: DriverEntity) {}
        override suspend fun upsertAll(drivers: List<DriverEntity>) {}
        override suspend fun deleteAll() {}
    }

    private class FakeReturnOrderDao : ReturnOrderDao {
        override suspend fun getById(returnOrderId: String): ReturnOrderEntity? = null
        override suspend fun listByStatus(status: String): List<ReturnOrderEntity> = emptyList()
        override fun observeByStatus(status: String): Flow<List<ReturnOrderEntity>> = emptyFlow()
        override fun observeAll(): Flow<List<ReturnOrderEntity>> = emptyFlow()
        override suspend fun search(query: String, status: String, limit: Int, offset: Int): List<ReturnOrderEntity> = emptyList()
        override suspend fun countByStatus(status: String): Int = 0
        override suspend fun upsert(order: ReturnOrderEntity) {}
        override suspend fun upsertAll(orders: List<ReturnOrderEntity>) {}
        override suspend fun deleteById(returnOrderId: String) {}
        override suspend fun deleteAll() {}
    }

    private class FakeReturnOrderItemDao : ReturnOrderItemDao {
        override suspend fun listByParent(returnOrderId: String): List<ReturnOrderItemEntity> = emptyList()
        override fun observeByParent(returnOrderId: String): Flow<List<ReturnOrderItemEntity>> = emptyFlow()
        override suspend fun countByParent(returnOrderId: String): Int = 0
        override suspend fun upsert(item: ReturnOrderItemEntity) {}
        override suspend fun upsertAll(items: List<ReturnOrderItemEntity>) {}
        override suspend fun deleteByParent(returnOrderId: String) {}
        override suspend fun deleteAll() {}
    }

    private fun createViewModel(
        barcodeDao: FakeBarcodeDao = FakeBarcodeDao(),
        barangDao: FakeBarangDao = FakeBarangDao()
    ): CreateReturnOrderViewModel {
        val context = createTestContext()
        val session = SessionPreferencesDataSource(context)

        val fakeDatabase = object : AppDatabase() {
            override fun barcodeDao(): BarcodeDao = barcodeDao
            override fun barangDao(): BarangDao = barangDao
            override fun barcodeRegistrationRequestDao() = throw NotImplementedError()
            override fun returnOrderDao(): ReturnOrderDao = FakeReturnOrderDao()
            override fun returnOrderItemDao(): ReturnOrderItemDao = FakeReturnOrderItemDao()
            override fun customerDao(): CustomerDao = FakeCustomerDao()
            override fun salesPersonDao(): SalesPersonDao = FakeSalesPersonDao()
            override fun driverDao(): DriverDao = FakeDriverDao()
            override fun clearAllTables() {}
            override fun createInvalidationTracker() = throw NotImplementedError()
            override fun createOpenHelper(config: androidx.room.DatabaseConfiguration) = throw NotImplementedError()
        }

        val captureRepo = ReturnOrderCaptureRepository(
            database = fakeDatabase,
            session = session
        )

        return CreateReturnOrderViewModel(
            captureRepository = captureRepo,
            customerDao = FakeCustomerDao(),
            salesPersonDao = FakeSalesPersonDao(),
            driverDao = FakeDriverDao(),
            barangDao = barangDao,
            barcodeDao = barcodeDao,
            session = session
        )
    }

    @Test
    fun onBarcodeScanned_populatesSatuanFromBarcodeWhenItemUnitsBlank() = runTest(testDispatcher) {
        val testBarcodeValue = "8998008152086"
        val testBarcodeKey = BarcodeEntity.keyOf(testBarcodeValue)
        val testBrgId = "BRG-BIO-MIWON"

        // Item with empty satKecil and satBesar (reproducing test case)
        val testItem = BarangEntity(
            brgId = testBrgId,
            brgCode = "BMW01",
            brgName = "BIO MIWON 200 GR PREMIUM M",
            isAktif = true,
            satKecil = "",
            satBesar = ""
        )

        // Barcode with registered satuan from BTRADE_BrgBarcode
        val testBarcode = BarcodeEntity(
            brgBarcodeId = "BC-001",
            barcodeValue = testBarcodeValue,
            barcodeValueKey = testBarcodeKey,
            brgId = testBrgId,
            brgCode = "BMW01",
            brgName = "BIO MIWON 200 GR PREMIUM M",
            satuan = "BKS"
        )

        val barcodeDao = FakeBarcodeDao(mapOf(testBarcodeKey to testBarcode))
        val barangDao = FakeBarangDao(mapOf(testBrgId to testItem))
        val viewModel = createViewModel(barcodeDao = barcodeDao, barangDao = barangDao)

        // Scan barcode
        viewModel.onBarcodeScanned(testBarcodeValue)
        advanceUntilIdle()

        // Verify item is identified
        assertEquals("BIO MIWON 200 GR PREMIUM M", viewModel.pendingItem.value?.brgName)

        // Verify available units contains the barcode's satuan
        val options = viewModel.unitOptions()
        assertTrue("unitOptions should contain BKS", options.contains("BKS"))

        // Verify unit is auto-preselected from the barcode
        assertEquals("BKS", viewModel.pendingUnit.value)

        // Add item
        viewModel.onQtyChange("5")
        viewModel.onJenisReturChange("BAGUS")
        viewModel.addItem()
        advanceUntilIdle()

        assertNull(viewModel.pendingError.value)
        assertEquals(1, viewModel.items.value.size)
        assertEquals("BKS", viewModel.items.value[0].satId)
        assertEquals(5.0, viewModel.items.value[0].qty, 0.0)
    }

    @Test
    fun onBarcodeScanned_rapidConsecutiveCallsWithSameBarcode_queriesRoomOnce() = runTest(testDispatcher) {
        val barcodeDao = FakeBarcodeDao()
        val viewModel = createViewModel(barcodeDao = barcodeDao)

        val unmappedBarcode = "9999999999999"

        // Fire multiple rapid scans within debounce window (< 1500ms)
        viewModel.onBarcodeScanned(unmappedBarcode)
        viewModel.onBarcodeScanned(unmappedBarcode)
        viewModel.onBarcodeScanned(unmappedBarcode)
        advanceUntilIdle()

        assertEquals("Should query Room only once for rapid identical barcodes", 1, barcodeDao.queryCount)
        assertEquals(ReturnOrderCaptureRepository.ITEM_NOT_CACHED, viewModel.pendingError.value)
    }

    @Test
    fun onBarcodeScanned_distinctBarcodeValues_bypassesDebounce() = runTest(testDispatcher) {
        val barcodeDao = FakeBarcodeDao()
        val viewModel = createViewModel(barcodeDao = barcodeDao)

        val barcode1 = "1111111111111"
        val barcode2 = "2222222222222"

        viewModel.onBarcodeScanned(barcode1)
        viewModel.onBarcodeScanned(barcode2)
        advanceUntilIdle()

        assertEquals("Distinct barcodes should query Room for each distinct scan", 2, barcodeDao.queryCount)
    }

    @Test
    fun onBarcodeScanned_resetDebounceAfterSelectPendingItem() = runTest(testDispatcher) {
        val testBarcodeValue = "8998008152086"
        val testBarcodeKey = BarcodeEntity.keyOf(testBarcodeValue)
        val testBrgId = "BRG-01"

        val testItem = BarangEntity(
            brgId = testBrgId,
            brgCode = "B01",
            brgName = "Item 1",
            isAktif = true,
            satKecil = "PCS",
            satBesar = "DUS"
        )
        val testBarcode = BarcodeEntity(
            brgBarcodeId = "BC-01",
            barcodeValue = testBarcodeValue,
            barcodeValueKey = testBarcodeKey,
            brgId = testBrgId,
            brgCode = "B01",
            brgName = "Item 1",
            satuan = "PCS"
        )

        val barcodeDao = FakeBarcodeDao(mapOf(testBarcodeKey to testBarcode))
        val barangDao = FakeBarangDao(mapOf(testBrgId to testItem))
        val viewModel = createViewModel(barcodeDao = barcodeDao, barangDao = barangDao)

        // 1st scan - successfully selected
        viewModel.onBarcodeScanned(testBarcodeValue)
        advanceUntilIdle()
        assertEquals(1, barcodeDao.queryCount)
        assertEquals("Item 1", viewModel.pendingItem.value?.brgName)

        // Clear pending item (user cancels or re-scans)
        viewModel.onClearPendingItem()
        assertNull(viewModel.pendingItem.value)

        // Scan the same barcode immediately without waiting 1500ms
        viewModel.onBarcodeScanned(testBarcodeValue)
        advanceUntilIdle()
        assertEquals("Debounce must be reset when item was previously selected and cleared", 2, barcodeDao.queryCount)
        assertEquals("Item 1", viewModel.pendingItem.value?.brgName)
    }

    @Test
    fun onBarcodeScanned_resetDebounceOnClearPendingItem_forUnregisteredBarcode() = runTest(testDispatcher) {
        val barcodeDao = FakeBarcodeDao()
        val viewModel = createViewModel(barcodeDao = barcodeDao)

        val unmappedBarcode = "9999999999999"

        viewModel.onBarcodeScanned(unmappedBarcode)
        advanceUntilIdle()
        assertEquals(1, barcodeDao.queryCount)

        // User clears pending item / error state
        viewModel.onClearPendingItem()

        // Scanning the same barcode immediately queries Room again because debounce was cleared
        viewModel.onBarcodeScanned(unmappedBarcode)
        advanceUntilIdle()
        assertEquals("Calling onClearPendingItem resets lastScannedRaw, allowing immediate retry", 2, barcodeDao.queryCount)
    }
}
