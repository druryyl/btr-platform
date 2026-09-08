# FI03 — Cash Flow Forecast Dashboard KPI Explanation

## Scope

**FI03 — Cash Flow Forecast** answers:

> If current collection performance continues, how much cash will we likely
> receive by month-end, will recovery keep up with billing, and which risks
> require collection attention?

**Route:** `/dashboard/cash-flow-forecast`

This is a read-only management view for the Owner, Director, General Manager,
Finance, Collection, and Sales managers. Monetary values are shown in IDR.

The forecast is indicative. It is a planning aid, not a guarantee of cash
receipt, a probability-of-default score, or an automatic credit decision.

## KPI Reference Note

The reviewed KPI Encyclopedia does not contain FI03 Finance KPI entries. Each
KPI below therefore states **KPI Canonical: Not Found** when no canonical
entry exists there. The corresponding portal KPI catalog identifier is listed
separately where one has been defined.

## Important Reading Rules

- The dashboard covers the current calendar month through the business date.
- **Cash** means cash received from customer settlements.
- **Total collections** include cash and other accepted settlement methods,
  such as Giro. Total collections are used when comparing recovery with
  billing.
- Forecasts use the current calendar-day collection pace. Weekends and
  holidays are included; this is not a working-day forecast.
- The implicit collection target is current-month billing. There is no
  separate collection-target master in this view.
- A low forecast early in the month may reflect limited elapsed-day data.
  Always read it together with Forecast Confidence.
- Risk rows are deterministic business signals. They are prompts for review,
  not proof that a customer will fail to pay.

## 1. Header and Period Context

The header identifies the page as **Cash Flow Forecast Dashboard** and shows
the data freshness and period context:

- Generated timestamp
- Current month and year
- Day elapsed versus total days in the month
- Business date used as the “as of” date

This context is important because the same collection amount has a different
meaning on day 3, day 15, and the final day of the month.

## 2. Executive Summary

The Executive Summary is a short, plain-language statement of the current
liquidity outlook. It brings together:

- Expected month-end cash
- Projected total collections
- Collection Forecast %
- Forecast confidence
- The relevant healthy, warning, or critical interpretation

### Business Use

Management should use this as the first orientation point, then verify the
underlying KPIs and risk rows before taking action. The summary is intended to
answer:

> What is the current collection outlook, and does it need management
> intervention?

An early-month summary may explicitly indicate that the forecast is not yet
stable because too few collection days have elapsed.

## 3. Cash Position KPIs

This section describes actual cash received and the projected month-end cash
position.

### 3.1 Cash Collected MTD

1. **KPI Canonical:** Not Found in KPI Encyclopedia  
   **Portal KPI catalog:** FI-KPI-016 — Cash Collected MTD

2. **Question Answered:**

   How much cash has actually been received from customers this month?

3. **Definition:**

   The total cash settlement received from the beginning of the current
   calendar month through the business date.

4. **Business Meaning:**

   This is the realized liquidity contribution from collection activity. It
   tells management how much money has actually entered the business, rather
   than how much has merely been billed or promised.

5. **How To Interpret:**

   - A high value indicates stronger realized cash recovery, but should be
     compared with current-month billing and the remaining collection target.
   - A low value may be normal at the beginning of the month or may indicate
     weak collection activity.
   - A flat value while billing continues can indicate increasing working
     capital pressure.
   - Compare it with Expected Cash Collection, Recovery vs Billing (Actual),
     and Overdue Outstanding.

### 3.2 Expected Cash Collection

1. **KPI Canonical:** Not Found in KPI Encyclopedia  
   **Portal KPI catalog:** FI-KPI-029 — Expected Cash Collection

2. **Question Answered:**

   How much cash will likely be received by month-end if the current pace
   continues?

3. **Definition:**

   The current average daily cash collection multiplied by the number of days
   in the month:

   ```text
   Expected Cash Collection =
     Cash Collected MTD ÷ Days Elapsed × Days in Month
   ```

4. **Business Meaning:**

   This is the primary forward-looking cash estimate. It helps management
   anticipate whether collection activity is likely to provide sufficient
   liquidity before month-end.

