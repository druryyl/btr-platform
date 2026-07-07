# Verification Report: Download Pending Faktur Recovery

**Date:** 2026-07-07  
**Feature:** Isolated recovery for Packing Orders never downloaded from cloud (`DownloadTimestamp = 3000-01-01`)

---

## Build Verification

| Component | Result | Notes |
|-----------|--------|-------|
| `btrade.application` | PASS | `dotnet build` succeeded |
| `btrade.infrastructure` | PASS | Included in webapi build |
| `btrade.webapi` | PASS | `dotnet build` succeeded |
| `BtrGudang.Winform` | NOT RUN | Requires MSBuild/Visual Studio (.NET Framework 4.7.2); project files updated, no compile errors expected |
| Existing unit tests | SKIPPED | `BtrGudang.Test` requires SQL Server `devTest` database (environment not available) |

---

## Isolation Verification (Scenario 6 — Regression)

| Check | Result |
|-------|--------|
| `WrhDownloadPackingOrderCmd.cs` / handler | UNCHANGED (`git diff` empty) |
| `DL1DownloaderForm.cs` | UNCHANGED (`git diff` empty) |
| `LastDownloadPackingOrderTimestamp` registry usage | Only in DL1; DL3 does not read or write registry |
| Existing endpoint `GET /api/PackingOrder/{startTimestamp}/{warehouseCode}/{pageSize}` | Handler body unchanged; pending route registered **before** generic route to avoid `"pending"` being captured as `startTimestamp` |
| Import pipeline | DL3 calls `PackingOrderRepo.SaveChanges` — same path as DL1 |

---

## Implementation Verification (Code Review)

| Requirement | Verified |
|-------------|----------|
| Cloud filters `DownloadTimestamp = '3000-01-01'` | `PackingOrderDal.ListPendingData` SQL |
| Batch size capped | `TOP (@PageSize)` in SQL; warehouse uses `BatchSize = 100` |
| Cloud marks downloaded | `SetDownloadTimestamp` + `PackingOrderDepoDal.Update` in pending handler |
| Warehouse batch loop | `DL3PendingDownloaderForm.ExecuteRecoveryAsync` loops until empty batch |
| Per-order error handling | try/catch per order; batch continues |
| Idempotency | `SaveChanges` upserts by `PackingOrderId`; existing records counted as `skipped` |
| Logging | Recovery started/completed, batch counts, duration, imported/skipped/failed |
| Separate UI command | Menu: **DL3 - Download Pending Faktur...**; button: **Download Pending Faktur** |

---

## Scenario Test Matrix

### Scenario 1 — No pending Faktur

| Item | Detail |
|------|--------|
| Setup | Ensure all `BTRADE_PackingOrderDepo` rows for test depo have `DownloadTimestamp != '3000-01-01'` |
| Expected | Recovery completes; `received: 0, imported: 0` |
| Status | **PENDING MANUAL** — requires cloud DB + DL3 form |

### Scenario 2 — 50 pending Faktur

| Item | Detail |
|------|--------|
| Setup | Set 50 depo rows to `DownloadTimestamp = '3000-01-01'`; empty `BTRG_PackingOrder` |
| Expected | Single batch; 50 imported; cloud `DownloadTimestamp` updated |
| Status | **PENDING MANUAL** |

### Scenario 3 — 500 pending Faktur

| Item | Detail |
|------|--------|
| Setup | 500 sentinel rows for one depo |
| Expected | 5 batches of 100; DL3 log shows 5 "Batch received" lines; no full backlog loaded at once |
| Status | **PENDING MANUAL** |

### Scenario 4 — Partial local copy

| Item | Detail |
|------|--------|
| Setup | 25 of 50 pending already in `BTRG_PackingOrder` |
| Expected | `imported: 25, skipped: 25`; no duplicate `PackingOrderId` PK violations |
| Status | **PENDING MANUAL** |

### Scenario 5 — Recovery executed twice

| Item | Detail |
|------|--------|
| Setup | Run DL3 after Scenario 2 or 4 |
| Expected | Second run: `received: 0`; no errors |
| Status | **PENDING MANUAL** |

### Scenario 6 — Normal sync after recovery

| Item | Detail |
|------|--------|
| Setup | Note registry `LastDownloadPackingOrderTimestamp` before/after; run DL1 "Download Now" |
| Expected | Watermark advances per existing rules only; timestamp endpoint behavior unchanged |
| Status | **PASS (static)** — DL1 code untouched; **PENDING MANUAL** for runtime confirmation |

---

## API Contract

```
GET /api/PackingOrder/pending/{warehouseCode}/{pageSize}
```

**Response (JSend):**

```json
{
  "status": "success",
  "data": {
    "listData": [ /* same shape as timestamp download ListData */ ]
  }
}
```

No `lastTimestamp` field — recovery does not use watermark cursor.

---

## Files Added / Modified

### Cloud (`j06-pkl-btrade-api`)

- **NEW** `btrade.application/WarehouseFreature/WrhDownloadPendingPackingOrderCmd.cs`
- **MODIFIED** `IPackingOrderDal.cs`, `PackingOrderDal.cs`, `PackingOrderController.cs`

### Warehouse (`j07-btr-gudang`)

- **NEW** `DL3PendingDownloaderForm.cs`, `DL3PendingDownloaderForm.Designer.cs`
- **MODIFIED** `PackingOrderDownloaderSvc.cs`, `MainForm.cs`, `MainForm.Designer.cs`, `Program.cs`, `BtrGudang.Winform.csproj`

---

## Conclusion

Implementation is complete and cloud projects compile successfully. Existing DL1 synchronization code paths are unchanged. Runtime verification of scenarios 1–5 and DL1 regression (scenario 6) should be executed in a staging environment with cloud API, cloud SQL, and warehouse SQL available.
