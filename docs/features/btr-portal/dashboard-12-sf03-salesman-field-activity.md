# SF03 — Salesman Field Activity

## 1. Purpose

**SF03 — Salesman Field Activity** is the individual salesman-day field
execution dashboard. It answers:

> Did this salesman execute the planned route, visit the right customers, and
> turn visits into productive sales activity?

The dashboard is an operational follow-up view for sales supervisors, area
managers, and sales management. It focuses on route coverage, customer
check-ins, GPS credibility, productive visits, and the sequence of activity
during one selected day.

SF03 is different from:

- **SF01 — Salesmen**, which focuses on monthly invoiced sales, target
  achievement, receivable exposure, and customer portfolios.
- **SF02 — Sales Force Overview**, which compares the whole sales organization
  and provides team rankings, trends, and territory charts.

All monetary values shown for field orders are in IDR. Field order value is not
the same as invoiced Faktur omzet.

## 2. Management Reading Sequence

Management should normally read SF03 in this order:

1. Select the salesman and date.
2. Confirm whether visit-plan data is available for the selected date.
3. Scan the seven field-activity KPIs.
4. Review the Missed Visits list and prioritize important customers.
5. Use the map to compare the planned route with the actual route.
6. Review the Visit Timeline for sequence, productivity, and GPS status.
7. Use Route Replay when the order or movement of visits needs investigation.
8. Cross-check monthly sales outcomes in SF01 when field execution and
   invoiced results do not agree.

The dashboard should be read as a combination of coverage, productivity, and
credibility:

- High Execution % with low Effective Call Rate means the route was followed,
  but visits produced few orders.
- High Effective Call Rate with low Execution % means the salesman sells
  where they go, but planned customer coverage is incomplete.
- High visit counts with weak GPS validity reduce confidence in the activity.
- High Unplanned Visits together with high Missed Visits may indicate that the
  salesman is replacing the planned route rather than extending it.

## 3. Header and Daily Selection

### 3.1 Salesman Selector

The selector identifies the salesman whose route and visits will be reviewed.
Salesmen without an available email identity remain visible so management can
identify a data-readiness or field-enablement gap.

### 3.2 Date Selection

SF03 supports:

- **Today**
- **Yesterday**
- **Custom Date**

The selected date defines the salesman-day being evaluated. SF03 is a daily
field-activity view, not a current-month sales-performance view.

### 3.3 Load Action

The **Load** action retrieves the selected salesman-day activity. Before a
salesman and date are selected, the dashboard shows an empty state rather than
presenting misleading zero activity.

### 3.4 Plan-Data Context

The dashboard indicates whether visit-plan data exists for the selected date.
When the selected date is before the visit-plan go-live date, planned activity
may be zero and Visit Execution % must not be interpreted as a performance
judgment.

## 4. KPI Section

The KPI strip contains seven core field-activity measures. The canonical
identifiers are from the BTR Portal KPI Catalog and KPI Encyclopedia.

### 4.1 Planned Visits

1. **KPI Canonical:** `SF-KPI-012` — Planned Visits  
   KPI Encyclopedia: `2.2.1.1` — Planned Visits

2. **Question Answered:**  
How many customers was this salesman expected to visit on the selected day?

3. **Definition:**  
The number of customers included in the effective visit plan for the selected
salesman and date. It is the expected-coverage denominator for Visit Execution
%.

4. **Business Meaning:**  
This is the daily coverage commitment. It establishes what management expects
the salesman to accomplish and provides the basis for a fair route-execution
assessment.

5. **How To Interpret:**

- A high value represents a broad coverage commitment and a demanding route.
- A low value may represent a deliberately light route or an incomplete plan.
- A zero value may indicate that no plan exists for the selected date.
- A high Execution % on a very small plan is not automatically a strong
  coverage result.

6. **Management Action:**

- Confirm that the plan reflects the salesman’s actual customer portfolio.
- If the count is zero or unexpectedly small, fix planning before judging
  route performance.
