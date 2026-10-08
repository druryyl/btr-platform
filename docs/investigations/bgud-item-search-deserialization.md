# Investigation — Pencarian Item Manual Tidak Muncul di Buat Return Order BGud

**Type:** Bug Investigation (Analyst + Architect) — Root Cause Analysis & Recommended Solution.  
**Surface:** Mobile App `BGud` (`src/BGud` → `CreateReturnOrderScreen.kt`, `BarangDao.kt`, `ApiModels.kt`, `BarcodeSyncRepository.kt`).  
**Status:** Root cause confirmed against source code and API serialization behavior.  
**Related Artifacts:**  
- `docs/issues/CR-002-BUG-INVESTIGATION.md` (Precedent: Customer, SalesPerson, Driver DTO casing mismatch)  
- `docs/work/bgud-customer-deserialization-fix/CR-002-IMPLEMENTATION-PLAN.md`  
- `src/BGud/app/src/main/java/com/elsasa/bgud/model/api/ApiModels.kt`  
- `src/BGud/app/src/main/java/com/elsasa/bgud/repository/BarcodeSyncRepository.kt`  
- `src/BGud/app/src/main/java/com/elsasa/bgud/dao/BarangDao.kt`  
- `src/BGud/app/src/main/java/com/elsasa/bgud/ui/screen/CreateReturnOrderScreen.kt`  
- `src/j06-pkl-btrade-api/btrade.webapi/Controllers/BrgController.cs`  

---

## 1. Executive Summary

Pengguna melaporkan bahwa saat membuka layar **Buat Return Order** di aplikasi Android **BGud** (`src/BGud`), pencarian item barang manual (misalnya mengetik kata *"regal"*) tidak memunculkan data barang sama sekali (*"Tidak ditemukan pada cache lokal perangkat."*).

Hasil investigasi mengidentifikasi bahwa masalah ini memiliki **akar masalah yang identik dengan insiden CR-002** (yang sebelumnya menimpa data Customer, SalesPerson, dan Driver), di mana DTO barang master `BrgDto` (dan `BarcodeDto`) di `ApiModels.kt` terlewat dan belum diperbaiki saat CR-002 diselesaikan.

### Ringkasan Akar Masalah:
1. **JSON Field Naming Mismatch (`camelCase` vs `PascalCase`):**  
   Server ASP.NET Core (`btrade.webapi` → `BrgController.cs`) secara default mengonfigurasi serialisasi JSON menggunakan standar penamaan properti **camelCase** (`System.Text.Json` default di `services.AddControllers()`). Respon `GET /api/Brg/{serverId}` mengirimkan field: `"brgId"`, `"brgCode"`, `"brgName"`, `"kategoriName"`, `"satBesar"`, `"satKecil"`, `"konversi"`, `"hrgSat"`, `"stok"`, `"serverId"`.
2. **DTO `BrgDto` pada BGud Menggunakan Anotasi PascalCase Mutlak Tanpa `alternate`:**  
   Pada `src/BGud/app/src/main/java/com/elsasa/bgud/model/api/ApiModels.kt`, model `BrgDto` dianotasikan dengan `@SerializedName("BrgId")`, `@SerializedName("BrgName")`, dll. (PascalCase). Karena parser Gson bersifat case-sensitive, Gson tidak menemukan field tersebut di payload JSON, sehingga seluruh properti diisi dengan nilai default konstruktor Kotlin (string kosong `""`).
3. **Data Corruption Semantik pada Cache Lokal SQLite Room (`barang_entity`):**  
   Saat proses sinkronisasi master barang dijalankan (`BarcodeSyncRepository.downloadBarang`), ribuan item dari server yang semuanya memiliki `brgId = ""` dipetakan ke `BarangEntity` dan di-upsert ke database Room SQLite. Karena `brgId` adalah `@PrimaryKey`, seluruh record tersebut saling menimpa satu sama lain dengan strategi `OnConflictStrategy.REPLACE`, menyisakan tepat **1 baris korup** dengan `brgId = ""` dan `brgName = ""`.
4. **Pencarian Query Selalu Kosong:**  
   Ketika operator mengetik `"regal"` pada kolom pencarian `CreateReturnOrderScreen`, query Room `barangDao.search("regal")` (`WHERE brgCode LIKE '%regal%' OR brgName LIKE '%regal%'`) dieksekusi terhadap tabel `barang_entity` yang hanya berisi 1 baris kosong, sehingga tidak ada item yang ditemukan.

---

## 2. Technical Evidence & Trace

### 2.1 Alur Pencarian Barang pada `CreateReturnOrderScreen.kt` & `CreateReturnOrderViewModel.kt`

Pada `CreateReturnOrderScreen.kt` (baris 380–391):
```kotlin
// Manual Item Search
ReferenceSearchSection(
    label = "Cari Item Manual (kode / nama barang)",
    query = itemQuery,
    results = itemResults,
    isSearching = isSearchingItem,
    enabled = !isSaving,
    onQueryChange = viewModel::onItemQueryChange,
    onSelect = viewModel::onSelectItem,
    rowHeadline = { it.brgName },
    rowSupporting = { it.brgCode }
)
```

