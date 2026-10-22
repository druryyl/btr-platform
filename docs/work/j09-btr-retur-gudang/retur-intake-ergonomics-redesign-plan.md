# Implementation Plan: Redesign Ergonomi Intake Retur Gudang (High-Load Operations)

- **Implementation Status:** IMPLEMENTED
- **Review Status:** VERIFIED
- **Test Results:** 86/86 Passing (including new unit tests for banner name, zero-prevention, re-scan auto-increment, and button dimensions)

## 1. Overview & Context

Dokumen ini merinci rancangan perbaikan antarmuka dan interaksi operasional untuk modul **Intake Retur Gudang (`ReturnOrderEntryView.cs`)** pada aplikasi desktop BTR Retur Gudang. 

Berdasarkan evaluasi langsung di lapangan dan pengujian operasional gudang dengan beban kerja tinggi *(high-throughput FMCG warehouse)*, diidentifikasi 3 kendala utama:
1. **Nama Barang Terpotong & Kode Barang Internal:** Area nama barang terlalu sempit sehingga terpotong *(ellipsis)*. Kode barang internal (misal `[BRG-002941-KCP]`) memakan ruang display banner padahal tidak memiliki nilai operasional bagi petugas gudang.
2. **Inisial Angka Nol pada Qty Bagus & Rusak:** Kontrol `NumericUpDown` default bernilai `0`. Saat petugas mengetik `"1"`, kursor berada sebelum angka `0` sehingga nilai berubah menjadi `"10"`. Hal ini memperlambat proses intake dan berisiko fatal pada akurasi stok fisik.
3. **Tombol Biru (+ Tambah) Kehilangan Caption:** Tombol aksi biru di sebelah kanan Catatan Item tidak menampilkan teks caption karena kombinasi pembungkusan teks *(word-wrap)* dan pemotongan vertikal *(vertical clipping)* di dalam `TableLayoutPanel` WinForms berukuran terlalu sempit.

---

## 2. Analisis Masalah & Solusi Arsitektural

### 2.1. Masalah 1: Visibilitas Nama Barang & Penghapusan Kode Barang
- **Akar Masalah:**
  - `_lblBannerTitle.Text` saat ini diformat: `$"📦 [{result.BrgId}] {result.BrgName}"`.
  - Kolom kiri `_pnlScannerBanner` hanya memiliki lebar 42% (~440px pada resolusi 1280x720).
  - Status pill badge `_lblBannerStatus` memakan ~110px, menyisakan ~330px untuk teks judul.
  - String kode internal `[BRG-002941-KCP]` memakan ~130px, menyisakan hanya ~200px untuk nama barang. Nama produk FMCG panjang seperti `KECAP BANGO MANIS REFILL 550ML (DUS ISI 24)` terpotong parah.
- **Solusi Redesign:**
  - **Hapus Kode Barang Internal:** `_lblBannerTitle.Text` diubah menjadi `$"📦 {result.BrgName}"`. Kode barang tetap tersimpan di backend/DTO untuk pencatatan transaksi, namun tidak lagi ditampilkan di banner utama intake.
  - **Alokasi Lebar Banner:** Tingkatkan proporsi kolom kiri dari 42% menjadi 48% (atau sesuaikan pembagian strip intake) dan perbesar font menjadi 11pt Bold berkontras tinggi (putih di atas Slate-900) agar dapat terbaca jelas dari jarak berdiri operator gudang (1 - 1.5 meter).

---

### 2.2. Masalah 2: Eliminasi Sticky Zero & Optimalisasi High-Load Warehouse Ergonomics
- **Akar Masalah:**
  - Kontrol bawaan `NumericUpDown` di WinForms selalu menyimpan nilai integer (minimal 0) dan tidak memiliki status kosong *(empty state)*.
  - Saat kontrol menerima fokus, seleksi teks sering hilang atau kursor diletakkan di indeks 0 (sebelum angka 0). Mengetik `1` menghasilkan teks `"10"`.
  - Pada gudang bervolume tinggi, operator memegang barcode scanner di satu tangan dan mengetik di numpad tangan lainnya. Mengharuskan operator menekan `Backspace` atau `Delete` pada setiap item menghancurkan kecepatan input dan memicu kesalahan hitung 10x lipat.
- **Solusi Redesign untuk High-Load:**
  - **Solusi A (Select-All Penuh pada Setiap Event Fokus):**
    - Kaitkan event `Enter`, `GotFocus`, dan `MouseDown` pada `NumericUpDown` inner TextBox:
      ```csharp
      _numQtyBagus.Enter += (s, e) => _numQtyBagus.Select(0, _numQtyBagus.Text.Length);
      _numQtyBagus.Click += (s, e) => _numQtyBagus.Select(0, _numQtyBagus.Text.Length);
      ```
    - Saat operator mengetik angka baru, seleksi penuh memastikan karakter lama langsung digantikan tanpa menyisakan nol.
  - **Solusi B (Custom Numeric TextBox dengan Placeholder Kosong - Rekomendasi Utama):**
    - Gantikan `NumericUpDown` dengan input numerik khusus yang mengizinkan string kosong dengan placeholder abu-abu `"0"`.
    - Nilai default saat discan adalah `1` (karena barang fisik yang discan minimal 1 unit) dalam kondisi terblok penuh *(all-selected)*. Jika operator hanya mengembalikan 1 unit, operator cukup menekan `[Enter]` langsung.
  - **Solusi C (Fitur High-Load Re-Scan Auto-Increment):**
    - Jika operator menscan ulang barcode fisik yang sama, sistem otomatis menaikkan nilai `Qty Bagus` sebesar `+1` tanpa mengharuskan operator menyentuh keyboard *(hands-free multi-scan)*.
  - **Navigasi Keyboard Cepat:**
    - `Enter` pada Qty Bagus, Qty Rusak, atau Catatan langsung melakukan `CommitQuantityStripToGrid()` dengan nada audio sukses (2000Hz beep) dan mengembalikan fokus ke kesiapan scan berikutnya.
    - Tombol `Tab` berpindah mulus: Qty Bagus ➔ Qty Rusak ➔ Catatan Item ➔ Tambah.

