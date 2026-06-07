# BTR Portal — Operational Guide

**Audience:** End Users, Trainers, Support Team  
**Purpose:** Explain how to use BTR Portal day to day.

**Related permanent docs:** [Domain (WHY)](./btr-portal-domain.md) · [Architecture (WHAT)](./btr-portal-architecture.md) · [Materialized dashboard ops](../materialized-dashboard/materialized-dashboard-operational.md) · [Extraction report M1–M15](./knowledge-extraction-report-m1-m15.md)

For business definitions and KPI formulas, see [btr-portal-domain.md](./btr-portal-domain.md).

---

## Login

1. Open the BTR Portal URL in a web browser.
2. Enter your **User ID** and **Password** (same credentials as BTR Desktop).
3. Click **Login**.
4. On success, you are redirected to the Dashboard home.
5. Your session persists across browser refreshes until you log out or the token expires.
6. If you access a protected page without signing in, you are redirected to Login and returned to your original page after authentication.
7. Click **Logout** in the header to end your session.

**Support note:** Invalid credentials show an error on the login page. A 401 response from the API (expired token) clears the session and returns you to Login.

---

## Dashboard Overview

The Dashboard has two levels:

| Level | Route | What You See |
| ----- | ----- | ------------ |
| **Home** | `/dashboard` | Three summary KPI cards: Sales, Piutang, Inventory. Each card links to detailed analytics. |
| **Detail** | `/dashboard/sales`, `/dashboard/piutang`, `/dashboard/inventory` | Full KPI row, charts, and Top 10 tables for that business area. |

Use **Refresh** on any dashboard page to reload data from the server. Each page shows a **generated-at** timestamp indicating when the underlying snapshot was last refreshed (not necessarily when you clicked Refresh — Refresh re-reads the stored snapshot).

**Data freshness:** Dashboard numbers update on a background schedule (Piutang every 15 minutes, Sales every 30 minutes, Inventory every 60 minutes). Home cards may show different timestamps per domain. If data looks stale, wait for the next scheduled refresh or ask your administrator to trigger a manual refresh.

**Unavailable data:** If the dashboard home shows a warning that data is not yet available, snapshot tables have not been populated. An administrator must run the snapshot worker before dashboards will load.

## Sales Dashboard

**Navigate:** Sidebar → Dashboard → Sales, or click **View details** on the Sales card at Dashboard home.

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

## Piutang Dashboard

**Navigate:** Sidebar → Dashboard → Piutang, or click the link on the Piutang card at Dashboard home.

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

**Navigate:** Sidebar → Dashboard → Inventory, or click the link on the Inventory card at Dashboard home.

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

## Sales Report

**Navigate:** Sidebar → Reports → Sales Report

**Route:** `/reports/sales`

### What You See

- Period label showing the **current calendar month**
- DataTable with columns: Date, Faktur, Customer, Sales, Total, Status
- Client-side pagination (default 25 rows per page)
- Sortable columns
- Refresh button and generated-at timestamp

### How to Use

1. Open the report to see all Fakturs issued in the current month.
2. Sort by any column (click column header).
3. Paginate through results using the table controls.
4. **Status** shows `Kembali` when the signed Faktur has been physically returned.
5. Use **Refresh** to reload after new Desktop transactions.

**Note:** This report does not show footer summary totals. Use the Sales Dashboard for aggregate KPIs.

---

## Piutang Report

**Navigate:** Sidebar → Reports → Piutang Report

**Route:** `/reports/piutang`

### What You See

- Period label: open receivables from **2000 through today**
- DataTable columns: Customer, Sales, Faktur, Tanggal, Jatuh Tempo, Total Jual, Kurang Bayar
- **Summary bar** below the table: Total Piutang, Total Customer
- Only open balances (`Kurang Bayar > 1`) — paid invoices are not shown

### How to Use

1. Review individual open Fakturs and their due dates.
2. Sort by Jatuh Tempo to prioritize collection calls.
3. Confirm footer **Total Piutang** and **Total Customer** match the Piutang Dashboard home card.
4. Use **Refresh** after payments are recorded in BTR Desktop.

---

## Inventory Report

**Navigate:** Sidebar → Reports → Inventory Report

**Route:** `/reports/inventory`

### What You See

- Point-in-time stock balance (no date-range label)
- DataTable columns: Item, Warehouse, Qty, HPP, Nilai Sediaan
- Only rows with Qty > 0; In-Transit warehouse excluded
- **Summary bar:** Total Inventory Value, Total Item
- Helper text explaining footer uses item-level aggregation

### How to Use

1. Browse stock by item and warehouse.
2. Confirm footer totals match the Inventory Dashboard.
3. Remember: footer totals group by item first — the sum of visible row values may differ from the footer.
4. Use **Refresh** after stock movements in BTR Desktop.

---

## Purchasing Report

**Navigate:** Sidebar → Reports → Purchasing Report

**Route:** `/reports/purchasing`

### What You See

