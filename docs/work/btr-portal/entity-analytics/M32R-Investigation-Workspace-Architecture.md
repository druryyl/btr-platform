# M32R Investigation Workspace Architecture

**Status:** Architectural Reference — Version 1.0  
**Milestone:** M32R Entity Analytics Investigation Workspace  
**Date:** 2026-06-27  
**Role:** Lead Product Architect  
**Audience:** Future architects, senior implementers, product reviewers  
**Authoritative Input:** M32-Entity-Analytics-Visualization-Feasibility-Study.md (Product Vision v2.0)

> This document is intentionally non-implementational. It defines how the product behaves — its shape, logic, navigation, and structure — without specifying frameworks, component trees, CSS, or rendering strategies. It should be consulted before designing any new screen, interaction, or capability in Entity Analytics.

---

## Architectural Review

The Product Vision is sound. The following architectural note does not contradict it; it clarifies one implication the Product Vision leaves open.

**On the workspace as a single-entity or multi-entity surface.**

The Product Vision states that comparison is limited to 2–4 entities and that Population Review Board is a future capability for larger sets. This is correct. However, the architecture must be designed so that the workspace has two distinct operating modes that share the same surfaces without producing two separate designs:

- **Discovery mode:** No entity selected. The Population Map is the full active surface.
- **Investigation mode:** One or more entities selected. The investigation panels activate progressively.

If this distinction is not explicit in the architecture, implementers may treat both modes as one and produce a workspace that is always partially empty (in discovery mode) or always cluttered (in investigation mode). The architecture below establishes this mode concept as the structural foundation of the workspace.

---

## 1. Workspace Philosophy

### 1.1 Why Investigation Workspace, Not Dashboard

A dashboard presents information for reading. An investigation workspace guides management toward a defensible conclusion.

The difference is structural. A dashboard answer is passive: the user opens it and reads what is there. An investigation workspace is active: the user enters with a question, the workspace helps narrow, compare, contextualise, explain, and validate.

BTR management decisions are rarely about one entity in isolation. Collection escalation requires knowing that a customer's outstanding balance is extreme relative to the same-region peers, not just that the absolute number is high. A purchase stop decision requires knowing that a principal's inventory concentration is growing faster than sales-out across the full active supplier portfolio, not just that one principal has high inventory. An intervention in a salesman's performance requires knowing whether the performance is genuinely anomalous within the active field team, not whether it fell short of an absolute target.

Every meaningful management question is population-relative. The investigation workspace is therefore designed around the population, not around the profile.

### 1.2 Population First

The workspace begins with the full eligible population. Management sees every active entity before selecting any.

This is a deliberate architectural decision with several consequences:

**It eliminates Top-N myopia.** Dashboard Top-10 lists hide the most concerning entity at rank #37. The Population Map does not hide rank #37.

**It anchors every individual fact in peer context.** When management later reads that Customer A has Rp 450M in outstanding, they already know that Customer A is in the top-right cluster of a population of 2,100 customers. The number has meaning.

**It reveals structural patterns.** Management may notice that an entire regional cluster is elevated before they notice any individual entity. Those structural patterns are invisible when the entry point is a profile list.

**It prevents premature investigation closure.** When management enters a profile directly, investigation often ends at the profile because there is no easy return to the population context. When investigation begins at the population, the population remains available throughout.

### 1.3 How Users Think

Management does not begin an investigation with a name. They begin with a question:

```
Which customers are becoming risky?
Are any items tying up significant capital without movement?
Is the field team performing uniformly or are there outliers?
Is our purchase portfolio becoming concentrated in one principal?
```

These questions are populational. The investigation workspace is designed to serve this cognitive starting point.

Once an entity of interest is identified, the user's question shifts:

```
Is this entity genuinely abnormal, or just at the high end of normal?
Is the situation recent or persistent?
What explains it?
What evidence supports the conclusion?
```

The workspace must support both states: the populational scan and the targeted investigation.

### 1.4 How Investigation Progresses

Investigation follows a natural forward movement:

```
Discover
  → Who deserves attention in this population?

Frame
  → What kind of situation is this (risk, health, performance, exposure)?

Compare
  → Is this entity meaningfully different from peers?

Inspect
  → What are the current facts?

Contextualise
  → Is this normal or abnormal? Is it improving or deteriorating?
    Has management noticed it before? Is rank movement meaningful?

Explain
  → Which related entities explain this situation?

Validate
  → What source evidence proves this conclusion?

Conclude
  → What should management consider doing next?
```

Every architectural decision — layout, navigation, interaction, information ordering — should support this forward movement. Nothing in the workspace should cause the user to step backward involuntarily.

### 1.5 Mental Model

Management's working mental model in this workspace is:

> "I am looking at a living map of my business. I can see where every entity stands. I can point to ones that concern me, investigate why they concern me, confirm the conclusion with data, and leave knowing what to do next."

This is not a reporting experience. Reports answer "what happened." The investigation workspace answers "which entities deserve my attention and why."

---

## 2. Navigation Architecture

### 2.1 Entry Points

The investigation workspace may be entered from multiple surfaces. The workspace architecture must handle each entry gracefully.

