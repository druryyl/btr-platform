# BTR Platform — Business Profile

**Audience:** Business owners, directors, operations managers, consultants  
**Purpose:** Business-oriented profile of the BTR Platform application — what problem it solves, who it serves, and the business value it delivers.  
**Scope:** Business language only. Not technical architecture, frameworks, or implementation details.

**Sources:** `docs/foundation/DOMAIN.md`, `PRODUCT.md`, `WORKFLOW.md`, `LANDSCAPE.md`, and related feature artifacts.

---

## 1. What Business Problem the Application Solves

BTR Platform solves the day-to-day operating problem of a **distribution company**: turning customer demand into invoices, fulfilled goods, collected cash, accurate stock, and management visibility.

It is built around distributor realities such as:

- Field sales visits and offline order capture
- Warehouse picking and packing
- Customer receivables (Piutang) and payment collection
- Customer returns (Retur)
- Supplier/principal relationships
- Stock opname (physical inventory reconciliation)
- Management attention signals across sales, inventory, collection, and purchasing

The product is explicitly **not** a full ERP or accounting platform. It is an **operational system of record** for distribution businesses, with a companion management portal for visibility and decision support.

---

## 2. What Type of Companies Would Benefit

The best-fit companies are **established wholesalers or distributors** with:

- Mobile sales teams visiting customers daily
- Warehouse operations (picking, packing, fulfillment)
- Credit sales and recurring collection risk
- Many SKUs and multiple suppliers/principals
- Inventory capital significant enough that slow-moving or dead stock matters

Likely industries include:

- FMCG and consumer goods distribution
- Building materials and construction supplies
- Automotive parts and spare parts
- Pharmaceuticals and medical supplies
- Electronics and electrical distribution
- B2B wholesale distribution generally

---

## 3. Ideal Customer Profile (ICP)

The likely ICP is an **owner-led or director-led distribution business** that has outgrown spreadsheets but does not necessarily need a full ERP replacement. They may already have accounting software, legacy ERP, or custom desktop tools, but still struggle with **operational execution and visibility**.

### Typical signs they need this kind of system

| Signal | Why it matters |
| ------ | -------------- |
| Sales reps collect orders in the field, often with unreliable internet | Needs offline-capable mobile order capture and sync |
| Office admins convert orders into invoices and coordinate fulfillment | Needs order-to-invoice workflow and warehouse handoff |
| Warehouse teams handle picking, packing, stock, and returns | Needs operational inventory control, not just accounting stock |
| Finance teams spend too much time monitoring overdue receivables | Needs receivable aging, collection visibility, and payment recording |
| Management needs daily answers to “what requires attention?” | Needs dashboards, alerts, and attention lists |
| Inventory capital is large enough that slow-moving or dead stock matters | Needs inventory health analytics and obsolescence signals |
| Multiple principals/suppliers with per-rep targets | Needs principal assignment and achievement tracking |
| Field route compliance is a management concern | Needs visit plans, check-ins, and route execution metrics |

---

## 4. Operational Challenges It Addresses

| Challenge | How BTR addresses it |
| --------- | --------------------- |
| Fragmented order-to-cash operations | Single operational system from Sales Order → Faktur → fulfillment → collection |
| Manual or delayed field order entry | Mobile sales app (BTrade3) with offline capture and sync |
| Inaccurate or delayed stock visibility | Real-time inventory updates on invoice, purchase, return, and opname |
| Receivable aging and uncollected debt | Piutang tracking, aging buckets, collection dashboards, attention signals |
| Customer returns processing | Retur workflow with inspection, inventory decision, and receivable adjustment |
| Purchasing backlog and supplier dependency | Purchase recording, posting status, qualified backlog, principal concentration |
| Route compliance gaps | Visit plan materialization, check-in tracking, visit execution metrics |
| Management reporting delays | BTR Portal with materialized dashboards, alerts, and investigation drill-down |
| Disconnected field, warehouse, and office teams | Synchronized desktop, mobile, and warehouse applications |

---

## 5. Business Outcomes It Delivers

### Operational outcomes

- Faster sales transaction processing
- Accurate inventory records maintained across warehouses and depots
- Effective receivable monitoring and collection prioritization
- Reliable warehouse fulfillment (picking and packing)
- Processed customer returns with inventory and receivable reconciliation
- Recorded customer payments and updated outstanding balances
- Operational and regulatory reports (including tax reporting)

### Management outcomes

