# SF02 — Sales Force Overview

## 1. Purpose

**SF02 — Sales Force Overview** is the management overview for daily field-sales
execution. It answers:

> Is the sales organization covering the planned customers, producing productive
> visits, and generating field orders?

The dashboard is an **execution lens**. It should be read together with SF01
Salesmen, which focuses on invoiced sales, target achievement, receivable
exposure, and customer-portfolio outcomes.

SF02 uses a selected salesman-day or team-day view. It is intended for sales
managers, field supervisors, area managers, and executives who need to identify
coverage gaps, weak visit productivity, suspicious activity, or uneven
performance across the sales force.

All monetary values are in IDR.

## 2. Management Reading Sequence

Management should normally read the dashboard in this order:

1. Select the relevant date and confirm the plan-data message.
2. Scan the KPI groups: Field Execution, Productivity, and Quality.
3. Review the Salesman Performance table for individual exceptions.
4. Compare the four salesman charts.
5. Use the rankings to identify the strongest and weakest performers.
6. Review Team Execution Trends and Visits by Wilayah for pattern and coverage.
7. Open salesman detail when a KPI, row, chart, or ranking requires evidence.

High visit volume is not automatically good. Management should read coverage,
productivity, and quality together:

- High Execution % with low Effective Call Rate means the route was followed
  but visits produced few orders.
- High Effective Call Rate with low Execution % means selling occurs where the
  salesman goes, but planned coverage is being missed.
- High activity with low GPS Valid Rate reduces confidence in the activity
  result.
- High Unplanned Visits together with high Missed Visits may indicate that the
  published plan does not match actual work or that the salesman is bypassing
  planned customers.

## 3. Header, Date Controls, and Data Context

### 3.1 Date Selection

The dashboard supports:

- **Today**
- **Yesterday**
- **Custom date**

The selected date defines the salesman-day and team-day being evaluated. It is
not a current-month sales-performance period.

### 3.2 Refresh and Data Freshness

The Refresh action requests the latest available overview data. The page also
shows whether the displayed result comes from a refreshed snapshot or a live
query, together with its time.

Management should check freshness before making an operational decision. A
stale result may not reflect the latest check-ins or field orders.

### 3.3 Visit-Plan Availability

The information message indicates whether visit-plan data is available for the
selected date. Before the visit-plan go-live date, planned KPIs may be zero and
Execution % should not be treated as evidence of poor performance.

## 4. KPI Sections

### 4.1 Field Execution

This section compares expected customer coverage with actual field activity.

#### 4.1.1 Active Salesmen

1. **KPI Canonical:** Not Found

2. **Question Answered:**
How many salesmen have field activity in the selected team-day?

3. **Definition:**
The number of salesmen represented as active in the selected field-activity
overview.

4. **Business Meaning:**
This provides the denominator and context for team activity. A low active count
may explain low team visits or orders, but it does not by itself indicate poor
performance.

5. **How To Interpret:**

- Compare the count with the expected staffing and daily route plan.
- A low count may indicate absence, synchronization delay, or no recorded
  activity.
- Do not compare total visits across days without considering how many salesmen
  were active.

#### 4.1.2 Planned Visits

1. **KPI Canonical:** `SF-KPI-012` — Planned Visits

2. **Question Answered:**
How many customer visits was the team expected to make?

3. **Definition:**
Customers included in an effective visit plan for the selected salesman-day or
team-day. This is the expected-coverage denominator for route execution.

4. **Business Meaning:**
The plan establishes what the organization committed to cover. Without a
credible plan, management cannot fairly judge route execution.

5. **How To Interpret:**

- A high planned count represents a broad coverage commitment.
- A low planned count may represent a light route or an incomplete plan.
- Planned Visits should be read with Actual Visits and Missed Visits.
- If the value is zero because plan data was unavailable, Execution % is not a
  meaningful performance judgment.

#### 4.1.3 Actual Visits

1. **KPI Canonical:** `SF-KPI-013` — Actual Visits

