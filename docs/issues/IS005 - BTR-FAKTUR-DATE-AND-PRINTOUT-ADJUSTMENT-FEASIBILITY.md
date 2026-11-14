---
Title: Feasibility Assessment - Penyesuaian Default Tanggal Faktur & Tanggal Kirim Serta Pemetaan Tanggal pada Printout Faktur Penjualan
Code: BTR-SALES-FAKTUR-DATE-PRINTOUT-001
Artifact: FEASIBILITY-ASSESSMENT
Version: 1.0
LastUpdated: 2026-11-14
Status: READY-FOR-PLANNING
---

# 1. Request Summary

Permintaan perubahan (Change Request) dari dokumen issue `docs/issues/IS005 - BTR-FAKTUR-DATE-AND-PRINTOUT-ADJUSTMENT-ISSUE.md` mengenai penyesuaian aturan default tanggal pada pembuatan faktur baru di `FakturForm` serta penyesuaian pemetaan tanggal pada dokumen cetak (*printout*) faktur penjualan (Yogyakarta & Magelang).

Referenced artifacts:
- DOMAIN: `docs/foundation/DOMAIN.md`
- FEATURE: `docs/features/faktur/feature.md`
- ISSUE: `docs/issues/IS005 - BTR-FAKTUR-DATE-AND-PRINTOUT-ADJUSTMENT-ISSUE.md`

## Objective

Menilai kelayakan teknis, menganalisis dampak terhadap modul dan komponen yang ada, menyelesaikan seluruh pertanyaan terbuka melalui klarifikasi kebutuhan, dan menetapkan kesiapan untuk tahapan implementasi.

---

# 2. Current State

## Existing Behavior

1. **Inisialisasi Tanggal di Form Faktur (`FakturForm.ClearForm`)**:
   - `fakturDate` diinisialisasi ke `_dateTime.Now.AddDays(1)` (H+1).
   - Jika jatuh pada hari Minggu, `fakturDate` dimajukan lagi +1 hari menjadi Senin (+2 hari).
   - `FakturDateText.Value` diisi `fakturDate`.
   - `TglRencanaKirimTextBox.Value` diisi `fakturDate.AddDays(1)`.
2. **DTO Cetak Faktur (`FakturPrintOutDto`)**:
   - Hanya memiliki satu properti tanggal: `FakturDate = $"Tgl: {faktur.FakturDate:dd MMMM yyyy}";`.
   - Tidak memuat representasi string untuk `TglRencanaKirim`.
3. **Template Laporan RDLC (`FakturPrintOut-Yk.rdlc` & `FakturPrintOut-Mgl.rdlc`)**:
   - **Header atas (`TglFakturText`)**: Menampilkan `=First(Fields!FakturDate.Value, "FakturJualDataset")`.
   - **Footer bawah samping label Admin**:
     - Pada Yogyakarta (`FakturPrintOut-Yk.rdlc`), terdapat `TglFakturText2` yang menampilkan `=Mid(First(Fields!FakturDate.Value, "FakturJualDataset"),5)`.
     - Pada Magelang (`FakturPrintOut-Mgl.rdlc`), hanya terdapat `Textbox19` ("Admin,") dan `Textbox20` (UserName), tanpa elemen tanggal di samping label Admin.
4. **Penyimpanan Database & Domain**:
   - `FakturModel` dan `FakturDal` sudah memiliki dan menyimpan field `TglRencanaKirim` bertipe `DateTime`.

## Existing Constraints

- Perubahan harus minimal, pragmatis, dan tidak mengubah alur operasional invoicing maupun integritas data transaksi.
- Layout RDLC harus tetap rapi dan tidak merusak margin/paginasi cetak thermal/dot-matrix faktur yang sudah berjalan.

---

# 3. Gap Analysis

| ID | Severity | Gap |
|---|---|---|
| GAP-001 | MINOR | Default Tanggal Faktur di form masih H+1 (atau +2 jika Minggu), bukan tanggal hari ini (H). |
| GAP-002 | MINOR | Tanggal Rencana Kirim dihitung dari fakturDate eksisting (+1), perlu disesuaikan agar defaultnya adalah H+1 dari tanggal hari ini dengan memperhitungkan hari Minggu (geser ke Senin). |
| GAP-003 | MINOR | `FakturPrintOutDto` belum menyediakan properti untuk tanggal rencana kirim ke RDLC. |
| GAP-004 | MINOR | RDLC header masih terikat ke Tanggal Faktur (`Fields!FakturDate.Value`), bukan Tanggal Rencana Kirim. |
| GAP-005 | MINOR | Layout `FakturPrintOut-Mgl.rdlc` belum memiliki textbox tanggal faktur di samping tanda tangan Admin. |

