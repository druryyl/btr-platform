---
Title: Feasibility Assessment - Realisasi Order Retur Jual ke Retur Jual via ReturJualForm dan List Monitoring Return Order
Code: BTR-RETURN-ORDER-REALISASI-001
Artifact: FEASIBILITY-ASSESSMENT
Version: 1.0
LastUpdated: 2026-11-09
Status: READY-FOR-PLANNING
---

# 1. Request Summary

Penilaian kelayakan (Feasibility Assessment) ini menganalisis implementasi perubahan mekanisme realisasi Order Retur Jual (`BTR_ReturnOrder`) menjadi dokumen Retur Jual fisik/akuntansi (`BTR_ReturJual`), sebagaimana diajukan pada issue `docs/issues/IS003-RETURN-ORDER-REALISASI-RETUR-JUAL-ISSUE.md`.

Referenced artifacts:

- ISSUE: `docs/issues/IS003-RETURN-ORDER-REALISASI-RETUR-JUAL-ISSUE.md`
- DOMAIN: `docs/work/return-order/RETURN-ORDER-DOMAIN.md`
- ARCHITECTURE (Baseline): `docs/work/return-order/RETURN-ORDER-ARCHITECTURE.md`

## Objective

Menilai kelayakan teknis, arsitektur, dan operasional dalam mengalihkan proses realisasi retur dari yang sebelumnya melalui form otomatisasi kaku (`GenerateReturnOrderForm.cs`) menjadi alur input terpandu langsung pada `ReturJualForm.cs` (mengadopsi pola relasi `OrderJualId` pada `FakturForm.cs`) serta menyediakan form monitoring daftar Order Retur (`ListReturnOrderForm.cs`). Penilaian berlandaskan pada 3 prinsip utama: **Pragmatis**, **Explicit**, dan **Minimalist**.

---

# 2. Current State

## Existing Behavior

1. **Intake & Sinkronisasi**: Dokumen Order Retur dicatat oleh petugas gudang via aplikasi gudang (`BtrReturGudang.Wpf`) dan disinkronkan ke sistem distribusi kantor pusat (`j05-btr-distrib`) melalui `j07-btrade-sync`, tersimpan pada tabel `BTR_ReturnOrder` dan `BTR_ReturnOrderItem` dengan status awal `'Synced'`.
2. **Upaya Realisasi Saat Ini**: Sistem pusat sebelumnya menyediakan `GenerateReturnOrderForm.cs` yang memanggil `GenerateSalesReturnFromReturnOrderCommand`. Command ini berusaha mengelompokkan item order secara otomatis lalu membentuk dokumen `BTR_ReturJual` dan langsung mengubah status `BTR_ReturnOrder` menjadi `'Imported'`.
3. **Form Retur Jual Eksisting**: `ReturJualForm.cs` saat ini beroperasi mandiri secara manual untuk menginput dokumen retur jual (RT1-Retur Jual). Form ini belum memiliki field input atau kemampuan memuat data dari `ReturnOrderId`.
4. **Pola Referensi di Sistem**: Pola relasi dokumen operasional ke dokumen transaksi yang sudah matang di sistem adalah alur Sales Order (`BTR_Order`) ke Faktur Penjualan (`BTR_Faktur`):
   - `ListOrderForm.cs` berfungsi sebagai form monitoring daftar order beserta status realisasinya.
   - User dapat melakukan double-click pada baris order di `ListOrderForm.cs` untuk membuka `FakturForm.cs` dan memanggil `fakturForm.LoadOrder(orderId)`.
   - `FakturForm.cs` memuat data header dan item ke grid secara terpandu, dan user melengkapi informasi pengiriman serta gudang logikal.

## Existing Constraints