- Read Planned Visits together with Actual Visits and Missed Visits.

### 4.2 Actual Visits

1. **KPI Canonical:** `SF-KPI-013` — Actual Visits  
   KPI Encyclopedia: `2.2.1.2` — Actual Visits

2. **Question Answered:**  
How many distinct customers did the salesman actually check in to?

3. **Definition:**  
The number of distinct customers with a recorded check-in on the selected
date. Multiple check-ins at the same customer count as one Actual Visit.

4. **Business Meaning:**  
Actual Visits show whether the salesman was physically active in the field and
how much customer coverage was achieved. They measure presence and activity,
not necessarily productive selling.

5. **How To Interpret:**

- A high value indicates broad recorded customer coverage.
- A low value indicates limited coverage and requires comparison with the
  planned route.
- Actual Visits far below Planned Visits indicate a coverage gap.
- High Actual Visits with low Effective Calls indicates activity without
  enough order-producing visits.

6. **Management Action:**

- Compare the number with Planned Visits and Missed Visits.
- Review GPS validity before treating unusually high activity as reliable.
- Review Effective Calls and Effective Call Rate to assess productivity.

### 4.3 Effective Calls

1. **KPI Canonical:** `SF-KPI-016` — Effective Calls  
   KPI Encyclopedia: `2.2.2.1` — Effective Calls

2. **Question Answered:**  
How many customer visits produced at least one sales order on the same day?

3. **Definition:**  
The number of visits that resulted in one or more field sales orders for the
same customer, salesman, and date.

4. **Business Meaning:**  
Effective Calls distinguish customer contact from productive customer contact.
They show whether the salesman was selling rather than only completing visits.

5. **How To Interpret:**

- A high value means many customer contacts generated demand.
- A low value despite high Actual Visits indicates a conversion, assortment,
  pricing, customer-quality, or selling-skill issue.
- Effective Calls are productive visits, not a count of Fakturs.
- A productive visit may still fail to become invoiced omzet later.

6. **Management Action:**

- Compare Effective Calls with Actual Visits and Effective Call Rate.
- Review Orders and Order Value when productivity is low or concentrated.
- Coach call quality and order-taking rather than simply increasing visit
  volume.

### 4.4 Missed Visits

1. **KPI Canonical:** `SF-KPI-014` — Missed Visits  
   KPI Encyclopedia: `2.2.1.3` — Missed Visits

2. **Question Answered:**  
How many planned customers did the salesman fail to visit?

3. **Definition:**  
Planned customers with no check-in on the selected date. Operationally, this
is the planned customer set that was not covered by an Actual Visit.

4. **Business Meaning:**  
Missed Visits identify coverage gaps where the company promised customer
contact but did not execute it. Repeated missed visits can lead to lost orders,
weaker relationships, declining activity, and eventually dormant customers.

5. **How To Interpret:**

- A high value means the published route was not completed.
- A low value means planned coverage was mostly delivered.
- Repeatedly missed customers deserve more attention than a single isolated
  miss.
- A missed strategic, declining, or overdue customer is more urgent than an
  ordinary missed visit.

6. **Management Action:**

- Review the customer names in the Missed Visits list.
- Require a reason and a catch-up date for important missed customers.
- Check whether the route was realistic or systematically too large.
- Coordinate with customer and collection management when missed customers
  carry commercial or receivable risk.

### 4.5 Unplanned Visits

1. **KPI Canonical:** `SF-KPI-015` — Unplanned Visits  
   KPI Encyclopedia: `2.2.1.4` — Unplanned Visits

2. **Question Answered:**  
How much field activity occurred outside the published visit plan?

3. **Definition:**  
Check-ins at customers who were not included in the effective visit plan for
the selected salesman-day.

4. **Business Meaning:**  
Unplanned activity can represent valuable ad-hoc selling, a customer request,
or a legitimate territory opportunity. It can also show that route planning
does not match actual work.

