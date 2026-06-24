# BTR Portal — KPI Catalog

**Audience:** Business Owner, Product Owner, Management, Analyst, Architect, Implementer, QA  
**Purpose:** Single source of truth untuk setiap KPI yang ada di BTR Portal — arti bisnis, cara perhitungan, relevansi manajemen, dan konteks penggunaan.  
**Language:** Bahasa Indonesia; istilah bisnis standar tetap dalam English (KPI, Dashboard, Target, Achievement, Drill-down, dll.).

**Related docs:** [btr-portal-domain.md](./btr-portal-domain.md) · [btr-portal-operational.md](./btr-portal-operational.md) · [portal-navigation-ux-analysis.md](./portal-navigation-ux-analysis.md) · [ALERT-REGISTRY.md](./ALERT-REGISTRY.md)

---

## 1. Tujuan Dokumen

Dokumen ini mendokumentasikan **setiap KPI** yang sudah diimplementasikan atau disetujui pada roadmap portal saat ini (M16–M31). Dokumen **bukan** spesifikasi fitur baru dan **tidak** mengubah business rules.

Pembaca harus dapat menjawab untuk setiap KPI:

- Apa artinya (WHAT)
- Bagaimana dihitung (HOW)
- Mengapa penting (WHY)
- Kapan digunakan dan Drill-down ke menu mana (WHEN)

---

## 2. Cara Membaca Dokumen

1. Cari KPI melalui **Daftar Isi KPI** (§ di bawah), **kode KPI** (mis. `FI-KPI-013`), atau **Indeks KPI per Menu** (§7).
2. Setiap entri memuat **Location** — semua menu yang menampilkan angka tersebut.
3. KPI yang sama di beberapa menu = **satu entri**, beberapa baris Location.
4. Threshold umum (Achievement band, aging bucket, movement class) ada di §5.
5. Aturan attribution dan period semantics ada di **Lampiran A**.
6. Crosswalk ID milestone implementasi (`CRF-KPI`, `COL-OPT-KPI`, dll.) ada di **Lampiran B**.

---

## 3. Konvensi Identitas KPI

| Prefix | Domain bisnis | Menu |
| ------ | ------------- | ---- |
| `EX-KPI-` | Executive / platform | EX01, EX02 |
| `SA-KPI-` | Sales | SA01, SA02, SA03 |
| `CU-KPI-` | Customers | CU01–CU05 |
| `FI-KPI-` | Finance (Piutang & Collection) | FI01–FI04 |
| `SF-KPI-` | Sales Force | SF01, SF02 |
| `IN-KPI-` | Inventory | IN01–IN05 |
| `PU-KPI-` | Purchasing | PU01, PU02 |
| `OP-KPI-` | Operations (Locations) | OP01 |

**Aturan:** Tiga digit sequential per prefix; ID immutable setelah publish; KPI lintas-menu dimiliki prefix domain **primary** (contoh: Total Piutang → `FI-KPI-001`).

### 3.1 Registry ID (ringkas)

| Range | Domain | ~Count |
| ----- | ------ | ------ |
| EX-KPI-001 – 029 | Executive & Alert Center | 29 |
| SA-KPI-001 – 019 | Sales & Forecast | 19 |
| CU-KPI-001 – 071 | Customer chain | 52 |
| FI-KPI-001 – 043 | Piutang, Collection, Cash Forecast, Report | 43 |
| SF-KPI-001 – 018 | Salesmen & Field Activity | 18 |
| IN-KPI-001 – 027 | Inventory lifecycle | 27 |
| PU-KPI-001 – 014 | Purchasing | 14 |
| OP-KPI-001 – 012 | Locations | 12 |

**Total:** ~214 entri terindeks (termasuk sub-bucket aging, payment mix, ranking pattern, dan ref lintas-menu).

---

## 4. Indeks Menu Portal

| Code | Label | Route |
| ---- | ----- | ----- |
| EX01 | Executive | `/dashboard` |
| EX02 | Alert Center | `/alerts` |
| SA01 | Sales | `/dashboard/sales` |
| SA02 | Sales Forecast | `/dashboard/sales-forecast` |
| SA03 | Sales Report | `/reports/sales` |
| CU01 | Customers | `/dashboard/customers` |
| CU02 | Customer Risk Forecast | `/dashboard/customer-risk-forecast` |
| CU03 | Collection Optimization | `/dashboard/collection-optimization` |
| CU04 | Customer Portfolio | `/dashboard/customer-portfolio` |
| CU05 | Customer Report | `/reports/customers` |
| FI01 | Piutang | `/dashboard/piutang` |
| FI02 | Collection | `/dashboard/collection` |
| FI03 | Cash Flow Forecast | `/dashboard/cash-flow-forecast` |
| FI04 | Piutang Report | `/reports/piutang` |
| SF01 | Salesmen | `/dashboard/salesmen` |
| SF02 | Field Activity | `/dashboard/field-activity` |
| IN01 | Inventory | `/dashboard/inventory` |
| IN02 | Inventory Risk | `/dashboard/inventory-risk` |
| IN03 | Inventory Forecast | `/dashboard/inventory-forecast` |
| IN04 | Inventory Optimization | `/dashboard/inventory-optimization` |
| IN05 | Inventory Report | `/reports/inventory` |
| PU01 | Purchasing | `/dashboard/purchasing` |
| PU02 | Purchasing Report | `/reports/purchasing` |
| OP01 | Locations | `/dashboard/locations` |

---

## 5. Referensi Threshold Umum

### 5.1 Achievement Bands (Sales)

| Band | Achievement % | Interpretasi |
| ---- | ------------- | -------------- |
| Healthy | ≥ 100% | On atau di atas Target |
| Warning | 80–99% | Di bawah Target — perlu perhatian |
| Critical | < 80% | Jauh di bawah Target — intervensi |
| Unknown | Target = 0 | Tidak ada Target — bukan penilaian performa |

### 5.2 Aging Bucket Piutang (dari Jatuh Tempo)

| Bucket | Rule |
| ------ | ---- |
| Current | Belum jatuh tempo |
| 1–30 Days | 1–30 hari lewat tempo |
| 31–60 Days | 31–60 hari lewat tempo |
| 61–90 Days | 61–90 hari lewat tempo |
| > 90 Days | Lebih dari 90 hari lewat tempo (chronic overdue) |

**Open balance:** `KurangBayar > 1` (Faktur belum lunas).

### 5.3 Inventory Movement Class (Last Faktur Date per Item)

| Class | Idle sejak penjualan terakhir |
| ----- | ------------------------------ |
| Active | 0–89 hari |
| Slow Moving | 90–179 hari |
| Dead Stock | ≥ 180 hari |
| Never Sold | Tidak pernah terjual via Faktur |

### 5.4 Forecast Confidence (Sales, Cash, Customer Risk)

| Days elapsed in month | Confidence |
| ----------------------- | ---------- |
| ≤ 5 | Low |
| 6–20 | Medium |
| ≥ 21 | High |

### 5.5 Collection Wilayah Hotspot

Wilayah qualifies when wilayah overdue ≥ **15%** of company total overdue (M20).

### 5.6 Qualified Purchasing Backlog

Invoice `BELUM` (belum posting stok) dengan `LastUpdate` ≥ **3 calendar days** ago (configurable).

---

## Daftar Isi KPI

Navigasi cepat ke setiap entri KPI di §6. Klik kode KPI untuk loncat ke definisi lengkap.

### 6.1 Executive (EX-KPI)

