using System;
using System.Windows;
using System.Windows.Input;
using BtrReturGudang.AppTier.Contracts;
using BtrReturGudang.Wpf.ViewModels;

namespace BtrReturGudang.Wpf.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml.
    /// SCR-01: Main Application Shell Window hosting docked sidebar navigation,
    /// dynamic view container (ContentControl), workstation header, operator session badge,
    /// 5-minute background sync timer, global hotkeys, and bottom status bar.
    /// Traceable to: ARCHITECTURE §4 TD-011, TD-012, TD-013, §5.2 SCR-01, P2-S04.
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(MainViewModel viewModel) : this()
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            DataContext = _viewModel;

            _viewModel.OnSwitchOperatorRequested = () =>
            {
                var regHelper = App.ServiceProvider?.GetService(typeof(IRegistryHelper)) as IRegistryHelper;
                var dlg = new OperatorSwitcherDialog(regHelper) { Owner = this };
                if (dlg.ShowDialog() == true)
                {
                    _viewModel.OperatorName = dlg.SelectedOperatorName;
                    _viewModel.OperatorEmail = dlg.SelectedOperatorEmail;
                    _ = _viewModel.ValidateOperatorSessionAsync();
                }
            };

            Loaded += OnWindowLoaded;
            Closed += OnWindowClosed;
        }

        private async void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            // Auto-maximize on 720p/768p displays per ARCHITECTURE TD-013
            if (SystemParameters.WorkArea.Height <= 768 || SystemParameters.WorkArea.Width <= 1366)
            {
                WindowState = WindowState.Maximized;
            }

            // Attach hardware scanner hook
            _viewModel?.ScannerListener?.Attach();

            // Validate operator session gate on startup
            if (_viewModel != null)
            {
                await _viewModel.ValidateOperatorSessionAsync();
            }
        }

        private void OnWindowClosed(object sender, EventArgs e)
        {
            _viewModel?.ScannerListener?.Detach();
            _viewModel?.Dispose();
        }

        /// <summary>
        /// Global keyboard hotkey routing (F1, F3, F4, F5, F9, F10, Esc).
        /// Supplements Window.InputBindings to guarantee interception even when focused child controls capture keys.
        /// </summary>
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (ViewModel == null)
            {
                base.OnPreviewKeyDown(e);
                return;
            }

            switch (e.Key)
            {
                case Key.F1:
                    ViewModel.NavigateToIntakeCommand?.Execute(null);
                    e.Handled = true;
                    break;
                case Key.F2:
                    if (ViewModel.CurrentViewModel is ReturnOrderEntryViewModel returnOrderVm)
                    {
                        returnOrderVm.TriggerManualItemSearch();
                        e.Handled = true;
                    }
                    break;
                case Key.F3:
                    ViewModel.NavigateToDaftarReturCommand?.Execute(null);
                    e.Handled = true;
                    break;
                case Key.F4:
                    ViewModel.NavigateToBarcodeRegistryCommand?.Execute(null);
                    e.Handled = true;
                    break;
                case Key.F5:
                    ViewModel.NavigateToSettingsCommand?.Execute(null);
                    e.Handled = true;
                    break;
                case Key.F9:
                    ViewModel.SaveDraftCommand?.Execute(null);
                    e.Handled = true;
                    break;
                case Key.F10:
                    ViewModel.SaveAndUploadCommand?.Execute(null);
                    e.Handled = true;
                    break;
                case Key.Escape:
                    ViewModel.CancelOrClearCommand?.Execute(null);
                    e.Handled = true;
                    break;
            }

            base.OnPreviewKeyDown(e);
        }

        public MainViewModel ViewModel => _viewModel ?? DataContext as MainViewModel;
    }
}
