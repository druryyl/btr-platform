# DOMAIN.md

## Purpose

This document defines the business language used throughout BTR.

All documentation, source code, discussions, and AI-generated artifacts should use these terms consistently.

This document describes what business concepts mean.

It does not describe workflows, business rules, implementation details, or system architecture.

---

# Core Entities

## Customer

A business partner that purchases goods from the company.

Customers may:

* Place orders
* Receive goods
* Have receivables (Piutang)
* Make payments
* Return goods

---

## Supplier (Principal)

Principal

Alias of Supplier.

Business users typically use the term Principal.

A business partner that provides goods to the company.

Within BTR, the terms **Supplier** and **Principal** are currently treated as equivalent.

Most business users use the term **Principal**, while parts of the system use the term **Supplier**.


---

## Item

A product that can be purchased, stored, sold, and returned.

Inventory quantities are maintained per Item.

---

# Sales Terms

## Sales Order

A customer's request to purchase goods.

A Sales Order represents customer demand before a sales transaction is finalized.

---

## Faktur

A Sales Invoice.

A Faktur represents an official sales transaction between the company and a customer.

---

## Faktur Kembali

A status indicating that a signed Faktur has been physically returned to the office.

---

# Inventory Terms

## Inventory

The recorded quantity of Items owned by the company.

Inventory is used to track stock availability and stock movement.

---

## Warehouse

A logical grouping of inventory.

A Warehouse does not necessarily represent a physical building.

Warehouses are primarily used to separate inventory ownership and reporting requirements.

A Faktur belongs to exactly one Warehouse.

---

## Depo

A physical storage location.

Depo represents the actual location where goods are stored, picked, and packed.

---

## Stok Opname

The process of physically counting inventory and reconciling it against recorded inventory data.

---

# Finance Terms

## Piutang

The amount owed by a Customer to the company.

Piutang is also referred to as Customer Receivable.

---

## Payment

Money received from a Customer to settle Piutang.

---

# Return Terms

## Retur

Goods returned by a Customer after a sales transaction.

Returned goods may be evaluated for their condition before further processing.

---

# Purchasing Terms

## Purchase

The acquisition of goods from a Supplier (Principal).

Purchase transactions are used to add stock into inventory.

---

# Operational Terms

## Picking

The process of collecting items required to fulfill a sales transaction.

---

## Packing

The process of preparing collected items for delivery.

---

# Ubiquitous Language

The following terms are considered official business language within BTR:

| Term           | Meaning                           |
| -------------- | --------------------------------- |
| Customer       | Business partner purchasing goods |
| Supplier       | Source of purchased goods         |
| Principal      | Equivalent to Supplier            |
| Item           | Product managed by the business   |
| Sales Order    | Customer purchase request         |
| Faktur         | Sales Invoice                     |
| Faktur Kembali | Returned signed Sales Invoice     |
| Inventory      | Recorded stock quantity           |
| Warehouse      | Logical inventory grouping        |
| Depo           | Physical storage location         |
| Stok Opname    | Physical stock counting process   |
| Piutang        | Customer receivable               |
| Payment        | Settlement of receivable          |
| Retur          | Customer returned goods           |
| Purchase       | Acquisition of goods              |
| Picking        | Collecting items for fulfillment  |
| Packing        | Preparing goods for delivery      |

---

# Notes

When there is a conflict between technical implementation and business terminology, the definitions in this document take precedence.

This document is the source of truth for BTR business vocabulary.
