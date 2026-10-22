using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using BtrReturGudang.AppTier.Contracts;
using BtrReturGudang.AppTier.Repositories;
using BtrReturGudang.AppTier.Services;
using BtrReturGudang.Wpf.ViewModels;

namespace BtrReturGudang.Wpf.Views
{
    /// <summary>
    /// Interaction logic for BarcodeRegistryView.xaml.
    /// SCR-06: Barcode Catalog Management &amp; Quick Registration Inspector view.
    /// Provides search filtering across local catalog and registration queues,
    /// colored status badges, pagination, barcode deletion, and modal registration dialog.
    /// Traceable to: ARCHITECTURE §5.2 SCR-06, TD-006, TD-011, TD-013, P4-S10.
    /// </summary>
    public partial class BarcodeRegistryView : UserControl
    {
        private BarcodeRegistryViewModel _viewModel;

        public BarcodeRegistryViewModel ViewModel => _viewModel ?? (DataContext as BarcodeRegistryViewModel);

        public BarcodeRegistryView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        public BarcodeRegistryView(BarcodeRegistryViewModel viewModel) : this()
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            DataContext = _viewModel;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is BarcodeRegistryViewModel vm)
            {
                _viewModel = vm;

                // Configure registration dialog if not custom configured
                if (_viewModel.ShowAddBarcodeDialogFunc == null)
                {
                    _viewModel.ShowAddBarcodeDialogFunc = ShowBarcodeRegistrationDialogAsync;
                }

                if (_viewModel.AllItems == null || _viewModel.AllItems.Count == 0)
                {
                    await _viewModel.LoadBarcodesAsync();
                }
            }
        }

        private Task<bool> ShowBarcodeRegistrationDialogAsync()
        {
            var window = Window.GetWindow(this);
            var barcodeRepo = App.ServiceProvider?.GetService(typeof(IBarcodeRepo)) as IBarcodeRepo;
            var barcodeService = App.ServiceProvider?.GetService(typeof(IBarcodeService)) as IBarcodeService;
            var referenceRepo = App.ServiceProvider?.GetService(typeof(IReferenceRepo)) as IReferenceRepo;
            var registryHelper = App.ServiceProvider?.GetService(typeof(IRegistryHelper)) as IRegistryHelper;
            var audioHelper = App.ServiceProvider?.GetService(typeof(IAudioHelper)) as IAudioHelper;

            var dlg = new BarcodeRegistrationDialog(
                initialBarcode: string.Empty,
                barcodeService: barcodeService,
                barcodeRepo: barcodeRepo,
                referenceRepo: referenceRepo,
                registryHelper: registryHelper,
                audioHelper: audioHelper);

            if (window != null)
            {
                dlg.Owner = window;
            }

            bool? result = dlg.ShowDialog();
            return Task.FromResult(result == true);
        }
    }
}
