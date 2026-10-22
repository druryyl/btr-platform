using System;
using System.Threading;
using System.Windows;
using BtrReturGudang.Wpf.ViewModels;
using BtrReturGudang.Wpf.Views;
using Xunit;

namespace BtrReturGudang.Wpf.Tests
{
    public class BarcodeRegistryViewTests
    {
        private static void ExecuteInStaThread(Action action)
        {
            Exception exception = null;
            var thread = new Thread(() =>
            {
                try
                {
                    if (Application.Current == null)
                    {
                        try
                        {
                            new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
                        }
                        catch (InvalidOperationException)
                        {
                        }
                    }
                    if (Application.Current != null && Application.Current.Dispatcher.CheckAccess() && Application.Current.Resources.MergedDictionaries.Count == 0)
                    {
                        Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
                        {
                            Source = new Uri("pack://application:,,,/BtrReturGudang.Wpf;component/Themes/Colors.xaml", UriKind.Absolute)
                        });
                        Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
                        {
                            Source = new Uri("pack://application:,,,/BtrReturGudang.Wpf;component/Themes/Styles.xaml", UriKind.Absolute)
                        });
                        Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
                        {
                            Source = new Uri("pack://application:,,,/BtrReturGudang.Wpf;component/Themes/DataGrid.xaml", UriKind.Absolute)
                        });
                    }
                    action();
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            if (exception != null)
            {
                throw new AggregateException("STA thread execution failed", exception);
            }
        }

        [Fact]
        public void BarcodeRegistryView_InitializesAndRendersControls()
        {
            ExecuteInStaThread(() =>
            {
                var vm = new BarcodeRegistryViewModel();
                var view = new BarcodeRegistryView(vm);

                Assert.NotNull(view.BtnAddBarcode);
                Assert.NotNull(view.BtnRefresh);
                Assert.NotNull(view.TxtSearch);
                Assert.NotNull(view.CboStatusFilter);
                Assert.NotNull(view.DgvBarcodes);
                Assert.NotNull(view.BtnPrevPage);
                Assert.NotNull(view.BtnNextPage);
            });
        }
    }
}
