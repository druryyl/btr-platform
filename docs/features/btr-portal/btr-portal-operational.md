# BTR Portal — Operational Guide

**Audience:** End Users, Trainers, Support Team  
**Purpose:** Explain how to use BTR Portal day to day.

**Related permanent docs:** [Domain (WHY)](./btr-portal-domain.md) · [Architecture (WHAT)](./btr-portal-architecture.md) · [Materialized dashboard ops](../materialized-dashboard/materialized-dashboard-operational.md) · [Extraction — M16/M17](./knowledge-extraction-report-m16-m17.md) · [Extraction — Purchasing](./knowledge-extraction-report-purchasing-dashboard.md)

For business definitions and KPI formulas, see [btr-portal-domain.md](./btr-portal-domain.md).

---

## Login

1. Open the BTR Portal URL in a web browser.
2. Enter your **User ID** and **Password** (same credentials as BTR Desktop).
3. Click **Login**.
4. On success, you are redirected to the **Management Attention Center** at `/dashboard`.
5. Your session persists across browser refreshes until you log out or the token expires.
6. If you access a protected page without signing in, you are redirected to Login and returned to your original page after authentication.
7. Click **Logout** in the header to end your session.

**Support note:** Invalid credentials show an error on the login page. A 401 response from the API (expired token) clears the session and returns you to Login.

---

## Management Attention Center (Executive Dashboard)

The Dashboard has two levels:

| Level | Route | What You See |
| ----- | ----- | ------------ |
| **Executive (Home)** | `/dashboard` | **Management Attention Center** — attention-oriented KPIs across Sales, Piutang, Inventory, and Purchasing; Top 5 exposure lists; domain summaries. Links go to domain dashboards only. |
| **Detail** | `/dashboard/sales`, `/dashboard/piutang`, `/dashboard/customers`, `/dashboard/inventory`, `/dashboard/purchasing` | Full KPI row, charts, and Top 10 tables for that business area (Customer Analytics uses attention-oriented layout). |

**Navigate:** Sidebar → Dashboard → **Executive** (default home).

**Daily review question:** *What requires management attention today?*

### Executive Page Sections

1. **Attention Cards** — Sales (Achievement % band), Piutang (overdue, > 90 day exposure), Purchasing (pending posting), Inventory (value and concentration).
2. **Critical Exposure Lists** — Top 5 Customers, Categories, Suppliers, Principals.
3. **Domain Summaries** — Compact summary with link to each domain dashboard.

**Navigation path:** Executive → Domain Dashboard → Report. No direct report links on the executive page. For customer-specific attention, use **Dashboard → Customers** (M17).

### Sales Achievement Bands

| Achievement % | Band |
| ------------- | ---- |
| ≥ 100% | Healthy |
| 80–99% | Warning |
| < 80% | Critical |
| No target | Unknown |

### Freshness

- **Last Refreshed** — Single timestamp in header (oldest domain snapshot on screen).
- **⚠ Dashboard Data Not Fresh** — When any domain exceeds its refresh interval.

Use **Refresh** on any dashboard page to reload data from the server. Detail pages show a **generated-at** timestamp per domain.

**Unavailable data:** If the executive page shows a warning that data is not yet available, snapshot tables have not been populated. An administrator must run the snapshot worker before dashboards will load.

## Sales Dashboard

**Navigate:** Sidebar → Dashboard → Sales, or click the Sales attention card on the Management Attention Center.

**Route:** `/dashboard/sales`

### What You See

1. **KPI row** — Total Target, Total Achievement, Achievement %
2. **Target vs Achievement chart** — Company-level bar comparing monthly target to invoiced omzet
3. **Weekly Invoiced Sales Trend** — Line chart of Faktur totals by week within the current month
4. **Top 10 Salesman** — Table ranked by invoiced omzet (highest first)

### How to Read It