**Population Entry (default):**
User navigates to Entity Analytics and selects an entity type. The workspace opens in Discovery Mode with the default Population Map preset. No entity is pre-selected. The user sees the full population.

**Alert-driven Entry:**
User is viewing an attention signal, notification, or management alert referencing a specific entity. Clicking the alert opens the workspace in Investigation Mode with the signalled entity pre-selected and the relevant map preset active. The population context is preserved — the user sees where the alerted entity sits among peers.

**Profile-direct Entry:**
User searches for a specific entity or arrives via a deep link. The workspace opens in Investigation Mode with the entity pre-selected. The population is loaded and visible, anchoring the entity in peer context immediately. The user does not start with a blank profile; they see the entity in population context.

**Report-driven Entry:**
User is viewing a source evidence report and clicks a link to investigate the entity in the workspace. The workspace opens in Investigation Mode with the entity pre-selected, the relevant preset active, and the investigation positioned at the Evidence Validation stage.

**Cross-entity Navigation Entry:**
User is investigating Entity A (e.g., Customer) and clicks a related entity in Business Drivers (e.g., the assigned Salesman). The workspace opens for the new entity type, with the selected entity pre-identified, and the previous investigation preserved in history for return.

### 2.2 Workspace Modes

The workspace operates in two structurally distinct modes that share the same layout surfaces.

**Discovery Mode:**

- Active when no entity is selected.
- The Population Map occupies the dominant surface.
- Filter controls, preset selector, and search are available.
- No context panels or investigation panels are visible.
- The workspace feels like a map: open, spacious, inviting exploration.
- User interaction at this stage is: scan, filter, zoom, hover for identification, select.

**Investigation Mode:**

- Active when one or more entities are selected.
- The Population Map reduces slightly but remains visible and dominant.
- Context panels appear progressively below the Population Map.
- The investigation flow is visible and navigable.
- User interaction at this stage is: inspect, compare, drill, validate.

The transition between modes is driven by entity selection. It is never forced by navigation; the user does not "change mode" explicitly. Selecting an entity transitions the workspace; deselecting all entities returns to Discovery Mode.

### 2.3 Drill-Down Model

Drill-down in the investigation workspace is conceptually one level: the user drills from population position to entity investigation. The workspace does not have nested drill-down stacks.

However, cross-entity navigation creates investigation continuity. When a user clicks a related entity in Business Drivers:

```
Customer Investigation
  → Click Assigned Salesman in Business Drivers
    → Salesman Investigation Workspace opens
      → Population Map shows salesman among peers
      → User may return to Customer investigation
```

This is not a drill-down stack; it is lateral navigation between entity investigations. The workspace must support navigating back to the originating entity without losing state.

### 2.4 Back Navigation

The workspace maintains a lightweight investigation history.

Back navigation returns the user to the previous workspace state, not to the previous page in the browser. If a user navigated from Customer A to Salesman B via Business Drivers, back navigation returns to Customer A's investigation in the state it was in at the time of departure.

Back navigation within the workspace:

- Restores entity selection
- Restores map preset
- Restores peer group filter
- Does NOT restore scroll position within the investigation panels (the user begins at the Population Map stage)

"Return to Population" is a persistent affordance within the workspace. It clears the entity selection and returns to Discovery Mode without losing the active filter and preset context.

### 2.5 Breadcrumbs

The workspace displays a contextual breadcrumb that reflects the active investigation:

```
Entity Analytics / [Entity Type] / [Map Preset] / [Entity Name]
```

Examples:
```
Entity Analytics / Customer / Customer Risk Map / PT Sumber Rejeki
Entity Analytics / Item / Inventory Health Map / SKU-8824
Entity Analytics / Salesman / Sales Performance Map / Budi Santoso
Entity Analytics / Supplier / Purchase Exposure Map / Principal Pratama
```

When no entity is selected:
```
Entity Analytics / Customer / Customer Risk Map
```

When comparison mode is active:
```
Entity Analytics / Customer / Customer Risk Map / 3 entities selected
```

Breadcrumb elements are navigable. Clicking "Customer Risk Map" clears entity selection and returns to Discovery Mode within the current preset. Clicking "Entity Analytics" returns to the entity type selector.

### 2.6 Workspace Persistence

Session-level persistence: the workspace preserves these states for the duration of the session.

| State | Persisted |
| --- | --- |
| Active entity type | Yes |
| Active map preset | Yes |
| Active peer group filter | Yes |
| Selected entity | Yes |
| Comparison set | Yes |
| Zoom level on Population Map | Yes |
| Active investigation panel (expanded/collapsed) | Yes |
| Evidence link history | No |

Workspace state should be serialisable to a URL so that a specific investigation can be shared between management users. A shared URL opens the workspace in the exact investigation state the sender had: entity type, preset, selected entity, active filter.

### 2.7 Context Preservation During Navigation

When a user navigates to a related entity (via Business Drivers or Alert), the originating investigation context is preserved:

- Originating entity remains in navigation history
- Originating filter and preset are recorded
- Return navigation restores the originating state exactly

The user should never feel they "lost" their investigation when exploring a related entity.

---

## 3. Workspace Composition

### 3.1 The Investigation as a Vertical Flow

