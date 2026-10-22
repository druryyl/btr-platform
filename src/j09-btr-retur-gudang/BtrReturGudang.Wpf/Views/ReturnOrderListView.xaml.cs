using System;
using System.Windows.Controls;
using BtrReturGudang.Wpf.ViewModels;

namespace BtrReturGudang.Wpf.Views
{
    /// <summary>
    /// Interaction logic for ReturnOrderListView.xaml (SCR-05).
    /// Realizes the responsive master-detail inspector layout with post-sync locked banner.
    /// </summary>
    public partial class ReturnOrderListView : UserControl
    {
        public ReturnOrderListView()
        {
            InitializeComponent();
            Loaded += async (s, e) =>
            {
                if (DataContext is ReturnOrderListViewModel vm && vm.Orders.Count == 0)
                {
                    await vm.LoadOrdersAsync();
                }
            };
        }
    }
}
