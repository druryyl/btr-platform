# M32R Entity Analytics - Product Vision and Visualization Feasibility Study

**Status:** Version 2.0 - Product Vision, ready for Architect planning  
**Milestone:** M32R Entity Analytics Investigation Workspace  
**Date:** 2026-06-27  
**Scope:** Customer, Item, Salesman, Supplier / Principal  
**Audience:** Product Owner, Architect, future implementers, future agents  
**Explicitly excluded:** UI design, database design, API contracts, Vue component proposals, component hierarchy, state management, rendering strategy, implementation planning

**Reference context:**

- `docs/foundation/PRODUCT.md`
- `docs/foundation/DOMAIN.md`
- `docs/foundation/LANDSCAPE.md`
- `docs/foundation/WORKFLOW.md`
- `docs/work/btr-portal/entity-analytics/Entity-Analytics-Feasibility-Study.md`
- `docs/work/btr-portal/entity-analytics/entity-analytics-roadmap-authoritative.md`
- `docs/features/entity-analytics/entity-analytics-developer-guide.md`
- `docs/features/btr-portal/btr-portal-kpi-catalog.md`

---

## 1. Executive Summary

M32R redefines Entity Analytics from a **Profile Summary** product into a **Population Investigation Workspace**.

The purpose of M32R is not to add more charts. The purpose is to help management investigate important business situations:

```text
Which entities deserve attention?
Why do they deserve attention?
How serious is the situation?
What evidence supports the conclusion?
What should management investigate next?
```

The visualizations exist only to support this investigation. The investigation is the product.

The previous feasibility study concluded that XY Scatter should replace Radar as the signature visualization. Product review adopted the deeper direction behind that conclusion: Entity Analytics must begin with the population, not with one isolated profile. M32R should therefore center on a business-facing **Population Map** that shows the full eligible entity population, highlights selected entities, reveals outliers, and navigates management into supporting evidence.

**Primary product recommendation:** M32R should introduce Entity Analytics as an **Investigation Workspace** with the **Population Map** as the primary investigation surface.

Business-facing names should not expose technical chart terminology. Internally, the primary visualization may be implemented using XY Scatter. Product language should describe the management question:

| Entity | Recommended business-facing name | Internal visualization technology |
| --- | --- | --- |
| Customer | Customer Risk Map | XY Scatter / population map |
| Item | Inventory Health Map | XY Scatter / population map |
| Salesman | Sales Performance Map | XY Scatter / population map |
| Supplier / Principal | Purchase Exposure Map | XY Scatter / population map |
| Cross-entity generic term | Portfolio Map or Population Map | XY Scatter / population map |

Radar should **not** be the visual identity of Entity Analytics. It should remain as a secondary **Performance Signature** for executive single-entity summary only.

Distribution analysis should be promoted from supporting visualization to **Core Investigation** because it answers a unique question that Scatter, Ranking, Trend, and Radar do not answer cleanly:

```text
Is this selected entity normal, unusual, or extreme within its peer population for one specific KPI?
```

Relationship presentation should be **Ranked Table plus Embedded Horizontal Bars**. This preserves reporting precision while improving visual readability.

Heatmap should become a **Future Population Review** capability for management review meetings involving dozens of selected entities. It should not be treated as an Entity Profile visualization.

Timeline should become a **Future Investigation** capability focused on business events, not KPI trend lines.

M32R ends at Population Investigation. Future milestones may evolve toward Decision Support by turning investigation conclusions into recommended actions, priority queues, and suggested next steps.

---

## 2. What Is M32R?

### 2.1 Milestone Definition

M32R is the Product milestone that turns Entity Analytics into a management investigation workspace.

It is separate from earlier M32 capability milestones. Earlier M32 work established entity profile infrastructure, history layers, comparison, relationships, attention history, and Performance Signature. M32R defines the next product direction: use those capabilities to support population-level investigation.

### 2.2 Objectives

M32R must enable management to:

1. See the full eligible population for an entity type.
2. Identify outliers, risks, opportunities, and abnormal entities.
3. Compare selected entities while preserving population context.
4. Inspect current KPI facts without leaving the investigation flow.
5. Understand whether a situation is temporary, persistent, improving, or deteriorating.
6. Explain a situation through related customers, items, salesmen, suppliers, and evidence reports.
7. Leave the workspace with a defensible management conclusion.

### 2.3 Scope

M32R covers four entity types:

- Customer
- Item
- Salesman
- Supplier / Principal

M32R covers the product role and relative priority of these investigation capabilities:

- Population Map
- Peer Position / Distribution
- KPI Summary
- Trend / Trajectory
- Ranking History / Position History
- Attention History
- Relationship Drivers
- Evidence Reports
- Performance Signature
- Future Population Review
- Future Business Event Timeline
- Future Decision Support

### 2.4 Non-Goals

M32R does not define:

- Screen layout
- UI component design
- Frontend framework choices
- API routes or contracts
- Database schema
- Data refresh orchestration
- Rendering strategy
- State management
- Detailed implementation roadmap
- AI recommendation logic
- CRM workflow, tasks, notes, or master-data editing

### 2.5 Expected Product Outcome

After M32R, Entity Analytics should no longer feel like opening an entity profile and reading sections. It should feel like entering a workspace where management investigates a population, narrows attention to meaningful entities, explains what is happening, validates evidence, and prepares action.

### 2.6 Relationship to Earlier Entity Analytics Milestones

Earlier Entity Analytics milestones established the profile foundation:

- KPI Summary answers "What are the current facts?"
- Trend answers "What changed over time?"
- Ranking History answers "Where did this entity rank?"
- Attention History answers "What signals fired and persisted?"
- Relationship Engine answers "Which related entities explain this behavior?"
- Performance Signature answers "Is this entity balanced or lopsided?"

M32R does not discard those capabilities. It changes their product role. They become steps inside a broader investigation workflow rather than independent profile sections.

### 2.7 Relationship to Future Decision Support

M32R stops at Population Investigation:

```text
Reporting             -> What happened?
Analytics             -> Why did it happen?
Population Investigation -> Which entities deserve management attention?
Future Decision Support  -> What action should management take?
```

M32R should prepare the foundation for Decision Support, but it must not attempt to design automated recommendations yet.

---

## 3. Product Maturity Model

Entity Analytics should evolve through these maturity levels:

| Level | Product stage | Management question | Product surface |
| --- | --- | --- | --- |
| 1 | Reporting | What happened? | Reports, tabular evidence, KPI totals |
| 2 | Analytics | Why did it happen? | Domain dashboards, trends, rankings, attention lists |
| 3 | Population Investigation | Which entities deserve management attention? | M32R Investigation Workspace |
| Future | Decision Support | What action should management take? | Recommended actions, priority queues, suggested next steps |

