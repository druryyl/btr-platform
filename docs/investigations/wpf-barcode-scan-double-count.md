# Investigation Report: Barcode Scan Double Increment in BtrReturGudang.Wpf

## Executive Summary
In `BtrReturGudang.Wpf`, scanning an item barcode unexpectedly increments the quantity by +2 instead of +1.
Investigation reveals that a single barcode scan triggers **two separate dispatches** to `ReturnOrderEntryViewModel.OnBarcodeScanned`:
1. Dispatch 1: from `MainViewModel.OnBarcodeScannedReceived` (subscribed to `WpfScannerWedgeListener.BarcodeScannedDetailed`).
2. Dispatch 2: from `WpfScannerWedgeListener.DispatchBarcode` (directly invoking `ActiveConsumer?.OnBarcodeScanned`).

Due to warehouse ergonomics (consecutive scan of the same barcode increments quantity), Dispatch 1 stages the item with `Qty = 1`, and Dispatch 2 immediately sees the staged item with matching barcode and increments `Qty += 1`, resulting in `Qty = 2`.

---

## 1. Problem Description
- **Target**: `src/j09-btr-retur-gudang/BtrReturGudang.Wpf` (WPF desktop client for Warehouse Return Order Intake).
- **Observed Behavior**: Scanning an item barcode once causes the active item quantity (`QtyBagus` or `QtyRusak`) to increment by +2 (and plays two audio success beeps). Consecutive scans of the same barcode add +2 on each scan instead of +1.
- **Expected Behavior**: Each barcode scan must increment the item quantity by exactly +1.

---

## 2. Root Cause Analysis

### A. The Dual Dispatch Mechanism
In `BtrReturGudang.Wpf\Services\WpfScannerWedgeListener.cs`:
```csharp
private void DispatchBarcode(string rawBarcode, string normalizedBarcode)
{
    BarcodeScanned?.Invoke(normalizedBarcode);
    BarcodeScannedDetailed?.Invoke(this, new BarcodeScannedEventArgs(rawBarcode, normalizedBarcode));
    ActiveConsumer?.OnBarcodeScanned(normalizedBarcode);
}
```
Notice that `DispatchBarcode` dispatches through two pathways:
1. It invokes `BarcodeScannedDetailed`.
2. It directly calls `ActiveConsumer?.OnBarcodeScanned(normalizedBarcode)`.

### B. Shell Wiring in MainViewModel
In `BtrReturGudang.Wpf\ViewModels\MainViewModel.cs`:
1. In the constructor:
```csharp
if (_scannerListener != null)
{
    _scannerListener.BarcodeScannedDetailed += OnBarcodeScannedReceived;
    UpdateScannerStatus();
}
```
2. In `NavigateTo(NavigationModule module)`:
```csharp
CurrentViewModel = targetViewModel;

// Route scanner listener to the newly active view model if it consumes scans
if (_scannerListener != null)
{
    _scannerListener.ActiveConsumer = CurrentViewModel as IScannerConsumer;
}
```
3. In `OnBarcodeScannedReceived`:
```csharp
private void OnBarcodeScannedReceived(object sender, BarcodeScannedEventArgs e)
{
    StatusInfoText = $"Scan Terakhir: [{e.NormalizedBarcode}] pada {DateTime.Now:HH:mm:ss}";

    if (CurrentViewModel is IScannerConsumer consumer)
    {
        consumer.OnBarcodeScanned(e.NormalizedBarcode);
    }
}
```

