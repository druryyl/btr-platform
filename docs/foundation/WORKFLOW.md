# WORKFLOW.md

## Purpose

This document describes the major business workflows within BTR.

The purpose of this document is to help developers and AI agents understand how the business operates at a high level.

This document describes business processes only.

It does not define business terminology, context ownership, technical architecture, or detailed feature behavior.

---

# Workflow Overview

BTR supports four primary operational workflows:

1. Sales Workflow
2. Receivable Collection Workflow
3. Return Workflow
4. Purchasing Workflow

---

# Sales Workflow

## Objective

Sell goods to customers and fulfill customer demand.

## Flow

```text
Customer
    ↓
Sales Order
    ↓
Faktur
    ↓
Warehouse Fulfillment
    ↓
Customer Receives Goods
    ↓
Faktur Kembali
```

## Description

A customer requests goods through a Sales Order or direct sales process.

A Faktur is created as the official sales transaction.

The warehouse fulfills the request through picking and packing activities.

The goods are delivered to the customer.

The signed Faktur is returned to the office as Faktur Kembali.

---

# Receivable Collection Workflow

## Objective

Collect outstanding customer receivables.

## Flow

```text
Customer Receivable
    ↓
Collection Visit
    ↓
Customer Payment
    ↓
Payment Recording
```

## Description

Sales Personnel visit customers and perform collection activities.

Payments received from customers are recorded by Finance Administration.

The customer's outstanding receivable balance is updated accordingly.

---

# Return Workflow

## Objective

Process customer returned goods.

## Flow

```text
Customer Return
    ↓
Return Inspection
    ↓
Inventory Decision
    ↓
Receivable Adjustment
```

## Description

Returned goods are received from customers.

The condition of the returned goods is evaluated.

Inventory and receivable records are updated according to the return outcome.

---

# Purchasing Workflow

## Objective

Replenish inventory stock.

## Flow

```text
Purchase
    ↓
Stock Receipt
    ↓
Inventory Update
```

## Description

Goods are purchased from a Supplier (Principal).

Received goods are recorded and added into inventory.

---

# Inventory Reconciliation Workflow

## Objective

Verify inventory accuracy.

## Flow

```text
Physical Stock Count
    ↓
Inventory Comparison
    ↓
Inventory Adjustment
```

## Description

Inventory staff perform physical stock counting activities.

Recorded inventory is compared against physical inventory.

Differences are reconciled through inventory adjustment activities.

---

# Supporting Operational Workflow

## Mobile Sales Synchronization

### Objective

Support distributed sales operations.

### Flow

```text
BTR Desktop
    ↓
Synchronization
    ↓
Mobile Sales Operations
```

### Description

Sales activities performed through mobile applications are synchronized with the main business system.

---

## Warehouse Synchronization

### Objective

Support warehouse fulfillment activities.

### Flow

```text
BTR Desktop
    ↓
Synchronization
    ↓
Warehouse Operations
```

### Description

Warehouse fulfillment activities receive and exchange data through synchronization mechanisms.

---

# Workflow Reference

| Workflow                          | Primary Contexts      |
| --------------------------------- | --------------------- |
| Sales Workflow                    | Sales, Inventory      |
| Receivable Collection Workflow    | Finance, Sales        |
| Return Workflow                   | Inventory, Finance    |
| Purchasing Workflow               | Purchasing, Inventory |
| Inventory Reconciliation Workflow | Inventory             |
| Mobile Sales Synchronization      | Sales                 |
| Warehouse Synchronization         | Inventory             |

---

# Notes

This document intentionally focuses on high-level business processes.

Detailed business rules, validations, exceptions, and feature behavior should be documented within their respective feature artifacts.

For business terminology, refer to DOMAIN.md.

For business responsibility boundaries, refer to CONTEXTS.md.

For system structure, refer to SYSTEM-LANDSCAPE.md.
