package com.elsasa.bgud.viewmodel

import android.content.Context
import android.content.ContextWrapper
import androidx.work.Data
import androidx.work.WorkInfo
import androidx.work.workDataOf
import com.elsasa.bgud.dao.BarcodeRegistrationRequestDao
import com.elsasa.bgud.datastore.SessionPreferencesDataSource
import com.elsasa.bgud.model.BarcodeRegistrationRequestEntity
import com.elsasa.bgud.sync.BarcodeSyncWorker
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.emptyFlow
import org.junit.Assert.assertEquals
import org.junit.Assert.assertNotNull
import org.junit.Assert.assertNull
import org.junit.Test
import java.io.File
import java.nio.file.Files
import java.util.UUID

class SynchronizationViewModelTest {

    private fun createTestContext(): Context {
        val tempDir: File = Files.createTempDirectory("sync_vm_test").toFile()
        return object : ContextWrapper(null) {
            override fun getApplicationContext(): Context = this
            override fun getFilesDir(): File = tempDir
            override fun getDataDir(): File = tempDir
        }
    }

    private class FakeBarcodeRegistrationRequestDao : BarcodeRegistrationRequestDao {
        override suspend fun getById(clientRequestId: String): BarcodeRegistrationRequestEntity? = null
        override fun listByStatus(status: String): Flow<List<BarcodeRegistrationRequestEntity>> = emptyFlow()
        override fun pendingCount(): Flow<Int> = emptyFlow()
        override fun syncedCount(): Flow<Int> = emptyFlow()
        override fun rejectedCount(): Flow<Int> = emptyFlow()
        override suspend fun enqueue(request: BarcodeRegistrationRequestEntity) {}
        override suspend fun updateOutcome(clientRequestId: String, status: String, serverNote: String) {}
        override suspend fun deleteAll() {}
    }

    private fun createViewModel(baseUrl: String = ""): SynchronizationViewModel {
        val context = createTestContext()
        val session = SessionPreferencesDataSource(context)
        val dao = FakeBarcodeRegistrationRequestDao()
        return SynchronizationViewModel(
            session = session,
            requestDao = dao,
            connectivityManager = null,
            baseUrl = baseUrl
        )
    }

    @Test
    fun parseBarcodeSyncSummaryFromOutputData_parsesAllFields() {
        val data = workDataOf(
            BarcodeSyncWorker.OUTPUT_SUBMITTED to 5,
            BarcodeSyncWorker.OUTPUT_SUBMIT_SKIPPED to 1,
            BarcodeSyncWorker.OUTPUT_SUBMIT_FAILED to 2,
            BarcodeSyncWorker.OUTPUT_BARCODE_COUNT to 10,
            BarcodeSyncWorker.OUTPUT_BARANG_COUNT to 20,
            BarcodeSyncWorker.OUTPUT_SYNCED to 5,
            BarcodeSyncWorker.OUTPUT_REJECTED to 1,
            BarcodeSyncWorker.OUTPUT_ERRORS to "Some warning"
        )

        val summary = BarcodeSyncSummary.fromOutputData(data)

        assertEquals(5, summary.submitted)
        assertEquals(1, summary.submitSkipped)
        assertEquals(2, summary.submitFailed)
        assertEquals(10, summary.barcodeCount)
        assertEquals(20, summary.barangCount)
        assertEquals(5, summary.synced)
        assertEquals(1, summary.rejected)
        assertEquals("Some warning", summary.errors)
    }

    @Test
    fun parseBarcodeSyncSummaryFromOutputData_emptyData_fallsBackToDefaults() {
        val summary = BarcodeSyncSummary.fromOutputData(Data.EMPTY)

        assertEquals(0, summary.submitted)
        assertEquals(0, summary.submitSkipped)
        assertEquals(0, summary.submitFailed)
        assertEquals(0, summary.barcodeCount)
        assertEquals(0, summary.barangCount)
        assertEquals(0, summary.synced)
        assertEquals(0, summary.rejected)
        assertEquals("", summary.errors)
    }

