using System;
using System.Threading;
using System.Windows;
using BtrReturGudang.AppTier.Contracts;
using BtrReturGudang.Wpf.Views;
using Xunit;

namespace BtrReturGudang.Wpf.Tests
{
    public class OperatorSwitcherDialogTests
    {
        private class FakeRegistryHelper : IRegistryHelper
        {
            public string WarehouseCode { get; set; } = "GDG-PST";
            public string ServerTargetId { get; set; } = "SRV-PST";
            public string ApiBaseUrl { get; set; } = "http://localhost:5000/";
            public string LastOperatorName { get; set; } = "Operator Awal";
            public string LastOperatorEmail { get; set; } = "awal@btr.co.id";
            public bool EnableSoundFeedback { get; set; } = true;
        }

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
        public void OperatorSwitcherDialog_InitializesWithCurrentRegistryValues()
        {
            ExecuteInStaThread(() =>
            {
                var reg = new FakeRegistryHelper();
                var dlg = new OperatorSwitcherDialog(reg);

                Assert.Equal("Operator Awal", dlg.TxtOperatorName.Text);
                Assert.Equal("awal@btr.co.id", dlg.TxtOperatorEmail.Text);
            });
        }

        [Fact]
        public void OperatorSwitcherDialog_SaveUpdatesRegistryAndExposesProperties()
        {
            ExecuteInStaThread(() =>
            {
                var reg = new FakeRegistryHelper();
                var dlg = new OperatorSwitcherDialog(reg);

                dlg.TxtOperatorName.Text = "Hendra Wijaya";
                dlg.TxtOperatorEmail.Text = "hendra@btr.co.id";

                // Simulate save click
                dlg.BtnSave.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));

                Assert.Equal("Hendra Wijaya", reg.LastOperatorName);
                Assert.Equal("hendra@btr.co.id", reg.LastOperatorEmail);
                Assert.Equal("Hendra Wijaya", dlg.SelectedOperatorName);
                Assert.Equal("hendra@btr.co.id", dlg.SelectedOperatorEmail);
            });
        }
    }
}