5. **How To Interpret:**

   - A high forecast is encouraging when it is supported by a high-confidence
     period and stable daily collection.
   - A low forecast means the current pace would produce limited month-end
     cash and may require additional collection action.
   - A forecast can change quickly early in the month because each new
     collection day materially changes the average.
   - Compare it with Billing and the Best / Expected / Worst Cash scenarios.

### 3.3 Projected Month-End Collection

1. **KPI Canonical:** Not Found in KPI Encyclopedia  
   **Portal KPI catalog:** FI-KPI-030 — Projected Month-End Collection

2. **Question Answered:**

   What is the expected cash position at the end of the month?

3. **Definition:**

   This is the same primary cash projection as Expected Cash Collection:

   ```text
   Projected Month-End Collection =
     Current daily cash pace × Days in Month
   ```

4. **Business Meaning:**

   It presents the month-end cash expectation as a headline planning number.
   The repeated presentation makes the expected month-end liquidity outcome
   easy to find in the Cash Position section.

5. **How To Interpret:**

   - Treat it as an estimate, not committed cash.
   - Compare it with current-month billing and the cash scenarios.
   - When the month is complete, the projection should converge to the actual
     month-to-date cash result.
   - Investigate a large difference between the forecast and the required
     collection pace.

### 3.4 Collection Forecast %

1. **KPI Canonical:** Not Found in KPI Encyclopedia  
   **Portal KPI catalog:** FI-KPI-031 — Collection Forecast %

2. **Question Answered:**

   If current collection performance continues, will total collections keep up
   with current-month billing?

3. **Definition:**

   The projected total collections at month-end divided by current-month
   billing:

   ```text
   Collection Forecast % =
     Projected Month-End Total Collections ÷ Month Faktur Omzet × 100
   ```

   The measure is unavailable when current-month billing is zero.

4. **Business Meaning:**

   This is the main forecast health indicator. It compares the expected
   recovery outcome with the value invoiced during the month.

5. **How To Interpret:**

   - **Healthy — 100% or higher:** projected collections are keeping up with
     billing.
   - **Warning — 80% to 99%:** projected collections may finish below billing.
   - **Critical — below 80%:** the projected recovery shortfall is material.
   - **Unknown:** there is no current-month billing benchmark.
   - This percentage uses total collections, not cash only. Read it together
     with the cash forecast when assessing actual liquidity.

## 4. Pace and Target KPIs

This section converts the forecast into a daily collection requirement and
shows how much time remains to close the gap.

### 4.1 Daily Cash Collection Average

1. **KPI Canonical:** Not Found in KPI Encyclopedia  
   **Portal KPI catalog:** FI-KPI-032 — Daily Cash Collection Average

2. **Question Answered:**

   What is the average cash collection pace so far this month?

3. **Definition:**

   Cash Collected MTD divided by the number of elapsed calendar days:

   ```text
   Daily Cash Collection Average =
     Cash Collected MTD ÷ Days Elapsed
   ```

4. **Business Meaning:**

   This is the baseline run-rate used to estimate expected month-end cash. It
   gives management a simple measure of how much cash collection the business
   is producing per day.

5. **How To Interpret:**

   - A rising average indicates improving collection momentum.
   - A falling or stagnant average indicates that additional effort may be
     needed to maintain the month-end outlook.
   - Compare it with Required Daily Collection to see whether the current pace
     is sufficient.
   - Use the Daily Collection Pace chart to determine whether the average is
     supported by consistent daily activity or only by one large receipt.

### 4.2 Required Daily Collection

1. **KPI Canonical:** Not Found in KPI Encyclopedia  
   **Portal KPI catalog:** FI-KPI-033 — Required Daily Collection

2. **Question Answered:**

   What average amount must be collected each remaining day to match billing?

3. **Definition:**

   The remaining collection target divided by the remaining calendar days:

   ```text
   Required Daily Collection =
     (Month Billing − Month Collections) ÷ Days Remaining
   ```

   The value is zero when collections already match or exceed billing, or when
   there are no remaining days.

4. **Business Meaning:**

   This is the most actionable pace KPI in the dashboard. It translates a
   month-end recovery gap into a daily operational requirement.

