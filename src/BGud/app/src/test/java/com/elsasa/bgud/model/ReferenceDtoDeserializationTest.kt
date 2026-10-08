package com.elsasa.bgud.model

import com.elsasa.bgud.model.api.CustomerDto
import com.elsasa.bgud.model.api.DriverDto
import com.elsasa.bgud.model.api.JSendEnvelope
import com.elsasa.bgud.model.api.SalesPersonDto
import com.elsasa.bgud.repository.ReturnOrderReferenceSyncRepository.Companion.toEntity
import com.google.gson.Gson
import com.google.gson.reflect.TypeToken
import org.junit.Assert.assertEquals
import org.junit.Assert.assertFalse
import org.junit.Assert.assertNotNull
import org.junit.Assert.assertTrue
import org.junit.Test

/**
 * Unit tests verifying JSON deserialization and Entity mapping for Return Order
 * reference DTOs (CustomerDto, SalesPersonDto, DriverDto).
 *
 * Verifies CR-002:
 * 1. camelCase deserialization (ASP.NET Core default WebAPI response).
 * 2. PascalCase deserialization fallback (case-resilient via Gson alternate).
 * 3. Conversion via .toEntity() produces valid Room cache entities.
 */
class ReferenceDtoDeserializationTest {

    private val gson = Gson()

    // --- CustomerDto Tests ---

    @Test
    fun customerDto_deserializesFromCamelCaseJson() {
        val json = """
            {
              "customerId": "CS0007",
              "customerCode": "ABD P",
              "customerName": "ABADI",
              "alamat": "PS.PRIPIH",
              "serverId": "JOG"
            }
        """.trimIndent()

        val dto = gson.fromJson(json, CustomerDto::class.java)

        assertNotNull(dto)
        assertEquals("CS0007", dto.customerId)
        assertEquals("ABD P", dto.customerCode)
        assertEquals("ABADI", dto.customerName)
        assertEquals("PS.PRIPIH", dto.alamat)
        assertEquals("JOG", dto.serverId)
    }

    @Test
    fun customerDto_deserializesFromPascalCaseJson() {
        val json = """
            {
              "CustomerId": "CS0007",
              "CustomerCode": "ABD P",
              "CustomerName": "ABADI",
              "Alamat": "PS.PRIPIH",
              "ServerId": "JOG"
            }
        """.trimIndent()

        val dto = gson.fromJson(json, CustomerDto::class.java)

        assertNotNull(dto)
        assertEquals("CS0007", dto.customerId)
        assertEquals("ABD P", dto.customerCode)
        assertEquals("ABADI", dto.customerName)
        assertEquals("PS.PRIPIH", dto.alamat)
        assertEquals("JOG", dto.serverId)
    }

    @Test
    fun customerDto_deserializesInsideJSendEnvelope() {
        val json = """
            {
              "status": "success",
              "code": "200",
              "data": [
                {
                  "customerId": "CS0001",
                  "customerCode": "32",
                  "customerName": "32",
                  "alamat": "WIROSABAN",
                  "wilayah": "YOGYAKARTA",
                  "serverId": "JOG"
                },
                {
                  "customerId": "CS0007",
                  "customerCode": "ABD P",
                  "customerName": "ABADI",
                  "alamat": "PS.PRIPIH",
                  "serverId": "JOG"
                }
              ]
            }
        """.trimIndent()

        val type = object : TypeToken<JSendEnvelope<List<CustomerDto>>>() {}.type
        val envelope: JSendEnvelope<List<CustomerDto>> = gson.fromJson(json, type)

        assertNotNull(envelope)
        assertEquals("success", envelope.status)
        assertEquals("200", envelope.code)
        val data = envelope.data
        assertNotNull(data)
        assertEquals(2, data!!.size)
        assertEquals("CS0001", data[0].customerId)
        assertEquals("32", data[0].customerCode)
        assertEquals("32", data[0].customerName)
        assertEquals("WIROSABAN", data[0].alamat)
        assertEquals("CS0007", data[1].customerId)
        assertEquals("ABD P", data[1].customerCode)
        assertEquals("ABADI", data[1].customerName)
    }

    @Test
    fun customerDto_toEntityConversion_producesValidCustomerEntity() {
        val dto = CustomerDto(
            customerId = "CS0007",
            customerCode = "ABD P",
            customerName = "ABADI",
            alamat = "PS.PRIPIH",
            serverId = "JOG"
        )

        val entity = dto.toEntity()

        assertEquals("CS0007", entity.customerId)
        assertEquals("ABD P", entity.customerCode)
        assertEquals("ABADI", entity.customerName)
        assertEquals("PS.PRIPIH", entity.address)
    }