---

# 4. Open Questions

Semua pertanyaan terbuka telah diklarifikasi dan disepakati melalui sesi grilling:

| ID | Question | Impact |
|---|---|---|
| OQ-001 | Bagaimana aturan default Tanggal Rencana Kirim jika H+1 jatuh pada hari Minggu (misal faktur dibuat hari Sabtu)? | Disepakati: Lewati hari Minggu (default menjadi hari Senin / +2 hari). |
| OQ-002 | Apakah perubahan manual Tanggal Faktur di form perlu otomatis menghitung ulang Tanggal Rencana Kirim? | Disepakati: Tidak. Cukup diinisialisasi saat form baru (`ClearForm`). |
| OQ-003 | Format teks apa yang ditampilkan di header atas printout faktur? | Disepakati: Tetap berawalan `"Tgl: "` (misal `"Tgl: 15 November 2026"`) agar format visual eksisting tidak berubah. |
| OQ-004 | Bagaimana pendekatan pemetaan DTO laporan dan RDLC? | Disepakati: Menambahkan properti baru `RencanaKirimDate` pada `FakturPrintOutDto` dan menggunakannya untuk header RDLC, sedangkan `FakturDate` tetap digunakan untuk tanggal faktur di bawah. |

---

# 5. Assumptions

| ID | Assumption |
|---|---|
| ASM-001 | Field `TglRencanaKirim` pada `FakturModel` selalu terisi valid saat faktur disimpan dan dibaca kembali untuk cetak. |
| ASM-002 | Format tanggal menggunakan nama bulan bahasa Indonesia sesuai culture sistem yang berjalan pada aplikasi Windows Form eksisting. |

---

# 6. Risks

| ID | Risk | Impact | Mitigation |
|---|---|---|---|
| RISK-001 | Penyesuaian layout RDLC Magelang menggeser posisi tanda tangan Admin | Tampilan cetak berantakan | Menyesuaikan lebar `Textbox19` (Admin,) menjadi `0.46747in` dan menaruh `TglFakturText2` berdampingan di `Left: 5.99214in`, persis seperti template Yogyakarta. |
| RISK-002 | Penambahan field pada RDLC menyebabkan error dataset mismatch | ReportViewer gagal me-render faktur | Menambahkan entri `<Field Name="RencanaKirimDate">` pada tag `<Fields>` di kedua file RDLC (`FakturPrintOut-Yk.rdlc` dan `FakturPrintOut-Mgl.rdlc`). |

---

# 7. Recommendations

## Opsi Terpilih (Minimalis & Pragmatis)

1. **`FakturForm.cs` (`ClearForm`)**:
   ```csharp
   var fakturDate = _dateTime.Now.Date;
   var tglKirim = fakturDate.AddDays(1);
   if (tglKirim.DayOfWeek == DayOfWeek.Sunday)
       tglKirim = tglKirim.AddDays(1);

   FakturDateText.Value = fakturDate;
   TglRencanaKirimTextBox.Value = tglKirim;
   ```
2. **`FakturPrintOutDto.cs`**:
   - Tambahkan properti: `public string RencanaKirimDate { get; set; }`
   - Inisialisasi: `RencanaKirimDate = $"Tgl: {faktur.TglRencanaKirim:dd MMMM yyyy}";`
   - Biarkan `FakturDate = $"Tgl: {faktur.FakturDate:dd MMMM yyyy}";` tetap seperti semula.
3. **RDLC Templates (`FakturPrintOut-Yk.rdlc` & `FakturPrintOut-Mgl.rdlc`)**:
   - Tambahkan `<Field Name="RencanaKirimDate"><DataField>RencanaKirimDate</DataField><rd:TypeName>System.String</rd:TypeName></Field>` ke `FakturJualDataset`.
   - Ubah ekspresi `TglFakturText` pada header menjadi:
     `=First(Fields!RencanaKirimDate.Value, "FakturJualDataset")`
   - Pada `FakturPrintOut-Mgl.rdlc`, sesuaikan lebar `Textbox19` dan tambahkan `TglFakturText2` dengan ekspresi:
     `=Mid(First(Fields!FakturDate.Value, "FakturJualDataset"),5)`
     sehingga identik dengan layout Yogyakarta.

