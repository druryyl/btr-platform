# PROFILING BASELINE

## PCM-003 — GAP-007 production profiling

| Field | Value |
| --- | --- |
| Slice | PCM-003 |
| Planning authority | FEASIBILITY ASSESSMENT |
| KPI semantics authority | PRINCIPAL KPI REGISTRY |
| Measured at | 2026-09-09 07:59:48 |
| As-of date | 2026-09-09 |
| Source | Database `btr2` on `JUDE7` |
| Void rule | Non-void only: `BTR_Faktur.VoidDate = '3000-01-01'` |
| Principal attribution | Current Item master `BTR_Brg.SupplierId`, matching an existing `BTR_Supplier` row |
| Unknown or blank Supplier | Missing Item, blank `SupplierId`, or `SupplierId` with no `BTR_Supplier` row |

This baseline records measured results. It does not change GAP-001 through GAP-023. It does not change the Principal KPI Registry. It does not change application behavior, schema, or any KPI definition.

Material findings below are implementation-effort and data-quality notes. They are not a reason to reopen the approved model, the registry, or the guardrails.

---

## Source window

| Measure | Result |
| --- | --- |
| Faktur rows, including void | 121,155 |
| Non-void Fakturs | 119,513 |
| Voided Fakturs | 1,642 |
| Non-void Fakturs with no items | 0 |
| Sold item lines on non-void Fakturs | 946,868 |
| Non-void Faktur date span | 2025-02-07 through 2026-06-24 |
| Distinct non-void Faktur months | 17 |
| Months of history from first non-void Faktur to as-of | 19 |
| `BTRPD_CustomerPrincipalRelationship` | Does not exist. Counts below are the expected population from historical transactions, not a stored projection. |

Latest available non-void invoice is 2026-06-24. July through September 2026 have no non-void invoices in this source. Trailing windows are evaluated against the as-of date, not against the latest invoice date.

Window definitions:

- Trailing 12 months: `FakturDate >= 2025-09-09` and `FakturDate < 2026-09-10`
- 6-month Active: last transaction on or after 2026-03-09
- 36-month backfill: `FakturDate >= 2023-09-09`

Available history starts on 2025-02-07, so the 36-month backfill equals all available non-void history.

---

## Check 1 — Distinct Salesmen per Customer

Salesman is `BTR_Faktur.SalesPersonId`.

### By month

| Month | Customers | 1 Salesman | 2 Salesmen | 3+ Salesmen | More than 1 | Min | Max | Avg |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| 2025-02 | 1,289 | 614 | 243 | 432 | 675 | 1 | 6 | 2.2141 |
| 2025-03 | 1,265 | 631 | 224 | 410 | 634 | 1 | 6 | 2.1676 |
| 2025-04 | 1,228 | 540 | 217 | 471 | 688 | 1 | 6 | 2.3616 |
| 2025-05 | 1,275 | 534 | 214 | 527 | 741 | 1 | 7 | 2.4667 |
| 2025-06 | 1,271 | 539 | 258 | 474 | 732 | 1 | 6 | 2.3249 |
| 2025-07 | 1,395 | 625 | 245 | 525 | 770 | 1 | 6 | 2.3090 |
| 2025-08 | 1,300 | 593 | 270 | 437 | 707 | 1 | 6 | 2.2246 |
| 2025-09 | 1,312 | 594 | 266 | 452 | 718 | 1 | 6 | 2.2264 |
| 2025-10 | 1,344 | 599 | 273 | 472 | 745 | 1 | 6 | 2.2537 |
| 2025-11 | 1,386 | 561 | 279 | 546 | 825 | 1 | 6 | 2.3766 |
| 2025-12 | 1,360 | 574 | 264 | 522 | 786 | 1 | 5 | 2.2647 |
| 2026-01 | 1,423 | 574 | 271 | 578 | 849 | 1 | 6 | 2.4680 |
| 2026-02 | 1,375 | 592 | 244 | 539 | 783 | 1 | 6 | 2.3695 |
| 2026-03 | 1,073 | 556 | 192 | 325 | 517 | 1 | 5 | 2.1081 |
| 2026-04 | 1,354 | 512 | 235 | 607 | 842 | 1 | 7 | 2.6071 |
| 2026-05 | 1,333 | 521 | 232 | 580 | 812 | 1 | 10 | 2.5881 |
| 2026-06 | 1,119 | 515 | 216 | 388 | 604 | 1 | 7 | 2.3056 |