- Period label: **current calendar month**
- DataTable columns: Invoice, Date, Supplier, Warehouse, Total, Disc, Tax, Grand Total, Posting Stok
- **Posting Stok:** `SUDAH` (stock posted) or `BELUM` (not yet posted)
- **Summary bar:** Grand Total Purchase, Total Invoice

### How to Use

1. Review purchase invoices received this month.
2. Filter visually for `BELUM` posting status to find invoices awaiting stock receipt.
3. Use **Refresh** after new purchases are entered in BTR Desktop.

**Note:** There is no Purchasing Dashboard. This report is the sole purchasing visibility in the portal.

---

## Navigation Structure

### Menu Hierarchy

```text
BTR Portal
├── Dashboard
│   ├── Overview          → /dashboard
│   ├── Sales             → /dashboard/sales
│   ├── Piutang           → /dashboard/piutang
│   └── Inventory         → /dashboard/inventory
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
| `/dashboard/inventory` | Inventory analytics | Yes |
| `/reports/sales` | Sales Report | Yes |
| `/reports/piutang` | Piutang Report | Yes |
| `/reports/inventory` | Inventory Report | Yes |
| `/reports/purchasing` | Purchasing Report | Yes |

### Navigation Flow

```text
Login → Dashboard Home
          ├── "View details" / Sidebar → Sales Dashboard
          ├── "View details" / Sidebar → Piutang Dashboard
          └── "View details" / Sidebar → Inventory Dashboard

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

### Monitoring Inventory Value

1. Sign in → Dashboard home → check Total Inventory Value and Total Item.
2. Open **Dashboard → Inventory** → review category and supplier charts.
3. Check Top 10 Categories and Top 10 Suppliers for concentration risk.
4. Open **Reports → Inventory Report** for item × warehouse detail.
5. Confirm report footer matches dashboard KPIs.

### Reviewing Monthly Purchasing

1. Sign in → **Reports → Purchasing Report**.
2. Review Grand Total Purchase and invoice count in the summary bar.
3. Scan Posting Stok column for `BELUM` invoices needing warehouse action in BTR Desktop.

---

## Frequently Asked Questions

### Why do my dashboard numbers differ from the report?

- **Sales:** Dashboard Total Omzet / Total Achievement should equal the sum of **Total** column values on the Sales Report for the same month (both use Faktur `GrandTotal`). If they differ after refreshing both pages, escalate to support.
- **Piutang & Inventory:** Footer totals should match dashboard KPIs. If they differ, refresh both pages. Persistent mismatch indicates a support escalation.
- **Inventory report footer vs row sum:** Footer groups by item first; row-level Nilai Sediaan sums across warehouses will not necessarily equal the footer.

### Why does the dashboard show an old timestamp?

Dashboard data refreshes on a background schedule, not on every page load. The **generated-at** time shows when snapshots were last rebuilt. Piutang refreshes every 15 minutes, Sales every 30 minutes, Inventory every 60 minutes. Click **Refresh** to re-read the latest stored snapshot; it does not force an immediate recalculation unless an administrator triggers a manual rebuild.

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

Verify `BTR_PortalDashboardRefreshLog` shows `Success` for Piutang, Inventory, and Sales.

**Scheduled tasks** — create three separate Windows Task Scheduler jobs:

| Task name | Interval | Command |
| --------- | -------- | ------- |
| `BTR-Portal-Dashboard-Piutang` | Every 15 min | `btr.portal.worker.exe --domain Piutang --triggered-by Scheduler` |
| `BTR-Portal-Dashboard-Sales` | Every 30 min | `btr.portal.worker.exe --domain Sales --triggered-by Scheduler` |
| `BTR-Portal-Dashboard-Inventory` | Every 60 min | `btr.portal.worker.exe --domain Inventory --triggered-by Scheduler` |

Task settings: run whether user is logged on or not; service account with SQL access; **Start in** = worker folder; stop if running longer than 30 minutes.

**Worker CLI reference:**

| Argument | Values | Default |
| -------- | ------ | ------- |
| `--domain` | `All`, `Piutang`, `Inventory`, `Sales` | `All` |
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

`domain` accepts `All` (default), `Piutang`, `Inventory`, or `Sales` (case-insensitive).

**Prefer worker CLI** for full rebuilds — the API runs refresh synchronously and may hit IIS request timeout (~110 seconds). Use the API for single-domain ad-hoc refresh; use the worker for `--domain All` or initial backfill.

### Monitoring

| Check | How |
| ----- | --- |
| API health | `GET /api/health` → 200 |
| Snapshot health | `GET /api/health/dashboard-snapshots` — status: `unknown`, `ok`, `refreshing`, or `degraded` |
| Last refresh (SQL) | `SELECT TOP 1 * FROM BTR_PortalDashboardRefreshLog WHERE Domain = 'Piutang' ORDER BY CompletedAt DESC` |
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
GET  /api/dashboard/overview             → 200 with KPI data (after worker run)
GET  /api/dashboard/sales                → 200 with token
GET  /api/reports/sales                  → 200 with token
```
