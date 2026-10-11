---
Title: BGud In-Screen Barcode Capture and Registration — Implementation Plan
Code: BGUD-BARCODE-REG-001
Artifact: IMPLEMENTATION-PLAN
Version: 1.0
LastUpdated: 2026-10-09
Status: COMPLETED
Execution Approval: APPROVED
---

# 1. Objective

Enable warehouse operators to capture physical barcodes directly within `RegisterScreen` in the BGud Android application—via camera viewfinder (`BarcodeScannerView`) or manual text entry fallback—resolving the design gap where direct navigation from `BarcodeRegistryScreen` leaves the read-only barcode field empty and unsavable.

Referenced artifacts:
- FEATURE: `docs/features/bgud-barcode-registration/feature.md`
- ARCHITECTURE: `docs/work/barcode-registry/BARCODE-REGISTRY-ARCHITECTURE.md` (§11.2, §12.6, §14.3)
- UX BLUEPRINT: `docs/work/barcode-registry/BARCODE-REGISTRY-UX-BLUEPRINT.md` (UX-001, UX-002, §8)
- NAVIGATION CONTRACT: `docs/work/bgud-return-order-nav/BGUD-RETURN-ORDER-NAV-ARCHITECTURE.md` (TD-003)

---

# 2. Planning Scope

This implementation plan focuses on the BGud Android application layer:
- Extending `RegisterBarcodeViewModel` to support barcode acquisition (`onBarcodeScanned`, `onBarcodeManualSubmit`), duplicate re-probing, and rescan/clearing (`onClearBarcode`).
- Enhancing `RegisterScreen` to display an embedded CameraX viewfinder (`BarcodeScannerView`) and manual input fallback when the barcode value is blank, and a "Ganti / Pindai Ulang" action when a barcode is present.
- Adding comprehensive unit tests for `RegisterBarcodeViewModel`.
- Preserving all downstream behaviors (local Item search, unit selector, local offline queue write, duplicate validation, and pre-filled route compatibility).

Backend systems (`j05-btr-distrib`, `j06-pkl-btrade-api`, `j07-btrade-sync`) and database schemas remain completely unchanged.

---

# 3. Dependencies

- External dependencies: None.
- Internal dependencies:
  - Phase 1 (ViewModel & Tests) must be completed before Phase 2 (UI Integration).
  - Phase 2 must be completed before Phase 3 (End-to-End Build & Validation).

---

# 4. Progress Summary

| Phase | Description | Implementation Status | Review Status | Progress |
| ----- | ----------- | --------------------- | ------------- | -------- |
| P1 | ViewModel Barcode Acquisition & Unit Tests | IMPLEMENTED | GO | 1/1 |
| P2 | RegisterScreen Camera & Manual Input Integration | IMPLEMENTED | GO | 1/1 |
| P3 | Verification & Build Validation | IMPLEMENTED | GO | 1/1 |

---

# 5. Phases and Slices

## Phase 1 — ViewModel Barcode Acquisition & Unit Tests

### Slice P1-S01: Extend `RegisterBarcodeViewModel` and Add Unit Tests

- **Status:** IMPLEMENTED
- **Review Status:** GO
- **Depends On:** None
- **Affected Files:**
  - `src/BGud/app/src/main/java/com/elsasa/bgud/viewmodel/RegisterBarcodeViewModel.kt`
  - `src/BGud/app/src/test/java/com/elsasa/bgud/viewmodel/RegisterBarcodeViewModelTest.kt`

#### Implementation Steps:
1. In `RegisterBarcodeViewModel.kt`:
   - Add method `onBarcodeScanned(rawValue: String)`:
     - Normalize `rawValue` using `BarcodeNormalization.normalize(rawValue)`.
     - Update `_barcodeValue.value`.
     - Probe `barcodeDao.getByKey(BarcodeNormalization.toKey(normalized))` to update `_duplicateFound`.
     - Clear `_saveError.value = null`.
   - Add method `onBarcodeManualSubmit(manualValue: String)`:
     - Perform identical normalization and duplicate probing as `onBarcodeScanned`.
   - Add method `onClearBarcode()`:
     - Reset `_barcodeValue.value = ""`.
     - Reset `_duplicateFound.value = null`.
     - Reset `_saveError.value = null`.
