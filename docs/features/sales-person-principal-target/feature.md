# Sales Person–Principal Target

## Purpose

Maintain monthly sales targets per Sales Person per Principal (Supplier).

Targets are planning master data used for achievement measurement in RO2 and Portal dashboards (M13, M18).

---

## Business Context

Operations assigns Principals to each Sales Person in SM5. Management then sets monthly target amounts per assigned Principal in SM6.

Principal is the business term. In BTR technical data, Principal maps to **Supplier** (`BTR_Supplier`).

---

## Core Concepts

### Sales Person–Principal Target

A planned sales amount for one Sales Person, one Principal, and one calendar month.

This is not a sales transaction. It does not create or modify Faktur or assignments.

---

## Business Rules

1. A target may only be saved for a Principal **currently assigned** to the Sales Person in SM5.
2. At most one target per `(Sales Person, Principal, Year, Month)`.
3. `TargetAmount >= 0`. Zero is allowed.
4. Mid-month revisions are allowed — last saved value wins.
5. Removing a Principal assignment in SM5 does **not** delete historical target rows.
6. Targets for removed Principals are retained for history but hidden from the active entry grid.

### Coexistence with legacy aggregate targets (`BTR_SalesOmzetTarget`)

| Rule | Detail |
|------|--------|
| Per-Principal targets are source of truth when present | Sum of Principal targets for a rep/month supersedes legacy row |
| Legacy row is fallback | Used when no Principal target rows exist (sum = 0) for that rep/month |
| No duplicate entry required | Staff maintain Principal lines only in SM6 |

### Achievement measurement (v1)

- **Basis:** Invoice Amount (Omzet Faktur) per Principal via `FakturItem` → `Brg.SupplierId`
- **Formula:** `Sales Omzet / Target Omzet × 100`
- **Retur:** Does not reduce achievement in v1

---

## Desktop Maintenance

| Item | Value |
|------|-------|
| Menu | SM6-Principal Target |
| Form | `SalesPersonPrincipalTargetForm` |
| Pattern | Sales Person list → period toolbar → Principal target grid |
| Permissions | Same scope as SM5 |

### Supported Operations

- Select Year + Month
- Select Sales Person
- View assigned Principals with target amounts
- Save targets for current rep/period
- Copy Previous Month (current rep)
- Copy Previous Month for All Sales Persons
- Completeness indicator per rep

---

## Technical Mapping

| Business | Technical |
|----------|-----------|
| Sales Person | `BTR_SalesPerson` / `SalesPersonId` |
| Principal | `BTR_Supplier` / `SupplierId` |
| Assignment (eligibility) | `BTR_SalesPersonSupplier` |
| Target | `BTR_SalesPersonPrincipalTarget` |
| Legacy rep total | `BTR_SalesOmzetTarget` (fallback) |

### Table: `BTR_SalesPersonPrincipalTarget`

| Column | Description |
|--------|-------------|
| `SalesPersonId` | FK to `BTR_SalesPerson` |
| `SupplierId` | FK to `BTR_Supplier` |
| `TargetYear` | Calendar year |
| `TargetMonth` | Calendar month (1–12) |
| `TargetAmount` | Planned amount `DECIMAL(18,2)` |
| `UpdatedDate` | Last change timestamp |

Primary key: `(SalesPersonId, SupplierId, TargetYear, TargetMonth)`

---

## Related Features

- [Sales Person–Principal Assignment](../sales-person-principal/feature.md) (SM5) — defines which Principals appear in SM6

---

## Out of Scope

- Principal-level achievement reports and Portal UI (consumers use resolved rep totals today)
- Excel import
- Retur-adjusted / net / collection-adjusted achievement
- Automatic migration of `BTR_SalesOmzetTarget` rows
- Target versioning / audit history table
