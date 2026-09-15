# Barcode Registry

## 1. Business Overview

Barcode Registry is a master data domain responsible for managing the relationship between physical barcodes and products maintained in the Item Master.

The domain enables operational applications to identify products through barcode scanning while maintaining a single authoritative barcode repository.

Barcode Registry serves as a shared foundation for warehouse mobility solutions, including Sales Return, Stock Opname, Receiving, Picking, Warehouse Transfer, and future warehouse operations.

Barcode Registry does not manage inventory quantities or warehouse transactions.

### Business Problem

Operational processes increasingly rely on barcode scanning for speed and accuracy. However:

- Products may have multiple barcodes.
- Different packaging levels may use different barcodes.
- Barcode information is often incomplete.
- Barcode mappings may only exist in individual user knowledge.
- Mobile applications require barcode lookup capability while the Main Office Server is not publicly accessible.

Without a centralized barcode registry:

- Product identification becomes inconsistent.
- Warehouse operations become slower.
- Future barcode-enabled applications become difficult to implement.

### Business Objectives

| ID | Objective |
| --- | --- |
| BO-001 | Provide a single source of truth for barcode-to-item mapping. |
| BO-002 | Allow products to be identified through barcode scanning. |
| BO-003 | Support barcode registration from both Desktop and Mobile applications. |
| BO-004 | Provide barcode lookup services for public-facing mobile applications. |
| BO-005 | Establish a reusable barcode foundation for future warehouse applications. |

### Scope

Included:

- Barcode registration
- Barcode maintenance
- Barcode lookup
- Barcode activation
- Barcode deactivation
- Barcode-to-item mapping
- Packaging level classification
- Barcode synchronization to cloud lookup services

Excluded:

- Inventory management
- Product Master maintenance
- Sales Return processing
- Stock Opname processing
- Receiving processing
- Picking processing
- Warehouse Transfer processing

### Authority Model

Main Office Server is the authoritative source of Barcode Registry data. It stores barcode mappings, validates barcode uniqueness, maintains barcode history, processes barcode modifications, and publishes lookup data.

Cloud services provide barcode lookup capabilities for public-facing applications. They support barcode lookup, mobile application support, and read-only barcode reference. Cloud services must never become the authoritative source of barcode information.

### Synchronization Model

Main Office Server is the System of Record. All barcode creation and modification operations are ultimately persisted there.

Cloud Platform is a Read Model. Barcode information is synchronized from Main Office Server to Cloud Platform. Synchronization is one-way: Main Office Server → Cloud Platform.

Mobile applications may create barcode registrations. The registration request is submitted to Main Office Server. Once accepted by Main Office Server, the barcode becomes authoritative and cloud lookup data is synchronized. No approval workflow exists.

---

## 2. Ubiquitous Language

| Term | Definition |
| --- | --- |
| Barcode Registry | The master data domain that owns the relationship between physical barcodes and Items. |
| Item | A product maintained by Item Master. Owned by Product Master Domain; referenced but not owned by Barcode Registry. |
| Barcode | A physical barcode value that can be scanned and that uniquely identifies a product mapping. |
| Barcode Mapping | The association between one Item and one Barcode. |
| Barcode Value | The scanned identifier itself, such as `8998866800015`. |
| Packaging Level | Optional information describing the packaging level represented by a barcode, such as Small Unit or Big Unit. |
| Active Barcode | A barcode that is available for operational barcode lookup. |
| Inactive Barcode | A barcode that is no longer used operationally but remains available for historical reference. |
| Barcode Registration | The act of creating a new Barcode Mapping for an existing Item. |
| Barcode Lookup | The act of resolving a scanned Barcode Value into Item information. |
| Authoritative Repository | The Main Office Server store of Barcode Registry data; the single source of truth. |
| Lookup Repository | The cloud read-only copy of barcode data used to serve public-facing mobile lookups. |
| System of Record | Main Office Server, where all barcode creation and modification operations are ultimately persisted. |
| Read Model | Cloud Platform, which receives synchronized barcode data and serves read-only lookups. |
| Synchronization | The one-way transfer of barcode information from Main Office Server to Cloud Platform. |

---

## 3. Business Capabilities

### 3.1 Barcode Registration

Create a new mapping between a physical barcode and an existing Item, optionally classified by Packaging Level, from either the Mobile App or the BTR Desktop App.

### 3.2 Barcode Maintenance

Correction of Barcode Mappings after registration, including editing the mapped Item and Packaging Level.

### 3.3 Barcode Lookup

Resolve a scanned Barcode Value into the associated Item so that operational activities can proceed.

### 3.4 Barcode Activation and Deactivation

Control whether a Barcode is available for operational lookup while retaining inactive barcodes for historical reference.

### 3.5 Barcode-to-Item Mapping

Maintain the authoritative association that binds each Barcode to exactly one Item and each Item to one or more Barcodes.

### 3.6 Packaging Level Classification

Optionally record the packaging level represented by a barcode.

### 3.7 Barcode Synchronization to Cloud Lookup

Publish authoritative barcode data from Main Office Server to Cloud services so that public-facing mobile applications can perform barcode lookup.

---

## 4. Actors & Roles

### Warehouse Officer

- Scan barcode
- Lookup barcode
- Register barcode
- Correct barcode mapping

### Office Admin

- Register barcode
- Modify barcode mapping
- Activate barcode
- Deactivate barcode
- Search barcode information

### System Administrator

- Resolve data issues
- Manage synchronization
- Support operational maintenance

---