---

### 2.3. Masalah 3: Perbaikan Button Biru (+ Tambah)
- **Akar Masalah:**
  - Lebar kolom tombol di `pnlQuantityStrip` diset kaku `115F`.
  - Tinggi baris kontrol diset `29F`, di dalam kartu setinggi `56px` dengan padding atas-bawah total 12px (tinggi efektif kontrol ~25px).
  - Teks tombol adalah `"+ Tambah [Enter]"` (17 karakter bold).
  - Karena teks melebihi 115px, WinForms `Button` memecah teks menjadi 2 baris. Ketinggian 2 baris font 9.5pt adalah ~32px.
  - Di dalam sel 25px-29px, GDI+ memotong teks vertikal secara total *(clipped)* sehingga tombol menjadi kotak biru polos tanpa teks.
  - Selain itu, `UiTheme.FontBodyBold` menginisialisasi `new Font("Segoe UI Bold", ...)` yang bukan merupakan nama family font valid di GDI+ Windows (seharusnya family `"Segoe UI"` dengan `FontStyle.Bold`).
- **Solusi Redesign:**
  - Perbesar lebar kolom tombol dari 115px menjadi **135px - 140px**.
  - Tingkatkan tinggi baris kontrol dari 29px menjadi **34px** (tinggi kartu intake disesuaikan menjadi 62-64px), memberikan ruang vertikal standar bagi tombol desktop.
  - Perbaiki inisialisasi font `UiTheme.FontBodyBold` menjadi `new Font("Segoe UI", 9.5F, FontStyle.Bold)`.
  - Teks tombol diatur menjadi `"+ Tambah [Enter]"` dengan padding internal yang pas sehingga tidak terpotong.

---

## 3. Rincian Perubahan File Codebase

### 3.1. `src/j09-btr-retur-gudang/BtrReturGudang.Winform/Controls/UiTheme.cs`
- Koreksi font definition:
  ```csharp
  // Sebelum:
  public static readonly Font FontBodyBold = new Font("Segoe UI Bold", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
  // Sesudah:
  public static readonly Font FontBodyBold = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
  ```

### 3.2. `src/j09-btr-retur-gudang/BtrReturGudang.Winform/Views/ReturnOrderEntryView.cs`
- **Metode `BuildIntakeCard()`:**
  - Ubah tinggi `pnlCard.Height` dari 56px menjadi 64px.
  - Perlebar proporsi kolom banner kiri dari 42% menjadi 46%, dan kolom strip kuantitas kanan 54%.
  - Kolom `pnlQuantityStrip`:
    - Satuan: 85px
    - Qty Bagus: 95px
    - Qty Rusak: 95px
    - Catatan: Fill (100%)
    - Tombol Tambah: 135px (sebelumnya 115px)
  - Baris `pnlQuantityStrip`:
    - Row 0 (Label): 16px
    - Row 1 (Input/Button): 34px (sebelumnya 29px)
- **Metode `UpdateScannerBannerHit()` & `OnBarcodeScanned()`:**
  - Hapus kode barang:
    ```csharp
    // Sebelum:
    _lblBannerTitle.Text = $"📦 [{result.BrgId}] {result.BrgName}";
    // Sesudah:
    _lblBannerTitle.Text = $"📦 {result.BrgName}";
    ```
  - Auto-select all pada `_numQtyBagus` saat barang discan dan set default awal yang bersih.
  - Tambahkan event handler `Enter` dan `Click` pada `_numQtyBagus` dan `_numQtyRusak` agar selalu melakukan `Select(0, Text.Length)`.
  - Implementasikan deteksi Re-Scan barcode yang sama untuk auto-increment `_numQtyBagus.Value += 1`.

---

## 4. Verification & Testing Criteria

1. **Pengujian Tampilan Nama Barang:**
   - Scan barang dengan nama panjang (contoh: FMCG kemasan karton 40-50 karakter).
   - Pastikan nama barang tampil penuh, jelas, dan tidak ada lagi prefix `[BRG-XXXXX]`.
2. **Pengujian Ergonomi QTY:**
   - Scan barcode ➔ fokus otomatis pada Qty Bagus.
   - Ketik angka `"1"` ➔ nilai harus menjadi `1` (BUKAN `10`).
   - Scan ulang barcode yang sama ➔ nilai Qty Bagus otomatis bertambah menjadi `2`.
   - Ketik angka pada Numpad ➔ nilai langsung menggantikan angka sebelumnya tanpa perlu menekan tombol Backspace/Delete.
3. **Pengujian Tombol Biru:**
   - Periksa tombol di sebelah kanan Catatan Item.
   - Caption `"+ Tambah [Enter]"` harus tampil utuh, berada di tengah vertikal, dan tidak ada teks terpotong pada resolusi 1280x720.
4. **Automated Unit & Integration Tests:**
   - Jalankan `dotnet test` pada `BtrReturGudang.Winform.Tests` untuk memastikan seluruh kontrak pengujian UI dan workflow intake tetap valid.
