# Implementation Summary: BTR Portal API — Milestone 3 (ReportingContext & Dashboard Skeleton)

## Status

Milestone 3 is complete. Phase 4 (Reporting Context Scaffolding) and Phase 5.2–5.4 (Dashboard Controllers) are implemented. The full solution builds, dashboard endpoints are JWT-protected, and authenticated requests return placeholder data via the MediatR → DAL pipeline.

## Scope Delivered

### Phase 4 — Reporting Context Scaffolding

| Step | Deliverable | Status |
| --- | --- | --- |
| 4.1 | Application `ReportingContext` query/handler placeholders (3 dashboards) | Done |
| 4.2 | Application DAL contracts (`IDashboard*Dal`) | Done |
| 4.3 | Infrastructure `ReportingContext` placeholder DALs | Done |
| 4.4 | Register ReportingContext DALs in `InfrastructurePortalExtensions` | Done |

### Phase 5.2–5.4 — Dashboard Controllers

| Step | Deliverable | Status |
| --- | --- | --- |
| 5.2 | `SalesDashboardController` — `GET /api/dashboard/sales` | Done |
| 5.3 | `PiutangDashboardController` — `GET /api/dashboard/piutang` | Done |
| 5.4 | `InventoryDashboardController` — `GET /api/dashboard/inventory` | Done |

### Explicitly excluded (per Milestone 3 constraints)

- SQL Server queries in dashboard DALs
- Reuse of existing `*Rpt` / `*ViewDal` report DALs
- Dashboard calculations or chart data
- Report endpoints
- Changes to authentication (Milestone 2 behavior preserved)
- Changes to health endpoint (Milestone 1 behavior preserved)

---

## 1. Folder Structure Created

### Application (`btr.application/ReportingContext/`)

```text
ReportingContext/
├── DashboardSalesAgg/
│   ├── Contracts/
│   │   └── IDashboardSalesDal.cs
│   └── Queries/
│       └── GetDashboardSalesQuery.cs
├── DashboardPiutangAgg/
│   ├── Contracts/
│   │   └── IDashboardPiutangDal.cs
│   └── Queries/
│       └── GetDashboardPiutangQuery.cs
└── DashboardInventoryAgg/
    ├── Contracts/
    │   └── IDashboardInventoryDal.cs
    └── Queries/
        └── GetDashboardInventoryQuery.cs
```

### Infrastructure (`btr.infrastructure/ReportingContext/`)

```text
ReportingContext/
├── DashboardSalesAgg/
│   └── DashboardSalesDal.cs
├── DashboardPiutangAgg/
│   └── DashboardPiutangDal.cs
└── DashboardInventoryAgg/
    └── DashboardInventoryDal.cs
```

### Portal API (`btr.portal.api/Controllers/Dashboard/`)

```text
Controllers/Dashboard/
├── SalesDashboardController.cs
├── PiutangDashboardController.cs
└── InventoryDashboardController.cs
```

---

## 2. Query / Handler Classes Created

| Aggregate | Query | Handler | Response DTO |
| --- | --- | --- | --- |
| `DashboardSalesAgg` | `GetDashboardSalesQuery` | `GetDashboardSalesHandler` | `DashboardSalesResponse` |
| `DashboardPiutangAgg` | `GetDashboardPiutangQuery` | `GetDashboardPiutangHandler` | `DashboardPiutangResponse` |
| `DashboardInventoryAgg` | `GetDashboardInventoryQuery` | `GetDashboardInventoryHandler` | `DashboardInventoryResponse` |

Each handler:

- Implements `IRequestHandler<TQuery, TResponse>`
- Injects the corresponding `IDashboard*Dal` contract
- Delegates to `_dal.GetPlaceholder()` with no business logic

Each response DTO defaults `Status` to `"not_implemented"`.

MediatR auto-registers handlers via `AddMediatR` in `ApplicationPortalExtensions` (assembly scan on `btr.application`).

---

## 3. DI Registrations Added

### `InfrastructurePortalExtensions.cs`

```csharp
services.AddScoped<IDashboardSalesDal, DashboardSalesDal>();
services.AddScoped<IDashboardPiutangDal, DashboardPiutangDal>();
services.AddScoped<IDashboardInventoryDal, DashboardInventoryDal>();
```

### `PortalPresentationExtensions.cs`

```csharp
services.AddTransient<Controllers.Dashboard.SalesDashboardController>();
services.AddTransient<Controllers.Dashboard.PiutangDashboardController>();
services.AddTransient<Controllers.Dashboard.InventoryDashboardController>();
```