1. **Warehouse Logikal vs Fisik**: Pada gudang cabang (`BtrReturGudang.Wpf`), lokasi fisik dicatat sebagai *Warehouse*. Di kantor pusat (`j05-btr-distrib`), lokasi fisik disebut *Depo*, sedangkan *Warehouse* adalah partisi logikal (misalnya gudang barang baik vs gudang barang rusak). Tidak ada mapping 1-to-1 otomatis antara depo fisik dengan warehouse logikal pusat; penentuan warehouse logikal harus eksplisit dipilih oleh user operasional kantor pusat.
2. **Granularitas JenisRetur**: 
   - Pada Order Retur (`BTR_ReturnOrderItem`), atribut `JenisRetur` berada pada detail item.
   - Pada Retur Jual (`BTR_ReturJual`), atribut `JenisRetur` berada pada header dokumen, dan pembuat mutasi stok (`GenStokReturJualWorker`) bergantung penuh pada nilai header ini.
   - **Batasan Arsitektur**: Struktur header `JenisRetur` pada `BTR_ReturJual` tidak boleh diubah menjadi level baris demi menjaga stabilitas generator stok dan menghindari regresi luas pada modul persediaan.
3. **Multi-Dokumen Retur Jual dari Satu Order Retur**: Jika satu Order Retur memiliki item heterogen (memuat item BAGUS dan item RUSAK sekaligus), order tersebut harus dipecah menjadi 2 dokumen Retur Jual terpisah.

---

# 3. Gap Analysis

| ID | Severity | Gap |
|------|------|------|
| GAP-001 | CRITICAL | Tabel `BTR_ReturJual` dan model `ReturJualModel` belum memiliki atribut `ReturnOrderId` untuk menyimpan referensi dokumen asal. |
| GAP-002 | CRITICAL | `ReturJualForm.cs` belum memiliki input field `ReturnOrderId` dan method `LoadReturnOrder(returnOrderId)` untuk menarik data header dan item dari Order Retur. |
| GAP-003 | MAJOR | `GenerateReturnOrderForm.cs` tidak fleksibel dan gagal mengakomodasi pemilihan warehouse logikal serta pemilahan `JenisRetur` secara interaktif. |
| GAP-004 | MAJOR | Belum ada form monitoring representatif (`ListReturnOrderForm.cs`) yang menampilkan daftar Order Retur beserta status penyelesaiannya (`Synced`, `Partial`, `Imported`) dan nomor Retur Jual terkait. |
| GAP-005 | MAJOR | Siklus status `BTR_ReturnOrder.Status` belum mendukung status antara (`Partial`) ketika baru satu dari dua `JenisRetur` (BAGUS/RUSAK) yang direalisasikan. |
| GAP-006 | MINOR | Belum ada mekanisme sinkronisasi balik (revert status) pada `BTR_ReturnOrder` ketika dokumen `BTR_ReturJual` yang bersumber darinya dibatalkan (di-void). |

---

# 4. Open Questions

| ID | Question | Impact |
|------|------|------|
| OQ-001 | Bagaimana model penyimpanan relasi antara `ReturnOrder` dan `ReturJual` serta mekanisme tracking statusnya? | Menentukan apakah perlu tabel perantara baru atau cukup menambah kolom `ReturnOrderId` di `BTR_ReturJual`. |
| OQ-002 | Bagaimana aturan pengeditan item dan evaluasi status `Partial` vs `Imported` saat data Order Retur dimuat ke `ReturJualForm`? | Menentukan fleksibilitas pengeditan kuantitas di grid dan logika penetapan transisi status Order Retur saat simpan. |
| OQ-003 | Bagaimana alur interaksi UI pada `ReturJualForm` untuk memilah item pada Order Retur heterogen (memuat BAGUS dan RUSAK sekaligus)? | Menentukan pengalaman pengguna (UX) saat memvalidasi `ReturnOrderId` agar tidak terjadi tumpang tindih data. |
| OQ-004 | Apa dampak pembatalan (void) Retur Jual terhadap status `BTR_ReturnOrder`? | Memastikan konsistensi data siklus hidup dokumen dan kemampuan realisasi ulang order retur yang dibatalkan. |
| OQ-005 | Bagaimana perilaku navigasi dari form monitoring Order Retur (`ListReturnOrderForm`) saat user memilih sebuah baris? | Menentukan keterpaduan alur kerja antara form monitoring dan form input transaksi retur jual. |
| OQ-006 | Di mana pelengkapan data Salesman dan Driver dilakukan jika data dari gudang masih kosong? | Menentukan apakah form monitoring membutuhkan tombol/fitur aksi pelengkapan terpisah atau didelegasikan penuh ke `ReturJualForm`. |

