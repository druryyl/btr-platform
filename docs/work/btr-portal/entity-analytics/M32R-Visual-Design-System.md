# M32R Visual Design System

**Status:** Visual Architecture Reference — Version 1.0  
**Milestone:** M32R Entity Analytics Investigation Workspace  
**Date:** 2026-06-27  
**Role:** Lead Product Architect  
**Audience:** Future implementers, designers, component authors, future architects  
**Authoritative Input:** M32-Entity-Analytics-Visualization-Feasibility-Study.md (Product Vision v2.0) and M32R-Investigation-Workspace-Architecture.md

> This document defines the visual language of Entity Analytics. It is not a pixel specification, CSS rulebook, Tailwind configuration, or Figma template. It defines the principles, semantics, and rules that govern how the investigation workspace looks, behaves visually, and communicates meaning. Every future implementer who adds a new visualization, panel, or interaction to Entity Analytics should read this document first.

---

## 1. Visual Philosophy

### 1.1 Investigation Before Decoration

The investigation workspace is not a showcase. It is a professional tool for management decisions about collection risk, inventory health, sales performance, and purchasing exposure.

Every visual element must serve an investigation function. If a visual element cannot be traced to a specific management question it helps answer, it should not be there.

This means:
- No illustrative icons used for decoration
- No colourful gradient fills used to add visual interest
- No animation used to signal technical capability
- No whitespace used to create a "modern" impression rather than to separate cognitive content

Decoration is the enemy of investigation. It consumes the attention budget that belongs to data.

### 1.2 Business Clarity Over Visual Complexity

A management user opening the Customer Risk Map at 7:00 AM should be able to identify the most concerning customers within 15 seconds. This is the usability standard for the investigation workspace.

Visual complexity works against this standard. Complexity emerges from:
- Too many color values in use simultaneously
- Too many competing visual weights
- Too many labels in the same spatial region
- Too many visual dimensions (size, color, shape, label) encoding different information at once

Business clarity requires restraint. A visually simpler workspace that makes the data speak is more valuable than a visually impressive workspace where the data is buried.

### 1.3 One Visual Focus Per Screen

At every point in the investigation, there is one dominant visual surface. Every other element is subordinate to it.

In Discovery Mode, the dominant visual is the Population Map. The filter controls, preset selector, and breadcrumb are all subordinate. They should not compete visually with the Population Map.

In Investigation Mode, the dominant visual shifts as the user moves through investigation stages. When reading Peer Position, the dominant visual is the distribution. When reading Trajectory, the dominant visual is the trend line. The workspace should never present two competing dominant visuals at the same time.

### 1.4 Visual Calm in Dense Information

The investigation workspace must handle 2,100 customer points, 3,000 item points, multiple overlaid trend lines, comparison color sets, and evidence tables — often in sequence within a single session.

Visual calm is the antidote to information overload. Visual calm is achieved through:
- A restrained background that recedes behind the data
- A controlled color vocabulary where most elements use neutral tones and only meaningful elements use accent colors
- Consistent spacing that separates investigation stages without requiring visual dividers
- Type that is legible at density without requiring large sizes
- Gridlines and reference elements that are present but not prominent

Visual calm does not mean visual emptiness. It means that nothing competes for attention except the data that deserves attention.

### 1.5 Data Reveals Itself

The Population Map should feel alive. When management scans it for the first time, the structure of the population — the clusters, the outliers, the zones — should be immediately apparent without explanation.

This is the standard that the visual design must achieve: the data structure is visible before the user reads a single label.

To achieve this:
- Point density and distribution must be visually readable at the default zoom level
- Zone separators and quadrant labels must reduce ambiguity, not explain what is already visually obvious
- Color encoding (risk state, attention state, category) must reinforce what the position in the map already suggests

### 1.6 Trust Through Consistency

Management will use the investigation workspace repeatedly. They build trust through predictability: "I know what blue means in this workspace. I know that the large number on the left side of the KPI card is the current value. I know that the dashed reference line is the peer average."

Consistency is a trust mechanism. Every inconsistency is a small cognitive friction that accumulates into user skepticism about the system's reliability.

The visual design system is the contract that enforces consistency across every panel, visualization, entity type, and future extension.

---

## 2. Layout Grammar

### 2.1 The Primacy of the Population Map

The Population Map is the dominant visual element of the investigation workspace. Its size must reflect its role: it is not one panel among equals; it is the investigation surface.

**In Discovery Mode:** The Population Map occupies the full available height of the primary viewport. Filter controls are compact and attached to the Population Map boundary. The preset selector is visible but does not compete for space. Nothing below the Population Map is visible until an entity is selected.

**In Investigation Mode:** The Population Map shrinks to its minimum effective investigation size — large enough to read population structure and selected entity position, but yielding vertical space to the investigation panels below. The Population Map never becomes a thumbnail.

**The Population Map minimum effective size:** At minimum, the Population Map must be large enough that a cluster of 5–10 related entities is visually distinguishable from surrounding points. Below this threshold, the map loses its investigation value and should instead present a summarised population view or prompt the user to filter.

### 2.2 Section Order and Reading Order

The workspace follows a top-to-bottom reading order that matches the investigation logic.

```
[Top of viewport]

  Population Stage
    — Entity type selector (horizontal)
    — Map preset selector (horizontal)
    — Population Map (dominant)
    — Filter controls (attached, compact)

  ↓ [on entity selection]

  Selection Stage
    — Entity identity (name, code, status)
    — KPI Summary cards (horizontal or compact grid)
    — Comparison legend (if multiple entities selected)

  ↓

  Context Stage
    — Peer Position
    — Trajectory
    — Signal History
    — Position History

  ↓

  Explanation Stage
    — Business Drivers (by relationship block)

  ↓

  Validation Stage
    — Evidence links
    — Performance Signature (optional, compact)

[Bottom of viewport]
```

This order is not negotiable. A panel from the Explanation Stage must not appear before the Context Stage panels. The reading order is the investigation order.

### 2.3 Visual Rhythm

