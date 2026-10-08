# Investigation — Pilihan Satuan Kosong Setelah Scan Barcode di BGud

**Type:** Bug Investigation (Analyst + Architect) — Root Cause Analysis & Recommended Solution.  
**Surface:** Mobile App `BGud` (`src/BGud` → `CreateReturnOrderViewModel.kt`, `EditReturnOrderViewModel.kt`, `ReturnOrderCaptureRepository.kt`, `BarcodeDao.kt`, `CreateReturnOrderScreen.kt`).  
**Status:** Root cause confirmed against codebase and cloud sync schema.  
**Related Test Data:**  
- Barcode Value: `8998008152086`  
- Identified Item: `BIO MIWON 200 GR PREMIUM M`  
- Cloud Table: `BTRADE_BrgBarcode` (`Satuan` terdaftar)  

---

## 1. Executive Summary

Pengguna melaporkan bahwa pada aplikasi **BGud** (`src/BGud`), setelah melakukan sinkronisasi data dan memindai barcode (`8998008152086`), nama item berhasil teridentifikasi secara tepat sebagai **"BIO MIWON 200 GR PREMIUM M"**. Namun, ketika hendak memilih satuan (**Satuan** / *Unit*) pada kartu input item, dropdown pilihan satuan tidak menampilkan opsi apa pun (kosong), meskipun pada database cloud satuan barang tersebut telah terdaftar pada tabel `BTRADE_BrgBarcode`.

### Ringkasan Akar Masalah:
1. **Pilihan Satuan diisolasi hanya pada `BarangEntity` (`satKecil` & `satBesar`):**  
   Pada `CreateReturnOrderViewModel.kt` dan `EditReturnOrderViewModel.kt`, fungsi `unitOptions()` hanya membaca kolom `satKecil` dan `satBesar` dari entitas `BarangEntity` (`BTRADE_Brg`).
2. **Data Satuan Barang Master Kosong / Berada di Barcode Registry:**  
   Pada data master produk tertentu (seperti `BIO MIWON 200 GR PREMIUM M`), kolom `SatKecil` dan `SatBesar` pada tabel `BTRADE_Brg` bernilai kosong (`""`). Informasi satuan kemasan fisik dicatat pada tabel `BTRADE_BrgBarcode` (`Satuan`).
3. **Data `satuan` dari `BarcodeEntity` Dibuang Saat Scanning:**  
   Saat scanner mendeteksi barcode `8998008152086`, `CreateReturnOrderViewModel.onBarcodeScanned` berhasil mengambil `BarcodeEntity` dari Room (`barcodeDao.getByKey(...)`) yang **memiliki nilai `satuan`**. Namun, ViewModel langsung memanggil `selectPendingItem(item)` yang hanya menerima `BarangEntity`, mengabaikan `BarcodeEntity`, dan mereset `_pendingUnit` ke string kosong `""`.
4. **Validasi Repository Menolak Satuan di Luar `cachedUnits`:**  
   Pada `ReturnOrderCaptureRepository.kt`, fungsi `BarangEntity.cachedUnits()` juga hanya memvalidasi `satKecil` dan `satBesar`. Jika ada satuan dari `BTRADE_BrgBarcode`, penyimpanan draft akan ditolak dengan error `"Satuan tidak sesuai dengan data item."`.

---

## 2. Technical Evidence & Trace

### 2.1 Alur Pemindaian Barcode pada `CreateReturnOrderViewModel.kt`
```kotlin
fun onBarcodeScanned(rawValue: String?) {
    ...
    val barcode = try {
        barcodeDao.getByKey(BarcodeNormalization.toKey(displayValue))
    } catch (e: Exception) { null }
    ...
    val item = try {
        barangDao.getById(barcode.brgId)
    } catch (e: Exception) { null }
    ...
    selectPendingItem(item) // <-- barcode.satuan TIDAK DISIMPAN & TIDAK DIGUNAKAN
}
```

