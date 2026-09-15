# ADR-006 — Cache-First Mobile Barcode Lookup

| Field | Value |
| --- | --- |
| Status | Accepted |
| Date | 2026-09-15 |
| Decided via | Barcode Registry feasibility, GAP-014 |
| Assessment | `docs/work/barcode-registry/BARCODE-REGISTRY-FEASIBILITY-ASSESMENT.md` |

## Context

Mobile clients must resolve scanned barcodes into Items during operational
warehouse activities (Stock Opname, Receiving, Picking, Retur, Warehouse
Transfer). Connectivity in warehouse/field conditions cannot be assumed. A
network round-trip per scan is both slow and impossible offline, and the Cloud
API is a read model, not the authority.

Barcode Registry already defines a one-way Main Office → Cloud → Mobile
synchronization of active barcode mappings (GAP-003/GAP-004). Mobile clients
already maintain local caches (Room on `BTrade3`, local SQL Server on
`j07-btr-gudang`).

## Decision

1. **Mobile barcode lookup is cache-first.** The locally synchronized barcode
   cache is the **primary** lookup mechanism and resolves barcodes without
   network connectivity.
2. **Cloud APIs are synchronization sources, not runtime dependencies for
   barcode scanning.** A scan must not require a live Cloud call.
3. Barcode-enabled screens are introduced for Desktop and Mobile.
4. An **unknown barcode** (not present in the local cache) may be captured as an
   **offline registration request** and synchronized later (GAP-007, BR-012).

## Consequences

### Positive

- Barcode scanning works offline and with low latency.
- Removes runtime coupling between operational scanning and Cloud availability.
- Enables capture of unknown barcodes as registration requests while in the field.

### Negative

- Lookup correctness depends on the freshness of the local cache.
- Cache refresh/versioning and stale/retired barcode handling must be defined.
- Clients must store barcode and Barang caches locally (see ADR-005).

### Neutral

- Does not change the Main Office authority or the one-way sync direction; the
  cache is a downstream copy of Active barcode mappings.

## Alternatives Considered

| Alternative | Rejected because |
| --- | --- |
| Online-only barcode lookup | Breaks the offline operation requirement; adds per-scan latency |
| Hybrid random online calls with cache fallback | Makes the Cloud a runtime dependency; unpredictable offline behavior |
| Cache as fallback only (online first) | Slower and fragile; contradicted by offline requirement |

## Compliance

Satisfies the GAP-014 resolution and supports GAP-003/GAP-007.
