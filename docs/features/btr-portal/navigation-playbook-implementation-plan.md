# IMPLEMENTATION-PLAN.md
# BTR OWNER NAVIGATION PLAYBOOK

## Objective

Membangun sebuah artefak tunggal:

```text
BTR-OWNER-NAVIGATION-PLAYBOOK.html
```

yang memungkinkan Business Owner memahami:

- Pertanyaan bisnis apa saja yang dapat dijawab oleh BTR Portal
- Dashboard mana yang harus dibuka
- KPI mana yang harus dilihat
- Bagaimana menginterpretasikan KPI tersebut
- KPI berikutnya yang perlu diinvestigasi
- Tindakan bisnis yang biasanya dilakukan

Playbook harus berorientasi pada cara berpikir Owner, bukan struktur teknis sistem.

---

# Success Criteria

Owner dapat menjawab pertanyaan berikut tanpa bantuan developer:

- Apakah bisnis saya sehat?
- Customer mana yang bermasalah?
- Piutang mana yang berisiko?
- Salesman mana yang tidak perform?
- Barang mana yang macet?
- Apakah stok saya sehat?
- Barang apa yang harus dibeli?
- Supplier mana yang terlalu dominan?
- Apa risiko terbesar saya saat ini?
- Apa yang perlu saya tindak hari ini?

---

# Guiding Principles

## Principle-1

Dokumentasi harus berorientasi pada:

```text
Business Question
```

bukan:

```text
Dashboard
```

dan bukan:

```text
KPI
```

---

## Principle-2

Dokumentasi harus menggunakan bahasa bisnis.

Hindari:

- SQL
- Table Database
- API
- Source Code
- Formula Teknis

---

## Principle-3

Setiap fase harus menghasilkan artefak yang dapat direview secara independen.

Tidak boleh ada fase yang menghasilkan output sangat besar dan sulit direview.

---

## Principle-4

Gunakan pendekatan:

```text
Small Artifact
→ Review
→ Refine
→ Assemble
```

Hindari:

```text
One Shot Giant Document
```

---

# Existing Artifact

Sudah tersedia:

```text
KPI Knowledge Map
```

Struktur:

```text
Entity
 └─ Business Intent
     └─ Business Data Source
         └─ KPI
```

Artifact ini menjadi fondasi seluruh pekerjaan berikutnya.

---

# Phase-1
# KPI Encyclopedia

## Goal

Membuat referensi bisnis untuk setiap Business Data Source.

Tujuan:

Owner memahami arti KPI yang ada.

---

## Unit Of Work

```text
1 Business Data Source
```

Contoh:

```text
Customer
 └─ Risiko Piutang
     └─ Piutang Exposure
```

---

## Output

```text
ENC-<Entity>-<Sequence>.html
```

Contoh:

```text
ENC-CUST-001.html
ENC-CUST-002.html

ENC-ITEM-001.html

ENC-SALES-001.html

ENC-SUPP-001.html
```

---

## Content

Per Business Data Source:

- Tujuan Area
- Pertanyaan Bisnis
- KPI List

Per KPI:

- Apa Artinya
- Mengapa Penting
- Cara Membaca
- Indikasi Perhatian
- KPI Terkait
- Next KPI To Check
- Typical Owner Action

---

## Review Criteria

- Mudah dipahami Owner
- Tidak mengandung istilah teknis
- KPI dapat dipahami secara mandiri

---

# Phase-2
# Business Question Inventory

## Goal

Mengidentifikasi seluruh pertanyaan bisnis yang dapat dijawab oleh Portal.

---

## Input

- KPI Knowledge Map
- KPI Encyclopedia

---

## Output

```text
BUSINESS-QUESTION-INVENTORY.md
```

---

## Structure

Untuk setiap pertanyaan:

```text
Question ID

Question

Business Objective

Primary Entity

Related Entity
```

---

## Example

```text
Q-001

Apakah bisnis saya sehat?

Business Objective:
Mengetahui kondisi umum bisnis.

Primary Entity:
Customer

Related Entity:
Item
Salesman
Supplier
```

