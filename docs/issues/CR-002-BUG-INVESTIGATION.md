# BUG-INVESTIGATION

## Context

Issue: `CR-002`
Problem Summary: Pada aplikasi mobile BGud (`src/BGud`), saat operator melakukan pencarian pelanggan pada input *"Cari Customer (kode / nama)"* di layar pembuatan/edit Return Order (`CreateReturnOrderScreen.kt` & `EditReturnOrderScreen.kt`), tidak ada data pelanggan yang muncul sama sekali (*"Tidak ditemukan pada cache lokal perangkat"*), meskipun proses sinkronisasi referensi retur berhasil dan tabel `BTRADE_Customer` di database cloud memiliki ribuan record.

## Current State

1. Operator melakukan sinkronisasi data pada layar *Sinkronisasi Data* (`SynchronizationScreen.kt`). Status referensi retur menunjukkan timestamp terbaru dan banner berwarna hijau (*"Semua berhasil disinkronkan dengan server"*).
2. Sesi login operator terhubung ke gudang `GAMPING` dengan kantor cabang yang teresolusi sebagai `JOG`.
3. Operator membuka layar pembuatan Return Order (`CreateReturnOrderScreen.kt`) dan mengetik nama atau kode pelanggan (contoh: `"ABADI"`, `"ABD"`, atau `"CS0007"`).
4. Indikator pencarian berputar sesaat, lalu menampilkan pesan: *"Tidak ditemukan pada cache lokal perangkat"*.
5. Tabel Room lokal `customer_entity` pada SQLite perangkat kosong atau hanya memiliki satu baris data korup dengan string kosong (`customerId = ""`).

## Problem Analysis

### 1. Root Cause 1 (Telah Diselesaikan pada Investigasi Awal): Ketidaksesuaian `ServerId` pada Pemetaan Gudang
- **Temuan:** Awalnya tabel pemetaan `[dbo].[BTR_WarehouseMapping]` dan `[dbo].[BTRADE_Location]` di server database `dev.smart-ics.com (dbKerjaPraktek)` memetakan gudang `GAMPING` dan `CONCAT` ke `ServerId = 'JOGJA'`.
- **Fakta Data:** Seluruh tabel data master (`BTRADE_Customer`, `BTRADE_Brg`, `BTRADE_SalesPerson`, dll.) menggunakan kode tenant `ServerId = 'JOG'` (3 karakter). Permintaan ke `GET /api/Customer/JOGJA` mengembalikan array kosong `[]`.
- **Status:** Masalah ini telah diperbaiki di database dengan mengupdate `ServerId` menjadi `'JOG'`. Respon endpoint `GET /api/Customer/JOG` kini valid mengembalikan 2.273 entitas pelanggan.

### 2. Root Cause 2 (Akar Masalah Utama Saat Ini): JSON Field Naming Mismatch (camelCase vs PascalCase)
- **Temuan Respon API:** Server ASP.NET Core (`btrade.webapi`) secara default mengonfigurasi serialisasi JSON menggunakan standar penamaan properti **camelCase**.
  Contoh respon aktual dari `http://dev.smart-ics.com:8089/belajar-api/api/Customer/JOG`:
  ```json
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
      }
    ]
  }
  ```
- **Temuan DTO BGud:** Pada `src/BGud/app/src/main/java/com/elsasa/bgud/model/api/ApiModels.kt`, model DTO didefinisikan dengan anotasi `@SerializedName` berformat **PascalCase** mutlak:
  ```kotlin
  data class CustomerDto(
      @SerializedName("CustomerId") val customerId: String = "",
      @SerializedName("CustomerCode") val customerCode: String = "",
      @SerializedName("CustomerName") val customerName: String = "",
      @SerializedName("Alamat") val alamat: String = "",
      @SerializedName("ServerId") val serverId: String = ""
  )
  ```
- **Mekanisme Kegagalan Deserialisasi:**
  1. Parser Gson yang digunakan oleh Retrofit (`ApiClient.kt`) bersifat *case-sensitive* saat mencocokkan field `@SerializedName`.
  2. Karena JSON dari server mengirimkan `"customerId"`, `"customerCode"`, `"customerName"`, dan `"alamat"`, Gson tidak menemukan field `"CustomerId"`, `"CustomerCode"`, dll.
  3. Akibatnya, seluruh properti diisi dengan nilai default konstruktor Kotlin: string kosong `""`.
  4. Repository `ReturnOrderReferenceSyncRepository.kt` memetakan DTO tersebut ke `CustomerEntity` dan memanggil `customerDao.upsertAll(entities)`.
  5. Karena `customerId` adalah `@PrimaryKey` pada Room, 2.273 record yang semuanya bernilai `customerId = ""` saling menimpa satu sama lain dengan strategi `OnConflictStrategy.REPLACE`, menyisakan tepat **1 baris kosong** di database SQLite lokal.
  6. Karena proses deserialisasi tidak melempar `Exception`, blok `try-catch` di worker menganggap operasi sukses, timestamp sinkronisasi diperbarui, dan UI menampilkan banner hijau.
