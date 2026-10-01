# Implementation Plan — Perbaikan Alamat pada Preview Faktur Penjualan

| Field | Value |
| --- | --- |
| Task Name | `faktur-preview-address-fix` |
| Authority Artifact | `docs/investigations/faktur-preview-address-missing.md` |
| Scope | `btr.application` (`FakturBuilder.cs`), `btr.distrib` (`FakturPrintOutDto.cs`) |
| Target Framework | .NET Framework 4.8 |
| Status | COMPLETED — Verified with MSBuild |

---

## 1. Context & Objective

Ketika user membuka preview Faktur Penjualan di `FakturForm` (baik melalui tombol **Save** sebelum data tersimpan, maupun tombol **Preview** untuk faktur void), informasi alamat customer (`ALAMAT`) tidak muncul pada report RDLC.

**Tujuan Implementasi:**
1. Memastikan `FakturBuilder.Customer()` memetakan secara lengkap field customer (`Address`, `Kota`, `CustomerCode`, `Npwp`, `Nitku`) ke dalam `FakturModel` (`_aggRoot`).
2. Memastikan `FakturPrintOutDto` membaca alamat secara defensif dengan fallback ke `customer?.Address1` dan placeholder `"-"` (mengikuti aturan D-004 `docs/features/faktur/feature.md`).

---

## 2. Affected Files

| No | File | Project | Perubahan |
| --- | --- | --- | --- |
| 1 | `src/j05-btr-distrib/btr.application/SalesContext/FakturAgg/Workers/FakturBuilder.cs` | `btr.application` | Lengkapi metode `Customer(ICustomerKey customerKey)` agar mengisi `_aggRoot.Address`, `_aggRoot.Kota`, `_aggRoot.CustomerCode`, `_aggRoot.Npwp`, dan `_aggRoot.Nitku`. |
| 2 | `src/j05-btr-distrib/btr.distrib/SalesContext/FakturAgg/FakturPrintOutDto.cs` | `btr.distrib` | Modifikasi assignment `Address1` dengan pengecekan `faktur?.Address`, fallback ke `customer?.Address1`, dan fallback default `"-"`. |

---

## 3. Step-by-Step Execution Plan

### Step 1: Modifikasi `FakturBuilder.cs`
- Lokasi: `src/j05-btr-distrib/btr.application/SalesContext/FakturAgg/Workers/FakturBuilder.cs`
- Pada baris 180–190 (metode `Customer`):
  Tambahkan pemetaan properti:
  ```csharp
  _aggRoot.CustomerCode = customer.CustomerCode;
  _aggRoot.Address = customer.Address1;
  _aggRoot.Kota = customer.Kota;
  _aggRoot.Npwp = customer.Npwp;
  _aggRoot.Nitku = customer.Nitku;
  ```

### Step 2: Modifikasi `FakturPrintOutDto.cs`
- Lokasi: `src/j05-btr-distrib/btr.distrib/SalesContext/FakturAgg/FakturPrintOutDto.cs`
- Pada baris 23:
  Ganti:
  ```csharp
  Address1 = $"{faktur.Address}";
  ```
  Menjadi:
  ```csharp
  var address1 = !string.IsNullOrWhiteSpace(faktur?.Address)
      ? faktur.Address
      : customer?.Address1;
  Address1 = string.IsNullOrWhiteSpace(address1) ? "-" : address1;
  ```

### Step 3: Verifikasi Build & Kompilasi
- Jalankan kompilasi MSBuild untuk proyek `btr.application` dan `btr.distrib`:
  - `MSBuild btr.application.csproj`
  - `MSBuild btr.distrib.csproj`
- Pastikan build sukses dengan 0 error dan 0 warning.

---

## 4. Verification & Acceptance Criteria

1. **AC-1 (Faktur Baru / Draft Preview):** Saat membuat faktur baru dan memilih customer, klik **Save** (membuka jendela preview). Alamat customer (`Address1`) dan kota/kecamatan (`Address2`) tampil dengan jelas pada template faktur.
2. **AC-2 (Faktur Edit):** Saat mengedit faktur yang ada, alamat customer tampil pada preview sebelum perubahan dikonfirmasi.
3. **AC-3 (Faktur Void Preview):** Preview faktur yang berstatus void menampilkan alamat customer secara akurat.
4. **AC-4 (Robustness D-004):** Jika customer tidak memiliki alamat (`Address1` null/kosong), preview tidak crash dan menampilkan `"-"`.
5. **AC-5 (Side-effect Free):** Tidak ada data yang tersimpan ke database sebelum tombol Confirm Save di jendela preview diklik.