- **Total Target** is the sum of all salesperson monthly targets set in BTR.
- **Total Achievement** is invoiced omzet (`GrandTotal` on Fakturs) for the current calendar month.
- **Achievement %** shows progress toward target; it is blank when no targets are set.
- Rankings show at most 10 salespeople.

### Typical Use

- Morning management review of monthly sales progress
- Identifying top performers for the current month
- Checking whether weekly omzet trend is accelerating or slowing

---

## Customer Analytics Dashboard (M17)

**Navigate:** Sidebar → Dashboard → Customers.

**Route:** `/dashboard/customers`

**Question answered:** Which customers require management attention?

### What You See (fixed section order)

1. **Attention Cards** — Collection, Concentration, Activity, Inactivity, Credit
2. **Customer Attention List** — one row per customer × signal
3. **Top 10 Rankings** — Omzet (current month) and Piutang (all open)
4. **Segmentation** — By Klasifikasi, By Wilayah, Active vs Dormant
5. **Navigation** — links to Sales/Piutang dashboards and reports

### Attention signals

| Signal | Rule |
| ------ | ---- |
| Overdue | Any overdue balance on the customer |
| Dormant | No Faktur for 90 days with prior purchase history; active this month excluded |
| Plafond breach | Open balance > `Plafond` when `Plafond > 0` |
| Suspended + Sales | `IsSuspend` and Faktur in current calendar month |

Concentration percentages (Top Omzet %, Top Piutang %) are informational — no automatic warning thresholds.

**Card shortcuts:** Collection card → Piutang Dashboard; Activity card → Sales Dashboard; Credit card → Attention List on this page.

### Drill-down

Click a customer row (attention list or rankings) → Sales or Piutang Report with customer name pre-filter (`?q=`). Piutang dashboard uses all-time open balance; Piutang Report defaults to a period filter — same semantic gap as the Piutang Dashboard.

**Supplements** Executive Dashboard Top 5 Customers — does not replace it.

---

## Piutang Dashboard

**Navigate:** Sidebar → Dashboard → Piutang, or click the Piutang attention card on the Management Attention Center.

**Route:** `/dashboard/piutang`

### What You See

1. **KPI row** — Total Piutang, Total Customer, Overdue Customer
2. **Aging Distribution** — Pie chart with five buckets: Current, 1–30 Days, 31–60 Days, 61–90 Days, > 90 Days
3. **Top 10 Outstanding Customers** — Table ranked by outstanding balance

### How to Read It

- **Total Piutang** is the sum of all open invoice balances (`KurangBayar`).
- **Total Customer** counts distinct customers with open balances.
- **Overdue Customer** counts customers with any balance past the due date (Current bucket excluded).
- Aging is calculated from **Jatuh Tempo** (due date) versus today.
- The sum of all aging slices equals Total Piutang.

### Typical Use

- Finance team daily monitoring of collection exposure
- Identifying customers with the largest outstanding balances
- Understanding how much debt is severely overdue (> 90 days)

---

## Inventory Dashboard

**Navigate:** Sidebar → Dashboard → Inventory, or click the Inventory attention card on the Management Attention Center.

**Route:** `/dashboard/inventory`

### What You See

1. **KPI row** — Total Inventory Value, Total Item
2. **Inventory by Category** — Horizontal bar chart
3. **Inventory by Supplier** — Horizontal bar chart
4. **Top 10 Categories** — Table ranked by inventory value
5. **Top 10 Suppliers** — Table ranked by inventory value

### How to Read It

- Values use **HPP × Qty** (cost × quantity).
- In-Transit warehouse stock is excluded.
- Items with zero net quantity are excluded.
- Blank category or supplier labels appear as **Unknown**.
- Chart and table data show the same Top 10 for each dimension.

### Typical Use

- Reviewing where inventory capital is concentrated
- Supporting purchasing decisions by supplier exposure
- Planning category-level stock reviews

---

