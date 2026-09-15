# ADR-002 — Authenticated JWT Identity on All Cloud Write Endpoints

| Field | Value |
| --- | --- |
| Status | Accepted |
| Date | 2026-09-15 |
| Decided via | Barcode Registry feasibility, GAP-008 |
| Assessment | `docs/work/barcode-registry/BARCODE-REGISTRY-FEASIBILITY-ASSESMENT.md` |
| Related | `ADR-003-mobile-authentication-against-cloud-api.md` |

## Context

The Cloud API (`j06-pkl-btrade-api`) currently configures JWT Bearer validation
but does not enforce it: no controller or action carries `[Authorize]`, there is
no token-issuing/login endpoint, CORS allows any origin, and mobile/sync clients
use cleartext HTTP. The API is effectively anonymous and unauthenticated-writable.

Barcode Registry introduces mobile-originated writes (Barcode Registration
Requests) and therefore cannot inherit an anonymous write surface. Because the
Main Office validates and owns the registry, a forged or unattributed request
would pollute the transport queue and could be mistaken for legitimate user
intent.

## Decision

1. **All cloud write endpoints must require an authenticated JWT identity.**
   Write operations include any state-changing command (for example order,
   check-in, customer location, packing order, and Barcode Registration Requests).
2. **Anonymous write operations are prohibited.** A missing, expired, or invalid
   token must produce an authentication failure; the command must not execute.
3. Authentication is enforced at the API boundary, not only in clients.
4. Enforcement applies to existing write endpoints as well as new Barcode
   Registry endpoints, so the guarantee is API-wide rather than barcode-specific.
5. Read (lookup) endpoint protection is not decided by this ADR; it follows the
   platform's overall endpoint-protection policy.

## Consequences

### Positive

- Establishes a single, uniform security boundary for all remote writes.
- Removes the anonymous-write exposure that would otherwise undermine the Main
  Office authority model.
- Provides an authenticated actor identity for audit and for future tenant binding.

### Negative

- Existing mobile and sync clients that send no `Authorization` header (for
  example `BTrade3`, `j07-btr-gudang`, `j07-btrade-sync`) will fail until updated.
- Requires a token-issuing mechanism and coordinated client changes before any
  authenticated write path can go live.

### Neutral

- No change to the Barcode Registry domain or business rules.

## Alternatives Considered

| Alternative | Rejected because |
| --- | --- |
| Leave write endpoints anonymous; validate only in the Main Office | Requests remain unauthenticated in transit; violates Main Office authority and audit expectations |
| API key / shared secret header | Weaker identity semantics than JWT; no per-user identity for audit |
| Enforce authentication only on new barcode endpoints | Leaves the rest of the API anonymously writable and the guarantee incomplete |

## Compliance

Satisfies the GAP-008 resolution for authentication enforcement.