Each month, more than half of Customers transact with more than one Salesman. The monthly average is about 2.1 to 2.6 Salesmen.

### Trailing 12 months

| Measure | Result |
| --- | ---: |
| Customers | 1,953 |
| 1 Salesman | 502 |
| 2 Salesmen | 357 |
| 3+ Salesmen | 1,094 |
| More than 1 Salesman | 1,451 (74.30%) |
| Min / Max / Avg | 1 / 12 / 3.5781 |

---

## Check 2 — Distinct Principals per Customer

Principal is the current Item-master `SupplierId` of a sold item. Unknown or blank Supplier lines are excluded from the Principal count. None were found.

### By month

| Month | Customers | 0 known Principal | 1 Principal | 2 Principals | 3+ Principals | More than 1 | Min | Max | Avg |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| 2025-02 | 1,289 | 0 | 435 | 210 | 644 | 854 | 1 | 16 | 3.7199 |
| 2025-03 | 1,265 | 0 | 463 | 206 | 596 | 802 | 1 | 15 | 3.5502 |
| 2025-04 | 1,228 | 0 | 384 | 180 | 664 | 844 | 1 | 16 | 3.9682 |
| 2025-05 | 1,275 | 0 | 396 | 158 | 721 | 879 | 1 | 16 | 4.1600 |
| 2025-06 | 1,271 | 0 | 394 | 192 | 685 | 877 | 1 | 17 | 4.0858 |
| 2025-07 | 1,395 | 0 | 468 | 184 | 743 | 927 | 1 | 17 | 4.0351 |
| 2025-08 | 1,300 | 0 | 441 | 192 | 667 | 859 | 1 | 16 | 3.8754 |
| 2025-09 | 1,312 | 0 | 412 | 201 | 699 | 900 | 1 | 17 | 3.9253 |
| 2025-10 | 1,344 | 0 | 426 | 205 | 713 | 918 | 1 | 16 | 3.9561 |
| 2025-11 | 1,386 | 0 | 443 | 207 | 736 | 943 | 1 | 16 | 4.0837 |
| 2025-12 | 1,360 | 0 | 447 | 197 | 716 | 913 | 1 | 16 | 3.9846 |
| 2026-01 | 1,423 | 0 | 454 | 226 | 743 | 969 | 1 | 17 | 4.0857 |
| 2026-02 | 1,375 | 0 | 429 | 228 | 718 | 946 | 1 | 17 | 4.0167 |
| 2026-03 | 1,073 | 0 | 422 | 178 | 473 | 651 | 1 | 16 | 3.5303 |
| 2026-04 | 1,354 | 0 | 400 | 198 | 756 | 954 | 1 | 18 | 4.3213 |
| 2026-05 | 1,333 | 0 | 417 | 203 | 713 | 916 | 1 | 19 | 4.1418 |
| 2026-06 | 1,119 | 0 | 418 | 197 | 504 | 701 | 1 | 18 | 3.7301 |

### Trailing 12 months

| Measure | Result |
| --- | ---: |
| Customers | 1,953 |
| 0 known Principal | 0 |
| 1 Principal | 536 |
| 2 Principals | 223 |
| 3+ Principals | 1,194 |
| More than 1 Principal | 1,417 (72.55%) |
| Min / Max / Avg | 1 / 21 / 5.3938 |

---

## Check 3 and TQ-001 — Distinct Principals per Faktur

Question: what percentage of non-void Fakturs contain items from multiple Principals?

| Measure | Result |
| --- | ---: |
| Non-void Fakturs | 119,513 |
| 0 known Principals | 0 |
| 1 Principal | 101,423 |
| 2 Principals | 13,202 |
| 3 Principals | 3,874 |
| 4 Principals | 873 |
| 5 Principals | 110 |
| 6 Principals | 29 |
| 7 Principals | 2 |
| Mixed-Principal Fakturs | 18,090 |
| Mixed-Principal rate | 15.14% |
| Max Principals per Faktur | 7 |

