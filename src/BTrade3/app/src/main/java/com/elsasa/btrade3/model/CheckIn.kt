package com.elsasa.btrade3.model


import androidx.room.Entity
import androidx.room.PrimaryKey
import java.text.SimpleDateFormat
import java.util.Locale
import java.util.concurrent.TimeUnit

@Entity(tableName = "checkin_table")
data class CheckIn(
    @PrimaryKey
    val checkInId: String,
    val checkInDate: String,        // yyyy-MM-dd
    val checkInTime: String,        // HH:mm:ss
    val userEmail: String,
    val checkInLatitude: Double,
    val checkInLongitude: Double,
    val accuracy: Float,
    val customerId: String,
    val customerCode: String,
    val customerName: String,
    val customerAddress: String,
    val customerLatitude: Double,
    val customerLongitude: Double,
    val statusSync: String,
    val checkOutTime: String = "",
    val checkOutLatitude: Double = 0.0,
    val checkOutLongitude: Double = 0.0,
    val checkOutAccuracy: Float = 0f,
    val checkOutMode: String = "",
    val isExplicitlyOpen: Boolean = false
) {
    fun isOpenVisit(): Boolean = checkOutTime.isEmpty() && isExplicitlyOpen

    fun checkInTimestampMillis(): Long {
        val formatter = SimpleDateFormat("yyyy-MM-dd HH:mm:ss", Locale.getDefault())
        return formatter.parse("$checkInDate $checkInTime")?.time ?: 0L
    }
}

fun formatCheckInElapsedTime(checkIn: CheckIn, nowMillis: Long = System.currentTimeMillis()): String {
    val elapsedMinutes = TimeUnit.MILLISECONDS.toMinutes(
        (nowMillis - checkIn.checkInTimestampMillis()).coerceAtLeast(0)
    )

    return when {
        elapsedMinutes < 60 -> "$elapsedMinutes mnt"
        elapsedMinutes % 60 == 0L -> "${elapsedMinutes / 60} jam"
        else -> "${elapsedMinutes / 60} jam ${elapsedMinutes % 60} mnt"
    }
}