Dashboard controllers require explicit DI registration (same pattern as `AuthController`) because `ServiceProviderControllerActivator` resolves constructor-injected controllers from the service provider.

---

## 4. Verification Results

| # | Check | Result |
| --- | --- | --- |
| 1 | Anonymous `GET /api/dashboard/*` returns HTTP 401 | Pass |
| 2 | Authenticated `GET /api/dashboard/*` returns HTTP 200 | Pass |
| 3 | Response flows through MediatR handler | Pass — controllers call `_mediator.Send(...)` only |
| 4 | Response originates from Infrastructure placeholder DAL | Pass — `Data.Status` is `"not_implemented"` |
| 5 | Full solution build (`j05-btr-distrib.sln`) | Pass |
| 6 | Milestone 1 health endpoint unchanged | Pass — `GET /api/health` → 200 anonymous |
| 7 | Milestone 2 auth unchanged | Pass — login + JWT filter still operational |
| 8 | No runtime DI errors on dashboard requests | Pass |

Test commands (IIS Express on port 5052):

```powershell
# Anonymous (401)
curl.exe http://localhost:5052/api/dashboard/sales

# Login
curl.exe -s -X POST http://localhost:5052/api/auth/login `
  -H "Content-Type: application/json" `
  --data-raw '{"UserId":"DIMAS","Password":"1111"}'

# Authenticated (200 + placeholder)
curl.exe http://localhost:5052/api/dashboard/sales -H "Authorization: Bearer <token>"
curl.exe http://localhost:5052/api/dashboard/piutang -H "Authorization: Bearer <token>"
curl.exe http://localhost:5052/api/dashboard/inventory -H "Authorization: Bearer <token>"
```

---

## 5. Example API Responses

### Anonymous request (HTTP 401)

```json
{
  "Status": "error",
  "Code": 401,
  "Message": "Missing or invalid authorization token.",
  "Data": null
}
```

### Authenticated sales dashboard (HTTP 200)

```json
{
  "Status": "success",
  "Code": 200,
  "Message": null,
  "Data": {
    "Status": "not_implemented"
  }
}
```

Piutang and inventory endpoints return the same envelope shape with `"Status": "not_implemented"` in `Data`.

---

## 6. Remaining Work for Milestone 4

Milestone 4 covers **Phase 6 — IIS Deployment Prep** and **Phase 7 — Verification** from the implementation plan, plus the first real dashboard business logic.

### Deployment & operations

- [ ] Create IIS publish profile (Folder, Release) → `publish/btr-portal-api/`
- [ ] Document `appsettings.{MACHINE}.json` template for production servers
- [ ] Deploy to local IIS; run post-deploy checklist (all 5+ endpoints)
- [ ] Confirm SQL connectivity from IIS app pool identity
- [ ] Implement `PortalConnStringFactory` for IIS-friendly database config (plan Section 11.2)
- [ ] Replace `Jwt:Key` placeholder with a strong server-specific secret

### Dashboard business logic (post-scaffold)

- [ ] Wire `DashboardSalesDal` to existing sales reporting DALs (`SalesOmzetAgg`, `FakturViewDal`, etc.)
- [ ] Wire `DashboardPiutangDal` to `PIutangLunasViewDal`, `PiutangSalesWilayahDal`, etc.
- [ ] Wire `DashboardInventoryDal` to `StokBalanceViewDal`, `KartuStokRpt`, etc.
- [ ] Expand response DTOs with real chart/summary fields
- [ ] Add query parameters (date range, warehouse, sales person) as needed
- [ ] Vue portal frontend (out of scope for API milestones)

### Optional follow-up

- [ ] API smoke tests in `btr.test`
- [ ] DI startup self-check endpoint
- [ ] Address NuGet vulnerability advisories on JWT / System.Text.Json packages

---

## Local Run Instructions

```powershell
# Build
& "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe" `
  "src\j05-btr-distrib\j05-btr-distrib.sln" /p:Configuration=Debug

# Run (IIS Express)
& "C:\Program Files\IIS Express\iisexpress.exe" `
  /path:"src\j05-btr-distrib\btr.portal.api" /port:5050

# Verify
curl.exe http://localhost:5050/api/health
curl.exe http://localhost:5050/api/dashboard/sales   # expect 401
curl.exe -s -X POST http://localhost:5050/api/auth/login `
  -H "Content-Type: application/json" `
  --data-raw '{"UserId":"<user>","Password":"<password>"}'
curl.exe http://localhost:5050/api/dashboard/sales -H "Authorization: Bearer <token>"
```