TQ-001 result: 15.14% of non-void Fakturs contain items from more than one Principal. This does not change the approved Invoice Item evidence grain.

---

## Check 4 and TQ-002 — Salesman × Principal target match

Pair grain: distinct non-void Faktur × Salesman × known Item Principal. Match is `BTR_SalesPersonPrincipalTarget` for that Faktur's year and month.

| Measure | Result |
| --- | ---: |
| Known Faktur × Salesman × Principal pairs | 143,679 |
| Pairs with a matching target | 5,074 |
| Pairs lacking a matching target | 138,605 |
| Lacking-target rate | 96.47% |

`BTR_SalesPersonPrincipalTarget` contains rows for 2026-06 only. Every month before 2026-06 is 100% lacking because no target row exists for that month.

| Month | Known pairs | Lacking target |
| --- | ---: | ---: |
| 2025-02 | 7,032 | 7,032 |
| 2025-03 | 7,325 | 7,325 |
| 2025-04 | 7,683 | 7,683 |
| 2025-05 | 8,739 | 8,739 |
| 2025-06 | 8,584 | 8,584 |
| 2025-07 | 9,649 | 9,649 |
| 2025-08 | 8,436 | 8,436 |
| 2025-09 | 8,852 | 8,852 |
| 2025-10 | 9,137 | 9,137 |
| 2025-11 | 9,387 | 9,387 |
| 2025-12 | 9,147 | 9,147 |
| 2026-01 | 9,705 | 9,705 |
| 2026-02 | 9,189 | 9,189 |
| 2026-03 | 6,175 | 6,175 |
| 2026-04 | 9,658 | 9,658 |
| 2026-05 | 9,236 | 9,236 |
| 2026-06 | 5,745 | 671 |

TQ-002 result: 96.47% of observed Faktur Salesman × Principal pairs lack a matching monthly target. In the only populated target month, 2026-06, 671 of 5,745 pairs lack a target (11.68%). This does not change the approved monthly target-responsibility rule. Missing targets remain data-quality exceptions.

---

## Check 5 and TQ-004 — Line total versus Faktur GrandTotal

Line total is `SUM(BTR_FakturItem.Total)` for the Faktur. Difference is that sum minus `BTR_Faktur.GrandTotal`.

| Measure | Result |
| --- | ---: |
| Non-void Fakturs | 119,513 |
| Exact match | 65,033 |
| Absolute difference greater than 0 and under 1 | 54,480 |
| Absolute difference 1 to under 100 | 0 |
| Absolute difference 100 to under 1,000 | 0 |
| Absolute difference 1,000 or more | 0 |
| Minimum difference | -0.36 |
| Maximum difference | 0.00 |
| Average difference | -0.02 |

TQ-004 result: every non-void Faktur reconciles `SUM(FakturItem.Total)` to `GrandTotal` within 1. There is no material outlier. This does not change the approved rule that Principal Sales-Out uses line `SubTotal - DiscRp` and does not require header allocation.

---

## Check 6 — Unknown or blank Supplier rate on sold items

| Measure | Result |
| --- | ---: |
| Sold item lines | 946,868 |
| Known Supplier lines | 946,868 |
| Blank Supplier lines | 0 |
| Unknown Supplier lines | 0 |
| Missing Item lines | 0 |
| Unknown or blank line rate | 0.00% |
| Known Sales-Out DPP (`SubTotal - DiscRp`) | 128,189,793,882.47 |
| Unknown or blank Sales-Out DPP | 0.00 |

Unknown or blank Supplier rate on sold items is zero in this source. No synthetic Principal is required by this measurement.

---

## Check 7 — Target coverage by Salesman, Principal, and company

Target source: `BTR_SalesPersonPrincipalTarget`. There is no independent company target record. Company target amount is the sum of Salesman Principal Target amounts.

| Measure | Result |
| --- | ---: |
| Target rows | 168 |
| Target year-month span | 2026-06 only |
| Salesmen with any target | 33 |
| Principals with any target | 21 |
| Sum of target amount | 7,312,614,080.33 |