---

# 8. Gap Closure

## GAP-001 & GAP-002 (Inisialisasi Form Faktur)
### Decision
Ubah inisialisasi pada `FakturForm.ClearForm`:
- `FakturDateText.Value = _dateTime.Now.Date`
- `TglRencanaKirimTextBox.Value = H+1` (jika Minggu di-skip ke Senin).
### Rationale
Sesuai permintaan operasional pengguna dan telah disepakati pada OQ-001 & OQ-002.
### Impact
Hanya mempengaruhi nilai awal saat klik tombol New / form baru dibuka.

---

## GAP-003, GAP-004 & GAP-005 (Printout Faktur)
### Decision
- Tambahkan properti `RencanaKirimDate` pada `FakturPrintOutDto`.
- Ubah ekspresi header RDLC Yk dan Mgl ke `RencanaKirimDate`.
- Tambahkan field tanggal faktur di samping label Admin pada RDLC Mgl menggunakan `=Mid(First(Fields!FakturDate.Value, "FakturJualDataset"),5)`.
### Rationale
Menyajikan tanggal rencana kirim pada header surat faktur untuk kejelasan jadwal kirim ke pelanggan, sementara tanggal faktur tetap tercatat di samping pengesahan Admin.
### Impact
Perubahan terisolasi pada `FakturPrintOutDto`, `FakturPrintOut-Yk.rdlc`, dan `FakturPrintOut-Mgl.rdlc`.

---

# 9. Architecture Applicability

## Decision
**ARCHITECTURE-NOT-REQUIRED**

## Rationale
Perubahan ini adalah penyesuaian fungsionalitas UI dan dokumen pelaporan pada modul yang sudah ada (`j05-btr-distrib`). Tidak ada komponen arsitektural baru, tidak ada skema database baru, dan tidak ada perubahan boundary integrasi. Seluruh keputusan teknis sudah lengkap dan konklusif pada tahap Feasibility Assessment ini.

---

# 10. Planning Readiness

## Readiness Checklist

- [x] All critical gaps resolved
- [x] All required decisions recorded
- [x] All blocking open questions resolved
- [x] Architecture can be finalized or updated (ARCHITECTURE-NOT-REQUIRED)

## Status
**READY-FOR-PLANNING**

## Notes
Seluruh aspek teknis dan keputusan desain telah divalidasi. Task dapat langsung dilanjutkan ke pembuatan Implementation Plan atau eksekusi implementasi terarah.

---

# 11. References

### Codebase Locations
- Form Faktur: [FakturForm.cs](file:///d:/Project.Private/btr-platform/src/j05-btr-distrib/btr.distrib/SalesContext/FakturAgg/FakturForm.cs#L308-L327)
- DTO Printout: [FakturPrintOutDto.cs](file:///d:/Project.Private/btr-platform/src/j05-btr-distrib/btr.distrib/SalesContext/FakturAgg/FakturPrintOutDto.cs#L17-L28)
- RDLC Yogyakarta: [FakturPrintOut-Yk.rdlc](file:///d:/Project.Private/btr-platform/src/j05-btr-distrib/btr.distrib/Reports/FakturPrintOut-Yk.rdlc#L1593-L1621)
- RDLC Magelang: [FakturPrintOut-Mgl.rdlc](file:///d:/Project.Private/btr-platform/src/j05-btr-distrib/btr.distrib/Reports/FakturPrintOut-Mgl.rdlc#L1614-L1635)
- Model Faktur: [FakturModel.cs](file:///d:/Project.Private/btr-platform/src/j05-btr-distrib/btr.domain/SalesContext/FakturAgg/FakturModel.cs#L46)
- DAL Faktur: [FakturDal.cs](file:///d:/Project.Private/btr-platform/src/j05-btr-distrib/btr.infrastructure/SalesContext/FakturAgg/FakturDal.cs)

### Document References
- Issue: [IS005 - BTR-FAKTUR-DATE-AND-PRINTOUT-ADJUSTMENT-ISSUE.md](file:///d:/Project.Private/btr-platform/docs/issues/IS005%20-%20BTR-FAKTUR-DATE-AND-PRINTOUT-ADJUSTMENT-ISSUE.md)
- Feature: [feature.md](file:///d:/Project.Private/btr-platform/docs/features/faktur/feature.md)
