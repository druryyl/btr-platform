# Peer Position Distribution

**Status:** Permanent knowledge  
**Feature area:** Entity Analytics — Investigation Workspace  
**Related:** [entity-analytics-developer-guide.md](entity-analytics-developer-guide.md)

---

## Purpose

Peer Position answers: for one KPI, is the selected entity normal, unusual, or extreme within its peer group?

The histogram shows peer count by value band. The summary line reports rank percentile (`above X% of peers`).

---

## Binning semantics

Bins are built server-side by `EntityPeerDistributionEngine` / `EntityPeerDistributionBinBuilder`.

| KPI unit | Binning mode | Behavior |
| --- | --- | --- |
| `IDR` | Rupiah segments | Fixed first bin **0 – 100,000**; mid edges from equal-count (quantile) cuts snapped to a **1-2-5 × 10^n** ladder; forced high-end edges **50M → 100M → 200M → 500M** (when below peer max); last edge = peer max |
| `Days` | Calendar segments | Week/month ladder **0 → 7 → 14 → 21 → 30 → 60 → 90 → 120 → 180 → 270 → 365**, clipped below peer max; when max &gt; 365 the last bin is **365 – peer max** |
| Any other unit | Linear | Equal-width bins on raw value (min → max) |

Default target bin count: **20** (IDR/Days use semantic ladders; count may differ).

### Why IDR uses round Rupiah segments

Open balance and other IDR KPIs are heavily right-skewed. Linear min→max bins collapse peers to the left. Pure log edges spread peers but produce awkward non-round labels (poor for Rupiah reading).

The IDR scheme:

1. Anchors a management-readable first band at **0 – 100,000**.
2. Places mid bands from the **actual distribution** of higher balances (quantile cuts).
3. Snaps mid edges to round steps: 100rb, 200rb, 500rb, 1jt, 2jt, 5jt, 10jt, …
4. Forces high-end bands at **50jt, 100jt, 200jt, 500jt** before peer max so a single oversized right-tail bin (e.g. 50jt–1.4M) does not hide concentration among large open balances.

### Why Days uses week/month calendar segments

Days of Supply and Days Since Last Faktur are also right-skewed: most items sit in days/weeks, while a few sit at hundreds or thousands of days. Linear bins hide that shape.

The Days scheme:

1. Uses **week** edges near term (7 / 14 / 21) where most peers concentrate.
2. Uses **month-scale** edges mid term (30 / 60 / 90 / 120 / 180 / 270).
3. Forces a **≥ 1 year** last band starting at **365** when peer max exceeds one year.

### Labels and edges

Bin edges and labels are always **business units**. IDR internal edges are on the nice ladder (including the forced high-end steps); Days edges are on the calendar ladder. The final bin may end at the observed peer max (not necessarily ladder-aligned).

### What does not change

- Peer group resolution and dimension filters
- Rank / percentile calculation (`SelectedPercentile`)
- `PeerMin` / `PeerMax` and formatted peer range (raw business values)

---

## Default Peer Position KPIs

### Customer

| KPI | Unit | Binning |
| --- | --- | --- |
| Open Balance (`CU-KPI-010`) | IDR | Rupiah segments |
| MTD Omzet (`CU-KPI-009`) | IDR | Rupiah segments |
| Overdue Exposure (`FI-KPI-013`) | IDR | Rupiah segments |

### Item

| KPI | Unit | Binning |
| --- | --- | --- |
| Days of Supply (`IN-KPI-020`) | Days | Calendar segments |
| Days Since Last Faktur | Days | Calendar segments |
| Inventory Value | IDR | Rupiah segments |

Percent / count Peer Position KPIs remain linear.
