# BARCODE-REGISTRY-UX-BLUEPRINT

## 1. Purpose

Barcode Registry provides a warehouse-oriented mechanism to identify, register, and maintain Item-to-Barcode mappings.

The application is offline-first and supports barcode-driven workflows for:

* Barcode Registration
* Sales Return
* Stock Opname (future)
* Warehouse Operations (future)

Barcode Registry is a shared infrastructure capability and not a standalone business process.

---

# 2. UX Principles

## UX-001 Offline First

Users must be able to perform barcode lookup and barcode registration without network connectivity.

Network availability must never be required for normal warehouse operations.

---

## UX-002 Scan First

Barcode scanning is the primary interaction model.

Manual typing is supported as a fallback only.

---

## UX-003 Minimal Input

Users should provide the minimum information required to register a barcode.

The system should use Item master data whenever possible.

---

## UX-004 Warehouse Friendly

The workflow must support operation while standing in front of shelves, cartons, and products.

The application should minimize typing and screen transitions.

---

# 3. User Roles

## Warehouse Officer

Responsibilities:

* Scan barcode
* Lookup item
* Register barcode
* Correct barcode mapping
* View synchronization status

---

## Office Admin

Responsibilities:

* Maintain barcode mappings
* Correct data issues
* Activate barcode
* Deactivate barcode
* Execute daily synchronization SOP

---

# 4. Navigation Structure

```text
Home

├── Scan Barcode
├── Barcode Registry
│    ├── Search Barcode
│    ├── Register Barcode
│    └── Edit Barcode
├── Synchronization
└── Settings
```

No additional modules are included in Phase 1.

---

# 5. Login Flow

## Login Screen

Purpose:

Authenticate user and establish operational context.

Fields:

* Username
* Password
* Warehouse

Warehouse Values:

* Gudang Gamping
* Gudang Concat
* Gudang Magelang

Actions:

* Login

Behavior:

After successful login:

```text
Warehouse
    ↓
Office Resolution
    ↓
Master Data Synchronization
    ↓
Home Screen
```

---

# 6. Home Screen

Purpose:

Provide entry point to barcode operations.

Sections:

## Quick Actions

* Scan Barcode
* Search Barcode
* Register Barcode

## Synchronization Status

Display:

* Online / Offline
* Last Sync Time
* Pending Upload Count

## Current Context

Display:

* Logged In User
* Warehouse
* Office

---

# 7. Scan Barcode Screen

Purpose:

Identify item from barcode.

Entry:

* Home Screen
* Barcode Registry

Scanner:

* Camera Scanner
* Manual Entry

Flow:

```text
Scan Barcode
      ↓
Lookup Local Cache
```

---

## Scenario A : Barcode Found

Display:

* Barcode
* Item Code
* Item Name
* Unit

Actions:

* Close

---

## Scenario B : Barcode Not Found

Display:

* Barcode Not Found

Actions:

* Register Barcode
* Cancel

---

# 8. Register Barcode Screen

Purpose:

Create a new Barcode Registration.

---

## Initial State

Barcode already captured from scanner.

Fields:

* Barcode (Read Only)
* Item Search
* Unit (Optional)

Actions:

* Save
* Cancel

---

## Item Search

Search Criteria:

* Item Code
* Item Name

Data Source:

Local Item Cache

---

## Validation Rules

Barcode:

* Required

Item:

* Required
* Must exist in local Item cache
* Must be Active

Unit:

* Optional

---

## Save Result

Successful Save:

```text
Create Registration
      ↓
Store Local Queue
      ↓
Show Success Message
```

No immediate server communication is required.

---

# 9. Barcode Registry Screen

Purpose:

Browse and search barcode mappings.

Search:

* Barcode
* Item Code
* Item Name

Display:

* Barcode
* Item Code
* Item Name
* Unit

Actions:

* Edit Barcode

---

# 10. Edit Barcode Screen

Purpose:

Correct existing barcode mapping.

Editable Fields:

* Item
* Unit

Non Editable:

* Barcode

Actions:

* Save
* Cancel

Behavior:

Changes are stored locally and synchronized later.

---

# 11. Synchronization Screen

Purpose:

Provide visibility of synchronization state.

Sections:

## Master Data

Display:

* Last Barang Sync
* Last Barcode Sync

---

## Registration Queue

Display:

* Pending
* Success
* Rejected

---

## Actions

* Sync Now

---

# 12. Offline Behavior

## Offline Lookup

Supported.

```text
Scan
    ↓
Local Cache
```

---

## Offline Registration

Supported.

```text
Register
    ↓
Local Queue
```

---

## Offline Editing

Supported.

```text
Edit
    ↓
Local Queue
```

---

# 13. Synchronization Behavior

Synchronization occurs:

* During Login
* Manual Sync

Phase 1 does not require automatic background synchronization.

---

# 14. Error Handling

## Item Not Found

Message:

"Item tidak ditemukan."

Action:

Close

---

## Item Inactive

Message:

"Item sudah tidak aktif."

Action:

Close

---

## Duplicate Barcode

Message:

"Barcode sudah terdaftar."

Action:

View Existing Mapping

---

## Synchronization Failed

Message:

"Gagal sinkronisasi."

Action:

Retry

---

# 15. Display States

## Connectivity

* Online
* Offline

---

## Registration Status

* Pending
* Synced
* Rejected

---

# 16. Out of Scope

The following are not included in Phase 1:

* Sales Return
* Stock Opname
* Receiving
* Picking
* Warehouse Transfer
* Principal Barcode Import
* Real-time Synchronization
* Push Notification
* Approval Workflow

These capabilities may consume Barcode Registry in future releases but are not part of the Barcode Registry MVP.