- Daily visibility into sales achievement vs target
- Receivable exposure and aging at a glance
- Collection effectiveness (recovery vs billing)
- Inventory capital composition and at-risk stock identification
- Purchasing spend monitoring and posting backlog alerts
- Customer, salesman, and territory attention signals
- Field activity and route compliance visibility
- Faster morning executive review via Management Attention Center and Alert Center

### Financial / strategic outcomes

- Reduced working-capital leakage from overdue receivables
- Better inventory capital allocation (fewer dead-stock surprises)
- Clearer purchasing decisions and supplier dependency awareness
- Improved field sales accountability and coaching data
- Single source of truth for operational data (external systems sync from BTR)

---

## 6. Business Functions and Workflows Supported

### Core operational workflows

1. **Sales Workflow** — Customer → Sales Order → Faktur → Warehouse Fulfillment → Customer Receives Goods → Faktur Kembali
2. **Receivable Collection Workflow** — Customer Receivable → Collection Visit → Customer Payment → Payment Recording
3. **Return Workflow** — Customer Return → Return Inspection → Inventory Decision → Receivable Adjustment
4. **Purchasing Workflow** — Purchase → Stock Receipt → Inventory Update
5. **Inventory Reconciliation Workflow** — Physical Stock Count → Inventory Comparison → Inventory Adjustment

### Supporting workflows

- **Mobile Sales Synchronization** — Field orders and check-ins synced to main system
- **Warehouse Synchronization** — Fulfillment activities exchange data with desktop
- **Territory Execution Plan** — Route templates materialized into dated visit schedules with exception overlay

### Business areas and concepts

| Area | Key concepts |
| ---- | ------------ |
| Master Data | Customer, Item, Supplier/Principal, Sales Person |
| Sales | Sales Order, Faktur, Faktur Kembali, Sales Target, Principal Assignment |
| Inventory | Inventory, Warehouse, Depo, Retur, Stok Opname, Picking, Packing |
| Finance | Piutang, Payment, CoreTax Export |
| Purchasing | Purchase, Posting Status |
| Field Sales | Visit Plan, Check-in, Route Compliance, Effective Call |
| Management | Dashboards, Reports, Attention Signals, Alerts, Investigation |

### Target users (by role)

| Role | Responsibility |
| ---- | -------------- |
| Sales Administration | Sales transaction processing, customer validation |
| Sales Personnel | Customer visits, order collection, invoice delivery, payment collection |
| Warehouse Staff | Picking and packing goods |
| Inventory Administration | Inventory monitoring, stock adjustment, return processing |
| Purchasing Administration | Purchase recording, stock replenishment |
| Finance Administration | Receivable management, payment recording, tax reporting |
| Management / Owners | Daily review, attention prioritization, performance monitoring |

---

## 7. Maturity Level of Businesses That Would Need This

BTR is designed for businesses that have **moved beyond startup/manual operations** but may not require or afford enterprise ERP complexity.

| Maturity stage | Fit |
| -------------- | --- |
| **Early growth (outgrowing Excel)** | Partial fit — may need simpler tools first unless field sales and warehouse complexity already exist |
| **Established distributor (10–100+ staff)** | **Strong fit** — multiple roles, field teams, warehouse, credit sales |
| **Multi-location / multi-principal distributor** | **Strong fit** — warehouse logic, principal targets, territory management |
| **Owner-led with operational pain** | **Strong fit** — management portal and attention signals align with owner review habits |
| **Enterprise requiring full GL/HR/manufacturing** | **Partial fit** — BTR complements ERP; does not replace accounting or HR |

**Sweet spot:** A distribution company with enough operational complexity (field sales + warehouse + credit + multiple SKUs/principals) that spreadsheets and ad-hoc reports no longer scale, but that wants a **practical operational system** rather than a full ERP implementation.

---

## 8. Alternative Solutions Companies Might Currently Use

| Alternative | Limitation for this use case |
| ----------- | ------------------------------ |
| **Excel spreadsheets** | No real-time sync, error-prone, no field/mobile workflow, no single source of truth |
| **Existing ERP systems** | Often too accounting-focused, rigid, or expensive for distributor-specific field/warehouse workflows |
| **BI dashboards (Power BI, Tableau, etc.)** | Report on data but do not run operations; require separate transactional system |
| **Internal reporting teams** | Manual, delayed, not actionable at operational level |
| **Generic CRM** | Customer relationship focus; weak on inventory, invoicing, receivables, warehouse |
| **Standalone SFA (Sales Force Automation)** | Order capture only; weak on warehouse, receivables, purchasing integration |
| **Standalone WMS** | Warehouse only; disconnected from sales and finance |
| **Accounting software (Accurate, etc.)** | Invoicing and GL; weak on field sales, route compliance, operational inventory |
| **Custom legacy desktop apps** | May work but lack mobile, portal, and modern management visibility |