5. **How To Interpret:**

   - If it is close to or below the current daily collection average, the
     target is generally achievable at the present pace.
   - If it is materially higher than the current average, collection activity
     must improve.
   - The dashboard treats a requirement above approximately 1.5 times the
     daily average as a warning and above approximately 2 times as critical.
   - Use the Top Collection Risks table to identify which accounts may help
     close the requirement.

### 4.3 Remaining Collection Target

1. **KPI Canonical:** Not Found in KPI Encyclopedia  
   **Portal KPI catalog:** FI-KPI-034 — Remaining Collection Target

2. **Question Answered:**

   How much more must be collected to match current-month billing?

3. **Definition:**

   The difference between current-month billing and total collections to date:

   ```text
   Remaining Collection Target =
     Month Billing − Month Collections
   ```

   The value is shown as zero when collections have already reached or exceeded
   billing.

4. **Business Meaning:**

   This is the total amount still needed to keep recovery level with billing.
   It gives Finance and Collection a common amount to work toward before
   considering how to distribute the effort across customers.

5. **How To Interpret:**

   - A positive value means additional collections are needed.
   - A zero value means current collections have caught up with billing.
   - A large value is more urgent when few calendar days remain.
   - Read it together with Required Daily Collection and Collection Forecast %.

### 4.4 Remaining Calendar Days

1. **KPI Canonical:** Not Found in KPI Encyclopedia  
   **Portal KPI catalog:** FI-KPI-035 — Days Remaining

2. **Question Answered:**

   How much calendar time remains to collect before month-end?

3. **Definition:**

   The number of calendar days from the business date through the end of the
   current month. Weekends and holidays are included.

4. **Business Meaning:**

   This provides the time context needed to judge the size of the required
   daily collection pace.

5. **How To Interpret:**

   - A high remaining-day count provides more time to close a gap.
   - A low remaining-day count means the same target requires a much higher
     daily pace.
   - Never evaluate Required Daily Collection without considering this value.
   - This is not a count of working days or expected collection visits.

## 5. Recovery and Scenario KPIs

This section compares recovery with billing and shows the range and confidence
of the forecast.

### 5.1 Recovery vs Billing — Actual

1. **KPI Canonical:** Not Found in KPI Encyclopedia  
   **Portal KPI catalog:** FI-KPI-017 — Recovery vs Billing %

2. **Question Answered:**

   Is total collection recovery keeping up with new billing right now?

3. **Definition:**

   Current-month total collections divided by current-month billing:

   ```text
   Recovery vs Billing (Actual) =
     Month Collections ÷ Month Faktur Omzet × 100
   ```

4. **Business Meaning:**

   This is the current, realized recovery position. It shows whether the
   business is converting customer receivables into settlements at a pace
   comparable with new billing.

5. **How To Interpret:**

   - Around or above 100% means recovery is keeping pace with current-month
     billing.
   - Below 100% means billing is currently ahead of recovery.
   - A declining percentage is more concerning when Overdue Outstanding is
     also rising.
   - A strong percentage does not prove that old receivables are being
     recovered; review aging and overdue risk separately.

### 5.2 Recovery vs Billing — Forecast

1. **KPI Canonical:** Not Found in KPI Encyclopedia  
   **Portal KPI catalog:** FI-KPI-036 — Recovery vs Billing Forecast

2. **Question Answered:**

   Where is recovery likely to finish compared with billing at month-end?

3. **Definition:**

   The projected total collections at month-end divided by current-month
   billing. It uses the same calculation as Collection Forecast %.

4. **Business Meaning:**

   This is the forward-looking counterpart to the actual recovery percentage.
   Comparing both values shows whether current performance is likely to improve
   or worsen by month-end.

5. **How To Interpret:**

   - Forecast above actual recovery suggests that the current pace may improve
     the month-end position.
   - Forecast below the desired level indicates that current activity will not
     be enough without intervention.
   - Compare it with the Actual percentage rather than reading it alone.
   - Use the Recovery Trend chart to see whether the forecast is supported by
     the observed collection and billing movement.

### 5.3 Best / Expected / Worst Cash

