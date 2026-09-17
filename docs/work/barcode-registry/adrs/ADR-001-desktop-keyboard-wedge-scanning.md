# ADR-001 — Desktop Keyboard-Wedge Barcode Scanning

| Field | Value |
| --- | --- |
| Status | Accepted |
| Date | 2026-09-15 |
| Decided via | Barcode Registry feasibility, GAP-006 |
| Assessment | `docs/work/barcode-registry/BARCODE-REGISTRY-FEASIBILITY-ASSESMENT.md` |

## Context

Barcode Registry requires barcode acquisition on two client families:

- **Desktop** — BTR Desktop (`j05-btr-distrib`), a .NET Framework WinForms application.
- **Mobile** — camera-based scanning.

No barcode acquisition capability exists today in any client. Desktop is a
text-entry form environment where a barcode read is, functionally, rapid
keyboard input terminated by a newline. Introducing vendor-specific scanner
SDKs would add per-vendor integration, deployment, and maintenance burden with
no benefit to the domain.

The Barcode Registry domain and business logic must not depend on how a barcode
value was acquired; they operate on a barcode value string only.

## Decision

1. Desktop barcode scanning **shall** use keyboard-wedge (HID) scanners. The
   scanner emits the barcode value and a terminator as standard keyboard input;
   the Desktop client captures it through its normal input control.
2. **No vendor-specific scanner SDK will be supported** on Desktop. No scanner
   model is bound to the codebase.
3. Barcode acquisition is isolated from the Barcode Registry domain and business
   logic, which remain scanner-technology agnostic: they receive a barcode value
   and perform lookup/registration without knowledge of the acquisition device.
4. Mobile barcode acquisition is camera-based and is outside this ADR; it is
   recorded under GAP-006.

## Consequences

### Positive

- Zero vendor lock-in and no additional runtime dependencies or licenses on Desktop.
- Works with any HID keyboard-wedge scanner, including existing/standard devices.
- Keeps the domain and application layers testable with plain barcode value inputs.
- Minimal change surface: standard WinForms text input plus capture/validate logic.

### Negative

- Scanner behavior configuration (prefix/suffix, terminator, keyboard layout)
  becomes an operational/deployment concern rather than an application concern.
- No device-level features (batch mode, on-device validation) are available through the application.

### Neutral

- No change to the Main Office schema, the Cloud read model, or synchronization.

## Alternatives Considered

| Alternative | Rejected because |
| --- | --- |
| Vendor-specific scanner SDK integration | Adds deployment, licensing, and maintenance burden; violates the scanner-agnostic requirement; no functional gain for value capture |
| Camera-based scanning on Desktop | Not aligned with the Desktop form-factor/workstation reality; mobile camera scanning is decided separately under GAP-006 |
| Custom serial/COM-port scanner driver | More complex than HID keyboard-wedge with no benefit |

## Compliance

Satisfies the GAP-006 resolution for Desktop: "Desktop uses keyboard-wedge
barcode scanners" and "Domain and business logic remain scanner-technology
agnostic."
