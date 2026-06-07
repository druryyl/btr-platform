# Investigation: btr.portal.api Publish Menu Disabled in Visual Studio

**Date:** 2026-06-06  
**Scope:** Why Visual Studio does not recognize `btr.portal.api` as an ASP.NET Web Application and disables **Publish**.  
**Status:** Resolved — project metadata restored; Folder publish profile added.

---

## Executive Summary

After reverting the repository, `btr.portal.api.csproj` again contains the correct ASP.NET Web Application metadata (`ProjectTypeGuids`, `FlavorProperties`, `Microsoft.WebApplication.targets`). Visual Studio should recognize the project as a web application and enable **Publish**.

A **FolderProfile** publish profile was added at `Properties/PublishProfiles/FolderProfile.pubxml`, targeting `src/j05-btr-distrib/publish/btr-portal-api/`. MSBuild `WebPublish` with this profile succeeds on VS 2022 Enterprise.

If Visual Studio still reports “missing project subtype” on load, install or repair the **ASP.NET and web development** workload — that is an IDE registration issue, not a project file defect.

---

## 1. Findings

### 1.1 Project File (`btr.portal.api.csproj`)

| Attribute | Current (working tree) | Committed (`HEAD`) | Expected for Web App |
| --- | --- | --- | --- |
| Project format | Legacy non-SDK (`ToolsVersion="15.0"`) | Same | Correct |
| Target framework | .NET Framework 4.8 | Same | Correct |
| Output type | `Library` | Same | Correct (IIS-hosted web apps use Library) |
| `ProjectTypeGuids` | **Missing** | `{349c5851-65df-11da-9484-00065b056f6b};{fae04ec0-301f-11d3-bf4b-00c04f79efbc}` | Required |
| IIS Express properties | Present in `PropertyGroup` | Same | Partial (needs `FlavorProperties` for full VS support) |
| `Microsoft.WebApplication.targets` import | Present (conditional on `VSToolsPath`) | Same | Correct |
| `ProjectExtensions` / `FlavorProperties` | **Removed** (comment left in file) | Present with IIS URL `http://localhost:5050/` | Required for VS web tooling |
| SDK-style (`<Project Sdk=...>`) | No | No | N/A — not applicable |

**Current end-of-file comment (lines 249–252):**

```xml
<!-- Removed web project flavor properties so the project can open on machines
     without the ASP.NET Web Application project subtype installed. If you
     need IIS/IIS Express settings restored, re-add the FlavorProperties or
     install the "ASP.NET and web development" workload in Visual Studio. -->
```

**Git diff summary:** The only modification to the csproj in the working tree is the removal of `ProjectTypeGuids` and the entire `ProjectExtensions` / `FlavorProperties` block. All other project content matches the committed version from `8bc5494` (*Develop BTR-Portal (Milestone 1 to 15)*).

**GUID note:** The background error cited `{349C5851-65DF-11DA-9384-00065B846F21}`. The project file uses the standard Microsoft ASP.NET Web Application GUID `{349c5851-65df-11da-9484-00065b056f6b}` (segments `9484` and `00065b056f6b`). The error message GUID differs in two segments and may be a transcription error; the committed project uses the correct, well-known value.

### 1.2 Solution File (`j05-btr-distrib.sln`)

| Check | Result |
| --- | --- |
| Project entry | `Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "btr.portal.api", ...` |
| Project GUID | `{C8F3E2A1-4B5D-6E7F-8A9B-0C1D2E3F4A5B}` — matches csproj |
| Build configurations | Debug/Release \| Any CPU — present for portal.api |
| Solution nesting | Under `backend` folder — correct |
| Web-specific solution GUID | Not used (normal — web subtype is declared in csproj, not sln) |

**Conclusion:** The solution file is correctly structured. No solution-level fix is required.

### 1.3 Visual Studio / Environment Requirements

| Check | Result |
| --- | --- |
| `Microsoft.WebApplication.targets` on disk | **Found** at `C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Microsoft\VisualStudio\v17.0\WebApplications\Microsoft.WebApplication.targets` |
| ASP.NET Web Application project subtype | Required at **IDE load time** via `ProjectTypeGuids`; separate from MSBuild targets |
| “Missing project subtype” error | Occurs when VS cannot register/load GUID `{349c5851-65df-11da-9484-00065b056f6b}` — typically missing **ASP.NET and web development** workload or corrupted web project system |
| Workaround applied | Removing `ProjectTypeGuids` lets VS open the project as a class library but **disables Publish** |

**Distinction:** MSBuild can build web projects when `Microsoft.WebApplication.targets` is present, but Visual Studio **Publish**, **Web** property tab, and F5/IIS Express debugging require the web application **project subtype** in the csproj.

### 1.4 Web Application Artifacts

The project content is a legitimate ASP.NET Web API 2 application, not a mis-templated class library:

