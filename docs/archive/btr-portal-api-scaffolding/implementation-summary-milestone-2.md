# Implementation Summary: BTR Portal API — Milestone 2 (Authentication)

## Status

Milestone 2 is complete. Phase 3 (Authentication) is implemented in `btr.portal.api`. The project builds, runs under IIS Express, and all Milestone 2 verification checks pass.

## Scope Delivered

### Phase 3 — Authentication

| Step | Deliverable | Status |
| --- | --- | --- |
| 3.1 | `Infrastructure/JwtOptions.cs` — bound from `appsettings.json` `Jwt` section | Done |
| 3.2 | `Infrastructure/JwtTokenService.cs` — create + validate tokens | Done |
| 3.3 | `Filters/JwtAuthenticationFilter.cs` — Bearer validation for `[Authorize]` routes | Done |
| 3.4 | `Controllers/AuthController.cs` — `POST /api/auth/login` using `IUserDal` + `HashSha256` | Done |
| 3.5 | Invalid credentials return HTTP 401 with `ApiResponse` envelope | Done |
| DI | Register `IUserDal`, `UserDal`, `IJwtTokenService` / `JwtTokenService` | Done |
| DI | `ServiceProviderControllerActivator` for constructor-injected controllers | Done |

### Supporting files

| File | Purpose |
| --- | --- |
| `Models/LoginRequest.cs` | Login request body (`UserId`, `Password`) |
| `Models/LoginResponse.cs` | Login success payload (`Token`, `ExpiresAt`, `User`) |
| `Infrastructure/ServiceProviderControllerActivator.cs` | Web API 2 DI controller activation |

### Verification helper (not in original endpoint table)

| Route | Auth | Purpose |
| --- | --- | --- |
| `GET /api/auth/me` | JWT | Returns current user from token claims; used to verify `JwtAuthenticationFilter` |

### Explicitly excluded (per Milestone 2 constraints)

- `ReportingContext` in Application/Infrastructure
- Dashboard controllers and queries
- Dashboard DALs
- Report logic
- LoginForm backdoor password (`jude777` / GOD MODE)

## Project Layout (additions)

```text
src/j05-btr-distrib/btr.portal.api/
├── Controllers/AuthController.cs
├── Filters/JwtAuthenticationFilter.cs
├── Infrastructure/
│   ├── JwtOptions.cs
│   ├── JwtTokenService.cs
│   └── ServiceProviderControllerActivator.cs
└── Models/
    ├── LoginRequest.cs
    └── LoginResponse.cs
```

## Authentication Flow

1. Client `POST /api/auth/login` with `{ "UserId": "...", "Password": "..." }`.
2. `AuthController` loads user via `IUserDal.GetData(new UserModel(userId))` (same as `LoginForm`).
3. Password hashed with `HashSha256()` from `btr.nuna.Domain`.
4. Hash compared to `user.Password` (no backdoor path).
5. On success, `JwtTokenService` issues HS256 JWT with claims: `userId`, `userName`, `roleId`, `roleName`, plus `sub` and `name`.
6. `JwtAuthenticationFilter` validates Bearer tokens on actions/controllers marked `[Authorize]`.
7. `HealthController` and `POST /api/auth/login` remain anonymous via `[AllowAnonymous]`.

## Example JWT Payload

Decoded payload for user `DIMAS` (claims inside the signed token):

```json
{
  "userId": "DIMAS",
  "userName": "DIMAS",
  "roleId": "FAKTR",
  "roleName": "Admin Faktur Penjualan",
  "sub": "DIMAS",
  "name": "DIMAS",
  "nbf": 1780680707,
  "exp": 1780709507,
  "iss": "btr-portal-api",
  "aud": "btr-portal-vue"
}
```

Expiry is controlled by `Jwt:ExpiryMinutes` (480 minutes in base `appsettings.json`).

## Sample API Responses

### Successful login (HTTP 200)

