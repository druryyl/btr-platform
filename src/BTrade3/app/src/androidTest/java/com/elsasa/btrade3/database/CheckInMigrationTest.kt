package com.elsasa.btrade3.database

import androidx.room.Room
import androidx.room.testing.MigrationTestHelper
import androidx.sqlite.db.framework.FrameworkSQLiteOpenHelperFactory
import androidx.test.ext.junit.runners.AndroidJUnit4
import androidx.test.platform.app.InstrumentationRegistry
import com.elsasa.btrade3.model.CheckIn
import com.elsasa.btrade3.repository.CheckInRepository
import kotlinx.coroutines.runBlocking
import org.junit.Assert.assertEquals
import org.junit.Assert.assertFalse
import org.junit.Assert.assertNull
import org.junit.Assert.assertTrue
import org.junit.Rule
import org.junit.Test
import org.junit.runner.RunWith

@RunWith(AndroidJUnit4::class)
class CheckInMigrationTest {
    private val testDb = "checkin-migration-test"

    @get:Rule
    val helper = MigrationTestHelper(
        InstrumentationRegistry.getInstrumentation(),
        AppDatabase::class.java,
        emptyList(),
        FrameworkSQLiteOpenHelperFactory()
    )

    @Test
    fun migrate26To27_legacyRecordsExcludedFromActiveVisit() = runBlocking {
        val userEmail = "sales@example.com"

        helper.createDatabase(testDb, 26).apply {
            execSQL(
                """
                INSERT INTO checkin_table (
                    checkInId, checkInDate, checkInTime, userEmail,
                    checkInLatitude, checkInLongitude, accuracy,
                    customerId, customerCode, customerName, customerAddress,
                    customerLatitude, customerLongitude, statusSync,
                    checkOutTime, checkOutLatitude, checkOutLongitude, checkOutAccuracy, checkOutMode
                ) VALUES (
                    'legacy-a', '2026-01-10', '09:00:00', '$userEmail',
                    -6.2, 106.8, 12.0,
                    'C1', 'C001', 'Customer A', 'Addr A',
                    -6.2, 106.8, 'SENT',
                    '', 0.0, 0.0, 0.0, ''
                )
                """.trimIndent()
            )
            execSQL(
                """
                INSERT INTO checkin_table (
                    checkInId, checkInDate, checkInTime, userEmail,
                    checkInLatitude, checkInLongitude, accuracy,
                    customerId, customerCode, customerName, customerAddress,
                    customerLatitude, customerLongitude, statusSync,
                    checkOutTime, checkOutLatitude, checkOutLongitude, checkOutAccuracy, checkOutMode
                ) VALUES (
                    'legacy-b', '2026-02-15', '10:00:00', '$userEmail',
                    -6.2, 106.8, 12.0,
                    'C2', 'C002', 'Customer B', 'Addr B',
                    -6.2, 106.8, 'SENT',
                    '', 0.0, 0.0, 0.0, ''
                )
                """.trimIndent()
            )
            close()
        }

        helper.runMigrationsAndValidate(testDb, 27, true, AppDatabase.MIGRATION_26_27)

        val context = InstrumentationRegistry.getInstrumentation().targetContext
        val roomDb = Room.databaseBuilder(context, AppDatabase::class.java, testDb)
            .build()
        val repository = CheckInRepository(roomDb.checkInDao())

        assertNull(repository.getOpenCheckInForUser(userEmail))

        val legacyA = roomDb.checkInDao().getCheckInById("legacy-a")!!
        assertFalse(legacyA.isExplicitlyOpen)
        assertFalse(legacyA.isOpenVisit())

        val activeCheckIn = CheckIn(
            checkInId = "active-c",
            checkInDate = "2026-06-19",
            checkInTime = "14:00:00",
            userEmail = userEmail,
            checkInLatitude = -6.2,
            checkInLongitude = 106.8,
            accuracy = 10f,
            customerId = "C3",
            customerCode = "C003",
            customerName = "Customer C",
            customerAddress = "Addr C",
            customerLatitude = -6.2,
            customerLongitude = 106.8,
            statusSync = "DRAFT",
            isExplicitlyOpen = true
        )
        repository.insertCheckIn(activeCheckIn)

        val openVisit = repository.getOpenCheckInForUser(userEmail)
        assertEquals("active-c", openVisit?.checkInId)
        assertTrue(openVisit!!.isOpenVisit())

        val closed = repository.manualCheckOut(openVisit)
        assertFalse(closed.isOpenVisit())
        assertNull(repository.getOpenCheckInForUser(userEmail))

        roomDb.close()
    }
}