---

## 9. Software Category / Categories

| Category | Relevance |
| -------- | --------- |
| **Distributor Management System (DMS)** | Primary category |
| **Sales Force Automation (SFA) for distributors** | Mobile order capture, visit tracking |
| **Order-to-cash operations software** | Sales Order → Faktur → fulfillment → collection |
| **Inventory and receivables control system** | Stock, piutang, returns |
| **Distribution ERP extension** | Operational layer; not full ERP |
| **Operational BI / management dashboard** | BTR Portal for decision support |
| **Field sales and warehouse operations platform** | Mobile + warehouse sync |

---

## 10. Keywords for Market Research

- distributor management system
- wholesale distribution software
- sales force automation for distributors
- order taking app for sales reps
- receivables management for distributors
- inventory control for wholesale business
- dead stock dashboard
- field sales route compliance
- distributor BI dashboard
- FMCG distributor software
- piutang aging dashboard
- distributor order to cash
- wholesale inventory management Indonesia

---

## 11. Keywords for Competitor Research

- Odoo distribution management
- SAP Business One distributor
- Microsoft Dynamics wholesale distribution
- Accurate Online distribution
- ERP distributor Indonesia
- FMCG sales force automation
- mobile salesman order app
- warehouse inventory software for distributors
- Power BI distributor dashboard
- distributor ERP comparison
- SFA distributor Indonesia
- wholesale ERP alternatives

---

## 12. Keywords for Pricing and Willingness-to-Pay Research

- distributor ERP pricing
- sales force automation pricing
- inventory management software pricing Indonesia
- custom ERP development cost
- wholesale distribution software subscription
- field sales app pricing
- Power BI dashboard consulting pricing
- custom business application development cost
- distributor software ROI
- order management system pricing SME
- receivables management software cost

---

## 13. Potential Consulting and Custom Development Services

Services that could be offered around this type of solution:

| Service | Description |
| ------- | ----------- |
| **Distributor operations assessment** | Map current workflows, pain points, and fit for custom vs packaged solutions |
| **Custom order-to-cash system development** | Sales order, invoicing, fulfillment, collection workflows |
| **Mobile sales order app development** | Offline-capable field order capture and sync |
| **Warehouse workflow digitization** | Picking, packing, stock opname, return processing |
| **Receivables and collection dashboards** | Aging, attention lists, recovery metrics |
| **Inventory health analytics** | Slow-moving, dead stock, at-risk capital visibility |
| **ERP/BI integration** | Connect operational system to accounting or reporting tools |
| **Legacy desktop modernization** | Migrate or extend existing distributor desktop systems |
| **Management reporting portal** | Executive dashboards, alerts, investigation drill-down |
| **Data cleanup and master-data governance** | Customer, item, supplier, warehouse reference data |
| **Custom workflow automation** | Visit plans, route compliance, principal targets, attention signals |
| **Implementation and change management** | Rollout support for field, warehouse, and office teams |

---

## 14. Concise Business Description (Freelance Consultant Use)

> I help distribution and wholesale businesses replace scattered spreadsheets, manual reporting, and disconnected operational tools with practical custom systems that support sales, inventory, warehouse fulfillment, receivables, purchasing, and management visibility. My focus is on making daily operations faster and giving owners and managers a clear view of what needs attention — from overdue customers and slow-moving stock to sales target gaps and field team execution.
>
> For companies that have outgrown Excel or find standard ERP systems too rigid, I design and build business applications tailored to the way their operations actually work. The goal is not just software delivery, but better control over cash, stock, sales activity, and decision-making.

---

## Document Maintenance

This is a temporary work artifact under `docs/work/`. When business knowledge is incorporated into permanent foundation or feature artifacts, this document may be archived or removed per repository workflow.

**Related permanent knowledge:**

- `docs/foundation/PRODUCT.md` — Product scope and vision
- `docs/foundation/DOMAIN.md` — Business terminology
- `docs/foundation/WORKFLOW.md` — Business workflows
- `docs/foundation/LANDSCAPE.md` — Business areas and system involvement
- `docs/features/btr-portal/btr-portal-domain.md` — Management portal business domain
