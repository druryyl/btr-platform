# Implementation Summary: BTR Portal API — Milestone 1

## Status

Milestone 1 is complete. The `btr.portal.api` project builds, starts under IIS Express, and `GET /api/health` returns HTTP 200 with the standard JSON envelope.

## Scope Delivered

### Phase 1 — Project Shell

| Step | Deliverable | Status |
| --- | --- | --- |
| 1.1 | `btr.portal.api` ASP.NET Web API 2 project (.NET 4.8) | Done |
| 1.2 | Added to `j05-btr-distrib.sln` under `backend` folder | Done |
| 1.3 | Project references: `btr.application`, `btr.infrastructure`, `btr.nuna` | Done |
| 1.4 | NuGet packages installed via `packages.config` | Done |
| 1.5 | `appsettings.json`, `NLog.config`, `Web.config` | Done |

### Phase 2 — Cross-Cutting Infrastructure

| Step | Deliverable | Status |
| --- | --- | --- |
| 2.1 | `Models/ApiResponse.cs` | Done |
| 2.2 | `Filters/GlobalExceptionFilter.cs` + registered in `WebApiConfig` | Done |
| 2.3 | `Infrastructure/ServiceProviderDependencyResolver.cs` | Done |
| 2.4 | `DependencyConfig` + Application/Infrastructure/Presentation extensions | Done |
| 2.5 | CORS from `appsettings.json` | Done |
| 2.6 | NLog configuration (startup + health request logging) | Done |

### Phase 5.1

| Step | Deliverable | Status |
| --- | --- | --- |
| 5.1 | `Controllers/HealthController.cs` — `GET /api/health` | Done |

### Explicitly Excluded (per Milestone 1 constraints)

- JWT authentication (`JwtTokenService`, `JwtAuthenticationFilter`, `JwtOptions`)
- `AuthController`
- `ReportingContext` in Application/Infrastructure
- Dashboard controllers and queries

## Project Layout

```text
src/j05-btr-distrib/btr.portal.api/
├── App_Start/WebApiConfig.cs
├── Configurations/
│   ├── ApplicationPortalExtensions.cs
│   ├── DependencyConfig.cs
│   ├── InfrastructurePortalExtensions.cs
│   └── PortalPresentationExtensions.cs
├── Controllers/HealthController.cs
├── Filters/GlobalExceptionFilter.cs
├── Infrastructure/ServiceProviderDependencyResolver.cs
├── Models/ApiResponse.cs
├── Global.asax / Global.asax.cs
├── Web.config
├── appsettings.json
├── NLog.config
├── packages.config
└── btr.portal.api.csproj
```

## Verification Results

| Check | Result |
| --- | --- |
| Full solution build (`j05-btr-distrib.sln`) | Pass |
| `btr.portal.api` starts (IIS Express, port 5050) | Pass |
| `GET /api/health` | HTTP 200 |
| Response envelope | `{"Status":"success","Code":200,"Message":null,"Data":{...}}` |
| NLog file written | `logs/btr-portal-api-{date}.log` in project root |

### Sample health response

```json
{
  "Status": "success",
  "Code": 200,
  "Message": null,
  "Data": {
    "Status": "ok",
    "Service": "btr.portal.api",
    "Version": "1.0.0.0",
    "TimestampUtc": "2026-06-05T17:16:36.8436106Z"
  }
}
```

## Compile / Build Issues Encountered

