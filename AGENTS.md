# AGENTS.md

## Purpose

This document is the entry point for all AI agents working within the BTR repository.

Before performing analysis, design, implementation, or documentation updates, read this document first.

The purpose of this document is to:

* Explain the repository knowledge structure
* Define the available specialized agents
* Define the expected workflow
* Guide context retrieval

This document does not contain business knowledge.

Business knowledge is maintained in dedicated artifacts under `docs/`.

---

# Repository Philosophy

BTR follows a Simple and Structured knowledge model.

Knowledge is divided into three categories:

## Permanent Knowledge

Long-lived business and system knowledge.

Permanent knowledge is continuously updated and represents the current truth of the system.

Examples:

* `docs/foundation/PRODUCT.md`
* `docs/foundation/DOMAIN.md`
* `docs/foundation/LANDSCAPE.md`
* `docs/foundation/WORKFLOW.md`
* Feature artifacts under `docs/features/`

---

## Temporary Work

Short-lived artifacts created during development.

Examples:

* `docs/work/<task>/implementation-plan.md`
* `docs/investigations/<name>.md`
* `docs/audits/<name>.md`
* `docs/migrations/<task>/migration-plan.md`

Temporary work should be removed after relevant knowledge has been incorporated into Permanent Knowledge.

---

## Workforce

Specialized AI agents and reusable skills.

Examples:

* `docs/agents/analyst-agent.md`
* `docs/agents/architect-agent.md`
* `docs/agents/implementer-agent.md`

---

# Knowledge Retrieval Order

When working on a task, retrieve context in the following order.

## Level 1 - Foundation Knowledge

Read:

1. `docs/foundation/PRODUCT.md`
2. `docs/foundation/DOMAIN.md`
3. `docs/foundation/LANDSCAPE.md`
4. `docs/foundation/WORKFLOW.md`

Purpose:

* Understand the product
* Understand business terminology
* Understand ownership and system involvement
* Understand business workflows

---

## Level 2 - Feature Knowledge

Read relevant feature artifacts.

Examples:

```text
docs/features/faktur/
docs/features/sales-order/
docs/features/customer/
docs/features/inventory/
docs/features/retur/
```

Each feature directory typically contains `feature.md`.

Purpose:

* Understand feature-specific behavior
* Understand feature-specific business rules

---

## Level 3 - Source Code

Read source code only after understanding the business context.

Source code explains implementation.

Artifacts explain intent.

When conflicts exist:

1. Feature artifacts take precedence over assumptions.
2. Foundation artifacts take precedence over assumptions.
3. Source code may contain legacy behavior and should be interpreted carefully.

---

# Agent Selection

Choose the appropriate agent based on the task.

---

## Analyst Agent

Use when:

* Understanding requirements
* Clarifying business needs
* Defining feature scope
* Producing feature specifications
* Identifying affected business areas
* Identifying affected workflows

Reference:

```text
docs/agents/analyst-agent.md
```

Primary Output:

```text
docs/features/<feature>/feature.md
```

---

## Architect Agent

Use when:

* Designing solutions
* Performing impact analysis
* Evaluating implementation approaches
* Identifying affected modules
* Creating implementation plans
* Assessing risks

Reference:

```text
docs/agents/architect-agent.md
```

Primary Output:

```text
docs/work/<task>/implementation-plan.md
```

---

## Implementer Agent

Use when:

* Modifying source code
* Creating database changes
* Implementing UI changes
* Implementing integrations
* Writing tests
* Updating documentation

Reference:

```text
docs/agents/implementer-agent.md
```

Primary Output:

```text
Source Code
Tests
Updated Artifacts
```

---

# Standard Workflow

All feature work should follow the same sequence.

```text
Requirement
    ↓
Analyst
    ↓
docs/features/<feature>/feature.md
    ↓
Architect
    ↓
docs/work/<task>/implementation-plan.md
    ↓
Implementer
    ↓
Code + Updated Artifacts
```

---

# Working Principles

## Business First

Understand the business before proposing solutions.

---

## Preserve Existing Behavior

BTR is a mature business system.

Existing behavior should be considered intentional unless proven otherwise.

Do not redesign workflows solely because they appear unusual.

---

## Simplicity First

Prefer the simplest solution that satisfies the requirement.

Avoid unnecessary abstractions.

---

## Knowledge Before Code

Read artifacts before modifying code.

Understanding the business is more important than understanding the implementation.

---

## Keep Knowledge Updated

When business knowledge changes:

* Update feature artifacts under `docs/features/`
* Update foundation artifacts under `docs/foundation/` if necessary

Code and knowledge must remain consistent.

---

# Success Criteria

A successful AI contribution:

1. Understands the business context.
2. Uses the appropriate specialized agent.
3. Produces the correct artifact.
4. Preserves existing business behavior.
5. Keeps knowledge synchronized with implementation.

---

# Agent Invocation Rules

Agents are internal workforce roles.

Users should describe the desired outcome rather than explicitly selecting agents.

The system should automatically select the appropriate agent workflow based on the request.

---

## Solution Request

When the user requests:

* Give me a solution
* Analyze this problem
* Recommend an approach
* Design a feature
* Create a plan
* How should we implement this?

Execute:

```text
Analyst
    ↓
Architect
```

Output:

```text
docs/features/<feature>/feature.md
docs/work/<task>/implementation-plan.md
```

Stop after producing the implementation plan.

Do not perform implementation.

User approval is required before implementation.

---

## Implementation Request

When the user requests:

* Implement implementation-plan-xxxx.md
* Implement this plan
* Write the code
* Apply the approved solution
* Execute the implementation

Execute:

```text
Implementer
```

Input:

```text
docs/work/<task>/implementation-plan.md
```

Output:

```text
Source Code
Tests
Updated Artifacts
```

---

## Investigation Request

When the user requests:

* Investigate this bug
* Analyze this issue
* Find root cause

Execute:

```text
Analyst
    ↓
Architect
```

Output:

```text
docs/investigations/<name>.md
root cause analysis
recommended solution
implementation plan (if applicable)
```

Do not perform implementation.

---

## Documentation Request

When the user requests:

* Create feature documentation
* Update knowledge artifacts
* Document this feature

Execute:

```text
Analyst
```

Output:

```text
docs/features/<feature>/feature.md
artifact updates
```

---

## Explicit Agent Request

If the user explicitly requests a specific agent, honor the request.

Examples:

```text
Use Analyst Agent
Use Architect Agent
Use Implementer Agent
```

Only execute the requested agent.
