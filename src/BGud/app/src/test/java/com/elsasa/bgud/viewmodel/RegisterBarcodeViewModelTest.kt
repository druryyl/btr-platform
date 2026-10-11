package com.elsasa.bgud.viewmodel

import android.content.Context
import android.content.ContextWrapper
import com.elsasa.bgud.dao.BarangDao
import com.elsasa.bgud.dao.BarcodeDao
import com.elsasa.bgud.dao.BarcodeRegistrationRequestDao
import com.elsasa.bgud.datastore.SessionPreferencesDataSource
import com.elsasa.bgud.model.BarangEntity
import com.elsasa.bgud.model.BarcodeEntity
import com.elsasa.bgud.model.BarcodeRegistrationRequestEntity
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
import org.junit.Assert.assertFalse
import org.junit.Assert.assertNotNull
import org.junit.Assert.assertNull
import org.junit.Assert.assertTrue
import org.junit.Before
import org.junit.Test
import java.io.File
import java.nio.file.Files

@OptIn(ExperimentalCoroutinesApi::class)
class RegisterBarcodeViewModelTest {

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
        val tempDir: File = Files.createTempDirectory("reg_bc_vm_test").toFile()
        return object : ContextWrapper(null) {
            override fun getApplicationContext(): Context = this
            override fun getFilesDir(): File = tempDir
            override fun getDataDir(): File = tempDir
        }
    }

    private class FakeBarcodeDao(
        private val barcodes: MutableMap<String, BarcodeEntity> = mutableMapOf()
    ) : BarcodeDao {
        override suspend fun getByKey(barcodeValueKey: String): BarcodeEntity? = barcodes[barcodeValueKey]
        override suspend fun getById(brgBarcodeId: String): BarcodeEntity? =
            barcodes.values.firstOrNull { it.brgBarcodeId == brgBarcodeId }
        override fun listByBrg(brgId: String): Flow<List<BarcodeEntity>> = emptyFlow()
        override suspend fun getUnitsByBrg(brgId: String): List<String> =
            barcodes.values.filter { it.brgId == brgId && it.satuan.isNotBlank() }.map { it.satuan }.distinct()
        override suspend fun paged(limit: Int, offset: Int): List<BarcodeEntity> = emptyList()
        override suspend fun search(query: String, limit: Int, offset: Int): List<BarcodeEntity> = emptyList()
        override suspend fun upsert(barcode: BarcodeEntity) {
            barcodes[barcode.barcodeValueKey] = barcode
        }
        override suspend fun upsertAll(barcodes: List<BarcodeEntity>) {
            barcodes.forEach { upsert(it) }
        }
        override suspend fun deleteById(brgBarcodeId: String) {
            barcodes.values.removeIf { it.brgBarcodeId == brgBarcodeId }
        }
        override suspend fun deleteAll() {
            barcodes.clear()
        }
    }

    private class FakeBarangDao(
        private val items: MutableMap<String, BarangEntity> = mutableMapOf()
    ) : BarangDao {
        override suspend fun getById(brgId: String): BarangEntity? = items[brgId]
        override suspend fun search(query: String): List<BarangEntity> =
            items.values.filter { it.brgCode.contains(query, ignoreCase = true) || it.brgName.contains(query, ignoreCase = true) }
        override fun getAll(): Flow<List<BarangEntity>> = emptyFlow()
        override suspend fun upsert(barang: BarangEntity) {
            items[barang.brgId] = barang
        }
        override suspend fun upsertAll(barangs: List<BarangEntity>) {
            barangs.forEach { upsert(it) }
        }
        override suspend fun deleteAll() {
            items.clear()
        }
    }

    private class FakeBarcodeRegistrationRequestDao : BarcodeRegistrationRequestDao {
        val enqueued = mutableListOf<BarcodeRegistrationRequestEntity>()

        override suspend fun getById(clientRequestId: String): BarcodeRegistrationRequestEntity? =
            enqueued.firstOrNull { it.clientRequestId == clientRequestId }
        override fun listByStatus(status: String): Flow<List<BarcodeRegistrationRequestEntity>> = emptyFlow()
        override fun pendingCount(): Flow<Int> = emptyFlow()
        override fun syncedCount(): Flow<Int> = emptyFlow()
        override fun rejectedCount(): Flow<Int> = emptyFlow()
        override suspend fun enqueue(request: BarcodeRegistrationRequestEntity) {
            enqueued.add(request)
        }
        override suspend fun updateOutcome(clientRequestId: String, status: String, serverNote: String) {}
        override suspend fun deleteAll() {
            enqueued.clear()
        }
    }

    private fun createViewModel(
        barcodeDao: FakeBarcodeDao = FakeBarcodeDao(),
        barangDao: FakeBarangDao = FakeBarangDao(),
        requestDao: FakeBarcodeRegistrationRequestDao = FakeBarcodeRegistrationRequestDao(),
        initialBarcode: String = "",
        locationId: String = "WHS-TEST"
    ): Triple<RegisterBarcodeViewModel, SessionPreferencesDataSource, FakeBarcodeRegistrationRequestDao> {
        val context = createTestContext()
        val session = SessionPreferencesDataSource(context)
        kotlinx.coroutines.runBlocking {
            session.saveSession(
                googleEmail = "operator@example.com",
                userId = "USR-01",
                userName = "Operator 1",
                roleId = "WHS",
                locationId = locationId,
                serverId = "SRV-01"
            )
        }

        val viewModel = RegisterBarcodeViewModel(
            barcodeDao = barcodeDao,
            barangDao = barangDao,
            requestDao = requestDao,
            session = session,
            initialBarcode = initialBarcode
        )
        return Triple(viewModel, session, requestDao)
    }

    @Test
    fun initialBlankBarcodeState_startsEmptyWithNoDuplicate() = runTest(testDispatcher) {
        val (viewModel, _, _) = createViewModel(initialBarcode = "")
        advanceUntilIdle()

        assertEquals("", viewModel.barcodeValue.value)
        assertNull(viewModel.duplicateFound.value)
        assertEquals("", viewModel.itemQuery.value)
        assertTrue(viewModel.itemResults.value.isEmpty())
        assertNull(viewModel.selectedItem.value)
        assertEquals("", viewModel.selectedUnit.value)
        assertNull(viewModel.saveError.value)
        assertFalse(viewModel.saved.value)
        assertFalse(viewModel.isSaving.value)
    }

    @Test
    fun initialPreFilledBarcode_normalizesAndProbesDuplicate() = runTest(testDispatcher) {
        val duplicateBarcode = BarcodeEntity(
            brgBarcodeId = "BC-01",
            barcodeValue = "8991234567890",
            barcodeValueKey = "8991234567890",
            brgId = "BRG-01",
            brgCode = "CODE01",
            brgName = "Sample Item",
            satuan = "PCS"
        )
        val barcodeDao = FakeBarcodeDao(mutableMapOf("8991234567890" to duplicateBarcode))

        val (viewModel, _, _) = createViewModel(
            barcodeDao = barcodeDao,
            initialBarcode = "  8991234567890 \r\n"
        )
        advanceUntilIdle()

        assertEquals("8991234567890", viewModel.barcodeValue.value)
        assertNotNull(viewModel.duplicateFound.value)
        assertEquals("BC-01", viewModel.duplicateFound.value?.brgBarcodeId)
    }

    @Test
    fun onBarcodeScanned_normalizesInputAndDetectsDuplicate() = runTest(testDispatcher) {
        val duplicateBarcode = BarcodeEntity(
            brgBarcodeId = "BC-DUP",
            barcodeValue = "8991111111111",
            barcodeValueKey = "8991111111111",
            brgId = "BRG-01",
            brgCode = "CODE01",
            brgName = "Duplicate Item",
            satuan = "PCS"
        )
        val barcodeDao = FakeBarcodeDao(mutableMapOf("8991111111111" to duplicateBarcode))

        val (viewModel, _, _) = createViewModel(barcodeDao = barcodeDao)
        advanceUntilIdle()

        // Scan barcode with whitespace and tabs
        viewModel.onBarcodeScanned(" \t8991111111111\n ")
        advanceUntilIdle()

        assertEquals("8991111111111", viewModel.barcodeValue.value)
        assertNotNull(viewModel.duplicateFound.value)
        assertEquals("Duplicate Item", viewModel.duplicateFound.value?.brgName)
        assertNull(viewModel.saveError.value)

        // Scan unique barcode
        viewModel.onBarcodeScanned("8992222222222")
        advanceUntilIdle()

        assertEquals("8992222222222", viewModel.barcodeValue.value)
        assertNull(viewModel.duplicateFound.value)
    }

    @Test
    fun onBarcodeManualSubmit_normalizesInputAndDetectsDuplicate() = runTest(testDispatcher) {
        val duplicateBarcode = BarcodeEntity(
            brgBarcodeId = "BC-MAN",
            barcodeValue = "8993333333333",
            barcodeValueKey = "8993333333333",
            brgId = "BRG-02",
            brgCode = "CODE02",
            brgName = "Manual Duplicate Item",
            satuan = "DUS"
        )
        val barcodeDao = FakeBarcodeDao(mutableMapOf("8993333333333" to duplicateBarcode))

        val (viewModel, _, _) = createViewModel(barcodeDao = barcodeDao)
        advanceUntilIdle()

        viewModel.onBarcodeManualSubmit("  8993333333333  ")
        advanceUntilIdle()

        assertEquals("8993333333333", viewModel.barcodeValue.value)
        assertEquals("Manual Duplicate Item", viewModel.duplicateFound.value?.brgName)
    }

    @Test
    fun onClearBarcode_resetsBarcodeDuplicateAndSaveError() = runTest(testDispatcher) {
        val duplicateBarcode = BarcodeEntity(
            brgBarcodeId = "BC-01",
            barcodeValue = "8991111111111",
            barcodeValueKey = "8991111111111",
            brgId = "BRG-01",
            brgCode = "CODE01",
            brgName = "Sample Item",
            satuan = "PCS"
        )
        val barcodeDao = FakeBarcodeDao(mutableMapOf("8991111111111" to duplicateBarcode))

        val (viewModel, _, _) = createViewModel(barcodeDao = barcodeDao)
        advanceUntilIdle()

        viewModel.onBarcodeScanned("8991111111111")
        advanceUntilIdle()
        assertNotNull(viewModel.duplicateFound.value)

        // Trigger a save error
        viewModel.save()
        advanceUntilIdle()
        assertEquals("Item wajib dipilih.", viewModel.saveError.value)

        // Clear barcode
        viewModel.onClearBarcode()
        advanceUntilIdle()

        assertEquals("", viewModel.barcodeValue.value)
        assertNull(viewModel.duplicateFound.value)
        assertNull(viewModel.saveError.value)
    }

    @Test
    fun save_failsWhenBarcodeIsBlank() = runTest(testDispatcher) {
        val item = BarangEntity(
            brgId = "BRG-01",
            brgCode = "B01",
            brgName = "Active Item",
            isAktif = true,
            satKecil = "PCS",
            satBesar = "DUS"
        )
        val barangDao = FakeBarangDao(mutableMapOf("BRG-01" to item))
        val (viewModel, _, requestDao) = createViewModel(barangDao = barangDao, initialBarcode = "")
        advanceUntilIdle()

        viewModel.onSelectItem(item)
        viewModel.save()
        advanceUntilIdle()

        assertEquals("Barcode wajib diisi.", viewModel.saveError.value)
        assertFalse(viewModel.saved.value)
        assertTrue(requestDao.enqueued.isEmpty())
    }

    @Test
    fun save_failsWhenItemNotSelected() = runTest(testDispatcher) {
        val (viewModel, _, requestDao) = createViewModel(initialBarcode = "")
        advanceUntilIdle()

        viewModel.onBarcodeScanned("8999999999999")
        advanceUntilIdle()

        viewModel.save()
        advanceUntilIdle()

        assertEquals("Item wajib dipilih.", viewModel.saveError.value)
        assertFalse(viewModel.saved.value)
        assertTrue(requestDao.enqueued.isEmpty())
    }

    @Test
    fun save_failsWhenSelectedItemIsInactive() = runTest(testDispatcher) {
        val inactiveItem = BarangEntity(
            brgId = "BRG-INACTIVE",
            brgCode = "BINACT",
            brgName = "Inactive Item",
            isAktif = false,
            satKecil = "PCS",
            satBesar = "DUS"
        )
        val barangDao = FakeBarangDao(mutableMapOf("BRG-INACTIVE" to inactiveItem))
        val (viewModel, _, requestDao) = createViewModel(barangDao = barangDao)
        advanceUntilIdle()

        viewModel.onBarcodeScanned("8999999999999")
        viewModel.onSelectItem(inactiveItem)
        advanceUntilIdle()

        viewModel.save()
        advanceUntilIdle()

        assertEquals("Item sudah tidak aktif.", viewModel.saveError.value)
        assertFalse(viewModel.saved.value)
        assertTrue(requestDao.enqueued.isEmpty())
    }

    @Test
    fun save_failsWhenBarcodeIsDuplicateAtSaveTime() = runTest(testDispatcher) {
        val duplicateBarcode = BarcodeEntity(
            brgBarcodeId = "BC-EXISTING",
            barcodeValue = "8991234567890",
            barcodeValueKey = "8991234567890",
            brgId = "BRG-01",
            brgCode = "CODE01",
            brgName = "Existing Barcode Item",
            satuan = "PCS"
        )
        val item = BarangEntity(
            brgId = "BRG-01",
            brgCode = "CODE01",
            brgName = "Existing Barcode Item",
            isAktif = true,
            satKecil = "PCS",
            satBesar = "DUS"
        )
        val barcodeDao = FakeBarcodeDao(mutableMapOf("8991234567890" to duplicateBarcode))
        val barangDao = FakeBarangDao(mutableMapOf("BRG-01" to item))

        val (viewModel, _, requestDao) = createViewModel(barcodeDao = barcodeDao, barangDao = barangDao)
        advanceUntilIdle()

        viewModel.onBarcodeScanned("8991234567890")
        viewModel.onSelectItem(item)
        advanceUntilIdle()

        viewModel.save()
        advanceUntilIdle()

        assertEquals("Barcode sudah terdaftar.", viewModel.saveError.value)
        assertFalse(viewModel.saved.value)
        assertTrue(requestDao.enqueued.isEmpty())
    }

    @Test
    fun save_succeedsWhenAllPreconditionsMet_enqueuesPendingRequest() = runTest(testDispatcher) {
        val item = BarangEntity(
            brgId = "BRG-OK",
            brgCode = "BOK",
            brgName = "Valid Item",
            isAktif = true,
            satKecil = "PCS",
            satBesar = "DUS"
        )
        val barangDao = FakeBarangDao(mutableMapOf("BRG-OK" to item))
        val (viewModel, _, requestDao) = createViewModel(
            barangDao = barangDao,
            locationId = "WHS-JOGJA"
        )
        advanceUntilIdle()

        viewModel.onBarcodeScanned("8998888888888")
        viewModel.onSelectItem(item)
        viewModel.onUnitChange("PCS")
        advanceUntilIdle()

        viewModel.save()
        advanceUntilIdle()

        val deadline = System.currentTimeMillis() + 3000
        while (viewModel.isSaving.value && System.currentTimeMillis() < deadline) {
            Thread.sleep(50)
            advanceUntilIdle()
        }

        assertTrue(
            "Expected saved=true, but saved=${viewModel.saved.value}, isSaving=${viewModel.isSaving.value}, saveError='${viewModel.saveError.value}'",
            viewModel.saved.value
        )
        assertNull(viewModel.saveError.value)
        assertFalse(viewModel.isSaving.value)

        assertEquals(1, requestDao.enqueued.size)
        val enqueued = requestDao.enqueued[0]
        assertEquals("8998888888888", enqueued.barcodeValue)
        assertEquals("BRG-OK", enqueued.brgId)
        assertEquals("PCS", enqueued.satuan)
        assertEquals(BarcodeRegistrationRequestEntity.STATUS_PENDING, enqueued.status)
        assertEquals("WHS-JOGJA", enqueued.warehouseCode)
        assertTrue(enqueued.createdAt > 0L)
    }

    @Test
    fun unitOptions_returnsDistinctNonBlankUnits() = runTest(testDispatcher) {
        val item = BarangEntity(
            brgId = "BRG-01",
            brgCode = "B01",
            brgName = "Item With Units",
            isAktif = true,
            satKecil = "PCS",
            satBesar = "BOX"
        )
        val barangDao = FakeBarangDao(mutableMapOf("BRG-01" to item))
        val (viewModel, _, _) = createViewModel(barangDao = barangDao)
        advanceUntilIdle()

        assertTrue(viewModel.unitOptions().isEmpty())

        viewModel.onSelectItem(item)
        val options = viewModel.unitOptions()
        assertEquals(listOf("PCS", "BOX"), options)

        viewModel.onClearSelection()
        assertTrue(viewModel.unitOptions().isEmpty())
    }
}