Visual rhythm is the regulated repetition of spacing, weight, and proportion throughout the workspace. It creates a sense of order without rigidity.

**Stage breaks:** Visible separation between Population Stage, Context Stage, Explanation Stage, and Validation Stage. The separation is achieved through whitespace — not through lines, cards, borders, or background changes. A clear vertical gap between stages signals a new investigation level.

**Panel breaks:** Within each stage, individual panels (Peer Position, Trajectory, Signal History) are separated by a lighter, smaller gap than stage breaks. The difference in gap size signals: "same stage, different question."

**Internal panel rhythm:** Within each panel, consistent spacing between the panel title, the visualization area, and any supporting labels. The visualization area is always the largest element within its panel.

**Horizontal rhythm:** KPI Summary cards follow a consistent horizontal spacing. Business Driver rows follow a consistent row height. Trend lines within Trajectory follow a consistent axis scale.

### 2.4 Spacing Philosophy

Spacing communicates thought separation. Empty space between two elements says: "these are different things." Dense proximity says: "these belong together."

Rules:
- Space between stages > space between panels within a stage > space between elements within a panel
- The Population Map has breathing space on all sides: it is not edge-to-edge
- KPI Summary cards have consistent gutters; they are not flush against each other
- Investigation panels are not packed together; each panel has headroom above its title

Spacing is not decorative. Every gap is a semantic signal.

### 2.5 Progressive Disclosure and Visual Balance

Context Stage, Explanation Stage, and Validation Stage are not visible until an entity is selected. This means the initial view of the workspace is intentionally sparse below the Population Map.

This sparseness is correct. It is not emptiness; it is the visual equivalent of a blank canvas awaiting investigation input. The workspace should not pre-populate with placeholder panels.

When investigation panels appear, they appear progressively. The first panel to appear is KPI Summary (immediate confirmation of the selected entity). Context Stage panels appear on a short delay or on user scroll. Explanation and Validation Stages appear as the user reaches them.

Visual balance in Investigation Mode: the Population Map (upper half) and the investigation panels (lower half) should be proportionally balanced. The investigation panels should feel like they belong to the same workspace as the Population Map, not like an afterthought appended below.

### 2.6 Responsive Philosophy

The investigation workspace is a management desktop application. The primary design target is a wide-format screen (minimum 1280px width, optimum 1440–1920px).

At M32R milestone, mobile is not a target surface. However, the workspace should not be designed so narrowly that it becomes completely unusable at 768px tablet width. A tablet-compatible reduced view shows: Population Map + Entity identity + KPI Summary. Investigation panels are accessible via scroll but not optimised for touch interaction.

The Population Map is not zoomable via touch at full tablet resolution without explicit touch gesture support. This is a noted constraint for future consideration.

---

## 3. Visual Hierarchy

### 3.1 Hierarchy Overview

The visual hierarchy of the workspace establishes which elements are dominant, which support, and which recede.

| Level | Element | Visual weight | Purpose |
| --- | --- | --- | --- |
| 1 | Population Map | Dominant | Primary investigation surface |
| 2 | Selected entity indicators | High | Identity and context of the entity under investigation |
| 3 | KPI Summary headline values | High | Immediate facts for the selected entity |
| 4 | Peer Position distribution | Medium-high | Normality context for the selected KPI |
| 5 | Trajectory lines | Medium | Direction and persistence |
| 6 | Signal History state | Medium | Persistence of management attention |
| 7 | Business Driver rows | Medium-low | Explanation layer |
| 8 | Position History rank paths | Medium-low | Supporting rank context |
| 9 | Evidence links | Low | Exit points to source data |
| 10 | Performance Signature | Low | Executive compression only |
| 11 | Labels, metadata, axis text | Minimal | Supporting reading aids |
| 12 | Grid lines, zone separators | Minimal | Spatial orientation only |

### 3.2 Why Population Map Dominates

The Population Map is the entry point, the orientation surface, and the persistent anchor of the investigation. It answers the primary question: "which entity deserves attention?"

If the Population Map does not dominate visually, management's first impression of the workspace is incorrect. They will scan the wrong element first and build the wrong mental model.

The Population Map must be visually arresting — not through color or decoration, but through scale and the immediate legibility of its data structure.

### 3.3 Why KPI Summary Is High Weight

After an entity is selected, management's immediate question is "what are the current facts?" KPI Summary must answer this immediately and prominently. A KPI Summary that requires zooming in, hunting for values, or parsing dense tables defeats its purpose.

Headline KPI values are the highest-weight typographic element below the Population Map. They should be readable at arm's length.

### 3.4 Why Evidence Is Low Weight

Evidence links are the exit point of the investigation, not the visual anchor. Management should spend most of the investigation inside the workspace. Evidence links are used when the conclusion has been formed and validation is needed.

Making evidence links visually prominent would draw management prematurely to reports before the investigation is complete. Low visual weight is intentional.

### 3.5 Why Performance Signature Is Low Weight

Performance Signature (Radar) is an executive compression tool. It provides a useful multi-dimensional summary after investigation, but it does not lead investigation. Making it visually prominent would suggest it is the primary investigation tool, which contradicts the Product Vision.

Performance Signature appears after evidence links and is the least visually dominant element in the workspace. It is available; it does not demand attention.

---

## 4. Color Language

### 4.1 Fundamental Rule

Color in the investigation workspace is a semantic vocabulary. Every color use must have a defined meaning. Using a color for visual variety, brand alignment, or aesthetic interest is prohibited.

Management must be able to trust that color means something. If color sometimes signals risk and sometimes means "this row is the second one," color loses its communicative power.

### 4.2 Color Semantic Definitions

**Neutral — Population Default**

The color of unselected, un-highlighted entities in the Population Map. Communicates: "this entity has no special current status. It is part of the population."

Neutral entities form the visual mass of the Population Map. They should be clearly visible but visually recessive compared to selected entities and attention-flagged entities.

**Selected — Active Investigation**

The color assigned to a selected entity. Communicates: "this is the entity currently under investigation."

