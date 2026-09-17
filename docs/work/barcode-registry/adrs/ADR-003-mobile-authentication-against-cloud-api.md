# ADR-003 — Mobile Authentication Against the Cloud API

| Field | Value |
| --- | --- |
| Status | Accepted |
| Date | 2026-09-15 |
| Decided via | Barcode Registry feasibility, GAP-008 |
| Assessment | `docs/work/barcode-registry/BARCODE-REGISTRY-FEASIBILITY-ASSESMENT.md` |
| Related | `ADR-002-authenticated-jwt-write-endpoints.md` |

## Context

Mobile clients currently do not present an API credential. `BTrade3` uses Google
Sign-In only as a local gate and sends no `Authorization` header; its Retrofit
client posts to the Cloud API anonymously. ADR-002 requires all cloud write
endpoints to require an authenticated JWT identity, so mobile clients must obtain
and present a token.

The Cloud API is `pkl.btrade.api` (`j06-pkl-btrade-api`). Barcode Registry
requires mobile-originated writes (Barcode Registration Requests) to carry a
verifiable identity.

## Decision

1. **Mobile applications authenticate against `pkl.btrade.api` before submitting
   operational commands.** No operational write (including Barcode Registration
   Requests) is submitted without first obtaining a valid token.
2. Authentication uses the Cloud API's token mechanism (JWT), obtained via the
   token-issuing mechanism decided in GAP-008 and enforced per ADR-002.
3. The mobile client presents the token on every subsequent write request.
4. Google Sign-In (or any local sign-in) does not by itself satisfy this
   requirement; it may inform local identity but does not replace the API token.

## Consequences

### Positive

- Every mobile-originated command is attributable to an authenticated identity.
- Consistent with ADR-002's API-wide prohibition of anonymous writes.
- Enables future tenant binding of the authenticated identity to a `ServerId`.

### Negative

- Mobile requires a login/token-refresh flow and secure token storage.
- Failed or expired tokens must be handled with a defined retry/re-authenticate
  path so offline-captured registrations are not lost.
- The mobile token endpoint and its transport must not rely on cleartext HTTP.

### Neutral

- Does not, by itself, define how the authenticated identity maps to a tenant
  (`ServerId`); that is resolved separately by ADR-007.

## Alternatives Considered

| Alternative | Rejected because |
| --- | --- |
| Continue anonymous mobile submissions | Violates ADR-002; no attributable identity for registered barcodes |
| Authenticate only when online, allow offline anonymous queueing | Queue may store unauthenticated intent; token must be obtained before submission (queue locally, authenticate at send time) |
| Per-device static credential | Weaker identity semantics; no per-user attribution |

## Compliance

Satisfies the GAP-008 resolution for mobile authentication.
