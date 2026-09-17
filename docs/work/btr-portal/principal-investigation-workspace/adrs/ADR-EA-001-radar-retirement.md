# ADR-EA-001 — Retire Performance Signature (Radar) from Entity Analytics

| Field | Value |
| --- | --- |
| Status | Accepted |
| Date | 2026-09-11 |
| Decided via | Principal Investigation Workspace feasibility, GAP-010 |
| Assessment | `docs/work/btr-portal/principal-investigation-workspace/FEASIBILITY-ASSESSMENT.md` |

## Context

The Principal Investigation Workspace feasibility (GAP-010) required a dual-lens
Performance Signature model. The approved direction instead removes the
Performance Signature (Radar) from Entity Analytics entirely.

## Decision

Entity Analytics no longer uses Performance Signature (Radar) for any entity type.

Affected entities:

- Customer
- Salesman
- Item
- Principal

The Principal Investigation Workspace will not implement a Principal
Performance Signature, and no replacement radar visualization is required.

Investigation shall rely on:

- KPI Summary
- Population Position
- Trajectory
- Business Drivers
- Relationships
- Attention Signals
- Evidence

## Consequences

- The shared signature surface (`ValidationStagePanel` embedding, radar profile
  sections, radar compare views) is retired once for all entity types.
- Radar axis registration and L5 signature composition leave planning scope;
  no dual-lens signature model is designed.
- Planning and review must verify that no entity profile or evidence route
  depends on the retired signature surface.
- Existing L5 snapshot history is not deleted by this decision; any retention
  or cleanup is an implementation concern outside this ADR.