## 5. Domain Objects

### Barcode

Represents a physical barcode value and its mapping to one Item.

Attributes:

- BarcodeId
- BarcodeValue
- ItemId
- PackagingLevel
- IsActive
- CreatedBy
- CreatedDate
- ModifiedBy
- ModifiedDate

A Barcode uniquely identifies a product mapping.

### Item Reference

Represents the Item that a Barcode is mapped to. Owned by Product Master Domain; referenced by Barcode Registry but not modified by it.

Attributes:

- ItemId
- ItemCode
- ItemName

### Packaging Level

Optional classification describing the packaging level represented by a Barcode, such as Small Unit or Big Unit. A barcode may exist without packaging information.

---

## 6. Aggregates

### Barcode (Aggregate Root)

Barcode is the Aggregate Root of the Barcode Registry domain.

Consistency boundary:

- Barcode Value must be globally unique.
- One Barcode belongs to exactly one Item.
- One Item may have many Barcodes.
- Activation state is maintained per Barcode.

Owned entities:

- Barcode Mapping, including Packaging Level and activation state.

Business responsibility:

- Preserve the authoritative and unique relationship between a physical barcode and its Item.

Item is an external reference owned by Product Master Domain and is not part of the Barcode aggregate.

---

## 7. Business Rules

### BR-001

Barcode values must be globally unique.

### BR-002

One Item may contain multiple Barcodes.

### BR-003

One Barcode may only belong to one Item.

### BR-004

Barcode registration requires an existing Item.

### BR-005

Packaging Level is optional.

### BR-006

Inactive Barcodes remain available for historical reference.

### BR-007

Only active Barcodes are returned during operational barcode lookup.

### BR-008

Barcode Registry cannot modify Product Master data.

### BR-009

Barcode registration may be performed from:

- Mobile App
- BTR Desktop App

Both registration channels have equal authority.

### BR-010

Barcode mappings may be corrected after registration.

### BR-011

All barcode uniqueness validation must be performed by Main Office Server.

### BR-012

Offline barcode registrations are treated as registration requests.

Authoritative Barcode Registry validation occurs in Main Office during synchronization.

### BR-013

When synchronization conflicts occur, the Main Office authoritative state wins.

Conflicting offline requests are rejected and do not modify the authoritative Barcode Registry.

### BR-014

Barcode Registration Requests are integration artifacts and are not subject to master-data synchronization replacement rules.

---

## 8. State Machines & Lifecycles

### Barcode Lifecycle

```text
Registered (Active)
        |
        | Deactivate
        v
    Inactive
        |
        | Activate
        v
Registered (Active)
```

States:

| State | Meaning |
| --- | --- |
| Active | The Barcode is available for operational barcode lookup and is eligible for synchronization to the Cloud lookup repository. |
| Inactive | The Barcode is no longer used operationally but remains stored for historical reference. It is not returned during operational barcode lookup. |

Transitions:

- Registration creates a Barcode in the Active state.
- Deactivation moves an Active Barcode to Inactive.
- Activation returns an Inactive Barcode to Active.
- Inactive Barcodes are retained permanently for historical reference.
- Correction of a Barcode Mapping does not change its activation state.

---

## 9. Domain Events

| Event | Meaning |
| --- | --- |
| Barcode Registered | A new Barcode Mapping between a Barcode and an Item was stored and became immediately available. |
| Barcode Mapping Corrected | An existing Barcode Mapping was changed after registration. |
| Barcode Activated | An Inactive Barcode became Active and available for operational lookup. |
| Barcode Deactivated | An Active Barcode became Inactive and was withdrawn from operational lookup while remaining for historical reference. |
| Barcode Synchronized to Cloud Lookup | Authoritative barcode data was published from Main Office Server to the Cloud lookup repository. |

---

## 10. Business Workflows

### UC-001 Barcode Lookup

Actors: Warehouse Officer, Office Admin.

```text
Scan Barcode
    ↓
System searches Barcode Registry
    ↓
System returns Item information
    ↓
User proceeds with operational activity
```

### UC-002 Register Barcode

Actors: Warehouse Officer, Office Admin.

```text
Scan Barcode
    ↓
System searches Barcode Registry
    ↓
Barcode is not found
    ↓
User selects Item
    ↓
User optionally selects Packaging Level
    ↓
System validates uniqueness
    ↓
Barcode Mapping is stored
    ↓
Barcode becomes immediately available
```

### UC-003 Maintain Barcode

Actors: Warehouse Officer, Office Admin.

```text
Search Item
    ↓
View associated Barcodes
    ↓
Add, edit, activate, deactivate, or correct Barcode Mappings
    ↓
Save changes
```

### UC-004 View Item Barcodes

Actors: Warehouse Officer, Office Admin.

```text
Search Item
    ↓
System displays all associated Barcodes
    ↓
User reviews barcode mappings
```

### Barcode Synchronization Workflow

```text
Barcode Created or Modified
    ↓
Main Office Server validates and persists
    ↓
Barcode becomes authoritative
    ↓
Barcode Synchronized to Cloud Lookup
    ↓
Cloud lookup repository serves public-facing mobile lookup
```

### Integration

- Product Master provides ItemId, ItemCode, and ItemName. Barcode Registry consumes Product Master data.
- Sales Return consumes Barcode Lookup.
- Stock Opname consumes Barcode Lookup.
- Receiving consumes Barcode Lookup.
- Picking consumes Barcode Lookup.
- Warehouse Transfer consumes Barcode Lookup.
