using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BtrReturGudang.AppTier.Contracts;
using BtrReturGudang.AppTier.Repositories;
using BtrReturGudang.Domain.Entities;
using BtrReturGudang.Wpf.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace BtrReturGudang.Wpf.Views
{
    /// <summary>
    /// Interaction logic for ManualCustomerSearchDialog.xaml (SCR-08).
    /// Keyboard-driven customer lookup modal dialog supporting real-time search,
    /// arrow key navigation, and walk-in fallback (CUST-MANUAL).
    /// Traceable to: ARCHITECTURE §5.2 SCR-08, TD-011, TD-013, P3-S08.
    /// </summary>
    public partial class ManualCustomerSearchDialog : Window
    {
        private ManualCustomerSearchViewModel _viewModel;

        public ManualCustomerSearchViewModel ViewModel =>
            _viewModel ?? DataContext as ManualCustomerSearchViewModel;

        public Customer SelectedCustomer => ViewModel?.SelectedCustomer;

        public ManualCustomerSearchDialog(ManualCustomerSearchViewModel viewModel)
        {
            InitializeComponent();
            AttachViewModel(viewModel);

            Loaded += OnWindowLoaded;
            PreviewKeyDown += OnWindowPreviewKeyDown;
            TxtSearch.PreviewKeyDown += OnSearchPreviewKeyDown;
            DgvCustomers.PreviewKeyDown += OnGridPreviewKeyDown;
            DgvCustomers.MouseDoubleClick += OnGridMouseDoubleClick;
        }

        public ManualCustomerSearchDialog(
            IReferenceRepo referenceRepo = null,
            IAudioHelper audioHelper = null,
            string initialKeyword = null)
            : this(new ManualCustomerSearchViewModel(
                referenceRepo ?? App.ServiceProvider?.GetService<IReferenceRepo>(),
                audioHelper ?? App.ServiceProvider?.GetService<IAudioHelper>(),
                initialKeyword))
        {
        }

        public ManualCustomerSearchDialog()
            : this(null, null, null)
        {
        }

        private void AttachViewModel(ManualCustomerSearchViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            DataContext = _viewModel;

            _viewModel.RequestClose += OnRequestClose;
            _viewModel.RequestFocusGrid += OnRequestFocusGrid;
            _viewModel.RequestFocusSearchBox += OnRequestFocusSearchBox;
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            TxtSearch.Focus();
            if (!string.IsNullOrWhiteSpace(TxtSearch.Text))
            {
                TxtSearch.SelectAll();
            }
        }

        private void OnRequestClose(bool? result)
        {
            try
            {
                DialogResult = result;
            }
            catch (InvalidOperationException)
            {
                // In headless unit test environments, window is not shown with ShowDialog()
            }
            Close();
        }

        private void OnRequestFocusGrid()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                DgvCustomers.Focus();
                if (DgvCustomers.SelectedItem != null)
                {
                    DgvCustomers.ScrollIntoView(DgvCustomers.SelectedItem);
                }
            }));
        }

        private void OnRequestFocusSearchBox()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                TxtSearch.Focus();
                TxtSearch.SelectAll();
            }));
        }

        private void OnWindowPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                ViewModel?.CancelCommand?.Execute(null);
            }
            else if (e.Key == Key.F6)
            {
                e.Handled = true;
                ViewModel?.SelectWalkInCustomerCommand?.Execute(null);
            }
        }

        private void OnSearchPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down)
            {
                e.Handled = true;
                ViewModel?.NavigateDownCommand?.Execute(null);
                if (DgvCustomers.SelectedItem != null)
                {
                    DgvCustomers.ScrollIntoView(DgvCustomers.SelectedItem);
                }
            }
            else if (e.Key == Key.Up)
            {
                e.Handled = true;
                ViewModel?.NavigateUpCommand?.Execute(null);
                if (DgvCustomers.SelectedItem != null)
                {
                    DgvCustomers.ScrollIntoView(DgvCustomers.SelectedItem);
                }
            }
            else if (e.Key == Key.Enter)
            {
                e.Handled = true;
                ViewModel?.ConfirmSelectionCommand?.Execute(null);
            }
        }

        private void OnGridPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                ViewModel?.ConfirmSelectionCommand?.Execute(null);
            }
        }

        private void OnGridMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Only confirm if a row item was double-clicked
            if (DgvCustomers.SelectedItem != null)
            {
                e.Handled = true;
                ViewModel?.ConfirmSelectionCommand?.Execute(null);
            }
        }
    }
}