M32R is Level 3. It should not be judged by whether it shows more metrics. It should be judged by whether management can discover and investigate important entities faster and with stronger evidence.

---

## 4. Product Principles

### 4.1 Population Before Individual

Entity Analytics should begin with the population because management decisions are usually relative. A single customer's omzet, a single item's stock value, or a single salesman's achievement is incomplete without knowing whether it is normal, unusual, or extreme compared with peers.

### 4.2 Question Before KPI

KPI availability does not justify visualization. A KPI belongs in M32R only when it helps answer a specific management question.

### 4.3 Investigation Before Reporting

Reports remain the evidence layer. M32R is not a report replacement. Its role is to guide investigation before management drills into detailed evidence.

### 4.4 Evidence Before Conclusion

The workspace may guide management toward a conclusion, but every conclusion must be traceable to KPI values, history, relationships, attention signals, or reports.

### 4.5 Visualization Supports Decision

Visualizations are useful only when they improve a management decision: collect, restrict credit, replenish, stop purchase, clear inventory, coach salesman, negotiate with principal, or investigate further.

### 4.6 Charts Are Navigation, Not Destination

Each visualization should naturally answer "what should I inspect next?" A chart that ends the investigation without giving a path to explanation or evidence is incomplete.

### 4.7 Every Visualization Answers One Primary Question

Each visualization must have one dominant job. If one chart tries to answer too many questions, it becomes a dashboard collage rather than an investigation tool.

### 4.8 Remove Redundant Visualizations

Two visualizations may use the same KPIs only if they answer different questions. For example, Population Map and Ranking both compare entities, but Population Map shows risk/opportunity tradeoff while Ranking shows exact ordinal position.

### 4.9 Business Terminology Over Technical Terminology

Management should not see "XY Scatter," "Histogram," or "Radar" as the product language. Product terms should describe the business purpose: Risk Map, Health Map, Performance Map, Exposure Map, Peer Position, Business Drivers, Performance Signature.

### 4.10 Presets Before Customization

Management needs trusted investigation questions, not blank chart configuration. Axis presets are product decisions. Custom selection can exist later for advanced users, but it must not be the default mental model.

### 4.11 Comparison Preserves Context

Selected entities should usually appear inside one shared visualization with population context retained. Comparison without population context risks over-interpreting differences between only two entities.

### 4.12 Progression Matters

M32R should feel like investigation movement:

```text
Discover -> Compare -> Understand -> Validate -> Explain -> Act
```

It should not feel like a collection of independent dashboard panels.

---

## 5. Business Terminology Strategy

### 5.1 Internal vs Business-Facing Vocabulary

The product should distinguish implementation vocabulary from management vocabulary.

| Internal term | Business-facing term | Reason |
| --- | --- | --- |
| XY Scatter | Population Map / Portfolio Map | Describes population investigation, not chart geometry |
| Customer scatter | Customer Risk Map | Names the management risk question |
| Item scatter | Inventory Health Map | Names the inventory health question |
| Salesman scatter | Sales Performance Map | Names the sales coaching question |
| Supplier scatter | Purchase Exposure Map | Names the purchasing exposure question |
| Histogram / box plot | Peer Position | Describes where an entity sits among peers |
| Ranking chart | Position History | Emphasizes rank movement, not just Top-N list |
| Relationship table | Business Drivers | Emphasizes explanation of behavior |
| Radar | Performance Signature | Keeps existing product language but de-emphasizes chart mechanics |
| Heatmap | Population Review Board | Positions it for review meetings |
| Timeline | Business Event Timeline | Separates events from metric trends |

### 5.2 Recommended Naming Approach

Use entity-specific names where the business question is clear:

- **Customer Risk Map** for customer sales vs receivable / risk investigation.
- **Inventory Health Map** for item stock value vs movement / risk investigation.
- **Sales Performance Map** for salesman achievement, omzet, receivable, and activity investigation.
- **Purchase Exposure Map** for supplier purchase, inventory, posting, and dependency investigation.

Use generic names where the same pattern applies across entity types:

- **Population Map** in product documentation.
- **Portfolio Map** when speaking broadly to management.
- **Peer Position** for distribution analysis.
- **Business Drivers** for relationship explanation.
- **Performance Signature** for Radar.

Avoid exposing these as primary management labels:

- XY Scatter
- Histogram
- Box Plot
- Radar Chart
- Sankey
- Network Graph
- Bubble Chart

Technical terminology can remain in architecture and implementation documents.

---

## 6. Investigation Workflow

M32R should be organized around a natural investigation process, not chart categories.

### 6.1 End-to-End Workflow

```text
1. Discover
   Find abnormal entities in the population.

2. Frame the Question
   Understand which management question is being investigated.

3. Compare
   Compare selected entities against peers and each other.

4. Inspect Current Facts
   Confirm the headline KPI facts for selected entities.

5. Check Peer Position
   Determine whether the selected entity is normal, unusual, or extreme for a single important KPI.

6. Understand Trajectory
   Determine whether the situation is temporary, persistent, improving, or worsening.

7. Check Position History
   Determine whether the entity's rank changed meaningfully.

8. Review Attention Persistence
   Determine whether management signals are new, recurring, resolved, or chronic.

9. Explain Drivers
   Identify related customers, items, salesmen, suppliers, or portfolios that explain the situation.

10. Validate Evidence
    Drill into reports or source evidence before concluding.

11. Decide Operational Follow-Up
    Leave with a management action candidate or further investigation target.
```

### 6.2 What Management Naturally Does Next

| Investigation stage | Primary question | Product capability | Natural next step |
| --- | --- | --- | --- |
| Discover | Which entities deserve attention? | Population Map | Select an outlier or compare candidates |
| Frame the Question | What kind of situation is this? | Preset business maps | Choose the relevant map: risk, health, performance, exposure |
| Compare | Is this entity meaningfully different from peers? | Shared Population Map with selected highlights | Inspect KPI Summary or Peer Position |
| Inspect Current Facts | What are the current numbers? | KPI Summary | Decide which KPI needs deeper context |
| Check Peer Position | Is this KPI normal or extreme? | Peer Position / Distribution | If extreme, check Trend or Ranking History |
| Understand Trajectory | Is the issue temporary or structural? | Trend / Trajectory | Check Attention History for persistence |
| Check Position History | Is rank movement meaningful? | Ranking / Position History | Compare with peer movement or inspect drivers |
| Review Attention Persistence | Is this signal new, recurring, or unresolved? | Attention History | Explain through Business Drivers |
| Explain Drivers | What related entities explain this? | Relationship Drivers | Navigate to related entity or validate report |
| Validate Evidence | What proves the conclusion? | Evidence Report | Move to operational follow-up outside M32R |
| Act | What should management do? | Future Decision Support | Collection, credit, purchase, clearance, visit, negotiation |