## Purchasing Dashboard

**Navigate:** Sidebar → Dashboard → Purchasing, or click the Purchasing attention card on the Management Attention Center.

**Route:** `/dashboard/purchasing`

### What You See

1. **KPI row** — Grand Total Purchase, Total Invoice, Pending Posting Invoice Count
2. **Weekly Purchase Trend** — Line chart of purchase invoice totals by week within the current month
3. **Posting Status Breakdown** — Pie chart with two buckets: `SUDAH` (posted) and `BELUM` (not yet posted)
4. **Top 10 Principal** — Table ranked by purchase amount (highest first)

### How to Read It

- **Grand Total Purchase** is the sum of `GrandTotal` on purchase invoices for the current calendar month.
- **Total Invoice** counts purchase invoice rows in the period.
- **Pending Posting Invoice Count** counts invoices where Posting Stok is `BELUM`.
- **Principal** is the supplier name on the purchase invoice; blank names appear as **Unknown**.
- Void invoices are excluded from all metrics.
- The sum of posting-status slices equals Grand Total Purchase.

### Typical Use

- Management review of monthly purchasing spend and volume
- Identifying principals with the largest purchase concentration
- Spotting backlog of invoices awaiting stock posting (`BELUM`)

---

## Report Filtering (All Reports)

Each report includes a filter bar above the table:

| Filter | Applies to | Behavior |
|--------|------------|----------|
| **Period** (date range) | Sales, Piutang, Purchasing | Server-side query; max **31 days**; defaults to current calendar month |
| **Filter by** (date field) | Piutang only | `Jatuh Tempo` (DueDate, default) or `Piutang Date` (PiutangDate) |
| **Search** (free text) | All four reports | Client-side only; filters visible rows instantly |

Click **Apply** after changing the period (or Piutang date field). **Refresh** reloads data using the active period filters.

When search text is active on reports with a summary bar, footer totals recalculate from the filtered rows.

---

## Sales Report

**Navigate:** Sidebar → Reports → Sales Report

**Route:** `/reports/sales`

### What You See

- Period label reflecting the active date range (default: current calendar month)
- Filter bar: period picker and search box
- DataTable with columns: Date, Faktur, Customer, Sales, Total, Status
- Client-side pagination (default 25 rows per page)
- Sortable columns
- Refresh button and generated-at timestamp

### How to Use

1. Open the report to see Fakturs issued in the selected period (default: current month).
2. Adjust the period (max 31 days) and click **Apply** to reload from the server.
3. Use **Search** to filter by Faktur code, customer, sales person, or status (`Kembali`).
4. Sort by any column (click column header).
5. **Status** shows `Kembali` when the signed Faktur has been physically returned.
6. Use **Refresh** to reload after new Desktop transactions.

**Search fields:** Faktur, Customer, Sales, Status.

**Note:** This report does not show footer summary totals. Use the Sales Dashboard for aggregate KPIs.

---

## Piutang Report

**Navigate:** Sidebar → Reports → Piutang Report

**Route:** `/reports/piutang`

### What You See

- Period label with active date field (`Jatuh Tempo` or `Piutang Date`) and range (default: current month on Jatuh Tempo)
- Filter bar: period picker, date-field selector, and search box
- DataTable columns: Customer, Sales, Faktur, Tanggal, Jatuh Tempo, Total Jual, Kurang Bayar
- **Summary bar** below the table: Total Piutang, Total Customer
- Only open balances (`Kurang Bayar > 1`) — paid invoices are not shown

### Piutang Report vs Piutang Dashboard

The **Piutang Dashboard** shows **all** open receivables (all-time analytics). The **Piutang Report** shows open receivables whose selected date field falls within the chosen period (max 31 days). Footer totals on the report therefore **do not** match the Piutang Dashboard when a period filter is applied.

### How to Use

