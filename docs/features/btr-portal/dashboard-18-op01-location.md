# OP01 — Locations Dashboard KPI Explanation

## Dashboard Purpose

**Dashboard:** Branch / Warehouse Performance Dashboard  
**Menu:** OP01 — Locations  
**Route:** `/dashboard/locations`

OP01 helps management answer:

> Are we becoming too dependent on a particular warehouse or territory?

The dashboard focuses on concentration and location-related operational signals. It is not a warehouse productivity dashboard and it does not measure receivable risk by wilayah; that analysis belongs to the Collection dashboard.

## Reading the Dashboard

- A high concentration percentage means that a large share of company activity or capital depends on one warehouse or wilayah.
- Concentration cards are informational indicators. They do not have an automatic good/bad threshold.
- Warehouse rankings show the largest contributors and their percentage of the company total.
- “MTD” means month-to-date.
- Warehouse rankings exclude inactive, special, and In-Transit warehouses unless the metric specifically identifies inactive stock.
- Wilayah is derived from the customer on the Faktur.

## Section 1 — Location Attention Cards

This section provides a quick management view of location concentration and exceptions.

### 1.1 Inventory Concentration

#### Top Warehouse Inventory %

1. **KPI Canonical:** OP-KPI-001 — Top 1 Warehouse Inventory %
2. **Question Answered:** How much of the company’s inventory is held by the largest warehouse?
3. **Definition:** The inventory value of the number-one warehouse divided by total company inventory.
4. **Business Meaning:** A high percentage indicates dependence on one warehouse and concentration of working capital in one location. A disruption, capacity issue, or stock-management problem at that warehouse can affect the wider business.
5. **How to Interpret:** Investigate a high or rising value together with the warehouse’s stock quality, service demand, and capacity. Management may need to rebalance stock or review whether the concentration is commercially intentional.

#### Top 3 Warehouse Inventory %

1. **KPI Canonical:** OP-KPI-002 — Top 3 Warehouse Inventory %
2. **Question Answered:** How much inventory is concentrated in the three largest warehouses?
3. **Definition:** The combined inventory value of the top three warehouses divided by total company inventory.
4. **Business Meaning:** This shows whether inventory is broadly distributed across the network or concentrated in a small group of warehouses. It is useful for identifying network-level dependency that may not be visible from the largest warehouse alone.
5. **How to Interpret:** A high value means the inventory network is concentrated in a few locations. Compare it with the Top Warehouse Inventory % to distinguish single-site dependency from three-site dependency.

### 1.2 At-Risk Concentration

#### Top Warehouse At-Risk %

1. **KPI Canonical:** OP-KPI-003 — Top 1 Warehouse At-Risk %
2. **Question Answered:** How much of the company’s at-risk inventory is located in the warehouse with the greatest at-risk exposure?
3. **Definition:** At-risk inventory value in the top warehouse divided by total at-risk inventory value.
4. **Business Meaning:** This identifies where inventory risk is concentrated. A warehouse may be the main location requiring clearance, transfer, demand stimulation, or closer stock review.
5. **How to Interpret:** A high value means inventory risk is localized rather than evenly distributed. Review the warehouse’s dead stock, slow-moving stock, and possible transfer or clearance actions.

### 1.3 Sales Concentration

#### Top Warehouse Sales %

1. **KPI Canonical:** OP-KPI-004 — Top 1 Warehouse Sales %
2. **Question Answered:** How much of month-to-date sales is billed from the largest warehouse?
3. **Definition:** Month-to-date omzet from the top warehouse divided by total month-to-date sales.
4. **Business Meaning:** This indicates dependence on one billing or distribution location for revenue production. A location issue could therefore become a sales-continuity issue.
5. **How to Interpret:** A high value requires comparison with inventory concentration and warehouse capacity. High sales concentration with low inventory concentration may indicate strong commercial dependence on one location without a matching stock concentration.

#### Top Wilayah Sales %

1. **KPI Canonical:** OP-KPI-005 — Top 1 Wilayah Sales %
2. **Question Answered:** How much of month-to-date sales comes from the largest customer territory?
3. **Definition:** Month-to-date omzet from the top Wilayah divided by total month-to-date sales.
4. **Business Meaning:** This measures geographic revenue dependency. It helps management identify whether sales performance depends heavily on one territory or market area.
5. **How to Interpret:** A high or increasing value can represent a strong market opportunity, but also a concentration risk. Review territory coverage, customer dependency, sales-force capacity, and whether other territories require development.

### 1.4 Operational Signals

#### Inactive Warehouse With Stock

1. **KPI Canonical:** OP-KPI-006 — Inactive Warehouse With Stock Count
2. **Question Answered:** How many inactive warehouses still contain inventory?
3. **Definition:** The count of inactive warehouses with inventory greater than zero, excluding In-Transit.
4. **Business Meaning:** Stock in an inactive warehouse may be stranded, difficult to serve, or overlooked during normal warehouse operations. It can represent trapped working capital and data or network-cleanup risk.
5. **How to Interpret:** Any non-zero value warrants investigation. Confirm whether the warehouse was intentionally closed, then plan stock transfer, clearance, adjustment, or reactivation as appropriate.

#### Stock Without Sales

1. **KPI Canonical:** Not Found
2. **Question Answered:** Which warehouse locations hold inventory but have no corresponding sales activity?
3. **Definition:** A location signal identifying inventory held by a warehouse with no sales activity for the relevant period.
4. **Business Meaning:** Stock without sales may indicate weak demand, incorrect allocation, inactive operations, or a mismatch between stock placement and market coverage.
5. **How to Interpret:** Review the warehouse’s inventory age, item mix, customer coverage, and replenishment history. Consider transfer, clearance, sales activation, or correction of the warehouse assignment.