### 6.3 Investigation Flow by Example

Customer example:

```text
Customer Risk Map shows Customer A in high sales / high outstanding quadrant
  -> KPI Summary confirms high MTD omzet and high open balance
  -> Peer Position shows open balance is extreme among same wilayah customers
  -> Trend shows outstanding grew for three months
  -> Attention History shows overdue signal persisted
  -> Business Drivers show top purchased principals and assigned salesman
  -> Piutang evidence report validates invoice-level exposure
  -> Management considers collection escalation or credit review
```

Item example:

```text
Inventory Health Map shows Item X in high inventory value / high days of supply zone
  -> KPI Summary confirms inventory value and days of supply
  -> Peer Position shows item is abnormal within its category
  -> Trend shows inventory value rising while movement stays weak
  -> Attention History shows Slow Moving became Dead Stock
  -> Business Drivers show primary supplier and top customers
  -> Inventory evidence report validates stock position
  -> Management considers purchase stop, clearance, or supplier negotiation
```

Salesman example:

```text
Sales Performance Map shows Salesman B above target but high outstanding
  -> KPI Summary confirms achievement and open balance
  -> Peer Position shows open balance is extreme among active salesmen
  -> Trend shows outstanding rising faster than omzet
  -> Attention History shows High Piutang Exposure persists
  -> Business Drivers show top overdue customers
  -> Piutang report validates exposure
  -> Management considers collection coaching
```

Supplier example:

```text
Purchase Exposure Map shows Principal Z high purchase and high inventory value
  -> KPI Summary confirms MTD purchase and attributed inventory
  -> Peer Position shows inventory concentration is extreme
  -> Trend shows purchase growth without matching sales-out penetration
  -> Attention History shows Compound Dependency or At-Risk Exposure
  -> Business Drivers show top items and customers
  -> Purchasing and inventory reports validate exposure
  -> Management considers purchase planning or supplier negotiation
```

---

## 7. Visualization Hierarchy

Not every visualization is equally important. M32R should classify each visualization by investigation role.

### 7.1 Hierarchy Overview

| Layer | Product purpose | Capabilities |
| --- | --- | --- |
| Primary Investigation | Start and steer investigation | Population Map, Peer Position |
| Supporting Evidence | Confirm facts and validate conclusions | KPI Summary, Evidence Reports |
| Supporting Context | Explain history, persistence, and drivers | Trend, Position History, Attention History, Business Drivers |
| Executive Summary | Compress one selected entity for quick review | Performance Signature |
| Future Capability | Extend M32R toward meeting review and decision support | Population Review Board, Business Event Timeline, Decision Support |

### 7.2 Primary Investigation

Primary Investigation capabilities are the surfaces management uses to discover and prioritize entities.

| Capability | Business-facing name | Primary question | Recommendation |
| --- | --- | --- | --- |
| Population Map | Customer Risk Map, Inventory Health Map, Sales Performance Map, Purchase Exposure Map | Which entities deserve attention? | **Core M32R** |
| Peer Position | Peer Position | Is this entity normal, unusual, or extreme for one KPI? | **Core M32R** |

### 7.3 Supporting Evidence

Supporting Evidence capabilities confirm the facts behind the investigation.

| Capability | Business-facing name | Primary question | Recommendation |
| --- | --- | --- | --- |
| KPI Summary | Current Facts | What are the numbers right now? | **Core M32R** |
| Evidence Reports | Evidence | What source detail proves this? | **Core M32R as validation path** |

### 7.4 Supporting Context

Supporting Context capabilities explain why an entity deserves attention and whether the issue is persistent.

| Capability | Business-facing name | Primary question | Recommendation |
| --- | --- | --- | --- |
| Trend | Trajectory | Is this improving, deteriorating, temporary, or persistent? | **Core M32R** |
| Ranking History | Position History | Did the entity's rank change meaningfully? | **Core M32R, narrow purpose** |
| Attention History | Signal History | Are management signals new, recurring, resolved, or chronic? | **Core M32R** |
| Relationship | Business Drivers | Which related entities explain this situation? | **Core M32R** |

### 7.5 Executive Summary

Executive Summary capabilities compress the state of one selected entity. They do not lead investigation.

| Capability | Business-facing name | Primary question | Recommendation |
| --- | --- | --- | --- |
| Radar | Performance Signature | Is this selected entity balanced or lopsided? | **Keep secondary** |

### 7.6 Future Capability

Future capabilities extend the investigation workspace but should not block M32R.

| Capability | Business-facing name | Primary question | Recommendation |
| --- | --- | --- | --- |
| Heatmap | Population Review Board | How do dozens of selected entities compare across many KPIs? | **Future milestone** |
| Timeline | Business Event Timeline | What business events explain the sequence of change? | **Future milestone** |
| Recommended Actions | Decision Support | What should management do next? | **Future milestone** |

---

## 8. Population Map - Primary Investigation Workspace

### 8.1 Product Definition

The Population Map is the primary M32R investigation workspace. Internally it may use XY Scatter technology, but product-wise it is not "a scatter chart." It is the workspace where management sees the population, discovers meaningful entities, compares selected entities, and navigates into evidence.

### 8.2 Responsibilities

The Population Map is responsible for:

- Discovering outliers.
- Showing the full eligible population.
- Preserving peer context while selected entities are highlighted.
- Comparing selected entities against each other and against the population.
- Separating important quadrants or zones.
- Serving as the investigation entry point.
- Navigating into KPI facts, peer position, history, drivers, and evidence.

### 8.3 Why This Is Different From Traditional Dashboards

Traditional dashboards usually start with aggregate cards and Top-N lists:

```text
Company metric -> dashboard section -> row click -> report
```

The Population Map starts with the entity population:

```text
Population shape -> outlier/entity selection -> investigation path -> evidence
```

This difference matters because Top-N dashboards hide many management-relevant entities:

- Customer ranked #37 may be the most worrying customer in one wilayah.
- Item outside Top 10 inventory value may still be abnormal within its category.
- Salesman with moderate omzet may carry extreme collection risk.
- Supplier outside Top 10 purchase may hold abnormal at-risk inventory.

The Population Map makes those mid-tier and peer-relative situations visible.

### 8.4 Default Business Maps

