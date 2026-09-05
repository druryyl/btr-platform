# SA02 — Sales Forecast Dashboard KPI Explanation

## Scope

SA02 helps management answer:

> If the current invoiced-sales pace continues, where will sales finish at
> month-end, and what daily pace is required to reach the target?

The dashboard uses official, non-voided **Faktur** for the current calendar
month and the monthly sales target. Forecasts are deterministic pace
projections, not AI or machine-learning predictions. Calendar days are used,
including weekends and holidays.

## 1. Actual vs Forecast

### 1.1 Current Sales

1. **KPI Canonical:** `SA-KPI-009` — Current Sales

2. **Question Answered:**  
   How much sales value has been invoiced so far this month?

3. **Definition:**  
   Month-to-date invoiced omzet from official, non-voided Faktur as of the
   business date.

4. **Business Meaning:**  
   This is the actual sales result delivered so far. It is the starting point
   for the month-end forecast and should reconcile with the Sales Dashboard.

5. **How To Interpret:**

   - A rising value indicates that current-month billing is progressing.
   - Compare it with **Current Achievement** to understand progress against
     the plan.
   - Compare it with **Forecast Sales** to see how the current result may
     finish if the pace continues.

### 1.2 Current Achievement

1. **KPI Canonical:** `SA-KPI-010` — Current Achievement %

2. **Question Answered:**  
   How much of the monthly sales target has been achieved so far?

3. **Definition:**  
   The percentage of the monthly target represented by current month-to-date
   invoiced sales.

   ```text
   Current Achievement = Current Sales ÷ Total Target × 100%
   ```

   When **Total Target** is zero, the result is unknown because actual
   performance cannot be evaluated against a configured plan.

4. **Business Meaning:**  
   This is the backward-looking view of sales performance. It tells
   management how much of the plan has already been delivered, independent of
   what may happen during the remaining days.

5. **How To Interpret:**

   - A higher percentage means more of the monthly plan has been delivered.
   - A low percentage late in the month indicates that recovery action may be
     needed.
   - Read it together with **Forecast Achievement**, because a reasonable
     current percentage can still lead to a projected miss if recent pace is
     slowing.

### 1.3 Forecast Sales

1. **KPI Canonical:** `SA-KPI-011` — Forecast Sales (Expected)

2. **Question Answered:**  
   Where will invoiced sales finish at month-end if the current pace
   continues?

3. **Definition:**  
   The expected month-end invoiced sales based on the average daily sales
   achieved so far.

   ```text
   Forecast Sales = (Current Sales ÷ Days Elapsed) × Days in Month
   ```

   Days elapsed has a minimum value of one to provide a usable early-month
   projection.

4. **Business Meaning:**  
   Forecast Sales gives management an early warning about the likely
   month-end result. It creates time to increase sales activity while there
   are still days available to influence the outcome.

5. **How To Interpret:**

   - A forecast at or above target suggests the current pace is sufficient.
   - A forecast below target indicates a projected shortfall if no corrective
     action is taken.
   - The forecast is less reliable early in the month; check
     **Forecast Confidence** before making a strong conclusion.

### 1.4 Forecast Achievement

1. **KPI Canonical:** `SA-KPI-012` — Forecast Achievement %

2. **Question Answered:**  
   What percentage of the monthly target is the business expected to achieve
   by month-end?

3. **Definition:**  
   The projected month-end sales expressed as a percentage of the monthly
   target.

   ```text
   Forecast Achievement = Forecast Sales ÷ Total Target × 100%
   ```

   When **Total Target** is zero, the result is unknown.

4. **Business Meaning:**  
   This is the forward-looking plan-attainment signal. It helps management
   decide whether to maintain the current sales approach, intensify execution,
   or escalate recovery action.

5. **How To Interpret:**

   - **100% or higher:** The current projection reaches or exceeds target.
   - **80%–99%:** The projection is below target and needs attention.
   - **Below 80%:** The projected miss is significant and may require urgent
     intervention.
   - Confirm the result with **Forecast Confidence**, especially during the
     first five days of the month.

## 2. Pace and Gap

### 2.1 Daily Average Sales

1. **KPI Canonical:** `SA-KPI-013` — Daily Average Sales

2. **Question Answered:**  
   What has been the average daily invoiced-sales pace so far this month?

3. **Definition:**  
   Month-to-date invoiced sales divided by the number of calendar days
   elapsed.

   ```text
   Daily Average Sales = Current Sales ÷ Days Elapsed
   ```

4. **Business Meaning:**  
   This is the observed sales pace used as the baseline for forecasting. It
   also gives management a practical benchmark for judging whether the
   required recovery pace is realistic.

5. **How To Interpret:**

   - Compare it with **Required Daily Sales** to see whether the target can be
     reached at a similar pace.
   - A falling average suggests that the month-end forecast may weaken.
   - It is also shown as a reference line on the **Daily Pace Trend** chart.