---

# 5. Assumptions

| ID | Assumption |
|------|------|
| ASM-001 | Skema penempatan atribut `JenisRetur` pada header `BTR_ReturJual` dan perilaku `GenStokReturJualWorker` dipertahankan tanpa perubahan. |
| ASM-002 | Seluruh data Order Retur yang disinkronkan dari gudang masuk ke `j05-btr-distrib` dengan status awal `'Synced'`. |
| ASM-003 | Form baru `ListReturnOrderForm.cs` menggantikan peran `GenerateReturnOrderForm.cs` secara penuh pada menu `MainForm`. |
| ASM-004 | Satu Order Retur maksimal menghasilkan 2 dokumen Retur Jual (1 untuk kondisi BAGUS dan 1 untuk kondisi RUSAK). |
| ASM-005 | User kantor pusat yang memproses Retur Jual memiliki wewenang untuk menentukan Warehouse logikal penerima barang retur secara mandiri. |

---

# 6. Risks

| ID | Risk | Impact | Mitigation |
|------|------|------|------|
| RISK-001 | User salah memilih Warehouse logikal (misal: barang rusak dimasukkan ke gudang baik). | Stok barang rusak tercatat pada gudang siap jual, berpotensi teralokasi ke Sales Order berikutnya. | Berikan default value atau visual alert pada `WarehouseIdText` berdasarkan `JenisRetur` yang sedang aktif di form. |
| RISK-002 | Order retur heterogen hanya dibuatkan satu Retur Jual (misal BAGUS saja), lalu item RUSAK terlupakan. | Barang rusak di gudang tidak kunjung terbukukan ke sistem akuntansi/stok pusat. | Form monitoring secara eksplisit menampilkan status `'Partial'` dan memberikan filter khusus untuk order yang belum selesai direalisasikan. |
| RISK-003 | Inkonsistensi status `ReturnOrder` jika koneksi database terputus di tengah proses simpan Retur Jual. | Status Order Retur tidak berubah meski Retur Jual terbentuk. | Eksekusi simpan Retur Jual dan update status `BTR_ReturnOrder` dibungkus dalam satu scope transaksi (`TransHelper.NewScope()`). |

---

# 7. Recommendations

## Option A: Pragmatis & Minimalist (Recommended)

1. Tambahkan kolom `ReturnOrderId VARCHAR(26)` langsung ke tabel `BTR_ReturJual` dengan index non-clustered bertapis (`WHERE ReturnOrderId <> ''`). Tidak membuat tabel mapping tambahan.
2. Tambahkan input field `ReturnOrderId` pada `ReturJualForm.cs` dengan fungsi `LoadReturnOrder(returnOrderId)`.
3. Pada saat `ReturnOrderId` divalidasi:
   - Data Customer, Salesman, dan Driver terisi otomatis ke form (user dapat melengkapi jika kosong).
   - User memilih Warehouse logikal secara eksplisit.
   - Jika order homogen: `JenisReturCombo` otomatis terset dan item dimuat ke grid.
   - Jika order heterogen: jika salah satu jenis retur sudah pernah terbit, otomatis pilih jenis yang tersisa; jika keduanya belum terbit, tampilkan prompt pilihan (BAGUS / RUSAK).
