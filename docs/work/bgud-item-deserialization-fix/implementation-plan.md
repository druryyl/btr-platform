---
Title: BGud Mobile — Koreksi Deserialisasi DTO Master Barang & Barcode (BrgDto, BarcodeDto, RegistrationStatusDto)
Code: CR-003
Artifact: IMPLEMENTATION-PLAN
Version: 1.0
LastUpdated: 2026-10-09
Status: COMPLETED
Execution Approval: APPROVED
---

# 1. Objective

Memperbaiki mapping deserialisasi JSON pada DTO master barang dan barcode di aplikasi BGud Mobile (`src/BGud`) agar mendukung format penamaan properti `camelCase` (standar respon ASP.NET Core) dan `PascalCase` secara toleran (*case-resilient* via Gson `alternate`). Hal ini memastikan seluruh data master barang (`BrgDto`), barcode (`BarcodeDto`), dan status registrasi barcode (`RegistrationStatusDto`) terunduh secara utuh ke database lokal Room (`barang_entity`, `barcode_entity`) sehingga dapat dicari dan dipilih saat input Return Order maupun pendaftaran barcode.

Planning Mode: BUG-FIX-PLANNING

Referenced artifacts:
- BUG-INVESTIGATION: [bgud-item-search-deserialization.md](file:///d:/Project.Private/btr-platform/docs/investigations/bgud-item-search-deserialization.md)
- Precedent: [CR-002-IMPLEMENTATION-PLAN.md](file:///d:/Project.Private/btr-platform/docs/work/bgud-customer-deserialization-fix/CR-002-IMPLEMENTATION-PLAN.md)

Architecture Applicability: ARCHITECTURE-NOT-REQUIRED
Perubahan ini terlokalisasi sepenuhnya pada lapisan kontrak DTO klien (`ApiModels.kt`) dan validasi filter di repository (`BarcodeSyncRepository.kt`).

---

# 2. Planning Scope

Paket pekerjaan ini mencakup pembaruan kontrak DTO deserialisasi dan pengujian unit pada aplikasi Android BGud:

1. **DTO Deserialization Resiliency:**
   - Memperbarui anotasi `@SerializedName` pada `BrgDto`, `BarcodeDto`, dan `RegistrationStatusDto` di `ApiModels.kt` agar menggunakan nama properti primer `camelCase` dan alternatif `PascalCase` melalui parameter `alternate = [...]`.
2. **Repository Guard & Safety Check:**
   - Menambahkan filter validitas primary key pada `BarcodeSyncRepository.kt` (`it.brgId.isNotBlank()` dan `it.brgBarcodeId.isNotBlank()`) sebelum melakukan `upsertAll`.
3. **Unit Testing:**
   - Menambahkan unit test komprehensif di `ReferenceDtoDeserializationTest.kt` untuk memverifikasi bahwa parser Gson berhasil mengekstrak field `BrgDto` dan `BarcodeDto` dari payload JSON format camelCase maupun PascalCase.
   - Memverifikasi konversi dari DTO ke `BarangEntity` dan `BarcodeEntity`.

**Target repository:** `src/BGud` (Android Jetpack Compose / Kotlin codebase)

---

# 3. Dependencies

Semua pekerjaan berada di dalam modul aplikasi `src/BGud`.

---

# 4. Progress Summary

| Phase | Implementation Status | Review Status | Progress |
|-------|----------------------|---------------|----------|
| P1 | IMPLEMENTED | GO | 2/2 |

---

# 5. Phases

## P1 - Item & Barcode DTO Serialization Fix & Verification

Implementation Status: IMPLEMENTED  
Review Status: GO  

---

### P1-S01

Title: Dual-casing Resilient DTO Annotations & Mapping Filter for Barang & Barcode in BGud

Implementation Status: IMPLEMENTED  
Review Status: GO  

Objective:  
Memperbarui definisi `BrgDto`, `BarcodeDto`, dan `RegistrationStatusDto` pada `ApiModels.kt` agar mendukung format camelCase dan PascalCase, serta menambahkan filter validitas primary key pada `BarcodeSyncRepository.kt`.

Depends On: None  

Repository: `src/BGud`  

Completion Criteria:  
1. `BrgDto` pada `src/BGud/app/src/main/java/com/elsasa/bgud/model/api/ApiModels.kt` memiliki anotasi:
   - `@SerializedName("brgId", alternate = ["BrgId"]) val brgId: String`
   - `@SerializedName("brgCode", alternate = ["BrgCode"]) val brgCode: String`
   - `@SerializedName("brgName", alternate = ["BrgName"]) val brgName: String`
   - `@SerializedName("kategoriName", alternate = ["KategoriName"]) val kategoriName: String`
   - `@SerializedName("satBesar", alternate = ["SatBesar"]) val satBesar: String`
   - `@SerializedName("satKecil", alternate = ["SatKecil"]) val satKecil: String`
   - `@SerializedName("konversi", alternate = ["Konversi"]) val konversi: Int`
   - `@SerializedName("hrgSat", alternate = ["HrgSat"]) val hrgSat: Double`
   - `@SerializedName("stok", alternate = ["Stok"]) val stok: Int`
   - `@SerializedName("serverId", alternate = ["ServerId"]) val serverId: String`
2. `BarcodeDto` memiliki anotasi camelCase primer dan PascalCase alternatif.
3. `RegistrationStatusDto` memiliki anotasi camelCase primer dan PascalCase alternatif.
4. `BarcodeSyncRepository.kt`:
   - `downloadBarang` memfilter item `it.brgId.isNotBlank()`.
   - `downloadBarcodes` memfilter item `it.brgBarcodeId.isNotBlank()`.

---

### P1-S02

Title: Unit Tests for BarangDto and BarcodeDto Deserialization & Entity Mapping

Implementation Status: IMPLEMENTED  
Review Status: GO  

Objective:  
Menambahkan unit test pada `ReferenceDtoDeserializationTest.kt` untuk memastikan parser Gson mampu membaca payload JSON camelCase maupun PascalCase untuk `BrgDto` dan `BarcodeDto`.

Depends On: P1-S01  

Repository: `src/BGud`  

Completion Criteria:  
1. Test suite `ReferenceDtoDeserializationTest.kt` mencakup pengujian:
   - `brgDto_deserializesFromCamelCaseJson()`
   - `brgDto_deserializesFromPascalCaseJson()`
   - `brgDto_toEntityConversion_producesValidBarangEntity()`
   - `barcodeDto_deserializesFromCamelCaseJson()`
   - `barcodeDto_deserializesFromPascalCaseJson()`
   - `barcodeDto_toEntityConversion_producesValidBarcodeEntity()`
2. Seluruh unit test pada `.\gradlew test` berhasil dieksekusi (BUILD SUCCESSFUL, exit code 0).