1. **KPI Canonical:** Not Found in KPI Encyclopedia  
   **Portal KPI catalog:** FI-KPI-037 — Scenario Cash (Best / Expected / Worst)

2. **Question Answered:**

   What range of month-end cash outcomes is reasonable based on recent
   collection performance?

3. **Definition:**

   Three month-end cash scenarios are shown:

   - **Expected:** the current month-to-date daily cash pace continues.
   - **Best:** the stronger of the month-to-date pace and the recent
     seven-day pace continues.
   - **Worst:** the weaker of those two paces continues.

4. **Business Meaning:**

   A single forecast can create false certainty. The scenario range helps
   management understand the effect of recent collection momentum and prepare
   for both a stronger and weaker outcome.

5. **How To Interpret:**

   - A narrow range suggests recent performance is close to the month average.
   - A wide range indicates that recent collection behavior differs
     materially from the month-to-date pattern.
   - The Worst scenario is useful for liquidity caution and contingency
     planning.
   - The Best scenario should not be treated as the expected result or as a
     reason to reduce collection effort.

### 5.4 Forecast Confidence

1. **KPI Canonical:** Not Found in KPI Encyclopedia  
   **Portal KPI catalog:** FI-KPI-038 — Forecast Confidence (Cash)

2. **Question Answered:**

   How much historical collection activity is available to support this
   month-end forecast?

3. **Definition:**

   Confidence is based on elapsed calendar days:

   - **Low:** 1–5 elapsed days
   - **Medium:** 6–20 elapsed days
   - **High:** 21 or more elapsed days

4. **Business Meaning:**

   This prevents management from treating a mathematically precise early-month
   estimate as a reliable commitment. More elapsed days provide a broader
   observation period for the current pace.

5. **How To Interpret:**

   - **Low:** use the forecast directionally and avoid overreacting to a small
     number of receipts.
   - **Medium:** use the forecast for active planning while monitoring daily
     movement.
   - **High:** the forecast is more stable, although business events can still
     change the outcome.
   - Day 1 with no cash collected produces a zero cash forecast and low
     confidence.

## 6. Receivable Context KPIs

These KPIs provide the receivable context behind the cash forecast. They show
near-term collectible balances, overdue exposure, and the projected gap.

### 6.1 Outstanding Due Remaining

1. **KPI Canonical:** Not Found in KPI Encyclopedia  
   **Portal KPI catalog:** FI-KPI-039 — Outstanding Due Remaining

2. **Question Answered:**

   How much open receivable is scheduled to become due during the remaining
   part of this month?

3. **Definition:**

   The open customer balance whose due date is after the business date and on
   or before the end of the current month.

4. **Business Meaning:**

   This is the near-term receivable pipeline that may support the remaining
   collection target under normal customer payment behavior. It adds context
   to the cash pace but is not itself a promise of payment.

5. **How To Interpret:**

   - A high value indicates a substantial amount is scheduled to become
     collectible before month-end.
   - A low value means the business may need to recover overdue balances or
     accelerate other collection opportunities.
   - Compare it with Cash Collected MTD to understand how much of the
     near-term pipeline has already converted to cash.
   - Validate important customers and due dates in the Piutang Report.

### 6.2 Overdue Outstanding

1. **KPI Canonical:** Not Found in KPI Encyclopedia for this FI03 display label  
   **Portal KPI catalog:** FI-KPI-013 — Overdue Exposure

2. **Question Answered:**

   How much customer receivable is already past due and creating collection
   pressure?

3. **Definition:**

   The total outstanding balance in overdue aging categories. Current,
   not-yet-due balances are excluded.

4. **Business Meaning:**

   This shows the size of the existing late-payment problem behind the
   forecast. A positive cash forecast does not remove the need to manage
   overdue exposure.

5. **How To Interpret:**

   - A high value means more company working capital is tied up in late
     receivables.
   - A rising value together with weakening Recovery vs Billing is a stronger
     warning than either measure alone.
   - Review the age and customer concentration of the overdue amount.
   - Use the Top Collection Risks table and Piutang Report to identify the
     accounts requiring action.