Pada `CreateReturnOrderViewModel.kt` (baris 345–369):
```kotlin
fun onItemQueryChange(value: String) {
    _itemQuery.value = value
    ...
    itemSearchJob = viewModelScope.launch {
        delay(SEARCH_DEBOUNCE_MILLIS)
        try {
            _itemResults.value = barangDao.search(query)
        } catch (e: Exception) {
            _itemResults.value = emptyList()
        }
    }
}
```

Pada `BarangDao.kt` (baris 26–32):
```kotlin
@Query(
    "SELECT * FROM barang_entity " +
        "WHERE brgCode LIKE '%' || :query || '%' " +
        "OR brgName LIKE '%' || :query || '%' " +
        "ORDER BY brgName LIMIT 50"
)
suspend fun search(query: String): List<BarangEntity>
```

### 2.2 Serialisasi Backend API (`BrgController.cs`)

Pada `src/j06-pkl-btrade-api/btrade.webapi/Controllers/BrgController.cs` (baris 19–26):
```csharp
[HttpGet]
[Route("{serverId}")]
public async Task<IActionResult> ListData(string serverId)
{
    var query = new BrgListDataQuery(serverId);
    var response = await _mediator.Send(query);
    return Ok(new JSendOk(response));
}
```
Metode `Ok(...)` mengembalikan respons JSON menggunakan `System.Text.Json` bawaan ASP.NET Core yang dikonfigurasi melalui `services.AddControllers()`. Konfigurasi default runtime .NET ini menghasilkan properti JSON dalam format **camelCase**:
```json
{
  "status": "success",
  "code": "200",
  "data": [
    {
      "brgId": "BRG0001",
      "brgCode": "RG001",
      "brgName": "REGAL MARIE BISKUIT 250G",
      "kategoriName": "BISKUIT",
      "satBesar": "KARTON",
      "satKecil": "BKS",
      "konversi": 24,
      "hrgSat": 18500.0,
      "stok": 120,
      "serverId": "JOG"
    }
  ]
}
```

### 2.3 Definisi DTO pada `ApiModels.kt` yang Mengalami Casing Mismatch

Pada `src/BGud/app/src/main/java/com/elsasa/bgud/model/api/ApiModels.kt` (baris 217–228):
```kotlin
data class BrgDto(
    @SerializedName("BrgId") val brgId: String = "",
    @SerializedName("BrgCode") val brgCode: String = "",
    @SerializedName("BrgName") val brgName: String = "",
    @SerializedName("KategoriName") val kategoriName: String = "",
    @SerializedName("SatBesar") val satBesar: String = "",
    @SerializedName("SatKecil") val satKecil: String = "",
    @SerializedName("Konversi") val konversi: Int = 0,
    @SerializedName("HrgSat") val hrgSat: Double = 0.0,
    @SerializedName("Stok") val stok: Int = 0,
    @SerializedName("ServerId") val serverId: String = ""
)
```
Karena `@SerializedName` bernilai `"BrgId"` dan tidak memiliki atribut `alternate = ["brgId"]` (atau sebaliknya):
- Gson mencari key `"BrgId"`, tetapi payload JSON menyediakan `"brgId"`.
- Gson mengabaikan nilai tersebut dan mengembalikan instance `BrgDto` dengan nilai default (`brgId = ""`, `brgName = ""`, dll.).

### 2.4 Kerentanan Serupa pada `BarcodeDto` dan `RegistrationStatusDto`

1. **`BarcodeDto` (baris 66–74):**
   ```kotlin
   data class BarcodeDto(
       @SerializedName("BrgBarcodeId") val brgBarcodeId: String = "",
       @SerializedName("BarcodeValue") val barcodeValue: String = "",
       @SerializedName("BrgId") val brgId: String = "",
       @SerializedName("BrgCode") val brgCode: String = "",
       @SerializedName("BrgName") val brgName: String = "",
       @SerializedName("Satuan") val satuan: String = "",
       @SerializedName("ServerId") val serverId: String = ""
   )
   ```
   Rute `GET /api/barcodes/sync` di `BarcodeController.cs` juga mengembalikan camelCase (`brgBarcodeId`, `barcodeValue`, dll.). Semua barcode yang terunduh juga tertimpa menjadi 1 baris kosong pada `barcode_entity`.
2. **`RegistrationStatusDto` (baris 98–106):**
   ```kotlin
   data class RegistrationStatusDto(
       @SerializedName("BarcodeRegistrationId") val barcodeRegistrationId: String = "",
       @SerializedName("ClientRequestId") val clientRequestId: String = "",
       @SerializedName("BarcodeValue") val barcodeValue: String = "",
       @SerializedName("BrgId") val brgId: String = "",
       @SerializedName("Satuan") val satuan: String = "",
       @SerializedName("Status") val status: String = "",
       @SerializedName("ProcessedNote") val processedNote: String = ""
   )
   ```
   Rute `GET /api/BarcodeRegistration/status` juga mengembalikan camelCase, sehingga refresh status pendaftaran barcode tidak pernah memproses `clientRequestId`.

