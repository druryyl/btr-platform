# Portal Connection Configuration Refactor

## Summary

Decoupled SQL Server configuration for `btr.portal.api` from the desktop registry mechanism. Desktop continues to use `HKCU\DrurySoftware\BTRApp`; portal reads exclusively from JSON configuration files.

## Previous Architecture

```
appsettings.json (DatabaseOptions)
        ↓
ConnStringHelper.Get(options)  [static, in btr.infrastructure]
        ↓
Generate() — always reads/writes HKCU\DrurySoftware\BTRApp when IsTest=false
        ↓
100+ DALs → SqlConnection
```

**Problem:** Under IIS, the app pool identity has no meaningful per-user registry. `ConnStringHelper` fell back to placeholder `appsettings.json` values (`ServerName: "SERVER"`) or wrote to the wrong HKCU hive, causing `SqlException` (Named Pipes error 40) on login.

## New Architecture

```
Desktop (btr.distrib)                    Portal (btr.portal.api)
        ↓                                        ↓
RegistryConnectionSettingProvider      JsonConnectionSettingProvider
  (registry + appsettings fallback)      (appsettings only, no registry)
        ↓                                        ↓
        └──────── ConnectionStringFactory ───────┘
                          ↓
              ConnStringHelper.Get() [static facade]
                          ↓
                    DALs unchanged
```

### New Types

| Type | Location | Role |
|------|----------|------|
| `IConnectionSettingProvider` | `btr.infrastructure/Helpers/` | Abstraction for server/database resolution |
| `RegistryConnectionSettingProvider` | `btr.infrastructure/Helpers/` | Desktop: registry read/write with appsettings fallback |
| `JsonConnectionSettingProvider` | `btr.infrastructure/Helpers/` | Portal: reads `DatabaseOptions` from JSON only |
| `ConnectionStringFactory` | `btr.infrastructure/Helpers/` | Centralized connection string template and caching |

## Modified Files

| File | Change |
|------|--------|
| `btr.infrastructure/Helpers/IConnectionSettingProvider.cs` | New interface |
| `btr.infrastructure/Helpers/RegistryConnectionSettingProvider.cs` | New desktop provider |
| `btr.infrastructure/Helpers/JsonConnectionSettingProvider.cs` | New portal provider |
| `btr.infrastructure/Helpers/ConnectionStringFactory.cs` | New shared factory |
| `btr.infrastructure/Helpers/ConnStringHelper.cs` | Refactored to facade over factory |
| `btr.infrastructure/btr.infrastructure.csproj` | Added Compile entries |
| `btr.distrib/Program.cs` | DI registration + `ConnStringHelper.Initialize` |
| `btr.portal.api/Configurations/InfrastructurePortalExtensions.cs` | Portal DI registration |
| `btr.portal.api/Global.asax.cs` | `ConnStringHelper.Initialize` at startup |

## DI Registrations

### Desktop (`btr.distrib/Program.cs`)

```csharp
services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SECTION_NAME));
services.AddSingleton<IConnectionSettingProvider, RegistryConnectionSettingProvider>();
services.AddSingleton<ConnectionStringFactory>();

// In IsSuccessLogin, after BuildServiceProvider:
ConnStringHelper.Initialize(servicesProvider.GetRequiredService<ConnectionStringFactory>());
```

### Portal (`InfrastructurePortalExtensions.cs`)

```csharp
services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SECTION_NAME));
services.AddSingleton<IConnectionSettingProvider, JsonConnectionSettingProvider>();
services.AddSingleton<ConnectionStringFactory>();

// In Global.asax.cs, after DependencyConfig.Configure:
ConnStringHelper.Initialize(serviceProvider.GetRequiredService<ConnectionStringFactory>());
```

## Portal Configuration

Portal uses the existing configuration loader in `Global.asax.cs`:

```csharp
.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
.AddJsonFile($"appsettings.{Environment.MachineName}.json", optional: true)
```

### Base `appsettings.json` (placeholders)

```json
{
  "Database": {
    "ServerName": "SERVER",
    "DbName": "btr",
    "IsTest": false
  }
}
```

### Per-office deployment: `appsettings.{MACHINE_NAME}.json`

Create this file beside the deployed site on each IIS server (machine name must match `Environment.MachineName`):

```json
{
  "Database": {
    "ServerName": "OFFICE-SQL01\\SQLEXPRESS",
    "DbName": "btr",
    "IsTest": false
  }
}
```

Property names follow existing `DatabaseOptions`: `ServerName`, `DbName`, `IsTest` (not `DatabaseName`).

## Registry Convention (Desktop Only)

| Item | Value |
|------|-------|
| Hive | `HKEY_CURRENT_USER` |
| Path | `DrurySoftware\BTRApp` |
| Keys | `Server`, `Database` |

Portal does **not** read or write these keys.

## Deployment Impact

- **Desktop:** No deployment changes. ClickOnce and registry behavior unchanged.
- **Portal:** Each IIS server requires `appsettings.{MACHINE_NAME}.json` with correct `Database:ServerName` and `Database:DbName` before login will succeed.
- **SQL credentials:** Unchanged (`btrLogin` embedded in `ConnectionStringFactory`). App pool must reach SQL Server with network access to the configured instance.
- **JWT / CORS:** Unaffected; configure separately in the same JSON files.

## Verification Steps

### Desktop

1. Run ClickOnce or local build; login succeeds.
2. Confirm `HKCU\DrurySoftware\BTRApp` contains `Server` and `Database` after first connection.
3. MainForm status bar shows `Connected Database: {db}@{server}`.
4. Login screen shows registry values via `Program.GetServerDb()`.

### Portal (IIS)

1. Deploy with `appsettings.{MACHINE_NAME}.json` containing real SQL server and database names.
2. App pool starts without SQL errors.
3. `POST /api/auth/login` returns a JWT token.
4. No dependency on developer registry under app pool identity.

### Tests

- `btr.test` projects using `IsTest = true` continue to work via `ConnStringHelper` fallback (no explicit `Initialize` required).

## Backward Compatibility

- All DALs still call `ConnStringHelper.Get(_opt)` — no DAL changes.
- `ConnStringHelper.ReadFromRegistry()` delegates to `RegistryConnectionSettingProvider.ReadFromRegistry()`.
- `ConnStringHelper.GetTestOptions()` unchanged for integration tests.
- When `Initialize` is not called (tests), fallback factory selects provider based on `IsTest` flag.
