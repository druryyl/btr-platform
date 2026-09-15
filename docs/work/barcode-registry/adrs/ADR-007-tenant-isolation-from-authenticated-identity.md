# ADR-007 — Tenant Isolation from Authenticated Identity

| Field | Value |
| --- | --- |
| Status | Accepted |
| Date | 2026-09-15 |
| Decided via | Barcode Registry feasibility, TQ-2 |
| Assessment | `docs/work/barcode-registry/BARCODE-REGISTRY-FEASIBILITY-ASSESMENT.md` |
| Related | `ADR-002-authenticated-jwt-write-endpoints.md`, `ADR-003-mobile-authentication-against-cloud-api.md` |

## Context

The Cloud API partitions data by `ServerId` (tenant). Historically, clients
supplied `ServerId` in request payloads or routes, so a client could choose any
tenant. With JWT authentication enforced on write endpoints (ADR-002) and mobile
clients authenticating against `pkl.btrade.api` (ADR-003), the authenticated
identity is now available and can determine the tenant.

A user is not permanently bound to a single `ServerId`. A person may work at an
Office or at a Gudang (warehouse), and the same user may operate at more than one
location over time. The tenant must therefore reflect the user's selected
operational location for the session, not a fixed attribute of the user account.

## Decision

1. **Tenant identity (`ServerId`) is derived from the authenticated user
   identity.** It is resolved server-side and is not a fixed, permanent attribute
   of the user.
2. **During authentication the user selects an operational location (Office or
   Gudang).** The authenticated session is bound to that selected location.
3. **`ServerId` is resolved server-side from the selected operational location**
   and is used for all tenant-scoped operations.
4. **Write operations must not accept `ServerId` from client payloads.** A
   client-supplied tenant value must be ignored or rejected.
5. **All tenant-scoped persistence and queries use the `ServerId` resolved from
   the authenticated session context.**
6. Subsequent API requests operate within the authenticated location context and
   do not carry a client-supplied `ServerId`.
7. JWT authentication identifies both the user and the bound location/tenant
   context. Barcode Registration, Sales Return, and future warehouse commands
   execute using the resolved `ServerId`.
8. **Existing legacy APIs that still require `ServerId` in the route may remain
   unchanged** and can be migrated later; they are out of scope for this
   decision.

### BGud location mapping (example)

| Gudang (location) | ServerId |
| --- | --- |
| Gudang Gamping | `JOGJA` |
| Gudang Concat | `JOGJA` |
| Gudang Magelang | `MGL` |

The mapping from operational location to `ServerId` is configuration/data, not
client input.

## Consequences

### Positive

- Prevents cross-tenant reads/writes and tenant spoofing.
- Removes client-controlled tenancy from new write paths.
- Models reality: a user selects where they are working for the session.
- Provides consistent tenant context for audit and persistence.

### Negative

- Authentication must include a location-selection step and the session/token
  must carry the bound location.
- A user switching locations must re-authenticate or re-bind the session; queued
  offline registrations belong to the location under which they were captured and
  must not be silently re-homed.
- New endpoints diverge from legacy endpoints that still take `ServerId` in the
  route, so two conventions coexist temporarily.

### Neutral

- Does not change legacy endpoint behavior; migration is deferred and optional.

## Alternatives Considered

| Alternative | Rejected because |
| --- | --- |
| Permanently bind each user to one `ServerId` | Users work at multiple locations over time; would require duplicate accounts |
| Continue accepting `ServerId` in payloads | Allows cross-tenant writes; defeats authentication guarantees |
| Accept `ServerId` but validate against the token | Still a client-controlled value with inconsistent handling |

## Compliance

Resolves TQ-2 and supports the GAP-008 authority and authentication decisions.