In single-entity investigation, selected color is one distinct, warm, high-contrast color from the comparison palette. The selected entity must be immediately distinguishable from the neutral population.

**Comparison Colors — Multi-entity Investigation**

A palette of four distinct colors, each assigned to one entity in the comparison set. Communicates: "this is Color 1 entity, this is Color 2 entity." Consistent across Population Map, Trend, Position History, KPI Summary, and Signal History.

Comparison colors must be:
- Discriminable from each other and from the neutral background
- Compatible for the most common color vision deficiencies (deuteranopia, protanopia)
- Consistent with each other in visual weight (no one comparison color overwhelms the others)

**Attention / Risk — Management Signal Active**

The color applied to entities with one or more active management attention signals. Communicates: "this entity has been flagged by the attention system."

Attention-colored entities in the Population Map should be visible without being visually dominant over selected entities. They are informational, not alarming.

**Critical — Extreme Position**

The color applied to entities in an extreme position relative to the population: extremely high-risk zone, extreme outlier in Peer Position, most severe attention signal category.

Critical color should draw the eye but not create panic. It is a professional signal, not an alarm. It should be clearly distinguishable from Attention color but not so visually aggressive that it dominates the entire map.

**Healthy / Positive — Favorable Position**

The color applied to entities in a clearly positive zone: high achievement with low risk, strong movement with healthy inventory, strong posting discipline. Communicates: "this entity is performing favorably."

Healthy color should be visually positive without being distracting. It should be easy to distinguish from Critical without creating a false binary between "good" and "bad."

**Historical / Inactive — Resolved or Dormant**

The color applied to entities whose attention signals have been resolved, or entities in a dormant state. Communicates: "this entity had a signal; it is no longer active" or "this entity is not currently active."

Historical/inactive color should be visually recessive. These entities are present in the population for context but are not current investigation targets.

**Hover — Transient Identification**

The color applied to an entity being hovered. Communicates: "I am considering this entity." Hover state is transient and should not persist. Hover color is lighter or more translucent than Selected color.

**Reference / Benchmark — Population Average**

The color applied to reference lines, peer average lines, benchmark targets, and threshold markers. Communicates: "this is the population standard." Used in Trend, Peer Position, and Position History.

Reference color is muted, often rendered as a dashed line. It should be readable but not compete with entity data.

**Background — Investigation Canvas**

The background of the workspace. Communicates nothing except "this is the surface on which investigation occurs."

Background should not be pure white. A very slightly warm or cool neutral reduces eye strain during extended investigation sessions and gives the data layer visual depth. The background is the stage; it should disappear.

### 4.3 Color Use Prohibitions

- Never use more than three attention-state colors simultaneously in the Population Map (neutral, attention, critical at most, plus selected).
- Never use comparison palette colors for any purpose other than entity comparison.
- Never use the Healthy color for decoration, success messages, or UI confirmation states.
- Never use the Critical color for non-data elements (buttons, navigation, headers).
- Never introduce a new color without defining its semantic meaning in this document.

---

## 5. Typography Language

### 5.1 Typographic Hierarchy

Typography in the investigation workspace serves two functions: information hierarchy (what should management read first) and data legibility (are numbers easy to scan).

| Level | Element | Weight | Size relative | Alignment |
| --- | --- | --- | --- | --- |
| 1 | KPI headline value | Bold/Heavy | Large | Right |
| 2 | Entity name in Selection Stage | Semi-bold | Medium-large | Left |
| 3 | Section titles (Population Map, Peer Position, etc.) | Medium | Medium | Left |
| 4 | KPI label (what the headline value represents) | Regular | Medium-small | Left |
| 5 | Table body (Business Drivers, Signal History) | Regular | Medium-small | Left for labels, right for values |
| 6 | Supporting metadata (period, unit, data freshness) | Light | Small | Left or trailing |
| 7 | Axis labels | Light | Small | Positioned at axis |
| 8 | Tooltip content | Regular | Small | Left |
| 9 | KPI delta / trend indicator | Medium | Small | Right or trailing |

### 5.2 KPI Presentation Rules

**Headline value prominence:** The primary KPI value in a KPI Summary card is the visual anchor of that card. It should be immediately readable without searching.

**Unit and period clarity:** Every KPI value must display its unit (Rp, %, count, days) and its period (MTD, cumulative, current, last 30 days) without requiring the user to hover or look elsewhere.

**Delta indicators:** When a KPI shows change from a previous period, the delta is displayed adjacent to the headline value. Delta direction (up, down, stable) should be encoded in color (positive / negative / neutral semantic) and in an icon, never in color alone.

**Numerical emphasis rule:** When numbers are the primary information being communicated (Peer Position distribution, KPI Summary, Position History ranks), typographic weight should reinforce the number's importance. A percentile number should read more strongly than the label surrounding it.

### 5.3 Numerical Alignment

All numeric values in tables, KPI cards, and ranked lists are right-aligned. This is non-negotiable.

Right alignment of numbers enables the reading behavior that management relies on: scanning a column of numbers from top to bottom and immediately identifying the largest or smallest value by visual field position (rightmost for large, leftmost-within-column for small).

Left-aligned numbers in a table are a data presentation error.

### 5.4 Density Rules

The workspace uses management-grade density: denser than a consumer app, less compressed than a financial spreadsheet.

**Minimum line height for table rows:** 1.4× the font size.

**Minimum column spacing in comparative tables:** Sufficient to prevent values from appearing to belong to the adjacent column.

**Minimum font size:** 12px for primary content, 11px for metadata and axis labels, never below 11px. At font sizes below 11px, legibility fails for management users who are not working at high-resolution displays.

**Abbreviation rules:** Never abbreviate entity names in tables without a full tooltip. Never truncate KPI values. Abbreviate units consistently: Rp (Rupiah), % (percent), K (thousands), M (millions) — and always define these in a persistent visible legend or tooltip.

### 5.5 Font Choice Principles

This document does not specify a font family. It specifies typographic properties that the chosen font must support:

- Tabular numerals: digits must be equal-width for alignment in tables
- Multiple weight levels: at minimum regular, medium/semibold, bold
- Latin and numeral clarity at small sizes
- Neutral character: the font should not call attention to itself; it should serve the data

---

## 6. Visualization Language

### 6.1 Universal Visualization Rules

These rules apply to every visualization in the investigation workspace.

**One primary question per visualization.** Each visualization answers one management question. The title or heading of each visualization should be that question or its answer, not a chart type description.

**Population context is preserved.** Visualizations that show selected entities must also show the context from which those entities were selected. This applies directly to Population Map and Peer Position. Trajectory and Position History may use a peer reference line as a context substitute.

**Axis labels are business terms.** "X axis: MTD Omzet (Rp)" not "X axis: kpiOmzetMtd." Axis labels are part of the business language contract.

**Tooltips complete the visualization.** Every interactive data point must have a tooltip that provides: entity identity, the displayed value in full, the peer context (rank, percentile, or population average), and any relevant attention state.

**Empty states are informative.** If a visualization has no data for the selected entity (e.g., no signals in Signal History), the empty state communicates "No signals recorded for this entity" — not a generic loading state or a blank space.

### 6.2 Population Map Visualization Rules

The Population Map is the signature visualization of the investigation workspace. Its visual language is the most detailed specification in this document.

**Point representation:**

Each entity in the population is a point. The default visual form is a small filled circle. The size of the circle is the smallest size that remains individually selectable and hoverable.

Optional size encoding (bubble size) is available only in specific map presets where a third KPI dimension provides meaningful investigation value. Size encoding is never on by default for all presets. When used, size should encode only one additional dimension, and the size scale should be visually progressive and easy to read from the legend.

**Quadrant and zone separators:**

The Population Map divides the investigation space into meaningful management zones. For example, the Customer Risk Map divides into: High Omzet / High Outstanding (review priority), High Omzet / Low Outstanding (healthy performers), Low Omzet / High Outstanding (collection problem), Low Omzet / Low Outstanding (dormant or weak).

Zone separators are visual aids, not hard boundaries. They are rendered as light hairlines or subtle background tonal differences — never as heavy borders that create the impression of distinct boxes.

Zone labels are management-facing: "Growth Opportunity," "High Risk," "Healthy," "Under Review." They are placed within the zone, at reduced visual weight, so they are readable without dominating the map.

**Selection visualization:**

A selected entity's circle becomes larger, fully opaque, and applies the selection or comparison color. A label appears adjacent to the point (entity name). The surrounding population dims to approximately 40% opacity — present and readable, but clearly subordinate.

**Hover visualization:**

Hovered entity: circle enlarges slightly, tooltip appears. The hovered point is distinguished from both selected and neutral states. The hover state disappears immediately when the cursor leaves the point or tooltip area.

**Outlier marking:**

Major outliers may be automatically labeled without selection. The outlier threshold is determined by the Preset Engine, not by the visualization layer. Labels for major outliers follow the same label style as selected entities but at slightly lower weight.

**Axis behavior:**

Axes are labeled at major gridlines only. Minor gridlines are not shown unless the zoom level demands them. Zero lines are visible when zero is a meaningful threshold for the displayed KPI. Axis scales are consistent within a preset to allow session-to-session comparison.

### 6.3 Peer Position Visualization Rules

Peer Position answers: "Is the selected entity normal, unusual, or extreme for one KPI within its peer group?"

**Default form:** Histogram (bar distribution) showing the peer population grouped into value bands. The selected entity is marked with a distinct vertical indicator (a colored line or arrow) at its position in the distribution.

**What the histogram shows:**
- The shape of the peer distribution (concentrated, spread, skewed)
- The selected entity's exact position
- An optional percentile annotation: "This entity is above 85% of peers"
- An optional range annotation: "Peer range: Rp 12M – Rp 890M"

**Multi-entity comparison in Peer Position:**
When multiple entities are selected, each entity marker appears at its own distribution position using its comparison color. The distribution itself remains neutral (it represents the full peer group, not the selected entities).

**Bin count guidance:** The number of histogram bins should reflect the distribution meaningfully. Too few bins (5) obscure distribution shape. Too many bins (50) produce noise. The default bin count is derived from the data range and the number of eligible peers.

**Secondary form — Box plot:**
Available as an optional compact view for users who want to see the statistical spread (median, quartiles, extremes) with the selected entity marked. Box plot is always secondary; histogram is the default.

### 6.4 Trajectory Visualization Rules

Trajectory answers: "Is the situation improving, deteriorating, temporary, or persistent?"

**Form:** Line chart with time on the horizontal axis. One primary KPI per view. Multiple entities overlaid as separate lines using comparison colors.

**Time axis:** Monthly periods are the default granularity. The visible window is the most recent 6–12 months. Future periods are not shown.

**Reference line:** An optional peer average reference line appears as a muted, dashed horizontal reference. It shows the peer group average for the selected KPI across the visible time window. Management can read the selected entity's line against the peer average to assess relative trajectory.

**Target line:** Where a KPI has a defined target (e.g., salesman achievement %), an optional target line appears at the target value. Distinct from the peer average line in visual form.

**Y-axis:** Scaled to the data range of the selected entities plus a proportional buffer. The Y-axis should not start at zero if zero is far from the data range and would compress the visible variation into a small portion of the chart area.

**Smoothing:** No smoothing is applied by default. Monthly values are plotted exactly. Smoothed trends are a potential advanced option but are not the default because they mask period-specific anomalies that may be management-relevant.

### 6.5 Signal History Visualization Rules

Signal History answers: "Are management signals new, recurring, resolved, or chronic?"

**Form:** A structured table or matrix of signals. Each row is a signal type. Columns represent months or state transitions. The signal state (active, resolved, recurring, new) is encoded in color and text.

**Signal state encoding:**
- Active signals: attention color
- Newly active signals (first occurrence): distinct marker
- Recurring signals (appeared, resolved, reappeared): visual continuity indicator
- Resolved signals: recessed/neutral color
- Chronic signals (persisted for many consecutive periods): critical color or intensity gradient

