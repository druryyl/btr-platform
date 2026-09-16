# RETURN-ORDER-DOMAIN

## 1. Purpose

Return Order is a warehouse operational capability used to record goods physically returned by Customers and received by Warehouse Officers.

Return Order serves as the operational source document for subsequent Sales Return processing in the Main Office system.

Return Order focuses on recording physical goods movement and does not perform inventory valuation, financial processing, customer balance adjustment, or accounting activities.

---

# 2. Business Goals

## BG-001 — Faster Return Receiving

Enable Warehouse Officers to record returned goods immediately when goods arrive at the warehouse.

---

## BG-002 — Operational Accuracy

Capture returned items directly at the receiving point to reduce manual transcription and data entry errors.

---

## BG-003 — Offline Warehouse Operation

Support warehouse operations even when network connectivity is unavailable.

---

## BG-004 — Sales Return Source

Provide structured operational data for Sales Return creation in BTR Desktop.

---

# 3. Business Vocabulary

| Term              | Definition                                                                              |
| ----------------- | --------------------------------------------------------------------------------------- |
| Return Order      | Warehouse document recording goods physically returned by a Customer                    |
| Return Order Item | Individual returned item recorded within a Return Order                                 |
| Customer          | Party returning goods to BTR                                                            |
| Warehouse         | Physical warehouse receiving returned goods                                             |
| Warehouse Officer | User responsible for recording Return Orders                                            |
| Office Admin      | User responsible for creating Sales Return transactions from synchronized Return Orders |
| Good Return       | Returned item considered sellable                                                       |
| Broken Return     | Returned item considered damaged                                                        |
| Sales Return      | Office document generated from Return Orders                                            |
| Barcode           | Product identifier used for item lookup                                                 |
| Unit              | Packaging unit used during return recording (Dus, Pak, Pcs, etc.)                       |

---

# 4. Actors

## Warehouse Officer

Responsibilities:

* Receive returned goods
* Create Return Orders
* Update Return Orders before synchronization
* Delete Return Orders before synchronization
* Synchronize Return Orders

---

## Office Admin

Responsibilities:

* Review synchronized Return Orders
* Complete Salesman information when required
* Complete Driver information when required
* Create Sales Return transactions
* Determine return pricing using invoice history
* Post Sales Return transactions

---

# 5. Business Capabilities

## BC-001 Create Return Order

Record returned goods received from a Customer.

---

## BC-002 Update Return Order

Modify Return Orders that have not yet been synchronized.

---

## BC-003 Delete Return Order

Remove Return Orders that have not yet been synchronized.

---

## BC-004 Search Return Order

Locate previously recorded Return Orders.

---

## BC-005 Synchronize Return Order

Transfer Return Orders from BGud to the Main Office integration process.

---

# 6. Domain Concepts

## Return Order

Return Order represents a single warehouse receiving event from a Customer.

A Return Order consists of:

* One Customer
* One Warehouse
* One or more Return Order Items

A Return Order becomes the source for Sales Return generation.

---

## Return Order Item

Return Order Item represents an individual returned product.

Each item records:

* Item
* Quantity
* Unit
* Return Type

A Return Order must contain at least one Return Order Item.

---

# 7. Aggregate

## Return Order

Return Order is the Aggregate Root.

### Identity

Business Identity:

```text
ReturnOrderNo
```

Technical Identity:

```text
ReturnOrderId
```

### Composition

```text
Return Order
    └── Return Order Item
    └── Return Order Item
    └── Return Order Item
```

---

# 8. Domain Objects

## Return Order

### Attributes

* ReturnOrderId
* ReturnOrderNo
* Customer
* Warehouse
* Salesman (Optional)
* Driver (Optional)
* Notes
* Status
* CreatedDate
* CreatedBy

---

## Return Order Item

### Attributes

* Item
* Quantity
* Unit
* ReturnType

---

# 9. Return Types

Return Type is recorded per Return Order Item.

Allowed values:

```text
Good
Broken
```

A single Return Order may contain both Good and Broken items.

Example:

```text
Customer A

Indomie Goreng     5 Dus     Good
Indomie Soto       2 Dus     Broken
```

---

# 10. Business Rules

## Customer Rules

### BR-001

Customer is mandatory.

---

### BR-002

A Return Order cannot exist without a Customer.

---

### BR-003

A Return Order is not required to reference an original Sales Invoice.

---

### BR-004

Returned items may originate from multiple historical invoices.

---

## Warehouse Rules

### BR-005

Warehouse is mandatory.

---

### BR-006

A Return Order belongs to exactly one Warehouse.

---

## Item Rules

### BR-007

Item is mandatory.

---

### BR-008

Item must exist in Item Master.

---

### BR-009

Item may be identified using:

* Barcode
* Item Search

---

## Quantity Rules

### BR-010

Quantity must be greater than zero.

---

### BR-011

Unit is mandatory.

---

### BR-012

Return Order must contain at least one Return Order Item.

---

## Salesman Rules

### BR-013

Salesman is optional.

---

### BR-014

Salesman may be completed later by Office Admin.

---

## Driver Rules

### BR-015

Driver is optional.

---

### BR-016

Driver may be completed later by Office Admin.

---

## Modification Rules

### BR-017

Return Orders may be modified before synchronization.

---

### BR-018

Return Orders may not be modified after synchronization.

---

## Deletion Rules

### BR-019

Return Orders may be deleted before synchronization.

---

### BR-020

Return Orders may not be deleted after synchronization.

---

# 11. Lifecycle

## Draft

Return Order exists only within warehouse operations.

---

## Synced

Return Order has been successfully synchronized.

---

## Imported

Return Order has been consumed by the Main Office Sales Return process.

---

### Lifecycle Flow

```text
Draft
   ↓
Synced
   ↓
Imported
```

---

# 12. Domain Events

## Return Order Created

Raised when a Warehouse Officer creates a new Return Order.

---

## Return Order Updated

Raised when a Return Order is modified.

---

## Return Order Deleted

Raised when a Return Order is removed.

---

## Return Order Synchronized

Raised when a Return Order is successfully transferred to the Main Office integration process.

---

## Return Order Imported

Raised when a Return Order is consumed by the Sales Return creation process.

---

# 13. Sales Return Integration Rules

Return Order and Sales Return are separate business entities.

```text
Return Order
      ↓
Synchronization
      ↓
Sales Return
```

Return Order belongs to the Warehouse domain.

Sales Return belongs to the Office domain.

---

When generating Sales Return documents, imported Return Orders shall be grouped using:

```text
Customer
Return Type
Salesman
Driver
```

The grouping process is performed by the Main Office system.

---

# 14. High-Level Workflow

```text
Customer
      ↓
Returns Goods
      ↓
Expedition Delivers Goods
      ↓
Warehouse Receives Goods
      ↓
Warehouse Officer Creates Return Order
      ↓
Return Order Synchronized
      ↓
Office Admin Creates Sales Return
      ↓
Sales Return Posted
```

---

# 15. Out of Scope

The following capabilities are outside the scope of Return Order:

* Inventory Update
* Inventory Valuation
* Customer Balance Adjustment
* Credit Note Processing
* Accounting Entries
* Return Approval Workflow
* Return Rejection Workflow
* Principal Claim Processing
* Damaged Goods Disposal
* Financial Settlement
* Invoice-Level Return Tracking

These capabilities belong to downstream processes outside the Return Order domain.