| Entity | Business-facing map | Default question | Default KPI relationship |
| --- | --- | --- | --- |
| Customer | Customer Risk Map | Which customers combine sales value and receivable risk? | MTD omzet vs open balance |
| Customer | Customer Growth Risk Map | Which growing customers are becoming risky? | Omzet growth vs collection / overdue risk |
| Item | Inventory Health Map | Which items tie up capital without enough movement? | Inventory value vs days of supply |
| Item | Replenishment Risk Map | Which items need purchase attention? | Recommended purchase qty vs stock-out / days of supply |
| Salesman | Sales Performance Map | Which reps sell well but carry receivable risk? | Achievement % vs open balance |
| Salesman | Field Effectiveness Map | Which reps need field execution coaching? | Visit execution % vs effective call rate |
| Supplier | Purchase Exposure Map | Which principals create purchase and inventory exposure? | MTD purchase vs inventory value |
| Supplier | Purchasing Discipline Map | Which principals have process risk? | Posted % vs backlog / attention risk |

### 8.5 Population Scale Feasibility

M32R can show the full eligible population at current BTR scale.

| Entity | Approximate count | Full population map feasibility | Product implication |
| --- | ---: | --- | --- |
| Customer | 2,100 | High | Show full population with selective labels |
| Item | 3,000 | High | Show full population, default peer/category filter important |
| Salesman | 50 | Very high | Most or all points can be inspectable |
| Supplier | 200 | Very high | Full population context is highly usable |

The main risk is not data volume. The main risk is cognitive overload. Labels, colors, bubbles, and filters must be disciplined.

### 8.6 Required Product Behaviors

| Behavior | Product decision |
| --- | --- |
| Full population context | Required by default |
| Selected entity highlighting | Required |
| Axis presets | Required |
| Custom axis selection | Advanced / optional, not default |
| Bubble size | Optional per preset; never always-on |
| Color encoding | Allowed only for meaningful business categories such as risk, movement class, attention state, wilayah, category |
| Labels | Selected, hovered, and major outliers only |
| Tooltips | Must provide identity, axis values, rank/percentile context, and attention state |
| Filtering | Required for peer fairness: wilayah, category, supplier, active status, attention/risk |
| Zooming | Useful for customer/item density, but not a substitute for filtering |

---

## 9. Peer Position - Core Distribution Analysis

### 9.1 Product Decision

Distribution analysis should be promoted to **Core Investigation** under the business-facing name **Peer Position**.

### 9.2 Unique Management Question

Peer Position answers a question no other visualization answers as cleanly:

```text
For this one KPI, is the selected entity normal, unusual, or extreme within the relevant population?
```

Comparison with other visualizations:

| Capability | What it answers well | What it cannot answer as cleanly as Peer Position |
| --- | --- | --- |
| Population Map | Two-KPI tradeoff and outlier discovery | Single-KPI normality and percentile clarity |
| Ranking | Exact order | Distribution shape and distance from normal population |
| Trend | Change over time | Whether current value is unusual among peers |
| Radar / Performance Signature | Balanced multi-dimensional summary | Raw distribution context for one KPI |
| KPI Summary | Current value | Whether the value is normal or extreme |

### 9.3 Why Peer Position Deserves Promotion

Peer Position is critical when management asks:

- Is this customer outstanding balance normal for this wilayah?
- Is this item's days of supply extreme within its category?
- Is this salesman's open balance unusual among active salesmen?
- Is this supplier's inventory exposure extreme among active principals?

Scatter may show the entity in context, but Peer Position isolates one KPI and makes abnormality easier to explain.

### 9.4 Recommended Product Forms

| Internal form | Business-facing role | Recommendation |
| --- | --- | --- |
| Histogram with selected marker | Show distribution and selected entity position | **Default** |
| Box plot with selected marker | Compact expert summary of spread and outliers | Secondary / optional |
| Distribution curve | Smoothed context | Use carefully; avoid false precision |

### 9.5 Default Peer Position KPIs

| Entity | Default Peer Position KPIs |
| --- | --- |
| Customer | Open balance, MTD omzet, overdue exposure, MoM omzet growth |
| Item | Inventory value, days of supply, qty on hand, distinct buyer count |
| Salesman | Achievement %, open balance, MTD omzet, active customer count |
| Supplier | MTD purchase, inventory value, posted %, catalog penetration |

---

## 10. Supporting Context and Evidence

### 10.1 KPI Summary - Current Facts

KPI Summary remains core, but its product role changes. It is not the main experience; it is the fact panel that confirms what the investigation selected.

Primary question:

```text
What are the current facts for this selected entity?
```

KPI Summary should support investigation by keeping values tied to catalog-backed meanings and evidence routes.

### 10.2 Trend - Trajectory

Trend remains core because it answers:

```text
Is the situation improving, worsening, temporary, seasonal, or persistent?
```

Trend is especially important after a Population Map or Peer Position identifies an entity as abnormal. It prevents management from overreacting to a one-period anomaly.

Default trend metrics:

| Entity | Default trajectory metrics |
| --- | --- |
| Customer | MTD omzet, open balance, attention count |
| Item | Inventory value, days of supply, recommended purchase qty or movement risk |
| Salesman | MTD omzet, achievement %, open balance |
| Supplier | MTD purchase, posted %, inventory value |

### 10.3 Ranking History - Position History

Ranking should remain, but under a narrower role: **Position History**.

Primary question:

```text
Did this entity's position among peers change meaningfully?
```

Ranking should not compete with Population Map as discovery surface. It should answer exact rank and rank movement.

### 10.4 Attention History - Signal History

Attention History remains core because it answers:

```text
Is this management signal new, recurring, resolved, or chronic?
```

This is essential for deciding whether management should escalate or observe.

### 10.5 Relationship Drivers

Relationship presentation should become **Ranked Table plus Embedded Horizontal Bars**.

Reason:

- Ranked tables preserve precision: code, name, value, rank, relationship type.
- Embedded horizontal bars improve scanning and visual comparison.
- This combination is better than a pure chart because relationships are evidence-oriented.
- It is better than a plain table because magnitude becomes easier to read.

Recommended relationship presentation:

| Primary entity | Relationship driver blocks |
| --- | --- |
| Customer | Assigned Salesman, Top Purchased Items, Top Principals |
| Item | Primary Supplier, Top Customers, Top Salesmen |
| Salesman | Managed Customers, Top Customers by Omzet/Piutang, Top Principals, Top Items |
| Supplier | Top Items, Top Customers, Top Salesmen |

Rejected default alternatives:

| Alternative | Reason rejected |
| --- | --- |
| Sankey | Implies flow conservation and becomes label-heavy |
| Network graph | Visually attractive but weak for precise management evidence |
| Pure horizontal bar chart | Loses tabular evidence detail |
| Plain table only | Precise but harder to scan visually |