---

## 3. Reproduction Steps

1. Pastikan aplikasi BGud terhubung ke server backend atau mock API yang mengembalikan format JSON standar ASP.NET Core (`camelCase`).
2. Masuk / login ke akun operator dan pilih gudang.
3. Lakukan sinkronisasi data pada layar *Sinkronisasi Data*.
4. Buka menu **Buat Return Order** (`CreateReturnOrderScreen`).
5. Pada bagian *"2. SCAN & INPUT ITEM RETUR"*, ketik `"regal"` pada kolom *"Cari Item Manual (kode / nama barang)"*.
6. **Hasil:** Muncul teks *"Tidak ditemukan pada cache lokal perangkat."*, meskipun barang Regal ada di database server.

---

## 4. Affected Components

- **Mobile Codebase (`src/BGud`):**
  - `src/BGud/app/src/main/java/com/elsasa/bgud/model/api/ApiModels.kt`:
    - `BrgDto`: Tambahkan dukungan dual-casing (`camelCase` primer, `PascalCase` alternatif).
    - `BarcodeDto`: Tambahkan dukungan dual-casing (`camelCase` primer, `PascalCase` alternatif).
    - `RegistrationStatusDto`: Tambahkan dukungan dual-casing (`camelCase` primer, `PascalCase` alternatif).
  - `src/BGud/app/src/main/java/com/elsasa/bgud/repository/BarcodeSyncRepository.kt`:
    - Tambahkan filter pertahanan `.filter { it.brgId.isNotBlank() }` pada `downloadBarang`.
    - Tambahkan filter pertahanan `.filter { it.brgBarcodeId.isNotBlank() }` pada `downloadBarcodes`.
  - `src/BGud/app/src/test/java/com/elsasa/bgud/model/ReferenceDtoDeserializationTest.kt`:
    - Tambahkan unit test deserialisasi untuk `BrgDto` dan `BarcodeDto` baik dari camelCase maupun PascalCase.

---

## 5. Recommended Solution

Menerapkan pola **Dual-casing Resiliency** via Gson `@SerializedName(..., alternate = [...])` persis seperti yang telah diterapkan pada `CustomerDto`, `SalesPersonDto`, dan `DriverDto` di CR-002:

```kotlin
data class BrgDto(
    @SerializedName("brgId", alternate = ["BrgId"]) val brgId: String = "",
    @SerializedName("brgCode", alternate = ["BrgCode"]) val brgCode: String = "",
    @SerializedName("brgName", alternate = ["BrgName"]) val brgName: String = "",
    @SerializedName("kategoriName", alternate = ["KategoriName"]) val kategoriName: String = "",
    @SerializedName("satBesar", alternate = ["SatBesar"]) val satBesar: String = "",
    @SerializedName("satKecil", alternate = ["SatKecil"]) val satKecil: String = "",
    @SerializedName("konversi", alternate = ["Konversi"]) val konversi: Int = 0,
    @SerializedName("hrgSat", alternate = ["HrgSat"]) val hrgSat: Double = 0.0,
    @SerializedName("stok", alternate = ["Stok"]) val stok: Int = 0,
    @SerializedName("serverId", alternate = ["ServerId"]) val serverId: String = ""
)

data class BarcodeDto(
    @SerializedName("brgBarcodeId", alternate = ["BrgBarcodeId"]) val brgBarcodeId: String = "",
    @SerializedName("barcodeValue", alternate = ["BarcodeValue"]) val barcodeValue: String = "",
    @SerializedName("brgId", alternate = ["BrgId"]) val brgId: String = "",
    @SerializedName("brgCode", alternate = ["BrgCode"]) val brgCode: String = "",
    @SerializedName("brgName", alternate = ["BrgName"]) val brgName: String = "",
    @SerializedName("satuan", alternate = ["Satuan"]) val satuan: String = "",
    @SerializedName("serverId", alternate = ["ServerId"]) val serverId: String = ""
)

data class RegistrationStatusDto(
    @SerializedName("barcodeRegistrationId", alternate = ["BarcodeRegistrationId"]) val barcodeRegistrationId: String = "",
    @SerializedName("clientRequestId", alternate = ["ClientRequestId"]) val clientRequestId: String = "",
    @SerializedName("barcodeValue", alternate = ["BarcodeValue"]) val barcodeValue: String = "",
    @SerializedName("brgId", alternate = ["BrgId"]) val brgId: String = "",
    @SerializedName("satuan", alternate = ["Satuan"]) val satuan: String = "",
    @SerializedName("status", alternate = ["Status"]) val status: String = "",
    @SerializedName("processedNote", alternate = ["ProcessedNote"]) val processedNote: String = ""
)
```

Solusi ini bersifat non-breaking, 100% kompatibel ke belakang, dan langsung menyelesaikan masalah hilangnya data item saat pencarian manual maupun scanning.