1. Choose **Filter by**: `Jatuh Tempo` (collection planning) or `Piutang Date` (receivable record date).
2. Set the period (max 31 days) and click **Apply**.
3. Use **Search** to filter by customer, sales person, or faktur code.
4. Sort by Jatuh Tempo to prioritize collection calls.
5. Use **Refresh** after payments are recorded in BTR Desktop.

**Search fields:** Customer, Sales, Faktur.

---

## Inventory Report

**Navigate:** Sidebar → Reports → Inventory Report

**Route:** `/reports/inventory`

### What You See

- Point-in-time stock balance (no date-range filter — current snapshot only)
- Filter bar: search box only
- DataTable columns: Item, Warehouse, Qty, HPP, Nilai Sediaan
- Only rows with Qty > 0; In-Transit warehouse excluded
- **Summary bar:** Total Inventory Value, Total Item
- Helper text explaining footer uses item-level aggregation

### How to Use

1. Browse stock by item and warehouse.
2. Use **Search** to filter by item name/code or warehouse.
3. Confirm footer totals match the Inventory Dashboard (when search is empty).
4. Remember: footer totals group by item first — the sum of visible row values may differ from the footer.
5. Use **Refresh** after stock movements in BTR Desktop.

**Search fields:** Item, Warehouse.

---

## Purchasing Report

**Navigate:** Sidebar → Reports → Purchasing Report

**Route:** `/reports/purchasing`

### What You See

- Period label reflecting the active date range (default: current calendar month)
- Filter bar: period picker and search box
- DataTable columns: Invoice, Date, Supplier, Warehouse, Total, Disc, Tax, Grand Total, Posting Stok
- **Posting Stok:** `SUDAH` (stock posted) or `BELUM` (not yet posted)
- **Summary bar:** Grand Total Purchase, Total Invoice

### How to Use

1. Review purchase invoices in the selected period (default: current month).
2. Adjust the period (max 31 days) and click **Apply** to reload from the server.
3. Use **Search** to filter by invoice code, supplier, warehouse, or posting status (`SUDAH` / `BELUM`).
4. Confirm footer **Grand Total Purchase** and **Total Invoice** match the Purchasing Dashboard when viewing the same period without search text.
5. Use **Refresh** after new purchases are entered in BTR Desktop.

**Search fields:** Invoice, Supplier, Warehouse, Posting Stok.

---

## Navigation Structure

### Menu Hierarchy

```text
BTR Portal
├── Dashboard
│   ├── Overview          → /dashboard
│   ├── Sales             → /dashboard/sales
│   ├── Piutang           → /dashboard/piutang
│   ├── Customers         → /dashboard/customers
│   ├── Inventory         → /dashboard/inventory
│   └── Purchasing        → /dashboard/purchasing
└── Reports
    ├── Sales Report      → /reports/sales
    ├── Piutang Report    → /reports/piutang
    ├── Inventory Report  → /reports/inventory
    └── Purchasing Report → /reports/purchasing
```

### Routes

| Route | Page | Auth Required |
| ----- | ---- | ------------- |
| `/login` | Login | No |
| `/` | Redirect to `/dashboard` | — |
| `/dashboard` | Dashboard home (summary KPIs) | Yes |
| `/dashboard/sales` | Sales analytics | Yes |
| `/dashboard/piutang` | Piutang analytics | Yes |
| `/dashboard/customers` | Customer Analytics | Yes |
| `/dashboard/inventory` | Inventory analytics | Yes |
| `/dashboard/purchasing` | Purchasing analytics | Yes |
| `/reports/sales` | Sales Report | Yes |
| `/reports/piutang` | Piutang Report | Yes |
| `/reports/inventory` | Inventory Report | Yes |
| `/reports/purchasing` | Purchasing Report | Yes |

### Navigation Flow