**Multi-entity comparison:** When comparing entities, Signal History panels are presented side-by-side per entity (not merged into one table). Signals are not comparable across entities in a merged view because signal configurations may differ by entity type.

### 6.6 Position History Visualization Rules

Position History answers: "Did the entity's rank among peers change meaningfully?"

**Form:** Line chart with rank on the Y-axis and time on the X-axis. Rank 1 is at the top of the Y-axis. The Y-axis is inverted relative to value charts because a lower rank number is better.

**Y-axis population context:** The Y-axis should show total population size so that rank 12 of 2,100 reads differently from rank 12 of 14.

**Entity paths:** Selected entities are shown as lines over time. Each line uses the comparison color of the corresponding entity.

**Peer movement context:** An optional band can show the rank range within which the majority of the peer population moved during the visible window. This provides context for whether a rank change is exceptional or routine.

### 6.7 Business Drivers Visualization Rules

Business Drivers answers: "Which related entities explain this situation?"

**Form:** Ranked table with embedded horizontal bars. Each row contains: entity identity (code, name), value, rank within the driver context, and an embedded horizontal bar representing the value magnitude.

**Embedded bar behavior:** The bar is scaled within the driver block, not across driver blocks. The longest bar in each block represents the highest value in that block. This prevents cross-block comparison that would require an impossible shared scale.

**Relationship block structure:** Each relationship type gets its own block with a clear title: "Assigned Salesman," "Top 5 Items by Omzet," "Top 3 Principals by Purchase." Blocks are presented in investigation priority order (as defined in the Product Vision).

**Navigation affordance:** Each entity row in Business Drivers is navigable to that entity's investigation workspace. A subtle navigation indicator (arrow or link styling) shows this affordance without cluttering the table.

**Table precision:** Business Drivers tables should not truncate entity names without a visible tooltip showing the full name. Numeric values should display in full or with clearly indicated abbreviation (K, M).

### 6.8 KPI Summary Visualization Rules

KPI Summary answers: "What are the current facts for this selected entity?"

**Form:** A horizontal row or compact grid of KPI cards. Each card contains: KPI label, headline value, unit, period, and optional delta indicator.

**Card count:** 6–8 KPIs per entity type. More than 8 cards creates scanning difficulty. Fewer than 4 cards may indicate insufficient investigation context.

**Comparison layout:** When comparing entities, KPI cards are aligned in columns per entity. The same KPI definition is used for all entities. Differences are readable through column juxtaposition.

**Evidence connection:** Each KPI card carries a subtle link to the source evidence for that KPI. The link is not prominent (it is part of the validation path, not the primary use of KPI Summary), but it is always accessible.

### 6.9 Performance Signature Visualization Rules

Performance Signature answers: "Is this selected entity balanced or lopsided across major dimensions?"

**Form:** Radar / spider chart with 4–6 dimensions per entity type.

**When to show:** Only when exactly one entity is selected. Never shown in comparison mode with 2+ entities (the overlay becomes unreadable). Never shown in Discovery Mode.

**Dimension count:** Minimum 4, maximum 6. Fewer than 4 dimensions produce a shape with too few points to be meaningful. More than 6 create a shape that becomes geometrically complex without adding management insight.

**Value encoding:** All dimensions must be normalised to the same scale for the radar to communicate shape meaningfully. The normalisation basis is defined in the Preset Engine per entity type.

**Visual placement:** Performance Signature is the last element in the Validation Stage. It is compact. It never occupies more than one-quarter of the viewport.

---

## 7. Interaction Language

### 7.1 Hover Behavior

Hover is a preview and identification mechanism. It does not change the state of the workspace.

**Population Map hover:**
On cursor entry to a point: the point enlarges slightly, a tooltip appears within 150ms. On cursor departure: the point returns to its previous size, the tooltip disappears immediately.

Tooltip content on hover:
- Entity name (full, not truncated)
- Axis KPI 1 value with label and unit
- Axis KPI 2 value with label and unit
- Population percentile for the primary axis KPI: "Above 78% of peers"
- Active attention signals: brief summary ("2 active signals: High Piutang, Overdue")

**Panel hover (tables, charts):**
On row or data point hover: the row or data point is highlighted. If the entity is present in the Population Map context, it is cross-highlighted in the Population Map.

**Cross-highlight behavior:**
Cross-highlighting is a transient state synchronized with hover. It disappears when hover ends. It does not modify selection state.

### 7.2 Selection Behavior

Selection is the mechanism by which investigation begins. Selected state persists until explicitly cleared.

**On selection:**
1. Population Map: selected entity highlighted, comparison color assigned, label appears, surrounding population dims.
2. Workspace transitions to Investigation Mode.
3. Selection Stage appears (KPI Summary and entity identity).
4. Context Stage begins loading (Peer Position, Trajectory, Signal History start populating).

**On deselection:**
1. Population Map: entity returns to neutral state.
2. If all entities are deselected: workspace transitions to Discovery Mode.
3. If other entities remain selected: comparison adjusts; deselected entity's comparison color is released.

**Selection indication:** The selected entity should have a clearly visible selection ring, fill, or scale change that is distinguishable from hover state and from peer entity appearance. The selection state must be unmistakable.

### 7.3 Animation and Transition

Animation in the investigation workspace is functional. It communicates state changes and maintains spatial orientation. It does not perform.

**Selection transition:** When an entity is selected, the transition from Discovery Mode to Investigation Mode involves:
- Population Map scale reduction: 200ms ease-in-out
- Investigation panels slide into view from below: 200ms staggered ease-out
- Selected entity highlight: 150ms

**Filter transition:** When a filter is applied, the Population Map redraws to show the filtered population:
- Non-matching entities fade to 0% opacity: 200ms
- Scope indicator updates: immediate

**Panel expansion:** When a Context Stage panel is expanded:
- Panel height expands from 0 to full height: 200ms ease-out
- Content fades in once panel reaches minimum legible height: 100ms delay

