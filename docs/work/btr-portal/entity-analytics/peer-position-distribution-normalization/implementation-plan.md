# Implementation Plan — Peer Position Distribution Normalization

**Status:** Implemented (IDR round segments + high-end ladder)  
**Scope:** Investigation Workspace Peer Position histogram binning  
**Primary code:** `EntityPeerDistributionEngine` / `EntityPeerDistributionBinBuilder`

---

## Objective

Make Peer Position readable for Indonesian Rupiah open balances: round segment labels, fixed first band 0–100,000, mid bands from data distribution, and forced high-end bands so the right tail is not one oversized bin. Non-IDR units keep linear equal-width bins.

---

## Approach

1. `EntityPeerDistributionBinBuilder.BuildBins(values, binCount, unit)`.
2. When `unit` is `IDR`:
   - Always start with edges `0` and `100_000`.
   - If peer max ≤ 100,000 → single bin `0 – 100,000`.
   - Else take peers ≥ 100,000, place equal-count (quantile) cuts for remaining bins, snap each internal cut with `SnapToNiceIdr` (1-2-5 × 10^n).
   - Force-insert high-end edges **50M, 100M, 200M, 500M** when each is strictly below peer max (sorted insert so gaps are filled even if quantile jumped ahead).
   - End at peer max.
3. When `unit` is `Days`:
   - Use calendar ladder `0, 7, 14, 21, 30, 60, 90, 120, 180, 270, 365` clipped to steps strictly below peer max.
   - When max &gt; 365, last bin is **365 – peer max**.
4. When not IDR/Days: linear equal-width min→max.
5. Leave percentile ranking and peer min/max formatting unchanged.

No frontend chart changes required.

---

## Acceptance criteria

- [x] First IDR bin ends at exactly `100_000`.
- [x] Internal IDR edges are on the 1-2-5 Rupiah ladder (round labels).
- [x] Skewed open-balance fixture spreads peers across multiple segments above 100k.
- [x] High-end ladder splits 50M → 100M → 200M → 500M → max when max &gt; 500M.
- [x] High-end steps at or above peer max are skipped.
- [x] Days unit uses week/month calendar ladder; last bin starts at 365 when max &gt; 365.
- [x] `SelectedPercentile` / ranking behavior unchanged.
- [x] Non-IDR / non-Days KPIs keep linear binning.
- [x] All values &lt; 100k → single `0 – 100,000` bin.
- [x] Permanent knowledge updated: `docs/features/entity-analytics/peer-position-distribution.md`.

---

## Test cases

| Case | Input | Expectation |
| --- | --- | --- |
| First floor | Mixed IDR including &lt; and &gt; 100k | Bin 0 is `0 – 100,000` |
| Nice edges | Skewed IDR population | Internal ends on 1-2-5 ladder |
| Skewed IDR | Many &lt; 100k / mid / 500M+ | Multiple occupied segments; counts sum to N |
| High-end | Max ≥ 1.4B | Edges include 50M, 100M, 200M, 500M; no single 50M→max bin |
| High-end clip | Max = 300M | 50M, 100M, 200M present; 500M absent |
| Days ladder | Mixed days incl. 800 | Week/month edges; last bin `365 – 800` |
| Days clip | Max = 100 | No 365 start; ladder clipped |
| All below 100k | Max &lt; 100k | Single bin `0 – 100,000` |
| Linear non-IDR | Unit `%` | Equal-width linear bins |
| Snap helper | Raw cuts | Nearest 1-2-5 step ≥ 100k |

---

## Out of scope

- Quantile binning for non-IDR units
- Selected-entity bin highlight on the chart UI
- Changing Population Map projection
