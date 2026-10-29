# Implementation Plan — Penanganan Konfigurasi Parameter `ROUTE_CYCLE_ANCHOR_DATE`

| Field | Value |
| --- | --- |
| Task Name | `visit-plan-anchor-date-config` |
| Authority Artifact | `docs/investigations/jadwal-kunjungan-route-cycle-anchor-date-missing.md` |
| Scope | Database Seed (`btr.sql`), Desktop UI Resilience (`btr.distrib`) |
| Target Framework | .NET Framework 4.8 / SQL Server |
| Status | DRAFT — Awaiting User Approval |

---

## 1. Context & Objective

Saat user membuka menu **`SM7 - Jadwal Kunjungan`**, sistem memicu error:
```text
System.InvalidOperationException: System parameter 'ROUTE_CYCLE_ANCHOR_DATE' is not configured.
```

**Tujuan Implementasi:**
1. **Solusi Operasional Database:** Menerapkan seed parameter sistem ke database aktif agar parameter `ROUTE_CYCLE_ANCHOR_DATE` dan `VISIT_PLAN_HORIZON_DAYS` terdaftar di tabel `BTR_ParamSistem`.
2. **Peningkatan Resiliensi Desktop:** Menambahkan penanganan eksepsi yang ramah user pada form `JadwalKunjunganForm` dan `MainForm` sehingga jika parameter sistem belum lengkap atau bermasalah, aplikasi menampilkan pesan panduan yang jelas dan tidak crash.

---

## 2. Affected Components & Files

| No | Component / File | Layer | Perubahan |
| --- | --- | --- | --- |
| 1 | `src/j05-btr-distrib/btr.sql/DataSeeds/BTR_ParamSistem_VisitPlan.sql` | Database | Eksekusi seed ke database operasional (`btr`). |
| 2 | `src/j05-btr-distrib/btr.distrib/SalesContext/VisitPlanAgg/JadwalKunjunganForm.cs` | Desktop UI | Tambahkan penanganan `InvalidOperationException` pada pemanggilan `RefreshPlans()` dengan pesan edukatif. |
| 3 | `src/j05-btr-distrib/btr.distrib/SharedForm/MainForm.cs` | Desktop Shell | Lindungi pemanggilan pembukaan menu SM7 dengan `try...catch` defensif. |

---

## 3. Step-by-Step Execution Plan

### Step 1: Database Seed Execution (Operasional)
Jalankan query seed pada database SQL Server yang dituju:

```sql
IF NOT EXISTS (SELECT 1 FROM BTR_ParamSistem WHERE ParamCode = 'ROUTE_CYCLE_ANCHOR_DATE')
BEGIN
    INSERT INTO BTR_ParamSistem (ParamCode, ParamValue)
    VALUES ('ROUTE_CYCLE_ANCHOR_DATE', '2026-01-05');
END;

IF NOT EXISTS (SELECT 1 FROM BTR_ParamSistem WHERE ParamCode = 'VISIT_PLAN_HORIZON_DAYS')
BEGIN
    INSERT INTO BTR_ParamSistem (ParamCode, ParamValue)
    VALUES ('VISIT_PLAN_HORIZON_DAYS', '90');
END;
```

*Keterangan:* `'2026-01-05'` adalah hari Senin pertama tahun 2026, menjadi patokan awal siklus 2 mingguan (Minggu-1). Nilai ini dapat disesuaikan kemudian via menu **XX2 – Parameter Sistem**.

---

### Step 2: Modifikasi `JadwalKunjunganForm.cs` (Resiliensi UI)
- **Lokasi:** `src/j05-btr-distrib/btr.distrib/SalesContext/VisitPlanAgg/JadwalKunjunganForm.cs`
- Pada method `RefreshPlans()`:
  Bungkus resolusi kalender dan pemuatan data dalam blok `try...catch`:
  ```csharp
  private void RefreshPlans()
  {
      if (string.IsNullOrWhiteSpace(SelectedSalesPersonId))
          return;

      try
      {
          var hariRuteId = _calendar.ResolveHariRuteId(SelectedVisitDate);
          CycleLabel.Text = string.IsNullOrWhiteSpace(hariRuteId)
              ? "Minggu (tidak ada rute)"
              : $"{_calendar.GetCycleWeekLabel(SelectedVisitDate)} / {hariRuteId}";

          _basePlanRows.Clear();
          foreach (var row in _visitPlanDal.ListData(new VisitPlanDateFilter(SelectedSalesPersonId, SelectedVisitDate))
                       ?? Enumerable.Empty<VisitPlanModel>())
          {
              _basePlanRows.Add(row);
          }

          _effectivePlanRows.Clear();
          foreach (var row in _effectiveVisitPlanDal.ListEffectivePlan(SelectedSalesPersonId, SelectedVisitDate))
          {
              _effectivePlanRows.Add(row);
          }

          _exceptionRows.Clear();
          foreach (var row in _exceptionDal.ListData(new VisitPlanDateFilter(SelectedSalesPersonId, SelectedVisitDate))
                       ?? Enumerable.Empty<VisitPlanExceptionModel>())
          {
              _exceptionRows.Add(row);
          }
      }
      catch (InvalidOperationException ex)
      {
          CycleLabel.Text = "Parameter rute belum dikonfigurasi";
          MessageBox.Show(
              ex.Message + "\n\nSilakan hubungi administrator atau konfigurasi parameter sistem melalui menu XX2.",
              "Konfigurasi Parameter Diperlukan",
              MessageBoxButtons.OK,
              MessageBoxIcon.Warning);
      }
  }
  ```

---

### Step 3: Modifikasi `MainForm.cs` (Safety Guard)
- **Lokasi:** `src/j05-btr-distrib/btr.distrib/SharedForm/MainForm.cs`
- Pada event handler `SM7JadwalKunjunganButton_Click`:
  ```csharp
  private void SM7JadwalKunjunganButton_Click(object sender, EventArgs e)
  {
      if (BringMdiChildToFrontIfLoaded<JadwalKunjunganForm>())
          return;

      try
      {
          var form = ThisServicesProvider.GetRequiredService<JadwalKunjunganForm>();
          form.StartPosition = FormStartPosition.CenterScreen;
          form.MdiParent = this;
          form.Show();
      }
      catch (Exception ex)
      {
          MessageBox.Show(
              $"Gagal membuka form Jadwal Kunjungan: {ex.Message}",
              "Kesalahan Sistem",
              MessageBoxButtons.OK,
              MessageBoxIcon.Error);
      }
  }
  ```

---

## 4. Verification & Acceptance Criteria

1. **AC-1 (Menu Dapat Dibuka):** Saat user mengklik menu `SM7 - Jadwal Kunjungan`, form terbuka normal tanpa unhandled exception.
2. **AC-2 (Informasi Siklus Terisi):** Label siklus (`CycleLabel`) menampilkan informasi siklus yang valid (contoh: `Minggu1 / H11` atau `Minggu2 / H23`).
3. **AC-3 (Data Plan Terisi):** Grid Base Plan, Effective Plan, dan Exception menampilkan data jadwal salesmen terkait.
4. **AC-4 (Resiliensi jika Belum Dikonfigurasi):** Jika parameter sistem belum diset di database pengujian, aplikasi menampilkan dialog peringatan informatif dan tidak mengalami force close/crash.
