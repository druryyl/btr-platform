# Microsoft.SqlServer.Types Usage Report

**Date:** 2025-06-05  
**Scope:** Entire `btr-platform` repository (all solutions/projects)

---

## Executive Summary

**`Microsoft.SqlServer.Types` is not used directly by application code.** There are no references to `SqlGeography`, `SqlGeometry`, spatial SQL, or map report elements anywhere in the codebase.

It **is required indirectly** because `btr.distrib` uses **ReportViewer** for RDLC printouts, and `Microsoft.ReportViewer.Common` has a compile-time dependency on the managed `Microsoft.SqlServer.Types.dll`.

The **`SqlServerTypes` native folder and `LoadNativeAssemblies` initialization are not used** and appear unnecessary for current reports.

---

## 1. Search Results

### C# / Project References

| Pattern | Matches | Notes |
|---------|---------|-------|
| `Microsoft.SqlServer.Types` | 2 | `btr.distrib.csproj`, `packages.config` only |
| `SqlServerTypes.Utilities` | 3 | `readme.htm` examples only |
| `LoadNativeAssemblies` | 4 | `Loader.cs` definition + `readme.htm` examples |
| `SqlGeography` | 0 | — |
| `SqlGeometry` | 0 | — |
| `using Microsoft.SqlServer` | 0 | — |

**Only project with SqlServer.Types:** `src/j05-btr-distrib/btr.distrib/`

No other solution (`j06-pkl-btrade-api`, `j07-btr-gudang`, `j07-btrade-sync`, `BTrade3`) references ReportViewer or SqlServer.Types.

### SQL Scripts and Embedded Queries

Searched 104 `.sql` files plus all `.cs` DAL/query code:

| Pattern | Matches |
|---------|---------|
| `geography` | 0 |
| `geometry` | 0 |
| `STDistance` | 0 |
| `STIntersects` | 0 |
| `STContains` | 0 |

### RDLC Files (13 total)

| Pattern | Matches |
|---------|---------|
| `Map` | 0 |
| `MapLayer` | 0 |
| `MapViewport` | 0 |
| `<Chart`, `<Map`, `<Gauge` | 0 |

All 13 reports use **Tablix** and **Textbox** only (invoice, faktur, tagihan, retur, mutasi printouts).

---

## 2. Direct Usages

**None in application logic.**

Infrastructure present only because the NuGet package was installed:

- **Project reference:** `src/j05-btr-distrib/btr.distrib/btr.distrib.csproj` (lines 167–169)
- **Package:** `src/j05-btr-distrib/btr.distrib/packages.config` — `Microsoft.SqlServer.Types` v14.0.314.76
- **Loader:** `src/j05-btr-distrib/btr.distrib/SqlServerTypes/Loader.cs` — defines `LoadNativeAssemblies`, never called
- **Native DLLs:** `SqlServerTypes/x86` and `SqlServerTypes/x64` copied to output via `CopyToOutputDirectory=PreserveNewest`
- **Readme:** `src/j05-btr-distrib/btr.distrib/SqlServerTypes/readme.htm` — NuGet boilerplate documentation

`Program.cs` has no initialization for `LoadNativeAssemblies`.

---

## 3. Indirect Usages

### ReportViewer (Sole Consumer)

`btr.distrib` references:

- `Microsoft.ReportViewer.WinForms` v15.0
- `Microsoft.ReportViewer.Common` v15.0
- `Microsoft.ReportViewer.DataVisualization` v15.0
- `Microsoft.ReportViewer.ProcessingObjectModel` v15.0
- `Microsoft.ReportViewer.Design` v15.0

**Forms using ReportViewer:**

| Form | Usage |
|------|-------|
| `RdlcViewerForm` | Dynamic RDLC from `Reports/` folder |
| `FakturPrintOutForm` | Embedded RDLC |
| `TagihanPrintOutForm` | Embedded RDLC |
| `InvoicePrintOutForm` | Embedded RDLC |
| `ReturJualPrintOutForm` | Embedded RDLC |

**Callers of `RdlcViewerForm`:** `FakturForm`, `FakturControlForm`, `InvoiceForm`, `ReturBeliForm`, `ReturJualForm`, `TagihanForm`, `MutasiForm`.

`Microsoft.ReportViewer.Common` has a compile-time dependency on `Microsoft.SqlServer.Types` (for map/spatial report processing). That dependency exists even when reports are plain tabular RDLC — a common cause of `FileNotFoundException` for `Microsoft.SqlServer.Types` if the managed DLL is missing at deploy time.

