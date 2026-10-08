package com.elsasa.bgud.viewmodel

import android.content.Context
import android.content.ContextWrapper
import androidx.work.Data
import androidx.work.WorkInfo
import androidx.work.workDataOf
import com.elsasa.bgud.dao.ReturnOrderDao
import com.elsasa.bgud.datastore.SessionPreferencesDataSource
import com.elsasa.bgud.model.ReturnOrderEntity
import com.elsasa.bgud.sync.ReturnOrderSyncWorker
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.emptyFlow
import org.junit.Assert.assertEquals
import org.junit.Assert.assertNotNull
import org.junit.Assert.assertNull
import org.junit.Test
import java.io.File
import java.nio.file.Files
import java.util.UUID

class ReturnOrderSyncViewModelTest {

    private fun createTestContext(): Context {
        val tempDir: File = Files.createTempDirectory("ro_sync_vm_test").toFile()
        return object : ContextWrapper(null) {
            override fun getApplicationContext(): Context = this
            override fun getFilesDir(): File = tempDir
            override fun getDataDir(): File = tempDir
        }
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

    private fun createViewModel(baseUrl: String = ""): ReturnOrderSyncViewModel {
        val context = createTestContext()
        val session = SessionPreferencesDataSource(context)
        val dao = FakeReturnOrderDao()
        return ReturnOrderSyncViewModel(
            session = session,
            returnOrderDao = dao,
            connectivityManager = null,
            baseUrl = baseUrl
        )
    }

    @Test
    fun parseReturnOrderSyncSummaryFromOutputData_parsesAllFields() {
        val data = workDataOf(
            ReturnOrderSyncWorker.OUTPUT_SUBMITTED to 8,
            ReturnOrderSyncWorker.OUTPUT_SUBMIT_FAILED to 2,
            ReturnOrderSyncWorker.OUTPUT_CUSTOMER_COUNT to 25,
            ReturnOrderSyncWorker.OUTPUT_SALESPERSON_COUNT to 10,
            ReturnOrderSyncWorker.OUTPUT_DRIVER_COUNT to 5,
            ReturnOrderSyncWorker.OUTPUT_ERRORS to "Connection timeout on customer reference"
        )

        val summary = ReturnOrderSyncSummary.fromOutputData(data)

        assertEquals(8, summary.submitted)
        assertEquals(2, summary.submitFailed)
        assertEquals(25, summary.customerCount)
        assertEquals(10, summary.salesPersonCount)
        assertEquals(5, summary.driverCount)
        assertEquals("Connection timeout on customer reference", summary.errors)
    }

    @Test
    fun parseReturnOrderSyncSummaryFromOutputData_emptyData_fallsBackToDefaults() {
        val summary = ReturnOrderSyncSummary.fromOutputData(Data.EMPTY)

        assertEquals(0, summary.submitted)
        assertEquals(0, summary.submitFailed)
        assertEquals(0, summary.customerCount)
        assertEquals(0, summary.salesPersonCount)
        assertEquals(0, summary.driverCount)
        assertEquals("", summary.errors)
    }

    @Test
    fun handleWorkInfo_succeeded_mapsOutputDataToSyncSummary() {
        val viewModel = createViewModel()
        val outputData = workDataOf(
            ReturnOrderSyncWorker.OUTPUT_SUBMITTED to 4,
            ReturnOrderSyncWorker.OUTPUT_SUBMIT_FAILED to 0,
            ReturnOrderSyncWorker.OUTPUT_CUSTOMER_COUNT to 15,
            ReturnOrderSyncWorker.OUTPUT_SALESPERSON_COUNT to 5,
            ReturnOrderSyncWorker.OUTPUT_DRIVER_COUNT to 3,
            ReturnOrderSyncWorker.OUTPUT_ERRORS to ""
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
        assertEquals(4, summary?.submitted)
        assertEquals(0, summary?.submitFailed)
        assertEquals(15, summary?.customerCount)
        assertEquals(5, summary?.salesPersonCount)
        assertEquals(3, summary?.driverCount)
        assertEquals("", summary?.errors)
        assertEquals(SyncState.SYNCHRONIZED, viewModel.syncState.value)
    }

    @Test
    fun handleWorkInfo_failed_mapsOutputDataToSyncSummary() {
        val viewModel = createViewModel()
        val outputData = workDataOf(
            ReturnOrderSyncWorker.OUTPUT_SUBMITTED to 1,
            ReturnOrderSyncWorker.OUTPUT_SUBMIT_FAILED to 3,
            ReturnOrderSyncWorker.OUTPUT_CUSTOMER_COUNT to 0,
            ReturnOrderSyncWorker.OUTPUT_SALESPERSON_COUNT to 0,
            ReturnOrderSyncWorker.OUTPUT_DRIVER_COUNT to 0,
            ReturnOrderSyncWorker.OUTPUT_ERRORS to "Server 503 unavailable"
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
        assertEquals(1, summary?.submitted)
        assertEquals(3, summary?.submitFailed)
        assertEquals(0, summary?.customerCount)
        assertEquals(0, summary?.salesPersonCount)
        assertEquals(0, summary?.driverCount)
        assertEquals("Server 503 unavailable", summary?.errors)
        assertEquals(SyncState.FAILED, viewModel.syncState.value)
    }

    @Test
    fun syncNow_resetsSyncSummaryToNull() {
        val viewModel = createViewModel()
        val outputData = workDataOf(
            ReturnOrderSyncWorker.OUTPUT_SUBMITTED to 5,
            ReturnOrderSyncWorker.OUTPUT_CUSTOMER_COUNT to 10
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
            ReturnOrderSyncWorker.OUTPUT_SUBMITTED to 5
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
        assertEquals(5, viewModel.syncSummary.value?.submitted)
    }
}