**No animation for:** Tooltip appearance/disappearance (should be near-instant), evidence link navigation (immediate page change), comparison color assignment (immediate).

### 7.4 Loading States

Loading is an unavoidable part of investigation when datasets are large or profiles are fetched on demand.

**Population Map loading:** The Population Map shows a skeleton state while the population data is being fetched. The skeleton shows the map bounding box with a subtle shimmer pattern. This state should last no more than 2 seconds under normal conditions.

**Profile data loading (after selection):** KPI Summary cards show skeleton card shapes while profile data loads. This state should last no more than 1 second.

**Progressive loading:** Context Stage panels load independently. Peer Position may load and display before Trajectory. Each panel shows its own skeleton state while loading, independently of other panels.

**Loading state principles:** Loading states must never show a blank white space. Blank white spaces create the impression that the workspace has crashed. A visible skeleton state communicates: "data is being retrieved, the workspace is functional."

### 7.5 Focus and Keyboard Navigation

Focus indicators must be visible and clearly differentiated from hover states.

**Tab order:** Entity type selector → Preset selector → Filter controls → Population Map → Entity search → (on entity selection: KPI Summary → Peer Position → Trajectory → Signal History → Business Drivers → Evidence links)

**Population Map keyboard navigation:** Arrow keys navigate between entities in the comparison set (cycling focus highlight). Enter selects a focused entity. The search field within the Population Map is keyboard-accessible.

**Panel navigation:** Tab navigates between investigation panels. Enter expands a collapsed panel. Escape collapses an expanded panel.

**Focus indicator:** A consistent focus ring is applied to all interactive elements. The ring uses a high-contrast color that is distinguishable from all semantic colors. Focus indicators are never hidden.

### 7.6 Filtering Interface Behavior

The filter panel is compact and attached to the Population Map boundary.

**Filter open state:** The filter panel expands when a filter control is activated. It does not push the Population Map content; it overlays a compact portion of the Population Map boundary.

**Filter application:** Each filter change immediately updates the Population Map. There is no "Apply" button for individual filter changes. A "Clear all filters" affordance resets all filters in one action.

**Filter scope indicator:** Prominently visible below the preset selector: "Showing [n] of [total] [entity type] — [active filter description]." This is always visible when any filter is active.

### 7.7 Tooltip Design

Tooltips in the investigation workspace are information-dense but structured.

**Tooltip anatomy:**
```
[Entity Name — bold]
[Axis KPI 1 Label]: [Value] [Unit]
[Axis KPI 2 Label]: [Value] [Unit]
[Peer context]: Above [X]% of [peer group]
[Attention state]: [Summary of active signals or "No active signals"]
```

**Tooltip positioning:** Tooltips appear adjacent to the cursor, positioned to avoid clipping against the viewport boundary. They do not obscure the entity being hovered if avoidable.

**Tooltip persistence:** Tooltips remain visible as long as the cursor is within the entity point or the tooltip area. They disappear immediately on cursor departure.

**Tooltip interaction:** Tooltips are non-interactive. Clicking within a tooltip area triggers the underlying entity interaction (selection), not a tooltip action.

---

## 8. Motion Language

### 8.1 Motion Philosophy

Motion in the investigation workspace is minimal and meaningful. Every animation either communicates a state change or maintains spatial orientation during a transition. No animation exists for aesthetic reasons.

The test for any animation: "If this animation were removed, would the user lose orientation or miss a state change?" If the answer is no, the animation should be removed.

### 8.2 Approved Motion Patterns

**Selection motion:**
Purpose: communicate that an entity has been selected and the workspace is entering Investigation Mode.
Form: Population Map scale reduces; investigation panels slide in from below. Selected entity scale increases and comparison color is applied.
Duration: 150–200ms. Ease-in-out.

**Deselection motion:**
Purpose: communicate that the investigation has been exited or an entity removed.
Form: Investigation panels slide out (collapse). Selected entity returns to neutral size. Population returns to full opacity.
Duration: 150ms. Ease-in.

**Panel expansion motion:**
Purpose: communicate that a new investigation panel is becoming available.
Form: Panel height animates from 0 to target height. Content fades in on completion.
Duration: 200ms. Ease-out.

**Filter application motion:**
Purpose: communicate that the visible population has changed.
Form: Non-matching entities fade out. Matching entities remain stable.
Duration: 200ms. Ease-in-out.

**Comparison addition motion:**
Purpose: communicate that a new entity has joined the comparison set.
Form: New entity gains comparison color and scale increase. All active investigation panels update (no transition animation for panel updates — they should update immediately to avoid confusion).
Duration: 150ms for entity highlight change.

**Navigation transition:**
Purpose: communicate that the workspace has shifted to a new entity type.
Form: Population Map fades out and fades in with the new entity population. Investigation panels clear with a 100ms fade.
Duration: 150ms fade out + 150ms fade in.

### 8.3 Prohibited Motion Patterns

**Elastic / bounce:** Animation that overshoots and returns. Never used in a data investigation tool.

**Decorative transitions:** Expanding circles, confetti, sparkles, or any animation not tied to a state change or spatial transition.

**Long animations:** No animation should exceed 350ms total. Animations longer than 350ms are perceptible as slowness in a professional tool.

**Attention-capturing motion:** Animation that draws the eye away from the data (pulsing indicators, rotating elements, continuous animation loops).

**Simultaneous competing animations:** When multiple things are happening, animate the most important one. Do not animate every element simultaneously.

---

## 9. Accessibility

### 9.1 Color Blindness

Color is never the sole encoding dimension for any investigation-critical information.

**Population Map:** Entity position and zone are the primary encoding dimensions. Color reinforces attention state and selection state. Shape or size variation can supplement color for critical-state encoding.

**Comparison colors:** The comparison palette is tested against deuteranopia and protanopia simulations. If any two comparison colors become indistinguishable in simulation, they must be replaced. Entity labels in comparison mode are always visible alongside color to prevent color-only identification.

