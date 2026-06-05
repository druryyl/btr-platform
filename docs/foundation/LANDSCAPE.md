# LANDSCAPE.md

## Purpose

This document describes the major business areas within BTR, the systems involved in each area, and their responsibilities.

The purpose of this document is to help developers and AI agents understand:

* Which business area owns a concept
* Which systems participate in a business area
* Which business areas collaborate with each other

This document does not define business terminology, business workflows, or technical implementation details.

---

# Landscape Overview

```text
Master Data
    │
    ├── Sales
    │       │
    │       ├── Inventory
    │       └── Finance
    │
    └── Purchasing
            │
            └── Inventory
```

---

# Master Data Area

## Responsibility

Manage shared business reference data used by all operational areas.

## Business Concepts

* Customer
* Item
* Supplier (Principal)

## Systems

* BTR Desktop

## Collaborates With

* Sales
* Inventory
* Finance
* Purchasing

---

# Sales Area

## Responsibility

Manage customer sales activities and sales transactions.

## Business Concepts

* Sales Order
* Faktur
* Faktur Kembali

## Systems

* BTR Desktop
* BTrade3

## Collaborates With

* Master Data
* Inventory
* Finance

---

# Inventory Area

## Responsibility

Manage stock records, warehouse operations, and inventory reconciliation.

## Business Concepts

* Inventory
* Warehouse
* Depo
* Retur
* Stok Opname

## Systems

* BTR Desktop
* BTR Gudang

## Collaborates With

* Master Data
* Sales
* Purchasing
* Finance

---

# Finance Area

## Responsibility

Manage customer receivables, payment recording, and tax reporting.

## Business Concepts

* Piutang
* Payment
* CoreTax Export

## Systems

* BTR Desktop

## Collaborates With

* Master Data
* Sales
* Inventory

---

# Purchasing Area

## Responsibility

Manage stock replenishment activities.

## Business Concepts

* Purchase

## Systems

* BTR Desktop

## Collaborates With

* Master Data
* Inventory

---

# Supporting Systems

## BTR.Sync

Synchronization bridge between BTR Desktop and distributed applications.

Supports:

* Sales Area
* Inventory Area

---

## Cloud API

Synchronization hub for distributed applications.

Supports:

* BTrade3
* BTR Gudang
* BTR.Sync

---

# System Reference

| System      | Primary Role            |
| ----------- | ----------------------- |
| BTR Desktop | Source of Truth         |
| BTrade3     | Mobile Sales Operations |
| BTR Gudang  | Warehouse Operations    |
| BTR.Sync    | Synchronization Bridge  |
| Cloud API   | Synchronization Hub     |

---

# Ownership Reference

| Business Concept     | Business Area |
| -------------------- | ------------- |
| Customer             | Master Data   |
| Item                 | Master Data   |
| Supplier (Principal) | Master Data   |
| Sales Order          | Sales         |
| Faktur               | Sales         |
| Faktur Kembali       | Sales         |
| Inventory            | Inventory     |
| Warehouse            | Inventory     |
| Depo                 | Inventory     |
| Retur                | Inventory     |
| Stok Opname          | Inventory     |
| Piutang              | Finance       |
| Payment              | Finance       |
| CoreTax Export       | Finance       |
| Purchase             | Purchasing    |

---

# Notes

When working on a feature, identify:

1. The business concept involved.
2. The business area that owns the concept.
3. The systems participating in the process.

This document is the primary navigation map for understanding responsibility and system involvement within BTR.

For business terminology, refer to `docs/foundation/DOMAIN.md`.

For business processes, refer to `docs/foundation/WORKFLOW.md`.

For product goals and scope, refer to `docs/foundation/PRODUCT.md`.
