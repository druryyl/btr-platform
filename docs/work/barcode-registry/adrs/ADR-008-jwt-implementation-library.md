# ADR-008 — JWT Implementation Library

| Field | Value |
| --- | --- |
| Status | Accepted |
| Date | 2026-09-16 |
| Decided via | Barcode Registry implementation, S3.6 (token issuance) |
| Plan | `docs/work/barcode-registry/BARCODE-REGISTRY-IMPL-PLAN.md` §6 S3.6 |
| Architecture | `docs/work/barcode-registry/BARCODE-REGISTRY-ARCHITECTURE.md` §9.1 |
| Related | `ADR-002-authenticated-jwt-write-endpoints.md`, `ADR-007-tenant-isolation-from-authenticated-identity.md` |

## Context

The Cloud API (`j06-pkl-btrade-api`) issues JWTs so that BGud and the
synchronization client can present an authenticated identity (ADR-002, ADR-007).
Token issuance needs a token handler.

The solution's dependency graph mixes two IdentityModel generations:
`MediatR` 13.0.0 brings `Microsoft.IdentityModel.JsonWebTokens` 8.0.1 →
`Microsoft.IdentityModel.Tokens` 8.0.1, while
`Microsoft.AspNetCore.Authentication.JwtBearer` 6.0.36 brings
`System.IdentityModel.Tokens.Jwt` 6.35.0.

During S3.6, issuing a token with `JwtSecurityTokenHandler` failed at runtime on
the net6.0 target:

```text
System.TypeLoadException: Could not load type
'Microsoft.IdentityModel.Json.JsonConvert' from assembly
'Microsoft.IdentityModel.Tokens, Version=8.0.1.0'
```

The legacy handler (6.x) expects JSON types removed in IdentityModel 8.x, so it
cannot run against the resolved `Microsoft.IdentityModel.Tokens` 8.0.1.

## Decision

Issue JWTs with `JsonWebTokenHandler` (`Microsoft.IdentityModel.JsonWebTokens`).

- Algorithm, issuer, audience, and signing key come from the existing `Jwt`
  configuration section (unchanged).
- Claims remain as approved by Architecture §9.1: `sub`, `role`, `locationId`,
  `serverId`.
- No new package reference is introduced; the library is already present in the
  graph via MediatR 13.

## Consequences

### Positive

- Token issuance works at runtime on the net6.0 target.
- Compatible with the resolved IdentityModel 8.x dependency graph.
- Standard HS256 JWT; wire format and claims are unaffected.

### Negative

- Two IdentityModel handler APIs coexist: `JsonWebTokenHandler` for issuance,
  while `JwtBearer` 6.0.36 validation defaults to `JwtSecurityTokenHandler`.
  Token validation must be aligned with the same generation when the
  authenticated endpoint path is exercised (S3.7).

### Neutral

- Does not change the authentication or tenant-binding decisions (ADR-002,
  ADR-007) or any domain/business rule.

## Alternatives Considered

| Alternative | Rejected because |
| --- | --- |
| `JwtSecurityTokenHandler` (primary `System.IdentityModel.Tokens.Jwt`) | Threw `TypeLoadException` at runtime against IdentityModel 8.x |
| Upgrade `JwtBearer` to 8.x and use the 6.x/8.x handler consistently | `JwtBearer` 8.x requires net8.0; the API targets net6.0 |
| Pin `Microsoft.IdentityModel.Tokens` to 6.35.0 | Conflicts with MediatR 13's `Microsoft.IdentityModel.JsonWebTokens` 8.0.1 dependency; produces a package downgrade and an inconsistent graph |

## Compliance

Supports ADR-002 (authenticated identity) and ADR-007 (tenant from authenticated
identity). Recorded from the S3.6 review finding INFO-002
(`BARCODE-REGISTRY-IMPL-PLAN.md` §8.1).