| Artifact | Present |
| --- | --- |
| `Global.asax` / `Global.asax.cs` | Yes |
| `Web.config` | Yes (targetFramework 4.8, extensionless URL handler) |
| `App_Start/WebApiConfig.cs` | Yes |
| `Controllers/` | Yes (Auth, Health, Dashboard, Reports) |
| `System.Web.Http.WebHost` reference | Yes |
| `Properties/PublishProfiles/` | **No** (never created — Publish was not configured) |
| `btr.portal.api.csproj.user` | **No** |

**Classification:** ASP.NET Web Application (Web API 2), .NET Framework 4.8 — matches the implementation plan (`docs/work/btr-portal-api-scaffolding/implementation-plan-btr-portal-api-scaffolding.md`, Section 2.3).

### 1.5 Comparison with Other Solution Projects

| Project | Type | `ProjectTypeGuids` |
| --- | --- | --- |
| `btr.portal.api` (current) | Intended web app; loaded as class library | Missing |
| `btr.distrib` | WinForms (`OutputType=WinExe`) | None (desktop app) |
| `btr.domain`, `btr.application`, etc. | Class libraries | None |
| `btr.test` | Test project | `{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}` only |

No other web application exists in this solution for side-by-side comparison. `btr.portal.api` is the only IIS-hosted web project.

---

## 2. Root Cause

**Primary cause (current Publish disabled state):**  
The csproj was modified to remove `ProjectTypeGuids` and `FlavorProperties`. Visual Studio no longer recognizes the project as an ASP.NET Web Application and treats it as a C# class library. Class libraries do not expose the **Publish** command or web-specific property pages.

**Underlying cause (original “missing project subtype” error):**  
Before the workaround, the project correctly declared the web application subtype. Visual Studio failed to load that subtype, which usually means the **ASP.NET and web development** workload (or its project-system package) was not installed or not properly registered on the affected machine. That error blocked project load; removing the GUIDs was a workaround to allow opening the solution, at the cost of web IDE features.

**Not root causes:**

- Solution file misconfiguration — solution entry is correct.
- Wrong project template — source files and references match Web API 2 web application.
- SDK-style incompatibility — project is correctly non-SDK for .NET Framework 4.8.
- Missing `Microsoft.WebApplication.targets` on the current machine — file is present under VS 2022 Enterprise.

---

## 3. Recommended Fix

**Category B + A (combination): Project file fix, with optional Visual Studio installation verification**

| Option | Description | Why | Risk | Effort |
| --- | --- | --- | --- | --- |
| **B — Restore csproj web metadata** (recommended) | Revert removal of `ProjectTypeGuids` and `FlavorProperties` | Restores web application recognition and Publish | Low — restores committed, intended state | ~5 minutes |
| **A — Verify VS workload** | Ensure **ASP.NET and web development** is installed; repair VS if subtype error returns | Addresses original load failure | None | 10–30 minutes if install/repair needed |
| C — Solution fix | Not needed | Solution is already correct | N/A | N/A |
| D — Recreate project | Not recommended | All source is correct; recreation adds risk and churn | Medium (reference/config drift) | 1–2 hours |
| E — CLI-only deploy | MSBuild/`PublishUrl` or Web Deploy without VS Publish | Fallback if VS workload cannot be installed | Manual process; no IDE publish profiles | 30–60 minutes setup |

**Recommended order:**

1. Verify/install **ASP.NET and web development** workload in Visual Studio Installer.
2. Restore `ProjectTypeGuids` and `FlavorProperties` in `btr.portal.api.csproj` (revert the workaround).
3. Reload solution and confirm Publish is enabled.
4. Create Folder publish profile per deployment plan (Section 12.3).

---

## 4. Detailed Fix Steps

### Step 1 — Verify Visual Studio workload (before or after csproj restore)

1. Open **Visual Studio Installer**.
2. Select **Modify** on Visual Studio 2022.
3. Under **Workloads**, ensure **ASP.NET and web development** is checked.
4. Under **Individual components** (optional verification), confirm items such as:
   - IIS Express
   - Web Deploy
5. Apply changes and restart Visual Studio.
6. If the subtype error persists after restore, run **Repair** on the VS installation.

**Verify workload from command line (optional):**

```powershell
& "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" `
  -latest -requires Microsoft.VisualStudio.Workload.NetWeb `
  -property displayName,installationPath
```

A non-empty result indicates the ASP.NET/web workload is installed.

### Step 2 — Restore web project metadata in csproj

Revert the working-tree change to `src/j05-btr-distrib/btr.portal.api/btr.portal.api.csproj`:

**A. Re-add `ProjectTypeGuids`** immediately after `<ProjectGuid>`:

```xml
<ProjectTypeGuids>{349c5851-65df-11da-9484-00065b056f6b};{fae04ec0-301f-11d3-bf4b-00c04f79efbc}</ProjectTypeGuids>
```