## Section 2 — Top Warehouse by Inventory

### Top 10 Warehouse by Inventory Value

1. **KPI Canonical:** OP-KPI-007 — Top Warehouse by Inventory (Ranking)
2. **Question Answered:** Which warehouses hold the most inventory value?
3. **Definition:** The ten active, non-special warehouses with the highest inventory value, displayed with each warehouse’s share of the company total.
4. **Business Meaning:** This ranking identifies where the largest amount of capital is physically or logically allocated. It supports stock balancing, warehouse capacity review, and prioritization of inventory investigations.
5. **How to Interpret:** Focus first on warehouses with both high value and high inventory risk. A large inventory value is not automatically a problem; it becomes more concerning when it is not supported by sales demand or healthy stock movement.

**Table content:** Rank, Code, Warehouse, Amount, and % of Total.

## Section 3 — Top Warehouse by At-Risk Inventory

### Top 10 Warehouse by At-Risk Value

1. **KPI Canonical:** OP-KPI-008 — Top Warehouse by At-Risk (Ranking)
2. **Question Answered:** Which warehouses contain the greatest value of at-risk inventory?
3. **Definition:** The ten warehouses with the highest at-risk inventory value, displayed with each warehouse’s share of the company total at-risk value.
4. **Business Meaning:** This ranking prioritizes locations where inventory remediation can protect cash and reduce obsolescence exposure.
5. **How to Interpret:** Start with warehouses at the top of the list and determine whether the appropriate response is clearance, transfer, sales activation, return handling, or purchase reduction.

**Table content:** Rank, Code, Warehouse, Amount, and % of Total.

## Section 4 — Top Warehouse by Sales

### Top 10 Warehouse by MTD Omzet

1. **KPI Canonical:** OP-KPI-009 — Top Warehouse by Sales (Ranking)
2. **Question Answered:** Which warehouses generate the most month-to-date sales?
3. **Definition:** The ten warehouses with the highest month-to-date sales omzet, displayed with their share of total month-to-date sales.
4. **Business Meaning:** This shows the locations carrying the greatest revenue contribution. It supports branch performance comparison and business-continuity planning.
5. **How to Interpret:** Compare the ranking with inventory and purchasing rankings. High sales with insufficient stock may create service risk; high stock with low sales may indicate allocation or demand problems.

**Table content:** Rank, Code, Warehouse, Amount, and % of Total.

## Section 5 — Top Warehouse by Purchasing

### Top 10 Warehouse by MTD Purchase

1. **KPI Canonical:** OP-KPI-010 — Top Warehouse by Purchasing (Ranking)
2. **Question Answered:** Which warehouses receive or represent the greatest month-to-date purchasing volume?
3. **Definition:** The ten warehouses with the highest month-to-date purchase value, displayed with their share of total month-to-date purchasing.
4. **Business Meaning:** This identifies where procurement intake is concentrated. It helps management compare purchasing activity with sales demand and inventory accumulation.
5. **How to Interpret:** High purchasing with low sales or high at-risk inventory deserves review. It may indicate over-replenishment, delayed sell-through, or a location-specific demand issue.

**Table content:** Rank, Code, Warehouse, Amount, and % of Total.

## Section 6 — Top Wilayah by Sales

### Top 10 Wilayah by MTD Omzet

1. **KPI Canonical:** OP-KPI-011 — Top Wilayah by Sales (Ranking)
2. **Question Answered:** Which customer territories contribute the most month-to-date sales?
3. **Definition:** Customer Wilayah ranked by month-to-date Faktur omzet.
4. **Business Meaning:** This identifies the strongest commercial territories and the geographic areas most important to current revenue.
5. **How to Interpret:** Use the ranking to recognize strong territories and identify geographic concentration. A territory with high sales may also require collection-risk review in the Collection dashboard.

**Table content:** Rank, Wilayah, MTD Omzet, and % of Total.

## Section 7 — Location Attention List

### Location Attention Signal Counts

1. **KPI Canonical:** OP-KPI-012 — Location Attention Signal Counts
2. **Question Answered:** Which warehouses have location-related conditions that require management attention?
3. **Definition:** Warehouse-by-signal rows covering inactive warehouses with stock, warehouses with inventory but no sales, and concentration signals generated by ranking positions.
4. **Business Meaning:** This converts location concentration and exceptions into an actionable investigation queue. It helps management move from a company-level indicator to the specific warehouse that needs review.
5. **How to Interpret:** Review inactive-stock signals first, then investigate no-sales and concentration signals. Multiple signals on the same warehouse indicate a higher-priority location review.

**Table content:** Warehouse, Signal, Detail, and an optional Investigate action.

## Section 8 — Navigation

The navigation section provides access to related analysis:

- Inventory
- Inventory Risk
- Sales
- Purchasing
- Collection
- Customer Analytics
- Salesman Performance

These links support follow-up analysis. For example, an at-risk warehouse can be investigated in Inventory Risk, while a high-sales Wilayah can be reviewed in Collection for overdue exposure.

## Chart Availability

OP01 currently contains **no chart**. Its visual analysis is provided through:

- Informational KPI cards
- Five Top 10 ranking tables
- The Location Attention List

## Management Review Sequence

1. Check inventory and at-risk concentration.
2. Identify the warehouses at the top of the inventory and at-risk rankings.
3. Compare warehouse sales, purchasing, and inventory positions.
4. Review the Top Wilayah sales ranking for geographic dependency.
5. Open the Location Attention List and investigate warehouses with multiple signals.
