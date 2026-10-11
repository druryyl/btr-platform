---
Title: BGud In-Screen Barcode Capture and Registration
Code: BGUD-BARCODE-REG-001
Artifact: FEATURE
Version: 1.0
LastUpdated: 2026-10-09
---

# 1. Purpose

This feature defines how a warehouse operator registers a new Item-to-Barcode mapping directly from within BGud's Barcode Registry module.

It resolves a design gap introduced during the Return Order navigation restructuring (`BGUD-RETURN-ORDER-NAV-001`), where the standalone scan route was detached from primary navigation, while the Register Barcode screen remained configured as a read-only form expecting a pre-filled barcode parameter.

With this feature, the **Register Barcode** screen (`RegisterScreen`) provides direct barcode acquisition capabilities—both through the device camera scanner and manual numeric/text fallback—enabling operators to register barcodes on-demand from the Barcode Registry menu.

---

# 2. Business Outcome

- Warehouse operators can register physical item barcodes directly from the Barcode Registry menu without encountering disabled or empty read-only forms.
- Physical barcodes can be captured quickly using the device camera (`BarcodeScannerView`) while standing in warehouse aisles.
- If a barcode label is damaged, degraded, or difficult to scan, the operator can manually enter the barcode value.
- Immediate local duplicate detection protects against accidental re-registration of existing barcodes before syncing.
- Registration requests are safely captured in the local offline queue and synchronized with the Main Office server during standard synchronization runs.

---

# 3. Participating Domains

| Domain | Responsibility within this feature |
| ------ | ---------------------------------- |
| Barcode Registry | Owns barcode-to-Item mapping intent, normalization, local cache lookup, and offline request staging. |
| Master Data (Item / Barang) | Owns product reference data (`BTR_Brg`) and packaging units (`BTR_BrgSatuan`) against which barcodes are mapped. |
| Inventory (Warehouse Operations) | Supplies the warehouse session context (`locationId` / `warehouseCode`) under which the registration request is captured. |

---

# 4. Trigger

The operator enters "More" -> "Barcode Registry" from the BGud Home screen and taps **"Daftarkan Barcode Baru"** (Register New Barcode).

---

# 5. Preconditions

- The operator is signed in to BGud with an active session bound to a warehouse location.
- Master Item data (`barang_entity`) and active Barcode catalog (`barcode_entity`) have been synchronized to the local SQLite database.
- Camera hardware and runtime permissions are available (for camera-based scanning), with manual input available as a fallback if camera permission is denied.

---

# 6. Operational Flow

```text
[Barcode Registry Screen]
       │
       ▼ Tap "Daftarkan Barcode Baru"
[Register Barcode Screen]
       │
       ├── Case A: Barcode already captured (e.g. from prefilled argument)
       │      └─▶ Display Barcode Card (with "Ganti / Pindai Ulang" action)
       │
       └── Case B: Barcode blank (direct navigation from Barcode Registry)
              ├─▶ Option 1: Point camera at barcode label (BarcodeScannerView)
              │      └─▶ Auto-detects barcode -> Populates Barcode Field
              └─▶ Option 2: Type barcode in manual input field -> Tap "Terapkan"
                     └─▶ Populates Barcode Field
       │
       ▼ (Barcode captured)
[Local Duplicate Probe]
       ├─ If Duplicate Found ──▶ Display Alert Banner & Disable Save
       └─ If Unique ───────────▶ Proceed to Item Selection
       │
       ▼
[Select Master Item]
       ├─ Search local Barang cache by code or name
       └─ Select active Item (inactive items rejected)
       │
       ▼
[Select Packaging Unit (Optional)]
       └─ Choose from Item's small/big unit conversions, or leave empty
       │
       ▼
[Save Registration]
       ├─ Tap "Simpan Registrasi Barcode"
       └─ Write BarcodeRegistrationRequestEntity (Status: PENDING) to local Room DB
       │
       ▼
[Success Confirmation] ──▶ Return to Barcode Registry
```

---

# 7. Domain Rules & Invariants

- **SCAN-FIRST (UX-002):** Scanning physical barcodes via the camera viewfinder is the primary interaction model; manual typing is a supported fallback.
- **VO-01 Normalization:** Barcode text is normalized upon capture:
  1. Strip CR, LF, and TAB characters.
  2. Trim leading and trailing whitespace.
  3. Preserve leading zeros.
  4. Compare case-insensitively using uppercase key.
- **BR-004 Active Item:** A barcode can only be registered against an Item that is present in the local cache and marked Active (`isAktif == true`).
- **BR-005 Optional Unit:** The packaging unit is optional. When selected, it must match one of the Item's configured unit conversion levels (Small Unit / Big Unit).
- **BR-011 / BR-012 Offline Queue:** Offline registrations are stored locally as requests (`PENDING`). They become authoritative only after synchronization to the Main Office server.
- **Duplicate Prevention:** If the barcode already exists in the local active catalog, the system surfaces a duplicate warning banner and prohibits saving.

---

# 8. Constraints

- **Offline Operation:** The entire registration workflow must function without network connectivity.
- **Scanner Agnostic:** The underlying domain model accepts normalized barcode string values regardless of whether they were acquired via camera scanner or manual entry.
- **Tenant Scoping:** The request records the current session's `warehouseCode` and must not accept external or manual tenant inputs.

---

# 9. Exceptions

| Exception | Handling |
| --------- | -------- |
| Camera permission denied | Camera viewfinder is replaced by an informative permission prompt card, and the manual input field remains fully functional. |
| Barcode already registered in local cache | System highlights the existing Item mapping and disables the Save button. Operator can tap "Ganti / Pindai Ulang". |
| Selected Item is inactive in master data | System shows "Item sudah tidak aktif di master data" and prevents saving. |
| Operator wants to re-scan after capture | Operator taps "Ganti / Pindai Ulang" on the barcode card to clear the current barcode and re-arm the scanner/input. |

---

# 10. Acceptance Criteria

1. Navigating from Barcode Registry to "Daftarkan Barcode Baru" opens the Register Barcode screen with an active camera scanner and manual input field.
2. Scanning a physical barcode automatically captures and normalizes the barcode value, displaying it in the barcode card.
3. Typing a barcode in manual entry and tapping "Terapkan" populates the barcode value.
4. If a barcode is captured, a "Ganti / Pindai Ulang" button allows clearing the barcode to scan or type another.
5. If the barcode already exists in the local database, an alert is displayed and saving is disabled.
6. The operator can search and select an active Item, select an optional unit, and tap "Simpan Registrasi Barcode".
7. Saving stores a new `PENDING` request in `barcode_registration_request_entity` and presents the success confirmation card.
8. Backward compatibility: If the screen is opened with a pre-filled barcode parameter (`register?barcode={code}`), the pre-filled barcode is immediately displayed and the camera is not armed unless the user chooses to re-scan.
