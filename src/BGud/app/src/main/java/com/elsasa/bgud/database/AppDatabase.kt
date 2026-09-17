package com.elsasa.bgud.database

import android.content.Context
import androidx.room.Database
import androidx.room.Room
import androidx.room.RoomDatabase
import androidx.room.migration.Migration
import androidx.sqlite.db.SupportSQLiteDatabase
import com.elsasa.bgud.dao.BarcodeDao
import com.elsasa.bgud.dao.BarangDao
import com.elsasa.bgud.dao.BarcodeRegistrationRequestDao
import com.elsasa.bgud.dao.CustomerDao
import com.elsasa.bgud.dao.DriverDao
import com.elsasa.bgud.dao.ReturnOrderDao
import com.elsasa.bgud.dao.ReturnOrderItemDao
import com.elsasa.bgud.dao.SalesPersonDao
import com.elsasa.bgud.model.BarcodeEntity
import com.elsasa.bgud.model.BarangEntity
import com.elsasa.bgud.model.BarcodeRegistrationRequestEntity
import com.elsasa.bgud.model.CustomerEntity
import com.elsasa.bgud.model.DriverEntity
import com.elsasa.bgud.model.ReturnOrderEntity
import com.elsasa.bgud.model.ReturnOrderItemEntity
import com.elsasa.bgud.model.SalesPersonEntity

/**
 * BGud local store (Architecture §6.4).
 *
 * - `barcode_entity`: Active barcode cache for the bound Office.
 * - `barang_entity`: Barang reference cache (ADR-005).
 * - `barcode_registration_request_entity`: offline intent queue (P-08).
 * - `return_order_entity`: Offline Return Order capture (DRAFT | SYNCED).
 * - `return_order_item_entity`: Return Order line items.
 * - `customer_entity`: Mandatory Customer reference cache (GAP-004).
 * - `salesperson_entity`: Optional Salesman reference cache (ADR-RO-005).
 * - `driver_entity`: Optional Driver reference cache (ADR-RO-005).
 *
 * Indexes (mandated): `barcode_entity(barcodeValueKey)` unique,
 * `barcode_entity(brgId)`, `barcode_registration_request_entity(status)`,
 * `return_order_entity(status)`, `return_order_entity(customerName)`.
 */
@Database(
    entities = [
        BarcodeEntity::class,
        BarangEntity::class,
        BarcodeRegistrationRequestEntity::class,
        ReturnOrderEntity::class,
        ReturnOrderItemEntity::class,
        CustomerEntity::class,
        SalesPersonEntity::class,
        DriverEntity::class
    ],
    version = 2,
    exportSchema = false
)
abstract class AppDatabase : RoomDatabase() {
    abstract fun barcodeDao(): BarcodeDao
    abstract fun barangDao(): BarangDao
    abstract fun barcodeRegistrationRequestDao(): BarcodeRegistrationRequestDao
    abstract fun returnOrderDao(): ReturnOrderDao
    abstract fun returnOrderItemDao(): ReturnOrderItemDao
    abstract fun customerDao(): CustomerDao
    abstract fun salesPersonDao(): SalesPersonDao
    abstract fun driverDao(): DriverDao

    companion object {
        @Volatile
        private var INSTANCE: AppDatabase? = null

        /**
         * Non-destructive 1 -> 2 migration (S4.1): creates only the five new
         * Return Order tables; existing barcode/barang caches and the pending
         * queue survive.
         */
        val MIGRATION_1_2: Migration = object : Migration(1, 2) {
            override fun migrate(db: SupportSQLiteDatabase) {
                db.execSQL(
                    "CREATE TABLE IF NOT EXISTS `return_order_entity` (" +
                        "`returnOrderId` TEXT NOT NULL, " +
                        "`customerId` TEXT NOT NULL, " +
                        "`customerCode` TEXT NOT NULL, " +
                        "`customerName` TEXT NOT NULL, " +
                        "`warehouseCode` TEXT NOT NULL, " +
                        "`salesPersonId` TEXT NOT NULL, " +
                        "`salesPersonName` TEXT NOT NULL, " +
                        "`driverId` TEXT NOT NULL, " +
                        "`driverName` TEXT NOT NULL, " +
                        "`note` TEXT NOT NULL, " +
                        "`status` TEXT NOT NULL, " +
                        "`createdAt` INTEGER NOT NULL, " +
                        "`createdBy` TEXT NOT NULL, " +
                        "PRIMARY KEY(`returnOrderId`))"
                )
                db.execSQL(
                    "CREATE TABLE IF NOT EXISTS `return_order_item_entity` (" +
                        "`returnOrderId` TEXT NOT NULL, " +
                        "`noUrut` INTEGER NOT NULL, " +
                        "`brgId` TEXT NOT NULL, " +
                        "`brgCode` TEXT NOT NULL, " +
                        "`brgName` TEXT NOT NULL, " +
                        "`qty` REAL NOT NULL, " +
                        "`satId` TEXT NOT NULL, " +
                        "`jenisRetur` TEXT NOT NULL, " +
                        "PRIMARY KEY(`returnOrderId`, `noUrut`))"
                )
                db.execSQL(
                    "CREATE TABLE IF NOT EXISTS `customer_entity` (" +
                        "`customerId` TEXT NOT NULL, " +
                        "`customerCode` TEXT NOT NULL, " +
                        "`customerName` TEXT NOT NULL, " +
                        "`address` TEXT NOT NULL, " +
                        "PRIMARY KEY(`customerId`))"
                )
                db.execSQL(
                    "CREATE TABLE IF NOT EXISTS `salesperson_entity` (" +
                        "`salesPersonId` TEXT NOT NULL, " +
                        "`salesPersonName` TEXT NOT NULL, " +
                        "PRIMARY KEY(`salesPersonId`))"
                )
                db.execSQL(
                    "CREATE TABLE IF NOT EXISTS `driver_entity` (" +
                        "`driverId` TEXT NOT NULL, " +
                        "`driverName` TEXT NOT NULL, " +
                        "`isAktif` INTEGER NOT NULL, " +
                        "PRIMARY KEY(`driverId`))"
                )
                db.execSQL(
                    "CREATE INDEX IF NOT EXISTS `index_return_order_entity_status` " +
                        "ON `return_order_entity` (`status`)"
                )
                db.execSQL(
                    "CREATE INDEX IF NOT EXISTS `index_return_order_entity_customerName` " +
                        "ON `return_order_entity` (`customerName`)"
                )
            }
        }

        fun getDatabase(context: Context): AppDatabase {
            return INSTANCE ?: synchronized(this) {
                val instance = Room.databaseBuilder(
                    context.applicationContext,
                    AppDatabase::class.java,
                    "bgud_database"
                ).addMigrations(MIGRATION_1_2).build()
                INSTANCE = instance
                instance
            }
        }
    }
}
