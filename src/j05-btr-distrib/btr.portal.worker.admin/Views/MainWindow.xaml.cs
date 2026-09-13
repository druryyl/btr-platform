using System.Windows;
using btr.portal.worker.admin.ViewModels;

namespace btr.portal.worker.admin.Views
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;

            Loaded += async (s, e) => await _viewModel.HealthViewModel.LoadHealthAsync();
            Closing += (s, e) => _viewModel.Dispose();
        }
    }
}