### 2.2 Required Daily Sales

1. **KPI Canonical:** `SA-KPI-014` — Required Daily Sales

2. **Question Answered:**  
   How much must the business invoice on each remaining day to reach the
   monthly target?

3. **Definition:**  
   The average daily invoiced sales needed over the remaining calendar days.

   ```text
   Required Daily Sales = (Total Target − Current Sales) ÷ Days Remaining
   ```

   The value is zero when the target has already been reached or there are no
   days remaining.

4. **Business Meaning:**  
   This converts the remaining target gap into an operational daily
   requirement. It is one of the most actionable numbers on SA02 because it
   tells sales leadership what pace must be delivered from today onward.

5. **How To Interpret:**

   - If it is close to or below **Daily Average Sales**, the target may be
     achievable without a major change in pace.
   - If it is materially higher, management should identify additional
     customers, orders, or sales activities.
   - More than 1.5 times the daily average is a warning; more than 2 times is
     critical.

### 2.3 Target Gap

1. **KPI Canonical:** `SA-KPI-015` — Target Gap

2. **Question Answered:**  
   At the current pace, how far above or below target is the projected
   month-end result?

3. **Definition:**  
   The monthly target less the forecasted month-end sales.

   ```text
   Target Gap = Total Target − Forecast Sales
   ```

4. **Business Meaning:**  
   Target Gap expresses the forecast risk in monetary terms. Management can
   use it to understand the value of sales that still needs to be recovered,
   rather than relying only on a percentage.

5. **How To Interpret:**

   - A positive value is a projected shortfall.
   - Zero means the forecast is exactly on target.
   - A negative value indicates projected sales above target.
   - A positive gap should be read together with **Required Daily Sales** and
     **Days Remaining** to assess whether recovery is feasible.

### 2.4 Days Remaining

1. **KPI Canonical:** `SA-KPI-016` — Days Remaining

2. **Question Answered:**  
   How much calendar time is left to close the projected sales gap?

3. **Definition:**  
   The number of calendar days from the business date through the end of the
   month, excluding the current date from the remaining-day count.

4. **Business Meaning:**  
   This provides the time context for every pace and gap measure. A fixed
   target gap becomes more difficult to recover as fewer days remain.

5. **How To Interpret:**

   - A larger value means there is more time to influence the result.
   - A small value combined with a high **Required Daily Sales** is a strong
     signal that immediate action is needed.
   - Weekends and holidays are included, so management should consider actual
     selling capacity when judging feasibility.

## 3. Scenario and Confidence

### 3.1 Best Case

1. **KPI Canonical:** `SA-KPI-017` — Scenario Projection (Best / Expected /
   Worst)

2. **Question Answered:**  
   What could month-end sales reach if the stronger recent pace is sustained?

3. **Definition:**  
   The higher of the month-to-date daily average and the recent seven-day
   daily average, projected through the end of the month.

   ```text
   Best Case = MAX(MTD Daily Average, Recent-7-Day Average) × Days in Month
   ```

4. **Business Meaning:**  
   Best Case shows the upside available when recent momentum is maintained or
   when the business returns to its stronger observed pace. It supports
   opportunity planning without treating the upside as the expected result.

5. **How To Interpret:**

   - If Best Case reaches target while Expected does not, recovery is possible
     but requires stronger execution.
   - A narrow difference between Best Case and Expected suggests stable pace.
   - A wide range signals greater uncertainty and the need to inspect the
     daily and weekly trends.

### 3.2 Expected

1. **KPI Canonical:** `SA-KPI-017` — Scenario Projection (Best / Expected /
   Worst)

2. **Question Answered:**  
   What is the central month-end sales projection based on the current
   month-to-date pace?

3. **Definition:**  
   Expected is the primary **Forecast Sales** projection.

   ```text
   Expected = Forecast Sales
   ```

4. **Business Meaning:**  
   Expected is the most direct answer to whether the current sales approach is
   sufficient. It should be the first scenario management reads before
   considering upside or downside conditions.

5. **How To Interpret:**

   - Compare Expected with **Total Target** to determine the projected
     month-end position.
   - Compare it with Best Case and Worst Case to understand the range around
     the central projection.
   - Use **Required Daily Sales** to determine the action needed when Expected
     is below target.

### 3.3 Worst Case

1. **KPI Canonical:** `SA-KPI-017` — Scenario Projection (Best / Expected /
   Worst)

2. **Question Answered:**  
   What could month-end sales look like if the weaker observed pace
   continues?

3. **Definition:**  
   The lower of the month-to-date daily average and the recent seven-day daily
   average, projected through the end of the month.

   ```text
   Worst Case = MIN(MTD Daily Average, Recent-7-Day Average) × Days in Month
   ```

   When fewer than seven days have elapsed, the recent-seven-day average
   falls back to the month-to-date average.