### C. Execution Sequence During a Single Scan
When a physical barcode is scanned (or `SimulateScan` is called):
1. `WpfScannerWedgeListener.DispatchBarcode` executes:
   - Line 291: `BarcodeScannedDetailed?.Invoke(...)` executes first.
     - `MainViewModel.OnBarcodeScannedReceived` handles the event.
     - `MainViewModel` calls `consumer.OnBarcodeScanned(e.NormalizedBarcode)`.
     - `ReturnOrderEntryViewModel.OnBarcodeScanned` executes (Invocation 1):
       - Since `_currentScanResult` is either null or the previous item, it looks up the barcode.
       - It stages the item via `ApplyActiveProduct(lookup)`.
       - It sets `QtyBagus = 1` (or `QtyRusak = 1` if in Rusak mode).
       - It plays a success beep.
   - Line 292: `ActiveConsumer?.OnBarcodeScanned(normalizedBarcode)` executes immediately after.
     - `ActiveConsumer` references the identical `ReturnOrderEntryViewModel` instance.
     - `ReturnOrderEntryViewModel.OnBarcodeScanned` executes again (Invocation 2):
       - It checks consecutive scan ergonomics:
         ```csharp
         if (_currentScanResult != null &&
             ((!string.IsNullOrWhiteSpace(_currentScanResult.NormalizedBarcode) && _currentScanResult.NormalizedBarcode == clean) ||
              (!string.IsNullOrWhiteSpace(_currentScanResult.BarcodeValue) && _currentScanResult.BarcodeValue == clean)))
         {
             if (ActiveQuantityMode == QuantityEntryMode.Bagus)
             {
                 QtyBagus += 1;
                 ...
         ```
       - Because Invocation 1 just staged `_currentScanResult` matching `clean`, this condition evaluates to `true`!
       - It increments `QtyBagus += 1` -> value becomes **2**!
       - It plays a second success beep!

Subsequent scans of the same barcode trigger the exact same duplicate call sequence:
- Invocation 3: `QtyBagus += 1` -> 3.
- Invocation 4: `QtyBagus += 1` -> 4.
Thus every single barcode scan produces a net change of **+2**.

---

## 3. Why Existing Tests Did Not Catch This
1. In `WorkflowIntegrationTests.cs`, the test cases directly invoke `vm.OnBarcodeScanned(barcode)`. This tests `ReturnOrderEntryViewModel` in isolation without going through `MainViewModel` and `WpfScannerWedgeListener` integration.
2. In `WpfScannerWedgeListenerTests.cs`, tests hook `listener.ActiveConsumer = mockConsumer`, but there is no `MainViewModel` subscribed to `BarcodeScannedDetailed` forwarding to the same consumer.
3. In `MainWindowTests.cs:409` (`MainViewModel_ScannerEvent_DispatchesToActiveConsumer`), `TestOrderActionHandler.OnBarcodeScanned` sets `LastScannedBarcode = barcode`. Because this is an idempotent assignment, double execution still resulted in `LastScannedBarcode == expected`.

---

## 4. Proposed Solution
Eliminate the duplicate dispatch by establishing a single, clear source of event routing:

1. **Keep `ActiveConsumer` on `WpfScannerWedgeListener`**:
   `WpfScannerWedgeListener` is already designed with `public IScannerConsumer ActiveConsumer { get; set; }` and directly dispatches to `ActiveConsumer?.OnBarcodeScanned(normalizedBarcode)`. This contract is tested and adhered to by wedge listener unit tests.

2. **Synchronize `ActiveConsumer` in `MainViewModel.CurrentViewModel`**:
   Update `MainViewModel.CurrentViewModel` property setter so whenever `CurrentViewModel` changes (whether via `NavigateTo` or test injection), `_scannerListener.ActiveConsumer` is updated to `value as IScannerConsumer`.

3. **Remove Duplicate Forwarding in `MainViewModel.OnBarcodeScannedReceived`**:
   `MainViewModel.OnBarcodeScannedReceived` should only update its shell status bar:
   ```csharp
   private void OnBarcodeScannedReceived(object sender, BarcodeScannedEventArgs e)
   {
       StatusInfoText = $"Scan Terakhir: [{e.NormalizedBarcode}] pada {DateTime.Now:HH:mm:ss}";
   }
   ```
   Remove the duplicate `consumer.OnBarcodeScanned(...)` call.

4. **Add End-to-End Orchestration Regression Tests**:
   Add unit tests in `MainWindowTests` and `WorkflowIntegrationTests` that wire `WpfScannerWedgeListener`, `MainViewModel`, and `ReturnOrderEntryViewModel` together, verifying that scanning a barcode results in `Qty = 1` for the first scan, `Qty = 2` for the second scan (+1 increment per physical scan).