---

## Expected Volume

Target:

```text
15 - 25
Core Business Questions
```

Bukan seluruh kemungkinan pertanyaan.

Fokus pada pertanyaan yang benar-benar relevan bagi Owner.

---

## Review Criteria

- Tidak ada duplikasi pertanyaan
- Tidak terlalu teknis
- Berorientasi keputusan bisnis

---

# Phase-3
# Navigation Registry

## Goal

Memetakan setiap Business Question ke jalur navigasi dashboard.

Ini adalah jantung Navigation Playbook.

---

## Input

- KPI Knowledge Map
- KPI Encyclopedia
- Business Question Inventory

---

## Output

```text
NAVIGATION-REGISTRY.md
```

---

## Structure

Untuk setiap Question:

```text
Question

Dashboard

Business Data Source

KPI

Interpretation

Next KPI

Decision
```

---

## Example

```text
Q-004

Piutang mana yang perlu ditagih?

Dashboard:
Receivable Overview

Data Source:
Piutang Exposure

KPI:
Overdue Exposure

Jika Tinggi:
↓

Next KPI:
Top Overdue Customers

Decision:
Collection Review
```

---

## Review Criteria

- Jalur navigasi jelas
- Tidak ada langkah yang ambigu
- Menghasilkan keputusan bisnis yang masuk akal

---

# Phase-4
# Navigation Playbook Assembly

## Goal

Merakit seluruh artefak menjadi satu playbook.

---

## Input

- KPI Knowledge Map
- KPI Encyclopedia
- Business Question Inventory
- Navigation Registry

---

## Output

```text
BTR-OWNER-NAVIGATION-PLAYBOOK.html
```

---

## Structure

### Cover

---

### How To Use This Guide

---

### Executive Questions

Daftar seluruh pertanyaan bisnis.

---

### Navigation Playbook

Untuk setiap Question:

```text
Question
 ↓
Dashboard
 ↓
Data Source
 ↓
KPI
 ↓
Interpretation
 ↓
Next KPI
 ↓
Decision
```

---

### KPI Reference

Ringkasan KPI Encyclopedia.

---

### Appendix

- KPI Knowledge Map
- Entity Index
- KPI Index

---

## Review Criteria

Owner dapat:

- Menemukan dashboard yang relevan
- Menemukan KPI yang relevan
- Memahami langkah investigasi berikutnya
- Menentukan tindakan bisnis

---

# Optional Phase-5
# Investigation Playbook

## Goal

Menyediakan root-cause analysis path.

---

## Output

```text
INVESTIGATION-PLAYBOOK.md
```

---

## Structure

```text
Problem

↓

Investigation

↓

Root Cause

↓

Decision

↓

Recommended Action
```

---

## Example

```text
Overdue Exposure Naik

↓

Top Overdue Customers

↓

Customer X Menyumbang 40%

↓

Credit Review

↓

Kurangi Exposure
```

---

## Status

Optional.

Tidak diperlukan untuk versi pertama Navigation Playbook.

---

# Recommended Execution Order

```text
Phase-1 KPI Encyclopedia
        ↓
Phase-2 Business Question Inventory
        ↓
Phase-3 Navigation Registry
        ↓
Review
        ↓
Phase-4 Playbook Assembly
```

---

# Recommended Slice Strategy

Gunakan slice berdasarkan:

```text
Entity
    ↓
Business Intent
        ↓
Business Data Source
```

Contoh:

Slice-001
Customer
→ Risiko Piutang
→ Piutang Exposure

Slice-002
Customer
→ Risiko Piutang
→ Collection Planning

Slice-003
Customer
→ Customer Lifecycle

Slice-004
Item
→ Inventory Aging

Slice-005
Item
→ Inventory Forecast
```

Pendekatan ini menjaga konteks kecil, hasil lebih stabil, dan biaya token lebih rendah dibanding ekskavasi lintas-entitas dalam satu iterasi.