The investigation workspace is composed as a vertical flow, not as a grid of panels. Each stage of investigation occupies a logical layer. Layers are revealed progressively as the investigation deepens.

This vertical flow mirrors the investigation logic itself:

```
[Population Stage]
  ↓
[Selection Stage]           — reveals on entity selection
  ↓
[Context Stage]             — reveals after selection
  ↓
[Explanation Stage]         — reveals after context is reviewed
  ↓
[Validation Stage]          — available throughout
```

This is not a dashboard collage. Each layer has a single dominant purpose. The workspace does not show all layers simultaneously at maximum size.

### 3.2 Population Stage

The Population Stage is the entry and anchor of the workspace.

Composition:
- **Entity type selector:** Customer / Item / Salesman / Supplier. Always visible.
- **Map preset selector:** Default presets per entity type (e.g., Customer Risk Map, Customer Growth Risk Map). The selected preset determines axis KPIs.
- **Population Map:** The primary investigation surface. Dominant, large, and interactive.
- **Filter controls:** Peer group filter (wilayah, category, supplier, active/risk status). Compact but accessible.
- **Legend:** Color and zone legend. Minimal, anchored within the Population Map canvas.

The Population Stage is the only stage visible in Discovery Mode. When no entity is selected, the workspace IS the Population Map.

### 3.3 Selection Stage

The Selection Stage appears when one or more entities are selected.

Composition:
- **Entity identity panel:** Entity name, code, category, status. Brief and factual.
- **KPI Summary:** Current Facts — the headline KPI values for the selected entity. Compact card layout, not a full table.
- **Comparison indicators:** If multiple entities are selected, comparison colors and entity names are shown as a persistent legend.

The Selection Stage anchors the investigation. It answers: "Which entity am I investigating, and what are its current headline facts?"

The KPI Summary at this stage is intentionally brief. Its purpose is to confirm the entity's identity and facts, not to be a complete profile.

### 3.4 Context Stage

The Context Stage provides the investigative depth behind the initial selection.

Composition:
- **Peer Position:** Distribution analysis for the primary KPI of interest. Shows the selected entity's position within the peer population.
- **Trajectory:** Trend visualization for selected KPIs. Shows direction and persistence.
- **Signal History:** Attention signal timeline. Shows persistence, recurrence, and resolution.
- **Position History:** Rank movement over time. Shows whether the entity's standing among peers has changed.

The panels within the Context Stage are individually collapsible and expandable. The default order follows the investigation priority established in the Product Vision: Peer Position → Trajectory → Signal History → Position History.

Not all panels need to be open simultaneously. Progressive disclosure applies within the Context Stage: users expand the panels they need for their current question.

### 3.5 Explanation Stage

The Explanation Stage connects the investigated entity to the related entities that explain its behaviour.

Composition:
- **Business Drivers:** Ranked table with embedded horizontal bars. One block per relationship type: assigned salesman, top customers, top items, top principals, depending on entity type.
- **Cross-entity navigation:** Each driver entry is a navigation entry point to the related entity's investigation workspace.

The Explanation Stage answers: "Why is this entity in its current situation?"

### 3.6 Validation Stage

The Validation Stage provides the evidence exit points.

Composition:
- **Evidence Links:** Catalog-backed links to source reports. Each link is specific to the selected entity and the active investigation context (entity type, KPI domain, time period).
- **Performance Signature:** Radar summary. Optional, compact. Appears only when a single entity is selected. Serves as an executive compression of the investigated entity before the user exits to evidence.

The Validation Stage answers: "What source evidence proves my conclusion?"

Evidence links are not embedded reports. They navigate the user to the existing BTR Portal report, pre-filtered to the current entity and context. The workspace guides the investigation; the report provides the final evidence.

### 3.7 Coherence Rule

No stage should be independently useful in isolation from the stages above it. Context Stage panels should only appear after an entity is selected. Explanation Stage should only appear after the entity's context is available. Validation Stage is always available once an entity is selected, but its value is understood only after the investigation above it has been conducted.

This is not a technical constraint. It is an intentional design discipline. If all stages were visible at once, the workspace becomes a dashboard collage. The progressive reveal forces the correct investigation order.

---

## 4. Information Architecture

### 4.1 Hierarchy Overview

Information in the investigation workspace is organized into four levels based on its role in the investigation decision.

| Level | Name | Investigation role | Components |
| --- | --- | --- | --- |
| Primary | Population Context | Answers "which entity deserves attention?" | Population Map, Peer Position |
| Secondary | Current State | Answers "what are the facts and is this normal?" | KPI Summary, Trajectory, Signal History, Position History |
| Tertiary | Explanation | Answers "why is this happening?" | Business Drivers |
| Validation | Evidence | Answers "what proves the conclusion?" | Evidence Reports |
| Executive | Compression | Answers "is this entity balanced?" | Performance Signature |

### 4.2 Why This Hierarchy

**Primary must be population-based.**
Individual KPI values cannot establish whether a situation is serious without population context. A customer with Rp 500M outstanding might be the most concerning customer in its wilayah or the median. The Population Map and Peer Position provide the relative context that gives individual facts meaning.

