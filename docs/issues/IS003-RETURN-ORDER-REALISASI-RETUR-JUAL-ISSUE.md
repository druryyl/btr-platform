# ISSUE

## Metadata

ID: BTR-RETURN-ORDER-REALISASI-001
Type: CHANGE-REQUEST
Status: Open
Title: Realisasi Order Retur Jual ke Retur Jual via ReturJualForm dan List Monitoring Return Order

## Source

Reported By: User
Reported Date: 2026-11-09

## Description

Saat ini data Intake Retur dari aplikasi gudang (`BtrReturGudang.Wpf`) disinkronkan ke kantor pusat (`j05-btr-distrib`) melalui modul sinkronisasi (`j07-btrade-sync`) dan tersimpan sebagai Order Retur Jual (tabel `BTR_ReturnOrder` dan `BTR_ReturnOrderItem`).

Data Order Retur Jual tersebut harus direalisasikan menjadi dokumen Retur Jual fisik/akuntansi (`BTR_ReturJual`). Mekanisme pembuatan retur jual melalui `GenerateReturnOrderForm.cs` saat ini menghadapi sejumlah kendala mendasar:

1. **Perbedaan Konsep Warehouse vs Depo**:
   - Pada aplikasi gudang (`BtrReturGudang.Wpf`), *Warehouse* merepresentasikan lokasi fisik gudang/depo.
   - Pada sistem distribusi pusat (`j05-btr-distrib`), lokasi fisik disebut *Depo*, sedangkan *Warehouse* adalah *logical location* (misalnya: gudang baik, gudang rusak).
   - Tidak ada mapping otomatis antara Warehouse fisik gudang dengan Warehouse logikal di `j05-btr-distrib`, sehingga pemilihan warehouse logikal penerima barang retur membutuhkan intervensi dan pertimbangan langsung dari user.

2. **Perbedaan Granularitas Jenis Retur (Kondisi Barang: Rusak vs Bagus)**:
   - Pada entitas Order Retur Jual (`BTR_ReturnOrderItem`), status kondisi barang (`JenisRetur`) tercatat pada baris detail per item.
   - Pada entitas Retur Jual (`BTR_ReturJual`), status `JenisRetur` tercatat pada header, dan aturan bisnis pembuatan mutasi stok (`GenStokReturJualWorker`) bergantung penuh pada nilai `JenisRetur` header ini.
   - Memindahkan `JenisRetur` dari header ke item pada entitas Retur Jual tidak diinginkan karena berdampak besar pada proses stok dan modul lainnya.

Perubahan alur diajukan sebagai berikut:
- Menggantikan proses generate otomatis di `GenerateReturnOrderForm.cs` dengan pendekatan input terpandu langsung pada form `ReturJualForm.cs`, dilengkapi input field `ReturnOrderId` (mengikuti pola relasi `OrderJualId` pada `FakturForm.cs`).
- Saat `ReturnOrderId` divalidasi, data header (Customer, Sales, Driver) dan item barang diambil dari Order Retur Jual terkait, dan user dapat menentukan Warehouse logikal yang tepat.
- Pengambilan item barang disesuaikan dengan `JenisRetur` (RUSAK / BAGUS):
  - Jika suatu Order Retur hanya memiliki satu status retur homogen, pilihan `JenisRetur` terisi otomatis dan seluruh item ditampilkan.
  - Jika suatu Order Retur memuat item heterogen (sebagian RUSAK dan sebagian BAGUS), user memilih salah satu `JenisRetur` terlebih dahulu sehingga hanya item berstatus sesuai yang dimuat ke grid detail. Pembentukan retur jual untuk status lainnya dilakukan dengan membuat entitas Retur Jual kedua untuk nomor order retur yang sama.
- Form `GenerateReturnOrderForm.cs` digantikan dengan form monitoring / list data Order Retur (serupa `ListOrderForm.cs`) yang menampilkan daftar order retur beserta status apakah sudah direalisasikan menjadi Retur Jual atau belum.

## Desired Outcome

