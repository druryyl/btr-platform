# BTR Portal — Operational Guide

**Audience:** End Users, Trainers, Support Team  
**Purpose:** Explain how to use BTR Portal day to day.

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

Use **Refresh** on any dashboard page to reload data from the server. Each page shows a **generated-at** timestamp indicating when data was last fetched.

---

## Sales Dashboard

**Navigate:** Sidebar → Dashboard → Sales, or click **View details** on the Sales card at Dashboard home.

**Route:** `/dashboard/sales`

### What You See

1. **KPI row** — Total Target, Total Achievement, Achievement %
2. **Target vs Achievement chart** — Company-level bar comparing monthly target to achieved omzet
3. **Weekly Omzet Trend** — Line chart of recognized omzet by week within the current month
4. **Top 10 Salesman** — Table ranked by Completed Omzet (highest first)

### How to Read It

- **Total Target** is the sum of all salesperson monthly targets set in BTR.
- **Total Achievement** is recognized (completed) omzet for the current calendar month.
- **Achievement %** shows progress toward target; it is blank when no targets are set.
- **Pipeline omzet** is not shown on this page (it appears in backend data but the detail view focuses on achievement).
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

- **Sales:** The dashboard uses omzet recognition rules (completed/pipeline); the Sales Report lists Faktur totals — different metrics by design.
- **Piutang & Inventory:** Footer totals should match dashboard KPIs. If they differ, refresh both pages. Persistent mismatch indicates a support escalation.
- **Inventory report footer vs row sum:** Footer groups by item first; row-level Nilai Sediaan sums across warehouses will not necessarily equal the footer.

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
