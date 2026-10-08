# Implementation Plan — Perbaikan Pilihan Satuan Barcode di BGud

**Task:** Fix Missing Unit Options on Scanned Item in BGud  
**Reference Investigation:** `docs/investigations/bgud-barcode-satuan-missing.md`  
**Target Codebase:** `src/BGud`  

---

## 1. Scope & Objective

Memperbaiki alur pemilihan satuan (*Satuan*) setelah pemindaian barcode maupun pencarian manual di aplikasi BGud (`CreateReturnOrderScreen` & `EditReturnOrderScreen`):
1. Mengambil seluruh satuan terdaftar pada tabel lokal `barcode_entity` (`BTRADE_BrgBarcode`) untuk barang terkait, digabungkan dengan `satKecil` dan `satBesar` dari `barang_entity`.
2. Melakukan pre-selection otomatis pada field `Satuan` jika barcode yang dipindai memiliki data `satuan`.
3. Memastikan validasi offline pada `ReturnOrderCaptureRepository` mengakomodasi satuan yang berasal dari `barcode_entity`.

---

## 2. Proposed Changes

### Slice 1: DAO Support (`BarcodeDao.kt`)
- Tambahkan query untuk mengambil daftar satuan unik:
  ```kotlin
  @Query("SELECT DISTINCT satuan FROM barcode_entity WHERE brgId = :brgId AND satuan != ''")
  suspend fun getUnitsByBrg(brgId: String): List<String>
  ```

### Slice 2: Repository Offline Guardrails (`ReturnOrderCaptureRepository.kt`)
- Pada `ReturnOrderCaptureRepository`, saat memvalidasi `line.satId`:
  - Ambil satuan dari `database.barcodeDao().getUnitsByBrg(item.brgId)`.
  - Gabungkan dengan `listOf(item.satKecil, item.satBesar).filter { it.isNotBlank() }`.
  - Satuan valid jika ada di dalam daftar gabungan tersebut.

### Slice 3: ViewModel Enhancements (`CreateReturnOrderViewModel.kt` & `EditReturnOrderViewModel.kt`)
- Tambahkan `_unitOptions = MutableStateFlow<List<String>>(emptyList())` dan StateFlow `unitOptions`.
- Jadikan `unitOptions(): List<String> = _unitOptions.value` agar UI Compose dan binding tetap kompatibel.
- Pada `onBarcodeScanned`:
  - Saat `barcode` ditemukan, ambil `barcode.satuan`.
  - Panggil `selectPendingItem(item, preselectedUnit = barcode.satuan)`.
  - Muat opsi satuan dari `barcodeDao.getUnitsByBrg(item.brgId)` + satuan barang, perbarui `_unitOptions`.
- Pada `onSelectItem` (pencarian manual):
  - Muat opsi satuan dari `barcodeDao.getUnitsByBrg(item.brgId)` + satuan barang, perbarui `_unitOptions`.
  - Jika hanya ada 1 opsi satuan, dapat dipertimbangkan untuk langsung dipilih secara default.

### Slice 4: Unit Testing
- Tambahkan unit test pada `CreateReturnOrderViewModelTest` / `CaptureViewModelTest`:
  - Uji scan barcode dengan item tanpa `satKecil`/`satBesar` tetapi barcode memiliki `satuan`.
  - Verifikasi bahwa `unitOptions()` memuat satuan dari barcode dan `pendingUnit` terisi otomatis.
  - Verifikasi bahwa `addItem()` berhasil dan tidak ditolak.

---

## 3. Verification & Criteria
- Gradle build dan test unit lulus: `./gradlew testDebugUnitTest`.
- Skenario pengujian pengguna:
  - Barcode: `8998008152086`
  - Item: `BIO MIWON 200 GR PREMIUM M`
  - Satuan: Muncul di dropdown dan terisi otomatis sesuai satuan dari `BTRADE_BrgBarcode`.
