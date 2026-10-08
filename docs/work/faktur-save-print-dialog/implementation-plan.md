# Implementation Plan — Pengaturan Setting Print pada SAVE & PRINT Faktur

| Field | Value |
| --- | --- |
| Task Name | `faktur-save-print-dialog` |
| Scope | `btr.distrib` (`FakturSavePreviewForm.cs`, `FakturForm.cs`), `docs/features/faktur/feature.md` |
| Target Framework | .NET Framework 4.8 |
| Status | COMPLETED — Verified with MSBuild |

---

## 1. Context & Objective

Pada alur Faktur Penjualan Desktop (`FakturForm`), saat operator memilih tombol **SAVE & PRINT** pada dialog draft preview, faktur disimpan ke database dan sistem langsung mencetak (*silent direct print*) ke default printer Windows tanpa konfirmasi.

User menginstruksikan:
> *"untuk save & print - setelah klik di munculkan dulu pengaturan setting print nya. Jangan langsung print."*
> *"permintaan tersebut hanya untuk form Faktur Penjualan. Jadi tidak menggunakan RdlcViewerForm.cs melainkan FakturSavePreviewForm.cs"*

**Tujuan:**
1. Menjaga `RdlcViewerForm.cs` tetap bersih dan tidak terikat dengan kebutuhan khusus Faktur Penjualan (karena `RdlcViewerForm` digunakan bersama oleh modul lain).
2. Menempatkan logika cetak dengan pengaturan printer (`PrintWithDialog`) pada `FakturSavePreviewForm.cs`.
3. Memperbarui `FakturForm.cs` agar memanggil `FakturSavePreviewForm.PrintWithDialog(...)` saat aksi **SAVE & PRINT** dieksekusi.
4. Menampilkan jendela pengaturan cetak Windows (`PrintDialog`) sebelum dokumen dikirim ke printer:
   - Operator dapat memilih printer tujuan, jumlah rangkap/copies, dan preferensi cetak lainnya.
   - Jika operator menekan **Print / OK**, dokumen dicetak ke printer terpilih.
   - Jika operator menekan **Cancel**, pencetakan dibatalkan tanpa membatalkan (*rollback*) faktur yang sudah tersimpan di database.

---

## 2. Affected Files

| No | File | Project | Perubahan |
| --- | --- | --- | --- |
| 1 | `src/j05-btr-distrib/btr.distrib/SalesContext/FakturAgg/FakturSavePreviewForm.cs` | `btr.distrib` | Tambahkan metode static `PrintWithDialog(...)` yang merender RDLC dan menampilkan `PrintDialog` sebelum `printDoc.Print()`. |
| 2 | `src/j05-btr-distrib/btr.distrib/SalesContext/FakturAgg/FakturForm.cs` | `btr.distrib` | Pada `PrintFakturRdlc`, ganti pemanggilan `RdlcViewerForm.PrintDirect(...)` menjadi `FakturSavePreviewForm.PrintWithDialog(...)`. |
| 3 | `docs/features/faktur/feature.md` | Dokumen Fitur | Perbarui aturan D-005 mengenai dialog pengaturan cetak pada aksi SAVE & PRINT. |

*Catatan: `RdlcViewerForm.cs` TIDAK diubah sama sekali.*

---

## 3. Step-by-Step Execution Plan

### Step 1: Modifikasi `FakturSavePreviewForm.cs`
- Tambahkan `using System.Drawing.Imaging;` dan `using System.Text;`.
- Tambahkan metode:
  ```csharp
  public static void PrintWithDialog(string reportName, List<ReportDataSource> listDatasource, bool isLandscape = false)
  ```
  yang melakukan render report lokal EMF dan menampilkan `PrintDialog` (`UseEXDialog = true`) sebelum mengeksekusi pencetakan.

### Step 2: Modifikasi `FakturForm.cs`
- Pada metode `PrintFakturRdlc(FakturPrintOutDto faktur)`:
  Ganti `RdlcViewerForm.PrintDirect(printOutTemplate, listDataset);`
  Menjadi `FakturSavePreviewForm.PrintWithDialog(printOutTemplate, listDataset);`.

### Step 3: Update `docs/features/faktur/feature.md`
- Perbarui dokumentasi aturan D-005.

### Step 4: Verifikasi Kompilasi MSBuild
- Jalankan kompilasi MSBuild pada `btr.distrib.csproj`.
- Pastikan build berhasil 100% dengan 0 error dan 0 warning.