2. **Question Answered:**
How many distinct planned or field customers did the team actually check in to?

3. **Definition:**
Distinct customers with a recorded field check-in for the selected date.
Multiple check-ins for the same customer are treated as one actual visit.

4. **Business Meaning:**
Actual Visits show physical coverage achieved by the field organization. They
are the numerator for Visit Execution % but are not, on their own, proof of
productive selling.

5. **How To Interpret:**

- Compare Actual Visits with Planned Visits to identify coverage performance.
- High Actual Visits with low Effective Calls indicates activity without enough
  order-producing visits.
- Review GPS Valid Rate before treating unusually high activity as reliable.

#### 4.1.4 Execution %

1. **KPI Canonical:** `SF-KPI-017` — Visit Execution %

2. **Question Answered:**
Did the team complete the customer coverage it planned?

3. **Definition:**
Actual Visits divided by Planned Visits, expressed as a percentage. At team
level, the measure is calculated from total actual visits divided by total
planned visits. It is not applicable when Planned Visits is zero.

4. **Business Meaning:**
This is the primary route-compliance measure. It shows whether the sales force
kept its coverage promise and whether planned customer contact is likely to be
consistent.

5. **How To Interpret:**

- A high value means the team completed most of its planned coverage.
- A low value means planned customers were left unvisited and may be exposed
  to lost orders or weakened relationships.
- A high value on a very small plan is not necessarily strong coverage.
- Investigate repeated low execution, especially when missed customers are
  important, overdue, or declining.

### 4.2 Productivity

This section distinguishes field presence from visits that create commercial
activity.

#### 4.2.1 Effective Calls

1. **KPI Canonical:** `SF-KPI-016` — Effective Calls

2. **Question Answered:**
How many customer visits produced at least one sales order on the same day?

3. **Definition:**
Visits that resulted in one or more field sales orders for the same customer,
salesman, and date.

4. **Business Meaning:**
Effective Calls measure productive customer contact. They prevent management
from treating visit volume alone as sales effectiveness.

5. **How To Interpret:**

- A high count indicates that more customer contacts generated demand.
- A low count despite high Actual Visits indicates a conversion or selling-
  quality problem.
- Compare with Effective Call Rate, Orders, and Order Value to distinguish
  breadth from value.

#### 4.2.2 Effective Call Rate

1. **KPI Canonical:** `SF-KPI-018` — Effective Call Rate

2. **Question Answered:**
What proportion of actual visits produced an order?

3. **Definition:**
Effective Calls divided by Actual Visits, expressed as a percentage. It is not
meaningful when there are no actual visits.

4. **Business Meaning:**
This is the visit-productivity measure. It indicates whether the team's
customer contacts are converting into demand.

5. **How To Interpret:**

- A high rate means visits are generally productive.
- A low rate means the team is visiting customers without generating enough
  orders.
- High Effective Call Rate with low Execution % indicates productive selling
  with incomplete route coverage.
- Low Effective Call Rate with high Execution % suggests a coaching,
  assortment, pricing, or customer-quality issue rather than a simple route
  discipline issue.

#### 4.2.3 Orders

1. **KPI Canonical:** Not Found

2. **Question Answered:**
How many field sales orders did the team generate on the selected date?

3. **Definition:**
The total number of field sales orders associated with the selected team-day.
This is an order-production measure, not a count of Fakturs.

4. **Business Meaning:**
Orders show whether field activity created demand. They provide an earlier
commercial signal than invoiced sales, while remaining separate from recognized
Faktur omzet.

5. **How To Interpret:**

- High visits with low Orders indicates weak order conversion.
- Orders concentrated among a few salesmen indicate dependency on a small
  productive core.
- Do not add Orders to current-month Faktur counts or treat them as invoices.

#### 4.2.4 Order Value

1. **KPI Canonical:** Not Found

2. **Question Answered:**
What is the value of field orders generated on the selected date?

