# Principal KPI Registry v1

## Purpose

This registry defines the authoritative Principal Analytics KPI catalog for BTR Portal.

All dashboards, Entity Analytics, rankings, trends, targets, reports, and future analytics features shall use these KPI definitions.

---

# Sales Domain

| KPI ID        | KPI Name            | Description                                     | Authority                               |
| ------------- | ------------------- | ----------------------------------------------- | --------------------------------------- |
| PRN-SALES-001 | Principal Sales-Out | Total Sales-Out (DPP) attributed to a Principal | Authoritative Principal Performance KPI |

### Rules

* Measured using Sales-Out (DPP).
* Does not deduct Returns.
* Does not deduct Claims.
* Does not deduct Inventory Adjustments.
* Ranking KPI for Principal performance.

### Evidence Grain

```text
Faktur Item
```

---

# Returns Domain

| KPI ID      | KPI Name             | Description                                                 |
| ----------- | -------------------- | ----------------------------------------------------------- |
| PRN-RET-001 | Good Return Amount   | Total Good Return value attributed to a Principal           |
| PRN-RET-002 | Broken Return Amount | Total Broken/Damaged Return value attributed to a Principal |
| PRN-RET-003 | Total Return Amount  | Good Return + Broken Return                                 |
| PRN-RET-004 | Return Percentage    | Return Amount ÷ Sales-Out                                   |

### Rules

* Returns are independent KPIs.
* Returns never reduce Principal Sales-Out.
* Returns may be used in Entity Analytics, rankings, attention signals, radar dimensions, and quality analysis.

### Evidence Grain

```text
Return Item
```

---

# Target & Achievement Domain

| KPI ID      | KPI Name               | Description                            | Unit    |
| ----------- | ---------------------- | -------------------------------------- | ------- |
| PRN-TGT-001 | Principal Target       | Sum of Salesman Principal Targets      | IDR     |
| PRN-TGT-002 | Achievement Amount     | Principal Sales-Out versus Target      | IDR     |
| PRN-TGT-003 | Achievement Percentage | Principal Sales-Out ÷ Principal Target | Ratio |
| PRN-TGT-004 | Pacing Achievement %   | Time-aware pacing achievement (MTD actual vs paced target) | Percent |

> `PRN-TGT-004` registered display name is **Pacing Achievement %**, unit **Percent**.
> Definition, formatting, axis mapping (`X`), and confidence threshold (`MinimumElapsedDays`)
> are sourced from the KPI Registry (`SupplierEntityAnalyticsRegistrar` / `PrincipalKpiCatalog`).
> This document introduces no independent formula; consumers resolve labels, units, and
> formatting from the registered identifiers.
> `PRN-TGT-003` remains unchanged (full-month Achievement %, SA04/ranking use preserved).

### Rules

* Principal Target is derived from Salesman Principal Targets.
* No independently maintained Principal Target exists.

### Evidence Grain

```text
SalesPersonPrincipalTarget
```

---

# Purchasing Domain

| KPI ID      | KPI Name    | Description                        |
| ----------- | ----------- | ---------------------------------- |
| PRN-PUR-001 | Purchase-In | Total purchases from the Principal |

### Rules

* Purchase-In remains independent from Sales-Out.
* Purchase-In is not used as the Principal ranking KPI.

### Evidence Grain

```text
Purchase Detail
```

---

# Inventory Domain

| KPI ID      | KPI Name        | Description                                    |
| ----------- | --------------- | ---------------------------------------------- |
| PRN-INV-001 | Inventory Value | Current inventory value for Principal products |
| PRN-INV-002 | Inventory Days  | Estimated days of inventory coverage           |

### Rules

* Inventory KPIs are independent operational indicators.
* Inventory KPIs do not modify Sales-Out performance.

### Evidence Grain

```text
Inventory Snapshot
```

---

# Customer Domain

| KPI ID      | KPI Name                     | Description                                         |
| ----------- | ---------------------------- | --------------------------------------------------- |
| PRN-CUS-001 | Active Customer Count        | Number of active Customers purchasing the Principal |
| PRN-CUS-002 | Customer Coverage Percentage | Customer reach against eligible customer base       |

### Rules

Active Customer:

```text
Last transaction within 6 months
```

Dormant Customer:

```text
No transaction within 6 months
```

Relationship history is retained indefinitely.

### Evidence Grain

```text
Customer × Principal Relationship Snapshot
```

---

# Growth Domain

| KPI ID      | KPI Name                           | Description              | Unit    |
| ----------- | ---------------------------------- | ------------------------ | ------- |
| PRN-GRW-001 | Month-over-Month Growth Percentage | Monthly Principal growth | Percent |
| PRN-GRW-002 | Year-over-Year Growth Percentage   | Annual Principal growth  | Percent |
| PRN-GRW-003 | YoY MTD Growth %                   | Time-aware YoY MTD growth (equivalent elapsed-day windows) | Percent |

> `PRN-GRW-003` registered display name is **YoY MTD Growth %**, unit **Percent**.
> Definition, formatting, axis mapping (`Y`), and confidence threshold (`MinimumBaseValue`)
> are sourced from the KPI Registry (`SupplierEntityAnalyticsRegistrar` / `PrincipalKpiCatalog`).
> This document introduces no independent formula; consumers resolve labels, units, and
> formatting from the registered identifiers.
> `PRN-GRW-002` remains unchanged (month-grain YoY Growth %, SA04/ranking use preserved).

### Rules

Growth calculations use Principal Sales-Out as the source KPI.

---

# Ranking Rules

## Authoritative Ranking KPI

```text
PRN-SALES-001
Principal Sales-Out
```

### Supporting KPIs

```text
PRN-RET-004   Return Percentage
PRN-TGT-003   Achievement Percentage
PRN-GRW-001   MoM Growth %
PRN-GRW-002   YoY Growth %
```

`PRN-TGT-003` and `PRN-GRW-002` remain unchanged.

### Time-Aware Map KPIs (PSOM)

```text
PRN-TGT-004   Pacing Achievement %
PRN-GRW-003   YoY MTD Growth %
```

Consumed by the `principal-sales-out-map` preset (X = `PRN-TGT-004`, Y = `PRN-GRW-003`),
the Sales-Out investigation lens, and the Principal Profile only.

---

# Governance (GAP-009)

The KPI Registry is the authoritative source of KPI identifiers, display names, units,
formatting, descriptions, axis mappings, and confidence thresholds.

New KPIs (`PRN-TGT-004`, `PRN-GRW-003`) were registered in the KPI Registry
(`PrincipalKpiCatalog` + `SupplierEntityAnalyticsRegistrar`) before consumption by
maps, lenses, profiles, or exports. Catalog documentation and lens configuration
reference the registered KPI identifiers only and maintain no independent KPI names
or formulas.

---

# Explicit Non-Goals

The following KPI is intentionally excluded:

```text
Principal Health Score
```

Reason:

* Composite scoring introduces subjective weighting.
* Individual KPI transparency is preferred.

No composite Principal Health KPI shall be introduced in V1.