**Signal History:** Signal states are encoded in color and text labels simultaneously. A colorblind user can read signal state from the text alone.

**Trajectory:** When comparing multiple entities in Trajectory, line style (solid, dashed, dotted) is used in addition to color to ensure discriminability.

### 9.2 Dense Information Accessibility

The Population Map with 2,100+ customer points creates a dense visual field.

**Minimum point size:** Interactive points (selectable, hoverable) must have a minimum touch/click target of 6px diameter, or a larger transparent interaction area around the visible point.

**Label collision avoidance:** Labels for selected and outlier entities must not overlap each other. The label rendering strategy must detect and resolve overlaps — typically by offsetting labels with leader lines.

**Filter as accessibility tool:** Peer group filters are not just an investigation refinement. For management users who find large population density overwhelming, filtering is the primary means of reducing visual complexity. The filter panel must be immediately accessible and easy to use.

### 9.3 Contrast Requirements

All text meets WCAG AA contrast standards at minimum. Headline KPI values and entity names that are critical to investigation should meet AAA standard.

Critical and Attention semantic colors must maintain sufficient contrast against both the workspace background and neutral entity points in the Population Map.

Reference lines and grid lines should be visible at the minimum required contrast for non-critical visual aids. They must not be so light as to be invisible, but they must not compete with data.

### 9.4 Keyboard Accessibility

The full investigation workflow must be completable without a mouse.

The Population Map is the most challenging element for keyboard accessibility. At 2,100+ points, individual point navigation by arrow key is impractical. The keyboard accessibility solution for the Population Map is:

1. Entity search via a keyboard-accessible search field within the Population Map boundary.
2. Search results can be navigated with arrow keys and selected with Enter.
3. The user can complete a full investigation of any specific entity entirely by keyboard.

Keyboard users should not feel that the workspace was designed without them. The entity-first navigation pattern (search → select → investigate) is a first-class keyboard experience.

### 9.5 Large Datasets and Legibility

At 3,000 item points, the Population Map may produce visual saturation in clustered zones.

**Label density rule:** At default zoom, labels appear only for selected entities, hovered entities, and extreme outliers. As the user zooms in, the label density threshold decreases — more labels become visible in the zoomed region.

**Zoom accessibility:** Keyboard-triggered zoom (+ and – keys) is available in addition to scroll-wheel zoom.

**Data freshness legibility:** Data freshness indicators (e.g., "Population data as of: 26 Jun 2026") must be clearly legible without being intrusive. They are always present but visually recessive.

---

## 10. Universal Design Rules

These rules are the foundation of the Entity Analytics visual design system. They apply to every current and future surface, visualization, interaction, and extension of the BTR Portal analytics product.

---

**Rule 1: One primary visual focus per screen.**

At every stage of the investigation, there is exactly one element that should receive the user's attention first. Every other element supports it. If two elements compete for visual dominance, one must be reduced.

---

**Rule 2: The Population Map is never a thumbnail.**

Even in Investigation Mode, the Population Map must be large enough to read population structure and locate the selected entity. It may not be reduced to a decorative element or a status badge.

---

**Rule 3: Color communicates meaning, always.**

Every color in the workspace has a defined semantic. Using a color for visual variety, decoration, or branding is prohibited. New colors may not be introduced without defining their semantic in this document.

---

**Rule 4: Numbers are right-aligned.**

All numeric values in tables, cards, and lists are right-aligned. This enables scan reading of numerical data. Left-aligned numbers in a data table are a legibility failure.

---

**Rule 5: Whitespace separates thought.**

Space between two elements communicates: "these are different things." Space is not decorative; it is structural. Regions of the workspace that belong together have less space between them than regions that belong to different investigation stages.

---

**Rule 6: Investigation always flows downward.**

The workspace flow is top-to-bottom: population, selection, context, explanation, validation. No panel from a lower stage appears above a panel from a higher stage.

---

**Rule 7: Users never lose context.**

The Population Map remains accessible throughout the investigation. Breadcrumbs are always visible. The active peer group scope is always displayed. The selected entity identity is always visible while the investigation panels are open.

---

**Rule 8: Comparison never removes the population.**

When multiple entities are selected for comparison, the full peer population remains visible in the Population Map. Comparison without population context is misleading.

---

**Rule 9: Every visualization answers one question.**

Before implementing any visualization, its primary management question must be stated. If the question cannot be stated in one sentence, the visualization is doing too much.

---

**Rule 10: Every interaction suggests the next investigation step.**

Selecting an entity on the Population Map reveals KPI Summary. KPI Summary cards link to evidence reports. Peer Position invites inspection of Trajectory. Business Driver rows navigate to related entities. Each interaction should feel like a natural continuation of investigation.

---

**Rule 11: Business language is the product interface.**

"Customer Risk Map," not "XY Scatter." "Peer Position," not "Histogram." "Signal History," not "Attention Log." "Business Drivers," not "Relationship Chart." The product speaks to management in management language at all times.

---

**Rule 12: Tooltips complete, not replace, visualization.**

Tooltips provide the precision detail that the visualization surface intentionally omits. They do not repeat what is already visually obvious. They add: exact value, peer context, evidence hint, or identity confirmation.

---

**Rule 13: The empty state is informative.**

When a visualization has no data, it explains why and what the user can do. "No signals recorded for this entity in the selected period." Never a blank space, never a generic loading state, never unexplained absence.

---

**Rule 14: Hover is transient; selection is persistent.**

Hover changes nothing in the workspace state. It provides information. Selection changes the workspace state. These two interaction modes must be visually distinguishable at all times.

---

**Rule 15: The investigation floor is the population.**

Every investigation begins by seeing the full population. No entry path may bypass the population context and begin with an entity profile in isolation. Alert-driven and deep-link entries still present the entity in population context.

---

**Rule 16: Peer normality before absolute value.**

Displaying an absolute KPI value (Rp 500M outstanding) without peer context is an incomplete investigation surface. Peer Position must be accessible immediately after KPI Summary to complete the normality picture.

