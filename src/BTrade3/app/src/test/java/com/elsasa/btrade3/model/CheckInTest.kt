package com.elsasa.btrade3.model



import org.junit.Assert.assertEquals

import org.junit.Assert.assertFalse

import org.junit.Assert.assertTrue

import org.junit.Test



class CheckInTest {



    @Test

    fun isOpenVisit_trueWhenExplicitlyOpenAndNoCheckOut() {

        val checkIn = sampleCheckIn(checkOutTime = "", isExplicitlyOpen = true)

        assertTrue(checkIn.isOpenVisit())

    }



    @Test

    fun isOpenVisit_falseForLegacyRecordWithEmptyCheckOut() {

        val checkIn = sampleCheckIn(checkOutTime = "", isExplicitlyOpen = false)

        assertFalse(checkIn.isOpenVisit())

    }



    @Test

    fun isOpenVisit_falseWhenCheckOutTimePresent() {

        val checkIn = sampleCheckIn(checkOutTime = "17:30:00", isExplicitlyOpen = true)

        assertFalse(checkIn.isOpenVisit())

    }



    @Test

    fun isOpenVisit_falseWhenCheckOutTimePresentEvenIfNotExplicitlyOpen() {

        val checkIn = sampleCheckIn(checkOutTime = "17:30:00", isExplicitlyOpen = false)

        assertFalse(checkIn.isOpenVisit())

    }



    @Test

    fun checkOutMode_constantsAreStable() {

        assertEquals("MANUAL", CheckOutMode.MANUAL)

        assertEquals("AUTO", CheckOutMode.AUTO)

        assertTrue(CheckOutMode.isValid(CheckOutMode.MANUAL))

        assertTrue(CheckOutMode.isValid(CheckOutMode.AUTO))

        assertFalse(CheckOutMode.isValid(""))

    }



    private fun sampleCheckIn(

        checkOutTime: String,

        isExplicitlyOpen: Boolean = false

    ) = CheckIn(

        checkInId = "01JCHECKIN00000000000001",

        checkInDate = "2026-06-19",

        checkInTime = "09:00:00",

        userEmail = "sales@example.com",

        checkInLatitude = -6.2,

        checkInLongitude = 106.8,

        accuracy = 12f,

        customerId = "C1",

        customerCode = "C001",

        customerName = "Customer A",

        customerAddress = "Jl. Test",

        customerLatitude = -6.2,

        customerLongitude = 106.8,

        statusSync = "DRAFT",

        checkOutTime = checkOutTime,

        isExplicitlyOpen = isExplicitlyOpen

    )

}