2. Create unit test `RegisterBarcodeViewModelTest.kt`:
   - Test blank initial barcode initialization (`barcodeValue` starts empty).
   - Test `onBarcodeScanned` normalizes input and triggers duplicate detection.
   - Test `onBarcodeManualSubmit` trims and populates barcode.
   - Test `onClearBarcode` clears the current barcode value and duplicate error.
   - Test save preconditions: save succeeds only when barcode is present, active item is selected, and no duplicate exists.

---

## Phase 2 — UI Integration

### Slice P2-S01: Embed Camera Scanner & Manual Input in `RegisterScreen`

- **Status:** IMPLEMENTED
- **Review Status:** GO
- **Depends On:** P1-S01
- **Affected Files:**
  - `src/BGud/app/src/main/java/com/elsasa/bgud/ui/screen/RegisterScreen.kt`

#### Implementation Steps:
1. Integrate Camera runtime permissions in `RegisterScreen.kt`:
   - Add permission state check using `ContextCompat.checkSelfPermission(context, Manifest.permission.CAMERA)`.
   - Add `rememberLauncherForActivityResult(ActivityResultContracts.RequestPermission())`.
2. Adapt Barcode Region based on `barcodeValue`:
   - **Case A: `barcodeValue.isBlank()` (Barcode acquisition mode):**
     - If camera permission granted: Render `BarcodeScannerView(enabled = true, onBarcode = viewModel::onBarcodeScanned, modifier = Modifier.fillMaxWidth().height(240.dp))`.
     - If camera permission not granted: Render permission prompt card with "Berikan Izin Kamera" action button.
     - Render Manual Entry card with `OutlinedTextField(value = manualEntry, onValueChange = ...)` and primary button "Gunakan Barcode / Terapkan" invoking `viewModel.onBarcodeManualSubmit(manualEntry)`.
   - **Case B: `barcodeValue.isNotBlank()` (Barcode captured mode):**
     - Render existing Barcode Read-Only Card displaying `barcodeValue`.
     - Add an action button "Ganti / Pindai Ulang" (Change / Rescan) that calls `viewModel.onClearBarcode()` to re-arm the acquisition mode.
     - Render duplicate alert banner if `duplicateFound != null`.
3. Preserve all existing Item Search, Unit Selector, and Save Button behaviors.

#### Implementation Notes:
- Added camera permission detection and runtime permission launcher (`rememberLauncherForActivityResult`).
- Acquisition mode renders camera viewfinder in a themed `Surface` when permission is granted, or a permission request card with action button when denied.
- Manual barcode entry card provides `OutlinedTextField` and primary button "Terapkan Barcode", delegating to `viewModel.onBarcodeManualSubmit`.
- Captured mode renders the monospace badge with a "Ganti / Pindai Ulang" outlined button, delegating to `viewModel.onClearBarcode`.
- Duplicate alert banner is displayed in captured mode when `duplicateFound != null`.
- Preserved item search, unit dropdown, and save/cancel actions.
- Verified build via `:app:compileDebugKotlin` and verified all unit tests via `:app:testDebugUnitTest --rerun-tasks` (BUILD SUCCESSFUL).

---

## Phase 3 — Verification & Validation

### Slice P3-S01: Compilation & Verification

- **Status:** IMPLEMENTED
- **Review Status:** GO
- **Depends On:** P2-S01
- **Affected Files:** None (Validation phase)

#### Verification Steps:
1. Run Kotlin compilation:
   ```powershell
   ./gradlew :app:compileDebugKotlin
   ```
2. Run Android unit test suite:
   ```powershell
   ./gradlew :app:testDebugUnitTest
   ```
3. Verify test report passes without regressions.

#### Implementation Notes:
- Successfully compiled Kotlin sources via `.\gradlew :app:compileDebugKotlin` in `src/BGud` (BUILD SUCCESSFUL, exit code 0).
- Successfully ran Android unit test suite via `.\gradlew :app:testDebugUnitTest` in `src/BGud` (BUILD SUCCESSFUL, exit code 0). Zero compilation errors and zero test failures.

---

# 6. Operational Checklist

- [ ] `BarcodeScannerView` lifecycle is properly disposed when moving off-screen.
- [ ] Barcodes scanned or typed with whitespace/special characters are cleanly normalized.
- [ ] Duplicate barcodes trigger warning banner and disable "Simpan Registrasi Barcode".
- [ ] Navigating from `BarcodeRegistryScreen` starts in acquisition mode without UI blocking.
- [ ] Navigating with existing pre-filled parameter `register?barcode=123` starts directly in captured mode.
- [ ] Offline queue write operates without network dependencies.