5. **How To Interpret:**

- A high value means the salesman spent significant time outside the planned
  route.
- A low value means activity mostly followed the published plan.
- High Unplanned Visits with low Missed Visits may indicate productive
  flexibility.
- High Unplanned Visits with high Missed Visits indicates route substitution:
  planned customers were skipped while other customers were visited.

6. **Management Action:**

- Check whether unplanned visits produced Effective Calls.
- Identify which planned customers were sacrificed.
- If deviations are consistently productive, improve the route plan.
- If deviations are not productive, reinforce the published route.

Unplanned does not mean invalid. GPS validity checks whether the visit appears
physically credible; Unplanned Visits check whether it followed the plan.

### 4.6 Visit Execution %

1. **KPI Canonical:** `SF-KPI-017` — Visit Execution %  
   KPI Encyclopedia: `2.2.1.5` — Visit Execution %

2. **Question Answered:**  
Did the salesman complete the customer coverage that was planned?

3. **Definition:**  
Actual Visits divided by Planned Visits, expressed as a percentage.

```text
Visit Execution % = Actual Visits ÷ Planned Visits × 100
```

When Planned Visits is zero, the result is not applicable and should be shown
as **N/A**, not as evidence of perfect or failed execution.

4. **Business Meaning:**  
This is the primary route-compliance KPI. It measures whether the salesman
kept the daily coverage commitment. Sales can remain acceptable for a period
even when route discipline is weak; the customer impact may appear later as
declining or dormant accounts.

5. **How To Interpret:**

- A high value means most planned customer coverage was completed.
- A low value means the salesman left planned customers unvisited.
- A high value on a trivial plan should be treated cautiously.
- Repeated low execution indicates a planning, staffing, supervision, route,
  or field-operations problem.

6. **Management Action:**

- Review the Missed Visits list rather than relying only on the percentage.
- Prioritize missed customers by value, overdue exposure, and business
  importance.
- Determine whether the plan is achievable before applying coaching or
  corrective action.
- Read the KPI together with Effective Call Rate; compliance without
  productive visits is incomplete.

### 4.7 Effective Call Rate

1. **KPI Canonical:** `SF-KPI-018` — Effective Call Rate  
   KPI Encyclopedia: `2.2.2.2` — Effective Call Rate

2. **Question Answered:**  
What proportion of the salesman’s actual visits produced an order?

3. **Definition:**  
Effective Calls divided by Actual Visits, expressed as a percentage.

```text
Effective Call Rate = Effective Calls ÷ Actual Visits × 100
```

When Actual Visits is zero, the result is not applicable and should be shown
as **N/A**.

4. **Business Meaning:**  
This is the visit-productivity KPI. It indicates whether customer contacts
converted into demand and prevents management from treating visit volume alone
as sales effectiveness.

5. **How To Interpret:**

- A high rate means visits are generally converting into orders.
- A low rate means visits are not producing enough orders.
- High Execution % with low Effective Call Rate means the salesman followed
  the route but was not productive enough.
- High Effective Call Rate with low Execution % means the salesman sells well
  where they go but is not covering the planned customer base.

6. **Management Action:**

- Review customer mix, product availability, pricing, and selling capability.
- Compare the rate with Effective Calls, Orders, and Order Value.
- Coach conversion and order-taking instead of responding only with more
  planned visits.

## 5. Data-Quality and Route Context

### 5.1 Coordinate Coverage

**KPI Canonical: Not Found**

Coordinate Coverage is a data-health indicator rather than one of the seven
canonical field-activity KPIs. It shows whether planned customers have usable
location coordinates for map presentation.

Management should read it as follows:

- High coverage means the route can be represented more completely on the map.
- Low coverage means some planned stops cannot be mapped, so the visual route
  is incomplete.
- Low coverage may indicate incomplete customer master data, not poor salesman
  execution.
- A missing coordinate should not automatically be treated as a missed visit.

### 5.2 GPS Validation