### 6.3 Collection Gap

1. **KPI Canonical:** Not Found in KPI Encyclopedia  
   **Portal KPI catalog:** FI-KPI-040 — Collection Gap

2. **Question Answered:**

   How far below or above billing are projected total collections at month-end?

3. **Definition:**

   The difference between current-month billing and projected month-end total
   collections:

   ```text
   Collection Gap =
     Month Billing − Projected Month-End Total Collections
   ```

   A negative result indicates projected collections are higher than billing.

4. **Business Meaning:**

   This is the projected recovery shortfall or surplus. It directly connects
   the month-end forecast to the amount of billing that must be covered.

5. **How To Interpret:**

   - A positive value means projected collections are below billing.
   - A zero value means projected collections are expected to match billing.
   - A negative value means projected collections are expected to exceed
     billing.
   - A positive gap is a warning; a gap greater than approximately 20% of
     billing is critical.
   - Use Required Daily Collection and Top Collection Risks to plan the
     response.

### 6.4 Forecast Variance — Cash

1. **KPI Canonical:** Not Found

2. **Question Answered:**

   How much additional cash is expected between today and month-end?

3. **Definition:**

   The expected month-end cash projection less cash already collected:

   ```text
   Forecast Variance (Cash) =
     Expected Cash Collection − Cash Collected MTD
   ```

4. **Business Meaning:**

   This separates cash already received from cash that the current pace
   suggests may still arrive. It helps management understand how much of the
   projected result depends on future collection activity.

5. **How To Interpret:**

   - A high value means a large portion of the forecast still depends on
     future receipts.
   - A low value means the current result is already close to the projected
     month-end outcome.
   - Read it with Forecast Confidence: a large future amount with low
     confidence should be treated cautiously.
   - Use the Daily Collection Pace chart to assess whether the required future
     cash is supported by recent activity.

## 7. Chart — Daily Collection Pace

**Chart type:** Bar chart with a reference line.

### Content

- Horizontal axis: calendar day of the month
- Bars: actual daily cash collection for elapsed days
- Dashed line: month-to-date daily cash collection average
- Calendar horizon: all days in the current month
- Visual distinction between elapsed and remaining days

### Business Question

> Is daily cash collection consistent with the pace required by the
> month-end forecast?

### Business Meaning

The chart reveals the behavior behind the average. A total cash amount can
look acceptable while daily collection is irregular or dependent on one large
receipt. The chart helps management see whether collection activity is
consistent enough to support the expected outcome.

### How To Read It

- Bars consistently above the reference line indicate stronger recent
  collection than the month average.
- Bars consistently below the line indicate weakening pace.
- A single unusually large bar should not be treated as a repeatable daily
  run-rate without supporting evidence.
- Compare the chart with Required Daily Collection when deciding whether
  additional collection action is needed.

## 8. Chart — Cash Forecast vs Billing

**Chart type:** Three-bar comparison.

### Content

- **Billing:** current-month Faktur Omzet
- **Cash MTD:** cash collected so far
- **Projected Cash:** expected month-end cash

### Business Question

> How does actual cash and projected cash compare with the value billed this
> month?

### Business Meaning

The chart gives management a quick liquidity comparison:

- Billing shows the value that must be recovered to keep pace.
- Cash MTD shows what has already arrived.
- Projected Cash shows the expected cash outcome if the current pace continues.

### How To Read It

- A Projected Cash bar close to Billing suggests the current cash pace may
  support the billed amount.
- A large gap between Projected Cash and Billing indicates potential
  month-end recovery pressure.
- A large gap between Cash MTD and Projected Cash means a substantial amount
  of the forecast still depends on future collections.
- This chart uses the cash forecast; use Collection Forecast % to assess total
  collections including non-cash settlement methods.

## 9. Chart — Recovery Trend

**Chart type:** Cumulative line chart.

### Content

- Horizontal axis: calendar day of the month
- Line 1: cumulative total collections
- Line 2: cumulative billing
- Values shown through elapsed days in the current period

### Business Question

> Is collection recovery building at a pace that keeps up with billing as the
> month progresses?

### Business Meaning

