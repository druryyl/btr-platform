# BTR Desktop Architecture Investigation
## Reporting Portal Feasibility Study (Read-Only)

**Scope:** `src/j05-btr-distrib` (BTR Desktop distribution system)  
**Date:** 2026-06-05  
**Purpose:** Understand where business logic currently lives before designing a web-based Reporting & Dashboard Portal.

---

## 1. Executive Summary

BTR Desktop is a **.NET Framework WinForms** application organized in a layered architecture:

| Layer | Project | Role |
|-------|---------|------|
| UI | `btr.distrib` | WinForms screens, grids, charts, RDLC print layouts |
| Application | `btr.application` | Workers, builders, policies, use cases (business rules) |
| Infrastructure | `btr.infrastructure` | Dapper-based DAL classes with inline SQL |
| Domain | `btr.domain` | Models, enums, aggregate keys |
| Framework | `btr.nuna` | `DapperHelper`, `SqlBulkCopy` helpers |
| Sync | `btr.sync` | Mobile order sync from BTrade API |
| Schema | `btr.sql` | Table DDL and seed data only |

**Key finding:** Business logic is concentrated in **C# application services** (builders, workers, policies), not in the database. The database stores **pre-calculated persisted values** (invoice totals, piutang balances, stock balances). There are **no SQL views, stored procedures, functions, or triggers** in the version-controlled schema (`btr.sql`) or anywhere else in this repository.

**Reporting today** is implemented as WinForms "Info" screens (`*Rpt`, `*InfoForm`) that query tables directly via inline SQL in `*Dal.cs` files, plus a smaller set of **RDLC print documents** bound to in-memory DTOs at print time.

**Dashboard-like features already exist** for sales omzet (KPI charts, salesman performance, pipeline vs recognized omzet) and basic customer distribution charts. These rely heavily on the **`BTR_SalesOmzet` materialized aggregate** and C# policy classes—not on database analytics objects.

**Implication for a read-only portal:** Many reports can query persisted tables directly with patterns already proven in `*ViewDal` classes. However, **stock quantities, piutang outstanding balances, sales omzet recognition, and order pipeline metrics** depend on application-layer rules or background materialization that must be understood before trusting raw SQL.

---

## 2. Findings

### 2.1 Data Access Architecture

#### ORM

**Not used.** No Entity Framework, NHibernate, or similar ORM references were found in `j05-btr-distrib`.

#### Dapper (primary data access)

Dapper is the standard access pattern across ~120 `*Dal.cs` files in `btr.infrastructure`. A shared helper wraps common operations:

```26:37:src/j05-btr-distrib/btr.nuna/Infrastructure/DapperHelper.cs
        public static IEnumerable<T> Read<T>(this SqlConnection conn, string sql, DynamicParameters param = null)
        {
            var result = conn.Query<T>(sql, param);
            if (result.Any())
                return result;
            else
                return default;
        }
        public static T ReadSingle<T>(this SqlConnection conn, string sql, DynamicParameters param)
        {
            return conn.QueryFirstOrDefault<T>(sql, param);
        }
```

**Example — CRUD with parameterized inline SQL:**

```22:38:src/j05-btr-distrib/btr.infrastructure/SupportContext/UserAgg/UserDal.cs
        public void Insert(UserModel model)
        {
            const string sql = @"
            INSERT INTO BTR_User(
                UserId, UserName, Password, Prefix, RoleId)
            VALUES (
                @UserId, @UserName, @Password, @Prefix, @RoleId)";
            // ...
            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Execute(sql, dp);
            }
        }
```

#### ADO.NET (direct usage)

ADO.NET primitives are used alongside Dapper:

- `SqlConnection` — every DAL class
- `SqlBulkCopy` — bulk inserts for child collections (order items, piutang lunas, role menus, faktur items)
- `DynamicParameters` — all parameterized queries

**Example — SqlBulkCopy:**

```32:45:src/j05-btr-distrib/btr.infrastructure/SupportContext/RoleFeature/RoleMenuDal.cs
        public void Insert(IEnumerable<RoleMenuDto> listModel)
        {
            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            using (var bcp = new SqlBulkCopy(conn))
            {
                bcp.DestinationTableName = "dbo.BTR_RoleMenu";
                // ...
                bcp.WriteToServer(fetched.AsDataTable());
            }
        }
```

#### Typed DataSet / TableAdapter

**Not used** for data access. `.resx` files reference `System.Data` designer metadata (WinForms resource convention), but no `TableAdapter`, `TypedDataSet`, or `xsd` data-access artifacts were found. Reporting uses **RDLC + in-memory objects**, not typed datasets.

#### Stored Procedures

