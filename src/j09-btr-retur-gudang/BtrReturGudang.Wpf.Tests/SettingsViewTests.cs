using System;
using System.Threading;
using System.Windows;
using BtrReturGudang.Wpf.ViewModels;
using BtrReturGudang.Wpf.Views;
using Xunit;

namespace BtrReturGudang.Wpf.Tests
{
    public class SettingsViewTests
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
        public void SettingsView_InitializesAndRendersControls()
        {
            ExecuteInStaThread(() =>
            {
                var vm = new SettingsViewModel();
                var view = new SettingsView(vm);

                Assert.NotNull(view.BtnSaveAll);
                Assert.NotNull(view.TxtWarehouseCode);
                Assert.NotNull(view.TxtServerTargetId);
                Assert.NotNull(view.TxtPcName);
                Assert.NotNull(view.BtnSwitchOperator);
                Assert.NotNull(view.BtnVerifyOperator);
                Assert.NotNull(view.TxtConnString);
                Assert.NotNull(view.BtnTestDb);
                Assert.NotNull(view.TxtApiUrl);
                Assert.NotNull(view.BtnTestApi);
                Assert.NotNull(view.ChkEnableAudio);
            });
        }
    }
}