The trend shows the relationship between recovery and billing over time rather
than only the latest totals. It helps management identify whether the
collection gap is stable, widening, or narrowing.

### How To Read It

- Collections moving close to or above billing indicates healthier recovery
  momentum.
- Billing rising faster than collections indicates that the recovery gap is
  widening.
- A widening gap requires attention even when absolute collections are
  increasing.
- Use Recovery vs Billing (Actual), Recovery vs Billing (Forecast), and
  Required Daily Collection together with this chart.

## 10. Table — Top Collection Risks

**Table purpose:** Identify the highest-priority customers or situations that
could prevent the forecast from being achieved.

**KPI Canonical:** Not Found in KPI Encyclopedia  
**Portal KPI catalog:** FI-KPI-041 — Top Collection Risks (Table)

### Visible Content

Each row displays:

- **Risk Type:** the collection risk category
- **Entity:** the customer or wilayah involved
- **Amount:** relevant overdue or due-soon amount
- **Due / Aging:** due-date or aging context
- **Rule:** short explanation of why the row was selected
- **Investigate:** link to supporting Piutang evidence when available

The table is priority ordered and contains at most ten rows.

### Risk Categories

The table may identify:

1. **Large Invoice Due Soon** — a large balance approaching its due date.
2. **Chronic Overdue — Large** — a large customer balance in the oldest
   overdue category.
3. **Collection Concentration Risk** — a customer represents a material share
   of company overdue exposure.
4. **Legacy Debt — Overdue** — an inactive customer still carries overdue
   receivables.
5. **Plafond Breach — Due Soon** — credit-limit pressure coincides with an
   upcoming due balance.
6. **Deteriorating — Low Recovery** — an overdue customer is associated with a
   low-recovery collection portfolio.
7. **Wilayah Hotspot — Due Exposure** — a territory has a material
   concentration of due-soon exposure.
8. **Expected Overdue Growth** — overdue exposure has no recent supporting cash
   collection activity.

### Business Question

> Which named accounts or situations should Collection and management review
> before the forecast deteriorates?

### Business Meaning

The table turns an aggregate forecast into an action list. It helps
management focus on the customers, territories, and aging situations most
likely to affect near-term recovery.

### How To Use The Table

1. Start with the highest-priority risk type and the largest relevant amount.
2. Check whether the same customer appears in multiple risk categories.
3. Review due date, aging, plafond position, and collection ownership.
4. Open the Piutang Report to validate invoice-level evidence.
5. Assign a collection action and owner; do not treat the risk label as a
   final credit or write-off decision.

The table is a deterministic prioritization aid. It does not calculate a
probability that a customer will default.

## 11. Traceability Footer

The footer explains the relationship between the forecast and the supporting
dashboards:

- The forecast is based on customer settlements through the business date.
- Cash Collected MTD matches the Collection Dashboard.
- Billing matches the Sales Dashboard.
- **Piutang Report** provides receivable evidence.
- **Collection Dashboard** provides current recovery and collection context.

The footer is supporting navigation, not an additional KPI, chart, or table.

## 12. Recommended Management Reading Order

1. Confirm the generated timestamp, business date, and Day X of Y context.
2. Read the Executive Summary for the current liquidity outlook.
3. Check Expected Cash Collection and Collection Forecast %.
4. Compare Daily Cash Collection Average with Required Daily Collection.
5. Review Remaining Collection Target and Remaining Calendar Days.
6. Compare Recovery vs Billing (Actual) with Recovery vs Billing (Forecast).
7. Check Forecast Confidence before relying on the projection.
8. Review Overdue Outstanding and Outstanding Due Remaining for receivable
   context.
9. Use the Daily Collection Pace and Recovery Trend charts to validate the
   direction of the forecast.
10. Open Top Collection Risks and verify important accounts in the Piutang
    Report.

## 13. Related Dashboards

- **FI01 — Piutang:** total open receivables, aging, and concentration.
- **FI02 — Collection:** realized recovery, overdue exposure, payment mix,
  and collection priorities.
- **FI04 — Piutang Report:** customer and invoice-level receivable evidence.
- **SA01 — Sales:** current-month billing benchmark.