**KPI Canonical: Not Found for SF03 display status**

GPS Validation is a visit-authenticity and data-quality signal. It compares the
check-in location with the customer location:

- **Valid:** 50 metres or closer
- **Warning:** More than 50 metres and up to 100 metres
- **Suspicious:** More than 100 metres
- **Invalid:** Coordinates are unavailable or unusable

GPS Validation does not measure sales ability. A low-validity result may come
from unreliable check-ins or incorrect customer coordinates. Management should
investigate both possibilities.

## 6. Missed Visits Section

The Missed Visits section converts the Missed Visits KPI into a named action
list. It contains:

- Planned sequence number
- Customer name
- Customer code
- **No Coordinates** indicator when the customer cannot be placed on the map

This section answers:

> Which planned customers were not covered, and which ones require follow-up?

Management should prioritize missed customers that are:

- strategically important;
- declining or at risk of becoming dormant;
- carrying overdue receivables;
- high-value or high-potential customers; or
- repeatedly missed across several days.

The list is more actionable than the total KPI because it identifies the
customers requiring a reason, rescheduled visit, or route-plan correction.

## 7. Route Activity Map

The map is the primary visual section of SF03. It compares intended route
coverage with recorded field activity.

### 7.1 Map Content

- **Planned route:** dashed line showing the planned visit sequence
- **Actual route:** solid line showing the recorded check-in sequence
- **Planned pins:** customers on the plan who were visited
- **Missed pins:** planned customers with no check-in
- **Actual pins:** recorded planned visits, colored by GPS status
- **Unplanned pins:** check-ins at customers outside the plan
- **Replay marker:** the currently selected or replayed visit

### 7.2 Layer Controls

Management can show or hide:

- Planned route
- Actual route
- Planned pins
- Actual pins
- Missed pins
- Unplanned pins

These controls help isolate route gaps, deviations, and suspicious activity.

### 7.3 How To Interpret the Map

- Planned and actual routes that closely overlap indicate route adherence.
- A short actual route compared with a long planned route indicates incomplete
  coverage.
- Actual pins outside the planned route indicate off-plan activity.
- Missed pins identify planned customers left uncovered.
- Suspicious GPS colors require validation before accepting the activity as
  genuine.
- A visually incomplete map may reflect missing customer coordinates rather than
  missing visits.

The map is an investigation aid. It does not prove the quality of the sales
conversation or replace sales and customer evidence.

## 8. Visit Timeline

The Visit Timeline presents actual check-ins in time sequence. Each visit
contains:

- Sequence number
- Check-in time
- Customer name
- Effective indicator when an order was generated
- Unplanned indicator when the customer was outside the plan
- GPS validation status
- Distance from the customer location when available

The timeline answers:

> In what order did the salesman visit customers, and which visits were
> productive and credible?

Management should use it to:

- identify long gaps or unusual sequence changes;
- compare actual order with the planned route;
- distinguish productive from non-productive stops;
- investigate suspicious GPS distances;
- select a visit for map review.

Selecting a timeline item highlights the corresponding activity on the map.

## 9. Route Replay

Route Replay allows management to review the sequence of actual visits over
time.

Controls:

- **Play**
- **Pause**
- **Reset**
- Adjustable replay speed

Replay is useful when management needs to understand the order of visits,
whether the actual route diverged from the plan, and where missed or unplanned
activity occurred.

Replay shows visit-to-visit segments. It is not a reconstruction of the actual
driven road path and should not be used to estimate travel time or distance
between all points.

## 10. Cross-Link to Sales Performance

The Salesmen dashboard link provides an outcome-oriented follow-up path.

Use SF03 to understand:

- whether the salesman visited customers;
- whether the planned route was executed;
- whether visits were productive; and
- whether activity appears credible.

Use SF01 to understand:

- invoiced sales;
- target achievement;
- receivable exposure; and
- customer portfolio health.