3. **Definition:**
The total value of field sales orders for the selected team-day, shown in IDR.
It represents order value and is not invoiced Faktur omzet.

4. **Business Meaning:**
Order Value indicates the commercial value created by field activity. It helps
management distinguish many small orders from fewer high-value opportunities.

5. **How To Interpret:**

- High Orders with low Order Value may indicate many small transactions.
- Low Orders with high Order Value may indicate concentrated but valuable
  selling.
- Use the Sales Report and Faktur-based sales dashboards to validate whether
  orders later become invoiced sales.
- Do not combine Order Value with current-month invoiced omzet.

### 4.3 Quality

This section tests whether activity is credible and whether the planned route
was followed.

#### 4.3.1 GPS Valid Rate

1. **KPI Canonical:** Not Found

2. **Question Answered:**
What share of recorded check-ins is physically consistent with the customer's
location?

3. **Definition:**
The percentage of check-ins whose coordinates are within the acceptable
distance of the customer's registered location. The documented validation bands
are Valid at 50 metres or closer, Warning from more than 50 to 100 metres, and
Suspicious beyond 100 metres.

4. **Business Meaning:**
GPS Valid Rate is a visit-authenticity and data-quality signal. It does not
measure selling skill, but weak GPS quality makes route and activity KPIs less
trustworthy.

5. **How To Interpret:**

- A high rate increases confidence that reported visits occurred at customer
  locations.
- A low rate requires review of check-in behavior and customer coordinates.
- Perfect Execution % with weak GPS should not be accepted without
  investigation.
- Invalid results may come from poor master data as well as from unreliable
  check-ins.

#### 4.3.2 Missed Visits

1. **KPI Canonical:** `SF-KPI-014` — Missed Visits

2. **Question Answered:**
How many planned customers were not visited?

3. **Definition:**
Planned Visits minus Actual Visits for the selected date.

4. **Business Meaning:**
Missed Visits identify coverage gaps. Repeatedly missed customers may stop
ordering, become dormant, or remain unsupported during collection activity.

5. **How To Interpret:**

- A high value means the published route was not completed.
- A low value means planned coverage was mostly delivered.
- Prioritize missed customers that are high-value, overdue, declining, or
  strategically important.
- Review repeated misses by salesman and customer rather than relying only on
  the team total.

#### 4.3.3 Unplanned Visits

1. **KPI Canonical:** `SF-KPI-015` — Unplanned Visits

2. **Question Answered:**
How much field activity occurred outside the published visit plan?

3. **Definition:**
Actual check-ins at customers who were not included in the effective visit plan
for that salesman-day.

4. **Business Meaning:**
Unplanned activity can represent valuable ad-hoc selling, but it can also show
that planning does not match the way the team works.

5. **How To Interpret:**

- High Unplanned Visits with low Missed Visits may indicate productive
  opportunities beyond the planned route.
- High Unplanned Visits with high Missed Visits indicates route substitution:
  planned customers are being skipped while other customers are visited.
- Review whether unplanned visits produced orders.
- If the pattern is consistently productive, management may need to improve the
  route plan.

## 5. Salesman Performance Table

### 5.1 Purpose

The Salesman Performance table is the main evidence table for comparing
individual field execution. It supports search, sorting, pagination, and
drill-down to salesman field activity.

### 5.2 Columns and Management Meaning

- **Rank:** Relative position in the overview population.
- **Salesman Code / Name:** Identifies the responsible salesperson.
- **Planned:** Expected customer coverage.
- **Actual:** Customers with recorded check-ins.
- **Execution %:** Completion of planned coverage.
- **Effective:** Visits that produced an order.
- **Eff. Rate:** Share of actual visits that produced an order.
- **Missed:** Planned customers not visited.
- **Unplanned:** Customers visited outside the plan.
- **GPS Valid %:** Credibility of recorded check-ins.
- **Orders:** Field sales-order count.
- **Order Value:** Value of field sales orders in IDR.
- **Status:** Summary status for the salesman based on the displayed activity
  indicators.

