package com.elsasa.bgud.database

import android.content.Context
import androidx.room.Database
import androidx.room.Room
import androidx.room.RoomDatabase
import com.elsasa.bgud.dao.BarcodeDao
import com.elsasa.bgud.dao.BarangDao
import com.elsasa.bgud.dao.BarcodeRegistrationRequestDao
import com.elsasa.bgud.model.BarcodeEntity
import com.elsasa.bgud.model.BarangEntity
import com.elsasa.bgud.model.BarcodeRegistrationRequestEntity

/**
 * BGud local store (Architecture §6.4).
 *
 * - `barcode_entity`: Active barcode cache for the bound Office.
 * - `barang_entity`: Barang reference cache (ADR-005).
 * - `barcode_registration_request_entity`: offline intent queue (P-08).
 *
 * Indexes (mandated): `barcode_entity(barcodeValueKey)` unique,
 * `barcode_entity(brgId)`, `barcode_registration_request_entity(status)`.
 */
@Database(
    entities = [
        BarcodeEntity::class,
        BarangEntity::class,
        BarcodeRegistrationRequestEntity::class
    ],
    version = 1,
    exportSchema = false
)
abstract class AppDatabase : RoomDatabase() {
    abstract fun barcodeDao(): BarcodeDao
    abstract fun barangDao(): BarangDao
    abstract fun barcodeRegistrationRequestDao(): BarcodeRegistrationRequestDao

    companion object {
        @Volatile
        private var INSTANCE: AppDatabase? = null

        fun getDatabase(context: Context): AppDatabase {
            return INSTANCE ?: synchronized(this) {
                val instance = Room.databaseBuilder(
                    context.applicationContext,
                    AppDatabase::class.java,
                    "bgud_database"
                ).build()
                INSTANCE = instance
                instance
            }
        }
    }
}