4. Saat `ReturJual` disimpan dalam transaksi:
   - Simpan `ReturJual` beserta `ReturnOrderId`.
   - Evaluasi kelengkapan jenis retur: jika seluruh jenis retur pada order sudah terbit Retur Jual-nya, set `BTR_ReturnOrder.Status = 'Imported'`. Jika baru sebagian, set `BTR_ReturnOrder.Status = 'Partial'`.
5. Saat `ReturJual` di-void:
   - Evaluasi ulang apakah masih ada Retur Jual aktif untuk `ReturnOrderId` terkait. Jika ada -> `Partial`; jika tidak ada -> kembali ke `Synced`.
6. Gantikan `GenerateReturnOrderForm.cs` dengan `ListReturnOrderForm.cs` (mengadopsi pola `ListOrderForm.cs`) yang menyediakan filter tanggal/pencarian, visualisasi status, dan navigasi double-click.

### Advantages

- **Pragmatis**: Mengikuti pola arsitektur yang sudah terbukti sukses pada pasangan `ListOrderForm` dan `FakturForm`.
- **Minimalist**: Tidak menambah tabel baru atau abstraksi berlebih; hanya menambah 1 kolom pada `BTR_ReturJual` dan memanfaatkan kolom `Status` yang sudah ada di `BTR_ReturnOrder`.
- **Explicit**: Aturan pemilihan jenis retur dan status penyelesaian tercatat gamblang di database tanpa perhitungan rumit di aplikasi.

### Disadvantages

- Memerlukan migrasi skema tabel `BTR_ReturJual` (penambahan kolom baru dan update model DAL).

---

## Option B: Separate Mapping Table & Batch Generation Wizard

1. Mempertahankan tabel `BTR_ReturJual` tanpa perubahan kolom, dan membuat tabel baru `BTR_ReturnOrderMap (ReturnOrderId, ReturJualId, JenisRetur, Timestamp)`.
2. Menyempurnakan form `GenerateReturnOrderForm.cs` dengan menambahkan wizard langkah: (1) pilih order, (2) mapping warehouse manual per baris/kelompok, (3) execute generate batch.

### Advantages

- Tidak mengubah skema tabel `BTR_ReturJual`.

### Disadvantages

- Sangat kaku dan rawan gagal validasi saat proses batch.
- Tidak memberikan fleksibilitas bagi user untuk mengoreksi harga, diskon, atau memeriksa item secara individual layaknya pembuatan Retur Jual normal di `ReturJualForm`.
- Menambah kompleksitas struktur data (tabel mapping tambahan yang harus dijaga sinkronisasinya saat void/delete).

---

# 8. Gap Closure

Resolusi berikut telah disepakati melalui proses wawancara interaktif (*Grill-Me session*) yang berfokus pada pendekatan **Pragmatis**, **Explicit**, dan **Minimalist**:

## GAP-001 / OQ-001: Model Relasi & Tracking Status Realisasi

### Decision
Simpan `ReturnOrderId` langsung di tabel `BTR_ReturJual` (kolom baru `ReturnOrderId VARCHAR(26) NOT NULL DEFAULT('')`), dilengkapi non-clustered index `IX_BTR_ReturJual_ReturnOrderId` dengan filter `WHERE ReturnOrderId <> ''`. Perbarui kolom `BTR_ReturnOrder.Status` secara eksplisit (`'Synced'` -> `'Partial'` -> `'Imported'`) saat Retur Jual disimpan.

### Rationale
Paling pragmatis dan konsisten dengan pola `OrderId` pada `BTR_Faktur`. Menghindari kompleksitas pemeliharaan tabel perantara baru, serta secara alamiah mendukung relasi 1-to-many (1 ReturnOrder menghasilkan 2 ReturJual untuk BAGUS dan RUSAK).

