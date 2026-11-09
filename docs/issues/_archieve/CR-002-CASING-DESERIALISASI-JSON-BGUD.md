# ISSUE

## Metadata

ID: `CR-002`
Type: `BUG`
Status: `New`
Title: **BGud Mobile — Data Customer Tidak Muncul pada Pencarian Pembuatan Return Order Akibat Mismatch Casing Deserialisasi JSON**

## Source

Reported By: Repository owner / User request
Reported Date: 2026-10-09

## Description

Pada aplikasi mobile BGud (`src/BGud`), saat operator membuka layar pembuatan Return Order (`CreateReturnOrderScreen.kt`) dan melakukan pencarian pelanggan pada input *"Cari Customer (kode / nama)"* (misalnya mengetik `"ABADI"`, kode `"ABD"`, atau `"CS0007"`), daftar pelanggan tidak menampilkan hasil sama sekali dan memunculkan status *"Tidak ditemukan pada cache lokal perangkat"*.

Meskipun tabel database cloud `BTRADE_Customer` memiliki ribuan baris data dan proses sinkronisasi referensi retur pada layar Sinkronisasi Data melaporkan status sukses hijau (*"Semua berhasil disinkronkan dengan server"*), data pelanggan yang tersimpan pada database lokal Room perangkat tetap kosong / tidak dapat dicari.

## Desired Outcome

1. **Pencarian Customer Berfungsi Normal**:
   - Operator dapat mencari dan memilih pelanggan berdasarkan kode maupun nama pelanggan pada layar pembuatan maupun edit Return Order (`CreateReturnOrderScreen` dan `EditReturnOrderScreen`).
   - Pencarian kata kunci seperti `"ABADI"` atau kode `"ABD"` menampilkan daftar pelanggan yang relevan dari cache lokal perangkat.
2. **Deserialisasi Data Customer Utuh**:
   - Saat proses sinkronisasi referensi retur berjalan, data pelanggan dari endpoint `GET /api/Customer/{serverId}` terdeserialisasi secara lengkap ke dalam model DTO tanpa kehilangan nilai kolom utama (`customerId`, `customerCode`, `customerName`, `alamat`, `serverId`).
   - Cache lokal Room (`customer_entity`) menyimpan entitas pelanggan secara valid sesuai data dari server.
3. **Robust Casing Support**:
   - Model penerima respon API (`CustomerDto` dan DTO terkait pada `ApiModels.kt`) mampu menerima format penamaan properti JSON baik berupa camelCase (standar serialisasi ASP.NET Core web API) maupun PascalCase secara konsisten.

## Current Situation

1. Endpoint server `GET /api/Customer/{serverId}` pada API mengembalikan payload JSON dengan penamaan properti berformat **camelCase** (contoh: `customerId`, `customerCode`, `customerName`, `alamat`, `serverId`).
2. Definisi `CustomerDto` pada aplikasi BGud (`src/BGud/app/src/main/java/com/elsasa/bgud/model/api/ApiModels.kt`) dikonfigurasi dengan anotasi `@SerializedName` berformat **PascalCase** eksklusif tanpa alternatif (contoh: `@SerializedName("CustomerId")`, `@SerializedName("CustomerCode")`, `@SerializedName("CustomerName")`).
3. Akibat ketidaksesuaian penamaan tersebut, parser Gson tidak menemukan kecocokan properti dan mengisi seluruh field `CustomerDto` dengan nilai default string kosong `""`.
4. Saat ditransformasikan ke `CustomerEntity` dan dimasukkan ke tabel Room `customer_entity`, seluruh data tertimpa ke dalam satu baris dengan primary key kosong `""` (`OnConflictStrategy.REPLACE`). Proses sinkronisasi selesai tanpa melempar exception sehingga status sinkronisasi tampak hijau/sukses, namun data pelanggan di cache lokal hilang.
5. Hal serupa juga teridentifikasi pada DTO referensi retur lainnya (`SalesPersonDto` dan `DriverDto`).

## Evidence

- [ApiModels.kt](file:///d:/Project.Private/btr-platform/src/BGud/app/src/main/java/com/elsasa/bgud/model/api/ApiModels.kt#L165-L208) — Definisi `CustomerDto`, `SalesPersonDto`, dan `DriverDto` menggunakan `@SerializedName` PascalCase:
  ```kotlin
  data class CustomerDto(
      @SerializedName("CustomerId") val customerId: String = "",
      @SerializedName("CustomerCode") val customerCode: String = "",
      @SerializedName("CustomerName") val customerName: String = "",
      @SerializedName("Alamat") val alamat: String = "",
      @SerializedName("ServerId") val serverId: String = ""
  )
  ```
- Respon mentah endpoint `http://dev.smart-ics.com:8089/belajar-api/api/Customer/JOG`:
  ```json
  {"status":"success","code":"200","data":[{"customerId":"CS0001","customerCode":"32","customerName":"32","alamat":"WIROSABAN","wilayah":"YOGYAKARTA","latitude":-7.8283195,"longitude":110.377866,"accuracy":13.916,"coordinateTimeStamp":1774644200740,"coordinateUser":"dwiyati.nuri24@gmail.com","isUpdated":false,"serverId":"JOG"}, ...]}
  ```
- [ReturnOrderReferenceSyncRepository.kt](file:///d:/Project.Private/btr-platform/src/BGud/app/src/main/java/com/elsasa/bgud/repository/ReturnOrderReferenceSyncRepository.kt#L112-L125) — `downloadCustomer` memetakan DTO ke Room Entity dan melakukan `upsertAll`.
- [CustomerDao.kt](file:///d:/Project.Private/btr-platform/src/BGud/app/src/main/java/com/elsasa/bgud/dao/CustomerDao.kt#L22-L28) — Pencarian lokal berbasis field `customerCode` dan `customerName`.

## Notes

1. **Solution-neutrality**: Dokumen ini mencatat fakta kegagalan pemuatan data pelanggan ke dalam antarmuka akibat kegagalan mapping serialisasi data, tanpa mendahului tahapan perencanaan atau perbaikan kode.
2. **Next Workflow Stage**:
   - Target rute alur kerja berikutnya: `ISSUE (BUG)` $\rightarrow$ `ica-analyst` atau `ica-architect` (tahap Bug Investigation / Architecture Update & Planning).
