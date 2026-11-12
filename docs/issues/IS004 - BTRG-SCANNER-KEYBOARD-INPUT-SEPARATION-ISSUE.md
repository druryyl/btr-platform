# ISSUE

## Metadata

ID: BTRG-SCANNER-INPUT-SEPARATION-001
Type: BUG
Status: Open
Title: Barcode Scanner Input Leaking into Focused UI Controls as Keyboard Input in BtrReturGudang.Wpf

## Source

Reported By: User
Reported Date: 2026-11-12

## Description

`BtrReturGudang.Wpf` is designed to minimize manual keyboard usage by enabling warehouse operators to perform operational workflows and actions using barcode scans (e.g., scanning item barcodes, scanning command barcodes).

Although scanner wedge interception was previously introduced, the discrimination between barcode scanner input and physical keyboard input is incomplete. Barcode scanner input is still processed through the standard keyboard message pipeline and leaks into whatever UI control currently has keyboard focus.

Observed scenarios in the Retur Intake screen (`ReturnOrderEntryView`):
1. **Accidental Dialog Triggering**: After selecting a customer, the user scans an item barcode. If the UI focus happens to rest on the "Cari Customer" button (or shortcut trigger), the incoming scan characters/signals trigger the button, opening the customer search dialog instead of scanning the item.
2. **Polluted Data Entry**: When scanning the same item barcode consecutively to increment quantity (e.g., scanning twice for quantity 2), if the UI focus is positioned inside the Qty textbox, the second scan dumps the raw barcode digits directly into the Qty textbox instead of incrementing the item quantity.
3. **Ergonomic Inefficiency**: Operators are forced to constantly verify and manually manage UI focus before scanning, defeating the operational design goal of seamless, scanner-driven warehouse operations.

## Desired Outcome

- The application cleanly discriminates between input originating from the barcode scanner and manual typing from the physical keyboard.
- Barcode scanner input is captured and dispatched to the intended domain/scanner handler regardless of which UI control holds focus, without polluting textboxes or triggering focused buttons.
- Physical keyboard input remains fully functional for manual user input and navigation when required.
- Warehouse operators can scan barcodes consecutively and fluidly without having to manage UI focus states manually.

## Current Situation

- Scanner input is handled via keyboard emulation wedge (`WpfScannerWedgeListener`), which lets printable keystrokes pass through to active controls while only attempting to intercept the terminating Enter keystroke.
- UI controls (e.g., search buttons, quantity text fields) actively receive raw keystrokes during a barcode scan burst whenever they hold focus.
- No reliable hardware-level or message-level distinction exists to prevent scanner keystrokes from being consumed by focused WPF elements.

## Evidence

- Target Project: `src/j09-btr-retur-gudang/BtrReturGudang.Wpf`
- Scanner Listener Service: [WpfScannerWedgeListener.cs](file:///d:/Project.Private/btr-platform/src/j09-btr-retur-gudang/BtrReturGudang.Wpf/Services/WpfScannerWedgeListener.cs)
- Affected UI View: [ReturnOrderEntryView.xaml](file:///d:/Project.Private/btr-platform/src/j09-btr-retur-gudang/BtrReturGudang.Wpf/Views/ReturnOrderEntryView.xaml)
- Affected View Logic: [ReturnOrderEntryView.xaml.cs](file:///d:/Project.Private/btr-platform/src/j09-btr-retur-gudang/BtrReturGudang.Wpf/Views/ReturnOrderEntryView.xaml.cs)

## Notes

- This issue is classified as a BUG because the existing scanner wedge implementation fails to fulfill the intended behavior of hands-free scanner processing, creating unintended side effects in active UI forms.
- Investigation into the technical mechanism for discriminating or isolating scanner input (e.g., Raw Input API, input suppression, timing heuristics, or device communication modes) will be conducted in the downstream BUG-INVESTIGATION stage.
