# KPI Explanation Skill

## Purpose

Transform dashboard metrics, KPI cards, alert counters, forecast indicators, rankings, queues, and analytical measures into business-oriented explanations that help owners and managers understand:

* What the metric means
* Why it matters
* What business decision it supports

The objective is **business understanding**, not technical documentation.

---

## Inputs

The user may provide:

* A dashboard name
* A section name
* One or more KPI names
* Additional domain documents

Example:

> In Sales Forecast Dashboard there is a section containing:
>
> * Total Target
> * Total Achievement
> * Achievement %
>
> Please explain them.

---

## Required Knowledge Sources

Before writing explanations:

1. Review the dashboard context and surrounding KPIs.
2. Review available business/domain documentation.
3. Review `@kpi-encyclopedia.html`.
4. Attempt to identify:

   * KPI Canonical Code
   * KPI Canonical Name
   * KPI Category
   * Existing KPI definition

If the KPI exists in the KPI Encyclopedia, include the canonical information.

If no canonical KPI exists, explicitly state:

> KPI Canonical : Not Found

Do not invent KPI codes.

---

## Explanation Philosophy

Write from the perspective of:

* Business Owner
* Director
* General Manager
* Department Manager

Do NOT write from the perspective of:

* Software Developer
* Database Administrator
* Data Analyst
* System Implementer

The explanation should answer:

> "Why should management care about this number?"

rather than:

> "How is this number stored or calculated in the system?"

Avoid:

* Table names
* Database fields
* SQL terminology
* API terminology
* Source code references
* Internal implementation details

---

## Recommended Structure

Use the following structure as the default format.

### <Serial Number> <KPI Name>

1. **KPI Canonical:** <Code and Name from KPI Encyclopedia or Not Found>

2. **Question Answered:**
What business question does this KPI help management answer?

3. **Definition:**
Explain what is being measured.

Include formulas, classification rules, thresholds, or trigger conditions only when they help business users understand the KPI.

4. **Business Meaning:**
Explain why this KPI matters.

Focus on:

* Business impact
* Risk
* Opportunity
* Operational significance
* Financial significance

5. **How To Interpret:**
Explain how management should read the value.

Examples:

* What does a high value mean?
* What does a low value mean?
* What does an increasing trend mean?
* What does a decreasing trend mean?
* When should management pay attention?

---

## Flexibility Rule

The structure above is a guideline, not a mandatory template.

You may adapt the explanation when the KPI naturally requires a different format.

Examples:

* Forecast KPIs may require forecast assumptions.
* Risk indicators may require warning interpretation.
* Queue metrics may require action-oriented explanation.
* Composite scores may require component explanation.
* Ratios and percentages may require benchmark interpretation.

The goal is clarity, not template compliance.

---

## Quality Rules

A good explanation:

* Uses business language.
* Helps management make decisions.
* Explains why the KPI exists.
* Connects the KPI to business outcomes.

A poor explanation:

* Merely repeats the formula.
* Reads like database documentation.
* Focuses on implementation details.
* Explains how the software works instead of how the business works.

---

## Expected Output Style

Produce one explanation per KPI.

Keep explanations concise but complete.

Prefer practical business interpretation over mathematical discussion.

When possible, explain:

* what management should notice,
* what management should investigate,
* and what management should do next.