### 10.6 Evidence Reports

Reports remain the final validation layer.

Primary question:

```text
What source detail proves the investigation conclusion?
```

M32R should guide management to evidence, not replace evidence.

---

## 11. Executive Summary Capability

### 11.1 Performance Signature

Radar should remain as **Performance Signature**, but it should be demoted to Executive Summary.

Primary question:

```text
Is this selected entity balanced or lopsided across major dimensions?
```

Performance Signature is useful when management has already selected an entity and wants a quick summary. It is not strong for:

- discovering outliers,
- comparing many entities,
- explaining exact KPI values,
- preserving full population context.

Product decision:

| Question | Decision |
| --- | --- |
| Should Radar remain? | Yes |
| Should Radar be the signature visualization? | No |
| Should Product call it Radar? | No, use Performance Signature |
| Should it lead the investigation flow? | No |
| Should it summarize selected entities? | Yes |

---

## 12. Management Question Catalog

### 12.1 Customer

| Business question | Importance | Decision supported | Primary capability |
| --- | --- | --- | --- |
| Which customers generate high sales but also high outstanding balance? | Very high | Credit review, collection escalation, credit limit restriction | Customer Risk Map |
| Which customers are growing but collection quality is weakening? | Very high | Early intervention before receivable risk becomes chronic | Customer Growth Risk Map |
| Which customers are declining after intervention or route changes? | High | Sales coaching, customer retention, route reassignment | Trajectory |
| Which customers are dormant despite previously meaningful contribution? | High | Reactivation campaign, account review | Population Map + Signal History |
| Which customers are outliers within the same wilayah or classification? | High | Peer benchmarking, fairness of sales expectations | Peer Position |
| Which customers depend on a small number of items or principals? | Medium | Assortment risk, upsell planning, supplier exposure | Business Drivers |
| Which customer attention signals persist month after month? | High | Escalation priority, management follow-up | Signal History |

### 12.2 Item

| Business question | Importance | Decision supported | Primary capability |
| --- | --- | --- | --- |
| Which items tie up inventory value but have weak or no sales movement? | Very high | Clearance, purchase stop, delist review | Inventory Health Map |
| Which items sell well but are at risk of stock-out? | Very high | Replenishment priority | Replenishment Risk Map |
| Which items have inventory value rising without matching sales velocity? | High | Purchase discipline, stock transfer, supplier negotiation | Population Map + Trajectory |
| Which items are abnormal within the same category? | High | Category review, substitution, SKU rationalization | Peer Position |
| Which items have shrinking customer reach? | High | Sales push, assortment correction | Trajectory + Business Drivers |
| Which items are driven by a small number of customers or salesmen? | Medium | Demand concentration risk | Business Drivers |
| Which item risk signals persist over time? | High | Inventory intervention tracking | Signal History |

### 12.3 Salesman

| Business question | Importance | Decision supported | Primary capability |
| --- | --- | --- | --- |
| Which salesmen achieve sales target while carrying high receivable exposure? | Very high | Coaching balance between selling and collection | Sales Performance Map |
| Which salesmen have low achievement but high customer opportunity? | High | Target review, route support, customer development | Population Map + Peer Position |
| Which salesmen have weak visit execution and weak effective call? | Very high | Field discipline intervention | Field Effectiveness Map |
| Which salesmen improved sustainably versus one-month spikes? | High | Incentive review, coaching validation | Trajectory |
| Which salesmen are overly dependent on a few customers or principals? | Medium | Portfolio diversification | Business Drivers |
| Which salesmen are outliers versus the sales team population? | High | Best-practice replication or performance escalation | Peer Position |
| Which salesman attention signals remain unresolved? | High | Management accountability | Signal History |

### 12.4 Supplier / Principal

| Business question | Importance | Decision supported | Primary capability |
| --- | --- | --- | --- |
| Which principals drive high purchase value but also high inventory exposure? | Very high | Purchase planning, supplier negotiation, dependency review | Purchase Exposure Map |
| Which principals have high inventory value but weak sales-out? | Very high | Purchase stop, promotion, return/clearance negotiation | Purchase Exposure Map + Trajectory |
| Which principals have poor posting discipline or qualified backlog? | High | Purchasing process intervention | Purchasing Discipline Map |
| Which principals dominate company purchase or inventory concentration? | High | Dependency management | Peer Position + Position History |
| Which principals are growing or declining in purchase activity? | Medium | Negotiation and assortment strategy | Trajectory |
| Which principals are abnormal compared with other active principals? | High | Portfolio balance review | Peer Position |
| Which principal attention signals persist? | High | Management follow-up with purchasing | Signal History |

---

## 13. Existing KPI Availability

M32R should reuse catalog-backed KPI semantics. Product must not invent parallel metric definitions inside the investigation workspace.

### 13.1 Customer

| KPI area | Availability | Product use |
| --- | --- | --- |
| Sales value | Strong | Customer Risk Map, Current Facts, Trajectory, Position History |
| Outstanding / piutang | Strong | Customer Risk Map, Peer Position, Signal History |
| Collection / overdue risk | Strong to moderate | Customer Growth Risk Map, Attention context |
| Growth | Available through monthly history | Trajectory, growth-risk investigation |
| Portfolio / priority | Strong | Performance Signature, Current Facts |
| Attention signals | Strong | Signal History |
| Relationships | Strong | Business Drivers |

### 13.2 Item

| KPI area | Availability | Product use |
| --- | --- | --- |
| Inventory value | Strong | Inventory Health Map, Peer Position, Trajectory |
| Days of supply / velocity risk | Strong | Inventory Health Map, Replenishment Risk Map |
| Recommended purchase qty | Strong | Replenishment Risk Map |
| Qty on hand | Strong | Current Facts, Peer Position |
| Customer reach | Moderate | Market reach investigation, Business Drivers |
| Movement / attention signals | Strong | Signal History |
| Relationships | Moderate to strong | Business Drivers |

### 13.3 Salesman

| KPI area | Availability | Product use |
| --- | --- | --- |
| MTD omzet | Strong | Sales Performance Map, Trajectory |
| Achievement % | Strong | Sales Performance Map, Peer Position, Position History |
| Open balance / piutang | Strong | Sales Performance Map, Peer Position, Signal History |
| Customer breadth | Strong | Current Facts, Performance Signature |
| Visit execution / effective call | Partial | High-value future Field Effectiveness Map |
| Attention signals | Strong | Signal History |
| Relationships | Strong | Business Drivers |

### 13.4 Supplier / Principal