- [EX-KPI-001 — Achievement %](#ex-kpi-001)
- [EX-KPI-002 — Total Achievement (Total Omzet MTD)](#ex-kpi-002)
- [EX-KPI-003 — Total Target](#ex-kpi-003)
- [EX-KPI-004 — Total Piutang](#ex-kpi-004)
- [EX-KPI-005 — Overdue Customer Count](#ex-kpi-005)
- [EX-KPI-006 — Piutang > 90 Hari (Amount & %)](#ex-kpi-006)
- [EX-KPI-007 — Top Customer % (Piutang Concentration)](#ex-kpi-007)
- [EX-KPI-008 — Pending Posting (Count & Value)](#ex-kpi-008)
- [EX-KPI-009 — Top Principal % (Purchasing)](#ex-kpi-009)
- [EX-KPI-010 — Total Inventory Value](#ex-kpi-010)
- [EX-KPI-011 — Top Category % (Inventory)](#ex-kpi-011)
- [EX-KPI-012 — Top Supplier % (Inventory)](#ex-kpi-012)
- [EX-KPI-013 — Top 5 Customers (Critical Exposure)](#ex-kpi-013)
- [EX-KPI-014 — Top 5 Categories (Critical Exposure)](#ex-kpi-014)
- [EX-KPI-015 — Top 5 Suppliers (Critical Exposure)](#ex-kpi-015)
- [EX-KPI-016 — Top 5 Principals (Critical Exposure)](#ex-kpi-016)
- [EX-KPI-017 — Snapshot Freshness (Last Refreshed)](#ex-kpi-017)
- [EX-KPI-018 — Domain Attention Summary Cards](#ex-kpi-018)
- [EX-KPI-019 — Portfolio Healthy %](#ex-kpi-019)
- [EX-KPI-020 — Customers At Risk Count](#ex-kpi-020)
- [EX-KPI-021 — Strategic Customers At Risk Count](#ex-kpi-021)
- [EX-KPI-022 — Alert Count — Sales](#ex-kpi-022)
- [EX-KPI-023 — Alert Count — Customer](#ex-kpi-023)
- [EX-KPI-024 — Alert Count — Collection](#ex-kpi-024)
- [EX-KPI-025 — Alert Count — Inventory](#ex-kpi-025)
- [EX-KPI-026 — Alert Count — Purchasing](#ex-kpi-026)
- [EX-KPI-027 — Alert Count — Location](#ex-kpi-027)
- [EX-KPI-028 — Inventory Risk Summary (Alert Center)](#ex-kpi-028)
- [EX-KPI-029 — Platform Alerts (Pinned)](#ex-kpi-029)

### 6.2 Sales (SA-KPI)

- [SA-KPI-004 — Total Faktur](#sa-kpi-004)
- [SA-KPI-005 — Total Customer](#sa-kpi-005)
- [SA-KPI-006 — Weekly Invoiced Sales Trend](#sa-kpi-006)
- [SA-KPI-007 — Top 10 Salesman (Omzet)](#sa-kpi-007)
- [SA-KPI-008 — Target vs Achievement Chart](#sa-kpi-008)
- [SA-KPI-009 — Current Sales](#sa-kpi-009)
- [SA-KPI-010 — Current Achievement %](#sa-kpi-010)
- [SA-KPI-011 — Forecast Sales (Expected)](#sa-kpi-011)
- [SA-KPI-012 — Forecast Achievement %](#sa-kpi-012)
- [SA-KPI-013 — Daily Average Sales](#sa-kpi-013)
- [SA-KPI-014 — Required Daily Sales](#sa-kpi-014)
- [SA-KPI-015 — Target Gap](#sa-kpi-015)
- [SA-KPI-016 — Days Remaining](#sa-kpi-016)
- [SA-KPI-017 — Scenario Projection (Best / Expected / Worst)](#sa-kpi-017)
- [SA-KPI-018 — Forecast Confidence](#sa-kpi-018)
- [SA-KPI-019 — Forecast Risk Indicator](#sa-kpi-019)

### 6.3 Customers (CU-KPI)

- [CU-KPI-001 — Overdue Customer Count](#cu-kpi-001)
- [CU-KPI-002 — >90 Day Exposure](#cu-kpi-002)
- [CU-KPI-003 — Top Omzet Customer %](#cu-kpi-003)
- [CU-KPI-004 — Top Piutang Customer %](#cu-kpi-004)
- [CU-KPI-005 — Active Customer Count](#cu-kpi-005)
- [CU-KPI-006 — Dormant Customer Count](#cu-kpi-006)
- [CU-KPI-007 — Plafond Breach Count](#cu-kpi-007)
- [CU-KPI-008 — Suspended + Sales Count](#cu-kpi-008)
- [CU-KPI-009 — Top 10 Omzet (Ranking)](#cu-kpi-009)
- [CU-KPI-010 — Top 10 Piutang (Ranking)](#cu-kpi-010)
- [CU-KPI-020 — Customers Forecasted at Risk](#cu-kpi-020)
- [CU-KPI-021 — High Risk Customer Count](#cu-kpi-021)
- [CU-KPI-022 — Elevated Risk Receivable](#cu-kpi-022)
- [CU-KPI-023 — Elevated Risk Receivable %](#cu-kpi-023)
- [CU-KPI-024 — Portfolio Health Score](#cu-kpi-024)
- [CU-KPI-025 — Forecast Confidence](#cu-kpi-025)
- [CU-KPI-026 — Total Piutang (Context)](#cu-kpi-026)
- [CU-KPI-027 — Payment Delay Signal Count](#cu-kpi-027)
- [CU-KPI-028 — Credit Limit Signal Count](#cu-kpi-028)
- [CU-KPI-029 — Inactivity Signal Count](#cu-kpi-029)
- [CU-KPI-030 — Purchase Decline Signal Count](#cu-kpi-030)
- [CU-KPI-031 — Collection Risk Signal Count](#cu-kpi-031)
- [CU-KPI-032 — Risk Category Count — Healthy](#cu-kpi-032)
- [CU-KPI-033 — Risk Category Count — Watch](#cu-kpi-033)
- [CU-KPI-034 — Risk Category Count — Attention](#cu-kpi-034)
- [CU-KPI-035 — Risk Category Count — High Risk](#cu-kpi-035)
- [CU-KPI-036 — Risk Category Count — Critical](#cu-kpi-036)
- [CU-KPI-040 — Actions Today](#cu-kpi-040)
- [CU-KPI-041 — Immediate Collection Count](#cu-kpi-041)
- [CU-KPI-042 — Proactive Reminder Count](#cu-kpi-042)
- [CU-KPI-043 — Credit Review Count](#cu-kpi-043)
- [CU-KPI-044 — Sales Recovery Count](#cu-kpi-044)
- [CU-KPI-045 — Management Escalation Count](#cu-kpi-045)
- [CU-KPI-046 — Collection Impact Total](#cu-kpi-046)
- [CU-KPI-047 — Overdue Exposure (Context)](#cu-kpi-047)
- [CU-KPI-048 — Due Within 7 Days](#cu-kpi-048)
- [CU-KPI-049 — Recovery vs Billing % (Context)](#cu-kpi-049)
- [CU-KPI-050 — Planning Confidence](#cu-kpi-050)
- [CU-KPI-051 — Immediate Impact Total](#cu-kpi-051)
- [CU-KPI-060 — Portfolio Health Score](#cu-kpi-060)
- [CU-KPI-061 — Portfolio Healthy %](#cu-kpi-061)
- [CU-KPI-062 — Attention Customer Count](#cu-kpi-062)
- [CU-KPI-063 — Strategic Customer Count](#cu-kpi-063)
- [CU-KPI-064 — Strategic At Risk Count](#cu-kpi-064)
- [CU-KPI-065 — Working Capital Tied Amount](#cu-kpi-065)
- [CU-KPI-066 — Total MTD Omzet (Portfolio)](#cu-kpi-066)
- [CU-KPI-067 — Total Open Balance (Portfolio)](#cu-kpi-067)
- [CU-KPI-068 — Never Purchased Count](#cu-kpi-068)
- [CU-KPI-069 — Dormant Count (Lifecycle)](#cu-kpi-069)
- [CU-KPI-070 — Declining Count](#cu-kpi-070)
- [CU-KPI-071 — Customer Report Row Metrics](#cu-kpi-071)

### 6.4 Finance (FI-KPI)

- [FI-KPI-001 — Total Piutang](#fi-kpi-001)
- [FI-KPI-002 — Total Customer (with balance)](#fi-kpi-002)
- [FI-KPI-003 — Overdue Customer](#fi-kpi-003)
- [FI-KPI-004 — Top 10 Customer %](#fi-kpi-004)
- [FI-KPI-005 — Top 20 Customer %](#fi-kpi-005)
- [FI-KPI-006 — Aging Bucket — Current](#fi-kpi-006)
- [FI-KPI-007 — Aging Bucket — 1–30 Days](#fi-kpi-007)
- [FI-KPI-008 — Aging Bucket — 31–60 Days](#fi-kpi-008)
- [FI-KPI-009 — Aging Bucket — 61–90 Days](#fi-kpi-009)
- [FI-KPI-010 — Aging Bucket — > 90 Days](#fi-kpi-010)
- [FI-KPI-011 — Piutang > 90 Hari (Amount & %)](#fi-kpi-011)
- [FI-KPI-012 — Top 10 Outstanding Customers](#fi-kpi-012)
- [FI-KPI-013 — Overdue Exposure](#fi-kpi-013)
- [FI-KPI-014 — >90d Exposure](#fi-kpi-014)
- [FI-KPI-015 — Overdue Concentration %](#fi-kpi-015)
- [FI-KPI-016 — Cash Collected MTD](#fi-kpi-016)
- [FI-KPI-017 — Recovery vs Billing %](#fi-kpi-017)
- [FI-KPI-018 — Payment Mix — Cash](#fi-kpi-018)
- [FI-KPI-019 — Payment Mix — Giro](#fi-kpi-019)
- [FI-KPI-020 — Payment Mix — Adjustment](#fi-kpi-020)
- [FI-KPI-021 — Legacy Debt Count](#fi-kpi-021)
- [FI-KPI-022 — Aging Risk Summary — 1–30 Days](#fi-kpi-022)
- [FI-KPI-023 — Aging Risk Summary — 31–60 Days](#fi-kpi-023)
- [FI-KPI-024 — Aging Risk Summary — 61–90 Days](#fi-kpi-024)
- [FI-KPI-025 — Aging Risk Summary — > 90 Days](#fi-kpi-025)
- [FI-KPI-026 — Top Overdue Customers](#fi-kpi-026)
- [FI-KPI-027 — Top Overdue Salesmen](#fi-kpi-027)
- [FI-KPI-028 — Top Overdue Wilayah](#fi-kpi-028)
- [FI-KPI-029 — Expected Cash Collection](#fi-kpi-029)
- [FI-KPI-030 — Projected Month-End Collection](#fi-kpi-030)
- [FI-KPI-031 — Collection Forecast %](#fi-kpi-031)
- [FI-KPI-032 — Daily Cash Collection Average](#fi-kpi-032)
- [FI-KPI-033 — Required Daily Collection](#fi-kpi-033)
- [FI-KPI-034 — Remaining Collection Target](#fi-kpi-034)
- [FI-KPI-035 — Days Remaining](#fi-kpi-035)
- [FI-KPI-036 — Recovery vs Billing Forecast](#fi-kpi-036)
- [FI-KPI-037 — Scenario Cash (Best / Expected / Worst)](#fi-kpi-037)
- [FI-KPI-038 — Forecast Confidence (Cash)](#fi-kpi-038)
- [FI-KPI-039 — Outstanding Due Remaining](#fi-kpi-039)
- [FI-KPI-040 — Collection Gap](#fi-kpi-040)
- [FI-KPI-041 — Top Collection Risks (Table)](#fi-kpi-041)
- [FI-KPI-042 — Report Footer — Total Piutang](#fi-kpi-042)
- [FI-KPI-043 — Report Footer — Total Customer](#fi-kpi-043)

### 6.5 Sales Force (SF-KPI)

- [SF-KPI-001 — Below Target Count](#sf-kpi-001)
- [SF-KPI-002 — Missing Target Setup Count](#sf-kpi-002)
- [SF-KPI-003 — High Overdue Exposure Count](#sf-kpi-003)
- [SF-KPI-004 — High Piutang Exposure Count](#sf-kpi-004)
- [SF-KPI-005 — Dormant Portfolio Count](#sf-kpi-005)
- [SF-KPI-006 — Top Omzet Salesman %](#sf-kpi-006)
- [SF-KPI-007 — Top Piutang Salesman %](#sf-kpi-007)
- [SF-KPI-008 — Top 10 Omzet (Ranking)](#sf-kpi-008)
- [SF-KPI-009 — Top 10 Achievement % (Ranking)](#sf-kpi-009)
- [SF-KPI-010 — Top 10 Piutang (Ranking)](#sf-kpi-010)
- [SF-KPI-011 — Principal Achievement Table](#sf-kpi-011)
- [SF-KPI-012 — Planned Visits](#sf-kpi-012)
- [SF-KPI-013 — Actual Visits](#sf-kpi-013)
- [SF-KPI-014 — Missed Visits](#sf-kpi-014)
- [SF-KPI-015 — Unplanned Visits](#sf-kpi-015)
- [SF-KPI-016 — Effective Calls](#sf-kpi-016)
- [SF-KPI-017 — Visit Execution %](#sf-kpi-017)
- [SF-KPI-018 — Effective Call Rate](#sf-kpi-018)

### 6.6 Inventory (IN-KPI)

- [IN-KPI-001 — Total Inventory Value](#in-kpi-001)
- [IN-KPI-002 — Total Item](#in-kpi-002)
- [IN-KPI-003 — Top 10 Category (Ranking)](#in-kpi-003)
- [IN-KPI-004 — Top 10 Supplier (Ranking)](#in-kpi-004)
- [IN-KPI-005 — Dead Stock Count & Value](#in-kpi-005)
- [IN-KPI-006 — Slow Moving Count & Value](#in-kpi-006)
- [IN-KPI-007 — Never Sold Count & Value](#in-kpi-007)
- [IN-KPI-008 — At-Risk Inventory %](#in-kpi-008)
- [IN-KPI-009 — Aging Distribution (Movement Classes)](#in-kpi-009)
- [IN-KPI-010 — Category Risk Exposure](#in-kpi-010)
- [IN-KPI-011 — Supplier Risk Exposure](#in-kpi-011)
- [IN-KPI-012 — Top 10 Dead / Slow Moving (Ranking)](#in-kpi-012)
- [IN-KPI-013 — Projected Inventory Value @ Horizon](#in-kpi-013)
- [IN-KPI-014 — Average Days of Supply (Company)](#in-kpi-014)
- [IN-KPI-015 — Inventory Health Score](#in-kpi-015)
- [IN-KPI-016 — Stock-Out Risk Items / Value](#in-kpi-016)
- [IN-KPI-017 — Overstock / Understock Value](#in-kpi-017)
- [IN-KPI-018 — Scenario Projected Value (Best/Expected/Worst)](#in-kpi-018)
- [IN-KPI-019 — Forecast Confidence (Inventory)](#in-kpi-019)
- [IN-KPI-020 — Days of Supply (Item)](#in-kpi-020)
- [IN-KPI-021 — Recommended Purchase Qty (Indicative)](#in-kpi-021)
- [IN-KPI-022 — Critical Actions Count](#in-kpi-022)
- [IN-KPI-023 — Recommended Purchase Budget](#in-kpi-023)
- [IN-KPI-024 — Recoverable Capital](#in-kpi-024)
- [IN-KPI-025 — Action Counts by Type (Purchase / Delay / Transfer / Clearance)](#in-kpi-025)
- [IN-KPI-026 — Report Footer — Total Inventory Value](#in-kpi-026)
- [IN-KPI-027 — Report Footer — Total Item](#in-kpi-027)

### 6.7 Purchasing (PU-KPI)

- [PU-KPI-001 — Grand Total Purchase](#pu-kpi-001)
- [PU-KPI-002 — Total Invoice](#pu-kpi-002)
- [PU-KPI-003 — Posted %](#pu-kpi-003)
- [PU-KPI-004 — Pending Posting Value](#pu-kpi-004)
- [PU-KPI-005 — Qualified Backlog Count & Value](#pu-kpi-005)
- [PU-KPI-006 — Top 1 Principal %](#pu-kpi-006)
- [PU-KPI-007 — Top 3 Principal %](#pu-kpi-007)
- [PU-KPI-008 — Compound Dependency Count](#pu-kpi-008)
- [PU-KPI-009 — Purchasing Inactivity Flag](#pu-kpi-009)
- [PU-KPI-010 — Principal At-Risk Count](#pu-kpi-010)
- [PU-KPI-011 — Top 10 Principal (Ranking)](#pu-kpi-011)
- [PU-KPI-012 — Principal Exposure Comparison](#pu-kpi-012)
- [PU-KPI-013 — Report Footer — Grand Total Purchase](#pu-kpi-013)
- [PU-KPI-014 — Report Footer — Total Invoice](#pu-kpi-014)

### 6.8 Operations (OP-KPI)

- [OP-KPI-001 — Top 1 Warehouse Inventory %](#op-kpi-001)
- [OP-KPI-002 — Top 3 Warehouse Inventory %](#op-kpi-002)
- [OP-KPI-003 — Top 1 Warehouse At-Risk %](#op-kpi-003)
- [OP-KPI-004 — Top 1 Warehouse Sales %](#op-kpi-004)
- [OP-KPI-005 — Top 1 Wilayah Sales %](#op-kpi-005)
- [OP-KPI-006 — Inactive Warehouse With Stock Count](#op-kpi-006)
- [OP-KPI-007 — Top Warehouse by Inventory (Ranking)](#op-kpi-007)
- [OP-KPI-008 — Top Warehouse by At-Risk (Ranking)](#op-kpi-008)
- [OP-KPI-009 — Top Warehouse by Sales (Ranking)](#op-kpi-009)
- [OP-KPI-010 — Top Warehouse by Purchasing (Ranking)](#op-kpi-010)
- [OP-KPI-011 — Top Wilayah by Sales (Ranking)](#op-kpi-011)
- [OP-KPI-012 — Location Attention Signal Counts](#op-kpi-012)

---

## 6. Katalog KPI per Domain

<!-- KPI entries follow — sections 6.1 through 6.8 -->
### 6.1 Executive (EX-KPI)

<a id="ex-kpi-001"></a>
## EX-KPI-001 — Achievement %

**Location**

- EX01 - Management Attention Center
- SA01 - Sales Dashboard
- SA02 - Sales Forecast Dashboard

---

### WHAT

KPI persentase **pencapaian Sales** vs Target bulan berjalan (company level).
- Mengukur performa billing aktual terhadap rencana.
- Audience: Owner, GM, Sales management.
- Sumber: snapshot Sales (Faktur non-void).

### HOW

* Rumus: Total Achievement ÷ Total Target × 100%.
- Achievement = SUM GrandTotal Faktur MTD.
- Target = SUM Target Salesman MTD.
- Blank jika Target = 0 (Unknown).
- Band: Healthy ≥100%, Warning 80–99%, Critical <80%.

### WHY

* Signal performa utama executive scan.
- Trigger intervensi Sales.
- Deteksi risiko miss Target.

### WHEN

* Daily — buka EX01 atau SA01.
- Warning/Critical → SA01 → SA03 (evidence).
- Forecast month-end → SA02.

<a id="ex-kpi-002"></a>
## EX-KPI-002 — Total Achievement (Total Omzet MTD)

**Location**

- EX01 - Executive (Sales card)
- SA01 - Sales Dashboard
- SA02 - Sales Forecast (Current Sales)

---

### WHAT

Total **invoiced omzet** bulan kalender berjalan dari Faktur resmi.
- Revenue recognition operasional.
- Audience: Management, Finance, Sales.
- Sumber: Faktur non-void.

### HOW

* SUM(GrandTotal) Faktur tanggal dalam bulan berjalan.
- Attribution Salesman = Salesman pada Faktur.
- Void excluded.
- Reconcile dengan SA03 (same period).

### WHY

* Baseline revenue harian/bulanan.
- Input Recovery vs Billing dan forecast.

### WHEN

* Daily EX01/SA01.
- Investigasi SA03 period bulan ini.

<a id="ex-kpi-003"></a>
## EX-KPI-003 — Total Target

**Location**

- EX01 - Executive
- SA01 - Sales Dashboard
- SA02 - Sales Forecast

---

### WHAT

Agregasi **Target Sales** bulanan semua Salesman dari BTR Desktop.
- Plan denominator untuk Achievement.
- Audience: Sales management.
- Sumber: master Target Salesman.

### HOW

* SUM Target Salesman bulan kalender berjalan.
- Rep tanpa Target tidak menambah total.
- Identical across SA01/SA02 same refresh.

### WHY

* Referensi plan vs actual.
- Tanpa Target, Achievement % tidak ditampilkan.

### WHEN

* Awal bulan review setup Target.
- Missing per rep → SF01 Missing Target Setup.

<a id="ex-kpi-004"></a>
## EX-KPI-004 — Total Piutang

**Location**

- EX01 - Executive (Piutang card)
- FI01 - Piutang Dashboard
- FI04 - Piutang Report (footer, all-open mode)
- CU02 - Customer Risk Forecast (context)

---

### WHAT

Total saldo **piutang terbuka** seluruh customer (all-time snapshot).
- Working capital at risk.
- Audience: Finance, Collection, Owner.
- Sumber: open Faktur balances.

### HOW

* SUM(KurangBayar) where KurangBayar > 1.
- All-time open — bukan period filter.
- FI04 default period berbeda kecuali investigation all-open mode.

### WHY

* Skala exposure piutang.
- Denominator concentration & risk KPIs.

### WHEN

* Daily EX01/FI01.
- Evidence FI04 all-open balances.

<a id="ex-kpi-005"></a>
## EX-KPI-005 — Overdue Customer Count

**Location**

- EX01 - Executive
- FI01 - Piutang Dashboard
- FI02 - Collection Dashboard
- CU01 - Customers (Collection card)

---

### WHAT

Jumlah customer distinct dengan saldo **past due** (aging ≠ Current).
- Breadth collection workload.
- Audience: Finance, Collection.
- Sumber: piutang snapshot.

### HOW

* COUNT DISTINCT customer dengan ≥1 open balance past Jatuh Tempo.
- Current bucket excluded.
- Consistent FI01/FI02/CU01 cards.

### WHY

* Berapa account perlu follow-up.
- Executive attention trigger.

### WHEN

* Daily FI02 attention list.
- Per customer CU01 atau FI04.

<a id="ex-kpi-006"></a>
## EX-KPI-006 — Piutang > 90 Hari (Amount & %)

**Location**

- EX01 - Executive
- FI01 - Piutang Dashboard
- FI02 - Collection (>90d Exposure)

---

### WHAT

Nilai piutang bucket **>90 Days** dan % terhadap Total Piutang.
- Chronic overdue / bad-debt signal.
- Audience: Finance, Owner.
- Sumber: aging Jatuh Tempo.

### HOW

* Amount = SUM bucket >90 Days.
- % = Amount ÷ Total Piutang × 100%.
- FI02 label: >90d Exposure (overdue subset).

### WHY

* Escalation & write-off risk.
- Chronic Overdue signal FI02/EX02.

### WHEN

* Monitor weekly/daily jika naik.
- FI01 aging → FI04 sort Jatuh Tempo.

<a id="ex-kpi-007"></a>
## EX-KPI-007 — Top Customer % (Piutang Concentration)

**Location**

- EX01 - Executive (Top 5 Customers)
- FI01 - Piutang Dashboard

---

### WHAT

Share Total Piutang pada **customer #1** by outstanding balance.
- Default / concentration risk.
- Audience: Finance, Owner.
- Sumber: customer ranking open balance.

### HOW

* Top 1 balance ÷ Total Piutang × 100%.
- Related: FI-KPI-004 Top 10 %, FI-KPI-005 Top 20 %.
- Informational — no auto threshold.

### WHY

* Dependency on single debtor.
- Credit risk diversification.

### WHEN

* EX01 Top 5 → FI01 → FI04 customer filter.

<a id="ex-kpi-008"></a>
## EX-KPI-008 — Pending Posting (Count & Value)

**Location**

- EX01 - Executive (Purchasing card)
- PU01 - Purchasing Management

---

### WHAT

Count & nilai invoice pembelian **BELUM posting stok**.
- Executive RequiresAttention uses Qualified Backlog only (PU-KPI-005).
- Audience: Purchasing, Warehouse.
- Sumber: purchase invoice MTD.

### HOW

* All BELUM count/value shown as context.
- Qualified = BELUM aged ≥3 calendar days.
- Void excluded.

### WHY

* Inventory intake delay.
- Warehouse posting bottleneck.

### WHEN

* Qualified >0: PU01 → PU02 BELUM → Desktop PT2.

<a id="ex-kpi-009"></a>
## EX-KPI-009 — Top Principal % (Purchasing)

**Location**

- EX01 - Executive (Top 5 Principals)
- PU01 - Purchasing Management

---

### WHAT

Share spend pembelian MTD pada Principal #1.
- Supplier dependency.
- Audience: Purchasing, Owner.
- Sumber: purchase invoice MTD.

### HOW

* Top 1 principal spend ÷ Grand Total Purchase × 100%.
- See PU-KPI-007 Top 3 %.
- Blank supplier = Unknown.

### WHY

* Supply chain concentration.
- Cross-check inventory PU01.

### WHEN

* Monthly PU01 → PU02.

<a id="ex-kpi-010"></a>
## EX-KPI-010 — Total Inventory Value

**Location**

- EX01 - Executive
- IN01 - Inventory Dashboard
- IN02 - Inventory Risk
- IN05 - Inventory Report (footer)

---

### WHAT

Total nilai modal inventory (HPP × Qty), point-in-time.
- Working capital in stock.
- Audience: Inventory, Purchasing, Owner.
- Sumber: stock balance snapshot.

### HOW

* BrgId-first aggregation.
- Exclude In-Transit warehouse.
- Exclude Qty ≤ 0.
- Must match IN05 footer (no search).

### WHY

* Capital tied in inventory.
- Baseline At-Risk % and forecast.

### WHEN

* Weekly IN01; risk IN02; detail IN05.

<a id="ex-kpi-011"></a>
## EX-KPI-011 — Top Category % (Inventory)

**Location**

- EX01 - Executive
- IN01 - Inventory Dashboard

---

### WHAT

Share inventory value pada kategori #1.
- Category concentration.
- Audience: Inventory management.
- Sumber: M15 breakdown.

### HOW

* Rank-1 category value ÷ Total Inventory Value × 100%.
- Blank → Unknown label.

### WHY

* Over-concentration by category.
- Category review planning.

### WHEN

* IN01 Top 10 Categories → IN05.

<a id="ex-kpi-012"></a>
## EX-KPI-012 — Top Supplier % (Inventory)

**Location**

- EX01 - Executive
- IN01 - Inventory Dashboard

---

### WHAT

Share inventory value pada supplier/principal #1.
- Supplier concentration.
- Audience: Inventory, Purchasing.
- Sumber: M15 supplier breakdown.

### HOW

* Rank-1 supplier value ÷ Total Inventory Value × 100%.
- Principal terminology = Supplier in BTR.

### WHY

* Capital dependency on one principal.
- Link PU01 compound dependency.

### WHEN

* IN01 → PU01 Principal Exposure.

<a id="ex-kpi-013"></a>
## EX-KPI-013 — Top 5 Customers (Critical Exposure)

**Location**

- EX01 - Executive

---

### WHAT

Ranking Top 5 customer by outstanding balance untuk executive priority.
- Subset Top 10 domain dashboards.
- Audience: Owner, GM.
- Informational — bukan Alert row.

### HOW

* First 5 rows domain ranking.
- Include concentration % per row.
- Same sort as producer dashboard Top 10.

### WHY

* Fast prioritization tanpa buka domain dashboard.
- Morning briefing.

### WHEN

* Daily EX01 → domain dashboard → Report.

<a id="ex-kpi-014"></a>
## EX-KPI-014 — Top 5 Categories (Critical Exposure)

**Location**

- EX01 - Executive

---

### WHAT

Ranking Top 5 category by inventory value untuk executive priority.
- Subset Top 10 domain dashboards.
- Audience: Owner, GM.
- Informational — bukan Alert row.

### HOW

* First 5 rows domain ranking.
- Include concentration % per row.
- Same sort as producer dashboard Top 10.

### WHY

* Fast prioritization tanpa buka domain dashboard.
- Morning briefing.

### WHEN

* Daily EX01 → domain dashboard → Report.

<a id="ex-kpi-015"></a>
## EX-KPI-015 — Top 5 Suppliers (Critical Exposure)

**Location**

- EX01 - Executive

---

### WHAT

Ranking Top 5 supplier by inventory value untuk executive priority.
- Subset Top 10 domain dashboards.
- Audience: Owner, GM.
- Informational — bukan Alert row.

### HOW

* First 5 rows domain ranking.
- Include concentration % per row.
- Same sort as producer dashboard Top 10.

### WHY

* Fast prioritization tanpa buka domain dashboard.
- Morning briefing.

### WHEN

* Daily EX01 → domain dashboard → Report.

<a id="ex-kpi-016"></a>
## EX-KPI-016 — Top 5 Principals (Critical Exposure)

**Location**

- EX01 - Executive

---

### WHAT

Ranking Top 5 principal by MTD purchase untuk executive priority.
- Subset Top 10 domain dashboards.
- Audience: Owner, GM.
- Informational — bukan Alert row.

### HOW

* First 5 rows domain ranking.
- Include concentration % per row.
- Same sort as producer dashboard Top 10.

### WHY

* Fast prioritization tanpa buka domain dashboard.
- Morning briefing.

### WHEN

* Daily EX01 → domain dashboard → Report.

<a id="ex-kpi-017"></a>
## EX-KPI-017 — Snapshot Freshness (Last Refreshed)

**Location**

- EX01 - Executive
- Semua Dashboard detail (Generated-at)

---

### WHAT

Recency data analytics — timestamp snapshot terakhir.
- Trust indicator.
- Audience: All management users.
- Sumber: worker refresh metadata.

### HOW

* Last Refreshed = oldest domain GeneratedAt on screen.
- Banner jika exceed interval.
- Intervals: Piutang 15m; Sales/Collection/Customer 30m; Inventory 60m.
- SF02 live query — excluded.

### WHY

* Decisions harus account staleness.
- SnapshotStale alert EX02.

### WHEN

* Every session.
- Stale → admin worker rebuild.

<a id="ex-kpi-018"></a>
## EX-KPI-018 — Domain Attention Summary Cards

**Location**

- EX01 - Executive

---

### WHAT

Ringkasan exception/band per domain di landing page.
- Aggregated RequiresAttention.
- Audience: Owner, GM.
- Sumber: cross-domain composers.

### HOW

* Sales: Achievement band Warning/Critical.
- Piutang: overdue counts, >90d.
- Purchasing: Qualified Backlog >0 only.
- Links to domain dashboards — no direct Report.

### WHY

* Management Attention Center — What is happening?
- Prioritize domain to open.

### WHEN

* Daily morning scan EX01 → domain → Report.

<a id="ex-kpi-019"></a>
## EX-KPI-019 — Portfolio Healthy %

**Location**

- EX01 - Executive (M31 promotion)
- CU04 - Customer Portfolio

---

### WHAT

% customer kategori forecast Healthy (M29) dalam universe M31.
- Forward portfolio quality.
- Audience: Owner, leadership.
- Sumber: M31 consumes M29.

### HOW

* Healthy Count ÷ Total Customer Count × 100%.
- Healthy = M29 category Healthy.
- M31 does not recompute M29 rules.

### WHY

* Executive portfolio health headline.
- Complements CU01 backward-looking signals.

### WHEN

* Weekly CU04; preventive CU02.

<a id="ex-kpi-020"></a>
## EX-KPI-020 — Customers At Risk Count

**Location**

- EX01 - Executive (M31 promotion)
- CU02 - Customer Risk Forecast
- CU04 - Customer Portfolio

---

### WHAT

Count customer kategori ≥ Watch (M29).
- Forward risk breadth.
- Audience: Finance, Sales management.
- Sumber: M29 rule engine.

### HOW

* COUNT category ∈ {Watch, Attention, High Risk, Critical}.
- Deterministic CRF-* rules.
- Same CU-KPI-020 / CRF-KPI-01.

### WHY

* Early warning before overdue/dormant.
- Preventive resource allocation.

### WHEN

* Mid-month CU02; actions CU03; portfolio CU04.

<a id="ex-kpi-021"></a>
## EX-KPI-021 — Strategic Customers At Risk Count

**Location**

- EX01 - Executive (M31 promotion)
- CU04 - Customer Portfolio

---

### WHAT

Count tier Strategic dengan M29 category ≥ Watch.
- High-value forward risk.
- Audience: Owner, KAM.
- Sumber: M31 tier + M29.

### HOW

* COUNT Tier=Strategic AND category ≥ Watch.
- Strategic: Top 10 omzet OR Top 10 piutang OR (M29≥Attention AND balance≥Rp10M).

### WHY

* Protect strategic accounts.
- Management escalation priority.

### WHEN

* CU04 filter Tier=Strategic; detail CU05.

<a id="ex-kpi-022"></a>
## EX-KPI-022 — Alert Count — Sales

**Location**

- EX02 - Alert Center

---

### WHAT

Jumlah alert rows kategori Sales (max Top 20 displayed).
- Company-wide exception volume.
- Audience: Management.
- Sumber: M17–M22 aggregated signals.

### HOW

* COUNT after deduplication & cap Top 20.
- Inventory: M19 summary only.
- Dedup: M20 wins customer overdue; LegacyDebt suppresses Dormant; M20 wins salesman overdue.

### WHY

* Prioritize domain for exception review.
- Complements EX01 cards.

### WHEN

* Daily EX02 → domain dashboard → Report.

<a id="ex-kpi-023"></a>
## EX-KPI-023 — Alert Count — Customer

**Location**

- EX02 - Alert Center

---

### WHAT

Jumlah alert rows kategori Customer (max Top 20 displayed).
- Company-wide exception volume.
- Audience: Management.
- Sumber: M17–M22 aggregated signals.

### HOW

* COUNT after deduplication & cap Top 20.
- Inventory: M19 summary only.
- Dedup: M20 wins customer overdue; LegacyDebt suppresses Dormant; M20 wins salesman overdue.

### WHY

* Prioritize domain for exception review.
- Complements EX01 cards.

### WHEN

* Daily EX02 → domain dashboard → Report.

<a id="ex-kpi-024"></a>
## EX-KPI-024 — Alert Count — Collection

**Location**

- EX02 - Alert Center

---

### WHAT

Jumlah alert rows kategori Collection (max Top 20 displayed).
- Company-wide exception volume.
- Audience: Management.
- Sumber: M17–M22 aggregated signals.

### HOW

* COUNT after deduplication & cap Top 20.
- Inventory: M19 summary only.
- Dedup: M20 wins customer overdue; LegacyDebt suppresses Dormant; M20 wins salesman overdue.

### WHY

* Prioritize domain for exception review.
- Complements EX01 cards.

### WHEN

* Daily EX02 → domain dashboard → Report.

<a id="ex-kpi-025"></a>
## EX-KPI-025 — Alert Count — Inventory

**Location**

- EX02 - Alert Center

---

### WHAT

Jumlah alert rows kategori Inventory (max Top 20 displayed).
- Company-wide exception volume.
- Audience: Management.
- Sumber: M17–M22 aggregated signals.

### HOW

* COUNT after deduplication & cap Top 20.
- Inventory: M19 summary only.
- Dedup: M20 wins customer overdue; LegacyDebt suppresses Dormant; M20 wins salesman overdue.

### WHY

* Prioritize domain for exception review.
- Complements EX01 cards.

### WHEN

* Daily EX02 → domain dashboard → Report.

<a id="ex-kpi-026"></a>
## EX-KPI-026 — Alert Count — Purchasing

**Location**

- EX02 - Alert Center

---

### WHAT

Jumlah alert rows kategori Purchasing (max Top 20 displayed).
- Company-wide exception volume.
- Audience: Management.
- Sumber: M17–M22 aggregated signals.

### HOW

* COUNT after deduplication & cap Top 20.
- Inventory: M19 summary only.
- Dedup: M20 wins customer overdue; LegacyDebt suppresses Dormant; M20 wins salesman overdue.

### WHY

* Prioritize domain for exception review.
- Complements EX01 cards.

### WHEN

* Daily EX02 → domain dashboard → Report.

<a id="ex-kpi-027"></a>
## EX-KPI-027 — Alert Count — Location

**Location**

- EX02 - Alert Center

---

### WHAT

Jumlah alert rows kategori Location (max Top 20 displayed).
- Company-wide exception volume.
- Audience: Management.
- Sumber: M17–M22 aggregated signals.

### HOW

* COUNT after deduplication & cap Top 20.
- Inventory: M19 summary only.
- Dedup: M20 wins customer overdue; LegacyDebt suppresses Dormant; M20 wins salesman overdue.

### WHY

* Prioritize domain for exception review.
- Complements EX01 cards.

### WHEN

* Daily EX02 → domain dashboard → Report.

<a id="ex-kpi-028"></a>
## EX-KPI-028 — Inventory Risk Summary (Alert Center)

**Location**

- EX02 - Alert Center

---

### WHAT

Ringkasan M19: Dead/Slow/Never counts & values, At-Risk %.
- Summary-only — no SKU rows in EX02.
- Audience: Management.
- Sumber: IN02 snapshot.

### HOW

* Reuse IN-KPI-003–006 aggregates.
- Link IN02 for detail.
- At-Risk % = at-risk value ÷ total inventory.

### WHY

* Cross-domain obsolescence visibility.
- Executive scan without SKU dump.

### WHEN

* EX02 → IN02 → IN05 item evidence.

<a id="ex-kpi-029"></a>
## EX-KPI-029 — Platform Alerts (Pinned)

**Location**

- EX02 - Alert Center

---

### WHAT

System alerts: SnapshotStale, SnapshotDegraded, DomainUnavailable.
- Pinned top always.
- Audience: All users; admin resolves.
- Sumber: health/refresh log.

### HOW

* Stale: exceed refresh interval.
- Degraded: worker failed.
- Unavailable: empty snapshot.
- Invalidates trust in other KPIs.

### WHY

* Prevent decisions on bad data.
- Trigger worker rebuild.

### WHEN

* Immediate when visible — before business KPI review.

### 6.2 Sales (SA-KPI)

<a id="sa-kpi-004"></a>
## SA-KPI-004 — Total Faktur

**Location**

- SA01 - Sales Dashboard
- EX01 - Executive (Sales card context)

---

### WHAT

Count Faktur non-void bulan berjalan.
- Billing activity volume.
- Audience: Sales management.
- Sumber: Faktur MTD.

### HOW

* COUNT DISTINCT Faktur issued MTD.
- Void excluded.
- Same period as Achievement.

### WHY

* Intensity billing vs omzet value.
- Activity indicator independent of ticket size.

### WHEN

* Daily SA01; verify SA03 row count.

<a id="sa-kpi-005"></a>
## SA-KPI-005 — Total Customer

**Location**

- SA01 - Sales Dashboard
- EX01 - Executive

---

### WHAT

Count distinct customer invoiced MTD.
- Customer reach / market breadth.
- Audience: Sales management.
- Sumber: Faktur MTD.

### HOW

* COUNT DISTINCT CustomerId/Code with Faktur MTD.
- Non-void only.
- One customer multiple Faktur = 1 count.

### WHY

* Breadth vs depth revenue.
- Complement Top 10 concentration.

### WHEN

* Daily SA01; segment CU01 Active Customer.

<a id="sa-kpi-006"></a>
## SA-KPI-006 — Weekly Invoiced Sales Trend

**Location**

- SA01 - Sales Dashboard
- SA02 - Sales Forecast (Weekly Pace)

---

### WHAT

Omzet Faktur per calendar week dalam bulan berjalan.
- Billing pace / momentum.
- Audience: Sales management.
- Sumber: Faktur grouped by week.

### HOW

* SUM GrandTotal per ISO/calendar week within month.
- Line chart SA01; context chart SA02.
- Non-void Faktur only.

### WHY

* Detect mid-month acceleration/deceleration.
- Context for forecast SA02.

### WHEN

* Mid-month SA01 trend review.
- Deceleration → SA02 Required Daily.

<a id="sa-kpi-007"></a>
## SA-KPI-007 — Top 10 Salesman (Omzet)

**Location**

- SA01 - Sales Dashboard
- EX02 - Alert Center (Below Target context)

---

### WHAT

Ranking Top 10 Salesman by invoiced omzet MTD.
- Performance leaders.
- Audience: Sales management.
- Sumber: Faktur attribution.

### HOW

* Sort Salesman by SUM GrandTotal MTD DESC.
- Max 10 rows.
- Include Code, omzet value.
- Same omzet basis SF01 Top 10.

### WHY

* Identify top performers.
- Coaching context for laggards.

### WHEN

* Daily SA01; compare SF01 for target/achievement %.

<a id="sa-kpi-008"></a>
## SA-KPI-008 — Target vs Achievement Chart

**Location**

- SA01 - Sales Dashboard

---

### WHAT

Visual comparison company Target vs Achievement MTD.
- Bar chart analytics.
- Audience: Sales management.
- Sumber: SA snapshot.

### HOW

* Bar heights = Total Target & Total Achievement.
- Same values SA-KPI-001/002.
- Read-only chart.

### WHY

* Quick visual plan vs actual.
- Presentation to management.

### WHEN

* Daily SA01 executive review.

<a id="sa-kpi-009"></a>
## SA-KPI-009 — Current Sales

**Location**

- SA02 - Sales Forecast

---

### WHAT

Invoiced omzet MTD as-of business date.
- Forecast baseline actual.
- Audience: Sales leadership.
- Sumber: Sales snapshot.

### HOW

* Must equal SA-KPI-002 / BTRPD_SalesKpi.TotalOmzet same refresh.
- Non-void Faktur MTD.

### WHY

* Anchor forecast projection.
- Traceability to SA01.

### WHEN

* Mid-month SA02; evidence SA03.

<a id="sa-kpi-010"></a>
## SA-KPI-010 — Current Achievement %

**Location**

- SA02 - Sales Forecast

---

### WHAT

Achievement % MTD actual.
- Baseline before forecast.
- Audience: Sales leadership.
- Sumber: Sales snapshot.

### HOW

* Current Sales ÷ Total Target × 100%.
- Same band rules §5.1.

### WHY

* Compare actual vs projected achievement.

### WHEN

* SA02 row Actual vs Forecast.

<a id="sa-kpi-011"></a>
## SA-KPI-011 — Forecast Sales (Expected)

**Location**

- SA02 - Sales Forecast

---

### WHAT

Proyeksi omzet month-end jika pace saat ini continues.
- Linear calendar-day extrapolation.
- Audience: Owner, Sales leadership.
- Sumber: computed at refresh.

### HOW

* Forecast Sales = (Current Sales ÷ DE) × DIM.
- DE = max(1, days elapsed); DIM = days in month.
- B = business date from provider.

### WHY

* Answer: will we hit target at current pace?
- Mid-month planning.

### WHEN

* Mid-month SA02 daily.
- Critical band → SA01 coaching.

<a id="sa-kpi-012"></a>
## SA-KPI-012 — Forecast Achievement %

**Location**

- SA02 - Sales Forecast

---

### WHAT

Projected Achievement at month-end.
- Forecast vs Target.
- Audience: Sales leadership.
- Sumber: derived.

### HOW

* Forecast Sales ÷ Total Target × 100%.
- Risk band via Forecast Achievement (§5.1).
- Unknown if Target=0.

### WHY

* Month-end performance projection.
- Trigger corrective action.

### WHEN

* SA02 Forecast Risk card; compare SA-KPI-003.

<a id="sa-kpi-013"></a>
## SA-KPI-013 — Daily Average Sales

**Location**

- SA02 - Sales Forecast

---

### WHAT

Rata-rata billing harian MTD.
- Pace metric.
- Audience: Sales management.
- Sumber: derived.

### HOW

* Current Sales ÷ DE.
- Reference line on Daily Pace Trend chart.

### WHY

* Benchmark vs Required Daily Sales.

### WHEN

* SA02 Daily Pace chart review.

<a id="sa-kpi-014"></a>
## SA-KPI-014 — Required Daily Sales

**Location**

- SA02 - Sales Forecast

---

### WHAT

Rata-rata billing harian needed on remaining days to hit Target.
- Actionable gap metric.
- Audience: Sales leadership.
- Sumber: derived.

### HOW

* (Target − Current Sales) ÷ DR when Target > Current and DR > 0; else 0.
- DR = days remaining in month.
- Warning if >1.5× Daily Average; Critical if >2×.

### WHY

* Quantify catch-up effort.
- Daily ops target for team.

### WHEN

* Mid-month when Forecast below Target.

<a id="sa-kpi-015"></a>
## SA-KPI-015 — Target Gap

**Location**

- SA02 - Sales Forecast

---

### WHAT

Selisih Target vs Forecast Sales.
- Projected shortfall/surplus.
- Audience: Sales leadership.
- Sumber: derived.

### HOW

* Target − Forecast Sales.
- Critical if gap >20% of Target; Warning if >0.

### WHY

* Monetary gap at projected pace.
- Finance cross-check.

### WHEN

* Month-end close planning.

<a id="sa-kpi-016"></a>
## SA-KPI-016 — Days Remaining

**Location**

- SA02 - Sales Forecast

---

### WHAT

Sisa hari kalender dalam bulan.
- Forecast denominator context.
- Audience: Sales leadership.
- Sumber: calendar.

### HOW

* (Month End − Business Date) in calendar days.
- Includes weekends/holidays.

### WHY

* Time left to close gap.
- Confidence context early month.

### WHEN

* SA02 Pace & Gap row.

<a id="sa-kpi-017"></a>
## SA-KPI-017 — Scenario Projection (Best / Expected / Worst)

**Location**

- SA02 - Sales Forecast

---

### WHAT

Range proyeksi month-end omzet.
- Scenario bands.
- Audience: Sales leadership.
- Sumber: derived.

### HOW

* Expected = Forecast Sales.
- Best = MAX(MTD daily avg, recent-7-day avg) × DIM.
- Worst = MIN(MTD daily avg, recent-7-day avg) × DIM.

### WHY

* Uncertainty range for planning.
- Sensitivity to recent momentum.

### WHEN

* Mid-month SA02 Scenario row.

<a id="sa-kpi-018"></a>
## SA-KPI-018 — Forecast Confidence

**Location**

- SA02 - Sales Forecast

---

### WHAT

Reliability indicator for linear forecast.
- Trust label.
- Audience: Management.
- Sumber: days elapsed rule.

### HOW

* DE ≤5 Low; 6–20 Medium; ≥21 High (§5.4).
- Early month summary notes preliminary.

### WHY

* Avoid over-reacting day 1–5.
- Interpret Required Daily cautiously when Low.

### WHEN

* SA02 Scenario & Confidence row.

<a id="sa-kpi-019"></a>
## SA-KPI-019 — Forecast Risk Indicator

**Location**

- SA02 - Sales Forecast

---

### WHAT

Band Healthy/Warning/Critical on **Forecast Achievement %**.
- Forward-looking urgency.
- Audience: Sales leadership.
- Sumber: band resolver.

### HOW

* Same thresholds §5.1 applied to Forecast Achievement %.
- Distinct from current Achievement band.

### WHY

* Proactive intervention before month-end miss.
- Complements SA01 backward view.

### WHEN

* SA02 Forecast Risk card → SA01 if Critical.

### 6.3 Customers (CU-KPI)

<a id="cu-kpi-001"></a>
## CU-KPI-001 — Overdue Customer Count

**Location**

- CU01 - Customers (Collection card)

---

### WHAT

Sama dengan definisi bisnis **FI-KPI-003**.
- Ditampilkan pada menu di Location.
- Lihat entri FI-KPI-003 untuk detail lengkap.

### HOW

* Formula dan business rules identik dengan FI-KPI-003.
- Must reconcile same snapshot refresh.

### WHY

* Tampilan domain-spesifik dari **FI-KPI-003** pada Customer Analytics.
- Tujuan manajemen sama: breadth collection workload di lens customer.

### WHEN

* Review harian **CU01** Collection card.
- Drill-down mengikuti **FI-KPI-003** → **FI04** all-open mode.

<a id="cu-kpi-002"></a>
## CU-KPI-002 — >90 Day Exposure

**Location**

- CU01 - Customers (Collection card)

---

### WHAT

Sama dengan definisi bisnis **FI-KPI-011**.
- Ditampilkan pada menu di Location.
- Lihat entri FI-KPI-011 untuk detail lengkap.

### HOW

* Formula dan business rules identik dengan FI-KPI-011.
- Must reconcile same snapshot refresh.

### WHY

* Tampilan domain-spesifik dari **FI-KPI-011** pada Customer Analytics.
- Chronic exposure context untuk customer attention.

### WHEN

* Review **CU01** Collection card.
- Drill-down **FI01** / **FI02** → **FI04**.

<a id="cu-kpi-003"></a>
## CU-KPI-003 — Top Omzet Customer %

**Location**

- CU01 - Customers (Concentration card)

---

### WHAT

Share MTD omzet held by largest customer.
- Revenue concentration.
- Audience: Sales, Owner.
- Informational — no auto threshold.

### HOW

* Top 1 customer MTD GrandTotal ÷ company MTD omzet × 100%.
- Current month Faktur.

### WHY

* Dependency on single customer for revenue.
- Account management risk.

### WHEN

* CU01 card → SA01 Top rankings.

<a id="cu-kpi-004"></a>
## CU-KPI-004 — Top Piutang Customer %

**Location**

- CU01 - Customers (Concentration card)

---

### WHAT

Share all-time open piutang held by largest customer.
- Receivable concentration.
- Informational.

### HOW

* Top 1 customer open balance ÷ Total Piutang × 100%.
- All-time open snapshot.

### WHY

* Credit concentration risk.
- Complement FI-KPI-004.

### WHEN

* CU01 → FI01 → FI04.

<a id="cu-kpi-005"></a>
## CU-KPI-005 — Active Customer Count

**Location**

- CU01 - Customers (Activity card)

---

### WHAT

Customers with Faktur in current calendar month.
- Healthy activity breadth.

### HOW

* DISTINCT customer with ≥1 non-void Faktur MTD.
- Excludes dormant classification.

### WHY

* Market activity health.
- Compare Dormant count.

### WHEN

* Daily CU01; cross-check SA-KPI-005.

<a id="cu-kpi-006"></a>
## CU-KPI-006 — Dormant Customer Count

**Location**

- CU01 - Customers (Inactivity card)

---

### WHAT

Customers 90+ days no Faktur with prior history, not active MTD.
- Revenue attrition risk.

### HOW

* Last Faktur ≥90 days ago.
- Prior purchase history exists.
- Active MTD excluded.
- Attribution: last invoicing salesman.

### WHY

* Identify lost accounts for recovery.
- Dormant signal in attention list.

### WHEN

* CU01 → Sales Report history; SF01 dormant portfolio.

<a id="cu-kpi-007"></a>
## CU-KPI-007 — Plafond Breach Count

**Location**

- CU01 - Customers (Credit card)

---

### WHAT

Customers with open balance > Plafond when Plafond > 0.
- Credit policy breach.

### HOW

* SUM open balance > customer Plafond master.
- Plafond must be > 0 configured.

### WHY

* Credit limit enforcement.
- Plafond Breach attention signal.

### WHEN

* CU01 attention list → FI04.

<a id="cu-kpi-008"></a>
## CU-KPI-008 — Suspended + Sales Count

**Location**

- CU01 - Customers (Credit card)

---

### WHAT

Customers IsSuspend=true but still invoiced MTD.
- Policy violation.

### HOW

* Master IsSuspend AND ≥1 Faktur current month.
- Suspended + Sales signal.

### WHY

* Operational/compliance exception.
- Master data vs billing mismatch.

### WHEN

* CU01 → Customer master Desktop; SA03 Faktur evidence.

<a id="cu-kpi-009"></a>
## CU-KPI-009 — Top 10 Omzet (Ranking)

**Location**

- CU01 - Customers
- CU04 - Customer Portfolio (concentration)

---

### WHAT

Top 10 customers by MTD invoiced omzet with % of total.

### HOW

* Sort customer MTD GrandTotal DESC, take 10.
- Include % of company MTD omzet.
- Drill SA03 ?q= customer.

### WHY

* Account management priorities.
- Revenue concentration detail.

### WHEN

* CU01 rankings → SA03.

<a id="cu-kpi-010"></a>
## CU-KPI-010 — Top 10 Piutang (Ranking)

**Location**

- CU01 - Customers
- CU04 - Customer Portfolio

---

### WHAT

Top 10 customers by all-time open balance with % of total.

### HOW

* Sort open balance DESC, take 10.
- All-time open semantics.
- Drill FI04 all-open ?q=.

### WHY

* Largest debtors for collection.
- Complement FI-KPI-012.

### WHEN

* CU01 → FI04 investigation.

<a id="cu-kpi-020"></a>
## CU-KPI-020 — Customers Forecasted at Risk

**Location**

- CU02 - Customer Risk Forecast

---

### WHAT

Customer Risk Forecast — Customers Forecasted at Risk.
- Forward-looking 30-day horizon.
- Audience: Finance, Sales leadership.
- Deterministic rules (CRF-*).

### HOW

* Count category ∈ {Watch, Attention, High Risk, Critical}.
- Traceability: CRF-KPI-01.
- Indicative forecast — not automatic credit hold.

### WHY

* Preventive collection and credit planning.
- Early intervention before overdue.

### WHEN

* Mid-month CU02 → CU03 action queue → FI04/FI02.

<a id="cu-kpi-021"></a>
## CU-KPI-021 — High Risk Customer Count

**Location**

- CU02 - Customer Risk Forecast

---

### WHAT

Customer Risk Forecast — High Risk Customer Count.
- Forward-looking 30-day horizon.
- Audience: Finance, Sales leadership.
- Deterministic rules (CRF-*).

### HOW

* Count {High Risk, Critical}.
- Traceability: CRF-KPI-02.
- Indicative forecast — not automatic credit hold.

### WHY

* Preventive collection and credit planning.
- Early intervention before overdue.

### WHEN

* Mid-month CU02 → CU03 action queue → FI04/FI02.

<a id="cu-kpi-022"></a>
## CU-KPI-022 — Elevated Risk Receivable

**Location**

- CU02 - Customer Risk Forecast

---

### WHAT

Customer Risk Forecast — Elevated Risk Receivable.
- Forward-looking 30-day horizon.
- Audience: Finance, Sales leadership.
- Deterministic rules (CRF-*).

### HOW

* SUM open balance High Risk + Critical.
- Traceability: CRF-KPI-03.
- Indicative forecast — not automatic credit hold.

### WHY

* Preventive collection and credit planning.
- Early intervention before overdue.

### WHEN

* Mid-month CU02 → CU03 action queue → FI04/FI02.

<a id="cu-kpi-023"></a>
## CU-KPI-023 — Elevated Risk Receivable %

**Location**

- CU02 - Customer Risk Forecast

---

### WHAT

Customer Risk Forecast — Elevated Risk Receivable %.
- Forward-looking 30-day horizon.
- Audience: Finance, Sales leadership.
- Deterministic rules (CRF-*).

### HOW

* CRF-KPI-03 ÷ Total Piutang × 100.
- Traceability: CRF-KPI-04.
- Indicative forecast — not automatic credit hold.

### WHY

* Preventive collection and credit planning.
- Early intervention before overdue.

### WHEN

* Mid-month CU02 → CU03 action queue → FI04/FI02.

<a id="cu-kpi-024"></a>
## CU-KPI-024 — Portfolio Health Score

**Location**

- CU02 - Customer Risk Forecast

---

### WHAT

Customer Risk Forecast — Portfolio Health Score.
- Forward-looking 30-day horizon.
- Audience: Finance, Sales leadership.
- Deterministic rules (CRF-*).

### HOW

* 100 − MIN(100, (elevated%/total×50) + (highRiskCount/active×50)).
- Traceability: CRF-KPI-05.
- Indicative forecast — not automatic credit hold.

### WHY

* Preventive collection and credit planning.
- Early intervention before overdue.

### WHEN

* Mid-month CU02 → CU03 action queue → FI04/FI02.

<a id="cu-kpi-025"></a>
## CU-KPI-025 — Forecast Confidence

**Location**

- CU02 - Customer Risk Forecast

---

### WHAT

Customer Risk Forecast — Forecast Confidence.
- Forward-looking 30-day horizon.
- Audience: Finance, Sales leadership.
- Deterministic rules (CRF-*).

### HOW

* §5.4 days elapsed.
- Traceability: CRF-KPI-06.
- Indicative forecast — not automatic credit hold.

### WHY

* Preventive collection and credit planning.
- Early intervention before overdue.

### WHEN

* Mid-month CU02 → CU03 action queue → FI04/FI02.

<a id="cu-kpi-026"></a>
## CU-KPI-026 — Total Piutang (Context)

**Location**

- CU02 - Customer Risk Forecast

---

### WHAT

Customer Risk Forecast — Total Piutang (Context).
- Forward-looking 30-day horizon.
- Audience: Finance, Sales leadership.
- Deterministic rules (CRF-*).

### HOW

* Must match FI-KPI-001 same refresh.
- Traceability: CRF-KPI-07.
- Indicative forecast — not automatic credit hold.

### WHY

* Preventive collection and credit planning.
- Early intervention before overdue.

### WHEN

* Mid-month CU02 → CU03 action queue → FI04/FI02.

<a id="cu-kpi-027"></a>
## CU-KPI-027 — Payment Delay Signal Count

**Location**

- CU02 - Customer Risk Forecast (Signal mix KPI row)

---

### WHAT

Count customers with any **CRF-P** forecast rule triggered.
- Signal family mix.
- Audience: Risk analysts.

### HOW

* COUNT customers with ≥1 active CRF-P rule.
- Rules documented in portal-analysis M29.
- Each row includes rule traceability.

### WHY

* Understand dominant risk drivers.
- Allocate team specialization.

### WHEN

* CU02 signal mix chart → customer row detail.

<a id="cu-kpi-028"></a>
## CU-KPI-028 — Credit Limit Signal Count

**Location**

- CU02 - Customer Risk Forecast (Signal mix KPI row)

---

### WHAT

Count customers with any **CRF-C** forecast rule triggered.
- Signal family mix.
- Audience: Risk analysts.

### HOW

* COUNT customers with ≥1 active CRF-C rule.
- Rules documented in portal-analysis M29.
- Each row includes rule traceability.

### WHY

* Understand dominant risk drivers.
- Allocate team specialization.

### WHEN

* CU02 signal mix chart → customer row detail.

<a id="cu-kpi-029"></a>
## CU-KPI-029 — Inactivity Signal Count

**Location**

- CU02 - Customer Risk Forecast (Signal mix KPI row)

---

### WHAT

Count customers with any **CRF-I** forecast rule triggered.
- Signal family mix.
- Audience: Risk analysts.

### HOW

* COUNT customers with ≥1 active CRF-I rule.
- Rules documented in portal-analysis M29.
- Each row includes rule traceability.

### WHY

* Understand dominant risk drivers.
- Allocate team specialization.

### WHEN

* CU02 signal mix chart → customer row detail.

<a id="cu-kpi-030"></a>
## CU-KPI-030 — Purchase Decline Signal Count

**Location**

- CU02 - Customer Risk Forecast (Signal mix KPI row)

---

### WHAT

Count customers with any **CRF-D** forecast rule triggered.
- Signal family mix.
- Audience: Risk analysts.

### HOW

* COUNT customers with ≥1 active CRF-D rule.
- Rules documented in portal-analysis M29.
- Each row includes rule traceability.

### WHY

* Understand dominant risk drivers.
- Allocate team specialization.

### WHEN

* CU02 signal mix chart → customer row detail.

<a id="cu-kpi-031"></a>
## CU-KPI-031 — Collection Risk Signal Count

**Location**

- CU02 - Customer Risk Forecast (Signal mix KPI row)

---

### WHAT

Count customers with any **CRF-L** forecast rule triggered.
- Signal family mix.
- Audience: Risk analysts.

### HOW

* COUNT customers with ≥1 active CRF-L rule.
- Rules documented in portal-analysis M29.
- Each row includes rule traceability.

### WHY

* Understand dominant risk drivers.
- Allocate team specialization.

### WHEN

* CU02 signal mix chart → customer row detail.

<a id="cu-kpi-032"></a>
## CU-KPI-032 — Risk Category Count — Healthy

**Location**

- CU02 - Customer Risk Forecast (Risk distribution chart)

---

### WHAT

Count customers in forecast category **Healthy**.

### HOW

* COUNT category = Healthy per M29 category resolver.
- Categories derived from signal severity combinations.

### WHY

* Portfolio risk distribution.
- Resource planning by severity.

### WHEN

* CU02 distribution chart → Top risk customers table.

<a id="cu-kpi-033"></a>
## CU-KPI-033 — Risk Category Count — Watch

**Location**

- CU02 - Customer Risk Forecast (Risk distribution chart)

---

### WHAT

Count customers in forecast category **Watch**.

### HOW

* COUNT category = Watch per M29 category resolver.
- Categories derived from signal severity combinations.

### WHY

* Portfolio risk distribution.
- Resource planning by severity.

### WHEN

* CU02 distribution chart → Top risk customers table.

<a id="cu-kpi-034"></a>
## CU-KPI-034 — Risk Category Count — Attention

**Location**

- CU02 - Customer Risk Forecast (Risk distribution chart)

---

### WHAT

Count customers in forecast category **Attention**.

### HOW

* COUNT category = Attention per M29 category resolver.
- Categories derived from signal severity combinations.

### WHY

* Portfolio risk distribution.
- Resource planning by severity.

### WHEN

* CU02 distribution chart → Top risk customers table.

<a id="cu-kpi-035"></a>
## CU-KPI-035 — Risk Category Count — High Risk

**Location**

- CU02 - Customer Risk Forecast (Risk distribution chart)

---

### WHAT

Count customers in forecast category **High Risk**.

### HOW

* COUNT category = High Risk per M29 category resolver.
- Categories derived from signal severity combinations.

### WHY

* Portfolio risk distribution.
- Resource planning by severity.

### WHEN

* CU02 distribution chart → Top risk customers table.

<a id="cu-kpi-036"></a>
## CU-KPI-036 — Risk Category Count — Critical

**Location**

- CU02 - Customer Risk Forecast (Risk distribution chart)

---

### WHAT

Count customers in forecast category **Critical**.

### HOW

* COUNT category = Critical per M29 category resolver.
- Categories derived from signal severity combinations.

### WHY

* Portfolio risk distribution.
- Resource planning by severity.

### WHEN

* CU02 distribution chart → Top risk customers table.

<a id="cu-kpi-040"></a>
## CU-KPI-040 — Actions Today

**Location**

- CU03 - Collection Optimization

---

### WHAT

Collection Optimization — Actions Today.
- Daily prioritized contact workload.
- Consumes M29 — does not recalculate forecast.
- Audience: Finance, Collection.

### HOW

* Count ActionCategory ∉ {DeferCollection, NoActionToday}.
- Traceability: COL-OPT-KPI-01.
- Recommendation disclaimer — not auto Desktop action.

### WHY

* Morning collection planning.
- Separate Sales recovery vs Finance collection.

### WHEN

* Daily morning CU03 → FI04/FI02; Sales recovery → SA03.

<a id="cu-kpi-041"></a>
## CU-KPI-041 — Immediate Collection Count

**Location**

- CU03 - Collection Optimization

---

### WHAT

Collection Optimization — Immediate Collection Count.
- Daily prioritized contact workload.
- Consumes M29 — does not recalculate forecast.
- Audience: Finance, Collection.

### HOW

* Count ImmediateCollection.
- Traceability: COL-OPT-KPI-02.
- Recommendation disclaimer — not auto Desktop action.

### WHY

* Morning collection planning.
- Separate Sales recovery vs Finance collection.

### WHEN

* Daily morning CU03 → FI04/FI02; Sales recovery → SA03.

<a id="cu-kpi-042"></a>
## CU-KPI-042 — Proactive Reminder Count

**Location**

- CU03 - Collection Optimization

---

### WHAT

Collection Optimization — Proactive Reminder Count.
- Daily prioritized contact workload.
- Consumes M29 — does not recalculate forecast.
- Audience: Finance, Collection.

### HOW

* Count ProactiveReminder.
- Traceability: COL-OPT-KPI-03.
- Recommendation disclaimer — not auto Desktop action.

### WHY

* Morning collection planning.
- Separate Sales recovery vs Finance collection.

### WHEN

* Daily morning CU03 → FI04/FI02; Sales recovery → SA03.

<a id="cu-kpi-043"></a>
## CU-KPI-043 — Credit Review Count

**Location**

- CU03 - Collection Optimization

---

### WHAT

Collection Optimization — Credit Review Count.
- Daily prioritized contact workload.
- Consumes M29 — does not recalculate forecast.
- Audience: Finance, Collection.

### HOW

* Count CreditReview.
- Traceability: COL-OPT-KPI-04.
- Recommendation disclaimer — not auto Desktop action.

### WHY

* Morning collection planning.
- Separate Sales recovery vs Finance collection.

### WHEN

* Daily morning CU03 → FI04/FI02; Sales recovery → SA03.

<a id="cu-kpi-044"></a>
## CU-KPI-044 — Sales Recovery Count

**Location**

- CU03 - Collection Optimization

---

### WHAT

Collection Optimization — Sales Recovery Count.
- Daily prioritized contact workload.
- Consumes M29 — does not recalculate forecast.
- Audience: Finance, Collection.

### HOW

* Count SalesRecoveryVisit.
- Traceability: COL-OPT-KPI-05.
- Recommendation disclaimer — not auto Desktop action.

### WHY

* Morning collection planning.
- Separate Sales recovery vs Finance collection.

### WHEN

* Daily morning CU03 → FI04/FI02; Sales recovery → SA03.

<a id="cu-kpi-045"></a>
## CU-KPI-045 — Management Escalation Count

**Location**

- CU03 - Collection Optimization

---

### WHAT

Collection Optimization — Management Escalation Count.
- Daily prioritized contact workload.
- Consumes M29 — does not recalculate forecast.
- Audience: Finance, Collection.

### HOW

* Count EscalateManagement.
- Traceability: COL-OPT-KPI-06.
- Recommendation disclaimer — not auto Desktop action.

### WHY

* Morning collection planning.
- Separate Sales recovery vs Finance collection.

### WHEN

* Daily morning CU03 → FI04/FI02; Sales recovery → SA03.

<a id="cu-kpi-046"></a>
## CU-KPI-046 — Collection Impact Total

**Location**

- CU03 - Collection Optimization

---

### WHAT

Collection Optimization — Collection Impact Total.
- Daily prioritized contact workload.
- Consumes M29 — does not recalculate forecast.
- Audience: Finance, Collection.

### HOW

* SUM CollectionImpactAmount actionable customers.
- Traceability: COL-OPT-KPI-07.
- Recommendation disclaimer — not auto Desktop action.

### WHY

* Morning collection planning.
- Separate Sales recovery vs Finance collection.

### WHEN

* Daily morning CU03 → FI04/FI02; Sales recovery → SA03.

<a id="cu-kpi-047"></a>
## CU-KPI-047 — Overdue Exposure (Context)

**Location**

- CU03 - Collection Optimization

---

### WHAT

Collection Optimization — Overdue Exposure (Context).
- Daily prioritized contact workload.
- Consumes M29 — does not recalculate forecast.
- Audience: Finance, Collection.

### HOW

* Must match FI-KPI-013; cross-read M20.
- Traceability: COL-OPT-KPI-10.
- Recommendation disclaimer — not auto Desktop action.

### WHY

* Morning collection planning.
- Separate Sales recovery vs Finance collection.

### WHEN

* Daily morning CU03 → FI04/FI02; Sales recovery → SA03.

<a id="cu-kpi-048"></a>
## CU-KPI-048 — Due Within 7 Days

**Location**

- CU03 - Collection Optimization

---

### WHAT

Collection Optimization — Due Within 7 Days.
- Daily prioritized contact workload.
- Consumes M29 — does not recalculate forecast.
- Audience: Finance, Collection.

### HOW

* SUM balances due in next 7 days.
- Traceability: COL-OPT-KPI-11.
- Recommendation disclaimer — not auto Desktop action.

### WHY

* Morning collection planning.
- Separate Sales recovery vs Finance collection.

### WHEN

* Daily morning CU03 → FI04/FI02; Sales recovery → SA03.

<a id="cu-kpi-049"></a>
## CU-KPI-049 — Recovery vs Billing % (Context)

**Location**

- CU03 - Collection Optimization

---

### WHAT

Collection Optimization — Recovery vs Billing % (Context).
- Daily prioritized contact workload.
- Consumes M29 — does not recalculate forecast.
- Audience: Finance, Collection.

### HOW

* Copied FI-KPI-017 — not recomputed.
- Traceability: COL-OPT-KPI-12.
- Recommendation disclaimer — not auto Desktop action.

### WHY

* Morning collection planning.
- Separate Sales recovery vs Finance collection.

### WHEN

* Daily morning CU03 → FI04/FI02; Sales recovery → SA03.

<a id="cu-kpi-050"></a>
## CU-KPI-050 — Planning Confidence

**Location**

- CU03 - Collection Optimization

---

### WHAT

Collection Optimization — Planning Confidence.
- Daily prioritized contact workload.
- Consumes M29 — does not recalculate forecast.
- Audience: Finance, Collection.

### HOW

* §5.4 pattern.
- Traceability: COL-OPT-KPI-14.
- Recommendation disclaimer — not auto Desktop action.

### WHY

* Morning collection planning.
- Separate Sales recovery vs Finance collection.

### WHEN

* Daily morning CU03 → FI04/FI02; Sales recovery → SA03.

<a id="cu-kpi-051"></a>
## CU-KPI-051 — Immediate Impact Total

**Location**

- CU03 - Collection Optimization

---

### WHAT

Collection Optimization — Immediate Impact Total.
- Daily prioritized contact workload.
- Consumes M29 — does not recalculate forecast.
- Audience: Finance, Collection.

### HOW

* SUM impact Immediate + Priority actions.
- Traceability: COL-OPT-KPI-13.
- Recommendation disclaimer — not auto Desktop action.

### WHY

* Morning collection planning.
- Separate Sales recovery vs Finance collection.

### WHEN

* Daily morning CU03 → FI04/FI02; Sales recovery → SA03.

<a id="cu-kpi-060"></a>
## CU-KPI-060 — Portfolio Health Score

**Location**

- CU04 - Customer Portfolio

---

### WHAT

Customer Portfolio — Portfolio Health Score.
- Composition milestone M31.
- Consumes M17+M29+M30.
- Customer Value = omzet proxy NOT profitability.

### HOW

* Copied M29 CU-KPI-024.
- Tier/Lifecycle/Action rules in CustomerPortfolioActionBuilder.
- Collect action links CU03 ?customerKey=.

### WHY

* Strategic portfolio management.
- Grow/Retain/Protect/Collect/Recover decisions.

### WHEN

* Weekly CU04 filters → CU05 Customer Report row.

<a id="cu-kpi-061"></a>
## CU-KPI-061 — Portfolio Healthy %

**Location**

- CU04 - Customer Portfolio
- EX01 - Executive

---

### WHAT

Customer Portfolio — Portfolio Healthy %.
- Composition milestone M31.
- Consumes M17+M29+M30.
- Customer Value = omzet proxy NOT profitability.

### HOW

* Healthy Count ÷ Total × 100.
- Tier/Lifecycle/Action rules in CustomerPortfolioActionBuilder.
- Collect action links CU03 ?customerKey=.

### WHY

* Strategic portfolio management.
- Grow/Retain/Protect/Collect/Recover decisions.

### WHEN

* Weekly CU04 filters → CU05 Customer Report row.

<a id="cu-kpi-062"></a>
## CU-KPI-062 — Attention Customer Count

**Location**

- CU04 - Customer Portfolio

---

### WHAT

Customer Portfolio — Attention Customer Count.
- Composition milestone M31.
- Consumes M17+M29+M30.
- Customer Value = omzet proxy NOT profitability.

### HOW

* Count IsAttention=true.
- Tier/Lifecycle/Action rules in CustomerPortfolioActionBuilder.
- Collect action links CU03 ?customerKey=.

### WHY

* Strategic portfolio management.
- Grow/Retain/Protect/Collect/Recover decisions.

### WHEN

* Weekly CU04 filters → CU05 Customer Report row.

<a id="cu-kpi-063"></a>
## CU-KPI-063 — Strategic Customer Count

**Location**

- CU04 - Customer Portfolio

---

### WHAT

Customer Portfolio — Strategic Customer Count.
- Composition milestone M31.
- Consumes M17+M29+M30.
- Customer Value = omzet proxy NOT profitability.

### HOW

* Count tier=Strategic.
- Tier/Lifecycle/Action rules in CustomerPortfolioActionBuilder.
- Collect action links CU03 ?customerKey=.

### WHY

* Strategic portfolio management.
- Grow/Retain/Protect/Collect/Recover decisions.

### WHEN

* Weekly CU04 filters → CU05 Customer Report row.

<a id="cu-kpi-064"></a>
## CU-KPI-064 — Strategic At Risk Count

**Location**

- CU04 - Customer Portfolio
- EX01

---

### WHAT

Customer Portfolio — Strategic At Risk Count.
- Composition milestone M31.
- Consumes M17+M29+M30.
- Customer Value = omzet proxy NOT profitability.

### HOW

* Strategic AND M29≥Watch.
- Tier/Lifecycle/Action rules in CustomerPortfolioActionBuilder.
- Collect action links CU03 ?customerKey=.

### WHY

* Strategic portfolio management.
- Grow/Retain/Protect/Collect/Recover decisions.

### WHEN

* Weekly CU04 filters → CU05 Customer Report row.

<a id="cu-kpi-065"></a>
## CU-KPI-065 — Working Capital Tied Amount

**Location**

- CU04 - Customer Portfolio

---

### WHAT

Customer Portfolio — Working Capital Tied Amount.
- Composition milestone M31.
- Consumes M17+M29+M30.
- Customer Value = omzet proxy NOT profitability.

### HOW

* SUM open balance attention customers.
- Tier/Lifecycle/Action rules in CustomerPortfolioActionBuilder.
- Collect action links CU03 ?customerKey=.

### WHY

* Strategic portfolio management.
- Grow/Retain/Protect/Collect/Recover decisions.

### WHEN

* Weekly CU04 filters → CU05 Customer Report row.

<a id="cu-kpi-066"></a>
## CU-KPI-066 — Total MTD Omzet (Portfolio)

**Location**

- CU04 - Customer Portfolio

---

### WHAT

Customer Portfolio — Total MTD Omzet (Portfolio).
- Composition milestone M31.
- Consumes M17+M29+M30.
- Customer Value = omzet proxy NOT profitability.

### HOW

* SUM MtdOmzet all portfolio.
- Tier/Lifecycle/Action rules in CustomerPortfolioActionBuilder.
- Collect action links CU03 ?customerKey=.

### WHY

* Strategic portfolio management.
- Grow/Retain/Protect/Collect/Recover decisions.

### WHEN

* Weekly CU04 filters → CU05 Customer Report row.

<a id="cu-kpi-067"></a>
## CU-KPI-067 — Total Open Balance (Portfolio)

**Location**

- CU04 - Customer Portfolio

---

### WHAT

Customer Portfolio — Total Open Balance (Portfolio).
- Composition milestone M31.
- Consumes M17+M29+M30.
- Customer Value = omzet proxy NOT profitability.

### HOW

* SUM OpenBalance portfolio.
- Tier/Lifecycle/Action rules in CustomerPortfolioActionBuilder.
- Collect action links CU03 ?customerKey=.

### WHY

* Strategic portfolio management.
- Grow/Retain/Protect/Collect/Recover decisions.

### WHEN

* Weekly CU04 filters → CU05 Customer Report row.

<a id="cu-kpi-068"></a>
## CU-KPI-068 — Never Purchased Count

**Location**

- CU04 - Customer Portfolio

---

### WHAT

Customer Portfolio — Never Purchased Count.
- Composition milestone M31.
- Consumes M17+M29+M30.
- Customer Value = omzet proxy NOT profitability.

### HOW

* Lifecycle NeverPurchased.
- Tier/Lifecycle/Action rules in CustomerPortfolioActionBuilder.
- Collect action links CU03 ?customerKey=.

### WHY

* Strategic portfolio management.
- Grow/Retain/Protect/Collect/Recover decisions.

### WHEN

* Weekly CU04 filters → CU05 Customer Report row.

<a id="cu-kpi-069"></a>
## CU-KPI-069 — Dormant Count (Lifecycle)

**Location**

- CU04 - Customer Portfolio

---

### WHAT

Customer Portfolio — Dormant Count (Lifecycle).
- Composition milestone M31.
- Consumes M17+M29+M30.
- Customer Value = omzet proxy NOT profitability.

### HOW

* Lifecycle Dormant.
- Tier/Lifecycle/Action rules in CustomerPortfolioActionBuilder.
- Collect action links CU03 ?customerKey=.

### WHY

* Strategic portfolio management.
- Grow/Retain/Protect/Collect/Recover decisions.

### WHEN

* Weekly CU04 filters → CU05 Customer Report row.

<a id="cu-kpi-070"></a>
## CU-KPI-070 — Declining Count

**Location**

- CU04 - Customer Portfolio

---

### WHAT

Customer Portfolio — Declining Count.
- Composition milestone M31.
- Consumes M17+M29+M30.
- Customer Value = omzet proxy NOT profitability.

### HOW

* Lifecycle Declining.
- Tier/Lifecycle/Action rules in CustomerPortfolioActionBuilder.
- Collect action links CU03 ?customerKey=.

### WHY

* Strategic portfolio management.
- Grow/Retain/Protect/Collect/Recover decisions.

### WHEN

* Weekly CU04 filters → CU05 Customer Report row.

<a id="cu-kpi-071"></a>
## CU-KPI-071 — Customer Report Row Metrics

**Location**

- CU05 - Customer Report

---

### WHAT

Per-customer row: lifecycle, tier, portfolio action, salesman, MTD omzet, open balance.
- Evidence layer for M31.
- One row per customer from M31 snapshot.

### HOW

* Grain: one row per customer.
- Fields from M31 materialized snapshot.
- Optional ?customerCode= pre-filter.
- No footer aggregate totals.

### WHY

* Tabular evidence for portfolio decisions.
- Drill from CU04 queue.

### WHEN

* CU04 row click → CU05; cross-check SA03/FI04.

### 6.4 Finance (FI-KPI)

<a id="fi-kpi-001"></a>
## FI-KPI-001 — Total Piutang

**Location**

- FI01 - Piutang Dashboard
- FI04 - Piutang Report (footer, all-open mode)
- EX01 - Executive

---

### WHAT

Total saldo piutang terbuka all-time.
- Primary finance exposure KPI.
- Audience: Finance management.
- Sumber: open Faktur.

### HOW

* SUM(KurangBayar) where > 1.
- All-time snapshot FI01.
- FI04 footer matches when periodMode=allOpenBalances.
- Default FI04 period filter may differ (see WHEN).

### WHY

* Core receivable monitoring.
- Footer traceability builds trust.

### WHEN

* Daily FI01.
- FI04: use all-open mode from dashboard drill-down; default period for ad-hoc browse.

<a id="fi-kpi-002"></a>
## FI-KPI-002 — Total Customer (with balance)

**Location**

- FI01 - Piutang Dashboard
- FI04 - Piutang Report (footer)

---

### WHAT

Count distinct customer dengan open balance.
- Breadth receivable exposure.
- Audience: Finance.
- Sumber: open Faktur.

### HOW

* COUNT DISTINCT customer with KurangBayar > 1.
- FI04 footer must match FI01 when all-open unfiltered.

### WHY

* How many accounts owe money.
- Collection workload breadth.

### WHEN

* Daily FI01; verify FI04 footer.

<a id="fi-kpi-003"></a>
## FI-KPI-003 — Overdue Customer

**Location**

- FI01 - Piutang Dashboard
- EX01 - Executive
- CU01 - Collection card

---

### WHAT

Count customer dengan any past-due balance.
- Collection action breadth.
- Audience: Finance, Collection.
- Sumber: aging vs Jatuh Tempo.

### HOW

* DISTINCT customers with balance in bucket ≠ Current.
- Current = not yet due.

### WHY

* Priority count for collection team.
- Executive attention signal.

### WHEN

* Daily FI01 → FI02 attention list.

<a id="fi-kpi-004"></a>
## FI-KPI-004 — Top 10 Customer %

**Location**

- FI01 - Piutang Dashboard

---

### WHAT

Share Total Piutang held by top 10 customers.
- Concentration metric.
- Audience: Finance, Owner.
- Sumber: customer ranking.

### HOW

* SUM(top 10 balances) ÷ Total Piutang × 100%.
- Informational threshold.

### WHY

* Default risk if top debtors fail.
- Credit policy review.

### WHEN

* Monthly FI01 Top 10 table → FI04.

<a id="fi-kpi-005"></a>
## FI-KPI-005 — Top 20 Customer %

**Location**

- FI01 - Piutang Dashboard

---

### WHAT

Share Total Piutang held by top 20 customers.
- Wider concentration view.
- Audience: Finance.
- Sumber: customer ranking.

### HOW

* SUM(top 20 balances) ÷ Total Piutang × 100%.
- M14 V2 Top 20 Risk table companion.

### WHY

* Broader concentration than Top 10.
- Portfolio risk assessment.

### WHEN

* FI01 Top 20 table review.

<a id="fi-kpi-006"></a>
## FI-KPI-006 — Aging Bucket — Current

**Location**

- FI01 - Piutang Dashboard (Aging Distribution pie)

---

### WHAT

Nilai piutang di bucket aging **Current**.
- Portfolio quality distribution.
- Audience: Finance.
- Sumber: Jatuh Tempo vs today.

### HOW

* SUM KurangBayar assigned to bucket Current.
- Sum of all buckets = Total Piutang.
- Boundaries per M14 inclusive policy.

### WHY

* Understand receivable age profile.
- Bucket >90 = chronic risk.

### WHEN

* Daily FI01 pie chart.
- >90 Days → FI02 Chronic Overdue signals.

<a id="fi-kpi-007"></a>
## FI-KPI-007 — Aging Bucket — 1–30 Days

**Location**

- FI01 - Piutang Dashboard (Aging Distribution pie)

---

### WHAT

Nilai piutang di bucket aging **1–30 Days**.
- Portfolio quality distribution.
- Audience: Finance.
- Sumber: Jatuh Tempo vs today.

### HOW

* SUM KurangBayar assigned to bucket 1–30 Days.
- Sum of all buckets = Total Piutang.
- Boundaries per M14 inclusive policy.

### WHY

* Understand receivable age profile.
- Bucket >90 = chronic risk.

### WHEN

* Daily FI01 pie chart.
- >90 Days → FI02 Chronic Overdue signals.

<a id="fi-kpi-008"></a>
## FI-KPI-008 — Aging Bucket — 31–60 Days

**Location**

- FI01 - Piutang Dashboard (Aging Distribution pie)

---

### WHAT

Nilai piutang di bucket aging **31–60 Days**.
- Portfolio quality distribution.
- Audience: Finance.
- Sumber: Jatuh Tempo vs today.

### HOW

* SUM KurangBayar assigned to bucket 31–60 Days.
- Sum of all buckets = Total Piutang.
- Boundaries per M14 inclusive policy.

### WHY

* Understand receivable age profile.
- Bucket >90 = chronic risk.

### WHEN

* Daily FI01 pie chart.
- >90 Days → FI02 Chronic Overdue signals.

<a id="fi-kpi-009"></a>
## FI-KPI-009 — Aging Bucket — 61–90 Days

**Location**

- FI01 - Piutang Dashboard (Aging Distribution pie)

---

### WHAT

Nilai piutang di bucket aging **61–90 Days**.
- Portfolio quality distribution.
- Audience: Finance.
- Sumber: Jatuh Tempo vs today.

### HOW

* SUM KurangBayar assigned to bucket 61–90 Days.
- Sum of all buckets = Total Piutang.
- Boundaries per M14 inclusive policy.

### WHY

* Understand receivable age profile.
- Bucket >90 = chronic risk.

### WHEN

* Daily FI01 pie chart.
- >90 Days → FI02 Chronic Overdue signals.

<a id="fi-kpi-010"></a>
## FI-KPI-010 — Aging Bucket — > 90 Days

**Location**

- FI01 - Piutang Dashboard (Aging Distribution pie)

---

### WHAT

Nilai piutang di bucket aging **> 90 Days**.
- Portfolio quality distribution.
- Audience: Finance.
- Sumber: Jatuh Tempo vs today.

### HOW

* SUM KurangBayar assigned to bucket > 90 Days.
- Sum of all buckets = Total Piutang.
- Boundaries per M14 inclusive policy.

### WHY

* Understand receivable age profile.
- Bucket >90 = chronic risk.

### WHEN

* Daily FI01 pie chart.
- >90 Days → FI02 Chronic Overdue signals.

<a id="fi-kpi-011"></a>
## FI-KPI-011 — Piutang > 90 Hari (Amount & %)

**Location**

- FI01 - Piutang Dashboard
- EX01 - Executive
- FI02 - Collection

---

### WHAT

Chronic overdue amount and % of Total Piutang.
- Bad-debt escalation signal.
- Audience: Finance, Owner.
- Sumber: >90 Days bucket.

### HOW

* Amount = FI-KPI-010 bucket sum.
- % = Amount ÷ Total Piutang × 100%.

### WHY

* Escalation and write-off review.
- Same as EX-KPI-006 / FI-KPI-010 aggregate.

### WHEN

* FI01 → FI02 >90d Exposure card.

<a id="fi-kpi-012"></a>
## FI-KPI-012 — Top 10 Outstanding Customers

**Location**

- FI01 - Piutang Dashboard

---

### WHAT

Ranking Top 10 customer by total open balance.
- Collection priority queue.
- Audience: Finance.
- Sumber: all-time open balance.

### HOW

* Sort customer total KurangBayar DESC, take 10.
- Include balance amount per row.
- Drill-down FI04 ?q= customer name.

### WHY

* Largest debtors first.
- Cash recovery prioritization.

### WHEN

* Daily FI01 table → FI04 investigation.

<a id="fi-kpi-013"></a>
## FI-KPI-013 — Overdue Exposure

**Location**

- FI02 - Collection Dashboard
- FI03 - Cash Flow Forecast (context)
- CU03 - Collection Optimization

---

### WHAT

Total past-due receivable amount (exclude Current bucket).
- Collection workload scale.
- Audience: Collection management.
- Sumber: open balance aging.

### HOW

* SUM KurangBayar where bucket ≠ Current.
- All-time open.
- Must match CU-KPI-030 / COL-OPT-KPI-10 same cycle.

### WHY

* Monetary urgency for collection.
- Differentiator from FI01 Total Piutang (includes current).

### WHEN

* Daily FI02 Exposure card → attention list.

<a id="fi-kpi-014"></a>
## FI-KPI-014 — >90d Exposure

**Location**

- FI02 - Collection Dashboard

---

### WHAT

Sum overdue balance in >90 Days bucket only.
- Chronic collection risk.
- Audience: Collection, Finance.
- Sumber: aging buckets.

### HOW

* SUM KurangBayar in DaysOver90 bucket.
- Subset of FI-KPI-013.

### WHY

* Escalation queue severity.
- ChronicOverdue signal basis.

### WHEN

* FI02 Aging Risk Summary → FI04.

<a id="fi-kpi-015"></a>
## FI-KPI-015 — Overdue Concentration %

**Location**

- FI02 - Collection Dashboard
- EX02 - Alert Center (Concentrations)
- FI03 - Cash Flow Forecast

---

### WHAT

Top-1 customer share of **total company overdue**.
- Collection priority concentration.
- Audience: Collection, Owner.
- Sumber: overdue ranking.

### HOW

* Top customer overdue balance ÷ total company overdue × 100%.
- Wilayah Hotspot uses 15% company overdue threshold (different grain).

### WHY

* Focus collection on largest overdue debtor.
- OverdueConcentration risk rule FI03.

### WHEN

* FI02 Recovery card context; FI04 top overdue customer.

<a id="fi-kpi-016"></a>
## FI-KPI-016 — Cash Collected MTD

**Location**

- FI02 - Collection Dashboard
- FI03 - Cash Flow Forecast
- EX01 - Executive (future promotion)

---

### WHAT

Total cash payments received MTD (BayarTunai).
- Actual recovery pace.
- Audience: Collection, Finance.
- Sumber: pelunasan FF2 current month.

### HOW

* SUM(BayarTunai) grouped by LunasDate in current month.
- UangMuka excluded.
- Must match FI-KPI-019 / CFR traceability.

### WHY

* Liquidity from collections.
- Cash component of recovery.

### WHEN

* Daily FI02 Recovery Summary → FI03 forecast.

<a id="fi-kpi-017"></a>
## FI-KPI-017 — Recovery vs Billing %

**Location**

- FI02 - Collection Dashboard
- FI03 - Cash Flow Forecast
- CU03 - Collection Optimization
- EX02 - Concentrations

---

### WHAT

Ratio collections to new invoiced omzet MTD.
- Are we keeping up with new debt?
- Audience: Collection, Finance.
- Sumber: pelunasan + Faktur MTD.

### HOW

* TotalBayar (Cash+Giro) ÷ Month Faktur Omzet × 100%.
- Null when omzet = 0.
- CU03 copies from M20 — may lag ≤30 min.
- RequiresAttention FI02 when <100% and omzet>0.

### WHY

* Working capital cycle health.
- Low recovery → growing net exposure.

### WHEN

* Daily FI02; mid-month FI03; action queue CU03.

<a id="fi-kpi-018"></a>
## FI-KPI-018 — Payment Mix — Cash

**Location**

- FI02 - Collection Dashboard (Recovery Summary)

---

### WHAT

Share of settlement total from **Cash** component MTD.
- Payment pattern.
- Audience: Finance.
- Sumber: pelunasan breakdown.

### HOW

* Cash amount ÷ total settlement (Cash+Giro+Adjustment) × 100%.
- Current calendar month.
- Three components sum to 100%.

### WHY

* Liquidity vs giro reliance.
- Settlement behavior insight.

### WHEN

* Monthly FI02 Recovery Summary review.

<a id="fi-kpi-019"></a>
## FI-KPI-019 — Payment Mix — Giro

**Location**

- FI02 - Collection Dashboard (Recovery Summary)

---

### WHAT

Share of settlement total from **Giro** component MTD.
- Payment pattern.
- Audience: Finance.
- Sumber: pelunasan breakdown.

### HOW

* Giro amount ÷ total settlement (Cash+Giro+Adjustment) × 100%.
- Current calendar month.
- Three components sum to 100%.

### WHY

* Liquidity vs giro reliance.
- Settlement behavior insight.

### WHEN

* Monthly FI02 Recovery Summary review.

<a id="fi-kpi-020"></a>
## FI-KPI-020 — Payment Mix — Adjustment

**Location**

- FI02 - Collection Dashboard (Recovery Summary)

---

### WHAT

Share of settlement total from **Adjustment** component MTD.
- Payment pattern.
- Audience: Finance.
- Sumber: pelunasan breakdown.

### HOW

* Adjustment amount ÷ total settlement (Cash+Giro+Adjustment) × 100%.
- Current calendar month.
- Three components sum to 100%.

### WHY

* Liquidity vs giro reliance.
- Settlement behavior insight.

### WHEN

* Monthly FI02 Recovery Summary review.

<a id="fi-kpi-021"></a>
## FI-KPI-021 — Legacy Debt Count

**Location**

- FI02 - Collection Dashboard (Portfolio card)

---

### WHAT

Count customers: dormant (90-day rule) AND open balance > 1.
- Low-recovery receivable on inactive accounts.
- Audience: Collection.
- Sumber: M17 dormant + piutang.

### HOW

* Customer matches M17 dormant definition.
- AND total open balance > 1.
- LegacyDebt signal suppresses M17 Dormant in EX02.

### WHY

* Identify write-off / escalation candidates.
- Revenue attrition + stuck receivable.

### WHEN

* FI02 attention list → FI04; compare CU01 dormant.

<a id="fi-kpi-022"></a>
## FI-KPI-022 — Aging Risk Summary — 1–30 Days

**Location**

- FI02 - Collection Dashboard

---

### WHAT

Overdue-only aging bucket **1–30 Days** amount.
- Excludes Current bucket.
- Audience: Collection.
- Sumber: overdue subset aging.

### HOW

* SUM overdue balance in 1–30 Days.
- Current bucket excluded from this summary.
- Four buckets sum to Overdue Exposure.

### WHY

* Stage collection urgency.
- Complement FI01 full aging pie.

### WHEN

* FI02 daily aging risk review.

<a id="fi-kpi-023"></a>
## FI-KPI-023 — Aging Risk Summary — 31–60 Days

**Location**

- FI02 - Collection Dashboard

---

### WHAT

Overdue-only aging bucket **31–60 Days** amount.
- Excludes Current bucket.
- Audience: Collection.
- Sumber: overdue subset aging.

### HOW

* SUM overdue balance in 31–60 Days.
- Current bucket excluded from this summary.
- Four buckets sum to Overdue Exposure.

### WHY

* Stage collection urgency.
- Complement FI01 full aging pie.

### WHEN

* FI02 daily aging risk review.

<a id="fi-kpi-024"></a>
## FI-KPI-024 — Aging Risk Summary — 61–90 Days

**Location**

- FI02 - Collection Dashboard

---

### WHAT

Overdue-only aging bucket **61–90 Days** amount.
- Excludes Current bucket.
- Audience: Collection.
- Sumber: overdue subset aging.

### HOW

* SUM overdue balance in 61–90 Days.
- Current bucket excluded from this summary.
- Four buckets sum to Overdue Exposure.

### WHY

* Stage collection urgency.
- Complement FI01 full aging pie.

### WHEN

* FI02 daily aging risk review.

<a id="fi-kpi-025"></a>
## FI-KPI-025 — Aging Risk Summary — > 90 Days

**Location**

- FI02 - Collection Dashboard

---

### WHAT

Overdue-only aging bucket **> 90 Days** amount.
- Excludes Current bucket.
- Audience: Collection.
- Sumber: overdue subset aging.

### HOW

* SUM overdue balance in > 90 Days.
- Current bucket excluded from this summary.
- Four buckets sum to Overdue Exposure.

### WHY

* Stage collection urgency.
- Complement FI01 full aging pie.

### WHEN

* FI02 daily aging risk review.

<a id="fi-kpi-026"></a>
## FI-KPI-026 — Top Overdue Customers

**Location**

- FI02 - Collection Dashboard

---

### WHAT

Top 10 customers ranked by **overdue balance only** (not total balance).
- Collection priority queue.
- Audience: Collection.
- Sumber: overdue aggregation.

### HOW

* GROUP BY Customer — sort SUM overdue DESC, take 10.
- Wilayah rows display-only (no click drill V1).
- Customer/Salesman → FI04 ?q=.

### WHY

* Focus collection effort.
- Geographic/salesman accountability.

### WHEN

* Daily FI02 rankings → FI04.

<a id="fi-kpi-027"></a>
## FI-KPI-027 — Top Overdue Salesmen

**Location**

- FI02 - Collection Dashboard

---

### WHAT

Top 10 salesmen ranked by **overdue balance only** (not total balance).
- Collection priority queue.
- Audience: Collection.
- Sumber: overdue aggregation.

### HOW

* GROUP BY Salesme — sort SUM overdue DESC, take 10.
- Wilayah rows display-only (no click drill V1).
- Customer/Salesman → FI04 ?q=.

### WHY

* Focus collection effort.
- Geographic/salesman accountability.

### WHEN

* Daily FI02 rankings → FI04.

<a id="fi-kpi-028"></a>
## FI-KPI-028 — Top Overdue Wilayah

**Location**

- FI02 - Collection Dashboard

---

### WHAT

Top 10 wilayah ranked by **overdue balance only** (not total balance).
- Collection priority queue.
- Audience: Collection.
- Sumber: overdue aggregation.

### HOW

* GROUP BY Wilayah — sort SUM overdue DESC, take 10.
- Wilayah rows display-only (no click drill V1).
- Customer/Salesman → FI04 ?q=.

### WHY

* Focus collection effort.
- Geographic/salesman accountability.

### WHEN

* Daily FI02 rankings → FI04.

<a id="fi-kpi-029"></a>
## FI-KPI-029 — Expected Cash Collection

**Location**

- FI03 - Cash Flow Forecast

---

### WHAT

Cash Flow Forecast KPI — Expected Cash Collection.
- (Cash MTD ÷ DE) × DIM — projected month-end cash.
- Audience: Finance leadership.
- Sumber: Collection + Sales snapshot cross-read.

### HOW

* Linear calendar-day extrapolation of BayarTunai.
- DE=max(1,days elapsed); DIM=days in month.

### WHY

* Projected liquidity from cash collections.

### WHEN

* Mid-month FI03 daily.
- Traceability: FI03 footer links FI02, FI04.

<a id="fi-kpi-030"></a>
## FI-KPI-030 — Projected Month-End Collection

**Location**

- FI03 - Cash Flow Forecast

---

### WHAT

Cash Flow Forecast KPI — Projected Month-End Collection.
- Same as Expected Cash Collection — headline month-end cash projection.
- Audience: Finance leadership.
- Sumber: Collection + Sales snapshot cross-read.

### HOW

* Identical FI-KPI-029.
- When B=month end, equals actual MTD.

### WHY

* Month-end cash planning.

### WHEN

* FI03 Cash Position row.
- Traceability: FI03 footer links FI02, FI04.

<a id="fi-kpi-031"></a>
## FI-KPI-031 — Collection Forecast %

**Location**

- FI03 - Cash Flow Forecast

---

### WHAT

Cash Flow Forecast KPI — Collection Forecast %.
- Projected total collections ÷ month billing × 100%.
- Audience: Finance leadership.
- Sumber: Collection + Sales snapshot cross-read.

### HOW

* (Projected Month-End Total Collections ÷ Month Faktur Omzet) × 100%.
- Band §5.1 on forecast %.
- Null when billing=0.

### WHY

* Will collections keep pace with billing?

### WHEN

* FI03 vs FI-KPI-017 actual recovery.
- Traceability: FI03 footer links FI02, FI04.

<a id="fi-kpi-032"></a>
## FI-KPI-032 — Daily Cash Collection Average

**Location**

- FI03 - Cash Flow Forecast

---

### WHAT

Cash Flow Forecast KPI — Daily Cash Collection Average.
- Cash MTD ÷ DE.
- Audience: Finance leadership.
- Sumber: Collection + Sales snapshot cross-read.

### HOW

* Cash Collected MTD ÷ days elapsed.
- Reference line Daily Collection Pace chart.

### WHY

* Benchmark vs Required Daily.

### WHEN

* FI03 Pace row.
- Traceability: FI03 footer links FI02, FI04.

<a id="fi-kpi-033"></a>
## FI-KPI-033 — Required Daily Collection

**Location**

- FI03 - Cash Flow Forecast

---

### WHAT

Cash Flow Forecast KPI — Required Daily Collection.
- Daily cash needed on remaining days to match billing.
- Audience: Finance leadership.
- Sumber: Collection + Sales snapshot cross-read.

### HOW

* (Month Omzet − Month Collections) ÷ DR when omzet > collections.
- Warning >1.5× daily avg; Critical >2×.

### WHY

* Actionable collection pace target.

### WHEN

* Mid-month liquidity gap.
- Traceability: FI03 footer links FI02, FI04.

<a id="fi-kpi-034"></a>
## FI-KPI-034 — Remaining Collection Target

**Location**

- FI03 - Cash Flow Forecast

---

### WHAT

Cash Flow Forecast KPI — Remaining Collection Target.
- Billing MTD minus collections MTD when billing > collections.
- Audience: Finance leadership.
- Sumber: Collection + Sales snapshot cross-read.

### HOW

* BO − MC when BO > MC; else 0.
- BO = month Faktur omzet.

### WHY

* Gap to close before month-end.

### WHEN

* FI03 Pace & Target row.
- Traceability: FI03 footer links FI02, FI04.

<a id="fi-kpi-035"></a>
## FI-KPI-035 — Days Remaining

**Location**

- FI03 - Cash Flow Forecast

---

### WHAT

Cash Flow Forecast KPI — Days Remaining.
- Calendar days left in month.
- Audience: Finance leadership.
- Sumber: Collection + Sales snapshot cross-read.

### HOW

* (Month End − B).Days.

### WHY

* Time context for Required Daily.

### WHEN

* FI03 header context.
- Traceability: FI03 footer links FI02, FI04.

<a id="fi-kpi-036"></a>
## FI-KPI-036 — Recovery vs Billing Forecast

**Location**

- FI03 - Cash Flow Forecast

---

### WHAT

Cash Flow Forecast KPI — Recovery vs Billing Forecast.
- Projected recovery % at month-end (same as Collection Forecast %).
- Audience: Finance leadership.
- Sumber: Collection + Sales snapshot cross-read.

### HOW

* Same formula FI-KPI-031.
- Distinct label for forecast context row.

### WHY

* Forward-looking recovery health.

### WHEN

* Compare FI-KPI-017 actual.
- Traceability: FI03 footer links FI02, FI04.

<a id="fi-kpi-037"></a>
## FI-KPI-037 — Scenario Cash (Best / Expected / Worst)

**Location**

- FI03 - Cash Flow Forecast

---

### WHAT

Cash Flow Forecast KPI — Scenario Cash (Best / Expected / Worst).
- Range month-end cash projection.
- Audience: Finance leadership.
- Sumber: Collection + Sales snapshot cross-read.

### HOW

* Expected = FI-KPI-029.
- Best/Worst use MAX/MIN of MTD vs recent-7-day daily cash avg × DIM.

### WHY

* Uncertainty in liquidity forecast.

### WHEN

* FI03 Scenario row.
- Traceability: FI03 footer links FI02, FI04.

<a id="fi-kpi-038"></a>
## FI-KPI-038 — Forecast Confidence (Cash)

**Location**

- FI03 - Cash Flow Forecast

---

### WHAT

Trust label for cash forecast (§5.4 days elapsed).
- Audience: Finance leadership.
- Sumber: derived from calendar.

### HOW

* DE ≤5 Low; 6–20 Medium; ≥21 High.
- Day 1 CC=0: forecasts=0, Low confidence.

### WHY

* Interpret early-month cash forecast cautiously.
- Avoid over-reaction on day 1–5.

### WHEN

* FI03 Scenario & Confidence row.

<a id="fi-kpi-039"></a>
## FI-KPI-039 — Outstanding Due Remaining

**Location**

- FI03 - Cash Flow Forecast

---

### WHAT

Open balance with Jatuh Tempo still in current month (not yet due passed).

### HOW

* SUM KurangBayar where JatuhTempo > B AND JatuhTempo ≤ month end.
- KurangBayar > 1.

### WHY

* Near-term collectible exposure context.

### WHEN

* FI03 Receivable Context row → FI04.

<a id="fi-kpi-040"></a>
## FI-KPI-040 — Collection Gap

**Location**

- FI03 - Cash Flow Forecast

---

### WHAT

Month billing minus projected month-end collections.

### HOW

* Month Omzet − Projected Month-End Total Collections.
- Critical if >20% of billing; Warning if >0.

### WHY

* Projected liquidity shortfall.

### WHEN

* Mid-month before cash shortfall.

<a id="fi-kpi-041"></a>
## FI-KPI-041 — Top Collection Risks (Table)

**Location**

- FI03 - Cash Flow Forecast

---

### WHAT

Priority-ordered table (max 10) of deterministic collection forecast risks.

### HOW

* Rules: LargeDueSoon, ChronicOverdueLarge, OverdueConcentration, LegacyDebtOverdue, PlafondBreachDueSoon, LowRecoveryCustomer, WilayahHotspotDue, ExpectedOverdueGrowth.
- No AI scoring.
- Drill FI04 per row.

### WHY

* Proactive collection before due dates.
- Bridge forecast to action.

### WHEN

* FI03 daily → FI04 customer evidence.

<a id="fi-kpi-042"></a>
## FI-KPI-042 — Report Footer — Total Piutang

**Location**

- FI04 - Piutang Report

---

### WHAT

Sama dengan definisi bisnis **FI-KPI-001**.
- Ditampilkan pada menu di Location.
- Lihat entri FI-KPI-001 untuk detail lengkap.

### HOW

* Formula dan business rules identik dengan FI-KPI-001.
- Must reconcile same snapshot refresh.

### WHY

* Layer bukti tabular untuk **FI-KPI-001**.
- Reconciliation Report vs Dashboard.

### WHEN

* **FI04** summary bar; buka dari **FI01** drill-down dengan mode all-open balances.

<a id="fi-kpi-043"></a>
## FI-KPI-043 — Report Footer — Total Customer

**Location**

- FI04 - Piutang Report

---

### WHAT

Sama dengan definisi bisnis **FI-KPI-002**.
- Ditampilkan pada menu di Location.
- Lihat entri FI-KPI-002 untuk detail lengkap.

### HOW

* Formula dan business rules identik dengan FI-KPI-002.
- Must reconcile same snapshot refresh.

### WHY

* Layer bukti tabular untuk **FI-KPI-002**.
- QA traceability customer count.

### WHEN

* **FI04** footer; harus match **FI01** saat all-open tanpa filter.

### 6.5 Sales Force (SF-KPI)

<a id="sf-kpi-001"></a>
## SF-KPI-001 — Below Target Count

**Location**

- SF01 - Salesmen (Performance card)

---

### WHAT

Count Salesman with Target configured AND Achievement in Warning or Critical band.

### HOW

* Target > 0 AND Achievement % in 80–99% or <80%.
- Same bands §5.1.
- Below Target attention signal per rep.

### WHY

* Coaching and intervention queue.
- Performance accountability.

### WHEN

* Daily SF01 → SA03 per salesman.

<a id="sf-kpi-002"></a>
## SF-KPI-002 — Missing Target Setup Count

**Location**

- SF01 - Salesmen (Performance card)

---

### WHAT

Count active Salesman with month activity but no Target configured.

### HOW

* Month omzet > 0 OR customers invoiced > 0.
- AND Target = 0 or not configured.
- Missing Target Setup signal.

### WHY

* Planning completeness gap.
- Cannot judge performance without Target.

### WHEN

* Start of month SF01 → Desktop Target setup.

<a id="sf-kpi-003"></a>
## SF-KPI-003 — High Overdue Exposure Count

**Location**

- SF01 - Salesmen (Collection Exposure card)
- EX02 - Alert Center (dedup with M20)

---

### WHAT

Count Salesman in top 20% by overdue among reps with overdue > 0.

### HOW

* Rank reps by SUM overdue balance.
- Top 20% percentile (configurable).
- Suppressed in EX02 when M20 HighOverdueWorkload applies.

### WHY

* Collection accountability by rep.
- Salesman owns invoiced book overdue.

### WHEN

* SF01 → FI04 salesman filter.

<a id="sf-kpi-004"></a>
## SF-KPI-004 — High Piutang Exposure Count

**Location**

- SF01 - Salesmen (Collection Exposure card)

---

### WHAT

Count Salesman in top 20% by open balance among reps with balance > 0.

### HOW

* Rank by total open KurangBayar attributed to invoicing salesman.
- Top 20% threshold.

### WHY

* Receivable risk concentration by rep.
- Working capital ownership.

### WHEN

* SF01 exposure rankings → FI04.

<a id="sf-kpi-005"></a>
## SF-KPI-005 — Dormant Portfolio Count

**Location**

- SF01 - Salesmen (Portfolio card)

---

### WHAT

Count Salesman with ≥1 dormant customer (90-day) on their book.

### HOW

* Dormant via last invoicing salesman attribution.
- Same 90-day M17 rule.

### WHY

* Account maintenance gap by rep.
- Revenue recovery opportunity.

### WHEN

* SF01 → CU01 dormant list → SA03.

<a id="sf-kpi-006"></a>
## SF-KPI-006 — Top Omzet Salesman %

**Location**

- SF01 - Salesmen (Portfolio card)

---

### WHAT

Share company MTD omzet from top-1 Salesman.
- Informational concentration.

### HOW

* Top rep omzet ÷ company MTD omzet × 100%.
- No auto threshold.

### WHY

* Dependency on single rep for revenue.

### WHEN

* SF01 vs SA-KPI-007 Top 10.

<a id="sf-kpi-007"></a>
## SF-KPI-007 — Top Piutang Salesman %

**Location**

- SF01 - Salesmen (Portfolio card)

---

### WHAT

Share Total Piutang from top-1 Salesman book.

### HOW

* Top rep open balance ÷ Total Piutang × 100%.
- Informational.

### WHY

* Receivable concentration by rep.

### WHEN

* SF01 Top 10 Piutang ranking.

<a id="sf-kpi-008"></a>
## SF-KPI-008 — Top 10 Omzet (Ranking)

**Location**

- SF01 - Salesmen

---

### WHAT

Top 10 Salesman ranked by Omzet MTD or all-time open.

### HOW

* Sort by Omzet DESC, max 10 rows.
- Achievement % uses §5.1 bands in table.
- Drill SA03 or FI04 ?q= salesman.

### WHY

* Performance leaderboards.
- Coaching priorities.

### WHEN

* Daily SF01 → Report evidence.

<a id="sf-kpi-009"></a>
## SF-KPI-009 — Top 10 Achievement % (Ranking)

**Location**

- SF01 - Salesmen

---

### WHAT

Top 10 Salesman ranked by Achievement % MTD or all-time open.

### HOW

* Sort by Achievement % DESC, max 10 rows.
- Achievement % uses §5.1 bands in table.
- Drill SA03 or FI04 ?q= salesman.

### WHY

* Performance leaderboards.
- Coaching priorities.

### WHEN

* Daily SF01 → Report evidence.

<a id="sf-kpi-010"></a>
## SF-KPI-010 — Top 10 Piutang (Ranking)

**Location**

- SF01 - Salesmen

---

### WHAT

Top 10 Salesman ranked by Piutang MTD or all-time open.

### HOW

* Sort by Piutang DESC, max 10 rows.
- Achievement % uses §5.1 bands in table.
- Drill SA03 or FI04 ?q= salesman.

### WHY

* Performance leaderboards.
- Coaching priorities.

### WHEN

* Daily SF01 → Report evidence.

<a id="sf-kpi-011"></a>
## SF-KPI-011 — Principal Achievement Table

**Location**

- SF01 - Salesmen (detail drawer)

---

### WHAT

Per-supplier Target vs Actual omzet per Salesman in drill-down drawer.

### HOW

* Breakdown by Principal on rep's MTD Faktur.
- Target per principal if configured.
- Drawer from ranking/attention row click.

### WHY

* Principal-level coaching.
- Supplier push effectiveness.

### WHEN

* SF01 row click → drawer → SA03 filter principal.

<a id="sf-kpi-012"></a>
## SF-KPI-012 — Planned Visits

**Location**

- SF02 - Field Activity

---

### WHAT

Customers on effective visit plan for salesman-day.
- Field execution metric.
- Audience: Field supervisors.
- **Live query** — not snapshot.

### HOW

* COUNT planned customers from BTR_VisitPlan + exceptions.
- Requires salesman + date selection.
- GPS validation bands: Valid ≤50m, Warning 50–100m, Suspicious >100m.

### WHY

* Expected coverage denominator.
- Complements SF01 outcome lens.

### WHEN

* Select salesman-day SF02 first.
- Not in EX02 Alert Center V1.

<a id="sf-kpi-013"></a>
## SF-KPI-013 — Actual Visits

**Location**

- SF02 - Field Activity

---

### WHAT

Distinct customers checked in.
- Field execution metric.
- Audience: Field supervisors.
- **Live query** — not snapshot.

### HOW

* COUNT DISTINCT CustomerId check-in; dedupe multiple same-day.
- Requires salesman + date selection.
- GPS validation bands: Valid ≤50m, Warning 50–100m, Suspicious >100m.

### WHY

* Route execution numerator.
- Complements SF01 outcome lens.

### WHEN

* Daily SF02 supervisor review.
- Not in EX02 Alert Center V1.

<a id="sf-kpi-014"></a>
## SF-KPI-014 — Missed Visits

**Location**

- SF02 - Field Activity

---

### WHAT

Planned minus actual.
- Field execution metric.
- Audience: Field supervisors.
- **Live query** — not snapshot.

### HOW

* Planned − Actual.
- Requires salesman + date selection.
- GPS validation bands: Valid ≤50m, Warning 50–100m, Suspicious >100m.

### WHY

* Coverage gaps.
- Complements SF01 outcome lens.

### WHEN

* SF02 missed list → customer follow-up.
- Not in EX02 Alert Center V1.

<a id="sf-kpi-015"></a>
## SF-KPI-015 — Unplanned Visits

**Location**

- SF02 - Field Activity

---

### WHAT

Check-ins not on plan.
- Field execution metric.
- Audience: Field supervisors.
- **Live query** — not snapshot.

### HOW

* Actual visits NOT on effective plan.
- Requires salesman + date selection.
- GPS validation bands: Valid ≤50m, Warning 50–100m, Suspicious >100m.

### WHY

* Ad-hoc selling or plan deviation.
- Complements SF01 outcome lens.

### WHEN

* SF02 map unplanned pins.
- Not in EX02 Alert Center V1.

<a id="sf-kpi-016"></a>
## SF-KPI-016 — Effective Calls

**Location**

- SF02 - Field Activity

---

### WHAT

Visits producing ≥1 order same day.
- Field execution metric.
- Audience: Field supervisors.
- **Live query** — not snapshot.

### HOW

* Check-in + BTR_Order same date/customer/UserEmail.
- Requires salesman + date selection.
- GPS validation bands: Valid ≤50m, Warning 50–100m, Suspicious >100m.

### WHY

* Productive visit count.
- Complements SF01 outcome lens.

### WHEN

* Distinguish activity vs productivity.
- Not in EX02 Alert Center V1.

<a id="sf-kpi-017"></a>
## SF-KPI-017 — Visit Execution %

**Location**

- SF02 - Field Activity

---

### WHAT

Actual ÷ Planned.
- Field execution metric.
- Audience: Field supervisors.
- **Live query** — not snapshot.

### HOW

* Actual/Planned × 100%; N/A if Planned=0.
- Team level: SUM(actual)÷SUM(planned).
- Requires salesman + date selection.
- GPS validation bands: Valid ≤50m, Warning 50–100m, Suspicious >100m.

### WHY

* Route compliance KPI.
- Complements SF01 outcome lens.

### WHEN

* Daily field supervision.
- Not in EX02 Alert Center V1.

<a id="sf-kpi-018"></a>
## SF-KPI-018 — Effective Call Rate

**Location**

- SF02 - Field Activity

---

### WHAT

Effective ÷ Actual.
- Field execution metric.
- Audience: Field supervisors.
- **Live query** — not snapshot.

### HOW

* Effective Calls ÷ Actual Visits × 100%.
- Requires salesman + date selection.
- GPS validation bands: Valid ≤50m, Warning 50–100m, Suspicious >100m.

### WHY

* Visit productivity.
- Complements SF01 outcome lens.

### WHEN

* Compare with SF01 omzet outcome.
- Not in EX02 Alert Center V1.

### 6.6 Inventory (IN-KPI)

<a id="in-kpi-001"></a>
## IN-KPI-001 — Total Inventory Value

**Location**

- IN01 - Inventory Dashboard
- IN05 - Inventory Report (footer)
- EX01 - Executive

---

### WHAT

Total stock value HPP × Qty point-in-time.
- Same as EX-KPI-010.

### HOW

* BrgId-first aggregation.
- Exclude In-Transit; Qty>0.
- Must match IN05 footer.

### WHY

* Working capital in inventory.

### WHEN

* IN01 daily; verify IN05.

<a id="in-kpi-002"></a>
## IN-KPI-002 — Total Item

**Location**

- IN01 - Inventory Dashboard
- IN05 - Inventory Report (footer)

---

### WHAT

Distinct products with stock on hand.

### HOW

* COUNT distinct BrgId with net Qty > 0.
- Same exclusion rules IN-KPI-001.

### WHY

* SKU breadth.

### WHEN

* IN01 KPI row.

<a id="in-kpi-003"></a>
## IN-KPI-003 — Top 10 Category (Ranking)

**Location**

- IN01 - Inventory Dashboard

---

### WHAT

Top 10 category by inventory value with chart.

### HOW

* Sort Category value DESC, take 10.
- Unknown label if blank.
- Chart matches table Top 10.

### WHY

* Capital concentration by category.

### WHEN

* IN01 → IN05 search category.

<a id="in-kpi-004"></a>
## IN-KPI-004 — Top 10 Supplier (Ranking)

**Location**

- IN01 - Inventory Dashboard

---

### WHAT

Top 10 supplier by inventory value with chart.

### HOW

* Sort Supplier value DESC, take 10.
- Unknown label if blank.
- Chart matches table Top 10.

### WHY

* Capital concentration by supplier.

### WHEN

* IN01 → IN05 search supplier.

<a id="in-kpi-005"></a>
## IN-KPI-005 — Dead Stock Count & Value

**Location**

- IN02 - Inventory Risk
- EX02 - Inventory Risk Summary

---

### WHAT

Items idle ≥180 days since last Faktur — count and total value.

### HOW

* Last Faktur ≥180 days ago; stock > 0.
- Mutually exclusive classification.
- Gross Faktur history only.

### WHY

* Write-off / clearance candidates.

### WHEN

* IN02 attention card → IN05 item.

<a id="in-kpi-006"></a>
## IN-KPI-006 — Slow Moving Count & Value

**Location**

- IN02 - Inventory Risk
- EX02 - Summary

---

### WHAT

Items idle 90–179 days since last Faktur.

### HOW

* Last Faktur 90–179 days.
- At most one signal per item.

### WHY

* Promotion / replenishment review.

### WHEN

* IN02 aging pie Slow Moving slice.

<a id="in-kpi-007"></a>
## IN-KPI-007 — Never Sold Count & Value

**Location**

- IN02 - Inventory Risk
- EX02 - Summary

---

### WHAT

Stock with no Faktur sales history ever.

### HOW

* Qty > 0 AND no non-void Faktur line history.
- Distinct from Dead Stock.

### WHY

* Bad intake or demand failure.

### WHEN

* IN02 attention list Never Sold signal.

<a id="in-kpi-008"></a>
## IN-KPI-008 — At-Risk Inventory %

**Location**

- IN02 - Inventory Risk
- EX02 - Concentrations
- IN03 - Inventory Forecast

---

### WHAT

At-risk value ÷ Total Inventory Value × 100%.

### HOW

* At-risk = Never Sold ∪ Slow ∪ Dead (exclusive sets).
- Reused in IN03 context.

### WHY

* Share of capital needing attention.

### WHEN

* IN02 card → IN04 optimization.

<a id="in-kpi-009"></a>
## IN-KPI-009 — Aging Distribution (Movement Classes)

**Location**

- IN02 - Inventory Risk

---

### WHAT

Inventory value split Active / Slow / Dead / Never Sold pie.

### HOW

* Active: last Faktur within 89 days.
- Sum slices = Total Inventory Value.

### WHY

* Portfolio health overview.

### WHEN

* IN02 vs IN01 composition.

<a id="in-kpi-010"></a>
## IN-KPI-010 — Category Risk Exposure

**Location**

- IN02 - Inventory Risk

---

### WHAT

At-risk inventory value by category Top 10 bar chart.

### HOW

* SUM at-risk value GROUP BY Category; Top 10 bars.

### WHY

* Where obsolescence clusters.

### WHEN

* IN02 → IN05 → PU01 cross-risk.

<a id="in-kpi-011"></a>
## IN-KPI-011 — Supplier Risk Exposure

**Location**

- IN02 - Inventory Risk

---

### WHAT

At-risk inventory value by supplier Top 10 bar chart.

### HOW

* SUM at-risk value GROUP BY Supplier; Top 10 bars.

### WHY

* Where obsolescence clusters.

### WHEN

* IN02 → IN05 → PU01 cross-risk.

<a id="in-kpi-012"></a>
## IN-KPI-012 — Top 10 Dead / Slow Moving (Ranking)

**Location**

- IN02 - Inventory Risk

---

### WHAT

Highest-value dead stock and slow moving items (Top 10 each).

### HOW

* Sort by Nilai Sediaan DESC within class.
- Drill IN05 ?q= item name.

### WHY

* Item-level action list.

### WHEN

* IN02 table → IN05 evidence.

<a id="in-kpi-013"></a>
## IN-KPI-013 — Projected Inventory Value @ Horizon

**Location**

- IN03 - Inventory Forecast

---

### WHAT

(Current value depletion model at H=30 days).
- Forward 30-day horizon.
- Audience: Inventory, Purchasing.
- Consumption = 30-day Faktur qty gross.

### HOW

* SUM MAX(0, Q−ADC×H)×Hpp.
- Exclude M19 Dead/Never Sold from forecast-eligible.
- Exclude In-Transit.

### WHY

* Replenishment planning before stock-out.
- Working capital projection.

### WHEN

* IN03 → IN04 actions → PU01 posting backlog check.

<a id="in-kpi-014"></a>
## IN-KPI-014 — Average Days of Supply (Company)

**Location**

- IN03 - Inventory Forecast

---

### WHAT

Company-level DOS.
- Forward 30-day horizon.
- Audience: Inventory, Purchasing.
- Consumption = 30-day Faktur qty gross.

### HOW

* SUM(Q)÷SUM(ADC) when ADC>0.
- Exclude M19 Dead/Never Sold from forecast-eligible.
- Exclude In-Transit.

### WHY

* Replenishment planning before stock-out.
- Working capital projection.

### WHEN

* IN03 → IN04 actions → PU01 posting backlog check.

<a id="in-kpi-015"></a>
## IN-KPI-015 — Inventory Health Score

**Location**

- IN03 - Inventory Forecast

---

### WHAT

Composite score 0–100.
- Forward 30-day horizon.
- Audience: Inventory, Purchasing.
- Consumption = 30-day Faktur qty gross.

### HOW

* 100 − weighted stock-out/overstock/at-risk penalties; floor 0.
- Exclude M19 Dead/Never Sold from forecast-eligible.
- Exclude In-Transit.

### WHY

* Replenishment planning before stock-out.
- Working capital projection.

### WHEN

* IN03 → IN04 actions → PU01 posting backlog check.

<a id="in-kpi-016"></a>
## IN-KPI-016 — Stock-Out Risk Items / Value

**Location**

- IN03 - Inventory Forecast

---

### WHAT

SKUs projected out within H.
- Forward 30-day horizon.
- Audience: Inventory, Purchasing.
- Consumption = 30-day Faktur qty gross.

### HOW

* COUNT/SUM where DOS≤H and ADC>0.
- Exclude M19 Dead/Never Sold from forecast-eligible.
- Exclude In-Transit.

### WHY

* Replenishment planning before stock-out.
- Working capital projection.

### WHEN

* IN03 → IN04 actions → PU01 posting backlog check.

<a id="in-kpi-017"></a>
## IN-KPI-017 — Overstock / Understock Value

**Location**

- IN03 - Inventory Forecast

---

### WHAT

Overstock DOS>90 default.
- Forward 30-day horizon.
- Audience: Inventory, Purchasing.
- Consumption = 30-day Faktur qty gross.

### HOW

* SUM value where DOS>OverstockThreshold.
- Exclude M19 Dead/Never Sold from forecast-eligible.
- Exclude In-Transit.

### WHY

* Replenishment planning before stock-out.
- Working capital projection.

### WHEN

* IN03 → IN04 actions → PU01 posting backlog check.

<a id="in-kpi-018"></a>
## IN-KPI-018 — Scenario Projected Value (Best/Expected/Worst)

**Location**

- IN03 - Inventory Forecast

---

### WHAT

ADC sensitivity bands.
- Forward 30-day horizon.
- Audience: Inventory, Purchasing.
- Consumption = 30-day Faktur qty gross.

### HOW

* Expected primary; Best/Worst MIN/MAX ADC30 vs ADC90.
- Exclude M19 Dead/Never Sold from forecast-eligible.
- Exclude In-Transit.

### WHY

* Replenishment planning before stock-out.
- Working capital projection.

### WHEN

* IN03 → IN04 actions → PU01 posting backlog check.

<a id="in-kpi-019"></a>
## IN-KPI-019 — Forecast Confidence (Inventory)

**Location**

- IN03 - Inventory Forecast

---

### WHAT

Low if <30 days consumption data.
- Forward 30-day horizon.
- Audience: Inventory, Purchasing.
- Consumption = 30-day Faktur qty gross.

### HOW

* Company consumption history depth.
- Exclude M19 Dead/Never Sold from forecast-eligible.
- Exclude In-Transit.

### WHY

* Replenishment planning before stock-out.
- Working capital projection.

### WHEN

* IN03 → IN04 actions → PU01 posting backlog check.

<a id="in-kpi-020"></a>
## IN-KPI-020 — Days of Supply (Item)

**Location**

- IN03 - Inventory Forecast

---

### WHAT

Per-SKU DOS.
- Forward 30-day horizon.
- Audience: Inventory, Purchasing.
- Consumption = 30-day Faktur qty gross.

### HOW

* Q÷ADC when ADC>0.
- Exclude M19 Dead/Never Sold from forecast-eligible.
- Exclude In-Transit.

### WHY

* Replenishment planning before stock-out.
- Working capital projection.

### WHEN

* IN03 → IN04 actions → PU01 posting backlog check.

<a id="in-kpi-021"></a>
## IN-KPI-021 — Recommended Purchase Qty (Indicative)

**Location**

- IN03 - Inventory Forecast

---

### WHAT

Decision support only.
- Forward 30-day horizon.
- Audience: Inventory, Purchasing.
- Consumption = 30-day Faktur qty gross.

### HOW

* MAX(0, CEILING(ADC×(LT+CD)−Q)); not approved PO.
- Exclude M19 Dead/Never Sold from forecast-eligible.
- Exclude In-Transit.

### WHY

* Replenishment planning before stock-out.
- Working capital projection.

### WHEN

* IN03 → IN04 actions → PU01 posting backlog check.

<a id="in-kpi-022"></a>
## IN-KPI-022 — Critical Actions Count

**Location**

- IN04 - Inventory Optimization

---

### WHAT

Recommendations category Critical.
- Consumes IN03 forecast + IN02 risk.
- Ranked action recommendations.
- Read-only decision support.

### HOW

* PriorityScore = category weight + value + lead time + strategic boost.
- Health Score copied from IN03 — not recomputed.
- Categories: Critical/High/Medium/Low.

### WHY

* Daily prioritized inventory actions.
- Purchase, delay, transfer, clearance decisions.

### WHEN

* IN04 queue → IN05/PU02 evidence → Desktop.

<a id="in-kpi-023"></a>
## IN-KPI-023 — Recommended Purchase Budget

**Location**

- IN04 - Inventory Optimization

---

### WHAT

Estimasi nilai pembelian yang direkomendasikan untuk kategori **Critical** dan **High** priority.
- Budget indikatif — bukan PO yang disetujui.
- Audience: Inventory, Purchasing management.
- Sumber: IN04 optimization aggregator.

### HOW

* SUM(RecommendedQty × HPP) untuk rekomendasi kategori Critical dan High.
- Qty dan HPP per item dari snapshot inventory.
- Tidak termasuk rekomendasi Delay/Transfer/Clearance.

### WHY

* Perkiraan cash outflow untuk replenishment priority.
- Mendukung approval budgeting harian.

### WHEN

* Daily **IN04** → validasi **PU02** / **IN05** → Desktop purchasing.

<a id="in-kpi-024"></a>
## IN-KPI-024 — Recoverable Capital

**Location**

- IN04 - Inventory Optimization

---

### WHAT

Estimasi modal yang dapat dipulihkan melalui **clearance** dead stock dan slow moving bernilai tinggi.
- Capital recovery opportunity metric.
- Audience: Inventory management, Owner.
- Sumber: IN02 dead/slow rankings + IN04 clearance recommendations.

### HOW

* M19 Dead Stock Value + subset slow moving high-value (Top 10 clearance candidate set).
- Nilai HPP × Qty untuk item dalam clearance review queue.
- Indikatif — bukan write-off otomatis.

### WHY

* Quantify working capital trapped in obsolescence.
- Prioritize clearance vs new purchase spend.

### WHEN

* **IN04** Clearance Review queue → **IN05** item detail → Desktop retur/promosi.

<a id="in-kpi-025"></a>
## IN-KPI-025 — Action Counts by Type (Purchase / Delay / Transfer / Clearance)

**Location**

- IN04 - Inventory Optimization

---

### WHAT

Jumlah rekomendasi tindakan per tipe: **Purchase Now**, **Delay**, **Transfer**, **Clearance Review**.
- Workload distribution by action type.
- Audience: Inventory, Purchasing, Warehouse.
- Sumber: IN04 recommendation categorizer.

### HOW

* COUNT rekomendasi per ActionType key.
- Purchase Now = immediate replenishment; Delay = defer purchase; Transfer = inter-warehouse; Clearance = obsolescence review.
- PriorityScore menentukan urutan queue — skor internal sorting, bukan KPI headline terpisah.

### WHY

* Balance intake vs deferral vs clearance effort.
- Operational planning for warehouse and purchasing teams.

### WHEN

* Daily **IN04** action segments → **IN03** forecast context → **PU01** posting backlog.

<a id="in-kpi-026"></a>
## IN-KPI-026 — Report Footer — Total Inventory Value

**Location**

- IN05 - Inventory Report

---

### WHAT

Total nilai inventory di **summary bar** Inventory Report — bukti tabular untuk **IN-KPI-001**.
- Footer aggregate BrgId-first.
- Audience: Inventory admin, QA traceability.
- Sumber: same snapshot as IN01.

### HOW

* Identical formula to **IN-KPI-001**.
- Footer groups by item first — sum visible row values may differ.
- Must match IN01 when search filter empty.

### WHY

* Report-dashboard reconciliation.
- Evidence validation for inventory KPIs.

### WHEN

* **IN05** open from **IN01**/**IN02** drill-down; compare footer to dashboard KPI.

<a id="in-kpi-027"></a>
## IN-KPI-027 — Report Footer — Total Item

**Location**

- IN05 - Inventory Report

---

### WHAT

Count distinct item di footer Inventory Report.
- Companion to IN-KPI-026 footer.
- Audience: Inventory admin.
- Sumber: inventory snapshot.

### HOW

* Identical to **IN-KPI-002**.
- Distinct BrgId with Qty > 0 after aggregation rules.

### WHY

* SKU count evidence layer.
- QA traceability.

### WHEN

* **IN05** footer check vs **IN01** Total Item KPI.

### 6.7 Purchasing (PU-KPI)

<a id="pu-kpi-001"></a>
## PU-KPI-001 — Grand Total Purchase

**Location**

- PU01 - Purchasing Management
- PU02 - Purchasing Report (footer)
- EX01 - Executive

---

### WHAT

Total purchase invoice value MTD.

### HOW

* SUM GrandTotal purchase invoice current month.
- Void excluded.
- Must match PU02 footer.

### WHY

* Cash outflow monitoring.

### WHEN

* Monthly PU01 → PU02.

<a id="pu-kpi-002"></a>
## PU-KPI-002 — Total Invoice

**Location**

- PU01 - Purchasing Management
- PU02 - Purchasing Report (footer)

---

### WHAT

Count purchase invoices MTD.

### HOW

* COUNT non-void invoices current month.

### WHY

* Purchasing activity volume.

### WHEN

* PU01 summary row.

<a id="pu-kpi-003"></a>
## PU-KPI-003 — Posted %

**Location**

- PU01 - Purchasing Management

---

### WHAT

Share of purchase value already posted to stock (SUDAH).

### HOW

* Posted value ÷ Grand Total Purchase × 100%.
- BELUM = pending posting.

### WHY

* Posting completion health.

### WHEN

* PU01 posting breakdown chart.

<a id="pu-kpi-004"></a>
## PU-KPI-004 — Pending Posting Value

**Location**

- PU01 - Purchasing Management (Posting Exposure card)

---

### WHAT

All BELUM invoice value (including fresh staging).

### HOW

* SUM GrandTotal where Posting Stok = BELUM.
- Context for Qualified Backlog.

### WHY

* Total unposted exposure.

### WHEN

* PU02 filter BELUM.

<a id="pu-kpi-005"></a>
## PU-KPI-005 — Qualified Backlog Count & Value

**Location**

- PU01 - Purchasing Management
- EX01 - Executive (RequiresAttention)

---

### WHAT

BELUM invoices aged ≥3 calendar days since LastUpdate.

### HOW

* Age threshold configurable (default 3 days).
- Executive attention when Count > 0.
- Fresh BELUM excluded from attention.

### WHY

* Actionable posting delay.
- Warehouse intake bottleneck.

### WHEN

* PU01 attention list → PU02 posting=BELUM → Desktop PT2.

<a id="pu-kpi-006"></a>
## PU-KPI-006 — Top 1 Principal %

**Location**

- PU01 - Purchasing Management

---

### WHAT

Spend concentration on top principal(s).

### HOW

* Top principal(s) MTD spend ÷ Grand Total Purchase × 100%.

### WHY

* Supplier dependency risk.

### WHEN

* PU01 Top 10 Principal table.

<a id="pu-kpi-007"></a>
## PU-KPI-007 — Top 3 Principal %

**Location**

- PU01 - Purchasing Management

---

### WHAT

Spend concentration on top principal(s).

### HOW

* Top principal(s) MTD spend ÷ Grand Total Purchase × 100%.

### WHY

* Supplier dependency risk.

### WHEN

* PU01 Top 10 Principal table.

<a id="pu-kpi-008"></a>
## PU-KPI-008 — Compound Dependency Count

**Location**

- PU01 - Purchasing Management

---

### WHAT

Principals in Top 10 MTD purchase AND (Top 10 inventory OR Top 10 at-risk).

### HOW

* Cross-read M15 inventory + M19 at-risk rankings.
- Compound Dependency signal.

### WHY

* Multi-dimensional supplier risk.

### WHEN

* PU01 → IN01/IN02 cross-exposure.

<a id="pu-kpi-009"></a>
## PU-KPI-009 — Purchasing Inactivity Flag

**Location**

- PU01 - Purchasing Management (Pace card)

---

### WHAT

Zero purchase invoices MTD AND today ≥ day 15.

### HOW

* TotalInvoice=0 AND Day≥15.
- Boolean indicator.

### WHY

* Replenishment gap mid-month.

### WHEN

* PU01 after mid-month.

<a id="pu-kpi-010"></a>
## PU-KPI-010 — Principal At-Risk Count

**Location**

- PU01 - Purchasing Management (Inventory Cross-Risk card)

---

### WHAT

Count principals with M19 at-risk exposure in cross-read.

### HOW

* Principals appearing in at-risk supplier ranking.
- Principal At-Risk Exposure signal.

### WHY

* Supplier linked to obsolescence.

### WHEN

* PU01 → IN02 supplier exposure.

<a id="pu-kpi-011"></a>
## PU-KPI-011 — Top 10 Principal (Ranking)

**Location**

- PU01 - Purchasing Management

---

### WHAT

Top 10 suppliers by MTD spend with % of purchase.

### HOW

* Sort GrandTotal DESC, take 10.
- Drill PU02 ?q= principal.

### WHY

* Supplier spend priorities.

### WHEN

* PU01 weekly.

<a id="pu-kpi-012"></a>
## PU-KPI-012 — Principal Exposure Comparison

**Location**

- PU01 - Purchasing Management

---

### WHAT

Table: MTD purchase · inventory value · at-risk value per principal.

### HOW

* Cross-domain read M15 + M19 + MTD purchase.
- One row per ranked principal.

### WHY

* Holistic supplier risk view.

### WHEN

* PU01 cross-risk review.

<a id="pu-kpi-013"></a>
## PU-KPI-013 — Report Footer — Grand Total Purchase

**Location**

- PU02 - Purchasing Report

---

### WHAT

Sama dengan definisi bisnis **PU-KPI-001** — total spend pembelian di footer Purchasing Report.
- Bukti tabular untuk KPI dashboard **PU01**.
- Audience: Purchasing admin, QA.

### HOW

* Identik **PU-KPI-001** untuk periode Report yang sama.
- Footer summary bar **PU02**.

### WHY

* Traceability Report vs Purchasing Dashboard.
- Validasi angka sebelum action posting Desktop.

### WHEN

* **PU02** periode bulan ini; bandingkan footer dengan **PU01** tanpa search filter.

<a id="pu-kpi-014"></a>
## PU-KPI-014 — Report Footer — Total Invoice

**Location**

- PU02 - Purchasing Report

---

### WHAT

Sama dengan definisi bisnis **PU-KPI-002** — count invoice pembelian di footer Report.
- Companion footer untuk **PU-KPI-013**.

### HOW

* Identik **PU-KPI-002** same period.
- COUNT non-void invoice MTD.

### WHY

* Activity volume evidence.
- QA reconciliation.

### WHEN

* **PU02** footer vs **PU01** Total Invoice KPI.

### 6.8 Operations (OP-KPI)

<a id="op-kpi-001"></a>
## OP-KPI-001 — Top 1 Warehouse Inventory %

**Location**

- OP01 - Locations

---

### WHAT

Top 1 Warehouse Inventory % — location concentration KPI.
- Informational cards — no auto threshold.
- Audience: Operations, Owner.

### HOW

* Rank-1 warehouse inventory ÷ total × 100.
- Ranking universe: IsAktif=true, IsSpecial=false, name≠In-Transit.
- Wilayah from Customer on Faktur.

### WHY

* Network dependency and capital trap detection.
- Site-level risk.

### WHEN

* OP01 weekly → IN05 warehouse filter or FI02 wilayah (overdue owned by FI02).

<a id="op-kpi-002"></a>
## OP-KPI-002 — Top 3 Warehouse Inventory %

**Location**

- OP01 - Locations

---

### WHAT

Top 3 Warehouse Inventory % — location concentration KPI.
- Informational cards — no auto threshold.
- Audience: Operations, Owner.

### HOW

* Sum rank 1–3 ÷ total × 100.
- Ranking universe: IsAktif=true, IsSpecial=false, name≠In-Transit.
- Wilayah from Customer on Faktur.

### WHY

* Network dependency and capital trap detection.
- Site-level risk.

### WHEN

* OP01 weekly → IN05 warehouse filter or FI02 wilayah (overdue owned by FI02).

<a id="op-kpi-003"></a>
## OP-KPI-003 — Top 1 Warehouse At-Risk %

**Location**

- OP01 - Locations

---

### WHAT

Top 1 Warehouse At-Risk % — location concentration KPI.
- Informational cards — no auto threshold.
- Audience: Operations, Owner.

### HOW

* Rank-1 at-risk warehouse ÷ M19 at-risk total × 100.
- Ranking universe: IsAktif=true, IsSpecial=false, name≠In-Transit.
- Wilayah from Customer on Faktur.

### WHY

* Network dependency and capital trap detection.
- Site-level risk.

### WHEN

* OP01 weekly → IN05 warehouse filter or FI02 wilayah (overdue owned by FI02).

<a id="op-kpi-004"></a>
## OP-KPI-004 — Top 1 Warehouse Sales %

**Location**

- OP01 - Locations

---

### WHAT

Top 1 Warehouse Sales % — location concentration KPI.
- Informational cards — no auto threshold.
- Audience: Operations, Owner.

### HOW

* Rank-1 warehouse MTD omzet ÷ Sales total × 100.
- Ranking universe: IsAktif=true, IsSpecial=false, name≠In-Transit.
- Wilayah from Customer on Faktur.

### WHY

* Network dependency and capital trap detection.
- Site-level risk.

### WHEN

* OP01 weekly → IN05 warehouse filter or FI02 wilayah (overdue owned by FI02).

<a id="op-kpi-005"></a>
## OP-KPI-005 — Top 1 Wilayah Sales %

**Location**

- OP01 - Locations

---

### WHAT

Top 1 Wilayah Sales % — location concentration KPI.
- Informational cards — no auto threshold.
- Audience: Operations, Owner.

### HOW

* Rank-1 wilayah MTD omzet ÷ Sales total × 100.
- Ranking universe: IsAktif=true, IsSpecial=false, name≠In-Transit.
- Wilayah from Customer on Faktur.

### WHY

* Network dependency and capital trap detection.
- Site-level risk.

### WHEN

* OP01 weekly → IN05 warehouse filter or FI02 wilayah (overdue owned by FI02).

<a id="op-kpi-006"></a>
## OP-KPI-006 — Inactive Warehouse With Stock Count

**Location**

- OP01 - Locations

---

### WHAT

Inactive Warehouse With Stock Count — location concentration KPI.
- Informational cards — no auto threshold.
- Audience: Operations, Owner.

### HOW

* IsAktif=false AND inventory>0, exclude In-Transit.
- Ranking universe: IsAktif=true, IsSpecial=false, name≠In-Transit.
- Wilayah from Customer on Faktur.

### WHY

* Network dependency and capital trap detection.
- Site-level risk.

### WHEN

* OP01 weekly → IN05 warehouse filter or FI02 wilayah (overdue owned by FI02).

<a id="op-kpi-007"></a>
## OP-KPI-007 — Top Warehouse by Inventory (Ranking)

**Location**

- OP01 - Locations

---

### WHAT

Top 10 warehouses by inventory with % company total.

### HOW

* Sort warehouse Inventory metric DESC; Top 10.
- Attention signals for Top-10 rank inclusion.
- Drill IN05 ?q= warehouse (inventory); IN02 (at-risk).

### WHY

* Multi-site performance comparison.

### WHEN

* OP01 attention list warehouse signals.

<a id="op-kpi-008"></a>
## OP-KPI-008 — Top Warehouse by At-Risk (Ranking)

**Location**

- OP01 - Locations

---

### WHAT

Top 10 warehouses by at-risk with % company total.

### HOW

* Sort warehouse At-Risk metric DESC; Top 10.
- Attention signals for Top-10 rank inclusion.
- Drill IN05 ?q= warehouse (inventory); IN02 (at-risk).

### WHY

* Multi-site performance comparison.

### WHEN

* OP01 attention list warehouse signals.

<a id="op-kpi-009"></a>
## OP-KPI-009 — Top Warehouse by Sales (Ranking)

**Location**

- OP01 - Locations

---

### WHAT

Top 10 warehouses by sales with % company total.

### HOW

* Sort warehouse Sales metric DESC; Top 10.
- Attention signals for Top-10 rank inclusion.
- Drill IN05 ?q= warehouse (inventory); IN02 (at-risk).

### WHY

* Multi-site performance comparison.

### WHEN

* OP01 attention list warehouse signals.

<a id="op-kpi-010"></a>
## OP-KPI-010 — Top Warehouse by Purchasing (Ranking)

**Location**

- OP01 - Locations

---

### WHAT

Top 10 warehouses by purchasing with % company total.

### HOW

* Sort warehouse Purchasing metric DESC; Top 10.
- Attention signals for Top-10 rank inclusion.
- Drill IN05 ?q= warehouse (inventory); IN02 (at-risk).

### WHY

* Multi-site performance comparison.

### WHEN

* OP01 attention list warehouse signals.

<a id="op-kpi-011"></a>
## OP-KPI-011 — Top Wilayah by Sales (Ranking)

**Location**

- OP01 - Locations

---

### WHAT

Top territories by customer Wilayah MTD omzet.

### HOW

* GROUP BY Wilayah on Faktur MTD.
- Drill Collection Dashboard for wilayah overdue (FI02 not OP01).

### WHY

* Commercial geography performance.

### WHEN

* OP01 → FI02 Wilayah Hotspot context.

<a id="op-kpi-012"></a>
## OP-KPI-012 — Location Attention Signal Counts

**Location**

- OP01 - Locations (Attention List)

---

### WHAT

Warehouse × Signal rows: Inactive With Stock, No Sales With Inventory, concentration ranks.

### HOW

* Six warehouse signal types per M22.
- Top-10 rank triggers concentration signals.
- Max rows in attention list.

### WHY

* Operational exceptions by site.

### WHEN

* OP01 list → IN05 / IN02 drill.

---

## 7. Indeks KPI per Menu

| Menu | KPI Codes (primary) |
| ---- | ------------------- |
| EX01 | EX-KPI-001–021 |
| EX02 | EX-KPI-022–029; concentrations reuse FI/CU/IN/PU KPIs |
| SA01 | SA-KPI-001–008 |
| SA02 | SA-KPI-009–019 |
| SA03 | SA-KPI-002 (derivable); no footer KPI |
| CU01 | CU-KPI-001–010 |
| CU02 | CU-KPI-020–036 |
| CU03 | CU-KPI-040–051 |
| CU04 | CU-KPI-060–070; EX-KPI-019–021 |
| CU05 | CU-KPI-071 |
| FI01 | FI-KPI-001–012 |
| FI02 | FI-KPI-013–028 |
| FI03 | FI-KPI-016–017 (context), FI-KPI-029–041 |
| FI04 | FI-KPI-042–043 |
| SF01 | SF-KPI-001–011 |
| SF02 | SF-KPI-012–018 |
| IN01 | IN-KPI-001–004 |
| IN02 | IN-KPI-005–012 |
| IN03 | IN-KPI-013–021 |
| IN04 | IN-KPI-022–025 |
| IN05 | IN-KPI-026–027 |
| PU01 | PU-KPI-001–012 |
| PU02 | PU-KPI-013–014 |
| OP01 | OP-KPI-001–012 |

---

## Lampiran A — Aturan Attribution & Period Semantics

| Metric type | Period | Attribution |
| ----------- | ------ | ----------- |
| Sales omzet | Current calendar month | Salesman on Faktur at invoice time |
| Piutang exposure | All-time open snapshot | Invoicing salesman on open Faktur |
| Collection recovery | Current calendar month | Company + salesman aggregates |
| Dormant customer | 90-day rule | Last invoicing salesman |
| Inventory value | Point-in-time | BrgId-first; exclude In-Transit |
| Inventory movement class | Point-in-time at refresh | Last Faktur Date per item (gross) |
| Purchasing spend | Current calendar month | Principal on purchase invoice |
| Field Activity | Selected salesman-day | Live query — not snapshot |
| Forecast KPIs | As-of business date B | Calendar-day linear extrapolation |

**Semantic gap:** FI01 all-open vs FI04 default period — use investigation `allOpenBalances` mode for reconciliation.

---

## Lampiran B — Milestone Traceability Crosswalk

| Milestone | Milestone ID prefix | Catalog prefix |
| --------- | ------------------- | -------------- |
| M16 Executive | — | EX-KPI |
| M17 Customer Analytics | — | CU-KPI-001–010 |
| M18 Salesman Performance | — | SF-KPI-001–011 |
| M18.5 Field Activity | — | SF-KPI-012–018 |
| M19 Inventory Risk | — | IN-KPI-005–012 |
| M20 Collection | — | FI-KPI-013–028 |
| M21 Purchasing | — | PU-KPI |
| M22 Locations | — | OP-KPI |
| M23 Alert Center | SignalKey in ALERT-REGISTRY | EX-KPI-022–029 |
| M26 Sales Forecast | FR-* | SA-KPI-009–019 |
| M27 Cash Flow Forecast | CFR-* | FI-KPI-029–041 |
| M28 Inventory Forecast | IFR-* | IN-KPI-013–021 |
| M28.5 Inventory Optimization | — | IN-KPI-022–025 |
| M29 Customer Risk Forecast | CRF-KPI-* | CU-KPI-020–036 |
| M30 Collection Optimization | COL-OPT-KPI-* | CU-KPI-040–051 |
| M31 Customer Portfolio | — | CU-KPI-060–071 |

---

## Document Maintenance

1. New KPI requires Product Owner approval and new catalog ID (next sequential in domain prefix).
2. Update this catalog before release; never renumber published IDs.
3. Move formulas from milestone analysis to this catalog when promoted to current.
4. Keep [`btr-portal-domain.md`](./btr-portal-domain.md) §7 as quick index linking here.

**Success criterion:** Business user understands any portal KPI without reading source code.

<!-- END KPI CATALOG -->