### 5.3 How Management Should Use It

Sort and compare the table to find:

- low Execution % and high Missed Visits;
- high Execution % but low Eff. Rate;
- high Unplanned Visits with weak execution;
- high Orders or Order Value concentrated in a few salesmen;
- weak GPS Valid % that reduces confidence in activity;
- salesmen requiring a detail review or coaching conversation.

The table is not a replacement for SF01 sales outcome analysis. A salesman
may have strong field execution but weak invoiced sales, or strong invoiced
sales but weak route compliance.

## 6. Salesman Comparison Charts

The comparison charts are horizontal bar charts. Each bar represents a salesman
and can be used to open detailed field activity.

### 6.1 Visit Execution %

Ranks salesmen by route-completion percentage. Use it to identify strong route
discipline and salesmen who repeatedly leave planned customers unvisited.

### 6.2 Effective Call Rate

Ranks salesmen by the proportion of visits producing orders. Use it to identify
coaching opportunities in customer conversion and order-taking.

### 6.3 Orders Generated

Compares the number of field orders generated by each salesman. Use it to
identify demand-generation volume, while remembering that orders are not
Faktur.

### 6.4 Order Value

Compares the IDR value of field orders by salesman. Use it to distinguish
transaction volume from commercial value and to identify concentration of order
value.

### 6.5 Combined Interpretation

The four charts should be read together:

- high Execution % and high Effective Call Rate: disciplined and productive;
- high Execution % and low Effective Call Rate: coverage completed but
  conversion is weak;
- low Execution % and high Effective Call Rate: productive where visited but
  route coverage is incomplete;
- high Orders but low Order Value: frequent small orders;
- low Orders but high Order Value: fewer, larger opportunities.

## 7. Performance Rankings

The Performance Rankings section provides selectable Top 5 or Top 10 tables.
Each ranking contains Rank, Salesman Code, Salesman Name, and the ranked value.

### 7.1 Top Visit Execution

Shows the salesmen with the highest route compliance. Use as a source of
planning and execution practices, while checking that the plan size is
meaningful.

### 7.2 Bottom Visit Execution

Shows the salesmen with the lowest route compliance. This is the primary
coaching queue for coverage discipline.

### 7.3 Top Effective Call Rate

Shows the salesmen whose visits most often produce orders. Use to identify
effective customer-contact practices.

### 7.4 Bottom Effective Call Rate

Shows salesmen whose customer visits convert least often. Investigate customer
selection, product availability, pricing, selling capability, and order-taking
quality before simply increasing visit volume.

### 7.5 Top Order Value

Shows the salesmen generating the highest field-order value. Compare with
Orders, Execution %, and GPS Valid % so large values are interpreted with
activity quality and authenticity.

### 7.6 Top Orders

Shows the salesmen generating the most field orders. High order count does not
necessarily mean high value or invoiced revenue.

### 7.7 Most Missed Visits

Shows the salesmen with the largest coverage gaps. Use it to prioritize
same-day review of skipped customers.

### 7.8 Most Unplanned Visits

Shows the salesmen deviating most from the published plan. Review this ranking
with Missed Visits and Effective Call Rate to distinguish productive flexibility
from weak route discipline.

## 8. Team Execution Trends

Team Execution Trends is a line chart with selectable **7-day** and **30-day**
horizons. It shows:

- Visit Execution %;
- Effective Call Rate;
- Orders; and
- Order Value.

### 8.1 Management Questions

- Is route compliance improving or deteriorating?
- Is visit productivity moving with execution, or diverging from it?
- Is order production increasing with field activity?
- Is order value concentrated in isolated spikes or sustained over time?

### 8.2 Interpretation

- Falling Execution % suggests a planning, staffing, supervision, or field-
  operations problem.
- Falling Effective Call Rate while Execution % is stable suggests a
  conversion or selling-quality problem.
- Rising Orders without rising Order Value may indicate smaller transactions.
- A single Order Value spike should not be treated as a sustained improvement.