| # | Issue | Resolution |
| --- | --- | --- |
| 1 | `btr.nuna` types not resolved in portal configuration classes | Added direct project reference to `btr.nuna` (types used in DI extensions are not re-exported through application/infrastructure references alone) |
| 2 | `AddPortalPresentation(configuration)` signature mismatch | `PortalPresentationExtensions` is a no-op for M1; call changed to parameterless `AddPortalPresentation()` |
| 3 | Missing `using` for `KeyNotFoundException` in `GlobalExceptionFilter` | Added `using System.Collections.Generic` |
| 4 | Missing `using btr.nuna.Application` in `InfrastructurePortalExtensions` | Added for `INunaCounterDal`, `ISaveChange<>`, etc. |
| 5 | `Microsoft.Extensions.Options` version conflict (7.0.0 vs 7.0.1) | Aligned portal package to 7.0.1; added binding redirect in `Web.config` |
| 6 | Runtime `FileNotFoundException` for `System.Web.Cors` | `Microsoft.AspNet.WebApi.Cors` does not include `System.Web.Cors.dll`; added separate `Microsoft.AspNet.Cors` 5.2.9 package and corrected `HintPath` |
| 7 | NuGet vulnerability warnings on install | Informational only for M1: `System.IdentityModel.Tokens.Jwt` 7.0.0, `System.Text.Json` 7.0.0 (JWT packages installed for future milestones, not used yet) |

## Remaining Work for Milestone 2

Milestone 2 covers **Phase 3 (Authentication)** and **Phase 4 (Reporting Context Scaffolding)** from the implementation plan, plus dashboard controllers (Phase 5.2–5.4).

### Phase 3 — Authentication

- [ ] `Infrastructure/JwtOptions.cs` — bind from `appsettings.json` `Jwt` section
- [ ] `Infrastructure/JwtTokenService.cs` — create and validate tokens
- [ ] `Filters/JwtAuthenticationFilter.cs` — Bearer token validation on protected routes
- [ ] `Controllers/AuthController.cs` — `POST /api/auth/login` using `IUserDal` + `HashSha256`
- [ ] Register `IUserDal` / `UserDal` and `IJwtTokenService` in `PortalPresentationExtensions`
- [ ] Verify invalid credentials return 401 with envelope

### Phase 4 — Reporting Context Scaffolding

**Application** (`btr.application/ReportingContext/`):

- [ ] `DashboardSalesAgg/Contracts/IDashboardSalesDal.cs`
- [ ] `DashboardSalesAgg/Queries/GetDashboardSalesQuery.cs`
- [ ] `DashboardPiutangAgg/Contracts/IDashboardPiutangDal.cs`
- [ ] `DashboardPiutangAgg/Queries/GetDashboardPiutangQuery.cs`
- [ ] `DashboardInventoryAgg/Contracts/IDashboardInventoryDal.cs`
- [ ] `DashboardInventoryAgg/Queries/GetDashboardInventoryQuery.cs`

**Infrastructure** (`btr.infrastructure/ReportingContext/`):

- [ ] `DashboardSalesAgg/DashboardSalesDal.cs` (placeholder)
- [ ] `DashboardPiutangAgg/DashboardPiutangDal.cs` (placeholder)
- [ ] `DashboardInventoryAgg/DashboardInventoryDal.cs` (placeholder)
- [ ] Register DALs in `InfrastructurePortalExtensions`

### Phase 5 — Dashboard Controllers

- [ ] `Controllers/Dashboard/SalesDashboardController.cs` — `GET /api/dashboard/sales`
- [ ] `Controllers/Dashboard/PiutangDashboardController.cs` — `GET /api/dashboard/piutang`
- [ ] `Controllers/Dashboard/InventoryDashboardController.cs` — `GET /api/dashboard/inventory`
- [ ] `Models/LoginRequest.cs`, `Models/LoginResponse.cs`

### Optional / Follow-up

- [ ] `Infrastructure/PortalConnStringFactory.cs` for IIS-friendly database config (documented in plan Section 11.2)
- [ ] IIS publish profile and deployment verification (Phase 6)
- [ ] Address NuGet vulnerability advisories when upgrading JWT / System.Text.Json packages

## Local Run Instructions

```powershell
# Build
& "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe" `
  "src\j05-btr-distrib\btr.portal.api\btr.portal.api.csproj" /p:Configuration=Debug

# Run (IIS Express)
& "C:\Program Files\IIS Express\iisexpress.exe" `
  /path:"src\j05-btr-distrib\btr.portal.api" /port:5050

# Verify
Invoke-WebRequest http://localhost:5050/api/health -UseBasicParsing
```
