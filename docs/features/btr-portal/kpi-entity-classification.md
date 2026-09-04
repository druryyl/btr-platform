# KPI Entity Classification

Classification of all KPIs from [`kpi-inventory.md`](kpi-inventory.md) into a single primary entity.

**Target entities:** Customer, Salesman, Item, Supplier

## Summary

| Entity | KPI Count |
| ------ | --------- |
| Customer | 107 |
| Salesman | 53 |
| Item | 38 |
| Supplier | 23 |
| **Total** | **221** |

## Classification

| KPI Code | KPI Name | Entity | Justification |
| -------- | -------- | ------ | ------------- |
| CU-KPI-001 | Overdue Customer Count | Customer | Identifies customers with overdue receivables to prioritize collection action. |
| CU-KPI-002 | >90 Day Exposure | Customer | Identifies customers with overdue receivables to prioritize collection action. |
| CU-KPI-003 | Top Omzet Customer % | Customer | Measures customer concentration risk in receivables or revenue. |
| CU-KPI-004 | Top Piutang Customer % | Customer | Quantifies customer receivables exposure for credit and collection management. |
| CU-KPI-005 | Active Customer Count | Customer | Counts customers with recent purchasing activity reflecting base engagement. |
| CU-KPI-006 | Dormant Customer Count | Customer | Tracks customer lifecycle status to identify engagement or churn risk. |
| CU-KPI-007 | Plafond Breach Count | Customer | Monitors customer credit limit compliance and review needs. |
| CU-KPI-008 | Suspended + Sales Count | Customer | Identifies customers with suspended status who still have active sales exposure. |
| CU-KPI-009 | Top 10 Omzet (Ranking) | Customer | Measures customer revenue contribution for portfolio value assessment. |
| CU-KPI-010 | Top 10 Piutang (Ranking) | Customer | Quantifies customer receivables exposure for credit and collection management. |
| CU-KPI-020 | Customers Forecasted at Risk | Customer | Assesses customer credit or payment risk for proactive portfolio management. |
| CU-KPI-021 | High Risk Customer Count | Customer | Assesses customer credit or payment risk for proactive portfolio management. |
| CU-KPI-022 | Elevated Risk Receivable | Customer | Quantifies customer receivables exposure for credit and collection management. |
| CU-KPI-023 | Elevated Risk Receivable % | Customer | Quantifies customer receivables exposure for credit and collection management. |
| CU-KPI-024 | Portfolio Health Score | Customer | Evaluates overall customer portfolio quality and relationship health. |
| CU-KPI-025 | Forecast Confidence | Customer | Indicates reliability of customer collection or risk planning assumptions. |
| CU-KPI-026 | Total Piutang (Context) | Customer | Quantifies customer receivables exposure for credit and collection management. |
| CU-KPI-027 | Payment Delay Signal Count | Customer | Counts early warning signals of deteriorating customer payment or engagement behavior. |
| CU-KPI-028 | Credit Limit Signal Count | Customer | Counts early warning signals of deteriorating customer payment or engagement behavior. |
| CU-KPI-029 | Inactivity Signal Count | Customer | Tracks customer lifecycle status to identify engagement or churn risk. |
| CU-KPI-030 | Purchase Decline Signal Count | Customer | Counts early warning signals of deteriorating customer payment or engagement behavior. |
| CU-KPI-031 | Collection Risk Signal Count | Customer | Assesses customer credit or payment risk for proactive portfolio management. |
| CU-KPI-032 | Risk Category Count — Healthy | Customer | Assesses customer credit or payment risk for proactive portfolio management. |
| CU-KPI-033 | Risk Category Count — Watch | Customer | Assesses customer credit or payment risk for proactive portfolio management. |
| CU-KPI-034 | Risk Category Count — Attention | Customer | Assesses customer credit or payment risk for proactive portfolio management. |
| CU-KPI-035 | Risk Category Count — High Risk | Customer | Assesses customer credit or payment risk for proactive portfolio management. |
| CU-KPI-036 | Risk Category Count — Critical | Customer | Assesses customer credit or payment risk for proactive portfolio management. |
| CU-KPI-040 | Actions Today | Customer | Tracks collection actions taken on customer accounts to measure recovery effort. |
| CU-KPI-041 | Immediate Collection Count | Customer | Supports collection planning and action on customer receivables. |
| CU-KPI-042 | Proactive Reminder Count | Customer | Tracks collection actions taken on customer accounts to measure recovery effort. |
| CU-KPI-043 | Credit Review Count | Customer | Monitors customer credit limit compliance and review needs. |
| CU-KPI-044 | Sales Recovery Count | Customer | Supports collection planning and action on customer receivables. |
| CU-KPI-045 | Management Escalation Count | Customer | Tracks collection actions taken on customer accounts to measure recovery effort. |
| CU-KPI-046 | Collection Impact Total | Customer | Supports collection planning and action on customer receivables. |
| CU-KPI-047 | Overdue Exposure (Context) | Customer | Identifies customers with overdue receivables to prioritize collection action. |
| CU-KPI-048 | Due Within 7 Days | Customer | Supports collection planning and action on customer receivables. |
| CU-KPI-049 | Recovery vs Billing % (Context) | Customer | Supports collection planning and action on customer receivables. |
| CU-KPI-050 | Planning Confidence | Customer | Indicates reliability of customer collection or risk planning assumptions. |
| CU-KPI-051 | Immediate Impact Total | Customer | Tracks collection actions taken on customer accounts to measure recovery effort. |
| CU-KPI-060 | Portfolio Health Score | Customer | Evaluates overall customer portfolio quality and relationship health. |
| CU-KPI-061 | Portfolio Healthy % | Customer | Evaluates overall customer portfolio quality and relationship health. |
| CU-KPI-062 | Attention Customer Count | Customer | Identifies customers requiring management attention based on portfolio criteria. |
| CU-KPI-063 | Strategic Customer Count | Customer | Tracks strategically important customers for focused relationship management. |
| CU-KPI-064 | Strategic At Risk Count | Customer | Assesses customer credit or payment risk for proactive portfolio management. |
| CU-KPI-065 | Working Capital Tied Amount | Customer | Quantifies working capital tied up in customer receivables. |
| CU-KPI-066 | Total MTD Omzet (Portfolio) | Customer | Evaluates overall customer portfolio quality and relationship health. |
| CU-KPI-067 | Total Open Balance (Portfolio) | Customer | Quantifies customer receivables exposure for credit and collection management. |
| CU-KPI-068 | Never Purchased Count | Customer | Tracks customer lifecycle status to identify engagement or churn risk. |
| CU-KPI-069 | Dormant Count (Lifecycle) | Customer | Tracks customer lifecycle status to identify engagement or churn risk. |
| CU-KPI-070 | Declining Count | Customer | Tracks customer lifecycle status to identify engagement or churn risk. |
| CU-KPI-071 | Customer Report Row Metrics | Customer | Provides per-customer detail metrics for account-level management review. |
| EX-KPI-001 | Achievement % | Salesman | Measures company-wide sales achievement against target, driving salesman and sales management accountability. |
| EX-KPI-002 | Total Achievement (Total Omzet MTD) | Salesman | Tracks total MTD revenue achievement as the primary sales performance outcome metric. |
| EX-KPI-003 | Total Target | Salesman | Represents the sales target baseline against which field and management performance is measured. |
| EX-KPI-004 | Total Piutang | Customer | Quantifies customer receivables exposure for credit and collection management. |
| EX-KPI-005 | Overdue Customer Count | Customer | Identifies customers with overdue receivables to prioritize collection action. |
| EX-KPI-006 | Piutang > 90 Hari (Amount & %) | Customer | Identifies customers with overdue receivables to prioritize collection action. |
| EX-KPI-007 | Top Customer % (Piutang Concentration) | Customer | Quantifies customer receivables exposure for credit and collection management. |
| EX-KPI-008 | Pending Posting (Count & Value) | Supplier | Highlights unposted purchase transactions that block procurement visibility and supplier invoice processing. |
| EX-KPI-009 | Top Principal % (Purchasing) | Supplier | Measures principal or supplier concentration and dependency in purchasing or inventory. |
| EX-KPI-010 | Total Inventory Value | Item | Quantifies total stock value for inventory asset management. |
| EX-KPI-011 | Top Category % (Inventory) | Item | Measures inventory concentration or risk by product category. |
| EX-KPI-012 | Top Supplier % (Inventory) | Supplier | Ranks supplier or principal share to assess purchasing concentration. |
| EX-KPI-013 | Top 5 Customers (Critical Exposure) | Customer | Measures customer concentration risk in receivables or revenue. |
| EX-KPI-014 | Top 5 Categories (Critical Exposure) | Item | Measures product stock levels, movement, or inventory risk for stock management. |
| EX-KPI-015 | Top 5 Suppliers (Critical Exposure) | Supplier | Ranks supplier or principal share to assess purchasing concentration. |
| EX-KPI-016 | Top 5 Principals (Critical Exposure) | Supplier | Measures principal or supplier concentration and dependency in purchasing or inventory. |
| EX-KPI-017 | Snapshot Freshness (Last Refreshed) | Customer | Ensures receivables and customer portfolio figures are current enough for timely collection decisions. |
| EX-KPI-018 | Domain Attention Summary Cards | Customer | Directs executive attention to domains needing action, with customer and collection risk as the primary decision drivers. |
| EX-KPI-019 | Portfolio Healthy % | Customer | Evaluates overall customer portfolio quality and relationship health. |
| EX-KPI-020 | Customers At Risk Count | Customer | Assesses customer credit or payment risk for proactive portfolio management. |
| EX-KPI-021 | Strategic Customers At Risk Count | Customer | Assesses customer credit or payment risk for proactive portfolio management. |
| EX-KPI-022 | Alert Count — Sales | Salesman | Counts sales-domain alerts requiring salesman or sales management intervention. |
| EX-KPI-023 | Alert Count — Customer | Customer | Counts customer-domain alerts requiring portfolio or collection intervention. |
| EX-KPI-024 | Alert Count — Collection | Customer | Counts collection-domain alerts tied to customer receivables and payment behavior. |
| EX-KPI-025 | Alert Count — Inventory | Item | Counts inventory-domain alerts about stock levels, movement, and product risk. |
| EX-KPI-026 | Alert Count — Purchasing | Supplier | Counts purchasing-domain alerts about supplier transactions and procurement backlog. |
| EX-KPI-027 | Alert Count — Location | Item | Counts location-domain alerts primarily reflecting warehouse inventory concentration and stock risk. |
| EX-KPI-028 | Inventory Risk Summary (Alert Center) | Item | Counts inventory-domain alerts about product stock and movement risk. |
| EX-KPI-029 | Platform Alerts (Pinned) | Customer | Surfaces pinned platform alerts that most often flag critical customer or collection issues. |
| FI-KPI-001 | Total Piutang | Customer | Quantifies customer receivables exposure for credit and collection management. |
| FI-KPI-002 | Total Customer (with balance) | Customer | Counts customers with outstanding balances for receivables portfolio sizing. |
| FI-KPI-003 | Overdue Customer | Customer | Identifies customers with overdue receivables to prioritize collection action. |
| FI-KPI-004 | Top 10 Customer % | Customer | Measures customer concentration risk in receivables or revenue. |
| FI-KPI-005 | Top 20 Customer % | Customer | Measures customer concentration risk in receivables or revenue. |
| FI-KPI-006 | Aging Bucket — Current | Customer | Breaks down customer receivables by aging bucket to assess payment delay severity. |
| FI-KPI-007 | Aging Bucket — 1–30 Days | Customer | Breaks down customer receivables by aging bucket to assess payment delay severity. |
| FI-KPI-008 | Aging Bucket — 31–60 Days | Customer | Breaks down customer receivables by aging bucket to assess payment delay severity. |
| FI-KPI-009 | Aging Bucket — 61–90 Days | Customer | Breaks down customer receivables by aging bucket to assess payment delay severity. |
| FI-KPI-010 | Aging Bucket — > 90 Days | Customer | Breaks down customer receivables by aging bucket to assess payment delay severity. |
| FI-KPI-011 | Piutang > 90 Hari (Amount & %) | Customer | Identifies customers with overdue receivables to prioritize collection action. |
| FI-KPI-012 | Top 10 Outstanding Customers | Customer | Quantifies customer receivables exposure for credit and collection management. |
| FI-KPI-013 | Overdue Exposure | Customer | Identifies customers with overdue receivables to prioritize collection action. |
| FI-KPI-014 | >90d Exposure | Customer | Identifies customers with overdue receivables to prioritize collection action. |
| FI-KPI-015 | Overdue Concentration % | Customer | Identifies customers with overdue receivables to prioritize collection action. |
| FI-KPI-016 | Cash Collected MTD | Customer | Measures cash collected from customer payments during the month. |
| FI-KPI-017 | Recovery vs Billing % | Customer | Compares collections recovered against customer billing to assess receivables efficiency. |
| FI-KPI-018 | Payment Mix — Cash | Customer | Shows share of customer payments received in cash, reflecting customer payment behavior. |
| FI-KPI-019 | Payment Mix — Giro | Customer | Shows share of customer payments via giro, reflecting customer payment mix. |
| FI-KPI-020 | Payment Mix — Adjustment | Customer | Shows share of customer payment adjustments, reflecting customer settlement patterns. |
| FI-KPI-021 | Legacy Debt Count | Customer | Identifies customers with overdue receivables to prioritize collection action. |
| FI-KPI-022 | Aging Risk Summary — 1–30 Days | Customer | Assesses customer credit or payment risk for proactive portfolio management. |
| FI-KPI-023 | Aging Risk Summary — 31–60 Days | Customer | Assesses customer credit or payment risk for proactive portfolio management. |
| FI-KPI-024 | Aging Risk Summary — 61–90 Days | Customer | Assesses customer credit or payment risk for proactive portfolio management. |
| FI-KPI-025 | Aging Risk Summary — > 90 Days | Customer | Assesses customer credit or payment risk for proactive portfolio management. |
| FI-KPI-026 | Top Overdue Customers | Customer | Identifies customers with overdue receivables to prioritize collection action. |
| FI-KPI-027 | Top Overdue Salesmen | Salesman | Evaluates salesman portfolio quality through receivables exposure in their accounts. |
| FI-KPI-028 | Top Overdue Wilayah | Customer | Ranks regions by overdue customer exposure to prioritize geographic collection focus. |
| FI-KPI-029 | Expected Cash Collection | Customer | Forecasts expected cash inflows from upcoming customer collections. |
| FI-KPI-030 | Projected Month-End Collection | Customer | Projects month-end collection totals from customer receivables. |
| FI-KPI-031 | Collection Forecast % | Customer | Expresses collection forecast as a percentage of customer billing expectations. |
| FI-KPI-032 | Daily Cash Collection Average | Customer | Averages daily cash collected from customers for collection pacing. |
| FI-KPI-033 | Required Daily Collection | Customer | Calculates daily collection needed to meet customer receivables targets. |
| FI-KPI-034 | Remaining Collection Target | Customer | Shows remaining collection target owed by customers for the period. |
| FI-KPI-035 | Days Remaining | Customer | Counts days remaining to complete planned customer collections. |
| FI-KPI-036 | Recovery vs Billing Forecast | Customer | Forecasts recovery versus billing ratio based on expected customer payments. |
| FI-KPI-037 | Scenario Cash (Best / Expected / Worst) | Customer | Models best, expected, and worst cash collection scenarios from customer receivables. |
| FI-KPI-038 | Forecast Confidence (Cash) | Customer | Indicates confidence in the customer cash collection forecast. |
| FI-KPI-039 | Outstanding Due Remaining | Customer | Shows outstanding customer amounts still due for collection. |
| FI-KPI-040 | Collection Gap | Customer | Quantifies the gap between forecast and required customer collections. |
| FI-KPI-041 | Top Collection Risks (Table) | Customer | Assesses customer credit or payment risk for proactive portfolio management. |
| FI-KPI-042 | Report Footer — Total Piutang | Customer | Quantifies customer receivables exposure for credit and collection management. |
| FI-KPI-043 | Report Footer — Total Customer | Customer | Counts customers with outstanding balances for receivables portfolio sizing. |
| IN-KPI-001 | Total Inventory Value | Item | Quantifies total stock value for inventory asset management. |
| IN-KPI-002 | Total Item | Item | Counts distinct items in stock for inventory portfolio sizing. |
| IN-KPI-003 | Top 10 Category (Ranking) | Item | Measures inventory concentration or risk by product category. |
| IN-KPI-004 | Top 10 Supplier (Ranking) | Supplier | Ranks supplier or principal share to assess purchasing concentration. |
| IN-KPI-005 | Dead Stock Count & Value | Item | Identifies non-moving inventory tying up capital and requiring clearance action. |
| IN-KPI-006 | Slow Moving Count & Value | Item | Flags products with sluggish movement for inventory optimization decisions. |
| IN-KPI-007 | Never Sold Count & Value | Item | Identifies items with no sales history indicating product demand failure. |
| IN-KPI-008 | At-Risk Inventory % | Item | Summarizes overall inventory health and exposure to write-down or obsolescence. |
| IN-KPI-009 | Aging Distribution (Movement Classes) | Item | Classifies items by movement velocity to prioritize inventory intervention. |
| IN-KPI-010 | Category Risk Exposure | Item | Measures inventory concentration or risk by product category. |
| IN-KPI-011 | Supplier Risk Exposure | Supplier | Quantifies inventory or purchasing exposure tied to supplier concentration risk. |
| IN-KPI-012 | Top 10 Dead / Slow Moving (Ranking) | Item | Flags products with sluggish movement for inventory optimization decisions. |
| IN-KPI-013 | Projected Inventory Value @ Horizon | Item | Quantifies total stock value for inventory asset management. |
| IN-KPI-014 | Average Days of Supply (Company) | Item | Measures how long current stock will last based on demand, guiding replenishment. |
| IN-KPI-015 | Inventory Health Score | Item | Summarizes overall inventory health and exposure to write-down or obsolescence. |
| IN-KPI-016 | Stock-Out Risk Items / Value | Item | Assesses inventory balance risk between stock-outs and excess holding. |
| IN-KPI-017 | Overstock / Understock Value | Item | Assesses inventory balance risk between stock-outs and excess holding. |
| IN-KPI-018 | Scenario Projected Value (Best/Expected/Worst) | Item | Projects future inventory levels to support stock planning decisions. |
| IN-KPI-019 | Forecast Confidence (Inventory) | Item | Projects future inventory levels to support stock planning decisions. |
| IN-KPI-020 | Days of Supply (Item) | Item | Measures how long current stock will last based on demand, guiding replenishment. |
| IN-KPI-021 | Recommended Purchase Qty (Indicative) | Item | Recommends replenishment actions to optimize item stock levels. |
| IN-KPI-022 | Critical Actions Count | Item | Counts recommended inventory actions for item-level stock remediation. |
| IN-KPI-023 | Recommended Purchase Budget | Item | Recommends replenishment actions to optimize item stock levels. |
| IN-KPI-024 | Recoverable Capital | Item | Quantifies capital recoverable from inventory optimization actions on items. |
| IN-KPI-025 | Action Counts by Type (Purchase / Delay / Transfer / Clearance) | Item | Counts recommended inventory actions for item-level stock remediation. |
| IN-KPI-026 | Report Footer — Total Inventory Value | Item | Quantifies total stock value for inventory asset management. |
| IN-KPI-027 | Report Footer — Total Item | Item | Counts distinct items in stock for inventory portfolio sizing. |
| OP-KPI-001 | Top 1 Warehouse Inventory % | Item | Measures inventory concentration in the top warehouse, indicating stock allocation risk. |
| OP-KPI-002 | Top 3 Warehouse Inventory % | Item | Measures inventory concentration across top warehouses for stock distribution review. |
| OP-KPI-003 | Top 1 Warehouse At-Risk % | Item | Shows at-risk inventory concentration in the primary warehouse. |
| OP-KPI-004 | Top 1 Warehouse Sales % | Salesman | Measures sales concentration by warehouse, reflecting branch or territory sales performance. |
| OP-KPI-005 | Top 1 Wilayah Sales % | Salesman | Measures regional sales concentration to assess wilayah-level sales force performance. |
| OP-KPI-006 | Inactive Warehouse With Stock Count | Item | Counts inactive warehouses still holding stock, indicating stranded inventory risk. |
| OP-KPI-007 | Top Warehouse by Inventory (Ranking) | Item | Ranks warehouses by inventory value for stock location management. |
| OP-KPI-008 | Top Warehouse by At-Risk (Ranking) | Item | Ranks warehouses by at-risk inventory for stock remediation prioritization. |
| OP-KPI-009 | Top Warehouse by Sales (Ranking) | Salesman | Ranks warehouses by sales output to compare branch or territory sales performance. |
| OP-KPI-010 | Top Warehouse by Purchasing (Ranking) | Supplier | Ranks warehouses by purchasing volume to assess supplier intake by location. |
| OP-KPI-011 | Top Wilayah by Sales (Ranking) | Salesman | Ranks wilayah by sales to evaluate regional salesman territory performance. |
| OP-KPI-012 | Location Attention Signal Counts | Item | Counts location attention signals driven primarily by warehouse inventory and stock risk. |
| PU-KPI-001 | Grand Total Purchase | Supplier | Measures total purchase spend, reflecting overall supplier procurement volume. |
| PU-KPI-002 | Total Invoice | Supplier | Counts supplier purchase invoices processed in the period. |
| PU-KPI-003 | Posted % | Supplier | Tracks percentage of supplier invoices posted, reflecting procurement processing efficiency. |
| PU-KPI-004 | Pending Posting Value | Supplier | Values unposted supplier invoices blocking procurement visibility. |
| PU-KPI-005 | Qualified Backlog Count & Value | Supplier | Counts and values qualified purchase backlog awaiting supplier invoice processing. |
| PU-KPI-006 | Top 1 Principal % | Supplier | Measures principal or supplier concentration and dependency in purchasing or inventory. |
| PU-KPI-007 | Top 3 Principal % | Supplier | Measures principal or supplier concentration and dependency in purchasing or inventory. |
| PU-KPI-008 | Compound Dependency Count | Supplier | Identifies compound supplier dependency that creates procurement concentration risk. |
| PU-KPI-009 | Purchasing Inactivity Flag | Supplier | Flags inactive purchasing activity that may signal supplier relationship gaps. |
| PU-KPI-010 | Principal At-Risk Count | Supplier | Measures principal or supplier concentration and dependency in purchasing or inventory. |
| PU-KPI-011 | Top 10 Principal (Ranking) | Supplier | Measures principal or supplier concentration and dependency in purchasing or inventory. |
| PU-KPI-012 | Principal Exposure Comparison | Supplier | Measures principal or supplier concentration and dependency in purchasing or inventory. |
| PU-KPI-013 | Report Footer — Grand Total Purchase | Supplier | Report total of all supplier purchases for procurement review. |
| PU-KPI-014 | Report Footer — Total Invoice | Supplier | Report count of supplier invoices for procurement reconciliation. |
| SA-KPI-004 | Total Faktur | Salesman | Measures total invoiced sales output as a core salesman performance result. |
| SA-KPI-005 | Total Customer | Customer | Counts customers transacting in the sales period, reflecting customer base engagement. |
| SA-KPI-006 | Weekly Invoiced Sales Trend | Salesman | Shows weekly invoiced sales trend to monitor salesman and team revenue momentum. |
| SA-KPI-007 | Top 10 Salesman (Omzet) | Salesman | Ranks salesman revenue or performance for sales force comparison. |
| SA-KPI-008 | Target vs Achievement Chart | Salesman | Compares target versus achievement to evaluate sales force performance visually. |
| SA-KPI-009 | Current Sales | Salesman | Tracks current-period sales as input to salesman target attainment forecasting. |
| SA-KPI-010 | Current Achievement % | Salesman | Measures current achievement percentage against the active sales target. |
| SA-KPI-011 | Forecast Sales (Expected) | Salesman | Projects expected sales to guide salesman performance planning for the period. |
| SA-KPI-012 | Forecast Achievement % | Salesman | Forecasts achievement percentage to assess likelihood of salesman target completion. |
| SA-KPI-013 | Daily Average Sales | Salesman | Calculates daily average sales needed to contextualize salesman run-rate performance. |
| SA-KPI-014 | Required Daily Sales | Salesman | Defines required daily sales to close the gap for salesman target achievement. |
| SA-KPI-015 | Target Gap | Salesman | Quantifies remaining sales gap that salesman management must address before period end. |
| SA-KPI-016 | Days Remaining | Salesman | Tracks days left in the period to pace salesman target execution. |
| SA-KPI-017 | Scenario Projection (Best / Expected / Worst) | Salesman | Models best, expected, and worst sales scenarios for salesman planning decisions. |
| SA-KPI-018 | Forecast Confidence | Salesman | Indicates reliability of the sales forecast used to manage salesman expectations. |
| SA-KPI-019 | Forecast Risk Indicator | Salesman | Flags forecast risk that may prevent salesman teams from hitting targets. |
| SF-KPI-001 | Below Target Count | Salesman | Assesses salesman performance against assigned sales targets. |
| SF-KPI-002 | Missing Target Setup Count | Salesman | Assesses salesman performance against assigned sales targets. |
| SF-KPI-003 | High Overdue Exposure Count | Salesman | Evaluates salesman portfolio quality through receivables exposure in their accounts. |
| SF-KPI-004 | High Piutang Exposure Count | Salesman | Evaluates salesman portfolio quality through receivables exposure in their accounts. |
| SF-KPI-005 | Dormant Portfolio Count | Salesman | Counts salesmen whose customer portfolios show inactivity, indicating coverage gaps. |
| SF-KPI-006 | Top Omzet Salesman % | Salesman | Ranks salesman revenue or performance for sales force comparison. |
| SF-KPI-007 | Top Piutang Salesman % | Salesman | Evaluates salesman portfolio quality through receivables exposure in their accounts. |
| SF-KPI-008 | Top 10 Omzet (Ranking) | Salesman | Measures salesman performance, targets, or field activity for sales management. |
| SF-KPI-009 | Top 10 Achievement % (Ranking) | Salesman | Assesses salesman performance against assigned sales targets. |
| SF-KPI-010 | Top 10 Piutang (Ranking) | Salesman | Evaluates salesman portfolio quality through receivables exposure in their accounts. |
| SF-KPI-011 | Principal Achievement Table | Salesman | Assesses salesman performance against assigned sales targets. |
| SF-KPI-012 | Planned Visits | Salesman | Measures field visit execution as core salesman activity performance. |
| SF-KPI-013 | Actual Visits | Salesman | Measures field visit execution as core salesman activity performance. |
| SF-KPI-014 | Missed Visits | Salesman | Measures field visit execution as core salesman activity performance. |
| SF-KPI-015 | Unplanned Visits | Salesman | Measures field visit execution as core salesman activity performance. |
| SF-KPI-016 | Effective Calls | Salesman | Evaluates quality of salesman customer interactions that generate outcomes. |
| SF-KPI-017 | Visit Execution % | Salesman | Measures field visit execution as core salesman activity performance. |
| SF-KPI-018 | Effective Call Rate | Salesman | Evaluates quality of salesman customer interactions that generate outcomes. |
| — | Active Salesmen | Salesman | Counts salesmen currently active in the field for workforce coverage assessment. |
| — | Bottom Effective Call Rate | Salesman | Evaluates quality of salesman customer interactions that generate outcomes. |
| — | Bottom Visit Execution | Salesman | Measures field visit execution as core salesman activity performance. |
| — | GPS Valid Rate | Salesman | Validates salesman field visit location accuracy for activity compliance. |
| — | Omzet Generated | Salesman | Links field activity to salesman-generated orders and revenue outcomes. |
| — | Orders Generated | Salesman | Links field activity to salesman-generated orders and revenue outcomes. |
| — | Sales Orders | Salesman | Links field activity to salesman-generated orders and revenue outcomes. |
| — | Top Effective Call Rate | Salesman | Evaluates quality of salesman customer interactions that generate outcomes. |
| — | Top Omzet | Salesman | Measures salesman performance, targets, or field activity for sales management. |
| — | Top Orders | Salesman | Links field activity to salesman-generated orders and revenue outcomes. |
| — | Top Visit Execution | Salesman | Measures field visit execution as core salesman activity performance. |
