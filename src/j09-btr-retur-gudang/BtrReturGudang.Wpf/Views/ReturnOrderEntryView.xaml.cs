using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BtrReturGudang.AppTier.Contracts;
using BtrReturGudang.AppTier.Repositories;
using BtrReturGudang.AppTier.Services;
using BtrReturGudang.Wpf.Controls;
using BtrReturGudang.Wpf.ViewModels;

namespace BtrReturGudang.Wpf.Views
{
    /// <summary>
    /// Interaction logic for ReturnOrderEntryView.xaml (SCR-02).
    /// Handles keyboard focus orchestration, input accelerators, and quantity strip commits.
    /// Traceable to: ARCHITECTURE §4 TD-011, TD-013, §5.2 SCR-02, P2-S06.
    /// </summary>
    public partial class ReturnOrderEntryView : UserControl
    {
        public ReturnOrderEntryView()
        {
            InitializeComponent();

            DataContextChanged += OnDataContextChanged;
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;

            // Wire keyboard accelerators in quantity strip inputs
            TxtQtyBagus.PreviewKeyDown += OnQuantityInputKeyDown;
            TxtQtyRusak.PreviewKeyDown += OnQuantityInputKeyDown;
            TxtItemNotes.PreviewKeyDown += OnQuantityInputKeyDown;
            TxtCustomer.PreviewKeyDown += OnCustomerInputKeyDown;
            PreviewKeyDown += OnControlPreviewKeyDown;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // By default, if no item is active, focus customer input or wait for scanner
            if (DataContext is ReturnOrderEntryViewModel vm && vm.CurrentScanResult != null)
            {
                if (vm.IsModeRusak)
                {
                    FocusQtyRusak();
                }
                else
                {
                    FocusQtyBagus();
                }
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ReturnOrderEntryViewModel vm)
            {
                vm.RequestFocusQtyBagus -= FocusQtyBagus;
                vm.RequestFocusQtyRusak -= FocusQtyRusak;
                vm.RequestFocusGrid -= FocusGrid;
            }
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is ReturnOrderEntryViewModel oldVm)
            {
                oldVm.RequestFocusQtyBagus -= FocusQtyBagus;
                oldVm.RequestFocusQtyRusak -= FocusQtyRusak;
                oldVm.RequestFocusGrid -= FocusGrid;
            }

            if (e.NewValue is ReturnOrderEntryViewModel newVm)
            {
                newVm.RequestFocusQtyBagus += FocusQtyBagus;
                newVm.RequestFocusQtyRusak += FocusQtyRusak;
                newVm.RequestFocusGrid += FocusGrid;

                if (newVm.CustomerSearchDialogLauncher == null)
                {
                    newVm.CustomerSearchDialogLauncher = (initialKey) =>
                    {
                        var owner = Window.GetWindow(this);
                        var dlg = new ManualCustomerSearchDialog(initialKeyword: initialKey);
                        if (owner != null)
                        {
                            dlg.Owner = owner;
                        }
                        if (dlg.ShowDialog() == true)
                        {
                            return dlg.SelectedCustomer;
                        }
                        return null;
                    };
                }

                if (newVm.ManualSearchDialogLauncher == null)
                {
                    newVm.ManualSearchDialogLauncher = () =>
                    {
                        var owner = Window.GetWindow(this);
                        var refRepo = App.ServiceProvider?.GetService(typeof(IReferenceRepo)) as IReferenceRepo;
                        var audio = App.ServiceProvider?.GetService(typeof(IAudioHelper)) as IAudioHelper;
                        var dlg = new ManualItemSearchDialog(refRepo, audio);
                        if (owner != null)
                        {
                            dlg.Owner = owner;
                        }
                        if (dlg.ShowDialog() == true && dlg.SelectedBarang != null)
                        {
                            if (!string.IsNullOrWhiteSpace(dlg.SelectedSatuan))
                            {
                                newVm.SetSelectedItem(dlg.SelectedBarang, dlg.SelectedSatuan);
                                return null;
                            }
                            return dlg.SelectedBarang;
                        }
                        return null;
                    };
                }

                if (newVm.QuickRegistrationDialogLauncher == null)
                {
                    newVm.QuickRegistrationDialogLauncher = (unmappedBarcode) =>
                    {
                        var owner = Window.GetWindow(this);
                        var barcodeService = App.ServiceProvider?.GetService(typeof(IBarcodeService)) as IBarcodeService;
                        var barcodeRepo = App.ServiceProvider?.GetService(typeof(IBarcodeRepo)) as IBarcodeRepo;
                        var refRepo = App.ServiceProvider?.GetService(typeof(IReferenceRepo)) as IReferenceRepo;
                        var regHelper = App.ServiceProvider?.GetService(typeof(IRegistryHelper)) as IRegistryHelper;
                        var audio = App.ServiceProvider?.GetService(typeof(IAudioHelper)) as IAudioHelper;

                        var dlg = new QuickBarcodeRegistrationDialog(
                            unmappedBarcode,
                            barcodeService,
                            barcodeRepo,
                            refRepo,
                            regHelper,
                            audio);
                        if (owner != null)
                        {
                            dlg.Owner = owner;
                        }
                        if (dlg.ShowDialog() == true && dlg.RegisteredResult != null)
                        {
                            return dlg.RegisteredResult;
                        }
                        return null;
                    };
                }
            }
        }

        public void FocusQtyBagus()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                TxtQtyBagus.Focus();
                TxtQtyBagus.SelectAll();
            }));
        }

        public void FocusQtyRusak()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                TxtQtyRusak.Focus();
                TxtQtyRusak.SelectAll();
            }));
        }

        public void FocusGrid()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                DgvItems.Focus();
            }));
        }

        private void OnControlPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F2)
            {
                e.Handled = true;
                if (DataContext is ReturnOrderEntryViewModel vm)
                {
                    vm.TriggerManualItemSearch();
                }
            }
            else if (e.Key == Key.F8)
            {
                e.Handled = true;
                if (DataContext is ReturnOrderEntryViewModel vm)
                {
                    vm.CustomerSearchCommand.Execute(null);
                }
            }
        }

        private void OnQuantityInputKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                if (DataContext is ReturnOrderEntryViewModel vm)
                {
                    vm.CommitQuantityStripToGrid();
                }
            }
        }

        private void OnCustomerInputKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.F8)
            {
                e.Handled = true;
                if (DataContext is ReturnOrderEntryViewModel vm)
                {
                    vm.CustomerSearchCommand.Execute(null);
                }
            }
        }
    }
}