A salesman can have strong field execution but weak invoiced sales, or strong
invoiced sales but weak route compliance. These are different management
questions and should not be treated as contradictory measures.

## 11. Combined Interpretation

### High Execution % and High Effective Call Rate

The salesman is covering the planned route and converting visits into orders.
Confirm that GPS validity is acceptable and that order value is commercially
meaningful.

### High Execution % and Low Effective Call Rate

The salesman is completing the route but visits are not converting. Investigate
customer quality, stock availability, assortment, pricing, selling capability,
and order-taking.

### Low Execution % and High Effective Call Rate

The salesman sells effectively where they go but is not covering the planned
customer base. Review missed customers and determine whether the plan or route
discipline is the main issue.

### High Unplanned Visits and High Missed Visits

The salesman is likely substituting off-plan activity for planned coverage.
Review whether the deviations are productive and whether the route plan should
be changed.

### High Actual Visits and Low GPS Validity

Recorded activity is not sufficiently credible to support a strong execution
conclusion. Review suspicious visits and verify customer coordinates.

### High Effective Calls and Low Order Value

Many visits produced orders, but the orders may be small. Review assortment,
customer potential, and order size rather than relying on the Effective Calls
count alone.

## 12. Business Rules and Limitations

- SF03 evaluates one salesman and one selected date at a time.
- Planned Visits is meaningful only when visit-plan data exists.
- Visit Execution % is not applicable when Planned Visits is zero.
- Effective Call Rate is not applicable when Actual Visits is zero.
- Actual Visits count distinct customers; repeated same-day check-ins do not
  create multiple visits.
- Effective Calls require a same-day field sales order.
- Unplanned Visits are not automatically negative.
- GPS Validation measures visit authenticity and data quality, not selling
  skill.
- Missing coordinates affect map completeness but do not by themselves prove
  that a visit was missed.
- Field orders and Order Value are not invoiced Faktur and must not be
  reconciled directly to monthly invoiced omzet.
- SF03 is a read-only management view. Operational route changes, customer
  master-data corrections, and transaction actions continue in the appropriate
  operational systems.
- Field-activity measures are not currently Alert Center V1 signals; managers
  should use the dashboard and its drill-down evidence for review.

## 13. Recommended Management Actions

### Low Visit Execution % or High Missed Visits

- Review the named missed customers.
- Confirm whether the route was realistic.
- Prioritize important, overdue, and declining customers.
- Coach route discipline or adjust route capacity.

### High Visit Execution % but Low Effective Call Rate

- Review customer and product mix.
- Check stock availability and pricing.
- Coach selling and order-taking quality.
- Avoid solving a conversion problem by adding random visits.

### High Unplanned Visits

- Compare unplanned customers with missed planned customers.
- Retain productive deviations where they reflect real demand.
- Update the route plan when the pattern is consistently justified.

### Low GPS Validity

- Review suspicious visits in the map and timeline.
- Validate customer coordinates.
- Investigate repeated distance anomalies before relying on execution
  rankings.

### Strong Activity but Weak SF01 Sales Outcome

- Compare Effective Calls and Order Value with later invoiced omzet.
- Investigate order fulfilment, billing conversion, product availability, and
  customer mix.
- Do not assume that more visits alone will solve an invoiced-sales gap.

## 14. Canonical KPI Coverage

SF03 uses the following canonical KPIs from the BTR Portal KPI Catalog:

- `SF-KPI-012` — Planned Visits
- `SF-KPI-013` — Actual Visits
- `SF-KPI-014` — Missed Visits
- `SF-KPI-015` — Unplanned Visits
- `SF-KPI-016` — Effective Calls
- `SF-KPI-017` — Visit Execution %
- `SF-KPI-018` — Effective Call Rate

The following are supporting analytical or data-quality elements and do not
have separate canonical KPI entries for SF03:

- Coordinate Coverage
- GPS Validation status
- Missed Visits list
- Route Activity Map
- Visit Timeline
- Route Replay