    @Test
    fun handleWorkInfo_succeeded_mapsOutputDataToSyncSummary() {
        val viewModel = createViewModel()
        val outputData = workDataOf(
            BarcodeSyncWorker.OUTPUT_SUBMITTED to 7,
            BarcodeSyncWorker.OUTPUT_SUBMIT_SKIPPED to 0,
            BarcodeSyncWorker.OUTPUT_SUBMIT_FAILED to 0,
            BarcodeSyncWorker.OUTPUT_BARCODE_COUNT to 14,
            BarcodeSyncWorker.OUTPUT_BARANG_COUNT to 28,
            BarcodeSyncWorker.OUTPUT_SYNCED to 7,
            BarcodeSyncWorker.OUTPUT_REJECTED to 0,
            BarcodeSyncWorker.OUTPUT_ERRORS to ""
        )
        val workInfo = WorkInfo(
            UUID.randomUUID(),
            WorkInfo.State.SUCCEEDED,
            emptySet(),
            outputData
        )

        viewModel.handleWorkInfo(workInfo)

        val summary = viewModel.syncSummary.value
        assertNotNull(summary)
        assertEquals(7, summary?.submitted)
        assertEquals(0, summary?.submitSkipped)
        assertEquals(0, summary?.submitFailed)
        assertEquals(14, summary?.barcodeCount)
        assertEquals(28, summary?.barangCount)
        assertEquals(7, summary?.synced)
        assertEquals(0, summary?.rejected)
        assertEquals("", summary?.errors)
        assertEquals(SyncState.SYNCHRONIZED, viewModel.syncState.value)
    }

    @Test
    fun handleWorkInfo_failed_mapsOutputDataToSyncSummary() {
        val viewModel = createViewModel()
        val outputData = workDataOf(
            BarcodeSyncWorker.OUTPUT_SUBMITTED to 3,
            BarcodeSyncWorker.OUTPUT_SUBMIT_SKIPPED to 1,
            BarcodeSyncWorker.OUTPUT_SUBMIT_FAILED to 4,
            BarcodeSyncWorker.OUTPUT_BARCODE_COUNT to 0,
            BarcodeSyncWorker.OUTPUT_BARANG_COUNT to 0,
            BarcodeSyncWorker.OUTPUT_SYNCED to 0,
            BarcodeSyncWorker.OUTPUT_REJECTED to 2,
            BarcodeSyncWorker.OUTPUT_ERRORS to "Cloud 500 error"
        )
        val workInfo = WorkInfo(
            UUID.randomUUID(),
            WorkInfo.State.FAILED,
            emptySet(),
            outputData
        )

        viewModel.handleWorkInfo(workInfo)

        val summary = viewModel.syncSummary.value
        assertNotNull(summary)
        assertEquals(3, summary?.submitted)
        assertEquals(1, summary?.submitSkipped)
        assertEquals(4, summary?.submitFailed)
        assertEquals(0, summary?.barcodeCount)
        assertEquals(0, summary?.barangCount)
        assertEquals(0, summary?.synced)
        assertEquals(2, summary?.rejected)
        assertEquals("Cloud 500 error", summary?.errors)
        assertEquals(SyncState.FAILED, viewModel.syncState.value)
    }

    @Test
    fun syncNow_resetsSyncSummaryToNull() {
        val viewModel = createViewModel()
        val outputData = workDataOf(
            BarcodeSyncWorker.OUTPUT_SUBMITTED to 10,
            BarcodeSyncWorker.OUTPUT_BARANG_COUNT to 20
        )
        val workInfo = WorkInfo(
            UUID.randomUUID(),
            WorkInfo.State.SUCCEEDED,
            emptySet(),
            outputData
        )

        // Populate summary first
        viewModel.handleWorkInfo(workInfo)
        assertNotNull(viewModel.syncSummary.value)

        // Make online and trigger syncNow
        viewModel.setOnlineForTesting(true)
        val context = createTestContext()
        viewModel.syncNow(context)

        // Verify summary is reset to null
        assertNull(viewModel.syncSummary.value)
    }

    @Test
    fun syncNow_whenOffline_doesNotResetSyncSummary() {
        val viewModel = createViewModel()
        val outputData = workDataOf(
            BarcodeSyncWorker.OUTPUT_SUBMITTED to 10
        )
        val workInfo = WorkInfo(
            UUID.randomUUID(),
            WorkInfo.State.SUCCEEDED,
            emptySet(),
            outputData
        )

        viewModel.handleWorkInfo(workInfo)
        assertNotNull(viewModel.syncSummary.value)

        // Offline ensures syncNow is ignored
        viewModel.setOnlineForTesting(false)
        val context = createTestContext()
        viewModel.syncNow(context)

        // Summary remains unchanged
        assertNotNull(viewModel.syncSummary.value)
        assertEquals(10, viewModel.syncSummary.value?.submitted)
    }
}
