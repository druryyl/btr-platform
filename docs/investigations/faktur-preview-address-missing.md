# Investigation — Faktur Preview Alamat Tidak Muncul

**Type:** Bug Investigation (Analyst + Architect) — Root Cause Analysis & Recommended Solution.  
**Surface:** Desktop `FakturForm` (`btr.distrib` → `SalesContext/FakturAgg/FakturForm.cs`).  
**Status:** Root cause confirmed against source code and RDLC template bindings.  
**Related Artifacts:**  
- `docs/features/faktur/feature.md` (Business Rules: Preview Before Save D-002, D-004, D-006, D-011)  
- `docs/work/faktur-preview-save-print-flow/IMPLEMENTATION-PLAN-SAVE-PREVIEW-FLOW.md`  
- `src/j05-btr-distrib/btr.distrib/SalesContext/FakturAgg/FakturForm.cs`  
- `src/j05-btr-distrib/btr.distrib/SalesContext/FakturAgg/FakturPrintOutDto.cs`  
- `src/j05-btr-distrib/btr.application/SalesContext/FakturAgg/Workers/FakturBuilder.cs`  
- `src/j05-btr-distrib/btr.infrastructure/SalesContext/FakturAgg/FakturDal.cs`  
- `src/j05-btr-distrib/btr.distrib/Reports/FakturPrintOut-Yk.rdlc` & `FakturPrintOut-Mgl.rdlc`  

---

## 1. Executive Summary

Pengguna melaporkan bahwa saat melakukan preview Faktur Penjualan di `FakturForm`, informasi **ALAMAT** customer tidak muncul (kosong).

Hasil investigasi mengidentifikasi **dua akar masalah (root cause)** yang saling berkaitan:
1. **`FakturBuilder.Customer()` tidak menyalin `Address1` (dan atribut customer lainnya) ke `FakturModel`**:  
   Pada alur *preview before save*, `previewAggregate` dibuat di memori melalui `BuildFakturAggregateWorker` → `FakturBuilder.Customer(req)`. Walaupun `customer` berhasil diambil dari database via `_customerDal.GetData()`, `FakturBuilder.Customer()` hanya menyalin `CustomerId`, `CustomerName`, `Plafond`, `CreditBalance`, dan `HargaTypeId`. Properti `_aggRoot.Address` (serta `Kota`, `CustomerCode`, `Npwp`, `Nitku`) **tidak pernah diisi** sehingga nilainya tetap `null`.
2. **`FakturPrintOutDto` hanya membaca `faktur.Address` tanpa fallback ke `customer.Address1`**:  
   Konstruktor `FakturPrintOutDto(faktur, customer, user, isKlaim)` menerima objek `CustomerModel customer`, namun properti `Address1` di-set secara kaku dengan `Address1 = $"{faktur.Address}";`. Karena `faktur.Address` bernilai `null` pada in-memory aggregate preview, `Address1` menjadi string kosong `""`. Selain itu, jika `Address1` kosong, tidak ada fallback ke `customer.Address1` maupun fallback `"-"` sesuai aturan D-004 (`docs/features/faktur/feature.md`).

Sebaliknya, pada alur cetak setelah simpan (*post-save print*), data diambil dari database via `FakturDal.GetData(fakturDb)` yang mengeksekusi `LEFT JOIN BTR_Customer cc ON aa.CustomerId = cc.CustomerId` dengan `ISNULL(cc.Address1, '') AS Address`. Oleh karena itu, faktur yang di-reload dari database memiliki `Address`, sedangkan preview in-memory draft sebelum simpan kehilangan alamat.

---

## 2. Technical Evidence & Trace

### 2.1 Alur Preview Pada `FakturForm.cs`

Pada `FakturForm.cs` (metode `SaveButton_Click` baris 959–991 dan `PreviewVoidButton_Click` baris 1108–1139):
```csharp
// 1. Bangun in-memory aggregate draft
previewAggregate = _aggregateBuilder.Execute(req);

// 2. Baca customer untuk keperluan tampilan
CustomerModel previewCustomer = null;
try
{
    previewCustomer = _customerDal.GetData(previewAggregate);
}
catch (KeyNotFoundException) { }

// 3. Bangun DTO cetak/preview
var previewDto = new FakturPrintOutDto(previewAggregate, previewCustomer, previewUser, isKlaim);
var choice = ShowPreviewDialog(previewDto);
```

### 2.2 Kelalaian Properti pada `FakturBuilder.cs`

Pada `FakturBuilder.Customer(ICustomerKey customerKey)` (`src/j05-btr-distrib/btr.application/SalesContext/FakturAgg/Workers/FakturBuilder.cs` baris 180–190):
```csharp
public IFakturBuilder Customer(ICustomerKey customerKey)
{
    var customer = _customerDal.GetData(customerKey)
                   ?? throw new KeyNotFoundException($"CustomerId not found ({customerKey.CustomerId})");
    _aggRoot.CustomerId = customer.CustomerId;
    _aggRoot.CustomerName = customer.CustomerName;
    _aggRoot.Plafond = customer.Plafond;
    _aggRoot.CreditBalance = customer.CreditBalance;
    _aggRoot.HargaTypeId = customer.HargaTypeId;
    // MISSING:
    // _aggRoot.Address = customer.Address1;
    // _aggRoot.Kota = customer.Kota;
    // _aggRoot.CustomerCode = customer.CustomerCode;
    // _aggRoot.Npwp = customer.Npwp;
    // _aggRoot.Nitku = customer.Nitku;
    return this;
}
```
Akibatnya: `previewAggregate.Address` selalu `null`.