| KPI area | Availability | Product use |
| --- | --- | --- |
| MTD purchase | Strong | Purchase Exposure Map, Trajectory, Position History |
| MTD invoice count | Strong | Current Facts, Trajectory |
| Posted % | Strong | Purchasing Discipline Map, Peer Position |
| Inventory value | Strong | Purchase Exposure Map, Peer Position |
| Active SKU count | Strong | Current Facts, Performance Signature |
| Catalog penetration | Moderate | Purchase alignment investigation |
| Attention signals | Strong | Signal History |
| Relationships | Strong | Business Drivers |

---

## 14. Entity Investigation Blueprints

### 14.1 Customer Investigation Blueprint

| Stage | Capability | Customer-specific product role |
| --- | --- | --- |
| Discover | Customer Risk Map | Find customers combining sales value and receivable exposure |
| Compare | Shared Population Map | Compare selected customers against wilayah/classification peers |
| Inspect | Current Facts | Confirm omzet, open balance, overdue/risk facts |
| Peer Position | Peer Position | Determine whether open balance or growth is abnormal |
| Trajectory | Trend | Check if sales decline or receivable growth is persistent |
| Position | Position History | Check whether customer rank changed materially |
| Signals | Signal History | Check overdue, dormant, plafond, suspended-sales persistence |
| Explain | Business Drivers | Assigned salesman, top items, top principals |
| Validate | Evidence Reports | Sales and piutang report evidence |
| Act | Future Decision Support | Collection escalation, credit review, retention, sales visit |

### 14.2 Item Investigation Blueprint

| Stage | Capability | Item-specific product role |
| --- | --- | --- |
| Discover | Inventory Health Map | Find high-value slow-moving or dead capital items |
| Compare | Shared Population Map | Compare items within category or supplier context |
| Inspect | Current Facts | Confirm inventory value, qty, days of supply, purchase recommendation |
| Peer Position | Peer Position | Determine whether value or days of supply is abnormal in category |
| Trajectory | Trend | Check whether inventory is normalizing or worsening |
| Position | Position History | Check rank movement in inventory value or velocity risk |
| Signals | Signal History | Track Never Sold, Slow Moving, Dead Stock, Overstock, Stock-Out |
| Explain | Business Drivers | Primary supplier, top customers, top salesmen |
| Validate | Evidence Reports | Inventory report evidence |
| Act | Future Decision Support | Purchase stop, replenishment, promotion, clearance |

### 14.3 Salesman Investigation Blueprint

| Stage | Capability | Salesman-specific product role |
| --- | --- | --- |
| Discover | Sales Performance Map | Find reps with achievement/omzet and receivable tradeoffs |
| Compare | Shared Population Map | Compare reps against active team population |
| Inspect | Current Facts | Confirm omzet, achievement, open balance, customer count |
| Peer Position | Peer Position | Determine whether achievement or exposure is abnormal |
| Trajectory | Trend | Check if performance is improving sustainably |
| Position | Position History | Check rank movement in omzet, achievement, piutang |
| Signals | Signal History | Track below target, high exposure, concentration, dormant portfolio |
| Explain | Business Drivers | Managed customers, top customers, principals, items |
| Validate | Evidence Reports | Sales and piutang report evidence |
| Act | Future Decision Support | Coaching, visit plan, collection focus, route review |

### 14.4 Supplier / Principal Investigation Blueprint

| Stage | Capability | Supplier-specific product role |
| --- | --- | --- |
| Discover | Purchase Exposure Map | Find principals with purchase/inventory dependency |
| Compare | Shared Population Map | Compare active principals against supplier population |
| Inspect | Current Facts | Confirm purchase, invoice count, posted %, inventory value |
| Peer Position | Peer Position | Determine whether purchase or inventory exposure is extreme |
| Trajectory | Trend | Check whether purchase growth aligns with sales-out / inventory |
| Position | Position History | Check purchase and inventory exposure rank |
| Signals | Signal History | Track backlog, concentration, at-risk exposure, compound dependency |
| Explain | Business Drivers | Top items, top customers, top salesmen |
| Validate | Evidence Reports | Purchasing and inventory report evidence |
| Act | Future Decision Support | Purchase planning, negotiation, purchase stop, promotion support |

---

## 15. Comparison Mode

### 15.1 Product Decision

Comparison should happen in a shared context by default.

| Approach | Verdict | Reason |
| --- | --- | --- |
| Separate chart per selected entity | Secondary fallback | Useful only when trend overlays become unreadable |
| Shared visualization with population context | **Default** | Preserves peer context and enables direct comparison |

### 15.2 Comparison Rules by Capability

| Capability | Comparison behavior |
| --- | --- |
| Population Map | Show full population in background and selected entities highlighted together |
| Peer Position | Show selected marker; allow multiple selected markers for small comparisons |
| Current Facts | Align selected entities by the same KPI definitions |
| Trend | Overlay selected entities for one KPI; use separated views only when readability requires |
| Position History | Show selected rank paths for the same rank metric |
| Signal History | Compare signal persistence per selected entity |
| Business Drivers | Keep relationship blocks per entity because drivers are entity-specific |
| Performance Signature | Overlay only for small selected sets; secondary, not primary |

### 15.3 Comparison Limit

For profile-style investigation, comparison should focus on 2-4 selected entities. When the review set grows beyond that, the product need changes from **Entity Investigation** to **Population Review**, which belongs to the future Heatmap / Population Review Board capability.

---

## 16. Visualization Matrix by Investigation Question

Legend:

- **Best** = primary capability for the question
- **Support** = useful supporting context
- **Weak** = possible but not recommended
- **No** = does not answer the question well

| Management question | Population Map | Peer Position | Current Facts | Trajectory | Position History | Signal History | Business Drivers | Performance Signature | Population Review Board |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Which entities deserve attention? | **Best** | Support | Weak | Weak | Support | Support | Weak | Weak | Support |
| Is this entity normal or extreme for one KPI? | Support | **Best** | Support | No | Support | Weak | No | Weak | Support |
| Which entities are high value and high risk? | **Best** | Support | Support | Weak | Support | Support | Weak | Weak | Support |
| Is this entity improving or declining? | Support | Weak | Support | **Best** | Support | Support | Weak | Weak | Support |
| Is decline temporary or structural? | Weak | No | Weak | **Best** | Support | Support | Support | Weak | No |
| Did intervention work? | Support | Support | Support | **Best** | Support | Support | Support | Weak | Support |
| What exact rank is this entity? | Support | Support | Support | Weak | **Best** | Weak | No | No | Weak |
| Which signals are chronic? | Support | Weak | Support | Support | Weak | **Best** | Support | Support | Support |
| What explains this entity's behavior? | Weak | No | Support | Support | Weak | Support | **Best** | Weak | Support |
| Is this entity balanced across dimensions? | Weak | Weak | Support | Weak | Weak | Support | Weak | **Best** | Support |
| Which dozens of selected entities need review? | Support | Support | Support | Support | Support | Support | Support | Weak | **Best** |