- User dapat merealisasikan Order Retur Jual langsung dari [ReturJualForm.cs](file:///d:/Project.Private/btr-platform/src/j05-btr-distrib/btr.distrib/InventoryContext/ReturJualAgg/ReturJualForm.cs) dengan memasukkan `ReturnOrderId`.
- User memiliki fleksibilitas untuk memilih Warehouse logikal penerima retur secara eksplisit pada [ReturJualForm.cs](file:///d:/Project.Private/btr-platform/src/j05-btr-distrib/btr.distrib/InventoryContext/ReturJualAgg/ReturJualForm.cs).
- Pemilahan item berdasarkan `JenisRetur` berjalan rapi tanpa merombak skema detail `BTR_ReturJual` maupun generator stok:
  - Otomatis memilih status bila seluruh item berstatus seragam.
  - Memfilter item sesuai `JenisRetur` yang dipilih user bila order retur memuat item rusak dan bagus sekaligus, menghasilkan dua dokumen Retur Jual terpisah.
- Tersedia form monitoring daftar Order Retur (menggantikan `GenerateReturnOrderForm.cs`) yang memvisualisasikan seluruh dokumen Order Retur beserta status penyelesaian / realisasi retur jualnya (seperti analogi `ListOrderForm` terhadap `FakturForm`).

## Current Situation

- Proses pembuatan Retur Jual dari Order Retur saat ini diarahkan melalui [GenerateReturnOrderForm.cs](file:///d:/Project.Private/btr-platform/src/j05-btr-distrib/btr.distrib/InventoryContext/ReturnOrderAgg/GenerateReturnOrderForm.cs), namun terhambat karena tidak adanya pemetaan otomatis depo fisik ke warehouse logikal serta ketidaksesuaian penempatan atribut `JenisRetur` (detail di Order Retur vs header di Retur Jual).
- [ReturJualForm.cs](file:///d:/Project.Private/btr-platform/src/j05-btr-distrib/btr.distrib/InventoryContext/ReturJualAgg/ReturJualForm.cs) saat ini beroperasi mandiri secara manual tanpa kemampuan memuat referensi `ReturnOrderId`.
- Belum ada form list/monitoring status realisasi Order Retur yang representatif bagi user operasional.

## Evidence

- Form Realisasi Saat Ini: [GenerateReturnOrderForm.cs](file:///d:/Project.Private/btr-platform/src/j05-btr-distrib/btr.distrib/InventoryContext/ReturnOrderAgg/GenerateReturnOrderForm.cs)
- Form Target Input Retur Jual: [ReturJualForm.cs](file:///d:/Project.Private/btr-platform/src/j05-btr-distrib/btr.distrib/InventoryContext/ReturJualAgg/ReturJualForm.cs)
- Referensi Pola List & Form Faktur:
  - [ListOrderForm.cs](file:///d:/Project.Private/btr-platform/src/j05-btr-distrib/btr.distrib/SalesContext/OrderFeature/ListOrderForm.cs)
  - [FakturForm.cs](file:///d:/Project.Private/btr-platform/src/j05-btr-distrib/btr.distrib/SalesContext/FakturAgg/FakturForm.cs)
- Definisi Skema Tabel:
  - [BTR_ReturnOrder.sql](file:///d:/Project.Private/btr-platform/src/j05-btr-distrib/btr.sql/Tables/InventoryContext/BTR_ReturnOrder.sql)
  - [BTR_ReturnOrderItem.sql](file:///d:/Project.Private/btr-platform/src/j05-btr-distrib/btr.sql/Tables/InventoryContext/BTR_ReturnOrderItem.sql) (`JenisRetur` pada baris item)
  - [BTR_ReturJual.sql](file:///d:/Project.Private/btr-platform/src/j05-btr-distrib/btr.sql/Tables/InventoryContext/BTR_ReturJual.sql) (`JenisRetur` pada level header)
- Model Terkait:
  - [ReturnOrderModel.cs](file:///d:/Project.Private/btr-platform/src/j05-btr-distrib/btr.domain/InventoryContext/ReturnOrderAgg/ReturnOrderModel.cs) & [ReturnOrderItemModel.cs](file:///d:/Project.Private/btr-platform/src/j05-btr-distrib/btr.domain/InventoryContext/ReturnOrderAgg/ReturnOrderItemModel.cs)
  - [ReturJualModel.cs](file:///d:/Project.Private/btr-platform/src/j05-btr-distrib/btr.domain/InventoryContext/ReturJualAgg/ReturJualModel.cs)

## Notes

- Keputusan mempertahankan status `JenisRetur` di header `BTR_ReturJual` merupakan batasan arsitektur (constraint) yang disepakati untuk menghindari regresi dan modifikasi masif pada generator mutasi stok (`GenStokReturJualWorker`).
- Analisis rinci terkait pemodelan tracking status pemenuhan order retur (apakah memerlukan kolom status tambahan, relasi mapping Order-ReturJual, dsb.) akan dievaluasi pada tahap FEASIBILITY-ASSESSMENT oleh role Analyst (`ica-analyst`) dan perancangan teknis oleh Architect (`ica-architect`).
