using System;
using System.Windows;
using System.Windows.Controls;
using BtrReturGudang.AppTier.Contracts;
using BtrReturGudang.Wpf.ViewModels;

namespace BtrReturGudang.Wpf.Views
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml.
    /// SCR-07: Workstation Settings &amp; Hardware Configuration view.
    /// Manages Depo/Warehouse code assignment, PC machine identification, shift operator switcher,
    /// local SQL Server connection tester, cloud API relay ping test, and audio feedback test/toggle.
    /// Traceable to: ARCHITECTURE §5.2 SCR-07, TD-009, TD-011, TD-012, TD-013, P4-S10.
    /// </summary>
    public partial class SettingsView : UserControl
    {
        private SettingsViewModel _viewModel;

        public SettingsViewModel ViewModel => _viewModel ?? (DataContext as SettingsViewModel);

        public SettingsView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        public SettingsView(SettingsViewModel viewModel) : this()
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            DataContext = _viewModel;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is SettingsViewModel vm)
            {
                _viewModel = vm;

                if (_viewModel.SwitchOperatorDialogFunc == null)
                {
                    _viewModel.SwitchOperatorDialogFunc = ShowOperatorSwitcherDialog;
                }
            }
        }

        private Tuple<string, string> ShowOperatorSwitcherDialog()
        {
            var window = Window.GetWindow(this);
            var registryHelper = App.ServiceProvider?.GetService(typeof(IRegistryHelper)) as IRegistryHelper;

            var dlg = new OperatorSwitcherDialog(registryHelper);
            if (window != null)
            {
                dlg.Owner = window;
            }

            if (dlg.ShowDialog() == true)
            {
                return Tuple.Create(dlg.SelectedOperatorName, dlg.SelectedOperatorEmail);
            }

            return null;
        }
    }
}