Trailing 12 months, known-Principal sales:

| Measure | Result |
| --- | ---: |
| Salesmen with sales | 37 |
| Principals with sales | 25 |
| Salesman × Principal × month combinations | 1,729 |
| Those combinations with a matching target | 125 |
| Those combinations lacking a target | 1,604 |
| Combination coverage rate | 7.23% |
| Salesmen with sales and at least one target row | 33 of 37 |
| Principals with sales and at least one target row | 21 of 25 |
| Company target amount in the window | 7,312,614,080.33, all in 2026-06 |

June 2026 only:

| Measure | Result |
| --- | ---: |
| Salesmen with sales | 33 |
| Principals with sales | 22 |
| Distinct Salesman × Principal pairs | 171 |
| Pairs lacking a June target | 46 |
| Pair coverage rate | 73.10% |

Company coverage is therefore the June 2026 Salesman allocation total. Months other than 2026-06 have sales and no target rows.

---

## Check 8 and TQ-008 — Expected relationship population and activity

Expected `BTRPD_CustomerPrincipalRelationship` population is the distinct Customer × known Principal pair observed on at least one non-void Faktur item. History is all available non-void history. Active uses the approved 6-month last-transaction rule against as-of 2026-09-09.

| Measure | Result |
| --- | ---: |
| Expected pair population | 11,912 |
| Customers in that population | 2,108 |
| Principals in that population | 27 |
| First transaction | 2025-02-07 |
| Last transaction | 2026-06-24 |
| Active pairs, last transaction within 6 months | 8,263 |
| Dormant pairs | 3,649 |
| Active rate | 69.37% |

Last-transaction activity against as-of 2026-09-09:

| Last transaction | Pairs |
| --- | ---: |
| 0–1 month | 0 |
| 1–3 months | 3,727 |
| 3–6 months | 4,536 |
| 6–12 months | 2,271 |
| 12–36 months | 1,378 |
| Older than 36 months | 0 |

The 0–1 month bucket is empty because the latest non-void invoice is 2026-06-24, which is more than one month before the as-of date.

Retention-window sensitivity, using last transaction only. The approved rule remains 6 months. Other windows are sensitivity only.

| Window | Active pairs |
| --- | ---: |
| 3 months | 3,727 |
| 6 months | 8,263 |
| 12 months | 10,534 |
| 36 months | 11,912 |
| All available history | 11,912 |

TQ-008 result: expected Customer × Principal population is 11,912 pairs. 36-month backfill volume equals all available history: 11,912 pairs, 119,513 non-void Fakturs, and 946,868 known-Principal item lines, covering 2,108 Customers and 27 Principals. No invoice history exists before 2025-02-07, so a 36-month request cannot be filled beyond this span.

---

## Check 9 and TQ-003 — Item SupplierId change evidence

Measured database objects:

| Check | Result |
| --- | --- |
| Tables named like Brg/Supplier/Item history, audit, or log | None |
| Triggers on `BTR_Brg` | None |
| Change data capture on `BTR_Brg` | Not enabled |
| Temporal history on `BTR_Brg` | Not enabled |

`BTR_Brg` stores the current `SupplierId` only. Item update writes replace `SupplierId` in place and do not retain a prior value.

TQ-003 result: no historical source exists for Item `SupplierId` change. How often an item's Principal changed cannot be measured from retained history. This does not change the approved Item-master attribution rule.

---

## TQ-005 — Customers with more than one Salesman and more than one Principal

| Window | Customers | More than 1 Salesman and more than 1 Principal |
| --- | ---: | ---: |
| Trailing 12 months | 1,953 | 1,296 |
| Calendar 2025 | 1,979 | 1,222 |
| Calendar 2026 through latest non-void invoice | 1,851 | 1,217 |

Monthly counts are in Check 1 combined with Check 2. Each measured month has hundreds of Customers in both conditions. The approved many-to-many model is consistent with this measurement.

---

## Authority statement

These findings do not change GAP-001 through GAP-023.

These findings do not change the Principal KPI Registry.

No application behavior, schema, or KPI definition was changed by this slice.
