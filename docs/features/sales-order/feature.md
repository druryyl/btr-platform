# Sales Order Domain

## Purpose

The Sales Order feature allows salespersons to record customer purchase requests during field sales activities.

The feature supports offline operation so that orders can be captured regardless of internet availability.

Sales Orders become the source document used by the office to create Faktur.

---

# Business Context

Salespersons visit customers and collect purchase requests.

Customer orders are recorded using the BTrade3 mobile application.

Because sales activities often occur in areas with unreliable internet connectivity, order entry must remain available while offline.

Orders may be synchronized to the office immediately or at a later time.

---

# Core Concepts

## Sales Order

A Sales Order represents a customer's request to purchase one or more Items.

A Sales Order is created by a SalesPerson for exactly one Customer.

A Sales Order is not a sales transaction.

A Sales Order represents customer demand before a Faktur is created.

---

## Sales Order Item

A Sales Order Item represents a requested Item within a Sales Order.

Each Sales Order Item contains:

* Item
* Quantity

A Sales Order contains one or more Sales Order Items.

---

## Draft Order

A Draft Order is a Sales Order that exists only on the salesperson's mobile device.

Draft Orders have not yet been successfully synchronized to the office.

Draft Orders remain valid business documents and may be synchronized later.

---

## Synced Order

A Synced Order is a Sales Order that has been successfully transmitted from the mobile application.

A Synced Order becomes available for office processing.

---

## Imported Order

An Imported Order is a Sales Order that has been received and recorded by the main office system.

Imported Orders are available for Faktur creation.

---

# Order Lifecycle

```text
Draft
    ↓
Synced
    ↓
Imported
    ↓
Invoiced
```

---

## Draft

The Sales Order has been created but has not yet been synchronized.

---

## Synced

The Sales Order has been successfully transmitted from the mobile application.

---

## Imported

The Sales Order has been received by the office system.

---

## Invoiced

A Faktur has been created based on the Sales Order.

The Sales Order is considered fulfilled from a sales administration perspective.

---

# Business Rules

## Customer Requirement

A Sales Order must belong to exactly one Customer.

The Customer must exist in the synchronized customer master data available to the salesperson.

---

## Order Item Requirement

A Sales Order must contain at least one Sales Order Item.

---

## Item Requirement

Each Sales Order Item must reference a valid Item.

The Item must exist in the synchronized item master data available to the salesperson.

---

## Offline Operation

Internet connectivity is not required for Sales Order creation.

Salespersons must be able to create and maintain Sales Orders while offline.

---

## Deferred Synchronization

Sales Orders created while offline remain valid business documents.

Synchronization may occur immediately or at a later time.

---

## Faktur Creation Source

Sales Orders serve as the source document for Faktur creation.

Faktur creation occurs within office operations and is outside the responsibility of this feature.

---

# Relationships

```text
Customer
    │
    └── Sales Order
            │
            ├── Sales Order Item
            │       └── Item
            │
            └── SalesPerson
```

---

# Out of Scope

The following concepts are outside the responsibility of the Sales Order feature:

* Customer Check-In
* Customer Visit Tracking
* GPS Location Tracking
* Route Management
* Faktur Creation
* Warehouse Fulfillment
* Inventory Allocation
* Goods Delivery
* Payment Collection
* Receivable Management
* Customer Returns

These concerns belong to other features within BTR.