```json
{
  "Status": "success",
  "Code": 200,
  "Message": null,
  "Data": {
    "Token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "ExpiresAt": "2026-06-06T01:31:47.3516574Z",
    "User": {
      "UserId": "DIMAS",
      "UserName": "DIMAS",
      "RoleId": "FAKTR",
      "RoleName": "Admin Faktur Penjualan"
    }
  }
}
```

### Invalid credentials (HTTP 401)

```json
{
  "Status": "error",
  "Code": 401,
  "Message": "Invalid credentials",
  "Data": null
}
```

Note: JSON property names use PascalCase (Web API default serializer), matching Milestone 1 health responses.

## Verification Results

| # | Check | Result |
| --- | --- | --- |
| 1 | Valid `BTR_User` login returns JWT | Pass — `DIMAS` / `1111` on dev DB (`JUDE7` / `btr_yk`) |
| 2 | Invalid password returns HTTP 401 | Pass — `DIMAS` / `wrongpassword` → 401 + envelope |
| 3 | JWT validated by `JwtAuthenticationFilter` | Pass — `GET /api/auth/me` with Bearer token → 200; without token → 401 |
| 4 | Health endpoint remains anonymous | Pass — `GET /api/health` → 200 without auth |
| 5 | Milestone 1 functionality operational | Pass — health envelope unchanged; full solution builds |

Test commands (IIS Express on port 5051):

```powershell
# Health (anonymous)
curl.exe http://localhost:5051/api/health

# Invalid login
curl.exe -X POST http://localhost:5051/api/auth/login `
  -H "Content-Type: application/json" `
  -d "{\"UserId\":\"DIMAS\",\"Password\":\"wrong\"}"

# Valid login (use real BTR_User credentials from your database)
curl.exe -X POST http://localhost:5051/api/auth/login `
  -H "Content-Type: application/json" `
  -d "{\"UserId\":\"DIMAS\",\"Password\":\"1111\"}"

# JWT-protected route
curl.exe http://localhost:5051/api/auth/me -H "Authorization: Bearer <token>"
```

The plan’s example `ADMIN` / `password` is illustrative; actual credentials depend on rows in `BTR_User`.

## Issues Encountered

| # | Issue | Resolution |
| --- | --- | --- |
| 1 | `AuthController` failed at runtime — “does not have a default constructor” | Added `ServiceProviderControllerActivator` implementing `IHttpControllerActivator`; registered in `WebApiConfig`; registered `AuthController` in DI |
| 2 | `btr.domain` types not available in portal project | Added direct project reference to `btr.domain` (same pattern as Milestone 1 `btr.nuna` reference) |
| 3 | `JwtTokenService` initially depended on `UserModel` | Refactored `GenerateToken` to accept primitive claim fields to keep infrastructure self-contained |
| 4 | Initial valid-login verification used wrong password | Plan example `password` does not match all users; verified against actual `BTR_User` hash (`DIMAS` → `1111`) |
| 5 | Port 5050 already in use by prior IIS Express instance | Used port 5051 for verification |

## Remaining Work (Milestone 3+)

- Phase 4 — Reporting Context scaffolding (`IDashboard*Dal`, placeholder queries/DALs)
- Phase 5 — Dashboard controllers with `[Authorize]`
- Phase 6 — IIS publish profile and deployment verification
- Replace `Jwt:Key` placeholder with a strong server-specific secret before production

## Local Run Instructions

```powershell
# Build
& "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe" `
  "src\j05-btr-distrib\btr.portal.api\btr.portal.api.csproj" /p:Configuration=Debug

# Run (IIS Express)
& "C:\Program Files\IIS Express\iisexpress.exe" `
  /path:"src\j05-btr-distrib\btr.portal.api" /port:5050

# Verify health + login
curl.exe http://localhost:5050/api/health
curl.exe -X POST http://localhost:5050/api/auth/login `
  -H "Content-Type: application/json" `
  -d "{\"UserId\":\"<your-user>\",\"Password\":\"<your-password>\"}"
```

Ensure `Database` settings resolve correctly for IIS (registry `DrurySoftware\BTRApp` or `appsettings.{MACHINE}.json` overrides per implementation plan Section 11.2).