### Impact
- Skema database: Tambah kolom `ReturnOrderId` pada `BTR_ReturJual.sql`.
- Model Domain: Tambah properti `ReturnOrderId` pada `ReturJualModel.cs`.
- Persistensi DAL: Update mapper dan builder `ReturJual`.

### Architecture Impact
Perubahan skema tabel `BTR_ReturJual` dan lifecycle state machine pada `BTR_ReturnOrder`.

### Resolved By
User & Analyst (Grill-Me alignment)

### Resolved Date
2026-11-09

---

## GAP-002 / OQ-002: Aturan Pengeditan Item & Status Realisasi

### Decision
Ketika `ReturnOrderId` dimuat ke `ReturJualForm`:
1. Daftar item barang dan kuantitas terisi otomatis dari Order Retur sesuai `JenisRetur` yang dipilih.
2. User melengkapi harga retur, diskon, PPN, dan menentukan Warehouse logikal.
3. User diperbolehkan menghapus baris item jika barang tersebut batal diretur.
4. Status `BTR_ReturnOrder` menjadi `'Imported'` jika seluruh `JenisRetur` yang ada pada order tersebut sudah dibuatkan Retur Jual aktifnya. Jika baru 1 dari 2 jenis retur yang dibuat, status menjadi `'Partial'`.

### Rationale
Menghormati data fisik intake yang dicatat gudang sebagai default yang akurat, namun tetap memberikan fleksibilitas operasional bagi kantor pusat jika ada item yang dianulir. Evaluasi status berbasis `JenisRetur` memberikan kejelasan operasional yang definitif tanpa overhead validasi kuantitas item per item yang kompleks.

### Impact
- Penambahan logika verifikasi jenis retur tersisa pada command/worker penyimpanan Retur Jual.

### Architecture Impact
Mekanisme evaluasi transisi state `ReturnOrder` pada application layer (`SaveReturJualWorker` / `ReturJualWriter`).

### Resolved By
User & Analyst (Grill-Me alignment)

### Resolved Date
2026-11-09

---

## GAP-003 / OQ-003: Alur Pemilahan JenisRetur pada Order Heterogen

### Decision
Pada `ReturJualForm.cs`:
1. Jika Order Retur homogen (hanya memuat BAGUS atau hanya RUSAK): `JenisReturCombo` otomatis terset sesuai status item dan grid langsung memuat seluruh item.
2. Jika Order Retur heterogen (memuat BAGUS dan RUSAK sekaligus):
   - Jika salah satu jenis retur sudah pernah diterbitkan Retur Jual sebelumnya, sistem otomatis memilih jenis retur yang tersisa dan memuat itemnya.
   - Jika kedua jenis retur belum pernah diterbitkan, sistem menampilkan prompt pilihan dialog sederhana kepada user: *"Order Retur ini memuat item BAGUS dan RUSAK. Pilih jenis retur yang ingin diproses terlebih dahulu: [BAGUS] [RUSAK]"*. Setelah dipilih, combo box terkunci pada pilihan tersebut dan hanya item yang sesuai yang dimuat ke grid.

### Rationale
Sangat eksplisit dan meminimalkan kesalahan interaksi pengguna (*frictionless*). Pengguna dipandu secara visual tanpa perlu menebak atau melakukan filter manual berulang kali.

### Impact
- Penambahan dialog prompt pemilihan jenis retur pada `ReturJualForm.cs`.
- Penyesuaian query item `ReturnOrderItem` berdasarkan `ReturnOrderId` dan `JenisRetur`.

### Architecture Impact
Komponen UI `ReturJualForm` dan helper pemilihan kondisi barang.

### Resolved By
User & Analyst (Grill-Me alignment)

### Resolved Date
2026-11-09

---

## GAP-004 / OQ-005: Form Monitoring Order Retur (ListReturnOrderForm)

