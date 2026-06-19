package com.elsasa.btrade3.model

import org.junit.Assert.assertFalse
import org.junit.Assert.assertTrue
import org.junit.Test

class CustomerTest {

    @Test
    fun hasCoordinates_trueWhenValidLatLng() {
        val customer = sampleCustomer(latitude = -6.2, longitude = 106.8)
        assertTrue(customer.hasCoordinates())
    }

    @Test
    fun hasCoordinates_falseWhenZeroZero() {
        val customer = sampleCustomer(latitude = 0.0, longitude = 0.0)
        assertFalse(customer.hasCoordinates())
    }

    @Test
    fun hasCoordinates_falseWhenNaN() {
        val customer = sampleCustomer(latitude = Double.NaN, longitude = 106.8)
        assertFalse(customer.hasCoordinates())
    }

    private fun sampleCustomer(latitude: Double, longitude: Double) = Customer(
        customerId = "C1",
        customerCode = "C001",
        customerName = "Customer A",
        alamat = "Jl. Test",
        wilayah = "AAA",
        latitude = latitude,
        longitude = longitude,
    )
}
