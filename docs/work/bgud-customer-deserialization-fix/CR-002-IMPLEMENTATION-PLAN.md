---
Title: BGud Mobile — Koreksi Deserialisasi DTO Referensi Retur (Customer, SalesPerson, Driver)
Code: CR-002
Artifact: IMPLEMENTATION-PLAN
Version: 1.0
LastUpdated: 2026-10-09
Status: COMPLETED
Execution Approval: APPROVED
---

# 1. Objective

Memperbaiki mapping deserialisasi JSON pada DTO referensi retur di aplikasi BGud Mobile (`src/BGud`) agar mendukung format penamaan properti `camelCase` (standar respon ASP.NET Core) dan `PascalCase` secara toleran (*case-resilient* via Gson `alternate`). Hal ini memastikan seluruh data referensi pelanggan (`CustomerDto`), salesman (`SalesPersonDto`), dan driver (`DriverDto`) terunduh secara utuh ke database lokal Room (`customer_entity`, `salesperson_entity`, `driver_entity`) sehingga dapat dicari dan dipilih pada layar pembuatan dan edit Return Order.

Planning Mode: FEATURE-PLANNING

Referenced artifacts:

- ISSUE: [CR-002.md](file:///d:/Project.Private/btr-platform/docs/issues/CR-002.md)
- BUG-INVESTIGATION: [CR-002-BUG-INVESTIGATION.md](file:///d:/Project.Private/btr-platform/docs/issues/CR-002-BUG-INVESTIGATION.md)
- Reference Architecture: [RETURN-ORDER-ARCHITECTURE.md](file:///d:/Project.Private/btr-platform/docs/work/return-order/RETURN-ORDER-ARCHITECTURE.md) §6.4, §8.3, §19.3

Architecture Applicability: ARCHITECTURE-NOT-REQUIRED

No architectural target-state artifact was required. Implementation relies on existing technical structure. Approved feasibility decisions are authoritative for the change. The current codebase is the source of current technical truth.

---

# 2. Planning Scope

Paket pekerjaan ini mencakup pembaruan kontrak DTO deserialisasi dan pengujian unit pada aplikasi Android BGud:

1. **DTO Deserialization Resiliency:**
   - Memperbarui anotasi `@SerializedName` pada `CustomerDto`, `SalesPersonDto`, dan `DriverDto` di `ApiModels.kt` agar menggunakan nama properti primer `camelCase` dan nama properti alternatif `PascalCase` melalui parameter `alternate = [...]`.
2. **Repository Guard & Safety Check:**
   - Memastikan pemetaan DTO ke Entity di `ReturnOrderReferenceSyncRepository.kt` mengabaikan item yang memiliki primary key kosong (`customerId.isNotBlank()`) sebagai pertahanan terhadap anomali data di masa depan.
3. **Unit Testing:**
   - Membuat unit test komprehensif (`ReferenceDtoDeserializationTest.kt`) untuk memverifikasi bahwa parser Gson berhasil mengekstrak field DTO secara utuh dari sampel JSON respon server aktual (baik format camelCase maupun PascalCase).
   - Memverifikasi integrasi mapping dari DTO ke `CustomerEntity`.

**Target repository:** `src/BGud` (Android Jetpack Compose / Kotlin codebase)

**Batasan Cakupan:**
- In: `src/BGud/app/src/main/java/com/elsasa/bgud/model/api/ApiModels.kt`, `src/BGud/app/src/main/java/com/elsasa/bgud/repository/ReturnOrderReferenceSyncRepository.kt`, dan `src/BGud/app/src/test/java/com/elsasa/bgud/model/ReferenceDtoDeserializationTest.kt`.
- Out: Tidak ada perubahan skema database Room, tidak ada perubahan backend ASP.NET Core API, dan tidak ada perubahan UI Jetpack Compose.

---

# 3. Dependencies

Semua pekerjaan berada di dalam modul aplikasi `src/BGud`.

Untuk dependensi antar-slice:
- `Depends On` mendeklarasikan prasyarat implementasi.
- Dependensi hanya mereferensikan Slice ID yang valid.
- Kepuasan dependensi mensyaratkan slice yang dirujuk memiliki status implementasi `IMPLEMENTED`.
- Dependensi mencerminkan prasyarat teknis nyata antar komponen kode.

---

# 4. Progress Summary

| Phase | Implementation Status | Review Status | Progress |
|-------|----------------------|---------------|----------|
| P1 | IMPLEMENTED | GO | 2/2 |

---

# 5. Phases

## P1 - Reference DTO Serialization Fix & Verification

Implementation Status: IMPLEMENTED  
Review Status: GO  

---

### P1-S01

Title: Dual-casing Resilient DTO Annotations & Mapping Filter in BGud

Implementation Status: IMPLEMENTED  
Review Status: GO  

Objective:  
Memperbarui definisi `CustomerDto`, `SalesPersonDto`, dan `DriverDto` pada `ApiModels.kt` agar menggunakan `@SerializedName` dengan dukungan `alternate` untuk format camelCase dan PascalCase, serta menambahkan filter validitas primary key pada `ReturnOrderReferenceSyncRepository.kt`.

Depends On: None  

Repository: `src/BGud`  

Completion Criteria:  
1. `CustomerDto` pada `src/BGud/app/src/main/java/com/elsasa/bgud/model/api/ApiModels.kt` memiliki anotasi:
   - `@SerializedName("customerId", alternate = ["CustomerId"]) val customerId: String`
   - `@SerializedName("customerCode", alternate = ["CustomerCode"]) val customerCode: String`
   - `@SerializedName("customerName", alternate = ["CustomerName"]) val customerName: String`
   - `@SerializedName("alamat", alternate = ["Alamat"]) val alamat: String`
   - `@SerializedName("serverId", alternate = ["ServerId"]) val serverId: String`
2. `SalesPersonDto` memiliki anotasi:
   - `@SerializedName("salesPersonId", alternate = ["SalesPersonId"]) val salesPersonId: String`
   - `@SerializedName("salesPersonCode", alternate = ["SalesPersonCode"]) val salesPersonCode: String`
   - `@SerializedName("salesPersonName", alternate = ["SalesPersonName"]) val salesPersonName: String`
   - `@SerializedName("email", alternate = ["Email"]) val email: String`
   - `@SerializedName("serverId", alternate = ["ServerId"]) val serverId: String`
3. `DriverDto` memiliki anotasi:
   - `@SerializedName("driverId", alternate = ["DriverId"]) val driverId: String`
   - `@SerializedName("driverName", alternate = ["DriverName"]) val driverName: String`
   - `@SerializedName("isAktif", alternate = ["IsAktif"]) val isAktif: Boolean`
   - `@SerializedName("serverId", alternate = ["ServerId"]) val serverId: String`
4. Di `ReturnOrderReferenceSyncRepository.kt`:
   - `downloadCustomer`: Mengabaikan entitas dengan `customerId.isBlank()`.
   - `downloadSalesPerson`: Mengabaikan entitas dengan `salesPersonId.isBlank()`.
   - `downloadDriver`: Mengabaikan entitas dengan `driverId.isBlank()`.
5. Kode berhasil dikompilasi tanpa error sintaksis.

Notes:  
- Pemberian parameter `alternate` menjamin kompatibilitas ganda tanpa mengubah struktur publik DTO atau kode pemanggil lainnya.
- Implementasi selesai: Anotasi `@SerializedName` pada `CustomerDto`, `SalesPersonDto`, dan `DriverDto` diperbarui dengan penamaan primer `camelCase` dan `alternate = [...]` untuk `PascalCase`.
- Filter `.filter { it.<pk>.isNotBlank() }` ditambahkan pada `downloadCustomer`, `downloadSalesPerson`, dan `downloadDriver` di `ReturnOrderReferenceSyncRepository.kt`.
- Kompilasi `compileDebugKotlin` dan unit test `testDebugUnitTest` berhasil lolos tanpa error.

---

### P1-S02

Title: Unit Tests for Reference DTO Deserialization and Mapping

Implementation Status: IMPLEMENTED  
Review Status: GO  

Objective:  
Membuat test suite unit di `src/BGud/app/src/test/java/com/elsasa/bgud/model/ReferenceDtoDeserializationTest.kt` untuk memvalidasi deserialisasi JSON camelCase (respon aktual server) dan PascalCase ke model `CustomerDto`, `SalesPersonDto`, dan `DriverDto`, serta pemetaan ke Room Entity.

Depends On: P1-S01  

Repository: `src/BGud`  

Completion Criteria:  
1. File pengujian unit `ReferenceDtoDeserializationTest.kt` dibuat dan mencakup:
   - Verifikasi payload JSON camelCase dari endpoint `GET /api/Customer/{serverId}` terdeserialisasi dengan benar (misal: `customerId = "CS0007"`, `customerCode = "ABD P"`, `customerName = "ABADI"`, `alamat = "PS.PRIPIH"`).
   - Verifikasi payload JSON PascalCase terdeserialisasi dengan benar sebagai fallback kompatibilitas.
   - Verifikasi deserialisasi `SalesPersonDto` dan `DriverDto` untuk kedua variasi casing.
   - Verifikasi fungsi konversi `.toEntity()` menghasilkan data `CustomerEntity`, `SalesPersonEntity`, dan `DriverEntity` yang identik.
2. Seluruh unit test dieksekusi dan lolos (*PASSED*).

Notes:  
- Test suite unit komprehensif diimplementasikan pada `src/BGud/app/src/test/java/com/elsasa/bgud/model/ReferenceDtoDeserializationTest.kt` dengan 11 test case:
  - `customerDto_deserializesFromCamelCaseJson`
  - `customerDto_deserializesFromPascalCaseJson`
  - `customerDto_deserializesInsideJSendEnvelope`
  - `customerDto_toEntityConversion_producesValidCustomerEntity`
  - `salesPersonDto_deserializesFromCamelCaseJson`
  - `salesPersonDto_deserializesFromPascalCaseJson`
  - `salesPersonDto_toEntityConversion_producesValidSalesPersonEntity`
  - `driverDto_deserializesFromCamelCaseJson`
  - `driverDto_deserializesFromPascalCaseJson`
  - `driverDto_toEntityConversion_producesValidDriverEntity`
  - `dtos_haveSafeDefaults_whenJsonIsEmpty`
- Eksekusi `./gradlew.bat testDebugUnitTest` berhasil 100% (semua 11 test kasus pada `ReferenceDtoDeserializationTest` serta seluruh unit test modul lolos tanpa kegagalan).

---

# 6. Change Log

- 2026-10-09: Dokumen dibuat untuk menindaklanjuti perbaikan cacat CR-002 (Koreksi deserialisasi JSON DTO Customer, SalesPerson, dan Driver). Rencana disetujui untuk eksekusi (`Execution Approval: APPROVED`).