Pada `selectPendingItem`:
```kotlin
private fun selectPendingItem(item: BarangEntity) {
    _pendingItem.value = item
    _pendingQty.value = ""
    _pendingUnit.value = "" // <-- Wiped to empty
    _pendingJenisRetur.value = ""
    ...
}
```

### 2.2 Komputasi `unitOptions()` yang Terbatas
```kotlin
fun unitOptions(): List<String> {
    val item = _pendingItem.value ?: return emptyList()
    return listOf(item.satKecil, item.satBesar)
        .filter { it.isNotBlank() }
        .distinct()
}
```
Ketika `item.satKecil` dan `item.satBesar` kosong (karena data di `BTRADE_Brg` kosong), `unitOptions()` mengembalikan `emptyList()`. Akibatnya, `DropdownMenuItem` pada Compose `ExposedDropdownMenuBox` tidak merender satu pun item pilihan.

### 2.3 Validasi Penjaga Offline pada `ReturnOrderCaptureRepository.kt`
```kotlin
private fun BarangEntity.cachedUnits(): List<String> =
    listOf(satKecil, satBesar).filter { it.isNotBlank() }

...
val cachedUnits = item?.cachedUnits().orEmpty()
when {
    line.satId.isBlank() -> errors.add("$row: $UNIT_REQUIRED")
    item != null && line.satId !in cachedUnits ->
        errors.add("$row: $UNIT_INVALID")
}
```
Repository menolak satuan yang tidak ada di `satKecil`/`satBesar`, sehingga sekalipun UI mengizinkan input, penyimpanan offline tetap gagal.

---

## 3. Recommended Solution

1. **Tambahkan Query Satuan di `BarcodeDao`:**
   Tambahkan query di `BarcodeDao.kt` untuk mengambil seluruh satuan terdaftar dari barcode lokal untuk item terkait:
   ```kotlin
   @Query("SELECT DISTINCT satuan FROM barcode_entity WHERE brgId = :brgId AND satuan != ''")
   suspend fun getUnitsByBrg(brgId: String): List<String>
   ```

2. **Perluas Pilihan Satuan pada `CreateReturnOrderViewModel` & `EditReturnOrderViewModel`:**
   - Tambahkan state `_unitOptions = MutableStateFlow<List<String>>(emptyList())`.
   - Saat `selectPendingItem(item, preselectedUnit = null)` dipanggil:
     - Ambil satuan dari `barcodeDao.getUnitsByBrg(item.brgId)`.
     - Gabungkan dengan `listOf(item.satKecil, item.satBesar)`.
     - Filter non-blank dan `.distinct()`.
     - Perbarui `_unitOptions.value`.
   - Jika item dipilih via scan barcode (`onBarcodeScanned`):
     - Jika `barcode.satuan.isNotBlank()`, otomatis set `_pendingUnit.value = barcode.satuan` (auto-select unit dari kemasan fisik yang dipindai).
     - Jika pengguna ingin mengubah, opsi dropdown tetap berisi seluruh satuan yang valid untuk barang tersebut.

3. **Perbarui Validasi pada `ReturnOrderCaptureRepository`:**
   - Gunakan `database.barcodeDao().getUnitsByBrg(item.brgId)` digabung dengan `item.cachedUnits()` untuk menentukan daftar satuan yang valid.
   - Izinkan satuan yang terdaftar di `barcode_entity` agar draft tersimpan tanpa error `UNIT_INVALID`.

4. **Tambahkan Unit Test:**
   - Uji skenario di mana `BarangEntity` memiliki `satKecil` & `satBesar` kosong, tetapi memiliki barcode dengan `satuan` di `barcode_entity`.
   - Pastikan `unitOptions()` memuat satuan dari barcode.
   - Pastikan `onBarcodeScanned` melakukan auto-select terhadap `barcode.satuan`.