**Secondary confirms the primary finding.**
Once population position establishes that an entity is an outlier, the Current State confirms what the specific numbers are and whether the situation is new or persistent. Secondary information is not more important than Primary; it explains the Primary finding in more detail.

**Tertiary explains the mechanism.**
Business Drivers explain which related entities contribute to the situation. This is the causal layer. It answers "why" but only makes sense after the investigation has already established that something is worth explaining.

**Validation proves the conclusion.**
Reports are not the investigation; they are the evidence trail. They exist to allow management to sign off on a conclusion with documented evidence, not to substitute for the investigation process.

**Executive compresses after investigation.**
Performance Signature is useful only after an entity has been investigated. Showing it first creates false confidence that the entity is understood before the population context has been established.

### 4.3 Information Density Rules

| Stage | Density | Rationale |
| --- | --- | --- |
| Population Map | High: 2,100+ points | Full population is the product; density is intentional |
| KPI Summary | Low: 6–8 headline KPIs | Summary confirmation only; not a full KPI catalogue |
| Peer Position | Medium: one KPI distribution | One question answered precisely |
| Trajectory | Medium: 3–6 monthly periods | Direction and trend are the goal; not a full history |
| Signal History | Medium: signal list with status | Persistence and state, not exhaustive log |
| Position History | Low: rank over 6–12 months | Movement, not exact rank per period |
| Business Drivers | Low: top 5–10 per relationship block | Explanation, not exhaustive catalogue |
| Evidence | Minimal: links only | Navigation, not inline display |

---

## 5. Interaction Model

### 5.1 Selection

Selection is the central interaction in the investigation workspace.

**Single selection:**
A single click or tap on any entity in the Population Map selects that entity. The workspace transitions from Discovery Mode to Investigation Mode. The selected entity is visually highlighted. The surrounding population dims but remains visible. The Selection Stage, Context Stage, and Explanation Stage begin to reveal progressively.

**Deselection:**
Clicking an already-selected entity deselects it. Clicking empty space on the Population Map deselects all entities and returns to Discovery Mode. An explicit "Clear Selection" control is always visible in Investigation Mode.

**Selection persistence:**
Selected entities remain selected as the user scrolls down through the investigation stages. The Population Map remains accessible at the top of the workspace at all times. The user can add or remove entities from the comparison set without scrolling back to the Population Map.

### 5.2 Multi-selection and Comparison Mode

Multi-selection is the mechanism for entering Comparison Mode.

**Adding to comparison:**
Clicking a second entity while one is already selected adds it to the comparison set. Both entities are now highlighted with distinct comparison colors. All active investigation panels update to show both entities.

**Comparison limit:**
Maximum 4 entities in a comparison set. Attempting to add a fifth entity requires removing one from the current set. The workspace presents a confirmation: "Maximum 4 entities for comparison. Remove one to add [Entity Name]?"

**Comparison color assignment:**
Colors are assigned in sequence on selection. Each color is consistent across the Population Map, Trend, Position History, and all other panels for the duration of the comparison session. Colors are released when an entity is deselected.

**Removing from comparison:**
A selected entity can be removed from the comparison set by clicking it again on the Population Map or by dismissing it from the comparison legend in the Selection Stage.

### 5.3 Hover

Hover is information retrieval, not selection.

**Population Map hover:**
Hovering any point displays a tooltip: entity name, axis KPI values, percentile or rank, and active attention state. The hovered entity is briefly highlighted; the surrounding population does not change. The tooltip does not persist after the cursor leaves.

**Panel hover:**
Hovering a row in Business Drivers or a bar in Peer Position highlights the corresponding entity in the Population Map (cross-highlight), if that entity is visible in the current Population Map context.

Hover never triggers state changes. Hover is always transient and reversible.

### 5.4 Filtering

Filtering narrows the active peer population without removing population context.

**Peer group filter:**
Management can filter by wilayah (region), category, supplier/principal affiliation, active status, and risk/attention state. The filter controls are always accessible within the Population Stage.

**Filter application:**
Applying a filter immediately redraws the Population Map with the filtered population. A "scope indicator" prominently displays the active filter context: "Showing 312 of 2,100 active customers — Wilayah Timur."

**Filtered selection:**
Selected entities that no longer match the active filter are noted with a warning: "Customer A is outside the current filter scope. Population context reflects filtered peers only." The entity remains selected and the investigation continues, but the peer comparison is transparently scoped.

**Filter reset:**
A single "Reset Filter" control restores the full population. Filter reset does not clear entity selection.

### 5.5 Zooming

Zooming is available on the Population Map as a density management tool.

**Zoom purpose:**
At full population scale (2,100 customers, 3,000 items), individual entities may cluster. Zoom allows management to inspect a specific region of the map more clearly.

**Zoom behaviour:**
Scroll-wheel zoom and pinch-to-zoom are both supported. Pan is available at any zoom level. Zoomed state is preserved during the session.

**Zoom and population context:**
Zooming does not hide non-visible entities. The legend and scope indicator reflect the full population. A mini-map or viewport indicator can optionally show the current zoom region relative to the full population extent. Zoom is not a substitute for filtering.

**Zoom and selection:**
Entities can be selected whether or not they are at the zoomed-to position. A "Locate selected entity" affordance centres the zoom on the selected entity.

