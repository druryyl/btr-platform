# Sales Person–Principal Assignment

## Purpose

Maintain which Principals (Suppliers) each Sales Person is responsible for.

This assignment is master data used to define the commercial relationship between field sales staff and product Principals.

---

## Business Context

BTR distributes goods from multiple Principals through Sales Persons in the field.

Each Sales Person may represent one or more Principals when visiting customers and recording sales activity.

Principal is the business term used by users. In BTR technical data, Principal maps to **Supplier** (`BTR_Supplier`).

---

## Core Concepts

### Sales Person–Principal Assignment

A link between one Sales Person and one Principal (Supplier).

An assignment means the Sales Person is authorized to sell products from that Principal.

This is not a sales transaction. It does not record sales volume or targets.

---

## Business Rules

1. A Sales Person may have **multiple** Principals assigned.
2. A Principal may be assigned to **multiple** Sales Persons.
3. The same Sales Person–Principal pair cannot be assigned more than once.
4. Removing an assignment deletes only the master-data link.
5. Removing an assignment does **not** change historical sales documents such as Faktur.

---

## Desktop Maintenance

| Item | Value |
|------|-------|
| Menu | SM5-Principal Assignment |
| Form | `SalesPersonSupplierForm` |
| Pattern | Sales Person list → Assigned Principal grid |

### Supported Operations

- Select Sales Person
- View assigned Principals
- Add Principal assignment (via Principal/Supplier browser)
- Remove Principal assignment

---

## Technical Mapping

| Business | Technical |
|----------|-----------|
| Sales Person | `BTR_SalesPerson` / `SalesPersonId` |
| Principal | `BTR_Supplier` / `SupplierId` |
| Assignment | `BTR_SalesPersonSupplier` |

### Table: `BTR_SalesPersonSupplier`

| Column | Description |
|--------|-------------|
| `SalesPersonId` | FK to `BTR_SalesPerson` |
| `SupplierId` | FK to `BTR_Supplier` |

Primary key: `(SalesPersonId, SupplierId)`

---

## Related Features

- [Sales Person–Principal Target](../sales-person-principal-target/feature.md) (SM6) — monthly targets per assigned Principal

---

## Out of Scope

- Portal sync of assignment data
- Automatic enforcement of assignment on Faktur entry (not part of this feature)
