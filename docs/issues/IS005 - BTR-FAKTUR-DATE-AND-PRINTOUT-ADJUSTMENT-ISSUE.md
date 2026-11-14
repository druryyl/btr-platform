# ISSUE

## Metadata

ID: BTR-SALES-FAKTUR-DATE-PRINTOUT-001
Type: CHANGE-REQUEST
Status: Open
Title: Penyesuaian Default Tanggal Faktur & Tanggal Kirim Serta Pemetaan Tanggal pada Printout Faktur Penjualan

## Source

Reported By: User
Reported Date: 2026-11-14

## Description

Pada modul Faktur Penjualan (`j05-btr-distrib`), terdapat kebutuhan penyesuaian aturan default tanggal pada pembuatan faktur baru serta penyesuaian penempatan tanggal pada dokumen cetak (*printout*) faktur:

1. **Default Tanggal Form Faktur**:
   - Saat ini nilai default Tanggal Faktur diinisialisasi ke tanggal hari ini ditambah 1 hari (jika keesokan harinya jatuh pada hari Minggu, maka ditambah 1 hari lagi menjadi +2 hari), dan Tanggal Kirim dihitung dari tanggal faktur + 1 hari.
   - Pengguna meminta agar Tanggal Faktur diubah sehingga secara default menggunakan tanggal hari ini (tanpa penambahan +1 hari).
   - Tanggal Rencana Kirim tetap mengikuti tanggal faktur + 1 hari.

2. **Tampilan Tanggal pada Dokumen Cetak (Printout Faktur)**:
   - Pada lembar cetak faktur penjualan (baik cabang Yogyakarta maupun Magelang), saat ini tanggal di bagian atas menampilkan Tanggal Faktur.
   - Pengguna menginginkan tanggal yang dicetak pada bagian header atas menggunakan **Tanggal Rencana Kirim**.
   - Sedangkan tanggal di bagian bawah dokumen (di samping tanda tangan / nama admin) menggunakan **Tanggal Faktur**.

## Desired Outcome

- Form pembuatan Faktur Penjualan baru secara default menampilkan Tanggal Faktur sesuai tanggal hari ini (H).
- Default Tanggal Rencana Kirim pada form tetap Tanggal Faktur + 1 hari.
- Pada dokumen cetak faktur (`FakturPrintOut-Yk.rdlc` dan `FakturPrintOut-Mgl.rdlc`):
  - Area tanggal di bagian atas (header) menampilkan Tanggal Rencana Kirim.
  - Area tanggal di bagian bawah (di sebelah nama/label Admin) menampilkan Tanggal Faktur.

## Current Situation

- Di `FakturForm.cs` (metode `ClearForm`), default `fakturDate` diatur ke `_dateTime.Now.AddDays(1)` (atau `+2` jika Minggu), dan `TglRencanaKirimTextBox.Value` diatur ke `fakturDate.AddDays(1)`.
- Pada layout laporan RDLC (`FakturPrintOut-Yk.rdlc` dan `FakturPrintOut-Mgl.rdlc`), elemen tanggal di header atas (`TglFakturText`) mengambil data dari field `FakturDate`.
- Pada layout Yogyakarta (`FakturPrintOut-Yk.rdlc`), terdapat `TglFakturText2` di samping label Admin yang mengambil `Mid(Fields!FakturDate.Value, 5)`, sedangkan pada layout Magelang (`FakturPrintOut-Mgl.rdlc`) elemen tanggal di samping label Admin belum tersedia / belum terpetakan.
- Data transfer object untuk cetak faktur (`FakturPrintOutDto`) saat ini hanya menyediakan string `FakturDate` dan belum memuat properti terpisah untuk tanggal rencana kirim.

## Evidence

- Form Faktur Penjualan: [FakturForm.cs](file:///d:/Project.Private/btr-platform/src/j05-btr-distrib/btr.distrib/SalesContext/FakturAgg/FakturForm.cs#L308-L327)
- DTO Printout Faktur: [FakturPrintOutDto.cs](file:///d:/Project.Private/btr-platform/src/j05-btr-distrib/btr.distrib/SalesContext/FakturAgg/FakturPrintOutDto.cs#L17-L28)
- Form Printout Viewer: [FakturPrintOutForm.cs](file:///d:/Project.Private/btr-platform/src/j05-btr-distrib/btr.distrib/SalesContext/FakturAgg/FakturPrintOutForm.cs)
- Template Printout Yogyakarta: [FakturPrintOut-Yk.rdlc](file:///d:/Project.Private/btr-platform/src/j05-btr-distrib/btr.distrib/Reports/FakturPrintOut-Yk.rdlc#L1593-L1614)
- Template Printout Magelang: [FakturPrintOut-Mgl.rdlc](file:///d:/Project.Private/btr-platform/src/j05-btr-distrib/btr.distrib/Reports/FakturPrintOut-Mgl.rdlc#L1614-L1635)

## Notes

- Klasifikasi ISSUE adalah **CHANGE-REQUEST** karena merupakan permintaan penyesuaian fungsionalitas dan tata letak bisnis yang telah ada.
- Detail teknis, analisis dampak, dan rancangan perubahan DTO/RDLC akan dianalisis lebih lanjut pada tahap analisis downstream (*Discovery* & *Feasibility Assessment*).