Trend values are for operational direction and should be interpreted with
changes in active salesman count, plan size, holidays, and data freshness.

## 9. Visits by Wilayah

Visits by Wilayah is a horizontal bar chart showing actual visit count by
commercial territory.

### 9.1 Question Answered

Where is field activity concentrated, and which territories may be receiving
insufficient coverage?

### 9.2 Business Meaning

The chart provides a territory-level view of actual coverage. It helps area
managers compare activity distribution and identify territories that are
over-served, under-served, or not represented in the selected day's activity.

### 9.3 How To Interpret

- High visits in a Wilayah indicate strong recorded activity, not necessarily
  strong sales results.
- Low visits may indicate weak staffing, route planning, customer density, or
  missing activity data.
- Compare territory activity with Planned Visits, Missed Visits, Orders, and
  Order Value before changing coverage.

## 10. Business Rules and Limitations

- Planned Visits is meaningful only when visit-plan data exists.
- Visit Execution % is not applicable when Planned Visits is zero.
- Effective Call Rate is not meaningful when Actual Visits is zero.
- Effective Calls require a same-day field order; a visit without an order is
  not an Effective Call.
- Actual Visits count distinct customers, not necessarily the number of
  check-in records.
- Unplanned Visits are not automatically bad; they must be read with Missed
  Visits and productivity.
- GPS Valid Rate measures visit authenticity and data quality, not sales skill.
- Orders and Order Value are field-order measures, not Faktur-based invoiced
  omzet.
- SF02 does not replace SF01 Salesmen, Sales Dashboard, Sales Report, or
  Salesman Field Activity detail.
- The field-activity KPIs are not currently Alert Center V1 signals; management
  must use the dashboard and drill-down workflow for review.

## 11. Recommended Management Actions

### Low Execution % or High Missed Visits

- Review the missed-customer list.
- Confirm whether the plan was realistic.
- Prioritize important, overdue, or declining customers.
- Coach route discipline or adjust route capacity.

### High Execution % but Low Effective Call Rate

- Review customer and product mix.
- Check stock availability, pricing, and selling capability.
- Coach order-taking and visit quality rather than adding random visits.

### High Unplanned Visits

- Compare unplanned activity with missed planned customers.
- Retain productive deviations where they reflect real territory demand.
- Update route planning when the pattern is consistently justified.

### Low GPS Valid Rate

- Review suspicious check-ins.
- Validate customer coordinates.
- Investigate repeated distance anomalies before relying on activity rankings.

### High Orders or Order Value Concentration

- Compare the top salesmen with Execution %, Effective Call Rate, and GPS
  Valid %.
- Confirm conversion into Faktur through sales evidence.
- Avoid making the team dependent on a small number of individual producers.

## 12. Canonical KPI Coverage

The KPI Encyclopedia currently provides canonical entries for:

- `SF-KPI-012` — Planned Visits
- `SF-KPI-013` — Actual Visits
- `SF-KPI-014` — Missed Visits
- `SF-KPI-015` — Unplanned Visits
- `SF-KPI-016` — Effective Calls
- `SF-KPI-017` — Visit Execution %
- `SF-KPI-018` — Effective Call Rate

The following SF02 display measures do not have individual canonical entries in
the reviewed encyclopedia and are therefore intentionally marked
**KPI Canonical: Not Found**:

- Active Salesmen
- GPS Valid Rate
- Orders
- Order Value

Rankings, comparison charts, trends, the Salesman Performance table, and Visits
by Wilayah are analytical presentation sections. They use the underlying
measures above and are not assigned invented KPI codes.

## 13. Related Menus

- **SF01 — Salesmen:** salesperson outcome and customer-portfolio performance.
- **SF02 — Sales Force Overview:** team field-execution overview.
- **SF03 — Salesman Field Activity:** individual route, visit, GPS, and replay
  detail.
- **SA03 — Sales Report:** Faktur-level evidence for invoiced sales.

