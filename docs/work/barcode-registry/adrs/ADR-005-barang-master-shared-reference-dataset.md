# ADR-005 — Barang Master as a Shared Reference Dataset for Warehouse Mobility

| Field | Value |
| --- | --- |
| Status | Accepted |
| Date | 2026-09-15 |
| Decided via | Barcode Registry feasibility, GAP-013 |
| Assessment | `docs/work/barcode-registry/BARCODE-REGISTRY-FEASIBILITY-ASSESMENT.md` |

## Context

Warehouse mobility applications (for example `j07-btr-gudang`, and `BTrade3`)
require the ability to search and select an Item (Barang) independently of a
Packing Order. Today, `j07-btr-gudang` receives item data only embedded inside
packing-order payloads and has no standalone Barang lookup. `BTrade3` already
receives a full item list via `GET api/Brg/{serverId}`.

Barcode Registry needs barcode-to-Item resolution and registration, which
requires the client to identify an Item. Mobile clients must operate offline,
so item reference data must be available locally without a live call.

The existing Barang master-data synchronization and API infrastructure already
transports Barang from the Main Office to the Cloud, and from the Cloud to
mobile clients.

## Decision

1. **Barang Master is a shared reference dataset** for warehouse mobility
   applications, not a packing-order-only detail.
2. Barang master **may be synchronized to mobile clients for offline lookup**,
   reusing the existing Barang synchronization and API infrastructure.
3. Warehouse/mobile clients may **search and select Barang independently of
   Packing Orders** and maintain a **local offline cache** for offline operation.
4. No new, parallel master-data transport is introduced; the existing Barang
   sync/API path is reused and extended to the clients that need it.

## Consequences

### Positive

- Enables Barcode Registry lookup/registration on clients that previously had no
  Item master.
- Consistent Item reference across packing, barcode, and future warehouse modules.
- Offline lookup becomes possible through the local cache.
- Reuses existing sync/API infrastructure; no new synchronization mechanism.

### Negative

- Clients must store and refresh an item cache, increasing local storage and sync volume.
- Cache refresh/versioning and stale-data behavior must be defined.

### Neutral

- Does not change the Main Office ownership of Barang or the one-way
  Main Office → Cloud synchronization direction.

## Alternatives Considered

| Alternative | Rejected because |
| --- | --- |
| Keep item data embedded in packing orders only | Cannot support barcode lookup/registration or independent Item selection |
| A new dedicated Item lookup service/transport | Duplicates existing Barang sync/API; violates "avoid new synchronization infrastructure" (GAP-004) |
| Online-only Item lookup | Breaks the offline operation requirement |

## Compliance

Satisfies the GAP-013 resolution and supports GAP-006 (mobile acquisition) and
GAP-007 (mobile registration).