### Decision
Gantikan `GenerateReturnOrderForm.cs` secara utuh dengan `ListReturnOrderForm.cs` (mengadopsi pola arsitektur `ListOrderForm.cs`). Form ini menyediakan:
- Filter rentang tanggal dan pencarian kata kunci (Customer, No Order, Salesman).
- Grid daftar Order Retur dengan kolom informasi status realisasi (`Synced`, `Partial`, `Imported`) dan nomor Retur Jual yang telah terbit.
- Navigasi interaktif: Double-click pada baris berstatus `'Synced'` atau `'Partial'` langsung membuka `ReturJualForm` dan mengeksekusi `LoadReturnOrder(returnOrderId)`. Jika baris berstatus `'Imported'`, double-click membuka dokumen Retur Jual yang sudah terbit.

### Rationale
Konsisten dengan user experience yang sudah akrab bagi pengguna BTR pada modul Penjualan (`ListOrderForm` -> `FakturForm`), serta membuang form generate otomatis yang tidak terpakai.

### Impact
- Form baru: `ListReturnOrderForm.cs` di namespace `btr.distrib.InventoryContext.ReturnOrderAgg`.
- Menu `MainForm`: Update tombol menu `RT3-Generate Return Order` menjadi `RT3-Monitoring Order Retur` (mengarah ke `ListReturnOrderForm`).
- Penghapusan/deprecasi form lama `GenerateReturnOrderForm.cs`.

### Architecture Impact
Penggantian entry point UI WinForms dan penyelarasan query monitoring `ListReturnOrderQuery`.

### Resolved By
User & Analyst (Grill-Me alignment)

### Resolved Date
2026-11-09

---

## GAP-005 / OQ-006: Lokasi Pelengkapan Data Opsional (Salesman & Driver)

### Decision
Data Salesman dan Driver yang belum terisi dari gudang dilengkapi langsung oleh user di `ReturJualForm.cs` saat proses realisasi berlangsung, memanfaatkan field dan browser lookup `SalesIdText` dan `DriverIdText` yang sudah ada di form tersebut. Form monitoring `ListReturnOrderForm` murni difokuskan untuk monitoring status dan navigasi realisasi.

### Rationale
Minimalist. Menghilangkan kebutuhan tombol "Complete" dan form dialog perantara terpisah yang sebelumnya ada di `GenerateReturnOrderForm.cs`. Mengurangi tahapan klik yang tidak perlu.

### Impact
- Form monitoring menjadi murni *read-only* list.
- Nilai Salesman dan Driver langsung tersimpan pada `BTR_ReturJual` (dan dapat di-update ke `BTR_ReturnOrder` jika sebelumnya kosong).

### Architecture Impact
Penyederhanaan alur kerja operasional.

### Resolved By
User & Analyst (Grill-Me alignment)

### Resolved Date
2026-11-09

---

## GAP-006 / OQ-004: Penanganan Pembatalan (Void) Retur Jual

### Decision
Saat sebuah dokumen `BTR_ReturJual` yang memiliki referensi `ReturnOrderId` dibatalkan (di-void) melalui `DeleteReturJual`:
1. Sistem memeriksa keberadaan dokumen `BTR_ReturJual` aktif lainnya (yang belum void) dengan `ReturnOrderId` yang sama.
2. Jika masih ada Retur Jual aktif lain -> status `BTR_ReturnOrder` diturunkan menjadi `'Partial'`.
3. Jika sudah tidak ada Retur Jual aktif lagi -> status `BTR_ReturnOrder` dikembalikan penuh menjadi `'Synced'`, sehingga Order Retur tersebut dapat direalisasikan ulang dari awal.

### Rationale
Mencegah *orphan lock* atau kondisi buntu di mana sebuah Order Retur tidak bisa diproses kembali padahal dokumen Retur Jual fisiknya telah dibatalkan karena kesalahan input.

### Impact
- Update pada use case / event handler void retur jual untuk mengevaluasi dan meng-update status `BTR_ReturnOrder`.

