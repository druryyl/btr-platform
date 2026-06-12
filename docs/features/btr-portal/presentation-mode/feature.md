# Presentation Mode

**Audience:** Product Owner, Operators, Support  
**Purpose:** Hide platform and infrastructure diagnostics during demonstrations, management presentations, proposal screenshots, and executive reviews.

**Related:** [BTR Portal operational guide](../btr-portal-operational.md) · [Deploy runbook](../../../src/j05-btr-distrib/docs/ops/btr-portal-deploy.md)

---

## Business Need

During normal operations, BTR Portal surfaces technical trust indicators (snapshot freshness, worker health, domain availability) so operators know when analytics may be incomplete. During presentations, these messages distract from business value and make screenshots look unfinished.

Presentation Mode addresses that gap. A related **Business Date** setting lets the portal calculate KPIs, aging, and report defaults as if "today" is the snapshot date—without changing the operating system clock.

---

## Behavior

### When disabled (default)

All existing platform diagnostics remain visible. No change from pre-feature behavior.

### When enabled

**Business date:** Dashboard metrics, aging, overdue counts, MTD periods, report default date ranges, and Field Activity "Today" use `Presentation.BusinessDate` as the effective calendar date. Infrastructure timestamps (snapshot `GeneratedAt`, refresh logs, JWT expiry, health checks) continue using actual system time.

**UI indicator:** The application header shows `Presentation Mode` and `Business Date: <date>` so internal users know a simulated date is active.

**Snapshot worker:** After enabling presentation business date, re-run the portal snapshot worker so materialized KPIs reflect the configured date.

The portal UI also hides platform/infrastructure diagnostics:

- Dashboard Data Not Fresh banners
- Snapshot refresh degraded / refreshing banners
- Platform status tag on Alert Center
- Platform Alerts section on Alert Center
- Last Refreshed and Data as of timestamps
- Worker and snapshot unavailable messages
- Data unavailable labels on attention cards
- Field Activity coordinate/plan diagnostic line

Business content remains visible:

- KPIs, charts, rankings, exposure lists
- Attention signals and Alert Center business alerts (Overdue, Dead Stock, Collection Risk, etc.)
- Reports and investigation drill-down

---

## Configuration

Set in API `appsettings.json` (or machine-specific override):

```json
{
  "Presentation": {
    "Enabled": true,
    "BusinessDate": "2026-06-05"
  }
}
```

| Setting | Behavior |
| ------- | -------- |
| `Enabled = false` | Normal operations; actual system date used for business calculations. |
| `Enabled = true` | Business calculations use `BusinessDate`; UI indicator shown; platform diagnostics hidden. |
| `Enabled = true` without valid `BusinessDate` | API/worker fail at startup with a configuration error. |

**Demo setup:** Set `BusinessDate` to match the restored database snapshot date. Re-run `btr.portal.worker.exe --domain All` after changing configuration. Revert `Enabled` to `false` after demonstrations and re-run the worker for production date. Recycle the IIS app pool or wait for configuration reload.

---

## Acceptance Criteria

1. `Presentation.Enabled = false` — unchanged operational diagnostics and actual system business date.
2. `Presentation.Enabled = true` — platform freshness and infrastructure warnings hidden; business calculations use `BusinessDate`.
3. Business alerts remain visible.
4. Header indicator shows configured business date when presentation is enabled.
5. Infrastructure timestamps (refresh, audit, JWT) use actual system time.