**B. Re-add `ProjectExtensions`** before the closing `</Project>` tag (replace the removal comment):

```xml
<ProjectExtensions>
  <VisualStudio>
    <FlavorProperties GUID="{349c5851-65df-11da-9484-00065b056f6b}">
      <WebProjectProperties>
        <UseIIS>True</UseIIS>
        <AutoAssignPort>True</AutoAssignPort>
        <DevelopmentServerPort>5050</DevelopmentServerPort>
        <DevelopmentServerVPath>/</DevelopmentServerVPath>
        <IISUrl>http://localhost:5050/</IISUrl>
        <NTLMAuthentication>False</NTLMAuthentication>
        <UseCustomServer>False</UseCustomServer>
        <CustomServerUrl></CustomServerUrl>
        <SaveServerSettingsInUserFile>False</SaveServerSettingsInUserFile>
      </WebProjectProperties>
    </FlavorProperties>
  </VisualStudio>
</ProjectExtensions>
```

**Alternative:** `git checkout HEAD -- src/j05-btr-distrib/btr.portal.api/btr.portal.api.csproj` restores the committed file entirely (only if no other intentional csproj edits must be kept).

### Step 3 — Reload and configure Publish

1. Close and reopen the solution, or unload/reload `btr.portal.api`.
2. Confirm the project loads without “missing project subtype” error.
3. Right-click **btr.portal.api** → **Publish**.
4. Create a **Folder** profile:
   - Configuration: **Release**
   - Target location: `publish\btr-portal-api\` (per deployment plan)
5. Publish once to generate `Properties\PublishProfiles\*.pubxml`.

### Step 4 — Fallback if Publish remains unavailable

If web metadata is restored but Publish is still disabled:

- Confirm project shows a **Web** tab in Properties (not only Application/Build).
- Run MSBuild publish from Developer Command Prompt:

```cmd
msbuild btr.portal.api.csproj /p:DeployOnBuild=true /p:PublishProfile=FolderProfile /p:Configuration=Release
```

(after creating a publish profile manually or via VS)

---

## 5. Verification Steps

### After csproj restore and VS workload check

| # | Check | Expected result |
| --- | --- | --- |
| 1 | Open `j05-btr-distrib.sln` in Visual Studio 2022 | No “missing project subtype” dialog |
| 2 | Solution Explorer project icon | Web application icon ( globe ), not generic class library |
| 3 | Right-click **btr.portal.api** | **Publish** menu item enabled |
| 4 | Project → Properties | **Web** tab visible with IIS Express / Local IIS settings |
| 5 | Debug (F5) | Launches at `http://localhost:5050/` (or configured IIS URL) |
| 6 | Build solution (Release) | Builds without errors |
| 7 | Publish to folder | Output includes `bin\`, `Global.asax`, `Web.config`, `appsettings.json`, content files |
| 8 | Hit health endpoint locally | `GET http://localhost:5050/api/health` → 200 |

### Post-IIS deployment (from implementation plan)

```text
GET  http://{server}/btr-portal-api/api/health        → 200
POST http://{server}/btr-portal-api/api/auth/login      → 200 with token (valid user)
GET  http://{server}/btr-portal-api/api/dashboard/sales → 401 without token
```

---

## 6. References

| Resource | Location |
| --- | --- |
| Project file | `src/j05-btr-distrib/btr.portal.api/btr.portal.api.csproj` |
| Solution | `src/j05-btr-distrib/j05-btr-distrib.sln` |
| Implementation plan (project type, IIS deploy) | `docs/work/btr-portal-api-scaffolding/implementation-plan-btr-portal-api-scaffolding.md` |
| Milestone 3 remaining deploy tasks | `docs/work/btr-portal-api-scaffolding/implementation-summary-milestone-3.md` |
| ASP.NET Web Application project type GUID | `{349c5851-65df-11da-9484-00065b056f6b}` |
| MSBuild web targets (verified on machine) | `...\VS2022\Enterprise\MSBuild\Microsoft\VisualStudio\v17.0\WebApplications\Microsoft.WebApplication.targets` |

---

## 7. Resolution (2026-06-06)

1. **Reverted** the csproj workaround — `ProjectTypeGuids` and `FlavorProperties` are present in `HEAD`.
2. **Added** `Properties/PublishProfiles/FolderProfile.pubxml` and registered it in the csproj.
3. **Verified** MSBuild Release build and `WebPublish` with `FolderProfile` succeed on VS 2022 Enterprise.

### Visual Studio checklist

1. Close and reopen `j05-btr-distrib.sln` (or unload/reload `btr.portal.api`).
2. Confirm **Publish** is enabled on right-click.
3. Select **FolderProfile** → Publish (output: `src/j05-btr-distrib/publish/btr-portal-api/`).

If the “missing project subtype” dialog appears on load, install **ASP.NET and web development** via Visual Studio Installer, then reload.