---

## 4. ReportViewer Dependencies

```
btr.distrib (WinForms app)
  └── Microsoft.ReportingServices.ReportViewerControl.Winforms 150.1652.0
        ├── Microsoft.ReportViewer.WinForms.dll
        ├── Microsoft.ReportViewer.Common.dll  ──► requires Microsoft.SqlServer.Types.dll (managed)
        ├── Microsoft.ReportViewer.DataVisualization.dll
        ├── Microsoft.ReportViewer.ProcessingObjectModel.dll
        └── Microsoft.ReportViewer.Design.dll

  └── Microsoft.SqlServer.Types 14.0.314.76 (installed separately)
        ├── Microsoft.SqlServer.Types.dll          → copied to output via reference
        ├── SqlServerTypes/Loader.cs               → compiled, never called
        ├── SqlServerTypes/x86/SqlServerSpatial140.dll  → copied to output
        ├── SqlServerTypes/x64/SqlServerSpatial140.dll  → copied to output
        └── SqlServerTypes/x86|x64/msvcr120.dll         → copied to output
```

No binding redirect for `Microsoft.SqlServer.Types` in `App.config` (v14 referenced directly).

---

## 5. Is SqlServerTypes Required at Runtime?

| Component | Required? | Reason |
|-----------|-----------|--------|
| **`Microsoft.SqlServer.Types.dll` (managed)** | **Yes** | ReportViewer.Common assembly reference; needed to render local RDLC reports |
| **`SqlServerSpatial140.dll` (native x86/x64)** | **No** (current app) | Only for `SqlGeography`/`SqlGeometry` or map reports |
| **`msvcr120.dll` (native)** | **No** (current app) | Support DLL for native spatial loader |
| **`LoadNativeAssemblies()` call** | **No** (current app) | Never invoked; readme says desktop apps need it only *"before any spatial operations"* |

**Practical runtime requirement:** deploy `Microsoft.SqlServer.Types.dll` next to `btr.distrib.exe` (and other ReportViewer DLLs). The app already does this via the project reference.

---

## 6. Deployment / Initialization

### Does the `SqlServerTypes` folder only need to be deployed?

**Partially.**

- **Managed DLL** (`Microsoft.SqlServer.Types.dll`): must be deployed; MSBuild copies it automatically.
- **Native `SqlServerTypes\x86` / `SqlServerTypes\x64` folders**: deployed today (`CopyToOutputDirectory=PreserveNewest`) but **not needed** for current tabular-only reports with no spatial data.
- **No explicit initialization is implemented or required** for current usage. `LoadNativeAssemblies` is documented in `readme.htm` but not called from `Program.cs` or anywhere else.

### Can initialization be skipped?

**Yes, for this codebase.** With no spatial types, no map RDLC elements, and no spatial SQL, native DLL loading is unnecessary. Reports should work with only the managed `Microsoft.SqlServer.Types.dll` present.

If map reports or `SqlGeography`/`SqlGeometry` data are added later, add this to `Program.Main()` before any report/spatial work:

```csharp
SqlServerTypes.Utilities.LoadNativeAssemblies(AppDomain.CurrentDomain.BaseDirectory);
```

The native `SqlServerTypes` folder must also be present on disk.

---

## 7. Recommendations

1. **Keep** the `Microsoft.SqlServer.Types` NuGet package and managed DLL reference — required by ReportViewer.
2. **Safe to omit** (optional cleanup): native `SqlServerTypes\x86`/`x64` content items and `Loader.cs` if you want a leaner deploy — only if you are certain spatial features will never be added.
3. **Do not remove** the managed `Microsoft.SqlServer.Types.dll` without also removing ReportViewer, or print forms will fail at runtime.
4. No application code changes are needed for current behavior; the package is effectively a **transitive deployment dependency of ReportViewer**, not a feature the app uses directly.

---

## Evidence Summary

| Category | Finding |
|----------|---------|
| Direct spatial API usage | None |
| Spatial SQL | None (0/104 SQL files) |
| Map RDLC elements | None (0/13 RDLC files) |
| `LoadNativeAssemblies` calls | None (definition only) |
| ReportViewer usage | Yes — 5 forms, 13 tabular RDLC reports |
| Runtime need for managed DLL | Yes (ReportViewer dependency) |
| Runtime need for native DLLs + init | No (with current reports) |