---

## 17. Re-Evaluated Candidate Capabilities

### 17.1 Population Map

| Evaluation item | Product decision |
| --- | --- |
| Layer | Primary Investigation |
| Business question | Which entities deserve management attention? |
| Required KPIs | Two numeric KPIs per preset, optional size KPI, optional business category/risk encoding |
| Advantages | Full population context, outlier discovery, selected-entity comparison, investigation entry point |
| Disadvantages | Requires disciplined labels, filters, and business presets |
| Scalability | Strong at BTR scale: 2,100 customers, 3,000 items, 50 salesmen, 200 suppliers |
| Recommendation | **Core M32R and signature capability** |

### 17.2 Peer Position

| Evaluation item | Product decision |
| --- | --- |
| Layer | Primary Investigation |
| Business question | Is this selected entity normal, unusual, or extreme for one KPI? |
| Required KPIs | One comparable numeric KPI and peer group |
| Advantages | Unique normality / percentile context; easier to explain than multi-axis maps |
| Disadvantages | One-dimensional; does not show tradeoff between two KPIs |
| Scalability | Strong; thousands of entities aggregate cleanly |
| Recommendation | **Promote to Core M32R** |

### 17.3 Trend / Trajectory

| Evaluation item | Product decision |
| --- | --- |
| Layer | Supporting Context |
| Business question | Is the situation improving, worsening, temporary, or persistent? |
| Required KPIs | Monthly history for selected metrics |
| Advantages | Best for intervention validation and persistence |
| Disadvantages | Weak as initial discovery surface |
| Scalability | Strong because it shows selected entities only |
| Recommendation | **Core supporting context** |

### 17.4 Position History

| Evaluation item | Product decision |
| --- | --- |
| Layer | Supporting Context |
| Business question | Did the entity's peer position change meaningfully? |
| Required KPIs | Rank-eligible metrics |
| Advantages | Exact rank and rank movement |
| Disadvantages | Single-metric, ordinal, not tradeoff-based |
| Scalability | Good for selected entities |
| Recommendation | **Keep, narrow role** |

### 17.5 Business Drivers

| Evaluation item | Product decision |
| --- | --- |
| Layer | Supporting Context |
| Business question | Which related entities explain this situation? |
| Required KPIs | Top-N relationship metric, target entity identity, rank/value |
| Advantages | Investigation explanation; evidence navigation |
| Disadvantages | Top-N only; does not show long-tail relationship structure |
| Scalability | Strong at Top 5-10 per block |
| Recommendation | **Ranked Table plus Embedded Horizontal Bars** |

### 17.6 Performance Signature

| Evaluation item | Product decision |
| --- | --- |
| Layer | Executive Summary |
| Business question | Is this selected entity balanced or lopsided? |
| Required KPIs | 4-6 normalized dimensions |
| Advantages | Compact executive summary |
| Disadvantages | Weak discovery, weak precision, weak multi-entity comparison |
| Scalability | Technically strong, cognitively limited |
| Recommendation | **Keep secondary, not signature** |

### 17.7 Population Review Board

| Evaluation item | Product decision |
| --- | --- |
| Layer | Future Capability |
| Business question | How do dozens of selected entities compare across many KPIs? |
| Required KPIs | Standardized KPI set, banding or normalization rules |
| Advantages | Strong for weekly review, sales review, portfolio review, executive review |
| Disadvantages | Can become a colored spreadsheet if not constrained |
| Scalability | Strong for dozens of selected entities; not for full population |
| Recommendation | **Future milestone, not M32R default** |

### 17.8 Business Event Timeline

| Evaluation item | Product decision |
| --- | --- |
| Layer | Future Capability |
| Business question | What sequence of business events explains the change? |
| Required data | Business events, interventions, status changes, attention events, major KPI inflection markers |
| Advantages | Explains causality better than trend lines alone |
| Disadvantages | Requires event semantics beyond KPI history |
| Scalability | Best for one selected entity at a time |
| Recommendation | **Future investigation capability** |

### 17.9 Rejected Defaults

| Candidate | Reason rejected as default |
| --- | --- |
| Sankey | Too complex for routine relationship explanation; implies flow semantics |
| Network Graph | High cognitive load; weak evidence precision |
| Treemap | Better for aggregate concentration dashboards than investigation workspace |
| Waterfall | Requires decomposition questions not yet established |
| Parallel Coordinates | Expert tool; too complex for operational management |
| Always-on Bubble Chart | Useful as Population Map option, but too much encoding for default |

---

## 18. Future Capability - Population Review Board

### 18.1 Product Decision

Heatmap should be recognized as a future milestone under the product name **Population Review Board**.

It should not be an Entity Profile visualization. It is a meeting and review capability.

### 18.2 Best-Fit Scenarios

| Review scenario | Why Population Review Board fits |
| --- | --- |
| Weekly Executive Review | Compare selected risk/opportunity entities across several KPIs |
| Sales Review Meeting | Compare salesmen or customers across sales, collection, activity, risk |
| Portfolio Review Meeting | Review dozens of customers or items selected from Population Map |
| Inventory Review | Compare slow-moving or stock-out-risk items across inventory KPIs |
| Supplier Review | Compare principals across purchase, inventory, posting, sales-out |

### 18.3 Why This Is Future, Not M32R Core

M32R focuses on discovering and investigating entities. Population Review Board focuses on reviewing a curated set of entities after selection.

Recommended sequence:

```text
M32R Population Investigation
  -> selected entities saved or carried into review context
  -> future Population Review Board compares dozens of selected entities
```

---

## 19. Future Capability - Business Event Timeline

### 19.1 Product Decision

Timeline deserves recognition as a future Investigation capability. It is not the same as Trend.

Trend answers:

```text
How did a KPI value move over time?
```

Business Event Timeline answers:

```text
What business events happened, in what order, and how might they explain the KPI movement?
```

### 19.2 Examples

Customer:

```text
Sales decline
  -> Credit limit changed
  -> Collection visit
  -> Payment recovery
  -> Sales normalized
```

Item:

```text
Promotion
  -> Sales spike
  -> Stock-out warning
  -> Replenishment
  -> Inventory normalized
```

Salesman:

```text
Route change
  -> Visit execution drops
  -> Effective call falls
  -> Coaching session
  -> Achievement recovers
```

Supplier:

```text
Large purchase intake
  -> Posting backlog
  -> Inventory exposure rises
  -> Promotion support
  -> Sales-out improves
```