- **Dampak pada DTO Terkait:**
  Kondisi yang sama juga ditemukan pada DTO referensi retur lainnya:
  - `SalesPersonDto`: Memerlukan `salesPersonId`, `salesPersonCode`, `salesPersonName`, `email`, `serverId`.
  - `DriverDto`: Memerlukan `driverId`, `driverName`, `isAktif`, `serverId`.

## Affected Components

- **Mobile Client (BGud):**
  - `com.elsasa.bgud.model.api.ApiModels.kt`: Definisi `CustomerDto`, `SalesPersonDto`, dan `DriverDto`.
  - `com.elsasa.bgud.repository.ReturnOrderReferenceSyncRepository.kt`: Logika mapping dan validasi data saat unduh referensi.
  - `com.elsasa.bgud.dao.CustomerDao.kt`: Target penyimpanan Room cache `customer_entity`.
  - `com.elsasa.bgud.ui.screen.CreateReturnOrderScreen.kt` & `EditReturnOrderScreen.kt`: Antarmuka pemilihan customer lokal.
- **Backend / Cloud API:**
  - `btrade.webapi` (`CustomerController`, `SalesPersonController`, `DriverController`): Pengirim serialisasi JSON.

## Impact Assessment

- **Business Impact:**
  - Operator gudang terhambat total dalam mencatat dan memproses fisik penerimaan retur pelanggan di lapangan, karena Customer merupakan entitas wajib (*mandatory*) untuk pembuatan Return Order (BR-001/002).
- **Operational Impact:**
  - Terjadi ketidaksinkronan data operasional; operator mengira sinkronisasi sudah berhasil (karena status hijau), padahal data referensi di perangkat kosong.
- **Technical Impact:**
  - Data corruption semantik pada database Room lokal (ribuan row tertimpa menjadi 1 row kosong).

## Assumptions

1. Kontrak endpoint Cloud API `GET /api/Customer/{serverId}`, `GET /api/SalesPerson/{serverId}`, dan `GET /api/Driver/{serverId}` tetap dipertahankan formatnya tanpa mengubah konfigurasi global serializer di backend ASP.NET Core yang berpotensi memengaruhi consumer API lainnya (seperti BTrade3 atau `j07-btrade-sync`).
2. Client mobile BGud harus memiliki ketahanan (*resilience*) terhadap variasi penamaan field (toleran terhadap camelCase maupun PascalCase).

## Open Questions

- *Tidak ada open questions yang memblokir.* Analisis penyebab dan bukti teknis sudah 100% konklusif dan terverifikasi melalui payload aktual dari server.

## Recommended Decision

Menerapkan penyesuaian deserialisasi JSON yang fleksibel (*dual-casing resilient*) pada DTO referensi di sisi klien BGud (`ApiModels.kt`) menggunakan fitur `alternate` pada anotasi `@SerializedName` Gson:
- Mendukung properti primer berformat **camelCase** (sesuai standar runtime ASP.NET Core saat ini).
- Mendukung properti alternatif berformat **PascalCase** (menjaga kompatibilitas jika format payload berubah di masa mendatang).

Pendekatan ini dipilih karena merupakan solusi non-breaking, terlokalisasi di lapisan integrasi mobile, tidak memerlukan redeployment backend, serta langsung memulihkan proses sinkronisasi customer, salesperson, dan driver.

## Decision

Menyetujui rekomendasi perbaikan penamaan field serialisasi pada DTO referensi BGud (`CustomerDto`, `SalesPersonDto`, dan `DriverDto`) agar mendukung format `camelCase` dan `PascalCase` melalui atribut alternatif serializer Gson.

## Decision Rationale

1. **Non-invasive & Low Risk:** Tidak memerlukan perubahan konfigurasi pada server API produksi yang dapat berdampak ke consumer lain.
2. **Immediate Resolution:** Memastikan Gson dapat memetakan data `customerId`, `customerCode`, `customerName`, `alamat`, dsb. secara presisi ke dalam entitas Room.
3. **Mencegah Masalah Serupa:** Sekaligus memitigasi kegagalan serupa pada data SalesPerson dan Driver sebelum dilaporkan oleh pengguna.

## Architecture Applicability

### Decision

**ARCHITECTURE-NOT-REQUIRED**

### Rationale

Perubahan ini terlokalisasi sepenuhnya pada lapisan kontrak pemodelan DTO klien (`ApiModels.kt`) untuk menyesuaikan penamaan field serialisasi terhadap kontrak runtime API yang sudah ada. Tidak ada perubahan pada boundary agregat, skema database (baik Room maupun SQL Server), alur workflow, interaksi antarmuka, maupun arsitektur sistem. Keputusan investigasi ini dapat langsung diimplementasikan melalui perencanaan teknis standar.