4. **Business Meaning:**  
   Worst Case highlights downside exposure if momentum deteriorates. It helps
   management prepare early for a larger target miss or decide whether the
   sales plan needs escalation.

5. **How To Interpret:**

   - A Worst Case substantially below target indicates material downside risk.
   - If Worst Case and Expected are close, the forecast is less sensitive to
     recent momentum.
   - Use the **Daily Pace Trend** and **Weekly Pace** to investigate why the
     downside scenario is emerging.

### 3.4 Forecast Confidence

1. **KPI Canonical:** `SA-KPI-018` — Forecast Confidence

2. **Question Answered:**  
   How much evidence is available to trust the current sales projection?

3. **Definition:**  
   A time-based reliability indicator for the pace forecast:

   | Days elapsed | Confidence |
   | ------------ | ---------- |
   | 1–5 | Low |
   | 6–20 | Medium |
   | 21 or more | High |

4. **Business Meaning:**  
   Confidence prevents management from overreacting to a small amount of
   early-month data. A forecast becomes more informative as more selling days
   contribute to the observed pace.

5. **How To Interpret:**

   - **Low:** Treat the forecast and scenarios as preliminary.
   - **Medium:** Use the forecast for active monitoring and corrective
     planning.
   - **High:** The projection is based on a longer observed period and is more
     useful for month-end decisions.
   - Confidence describes the reliability of the projection; it does not mean
     that target achievement is guaranteed.

## 4. Charts

### 4.1 Daily Pace Trend

1. **KPI Canonical:** Not Found as a standalone KPI. The chart uses
   `SA-KPI-013` — Daily Average Sales as its reference and supports the
   `SA-KPI-006` — Weekly Invoiced Sales Trend context.

2. **Question Answered:**  
   Is daily invoiced sales pace stable, improving, or weakening during the
   month?

3. **Definition:**  
   A bar chart showing daily invoiced omzet for the current month, with the
   month-to-date daily average as a reference line. Future-dated Faktur are
   excluded from the daily display.

4. **Business Meaning:**  
   The chart reveals volatility and momentum that a single month-to-date total
   can hide. It helps management determine whether the current forecast is
   supported by consistent daily execution.

5. **How To Interpret:**

   - Bars consistently above the reference line indicate stronger recent
     pace.
   - Bars consistently below it indicate weakening momentum and possible
     forecast deterioration.
   - Compare recent bars with **Required Daily Sales** to see whether the
     required recovery pace is being achieved.

### 4.2 Forecast vs Target

1. **KPI Canonical:** Not Found as a standalone KPI. It is a visual
   comparison built from `EX-KPI-003` — Total Target,
   `SA-KPI-009` — Current Sales, and `SA-KPI-011` — Forecast Sales.

2. **Question Answered:**  
   How does current sales and the projected month-end result compare with the
   monthly target?

3. **Definition:**  
   A three-bar comparison of **Total Target**, **Current Sales**, and
   **Forecast Sales**.

4. **Business Meaning:**  
   This chart gives management a fast visual separation between what has
   already been delivered, what is expected by month-end, and what the plan
   requires.

5. **How To Interpret:**

   - Current Sales below target is normal early in the month; compare the
     Forecast bar with target for the forward-looking conclusion.
   - Forecast below target indicates a projected shortfall.
   - Forecast at or above target indicates that the current pace is projected
     to meet or exceed the plan.

### 4.3 Weekly Pace

1. **KPI Canonical:** `SA-KPI-006` — Weekly Invoiced Sales Trend

2. **Question Answered:**  
   Is weekly invoiced-sales momentum accelerating, stable, or slowing?

3. **Definition:**  
   Weekly invoiced omzet totals for calendar weeks within the current month.
   SA02 presents this as context for the forecast; it is the same weekly trend
   used by the Sales Dashboard.

4. **Business Meaning:**  
   Weekly Pace adds momentum and timing context to the forecast. Two months
   can have the same current achievement but require different action when one
   is accelerating and the other is slowing.

5. **How To Interpret:**

   - Increasing weekly values generally support a stronger forecast.
   - Declining weekly values may indicate weakening demand or sales execution.
   - A strong early week followed by weaker weeks is a warning even when
     **Current Achievement** appears acceptable.
   - Read it together with **Forecast Confidence**, **Forecast Achievement**,
     and **Required Daily Sales**.

## Management Reading Order

1. Start with **Current Sales** and **Current Achievement** to understand what
   has been delivered.
2. Read **Forecast Sales** and **Forecast Achievement** to see the likely
   month-end result.
3. Compare **Daily Average Sales** with **Required Daily Sales** to judge the
   recovery effort.
4. Use **Target Gap** and **Days Remaining** to size the urgency.
5. Review Best Case, Expected, Worst Case, and **Forecast Confidence** to
   understand the range and reliability.
6. Use the charts to validate momentum and investigate the cause of a projected
   miss.