### 19.3 Product Role

Timeline should become the narrative layer after management has already identified an entity worth investigating.

It belongs after:

- Population Map
- Peer Position
- Trajectory
- Signal History

It should not replace those capabilities.

---

## 20. Future Evolution - Decision Support

### 20.1 Product Boundary

M32R ends at Population Investigation. It should prepare for Decision Support, but it must not design AI or automated recommendations.

Decision Support is a future maturity level where the product helps answer:

```text
What action should management take?
```

### 20.2 Natural Evolution Path

M32R investigation conclusions can later evolve into action suggestions:

| Investigation conclusion | Future decision-support evolution |
| --- | --- |
| Customer has high sales and extreme outstanding | Suggested collection escalation or credit review |
| Customer is dormant after prior contribution | Suggested sales visit or reactivation |
| Item has high inventory value and high days of supply | Suggested purchase stop or clearance |
| Item is stock-out risk with strong demand | Suggested replenishment |
| Salesman has high achievement and high receivable exposure | Suggested collection coaching |
| Salesman has weak visit execution and low effective call | Suggested field coaching or route review |
| Supplier has high purchase and high inventory exposure | Suggested purchase planning review |
| Supplier has poor posting discipline | Suggested purchasing process follow-up |

### 20.3 Future Decision Support Capabilities

Potential future capabilities:

- Recommended Actions
- Priority Queue
- Suggested Collection
- Suggested Purchase Stop
- Suggested Promotion
- Suggested Sales Visit
- Suggested Inventory Clearance
- Suggested Supplier Negotiation
- Suggested Coaching Focus

These should be built only after M32R establishes reliable investigation evidence and action vocabulary.

### 20.4 Product Rule for Future Recommendations

Future recommendations must be explainable through M32R evidence:

```text
Recommendation
  -> Population position
  -> KPI facts
  -> Peer position
  -> Trend
  -> Signals
  -> Drivers
  -> Evidence report
```

If a recommendation cannot trace back to investigation evidence, it should not be shown.

---

## 21. Product Decisions Resolved by This Document

The Architect should treat these as settled Product decisions:

| Decision | Product resolution |
| --- | --- |
| Milestone name | M32R |
| Product stage | Population Investigation |
| Product form | Entity Analytics Investigation Workspace |
| Primary capability | Population Map |
| Internal technology behind Population Map | XY Scatter-style population visualization |
| Business-facing terminology | Use Risk Map, Health Map, Performance Map, Exposure Map, Peer Position, Business Drivers |
| Radar role | Secondary Executive Summary as Performance Signature |
| Distribution role | Promoted to Core Investigation as Peer Position |
| Relationship presentation | Ranked Table plus Embedded Horizontal Bars |
| Heatmap role | Future Population Review Board milestone |
| Timeline role | Future Business Event Timeline capability |
| Comparison default | Shared visualization with population context |
| Profile comparison limit | 2-4 entities; larger sets belong to future Population Review Board |
| Full population display | Feasible and desirable at current BTR scale |
| Axis presets | Required; custom axes advanced/optional |
| Reports | Evidence validation layer, not replaced by M32R |
| Future Decision Support | Prepared conceptually, not implemented or specified in M32R |

---

## 22. Architect Handover

This document intentionally stops at Product decisions. The Architect should use this as the authoritative product reference and decide how to realize the workspace.

### 22.1 Architecture Decisions That Remain

| Area | Architect decision still required |
| --- | --- |
| Navigation model | How users move from dashboard, alert, search, Population Map, profile, and report |
| Workspace layout | How investigation stages are arranged without turning into a dashboard collage |
| Visualization framework | Which chart/rendering capabilities support Population Map, Peer Position, bars, trajectory, and future review |
| Interaction model | How selection, highlighting, filtering, drill-down, hover, zoom, and comparison behave |
| Filtering strategy | How peer filters, entity filters, active/risk filters, and saved investigation context work |
| Shared comparison model | How 2-4 selected entities are represented across Population Map, Peer Position, Trend, Position History, and Current Facts |
| Performance considerations | How full population rendering remains responsive for customer/item scale |
| Rendering strategy | SVG/canvas/library choices, point density handling, label strategy implementation |
| Component hierarchy | Which reusable components or shells implement the workspace |
| State management | How selected entity, peer group, filters, presets, comparison set, and evidence context are stored |
| Data contract | What responses are needed to support Product behavior without leaking implementation detail |
| Snapshot usage | How existing L0-L5 Entity Analytics data supports M32R and what additions, if any, are needed |
| Accessibility and readability | How maps remain understandable to non-technical management |
| Future extension points | How Population Review Board, Business Event Timeline, and Decision Support can plug in later |

### 22.2 Product Constraints for Architecture

Architecture should preserve these constraints:

- Product language must remain business-facing.
- The Population Map must preserve full population context.
- Scatter-style technology must not force users to configure axes before seeing value.
- Peer Position must be treated as core, not optional decoration.
- Relationship Drivers must preserve table precision and add visual magnitude cues.
- Radar / Performance Signature must not dominate the workspace.
- Future Decision Support must remain evidence-traceable.
- Reports remain the validation layer.
- The design must support all four entity types without per-entity product reinvention.

### 22.3 Suggested Architect Starting Point

The Architect should begin from the investigation flow, not from components:

```text
Discover -> Frame Question -> Compare -> Inspect Current Facts
  -> Check Peer Position -> Understand Trajectory
  -> Check Position History -> Review Signals
  -> Explain Drivers -> Validate Evidence -> Prepare Action
```

Architecture should map data, navigation, and interaction models to this flow.

---

## 23. Final Product Vision

M32R should make Entity Analytics the place where management investigates the business population.

The successful product is not one with the most charts. The successful product is one where management can quickly answer:

```text
Which customers, items, salesmen, and principals deserve attention?
Why?
Compared with whom?
Is the situation abnormal?
Is it getting better or worse?
What explains it?
What evidence proves it?
What should we consider doing next?
```

The Population Map starts the investigation. Peer Position clarifies abnormality. Current Facts confirm the numbers. Trajectory, Position History, and Signal History establish time and persistence. Business Drivers explain why. Evidence Reports validate. Performance Signature summarizes only after the entity is understood.

That is the M32R product vision.

---

## Document Control

| Version | Date | Author | Change |
| --- | --- | --- | --- |
| 1.0 | 2026-06-27 | Analyst | Initial visualization feasibility study |
| 2.0 | 2026-06-27 | Analyst | Reframed as M32R Product Vision and Investigation Workspace; promoted Population Map and Peer Position; added terminology strategy, workflow, hierarchy, future capabilities, Decision Support evolution, and Architect handover |