---

**Rule 17: Trajectory before urgency.**

A management signal or outlier position should not imply immediate action without first confirming whether the situation is new, persistent, improving, or deteriorating. Trajectory is a required step in the investigation before any conclusion about severity.

---

**Rule 18: Evidence links are always available.**

From the moment an entity is selected, evidence links to source reports are accessible. Management should never have to exit the workspace without a clear path to the evidence that supports their conclusion.

---

**Rule 19: Performance Signature never leads.**

Performance Signature (Radar) summarises an entity after investigation. It never appears before population context, KPI facts, and at least partial context stage information have been reviewed. It is the last visualization in the investigation, not the first.

---

**Rule 20: Comparison is relative, not absolute.**

In comparison mode, the goal is not to determine which entity has a larger absolute value. It is to determine how each entity stands relative to the population and relative to each other. Comparison visualisations always include population reference.

---

**Rule 21: Motion earns its presence.**

Any animation that exists solely because it looks good must be removed. Animation is functional: it communicates state change, directs attention, or maintains spatial orientation. If it does none of these, it does not belong.

---

**Rule 22: Font size is a legibility contract.**

No content-bearing text in the workspace may be smaller than 11px. Critical data (entity names in selection, KPI headline values) must be legible without hovering. Zoom should never be required to read essential content.

---

**Rule 23: Every filter change is immediate.**

Filters are exploratory tools. The feedback to a filter application must be immediate (< 300ms to visual change). Deferred filter application with an "Apply" button creates a cognitive gap between intent and feedback that breaks investigation flow.

---

**Rule 24: Data freshness is visible, not hidden.**

Every snapshot-based value in the workspace displays the data freshness date or period. This is non-negotiable because management decisions are time-sensitive. Stale data without visible freshness indication erodes trust.

---

**Rule 25: The workspace has no required reading order.**

While the investigation flow is designed to be top-to-bottom, management may enter any stage at any point. The workspace must not gate access to a lower stage because an upper stage has not been reviewed. The flow is a recommendation, not a lock.

---

**Rule 26: Consistency across entity types is absolute.**

Customer Risk Map, Inventory Health Map, Sales Performance Map, and Purchase Exposure Map follow the same visual language, interaction model, and panel structure. An entity-type-specific deviation in visual behavior requires explicit justification in this document.

---

**Rule 27: A high-risk entity is always visible.**

No filter, zoom, or interaction state may hide an entity with a Critical attention state unless the management user has explicitly excluded it. The workspace must prevent inadvertent hiding of the most important investigation targets.

---

**Rule 28: Reduce before you annotate.**

When a visualization is hard to read, the first instinct should be to reduce: fewer colors, fewer labels, smaller point range, filtered population. Annotation (callouts, arrows, labels, legend expansions) is a last resort after reduction has failed to achieve clarity.

---

**Rule 29: Tables preserve precision; charts add magnitude.**

Relationship tables (Business Drivers) are evidence-facing. They preserve entity codes, names, ranks, and exact values. Embedded bars add magnitude comparison without replacing the table. The table is the evidence; the bar is the aid.

---

**Rule 30: Future capabilities do not require workspace redesign.**

Population Review Board, Business Event Timeline, and Decision Support are designed to extend the workspace in reserved structural positions. A future implementer adding these capabilities should not need to redesign any existing stage or change any existing panel. Extension is additive only.

---

**Rule 31: The design system is the source of truth.**

When in doubt about a visual decision — color semantics, element hierarchy, spacing rules, motion duration, label strategy — this document takes precedence. If a decision not covered by this document is needed, it must be documented here before implementation.

---

**Rule 32: Management usability is the standard, not peer review.**

The measure of success for every visual decision is: can a management user, without data visualisation expertise, use this workspace to investigate a business situation and reach a defensible conclusion? If the answer requires design or analytical expertise, the design has failed.

---

## Appendix A: Color Semantic Summary

| Semantic role | Population Map use | Investigation panel use | Prohibited uses |
| --- | --- | --- | --- |
| Neutral | Default entity point | Unselected entities, inactive rows | UI elements, text, dividers |
| Selected | Active selected entity | KPI Summary border, section highlight | Background, icons, buttons |
| Comparison (4 colors) | Multi-selected entities | Trend lines, Position History paths, KPI columns | Anything other than comparison encoding |
| Attention / Risk | Entities with active signals | Signal History active state | Success indicators, confirmation UI |
| Critical | Extreme outliers or most severe signals | Signal History chronic state | Risk gradients for more than one severity level |
| Healthy / Positive | Entities in favorable zones | Positive delta indicators | Confirmation messages, progress bars |
| Historical / Inactive | Dormant or resolved entities | Resolved signals, inactive rows | Loading states, disabled UI |
| Hover | Transient hover state | Row hover in tables | Selection state |
| Reference / Benchmark | Not used directly | Peer average line, target line | Entity encoding |
| Background | Map canvas | Panel backgrounds | Text, interactive elements |

---

## Appendix B: Visualization-to-Question Mapping

| Visualization | Management question answered | When to show |
| --- | --- | --- |
| Population Map | Which entities deserve attention? | Always (Discovery Mode) |
| Peer Position | Is this entity normal, unusual, or extreme for one KPI? | On entity selection |
| KPI Summary | What are the current facts? | On entity selection |
| Trajectory | Is the situation improving, deteriorating, or persistent? | On entity selection, expand to view |
| Signal History | Are signals new, recurring, resolved, or chronic? | On entity selection, expand to view |
| Position History | Did the entity's rank change meaningfully? | On entity selection, expand to view |
| Business Drivers | Which related entities explain this? | On entity selection, expand to view |
| Evidence Links | What source data proves the conclusion? | Always after entity selection |
| Performance Signature | Is the entity balanced or lopsided? | Single entity selected only, Validation Stage |

---

## Document Control

| Version | Date | Author | Change |
| --- | --- | --- | --- |
| 1.0 | 2026-06-27 | Lead Product Architect | Initial visual design system reference for M32R |