```text
Login → Dashboard Home (Management Attention Center)
          ├── Sidebar → Sales Dashboard
          ├── Sidebar → Piutang Dashboard
          ├── Sidebar → Customer Analytics
          ├── Sidebar → Inventory Dashboard
          └── Sidebar → Purchasing Dashboard

Customer Analytics → click customer row → Sales or Piutang Report (pre-filtered)
Executive / Domain Dashboard → Report (domain reports)

Sidebar → Reports → [Sales | Piutang | Inventory | Purchasing] Report
```

Authenticated users visiting `/login` are redirected to Dashboard home.

---

## Common User Workflows

### Reviewing Sales Performance

1. Sign in → Dashboard home → check Sales card (Total Omzet, Faktur, Customer).
2. Open **Dashboard → Sales** for target, achievement %, weekly trend, and Top 10 salesman.
3. Open **Reports → Sales Report** to verify individual Fakturs for the current month.

### Monitoring Overdue Receivables

1. Sign in → Dashboard home → check Total Piutang on Piutang card.
2. Open **Dashboard → Piutang** → review Aging pie chart and Overdue Customer count.
3. Check **Top 10 Outstanding Customers** for collection priorities.
4. Open **Reports → Piutang Report** → sort by Jatuh Tempo → follow up on oldest balances.
5. Confirm report footer matches dashboard KPIs.

### Reviewing Customer Attention (M17)

1. Sign in → **Dashboard → Customers** (`/dashboard/customers`).
2. Scan **Attention Cards** — Collection (overdue, >90d exposure), Inactivity (dormant), Credit (plafond breach, suspended + sales).
3. Review **Customer Attention List** for specific customers and signals.
4. Check **Top 10 Rankings** for revenue and receivable concentration.
5. Click a customer row → Sales or Piutang Report opens with customer name pre-filled in search.
6. Use **Navigation** section to jump to Sales/Piutang dashboards for domain context.
7. Cross-check overdue count against **Dashboard → Piutang** when reconciling collection exposure.

### Monitoring Inventory Value

1. Sign in → Dashboard home → check Total Inventory Value and Total Item.
2. Open **Dashboard → Inventory** → review category and supplier charts.
3. Check Top 10 Categories and Top 10 Suppliers for concentration risk.
4. Open **Reports → Inventory Report** for item × warehouse detail.
5. Confirm report footer matches dashboard KPIs.

### Reviewing Monthly Purchasing

1. Sign in → Dashboard home → check Grand Total Purchase and Total Invoice on the Purchasing card.
2. Open **Dashboard → Purchasing** → review weekly trend, posting-status breakdown, and Top 10 Principal.
3. Check **Pending Posting Invoice Count** against the posting pie chart (`BELUM` slice).
4. Open **Reports → Purchasing Report** → scan Posting Stok column for `BELUM` invoices needing warehouse action in BTR Desktop.
5. Confirm report footer matches dashboard KPIs.

---

## Frequently Asked Questions

### Why do my dashboard numbers differ from the report?

- **Sales:** Dashboard Total Omzet / Total Achievement should equal the sum of **Total** column values on the Sales Report for the same month (both use Faktur `GrandTotal`). If they differ after refreshing both pages, escalate to support.
- **Piutang & Inventory:** Footer totals should match dashboard KPIs. If they differ, refresh both pages. Persistent mismatch indicates a support escalation.
- **Purchasing:** Grand Total Purchase and Total Invoice on the dashboard must match the Purchasing Report footer for the same month. If they differ after refreshing both pages, escalate to support.
- **Inventory report footer vs row sum:** Footer groups by item first; row-level Nilai Sediaan sums across warehouses will not necessarily equal the footer.

### Why does the dashboard show an old timestamp?

Dashboard data refreshes on a background schedule, not on every page load. The **generated-at** time shows when snapshots were last rebuilt. Piutang refreshes every 15 minutes, Sales, Purchasing, and Customer every 30 minutes, Inventory every 60 minutes. Click **Refresh** to re-read the latest stored snapshot; it does not force an immediate recalculation unless an administrator triggers a manual rebuild.