**None found** in the repository (schema or C# `CommandType.StoredProcedure` calls). All queries are inline SQL strings in DAL classes.

#### Direct SQL

**Universal pattern.** Every read/write operation uses hand-written SQL in `btr.infrastructure`. Report queries are typically multi-table `LEFT JOIN` statements with date-range filters and `VoidDate = '3000-01-01'` for active records.

**Example — sales report SQL:**

```28:80:src/j05-btr-distrib/btr.infrastructure/SalesContext/FakturPerCustomerRpt/FakturPerCustomerDal.cs
            const string sql = @"
                SELECT
                    ISNULL(bb.FakturCode, '') AS FakturCode,
                    // ... many joined columns ...
                    aa.Total,
                    ISNULL(jj.StatusFaktur,0) StatusFaktur
                FROM
                    BTR_FakturItem aa
                    LEFT JOIN BTR_Faktur bb ON aa.FakturId = bb.FakturId
                    LEFT JOIN BTR_Brg cc ON aa.BrgId = cc.BrgId
                    // ... additional joins ...
                WHERE
                    bb.FakturDate BETWEEN @Tgl1 AND @Tgl2 
                    AND bb.VoidDate = '3000-01-01'";
```

#### SQL Views (database objects)

**None in version control.** Classes named `*View` (e.g., `FakturView`, `StokBalanceView`, `BrgStokViewModel`) are **C# DTOs** mapped from inline SQL—not database views.

---

### 2.2 Business Logic Location

Business rules are distributed across three tiers:

1. **C# Application layer** (`btr.application`) — primary calculation engine
2. **C# Form layer** (`btr.distrib`) — UI re-calculation, display formatting, some totals
3. **Persisted table columns** — calculated values saved at write time

There is **no promotion engine** as a named module. "Promotion" behavior is implemented as **manual bonus quantity** (`QtyBonus`) entered via a `qtyBesar;qtyKecil;qtyBonus` input string.

---

#### Sales

| Rule | Location | Mechanism |
|------|----------|-----------|
| **Sales Order calculation** | External (BTrade mobile API) + persisted | `BTR_Order.TotalAmount` is written by sync (`btr.sync`); not recalculated in desktop. Order line items stored in `BTR_OrderItem`. |
| **Faktur line item calculation** | C# Service | `CreateFakturItemWorker` — qty conversion, subtotal, cascading discount, DPP, PPN, total |
| **Faktur header total** | C# Service + Form | `FakturBuilder.CalcTotal()` sums line items; `FakturForm.CalcTotal()` mirrors in UI |
| **Discount calculation** | C# Service | 4-level cascading percentage discount in `CreateFakturItemWorker.GenListDiscount()` |
| **Promotion / bonus** | C# Service (qty input) | `QtyBonus` parsed from input string; affects `QtyPotStok` and stock deduction, not monetary total |
| **PPN / DPP** | C# Service | `(SubTotal - DiscRp) * DppProsen / 100`, then `PpnRp = DppRp * PpnProsen / 100` |

**Faktur item calculation (C# Service):**

```136:155:src/j05-btr-distrib/btr.application/SalesContext/FakturAgg/Workers/CreateFakturItemWorker.cs
            item.QtyJual = (item.QtyBesar * item.Conversion) + item.QtyKecil;
            item.HrgSat = item.HrgSatKecil;
            item.SubTotal = item.QtyJual * item.HrgSat;
            item.QtyBonus = (int)qtys[2];
            item.QtyPotStok = item.QtyJual + item.QtyBonus;
            var listDisc = GenListDiscount(req.BrgId, item.SubTotal, item.DiscInputStr).ToList();
            item.DiscRp = listDisc.Sum(x => x.DiscRp);
            item.DppRp = (item.SubTotal - item.DiscRp) * req.DppProsen / 100;
            item.PpnRp = item.DppRp * req.PpnProsen / 100;
            item.Total = item.SubTotal - item.DiscRp + item.PpnRp;
```

**Cascading discount (C# Service):**

```217:235:src/j05-btr-distrib/btr.application/SalesContext/FakturAgg/Workers/CreateFakturItemWorker.cs
            discRp[0] = subTotal * discs[0] / 100;
            var newSubTotal = subTotal - discRp[0];
            discRp[1] = newSubTotal * discs[1] / 100;
            // ... levels 3 and 4 ...
            result.RemoveAll(x => x.DiscProsen == 0);
```

**Faktur header totals (C# Service):**

```317:323:src/j05-btr-distrib/btr.application/SalesContext/FakturAgg/Workers/FakturBuilder.cs
        public IFakturBuilder CalcTotal()
        {
            _aggRoot.Total = _aggRoot.ListItem.Sum(x => x.SubTotal);
            _aggRoot.Discount = _aggRoot.ListItem.Sum(x => x.DiscRp);
            _aggRoot.Tax = _aggRoot.ListItem.Sum(x => x.PpnRp);
            _aggRoot.GrandTotal = _aggRoot.ListItem.Sum(x => x.Total);
            _aggRoot.KurangBayar = _aggRoot.GrandTotal - _aggRoot.UangMuka;
```

**Order totals:** `BTR_Order` and `BTR_OrderItem` are referenced in code but **DDL is missing from `btr.sql`** (schema gap—likely created manually or via legacy migration). `TotalAmount` originates from the BTrade sync path (`j06-pkl-btrade-api` defines `BTRADE_Order` schema).

---

#### Inventory

| Rule | Location | Mechanism |
|------|----------|-----------|
| **Stock lot creation (FIFO)** | C# Service | `AddStokWorker`, `StokBuilder` — creates `BTR_Stok` rows with `QtyIn`/`Qty` |
| **Stock movement ledger** | C# Service + Table | `BTR_StokMutasi` written by stock workers; kartu stok reports aggregate this |
| **Warehouse stock balance** | C# Service + Table | `GenStokBalanceWorker` sums `BTR_Stok.Qty` per Brg+Warehouse → persists `BTR_StokBalanceWarehouse` |
| **Available stock (UI lookup)** | C# Service + Inline SQL | `BrgStokViewDal` sums `BTR_Stok.Qty`; `ListBrgStokQuery` groups by BrgId |
| **Bonus stock deduction** | C# Service | `GenStokFakturWorker` removes bonus qty separately (`QtyPotStok - QtyJual`) |
| **Periodic stock snapshot** | Inline SQL | `StokPeriodikDal` aggregates `SUM(QtyIn - QtyOut)` from `BTR_StokMutasi` as-of date |

**Stock balance materialization (C# Service):**

```40:49:src/j05-btr-distrib/btr.application/InventoryContext/StokAgg/GenStokUseCase/GenStokBalanceWorker.cs
        public void Execute(GenStokBalanceRequest request)
        {
            var listStok = _stokDal.ListData(request, request) ?? new List<StokModel>();
            var qtyBalance = listStok.Sum(x => x.Qty);
            var stokBalance = _builder.Load(request).Qty(request, qtyBalance).Build();
            _writer.Save(ref stokBalance);
        }
```

**Available stock query (Inline SQL — not a DB view):**

```25:33:src/j05-btr-distrib/btr.infrastructure/BrgContext/BrgAgg/BrgStokViewDal.cs
            const string sql = @"
                SELECT aa.BrgId, aa.BrgCode, aa.BrgName, SUM(bb.Qty) Stok
                FROM BTR_Brg aa
                    LEFT JOIN BTR_Stok bb ON aa.BrgId = bb.BrgId
                        AND bb.WarehouseId = @WarehouseId
                GROUP BY aa.BrgId, aa.BrgName, aa.BrgCode";
```

**Important:** Two stock sources exist:
- `BTR_Stok` (FIFO lots) — used for transactional stock and `BrgStokViewDal`
- `BTR_StokBalanceWarehouse` (denormalized balance) — used for stock balance reports

These can diverge if balance regeneration (`StokBalancerForm`) has not been run.

---

#### Finance

| Rule | Location | Mechanism |
|------|----------|-----------|
| **Piutang creation** | C# Service | `CreatePiutangWorker` sets `Total = faktur.GrandTotal` |
| **Piutang balance (Sisa)** | C# Service | `PiutangBuilder.ReCalc()` — `Sisa = Total + Potongan - Terbayar` |
| **Potongan (adjustments)** | C# Service + Table | Sum of `BTR_PiutangElement` (Retur, Potongan, Materai, Admin) |
| **Pelunasan (payments)** | C# Service + Table | `BTR_PiutangLunas` — Cash, Cek/BG, UangMuka |
| **Outstanding receivable (reporting)** | Persisted column + Inline SQL | `BTR_Piutang.Sisa` stored; reports read it directly |
| **Customer aging** | **Not implemented** as dedicated aging buckets | Reports expose `DueDate` / `JatuhTempo` but no 30/60/90-day aging logic found |

**Piutang balance recalculation (C# Service):**

```154:161:src/j05-btr-distrib/btr.application/FinanceContext/PiutangAgg/Workers/PiutangBuilder.cs
        private void ReCalc()
        {
            var totalElement = _aggregate.ListElement.Sum(x => x.NilaiPlus - x.NilaiMinus);
            _aggregate.Potongan = totalElement;
            _aggregate.Terbayar = _aggregate.ListLunas.Sum(x => x.Nilai);
            _aggregate.Sisa = _aggregate.Total + _aggregate.Potongan - _aggregate.Terbayar;
        }
```

**Note:** `AddLunas()` uses a slightly different formula (`Sisa = Total - Terbayar`) before `ReCalc()` is called. The persisted `BTR_Piutang.Sisa` reflects whatever was saved at write time.

**Piutang per wilayah report** decomposes payment types via inline SQL subqueries on `BTR_PiutangLunas` and `BTR_PiutangElement` (`PiutangSalesWilayahDal`).

---

### 2.3 Reporting Source Analysis

Reports follow a consistent pattern:

```
WinForms *InfoForm / *ReportForm
    → I*Dal.ListData(Periode)
        → Inline SQL in btr.infrastructure
            → Bound to Syncfusion grid or MS Chart
```

RDLC printouts (Faktur, Invoice, Tagihan, Retur) bind `LocalReport` to **DTO objects built in C#** at print time—not to database queries inside the RDLC.

#### Sales Reports

| Report (Form) | DAL | Data Source | Primary Tables | Views/SPs |
|---------------|-----|-------------|----------------|-----------|
| Faktur Info (`FakturInfoForm`) | `FakturViewDal` | Inline SQL | `BTR_Faktur`, `BTR_Customer`, `BTR_SalesPerson`, `BTR_Warehouse`, `BTR_FakturControlStatus` | None |
| Faktur per Barang (`FakturBrgInfoForm`) | `FakturBrgViewDal` | Inline SQL | `BTR_FakturItem`, `BTR_Faktur`, `BTR_Brg`, `BTR_FakturDiscount` | None |
| Faktur per Customer (`FakturPerCustomerForm`) | `FakturPerCustomerDal` | Inline SQL | `BTR_FakturItem`, `BTR_Faktur`, `BTR_Brg`, `BTR_Customer`, `BTR_FakturDiscount`, `BTR_FakturControlStatus` | None |
| Faktur per Supplier (`FakturPerSupplierForm`) | `FakturPerSupplierDal` | Inline SQL | Same family as per-customer, joined via `BTR_Brg.SupplierId` | None |
| Faktur Cash (`FakturCashInfoForm`) | `FakturCashViewDal` | Inline SQL | `BTR_Faktur` (filter `UangMuka > 0`) | None |
| Faktur Pajak Info | `FakturPajakInfoDal` | Inline SQL | `BTR_Faktur`, tax-related tables | None |
| Driver Faktur Info | `DriverFakturInfoDal` | Inline SQL | `BTR_Faktur`, `BTR_Driver`, packing tables | None |
| Omzet Supplier (`OmzetSupplierInfoForm`) | `OmzetSupplierViewDal` | Inline SQL | `BTR_FakturItem`, `BTR_Faktur`, `BTR_Brg`, `BTR_Supplier` (+ retur variant) | None |
| Sales Omzet Info/Chart | `SalesOmzetDal` | Table + C# policies | `BTR_SalesOmzet`, `BTR_SalesOmzetTarget` | None |

#### Piutang / Finance Reports

| Report (Form) | DAL | Data Source | Primary Tables | Views/SPs |
|---------------|-----|-------------|----------------|-----------|
| Piutang Sales Wilayah (`PiutangSalesWilayahForm`) | `PiutangSalesWilayahDal` | Inline SQL | `BTR_Piutang`, `BTR_Faktur`, `BTR_PiutangLunas`, `BTR_PiutangElement` | None |
| Pelunasan Info (`PelunasanInfoForm`) | `PelunasanInfoDal` | Inline SQL | `BTR_Piutang`, `BTR_PiutangLunas` | None |
| Lunas Piutang (`LunasPiutang2Form`) | `PIutangLunasViewDal` | Inline SQL | `BTR_Piutang`, `BTR_Faktur`, `BTR_Customer` | None |
| Penerimaan Pelunasan Sales | `PenerimaanPelunasanSalesDal` | Inline SQL | `BTR_PiutangLunas`, `BTR_Piutang`, `BTR_Faktur` | None |
| Piutang Tracker (`PiutangTrackerForm`) | `PiutangTrackerDal` | Inline SQL (UNION) | `BTR_Piutang`, `BTR_Tagihan`, `BTR_TagihanFaktur`, `BTR_PiutangLunas` | None |
| FP Keluaran Info | `FpKeluaranViewDal` | Inline SQL | `BTR_FpKeluaran`, related faktur tables | None |

#### Inventory Reports

| Report (Form) | DAL | Data Source | Primary Tables | Views/SPs |
|---------------|-----|-------------|----------------|-----------|
| Stok Balance (`StokBalanceInfoForm`) | `StokBalanceViewDal` | Inline SQL | `BTR_StokBalanceWarehouse`, `BTR_Brg`, `BTR_Warehouse`, `BTR_BrgSatuan` | None |
| Kartu Stok (`KartuStokInfoForm`) | `KartuStokDal` | Inline SQL | `BTR_StokMutasi`, `BTR_Stok`, `BTR_Faktur`, `BTR_Invoice` | None |
| Kartu Stok Summary | `KartuStokSummaryDal` | Inline SQL | `BTR_StokMutasi` (aggregated) | None |
| Stok Periodik (`StokPeriodikForm`) | `StokPeriodikDal` | Inline SQL | `BTR_StokMutasi`, `BTR_Brg` | None |
| Stok per Supplier (`StokBrgSupplierForm`) | `StokBrgSupplierDal` | Inline SQL | `BTR_StokBalanceWarehouse`, `BTR_Brg`, `BTR_Supplier` | None |
| Retur Jual (`ReturJualReportForm`) | `ReturJualViewDal` | Inline SQL | `BTR_ReturJual`, `BTR_Customer`, `BTR_SalesPerson` | None |
| Retur Jual per Supplier/Barang | `ReturJualBrgViewDal` | Inline SQL | `BTR_ReturJualItem`, `BTR_Brg`, `BTR_Supplier` | None |
| Mutasi Info (`MutasiInfoForm`) | `MutasiBrgViewDal` | Inline SQL | `BTR_Mutasi`, `BTR_MutasiItem` | None |

#### Purchasing Reports

| Report (Form) | DAL | Data Source | Primary Tables | Views/SPs |
|---------------|-----|-------------|----------------|-----------|
| Invoice Info (`InvoiceInfoForm`) | `InvoiceViewDal` | Inline SQL | `BTR_Invoice`, `BTR_Supplier` | None |
| Invoice per Barang (`InvoiceBrgInfoForm`) | `InvoiceBrgViewDal` | Inline SQL | `BTR_InvoiceItem`, `BTR_Invoice`, `BTR_Brg` | None |
| Invoice Harian Detil (`InvoiceHarianDetilForm`) | `InvoiceHarianDetilDal` | Inline SQL | `BTR_InvoiceItem`, `BTR_Invoice`, `BTR_InvoiceDisc` | None |
| Retur Beli Info | `ReturBeliBrgViewDal` | Inline SQL | `BTR_ReturBeli`, `BTR_ReturBeliItem` | None |

---

### 2.4 Existing Dashboard Logic

Dashboard and summary features **do exist**, primarily in the Sales domain.

#### Sales Omzet Dashboard (most mature)

| Feature | Location | Data Source |
|---------|----------|-------------|
| KPI: Recognized Omzet | `SalesOmzetChartSummaryBuilder` (C#) | `BTR_SalesOmzet` + `SalesOmzetChartAmountPolicy` |
| KPI: Pipeline Omzet | `SalesOmzetChartSummaryBuilder` (C#) | Pending + Outstanding rows from `BTR_SalesOmzet` |
| Weekly omzet chart | `SalesOmzetChartForm` (WinForms Chart) | `SalesOmzetDal` → `BTR_SalesOmzet` |
| Salesman comparison (top 15) | `SalesOmzetChartSummaryBuilder` | Grouped `BTR_SalesOmzet` by `SalesPersonName` |
| Target vs achievement | `SalesOmzetTargetResolver` + `BTR_SalesOmzetTarget` | C# policy |
| Omzet status classification | `SalesOmzetStatusPolicy` (C#) | Outstanding / PendingOmzet / Completed / Void |
| Period filter modes | `SalesOmzetPeriodPolicy` (C#) | Omzet Period vs Sales Period (includes outstanding) |
| Materialization / reconcile | `ReconcileSalesOmzetWorker` (C#) | Reads `BTR_Order`, `BTR_Faktur`, `BTR_FakturControlStatus` → writes `BTR_SalesOmzet` |
| Health metrics (weekly) | `SalesOmzetHealthWeeklyDal` + policies | `BTR_SalesOmzetHealthWeekly` |

**Omzet amount resolution (C# Policy):**

```8:22:src/j05-btr-distrib/btr.application/SalesContext/SalesOmzetAgg/Policies/SalesOmzetChartAmountPolicy.cs
        public decimal ResolveAmount(SalesOmzetView row)
        {
            switch (row.OmzetStatus)
            {
                case SalesOmzetStatusEnum.Completed:
                case SalesOmzetStatusEnum.PendingOmzet:
                    return row.FakturTotal;
                case SalesOmzetStatusEnum.Outstanding:
                    return row.OrderTotal;
                default:
                    return 0;
            }
        }
```

#### Other Summary Features

| Feature | Location | Data Source |
|---------|----------|-------------|
| Customer per Kota chart | `CustomerChartRpt` (Form LINQ) | `BTR_Customer` via `CustomerDal.ListData()` |
| Effective Call report | `EffectiveCallInfoForm` + `EffectiveCallDal` | `BTR_CheckIn` joined to `BTR_Order` |
| Omzet Supplier (daily/monthly) | `OmzetSupplierInfoForm` | `OmzetSupplierViewDal` — aggregates `BTR_FakturItem.Total` |
| Piutang Tracker timeline | `PiutangTrackerForm` | Union query across piutang lifecycle tables |

**No dedicated "daily sales summary" table** exists beyond `BTR_SalesOmzet` (per order/faktur row) and report-time SQL aggregation.

---

### 2.5 Database Complexity Analysis

> **Note:** Live database access was not available during this investigation. Row counts and runtime join frequencies are **estimates/inferences** from schema design, index definitions, and query patterns in code.

#### Schema Inventory (`btr.sql`)

- **~75 table DDL scripts** organized by bounded context (Sales, Inventory, Finance, Purchase, Brg, Helper)
- **No views, stored procedures, functions, or triggers** in version control
- **Known schema gaps:** `BTR_Order` and `BTR_OrderItem` are used extensively in C# but have **no DDL in `btr.sql`**

#### Top 20 Tables (Estimated by Transaction Volume)

| Rank | Table | Estimated Role | Row Count |
|------|-------|----------------|-----------|
| 1 | `BTR_StokMutasi` | Every stock in/out movement | Unknown — likely largest |
| 2 | `BTR_FakturItem` | One row per invoice line | Unknown — high |
| 3 | `BTR_Stok` | FIFO stock lots | Unknown — high |
| 4 | `BTR_FakturDiscount` | Up to 4 discount rows per faktur item | Unknown — high |
| 5 | `BTR_OrderItem` | Mobile order lines (sync) | Unknown — high |
| 6 | `BTR_Order` | Mobile orders (sync) | Unknown — high |
| 7 | `BTR_Faktur` | Sales invoices | Unknown — high |
| 8 | `BTR_PiutangLunas` | Payment entries | Unknown — medium-high |
| 9 | `BTR_PiutangElement` | Piutang adjustments | Unknown — medium |
| 10 | `BTR_Piutang` | Receivable headers | Unknown — medium |
| 11 | `BTR_InvoiceItem` | Purchase invoice lines | Unknown — medium |
| 12 | `BTR_Invoice` | Purchase invoices | Unknown — medium |
| 13 | `BTR_CheckIn` | Salesman visit check-ins | Unknown — medium |
| 14 | `BTR_FakturControlStatus` | Faktur workflow status history | Unknown — medium |
| 15 | `BTR_ReturJualItem` | Sales return lines | Unknown — medium |
| 16 | `BTR_TagihanFaktur` | Billing collection details | Unknown — medium |
| 17 | `BTR_SalesOmzet` | Materialized omzet aggregate | Unknown — medium (growing) |
| 18 | `BTR_StokBalanceWarehouse` | Denormalized stock balance | Unknown — medium |
| 19 | `BTR_PackingBrg` / `BTR_PackingFaktur` | Packing operations | Unknown — medium |
| 20 | `BTR_CustomerLocHistory` | GPS location history | Unknown — variable |

#### Frequently Joined Table Groups

Observed across report DAL classes:

1. **Sales core:** `BTR_Faktur` ↔ `BTR_FakturItem` ↔ `BTR_Brg` ↔ `BTR_Customer` ↔ `BTR_SalesPerson`
2. **Discount expansion:** `BTR_FakturDiscount` (pivoted by `NoUrut` 1–4 in SQL)
3. **Finance:** `BTR_Piutang` ↔ `BTR_Faktur` ↔ `BTR_PiutangLunas` / `BTR_PiutangElement`
4. **Billing:** `BTR_Tagihan` ↔ `BTR_TagihanFaktur` ↔ `BTR_Faktur`
5. **Inventory:** `BTR_StokMutasi` ↔ `BTR_Brg` ↔ `BTR_Warehouse`
6. **Stock balance:** `BTR_StokBalanceWarehouse` ↔ `BTR_Brg` ↔ `BTR_BrgSatuan`
7. **Classification:** `BTR_Customer` ↔ `BTR_Wilayah` / `BTR_Klasifikasi`

#### Frequently Used "Views" (C# query projections, not DB objects)

| Name | DAL | Underlying Tables |
|------|-----|-------------------|
| `FakturView` | `FakturViewDal` | `BTR_Faktur` + dimension joins |
| `FakturPerCustomerView` | `FakturPerCustomerDal` | `BTR_FakturItem` + dimensions |
| `StokBalanceView` | `StokBalanceViewDal` | `BTR_StokBalanceWarehouse` + `BTR_Brg` |
| `BrgStokViewModel` | `BrgStokViewDal` | `BTR_Stok` aggregated |
| `KartuStokView` | `KartuStokDal` | `BTR_StokMutasi` |
| `PiutangSalesWilayahDto` | `PiutangSalesWilayahDal` | `BTR_Piutang` + payment subqueries |
| `OmzetSupplierView` | `OmzetSupplierViewDal` | `BTR_FakturItem` aggregated |

#### Stored Procedures

**None identified** in codebase or schema.

---

### 2.6 Read-Only Reporting Feasibility

#### Safe Reporting Data (low risk if queried directly)

These values are **persisted at write time** and match what desktop reports already query:

| Data Domain | Tables / Columns | Notes |
|-------------|------------------|-------|
| Invoice headers | `BTR_Faktur` (`Total`, `Discount`, `Tax`, `GrandTotal`, `FakturDate`) | Filter `VoidDate = '3000-01-01'` |
| Invoice line detail | `BTR_FakturItem` (all calculated columns persisted) | Same discount/PPN values used in reports |
| Invoice discounts | `BTR_FakturDiscount` | Already pivoted in report SQL |
| Purchase invoices | `BTR_Invoice`, `BTR_InvoiceItem`, `BTR_InvoiceDisc` | Same pattern as sales |
| Customer / Supplier master | `BTR_Customer`, `BTR_Supplier`, `BTR_SalesPerson`, `BTR_Wilayah` | Reference data |
| Product master | `BTR_Brg`, `BTR_BrgSatuan`, `BTR_BrgHarga`, `BTR_Kategori` | Reference data |
| Sales returns | `BTR_ReturJual`, `BTR_ReturJualItem` | Persisted totals |
| Purchase returns | `BTR_ReturBeli`, `BTR_ReturBeliItem` | Persisted totals |
| Materialized omzet | `BTR_SalesOmzet` | **Safe only if reconcile job is current** |
| Omzet targets | `BTR_SalesOmzetTarget` | Reference targets |
| Check-in visits | `BTR_CheckIn` | Raw visit data |
| Faktur workflow status | `BTR_FakturControlStatus` | Status history |

#### Risky Reporting Data (requires application logic awareness)

| Data Domain | Risk | Why |
|-------------|------|-----|
| **Current stock quantity** | High | Two sources: `BTR_Stok` (FIFO sum) vs `BTR_StokBalanceWarehouse` (materialized). Balance may be stale without `GenStokBalanceWorker` / Stok Balancer run |
| **Historical stock as-of date** | Medium | `StokPeriodikDal` aggregates `BTR_StokMutasi`; correct only if mutasi ledger is complete and dates accurate |
| **Piutang outstanding (`Sisa`)** | Medium | Persisted but recalculated in `PiutangBuilder`; depends on `BTR_PiutangElement` and `BTR_PiutangLunas` completeness. Inconsistent `ReCalc()` vs `AddLunas()` paths |
| **Piutang adjustment breakdown** | Medium | Report SQL filters `ElementName` by string ('Retur', 'Potongan', 'Materai', 'Admin') — must match enum `ToString()` values exactly |
| **Customer aging buckets** | High | No aging calculation exists; only `DueDate` available |
| **Sales omzet (recognized vs pipeline)** | High | Requires `SalesOmzetStatusPolicy`, `SalesOmzetChartAmountPolicy`, and period mode logic. Raw `BTR_SalesOmzet` rows need status interpretation |
| **Omzet date** | High | `OmzetDate` set from `BTR_FakturControlStatus` where `StatusFaktur = KembaliFaktur` — business rule in `SalesOmzetSnapshotBuilder` |
| **Order totals** | Medium | `BTR_Order.TotalAmount` calculated externally (BTrade mobile); desktop does not verify |
| **Promotion-adjusted sales** | Medium | `QtyBonus` affects stock (`QtyPotStok`) but not monetary totals; bonus units invisible in revenue sums |
| **Voided records** | Medium | Active record filter uses sentinel `VoidDate = '3000-01-01'` — queries missing this filter will over-count |
| **Faktur control "kembali" status** | Medium | Joined with `StatusFaktur = 2` in reports — enum-specific |

---

### 2.7 Existing Security Model

#### Tables

| Table | Purpose | Key Columns |
|-------|---------|-------------|
| `BTR_User` | User accounts | `UserId`, `UserName`, `Password` (SHA-256 hash), `Prefix`, `RoleId` |
| `BTR_Role` | Role definitions | `RoleId`, `RoleName` |
| `BTR_Menu` | Menu/form registry | `MenuId`, `FormType`, `MenuName`, `Caption`, `GroupOrder` |
| `BTR_RoleMenu` | Role-to-menu permissions | `RoleId`, `MenuId` (composite PK) |
| `BTR_UserParam` | Per-user parameters | User-specific settings |

There is **no separate permission table** beyond `BTR_RoleMenu`. Permissions are **menu-level** (which WinForms screens a role can open), not field-level or operation-level (read/write/delete).

#### Authentication Flow

```62:71:src/j05-btr-distrib/btr.distrib/SharedForm/LoginForm.cs
            var user = _userDal.GetData(new UserModel(UserIdText.Text)) ?? new UserModel();
            var passHash = PasswrodText.Text.HashSha256();
            if (passHash == user.Password)
            {
                UserId = user.UserId;
                DialogResult = DialogResult.OK;
            }
```

- Password stored as **SHA-256 hash** (no salt observed in schema)
- Authorization loads allowed menus via `RoleMenuDal.ListData(roleId)` after login
- `MainForm.SetupUserMenu()` builds navigation from `BTR_RoleMenu` + `BTR_Menu`
- Hardcoded backdoor password exists in `LoginForm` (development/testing artifact)

#### Portal Reuse Assessment

| Aspect | Reusable? | Caveats |
|--------|-----------|---------|
| User credentials (`BTR_User`) | Partially | SHA-256 without salt is weak; no lockout/expiry |
| Role definitions (`BTR_Role`) | Yes | Role names available |
| Menu permissions (`BTR_RoleMenu`) | Partially | Maps to WinForms `MenuId`/`FormType`, not web routes |
| Password verification algorithm | Must match | Portal must replicate `HashSha256()` logic to authenticate against existing passwords |

---

## 3. Risks

1. **Business logic not in database.** A reporting portal querying SQL directly will not automatically apply C# policies (omzet status, period modes, piutang recalculation, stock balance materialization).

2. **Schema drift.** `BTR_Order`/`BTR_OrderItem` lack DDL in `btr.sql`. Production schema may contain objects not represented in version control.

3. **No database-level reporting layer.** All report SQL is embedded in C# strings. A portal must either replicate these queries or extract them—risk of divergence.

4. **Dual stock sources.** `BTR_Stok` vs `BTR_StokBalanceWarehouse` can disagree; reports use different sources for different screens.

5. **Materialized aggregates may be stale.** `BTR_SalesOmzet` requires `ReconcileSalesOmzetWorker` to run; `BTR_StokBalanceWarehouse` requires balance generation. Dashboard numbers depend on job freshness.

6. **Void/active filtering convention.** Sentinel date `3000-01-01` is used throughout; missing this in queries produces incorrect totals.

7. **Authentication security.** SHA-256 without salt, plaintext-equivalent backdoor in login form, no MFA or session management suitable for web exposure.

8. **External order dependency.** Order data originates from BTrade mobile sync (`j06-pkl-btrade-api`); reporting on orders requires understanding sync lag and `StatusSync` values.

9. **No customer aging logic.** Finance reports show `DueDate` but no aging bucket calculation exists to replicate.

10. **Cross-system scope.** Related systems (`j06-pkl-btrade-api`, `j07-btr-gudang`, `btr.sync`) participate in data creation but were not fully investigated in this pass.

---

## 4. Unknown Areas

| Area | What Is Unknown |
|------|-----------------|
| **Live database row counts** | No production/staging DB connection was available |
| **Production-only DB objects** | Views, SPs, triggers, or indexes may exist in deployed DB but not in `btr.sql` |
| **`BTR_Order` / `BTR_OrderItem` DDL** | Tables used in code but missing from version-controlled schema |
| **Order line total calculation** | How BTrade mobile computes `LineTotal` / `TotalAmount` before sync |
| **Stok Balancer schedule** | How often `BTR_StokBalanceWarehouse` is regenerated in production |
| **Sales omzet reconcile schedule** | Frequency and scope of `ReconcileSalesOmzetWorker` runs |
| **Historical data volume growth** | Rate of growth for `BTR_StokMutasi`, `BTR_FakturItem` |
| **Multi-depo / multi-server** | Whether multiple BTR instances share one database or are isolated |
| **IIS / existing web presence** | Whether any web layer already exists on the target server |
| **Report access patterns** | Which desktop reports are used most frequently by business users |
| **Legacy data anomalies** | Pre-migration records with non-standard `VoidDate`, missing piutang elements, etc. |
| **`BTR_UserParam` usage** | Whether user params affect report filtering or visibility |
| **Doc workflow (`BTR_Doc`, `BTR_DocAction`)** | Whether document approval state affects reportable data |

---

## 5. Recommended Areas For Further Investigation

1. **Connect to production/staging SQL Server** and run:
   - `sp_spaceused` or equivalent for top 20 tables by size
   - `sys.objects` query for views, procedures, functions, triggers not in `btr.sql`
   - Row counts on `BTR_StokMutasi`, `BTR_FakturItem`, `BTR_Faktur`, `BTR_Order`, `BTR_Piutang`

2. **Export and compare live schema vs `btr.sql`** — especially `BTR_Order`, `BTR_OrderItem`, and any undeclared objects.

3. **Document scheduled jobs / manual processes** for:
   - `ReconcileSalesOmzetWorker` (Sales Omzet Materialize form)
   - Stok Balancer (`StokBalancerForm`)
   - BTrade order sync (`btr.sync`)

4. **Trace BTrade order calculation** in `j06-pkl-btrade-api` to understand how `TotalAmount` and discounts are computed before reaching BTR Desktop.

5. **Inventory report validation** — compare `BTR_StokBalanceWarehouse` vs `SUM(BTR_Stok.Qty)` for a sample of Brg+Warehouse combinations to quantify drift.

6. **Piutang reconciliation test** — verify `BTR_Piutang.Sisa` matches manual calculation from `Total + Potongan - Terbayar` for open receivables.

7. **Interview business users** on top 10 reports and dashboard metrics they rely on daily.

8. **Map `BTR_RoleMenu` entries to planned portal features** — determine which existing roles should see which report sections.

9. **Review `BTR_FakturControlStatus` workflow** — omzet recognition depends on `KembaliFaktur` status; document full status lifecycle.

10. **Assess `j07-btr-gudang` data overlap** — determine whether warehouse/gudang data feeds the same SQL Server or a separate database relevant to inventory reporting.

---

## Appendix A: Solution Structure

```
src/j05-btr-distrib/
├── btr.distrib/          # WinForms UI (reports, dashboards, transactions)
├── btr.application/      # Business logic (workers, builders, policies)
├── btr.infrastructure/   # DAL (Dapper + inline SQL)
├── btr.domain/           # Domain models
├── btr.nuna/             # Shared infrastructure helpers
├── btr.sync/             # BTrade mobile order sync
├── btr.sql/              # Table DDL + seeds (no views/SPs)
├── btr.test/             # Unit tests (SalesOmzet policies prominent)
└── docs/                 # Sales omzet implementation/deployment docs
```

## Appendix B: Technology Stack

| Component | Technology |
|-----------|------------|
| Runtime | .NET Framework 4.8 |
| UI | Windows Forms + Syncfusion grids |
| Charts | `System.Windows.Forms.DataVisualization.Charting` |
| Print | Microsoft RDLC (`Microsoft.Reporting.WinForms`) |
| Data access | Dapper 2.x + ADO.NET SqlClient |
| DI | Microsoft.Extensions.DependencyInjection |
| Mediator | MediatR (limited use, e.g., `ListBrgStokQuery`) |
| Database | SQL Server (connection via `DatabaseOptions` / `ConnStringHelper`) |

---

*This document describes the current state of the BTR Desktop system only. It does not propose architecture or implementation approaches for the Reporting Portal.*