    // --- SalesPersonDto Tests ---

    @Test
    fun salesPersonDto_deserializesFromCamelCaseJson() {
        val json = """
            {
              "salesPersonId": "SP001",
              "salesPersonCode": "SLS01",
              "salesPersonName": "Budi Santoso",
              "email": "budi@btr.com",
              "serverId": "JOG"
            }
        """.trimIndent()

        val dto = gson.fromJson(json, SalesPersonDto::class.java)

        assertNotNull(dto)
        assertEquals("SP001", dto.salesPersonId)
        assertEquals("SLS01", dto.salesPersonCode)
        assertEquals("Budi Santoso", dto.salesPersonName)
        assertEquals("budi@btr.com", dto.email)
        assertEquals("JOG", dto.serverId)
    }

    @Test
    fun salesPersonDto_deserializesFromPascalCaseJson() {
        val json = """
            {
              "SalesPersonId": "SP001",
              "SalesPersonCode": "SLS01",
              "SalesPersonName": "Budi Santoso",
              "Email": "budi@btr.com",
              "ServerId": "JOG"
            }
        """.trimIndent()

        val dto = gson.fromJson(json, SalesPersonDto::class.java)

        assertNotNull(dto)
        assertEquals("SP001", dto.salesPersonId)
        assertEquals("SLS01", dto.salesPersonCode)
        assertEquals("Budi Santoso", dto.salesPersonName)
        assertEquals("budi@btr.com", dto.email)
        assertEquals("JOG", dto.serverId)
    }

    @Test
    fun salesPersonDto_toEntityConversion_producesValidSalesPersonEntity() {
        val dto = SalesPersonDto(
            salesPersonId = "SP001",
            salesPersonCode = "SLS01",
            salesPersonName = "Budi Santoso",
            email = "budi@btr.com",
            serverId = "JOG"
        )

        val entity = dto.toEntity()

        assertEquals("SP001", entity.salesPersonId)
        assertEquals("Budi Santoso", entity.salesPersonName)
    }

    // --- DriverDto Tests ---

    @Test
    fun driverDto_deserializesFromCamelCaseJson() {
        val json = """
            {
              "driverId": "DRV01",
              "driverName": "Joko Widodo",
              "isAktif": true,
              "serverId": "JOG"
            }
        """.trimIndent()

        val dto = gson.fromJson(json, DriverDto::class.java)

        assertNotNull(dto)
        assertEquals("DRV01", dto.driverId)
        assertEquals("Joko Widodo", dto.driverName)
        assertTrue(dto.isAktif)
        assertEquals("JOG", dto.serverId)
    }

    @Test
    fun driverDto_deserializesFromPascalCaseJson() {
        val json = """
            {
              "DriverId": "DRV02",
              "DriverName": "Supriadi",
              "IsAktif": false,
              "ServerId": "JOG"
            }
        """.trimIndent()

        val dto = gson.fromJson(json, DriverDto::class.java)

        assertNotNull(dto)
        assertEquals("DRV02", dto.driverId)
        assertEquals("Supriadi", dto.driverName)
        assertFalse(dto.isAktif)
        assertEquals("JOG", dto.serverId)
    }

    @Test
    fun driverDto_toEntityConversion_producesValidDriverEntity() {
        val dto = DriverDto(
            driverId = "DRV01",
            driverName = "Joko Widodo",
            isAktif = true,
            serverId = "JOG"
        )

        val entity = dto.toEntity()

        assertEquals("DRV01", entity.driverId)
        assertEquals("Joko Widodo", entity.driverName)
        assertTrue(entity.isAktif)
    }

    // --- Default Values / Incomplete Payload Test ---

    @Test
    fun dtos_haveSafeDefaults_whenJsonIsEmpty() {
        val emptyJson = "{}"

        val customer = gson.fromJson(emptyJson, CustomerDto::class.java)
        assertEquals("", customer.customerId)
        assertEquals("", customer.customerCode)
        assertEquals("", customer.customerName)
        assertEquals("", customer.alamat)
        assertEquals("", customer.serverId)

        val salesPerson = gson.fromJson(emptyJson, SalesPersonDto::class.java)
        assertEquals("", salesPerson.salesPersonId)
        assertEquals("", salesPerson.salesPersonCode)
        assertEquals("", salesPerson.salesPersonName)
        assertEquals("", salesPerson.email)
        assertEquals("", salesPerson.serverId)

        val driver = gson.fromJson(emptyJson, DriverDto::class.java)
        assertEquals("", driver.driverId)
        assertEquals("", driver.driverName)
        assertTrue(driver.isAktif) // default is true
        assertEquals("", driver.serverId)
    }
}
