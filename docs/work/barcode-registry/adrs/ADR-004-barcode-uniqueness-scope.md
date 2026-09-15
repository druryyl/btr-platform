# ADR-004 — Barcode Uniqueness Scope

| Field | Value |
| --- | --- |
| Status | Accepted |
| Date | 2026-09-15 |
| Decided via | Barcode Registry feasibility, GAP-010 |
| Assessment | `docs/work/barcode-registry/BARCODE-REGISTRY-FEASIBILITY-ASSESMENT.md` |

## Context

The Barcode Registry domain originally stated that barcode values must be
**globally unique** (BR-001). The platform is multi-tenant: the Main Office
Server is a single authoritative Product Master database, while the Cloud API
(`j06-pkl-btrade-api`) stores data for multiple Main Office tenants, keyed by
`ServerId`. A platform-global uniqueness rule would incorrectly forbid two
different tenants from using the same barcode value and would not match how the
Cloud read model is partitioned.

## Decision

1. **Barcode uniqueness is enforced within an authoritative Product Master
   scope**, not platform-globally.
2. **Main Office** enforces uniqueness within its own Product Master authority
   (its own database). The `btr.sql` unique constraint on `BarcodeValue` applies
   within that authority.
3. **Cloud read models enforce uniqueness using `(ServerId, BarcodeValue)`**, so
   different tenants may legitimately hold the same barcode value.
4. The domain rule is restated: from "globally unique" to **"unique within an
   authoritative Product Master scope"**.

## Consequences

### Positive

- Matches the existing multi-tenant (`ServerId`) partitioning of the Cloud read
  model.
- Allows distinct companies/tenants to reuse the same physical barcode value
  without collision.
- Removes a rule that could not be enforced correctly at the Cloud layer.

### Negative

- "Uniqueness" is now context-dependent; documentation and validation messages
  must state the scope explicitly to avoid confusion.
- A barcode value carries no cross-tenant global identity, so any future
  cross-tenant feature cannot assume global resolution.

### Neutral

- Does not change the Main Office single-authority behavior; its unique
  constraint remains valid within its scope.

## Alternatives Considered

| Alternative | Rejected because |
| --- | --- |
| Platform-global uniqueness across all tenants | Contradicts Cloud `ServerId` partitioning; would block legitimate cross-tenant reuse |
| No uniqueness enforcement at all | Permits duplicate mappings within one Product Master, violating BR-003/BO-001 |

## Compliance

Satisfies the GAP-010 resolution. Supersedes BR-001's "globally unique" wording
and the Barcode aggregate consistency boundary in
`docs/work/barcode-registry/BARCODE-REGISTRY-DOMAIN.md`.