### Why does the dashboard say data is not available?

Snapshot tables may be empty (new deployment or worker not running). Contact your administrator to run the snapshot worker. Detail dashboard pages return an error until snapshots exist.

### Why can't I change the date range?

V1 reports and dashboards use **fixed periods** (current month for sales/purchasing; open-balance snapshot for piutang; point-in-time for inventory). Date-range filters are planned for a future release.

### Why don't I see paid invoices on the Piutang Report?

Only open balances (`Kurang Bayar > 1`) are shown. Fully paid invoices are excluded by design — there is no "Show Paid" toggle.

### Why is Achievement % blank?

Achievement % requires at least one salesperson target row for the current month in BTR. When Total Target is zero, the percentage is not displayed.

### Can I enter transactions in the portal?

No. BTR Portal is read-only. All transactions must be entered in BTR Desktop.

### What does Posting Stok mean?

On the Purchasing Report, `SUDAH` means the invoice has been posted to inventory; `BELUM` means stock has not yet been received/posted. Warehouse staff complete posting in BTR Desktop.

### What does "Kembali" mean on the Sales Report?

The customer has returned the signed Faktur document to the office (Faktur Kembali status).

### Who can access the portal?

Any BTR user with valid credentials. All authenticated users see the same menus. There is no per-role menu filtering in the current version.

### How do I export to Excel?

Export is not available in the current version. Use BTR Desktop reports for Excel export if needed.

---

## For Administrators and IT

Operational steps for deploying and maintaining BTR Portal on a Windows server. End users can skip this section.

### Prerequisites

1. Deploy `btr.sql` schema (includes `BTR_PortalDashboard*` tables).
2. Deploy `btr.portal.api`, `btr.portal.web`, and `btr.portal.worker`.
3. Configure database, JWT, and CORS on each server (see below).
4. Run an initial snapshot backfill before users access dashboards.

### Database Configuration

Portal does **not** use the BTR Desktop registry. Create `appsettings.{MACHINE_NAME}.json` in **both** the API folder and the worker folder (`{MACHINE_NAME}` must match `Environment.MachineName`):

```json
{
  "Database": {
    "ServerName": "OFFICE-SQL01\\SQLEXPRESS",
    "DbName": "btr",
    "IsTest": false
  },
  "Jwt": {
    "Issuer": "btr-portal-api",
    "Audience": "btr-portal-vue",
    "Key": "REPLACE-WITH-STRONG-SECRET-256-BITS-MINIMUM",
    "ExpiryMinutes": 480
  },
  "Cors": {
    "AllowedOrigins": [ "http://your-server/btr-portal" ]
  }
}
```

The IIS app pool identity must reach SQL Server using the embedded `btrLogin` credentials.

### Publishing the API

1. Open `j05-btr-distrib.sln` in Visual Studio 2022 (**ASP.NET and web development** workload required).
2. Right-click `btr.portal.api` → **Publish** → **FolderProfile**.
3. Output: `src/j05-btr-distrib/publish/btr-portal-api/`
4. Copy to IIS physical path (e.g. `/btr-portal-api`).
5. Add `appsettings.{MACHINE_NAME}.json` and ensure `logs/` is writable.

### Publishing the Frontend

```powershell
cd src\j05-btr-distrib\btr.portal.web
npm install
npm run build
```

Copy `dist/` contents to the IIS static site (e.g. `/btr-portal`). Set `VITE_API_BASE_URL` at build time to the production API URL.

### Snapshot Worker Setup

**Initial backfill** (required once after deploy):

```powershell
cd C:\path\to\btr.portal.worker
.\btr.portal.worker.exe --domain All --triggered-by Manual
```

Verify `BTR_PortalDashboardRefreshLog` shows `Success` for Piutang, Inventory, Sales, Purchasing, and Customer.