### 2.3 Pembacaan `Address1` pada `FakturPrintOutDto.cs`

Pada `FakturPrintOutDto.cs` (baris 21–30):
```csharp
CustomerId = $"Kepada Yth Customer-{faktur.CustomerId}";
CustomerName = $"{faktur.CustomerName}";
Address1 = $"{faktur.Address}"; // <--- Langsung mengambil faktur.Address (yang bernilai null saat preview)
var address2 = customer?.Address2;
var kota = customer?.Kota;
var hasAddress2 = !string.IsNullOrWhiteSpace(address2);
var hasKota = !string.IsNullOrWhiteSpace(kota);
Address2 = hasAddress2 && hasKota ? $"{address2}-{kota}"
    : hasAddress2 ? $"{address2}"
    : hasKota ? $"{kota}" : "-";
```
Perhatikan bahwa `customer` (yaitu `previewCustomer`) yang dikirim ke `FakturPrintOutDto` memiliki `customer.Address1` yang valid dari database, tetapi `FakturPrintOutDto` mengabaikannya untuk `Address1` dan hanya bergantung pada `faktur.Address`.

### 2.4 Tampilan Template RDLC (`FakturPrintOut-Yk.rdlc` & `FakturPrintOut-Mgl.rdlc`)

Pada file report RDLC:
- `Address1Text`: `<Value>=First(Fields!Address1.Value, "FakturJualDataset")</Value>`
- `Address2Text`: `<Value>=First(Fields!Address2.Value, "FakturJualDataset")</Value>`

Karena `Address1` bernilai `""`, baris alamat utama tidak muncul sama sekali di layar preview.

---

## 3. Recommended Solution

Solusi dirancang dengan pertahanan ganda (*defense-in-depth*) untuk memastikan konsistensi domain aggregate dan keandalan presentasi DTO:

### 3.1 Perbaikan pada Domain/Application Builder (`FakturBuilder.cs`)
Lengkapi pemetaan customer pada `FakturBuilder.Customer()` agar menyalin seluruh atribut denormalisasi customer ke `_aggRoot`:
```csharp
public IFakturBuilder Customer(ICustomerKey customerKey)
{
    var customer = _customerDal.GetData(customerKey)
                   ?? throw new KeyNotFoundException($"CustomerId not found ({customerKey.CustomerId})");
    _aggRoot.CustomerId = customer.CustomerId;
    _aggRoot.CustomerName = customer.CustomerName;
    _aggRoot.CustomerCode = customer.CustomerCode;
    _aggRoot.Plafond = customer.Plafond;
    _aggRoot.CreditBalance = customer.CreditBalance;
    _aggRoot.HargaTypeId = customer.HargaTypeId;
    _aggRoot.Address = customer.Address1;
    _aggRoot.Kota = customer.Kota;
    _aggRoot.Npwp = customer.Npwp;
    _aggRoot.Nitku = customer.Nitku;
    return this;
}
```
**Manfaat:** Memastikan setiap `FakturModel` yang dibangun via `FakturBuilder` (baik mode baru, edit, maupun preview) memiliki atribut yang identik dengan `FakturModel` hasil query `FakturDal.GetData()`.

### 3.2 Perbaikan pada Presentation DTO (`FakturPrintOutDto.cs`)
Jadikan pembacaan `Address1` toleran dan null-safe sesuai aturan D-004:
```csharp
var address1 = !string.IsNullOrWhiteSpace(faktur?.Address)
    ? faktur.Address
    : customer?.Address1;
Address1 = string.IsNullOrWhiteSpace(address1) ? "-" : address1;
```
**Manfaat:** Menjamin bahwa jika salah satu sumber tidak memiliki data, data tetap diambil dari sumber alternatif (`customer.Address1` atau `faktur.Address`), dan jika keduanya kosong tetap menampilkan fallback `"-"` tanpa mengganggu kelancaran preview.

---

## 4. Impact Analysis & Risks

- **Perubahan Database:** Tidak ada. Tabel `BTR_Faktur` tidak menyimpan kolom alamat secara terpisah (selalu join ke `BTR_Customer`).
- **Alur Penyimpanan (Save Flow):** Tidak berubah. `SaveFakturWorker`, `FakturWriter`, pengurangan stok, dan pembentukan piutang tetap berjalan persis seperti sebelumnya.
- **Kompatibilitas:** Memperbaiki bug tampilan alamat pada preview Faktur Baru, Faktur Edit, Faktur Klaim, dan Faktur Void.
- **Risiko Regresi:** Sangat rendah (0 perubahan skema, 0 perubahan dependensi eksternal, hanya melengkapi properti yang terlewat).