### Architecture Impact
Mekanisme rollback state pada agregat `ReturnOrder` di dalam transaksi pembatalan Retur Jual.

### Resolved By
User & Analyst (Grill-Me alignment)

### Resolved Date
2026-11-09

---

# 9. Architecture Applicability

## Decision

`ARCHITECTURE-REQUIRED`

## Rationale

Pekerjaan ini memerlukan perancangan arsitektur teknis formal karena:
1. Melibatkan perubahan skema persistensi pada tabel inti distribusi (`BTR_ReturJual` ketambahan kolom `ReturnOrderId`).
2. Menghadirkan transisi *lifecycle state machine* baru pada agregat `ReturnOrder` (`Synced` -> `Partial` -> `Imported` dan *revert* saat void).
3. Melakukan refaktorisasi modul UI WinForms dengan menghapus/mengganti komponen lama (`GenerateReturnOrderForm` dan use case generate batch) menjadi komponen baru (`ListReturnOrderForm` dan penyesuaian `ReturJualForm`).
4. Memerlukan penyusunan *slices* implementasi yang rapi agar tidak mengganggu transaksi retur jual manual yang sedang berjalan di produksi.

---

# 10. Planning Readiness

## Readiness Checklist

- [x] All critical gaps resolved
- [x] All required decisions recorded
- [x] All blocking open questions resolved
- [x] Architecture can be finalized or updated

## Status

`READY-FOR-PLANNING`

*(Catatan: Analisis kelayakan telah tuntas dan seluruh keputusan telah disepakati melalui grill-me interview. Sesuai tata kelola SDLC, status NOT-READY dipertahankan hingga disahkan dan dinaikkan menjadi READY-FOR-PLANNING oleh peran Architect).*

## Notes

Seluruh ketidakpastian fungsional dan teknis telah terjawab dengan berlandaskan pada 3 prinsip utama:
1. **Pragmatis**: Menggunakan pola relasi `OrderJualId` pada `FakturForm` yang sudah terbukti andal.
2. **Explicit**: Status `Synced`, `Partial`, dan `Imported` dikelola secara gamblang dalam transaksi data.
3. **Minimalist**: Menghindari tabel perantara tambahan, memanfaatkan kolom tabel yang ada, dan merampingkan alur kerja tanpa form wizard bertingkat.

Tahap berikutnya adalah penugasan kepada **Architect Agent** (`ica-architect`) untuk menyusun dokumen spesifikasi arsitektur (`ARCHITECTURE.md`) dan memecah pekerjaan ke dalam *implementation slices* (`IMPLEMENTATION-PLAN.md`).

---

# 11. References

Referenced artifacts:

- `docs/issues/IS003-RETURN-ORDER-REALISASI-RETUR-JUAL-ISSUE.md`
- `docs/work/return-order/RETURN-ORDER-DOMAIN.md`
- `docs/work/return-order/RETURN-ORDER-ARCHITECTURE.md`

Referenced codebase locations:

- `src/j05-btr-distrib/btr.sql/Tables/InventoryContext/BTR_ReturnOrder.sql`
- `src/j05-btr-distrib/btr.sql/Tables/InventoryContext/BTR_ReturnOrderItem.sql`
- `src/j05-btr-distrib/btr.sql/Tables/InventoryContext/BTR_ReturJual.sql`
- `src/j05-btr-distrib/btr.distrib/InventoryContext/ReturJualAgg/ReturJualForm.cs`
- `src/j05-btr-distrib/btr.distrib/InventoryContext/ReturnOrderAgg/GenerateReturnOrderForm.cs`
- `src/j05-btr-distrib/btr.distrib/SalesContext/OrderFeature/ListOrderForm.cs`
- `src/j05-btr-distrib/btr.distrib/SalesContext/FakturAgg/FakturForm.cs`
- `src/j05-btr-distrib/btr.distrib/SharedForm/MainForm.cs`