### 5.6 Cross-Highlighting

Cross-highlighting creates coherence between the Population Map and the investigation panels.

When a user hovers a row in Business Drivers, the corresponding entity is highlighted in the Population Map if that entity is present in the current population view. For example: hovering "Budi Santoso" in a Customer's assigned salesman block would highlight Budi Santoso in the Salesman Population Map if the salesman workspace is co-active.

In the base M32R implementation, cross-highlighting operates within the same entity-type workspace. Cross-entity-type highlighting (hover a customer's salesman and see the salesman highlighted in a separate map) is a future enhancement.

### 5.7 Search

Entity search is available as a filter within the Population Map, not as a navigation mechanism.

**Search behaviour:**
Typing in the search field highlights matching entities in the Population Map by label. Non-matching entities remain visible but are visually recessed. The search helps management locate a specific entity without requiring them to scroll through a list.

**Search and selection:**
Pressing Enter on a search result selects the entity and enters Investigation Mode. Search does not replace the need to see the entity in population context.

### 5.8 Keyboard Shortcuts

| Shortcut | Action |
| --- | --- |
| Escape | Deselect all entities / return to Discovery Mode |
| Arrow keys | Cycle through entities in comparison set (focus highlight) |
| Enter / Space | Select / confirm entity under focus |
| F | Toggle filter panel |
| R | Reset to default map preset |
| Z | Reset zoom to full population view |
| C | Clear all filters |
| Backspace | Remove last entity from comparison set |

Keyboard shortcuts apply when the Population Map has focus. They are documented in a persistently accessible keyboard guide within the workspace.

### 5.9 Reset and Undo

**Full Reset:**
Returns the workspace to its default state: default preset, no entity selected, no filters active, full population visible. Accessible from the breadcrumb and from a workspace control.

**Partial Reset (filter reset):**
Clears all active filters while preserving entity selection.

**Selection Clear:**
Clears entity selection and returns to Discovery Mode while preserving filter and preset state.

**Undo:**
Workspace state changes (selection, deselection, filter, zoom) are recorded in a lightweight in-session history. Ctrl+Z / Cmd+Z reverses the last state change. History depth: 10 steps.

---

## 6. Comparison Architecture

### 6.1 Comparison Philosophy

Comparison in the investigation workspace is always conducted against a shared population context. Two entities are never compared in isolation.

The critical rule: **comparison never removes the population.**

When management compares Customer A and Customer B, they are not comparing two entities against each other in a vacuum. They are comparing two entities against their mutual peer population, and against each other as a secondary reading.

This matters because without population context, a small difference between two entities may seem important when both are actually within the normal range of the population. Population-preserving comparison prevents over-interpretation of relative differences.

### 6.2 How Comparison Behaves Per Panel

| Panel | Comparison behaviour |
| --- | --- |
| Population Map | Full population visible; selected entities highlighted with distinct comparison colors. All selected entities visible simultaneously. |
| Peer Position | Selected entity markers shown at their respective positions in the distribution. Multiple markers visible on the same distribution. |
| KPI Summary | Columns aligned per entity. Same KPI definitions used. Differences between entities are readable through column juxtaposition. |
| Trajectory | Overlaid lines per selected entity using comparison colors. A muted peer-average reference line optionally available. |
| Position History | Rank paths per selected entity overlaid. Same rank metric applied to all. |
| Signal History | Side-by-side columns per selected entity. Signal types aligned across columns. |
| Business Drivers | Independent blocks per entity. Drivers are entity-specific; they are not compared side-by-side. |
| Performance Signature | Overlay for 2 entities; separate views for 3–4 entities for readability. |

### 6.3 Comparison Color Identity

Each selected entity in a comparison set is assigned a color identity at the moment of selection.

**Color identity rules:**
- Colors are drawn from a fixed, designed comparison palette.
- The palette contains exactly 4 colors, one per maximum comparison slot.
- Colors are visually distinct for the most common forms of color blindness.
- The first selected entity always receives the first palette color.
- Color identity is consistent across all panels for the duration of the comparison session.
- Color identity is released when an entity is deselected.

Color identity is the mechanism by which management maintains coherence when reading across multiple panels simultaneously. If Trajectory shows three overlapping lines, the management user must be able to identify each line without reading a separate legend each time. Consistent color identity makes this possible.

### 6.4 Comparison Panel Synchronisation

All comparison panels are synchronised. Synchronisation means:

**Entity synchronisation:** When an entity is added or removed from the comparison set, all active panels immediately reflect the updated comparison set.

**Filter synchronisation:** When a filter is applied, all panels reflect the filtered peer group. Selected entities that fall outside the filter are handled consistently across all panels.

**KPI selection synchronisation:** When management changes the active KPI in Peer Position or Trajectory, the Population Map updates to the corresponding preset if applicable.

There is no explicit "synchronise" button. Synchronisation is always on.

### 6.5 Comparison Limit Rationale

The maximum of 4 entities is a product decision carried forward from the Product Vision. The architecture should enforce this limit and explain its rationale clearly to users.

When comparison involves 5 or more entities, the investigation question changes from "how does Entity A compare to Entity B?" to "how does this curated set of entities collectively behave?" That is the Population Review Board question, not the investigation workspace question. Enforcing the limit is therefore not a technical constraint; it is a product integrity rule.

---

## 7. Evidence Architecture

### 7.1 The Evidence Chain

Every investigation in the workspace must be capable of ending in validated evidence. The evidence chain is the architectural path from population position to source proof.

```
Population position
  → KPI facts
    → Peer normality
      → Trend direction and persistence
        → Signal persistence
          → Relationship drivers
            → Source evidence report
```

Each step in the chain narrows and deepens the investigation. Each step produces a finding that the next step either confirms or questions. The chain is complete when management has a finding that can be directly verified in a source report.

### 7.2 Evidence Connection Rules

The architecture establishes four types of evidence connection:

**KPI-to-Report connections:**
Every KPI value displayed in KPI Summary carries an evidence route to the source report. For example, the open balance (piutang) KPI links to the Piutang tracker report filtered to the selected customer. This is catalog-backed, not manually configured per visualization.

**Signal-to-Detail connections:**
Every attention signal in Signal History links to the signal detail or the underlying report that triggered the signal. Management can click a "Chronic Overdue" signal and reach the invoice-level evidence.

**Driver-to-Investigation connections:**
Every related entity in Business Drivers is a navigation entry point. Clicking the assigned salesman opens the Salesman investigation workspace for that entity. Clicking a top-purchased item opens the Item investigation workspace. These are not report links; they are lateral investigation continuations.

**Evidence-Report navigation:**
The Validation Stage provides curated evidence links. These are pre-scoped to the selected entity, entity type, and time context. Management does not need to configure the report context; the workspace has already established it.

### 7.3 Evidence Independence Rule

Evidence reports are the authoritative source of record. The investigation workspace does not replace, summarise, or substitute for evidence reports.

The workspace is the investigation. The report is the proof.

This means:
- KPI values in the workspace may lag report-level values if snapshots are used. The workspace should clearly indicate the data freshness of each value.
- Report-level values take precedence over workspace values when there is a discrepancy.
- Management should always be able to follow an evidence link to see the raw data that underlies any workspace display.

### 7.4 Investigation Completeness Signal

The workspace should provide a soft signal when an investigation has visited enough stages to support a conclusion. This is not a mandatory gate; management may exit at any point. But the workspace can hint: "You have reviewed Current Facts, Peer Position, and Signal History. Business Drivers and Evidence are available for this entity."

This completeness signal is not a checklist. It is an ambient indication of investigation depth, similar to how a progress bar in a form suggests remaining steps without blocking progress.

---

## 8. Future Extension Architecture

### 8.1 Design Principle for Extension

Future capabilities must plug into the existing investigation flow without requiring a workspace redesign. The architecture reserves explicit structural positions for each future capability.

The rule: **new capabilities extend the investigation chain; they do not replace existing stages.**

### 8.2 Population Review Board

**Position in architecture:** Post-investigation, activated from the Selection Stage.

**Extension mechanism:**
The workspace maintains a selection set (the comparison set). The Population Review Board capability accepts this selection set as its input. A "Send to Population Review Board" affordance becomes available when a comparison set is active.

The Population Review Board opens as a separate workspace view, not within the investigation workspace. It operates on the curated selection and returns management to the investigation workspace when complete.

**What does not change:**
Investigation workspace layout is unchanged. The selection set mechanism already exists at M32R. The Population Review Board is simply a new consumer of the selection set.

### 8.3 Business Event Timeline

**Position in architecture:** Within the Context Stage, after Signal History.

**Extension mechanism:**
The Context Stage reserves a structural position for Business Event Timeline between Signal History and Position History. At M32R, this position is empty. When Business Event Timeline is implemented, it fills this position.

**What does not change:**
The Context Stage layout, the Peer Position and Trajectory panels, and the overall investigation flow are unchanged. Timeline slots in naturally.

**Data requirement note:**
Business Event Timeline requires event-level data (status changes, interventions, visits, promotions, purchase events) that is not yet structured for investigation use. This data requirement does not affect M32R implementation.

### 8.4 Decision Support and AI Recommendations

**Position in architecture:** After the Validation Stage, as a separate conclusion layer.

**Extension mechanism:**
The Validation Stage is the current endpoint of the investigation. Decision Support adds a Conclusion Stage after Validation. The Conclusion Stage accepts the evidence chain accumulated during the investigation and presents recommended actions.

**Evidence traceability requirement:**
The architecture already builds the evidence chain required for Decision Support recommendations. The chain is:

```
Population position
  → KPI facts
    → Peer percentile
      → Signal persistence
        → Relationship drivers
          → Evidence report links
```

Every piece of information in this chain is accessible within the M32R architecture. Decision Support consuming this chain requires no architectural change to the investigation workspace. It requires only the addition of the Conclusion Stage layer.

**What does not change:**
The entire investigation flow from Population Stage through Validation Stage is preserved unchanged. Decision Support is additive.

### 8.5 Workflow Integration

**Position in architecture:** After Decision Support, or optionally within the Validation Stage.

**Extension mechanism:**
When an investigation reaches a conclusion (Decision Support), the workspace should allow management to initiate a workflow action (collection escalation, purchase stop, coaching flag, etc.) without leaving the investigation context.

Workflow actions are entity-specific and require an evidence-traceable conclusion. The M32R architecture supports this because the full evidence chain is available.

**What does not change:**
The investigation workspace is investigation-only at M32R. Workflow panels are additive in the post-Decision-Support layer.

---

## 9. Design Principles

These principles govern every architectural and design decision within the investigation workspace. They should be consulted whenever a new capability, panel, or interaction is proposed.

### Principle 1: Focus Before Detail

Management should always see the population context before reading individual entity details. The workspace enters at the population. Individual profiles are available only after population context is established.

*Why:* Individual values are meaningless without peer context. A high piutang value is only concerning if it is above the peer distribution. The population frame gives detail its meaning.

### Principle 2: Context Before Precision

The relative position of an entity in the population is more important than the exact value of any one KPI. The workspace should make relative position immediately legible and reserve precise values for the detail stage.

*Why:* Management decisions are comparative. Precision is needed to document a conclusion, not to form it. The investigation leads with context; precision follows.

### Principle 3: Evidence Before Conclusion

Every conclusion formed in the workspace must be traceable to evidence. The workspace actively guides management toward evidence by embedding evidence links at every relevant stage.

*Why:* Management decisions with financial, credit, or operational consequences must be defensible. An evidence chain protects management and ensures the system is trustworthy.

### Principle 4: Progressive Disclosure

The workspace reveals information as the investigation deepens. Initial state is minimal. Information density increases as the user selects, investigates, and drills.

*Why:* Cognitive load grows with visible information. Showing everything at once creates the same problem as a dashboard collage: management must work to identify what is important. Progressive disclosure ensures that each piece of information appears only when it is relevant.

### Principle 5: One Primary Question Per Visualization

Each visualization in the workspace has one dominant management question it answers. No visualization should try to answer multiple questions simultaneously.

*Why:* Multi-purpose visualizations produce ambiguous reading. Management should always know what question they are looking at. If a visualization cannot be described in one sentence ("this shows which entities are outliers in the context of two KPIs"), it is doing too much.

### Principle 6: Investigation Is a Forward Movement

The workspace flow always moves forward: Discover → Frame → Compare → Inspect → Contextualise → Explain → Validate → Conclude. Navigation should support this forward momentum without forcing it.

*Why:* Investigation that loops back repeatedly or allows management to jump to evidence before understanding context produces unreliable conclusions. The architecture creates a natural path that management follows at their own pace but in the correct direction.

### Principle 7: Comparison Preserves Population

Selecting entities for comparison never removes the population from view. The Population Map always shows the full eligible population alongside selected entities.

*Why:* Comparison without population context creates the illusion that two selected entities are the full universe. Management might over-interpret a difference between Entity A and Entity B that is actually within normal population variation.

### Principle 8: Business Language Throughout

Every label, title, placeholder, tooltip, and affordance in the workspace uses business-facing language. Technical terms (histogram, scatter, radar, percentile) are replaced by business terms (Peer Position, Population Map, Performance Signature, population rank).

*Why:* Management should never feel they are configuring a chart tool. The language of the workspace should feel like the language of a management conversation, not the language of data visualisation software.

### Principle 9: Entity Type Consistency

The investigation workflow, panel structure, and interaction model are identical across all four entity types: Customer, Item, Salesman, Supplier. Entity-specific content lives inside the shared framework.

*Why:* If each entity type requires a different mental model to use, the product produces four separate learning curves. A consistent framework means management learns once and applies across all contexts.

### Principle 10: Low Cognitive Load at Every Stage

At any point in the investigation, management should be able to describe what they are looking at and what question they are answering. Whenever a panel or visualization cannot meet this standard, it has too much cognitive load.

*Why:* Management users are not data analysts. The workspace exists to reduce the effort required to make good decisions, not to reward analytical expertise. Cognitive load reduction is a product quality criterion.

---

## 10. Implementation Guidance

This section provides architectural direction for implementers. It does not specify technology, framework, or code structure. It establishes boundaries that the implementation must respect.

### 10.1 Architectural Boundaries

**Investigation Context Boundary**

Responsible for: entity selection state, comparison set, active peer group filter, active map preset, workspace mode (Discovery / Investigation), session history, and URL serialisation.

This boundary owns the shared state of the workspace. All panels receive their data context from the Investigation Context. No panel should independently manage entity selection or filter state.

**Population Visualization Boundary**

Responsible for: rendering the Population Map, handling zoom and pan, managing entity point display, label strategy, tooltip rendering, selection gestures (click, hover), and filter application on the visible population.

This boundary is performance-sensitive. At 2,100+ customer points or 3,000+ item points, the rendering strategy must be efficient. This boundary should receive pre-computed population data (snapshot-based) and not perform real-time aggregation.

**Profile Context Boundary**

Responsible for: fetching and presenting KPI Summary, Trajectory, Peer Position, Signal History, Position History, and Business Drivers for the currently selected entity set.

This boundary is loaded on selection, not upfront. It should not load data until an entity is selected. It must support incremental loading as each investigation panel is expanded.

**Evidence Route Boundary**

Responsible for: resolving catalog-backed evidence links to source reports. Given an entity identity, entity type, KPI domain, and time context, this boundary produces navigable links to the correct evidence report.

This boundary must be catalog-driven, not hardcoded per visualization. Evidence routes are maintained in the KPI catalog and are not the responsibility of individual panels.

**Preset Engine Boundary**

Responsible for: managing map presets per entity type, axis KPI assignments per preset, default filter configurations, and custom axis configuration (future/advanced).

This boundary shields the Population Visualization from knowing about KPI semantics. The Preset Engine translates business preset names into the KPI axis configuration required for population rendering.

### 10.2 State Ownership

| State | Owner | Consumers |
| --- | --- | --- |
| Entity type | Investigation Context | All panels |
| Selected entity set | Investigation Context | All panels |
| Comparison set | Investigation Context | Population Map, all context panels |
| Active preset | Preset Engine | Population Map, KPI Summary |
| Peer group filter | Investigation Context | Population Map, Peer Position |
| Zoom level | Population Visualization | Population Map only |
| KPI data per entity | Profile Context | Context Stage, Explanation Stage |
| Evidence links | Evidence Route Boundary | Validation Stage |
| Session history | Investigation Context | Navigation controls |

### 10.3 Data Loading Strategy

**Population data:**
Loaded once per entity type and peer group filter change. Population data is snapshot-based (pre-computed). The Population Map renders from the snapshot and does not require real-time computation. Population snapshot contains: entity identity, axis KPI values per preset, category/wilayah attributes, active status, attention state summary.

**Profile data:**
Loaded on entity selection. Profile data loads incrementally: KPI Summary loads first, then Peer Position, Trend, Signal History, and Business Drivers load in parallel or on-demand as panels are expanded. Profile data is not pre-fetched for all entities in the population.

**Comparison data:**
Each selected entity loads its profile data independently. The Profile Context boundary manages the set of active profiles. Panels receive the full comparison set and render accordingly.

**Evidence links:**
Resolved synchronously from the catalog on entity selection. Evidence links require no network call if the catalog is locally available.

### 10.4 Performance Considerations

The critical performance surface is the Population Map. At 3,000 item points or 2,100 customer points, the rendering technology must support smooth pan/zoom without re-fetching data.

Points outside the current zoom viewport are still logically present (they are part of the population context), but rendering them at reduced detail is acceptable. The implementation choice between SVG and canvas is left to the architect for the specific milestone, but the architectural constraint is clear: full population must be renderable at interactive frame rates.

Label strategy is a performance and legibility concern. At full population scale, labelling every point is visually and computationally unacceptable. Labels are rendered only for: selected entities, hovered entities, and major outliers (as determined by the outlier detection logic within the Preset Engine).

### 10.5 Snapshot Layer Compatibility

M32R reuses the existing L0–L5 Entity Analytics snapshot infrastructure. The architecture does not require a new snapshot layer for M32R. The Preset Engine maps population map axis KPIs to existing snapshot fields.

If new KPI combinations required for presets are not available in the existing snapshot, the correct response is to extend the snapshot layer — not to introduce real-time aggregation into the Population Visualization Boundary.

### 10.6 Entity-Type Neutrality

Every architectural boundary, panel, and interaction must be designed to serve all four entity types without per-entity-type branching in the core architecture. Entity-specific content (KPI names, relationship block types, preset configurations, evidence route mappings) is configuration, not code structure.

---

## Appendix A: Investigation Stage Mapping

| Investigation stage | Workspace stage | Primary capability | Natural next step |
| --- | --- | --- | --- |
| Discover | Population Stage | Population Map | Select an outlier |
| Frame | Population Stage | Map preset selector | Choose relevant preset |
| Compare | Population Stage | Shared Population Map | Move to Inspect |
| Inspect | Selection Stage | KPI Summary | Decide which KPI needs context |
| Contextualise — normality | Context Stage | Peer Position | If extreme, check Trajectory |
| Contextualise — persistence | Context Stage | Trajectory | Check Signal History |
| Contextualise — signals | Context Stage | Signal History | Move to Explanation |
| Contextualise — rank | Context Stage | Position History | Compare with peer movement |
| Explain | Explanation Stage | Business Drivers | Navigate to related entity or validate |
| Validate | Validation Stage | Evidence links | Exit to source report |
| Compress | Validation Stage | Performance Signature | Used for executive review only |

---

## Appendix B: Navigation State Machine

```
Discovery Mode
  → [Entity selected]
    → Investigation Mode (single entity)
      → [Second entity selected]
        → Comparison Mode (2 entities)
          → [Third entity selected]
            → Comparison Mode (3 entities)
              → [Fourth entity selected]
                → Comparison Mode (4 entities) [MAX]
      → [Related entity clicked in Business Drivers]
        → New entity type Investigation Mode
          → [Back navigation]
            → Previous Investigation Mode (state restored)
  → [Filter applied]
    → Discovery Mode (filtered population)
  → [Reset]
    → Discovery Mode (full population, default preset)
```

---

## Document Control

| Version | Date | Author | Change |
| --- | --- | --- | --- |
| 1.0 | 2026-06-27 | Lead Product Architect | Initial architectural reference for M32R |
