# BTR Portal Milestone History (M1-M15)

## Project Vision

BTR Portal is a web-based Reporting and Dashboard application for BTR.

Scope:

- Read-only
- Reporting
- Dashboard
- Management analytics

Out of scope:

- Sales Order entry
- Inventory transactions
- Purchasing transactions
- Master data maintenance
- Any operational transaction currently handled by BTR Desktop

Technology:

Backend:

- [ASP.NET](http://ASP.NET) Web API 2 (.NET Framework 4.8)
- Existing BTR Application Layer
- Existing BTR Infrastructure Layer
- MediatR
- Dapper

Frontend:

- Vue 3
- TypeScript
- PrimeVue
- Pinia
- Axios

Architecture:

Vue Portal  
↓  
btr.portal.api  
↓  
Application Layer  
↓  
Infrastructure Layer  
↓  
SQL Server

---

# M1 — Portal API Foundation

Goal:

Create technical foundation for BTR Portal API.

Delivered:

- btr.portal.api
- Dependency Injection
- MediatR integration
- Global exception handling
- ApiResponse
- Logging
- CORS
- Health endpoint

Result:

Portal API can run independently.

---

# M2 — Authentication Foundation

Goal:

Secure the portal using existing BTR users.

Delivered:

- JWT Authentication
- AuthController
- Login endpoint
- JWT validation filter
- Integration with BTR_User

Result:

Users can authenticate and receive JWT tokens.

---

# M3 — Reporting Context Foundation

Goal:

Create reporting architecture for future dashboard and report features.

Delivered:

ReportingContext:

- DashboardSalesAgg
- DashboardPiutangAgg
- DashboardInventoryAgg

Using:

- Controller
- MediatR
- Application Layer
- Infrastructure Layer

Result:

Reporting features have dedicated architecture.

---

# M4 — Sales Dashboard V1

Goal:

Expose first management KPI.

Delivered:

GET /api/dashboard/sales

Metrics:

- Total Omzet
- Total Faktur
- Total Customer

Result:

Real sales KPI available through API.

---

# M5 — Piutang Dashboard V1

Goal:

Expose receivable KPI.

Delivered:

GET /api/dashboard/piutang

Metrics:

- Total Piutang
- Total Customer

Result:

Real piutang KPI available through API.

---

# M6 — Inventory Dashboard V1

Goal:

Expose inventory KPI.

Delivered:

GET /api/dashboard/inventory

Metrics:

- Total Inventory Value
- Total Item

Result:

Real inventory KPI available through API.

---

# M7 — Frontend Foundation

Goal:

Create the first usable web portal.

Delivered:

- Vue 3 application
- Login page
- JWT persistence
- Route protection
- Main layout
- Sidebar
- Dashboard home
- KPI cards

Result:

End-to-end portal operational.

---

# M8 — Sales Dashboard V2

Goal:

Enhance Sales Dashboard with trend visualization.

Delivered:

Sales Dashboard enhancements:

- Completed Omzet
- Pipeline Omzet
- Weekly Trend

Frontend:

- Sales Trend Card
- Weekly Omzet Trend Chart

Reuse:

- SalesOmzetChartSummaryBuilder
- Existing desktop sales chart logic

Result:

Dashboard approved by management after user review.

---

# M9 — Sales Report V1

Goal:

Create first operational report page.

Delivered:

- Sales Report page
- Sales report API
- PrimeVue DataTable
- Pagination
- Existing Faktur reporting logic reuse

Purpose:

Allow users to validate dashboard numbers against underlying transactions.

Result:

Dashboard KPI can be traced to actual sales transactions.

---

# M10 — Piutang Report V1

Goal:

Create receivable report page.

Expected Deliverables:

- Piutang Report page
- Piutang Report API
- Existing piutang reporting reuse
- DataTable presentation

Purpose:

Allow users to inspect receivable details behind dashboard KPI.

---

# M11 — Inventory Report V1

Goal:

Create inventory report page.

Expected Deliverables:

- Inventory Report page
- Inventory Report API
- Existing stock reporting reuse
- DataTable presentation

Purpose:

Allow users to inspect inventory details behind dashboard KPI.

---

# M12 — Purchasing Report V1

Goal:

Create purchasing report page.

Expected Deliverables:

- Purchasing Report page
- Purchasing Report API
- Existing purchasing reporting reuse
- DataTable presentation

Purpose:

Provide visibility into purchasing activities.

---

# M13 — Sales Dashboard V3

Goal:

Enhance sales analytics.

Expected Deliverables:

Target vs Achievement

Sales Ranking

Potential Features:

- Salesman ranking
- Achievement percentage
- Monthly target comparison
- Management performance dashboard

Purpose:

Provide management-level sales analytics.

---

# M14 — Piutang Dashboard V2

Goal:

Enhance receivable analytics.

Expected Deliverables:

Piutang Aging

Top Customer

Potential Features:

- Aging buckets
- Largest outstanding customers
- Collection monitoring
- Receivable distribution analysis

Purpose:

Improve finance monitoring and collection management.

---

# M15 — Inventory Dashboard V2

Goal:

Enhance inventory analytics.

Expected Deliverables:

Category Analysis

Supplier Analysis

Potential Features:

- Inventory by category
- Inventory by supplier
- Inventory composition
- Inventory concentration analysis

Purpose:

Improve inventory planning and purchasing decisions.

---

# Important Architectural Rules

1. Reuse existing BTR reporting logic whenever possible.
2. Do not duplicate business calculations already implemented in Desktop.
3. Prefer existing DALs, Builders, Policies, Aggregates, and Reporting screens.
4. Keep portal read-only.
5. Use ReportingContext for all new reporting features.
6. Keep Controllers thin.
7. Use MediatR between Controllers and Infrastructure.
8. Avoid introducing new SQL when equivalent reporting logic already exists.
9. Dashboard metrics must be traceable to report data.
10. Portal is an analytics/reporting product, not a transactional product.