**Scheduled tasks** — create five separate Windows Task Scheduler jobs:

| Task name | Interval | Command |
| --------- | -------- | ------- |
| `BTR-Portal-Dashboard-Piutang` | Every 15 min | `btr.portal.worker.exe --domain Piutang --triggered-by Scheduler` |
| `BTR-Portal-Dashboard-Sales` | Every 30 min | `btr.portal.worker.exe --domain Sales --triggered-by Scheduler` |
| `BTR-Portal-Dashboard-Purchasing` | Every 30 min | `btr.portal.worker.exe --domain Purchasing --triggered-by Scheduler` |
| `BTR-Portal-Dashboard-Inventory` | Every 60 min | `btr.portal.worker.exe --domain Inventory --triggered-by Scheduler` |
| `BTR-Portal-Dashboard-Customer` | Every 30 min | `btr.portal.worker.exe --domain Customer --triggered-by Scheduler` |

Task settings: run whether user is logged on or not; service account with SQL access; **Start in** = worker folder; stop if running longer than 30 minutes.

**Worker CLI reference:**

| Argument | Values | Default |
| -------- | ------ | ------- |
| `--domain` | `All`, `Piutang`, `Inventory`, `Sales`, `Purchasing`, `Customer` | `All` |
| `--triggered-by` | `Scheduler`, `Manual` | `Scheduler` |

Exit code `0` = success. Logs: `{worker-folder}/logs/btr-portal-worker-{date}.log`.

### Manual Dashboard Refresh

**From the portal (authenticated):**

```http
POST /api/admin/dashboard/refresh
Authorization: Bearer <token>
Content-Type: application/json

{ "domain": "All" }
```

`domain` accepts `All` (default), `Piutang`, `Inventory`, `Sales`, `Purchasing`, or `Customer` (case-insensitive).

**Prefer worker CLI** for full rebuilds — the API runs refresh synchronously and may hit IIS request timeout (~110 seconds). Use the API for single-domain ad-hoc refresh; use the worker for `--domain All` or initial backfill.

### Monitoring

| Check | How |
| ----- | --- |
| API health | `GET /api/health` → 200 |
| Snapshot health | `GET /api/health/dashboard-snapshots` — status: `unknown`, `ok`, `refreshing`, or `degraded`; each domain (Piutang, Sales, Purchasing, Inventory, Customer) shows `LastRefresh.Status` |
| Last refresh (SQL) | `SELECT TOP 1 * FROM BTR_PortalDashboardRefreshLog WHERE Domain = 'Purchasing' ORDER BY CompletedAt DESC` |
| Worker log | `{worker-folder}/logs/btr-portal-worker-{date}.log` |
| Task Scheduler | History tab on each scheduled task |

### Troubleshooting

| Symptom | Likely cause | Action |
| ------- | ------------ | ------ |
| Login fails with SQL error | Missing or wrong `appsettings.{MACHINE}.json` | Fix Database section; recycle app pool |
| Dashboard 503 / unavailable | Empty snapshot tables | Run worker `--domain All --triggered-by Manual` |
| Stale dashboard data | Scheduler not running | Check Task Scheduler history; re-run worker |
| API refresh times out | Full rebuild too slow for IIS | Use worker CLI instead |
| CORS error in browser | Origin not in `Cors:AllowedOrigins` | Add frontend URL to API appsettings |

### Post-Deploy Verification

```text
GET  /api/health                         → 200
GET  /api/health/dashboard-snapshots     → 200
POST /api/auth/login                     → 200 with token
GET  /api/dashboard/executive            → 200 with token (after worker run)
GET  /api/dashboard/customers            → 200 with token (after Customer worker run)
GET  /api/dashboard/sales                → 200 with token
GET  /api/dashboard/purchasing           → 200 with token
GET  /api/reports/sales                  → 200 with token
GET  /api/reports/purchasing             → 200 with token
```
