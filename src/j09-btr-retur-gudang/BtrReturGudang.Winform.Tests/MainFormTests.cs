using System;
using System.Threading;
using System.Windows.Forms;
using BtrReturGudang.AppTier.Contracts;
using BtrReturGudang.AppTier.Repositories;
using BtrReturGudang.AppTier.Services;
using BtrReturGudang.Infrastructure.Helpers;
using BtrReturGudang.Winform;
using BtrReturGudang.Winform.Hardware;
using BtrReturGudang.Winform.Views;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BtrReturGudang.Winform.Tests
{
    public class MainFormTests
    {
        private ServiceProvider CreateTestServiceProvider()
        {
            var services = new ServiceCollection();
            Program.ConfigureServices(services);
            return services.BuildServiceProvider();
        }

        [Fact]
        public void DependencyInjection_WiresAllRequiredServicesSuccessfully()
        {
            ExecuteInStaThread(() =>
            {
                using (var provider = CreateTestServiceProvider())
                {
                    // Assert repository registrations
                    Assert.NotNull(provider.GetService<IReturnOrderRepo>());
                    Assert.NotNull(provider.GetService<IBarcodeRepo>());
                    Assert.NotNull(provider.GetService<IReferenceRepo>());
                    Assert.NotNull(provider.GetService<ISyncLogRepo>());

                    // Assert helper registrations
                    Assert.NotNull(provider.GetService<IAudioHelper>());
                    Assert.NotNull(provider.GetService<IRegistryHelper>());

                    // Assert hardware listener
                    Assert.NotNull(provider.GetService<ScannerWedgeListener>());

                    // Assert services
                    Assert.NotNull(provider.GetService<IBtradeApiClient>());
                    Assert.NotNull(provider.GetService<ISyncService>());
                    Assert.NotNull(provider.GetService<IReturnOrderService>());
                    Assert.NotNull(provider.GetService<IBarcodeService>());

                    // Assert forms and views
                    Assert.NotNull(provider.GetService<ReturnOrderEntryView>());
                    Assert.NotNull(provider.GetService<ReturnOrderListView>());
                    Assert.NotNull(provider.GetService<BarcodeRegistryView>());
                    Assert.NotNull(provider.GetService<SettingsView>());
                    Assert.NotNull(provider.GetService<MainForm>());
                }
            });
        }

        [Fact]
        public void MainForm_InitializesWithDefaultIntakeView()
        {
            ExecuteInStaThread(() =>
            {
                using (var provider = CreateTestServiceProvider())
                {
                    using (var mainForm = provider.GetRequiredService<MainForm>())
                    {
                        Assert.NotNull(mainForm);
                        Assert.Equal("BTR Retur Gudang - Desktop Application", mainForm.Text);
                    }
                }
            });
        }

        [Fact]
        public void MainForm_NavigationSwitchesViewsWithoutCrash()
        {
            ExecuteInStaThread(() =>
            {
                using (var provider = CreateTestServiceProvider())
                {
                    using (var mainForm = provider.GetRequiredService<MainForm>())
                    {
                        mainForm.NavigateTo(NavigationModule.DaftarRetur);
                        mainForm.NavigateTo(NavigationModule.BarcodeRegistry);
                        mainForm.NavigateTo(NavigationModule.Pengaturan);
                        mainForm.NavigateTo(NavigationModule.Intake);
                    }
                }
            });
        }

        [Fact]
        public void MainForm_OnlineIndicatorTogglesCorrectly()
        {
            ExecuteInStaThread(() =>
            {
                using (var provider = CreateTestServiceProvider())
                {
                    using (var mainForm = provider.GetRequiredService<MainForm>())
                    {
                        mainForm.SetOnlineIndicator(true);
                        mainForm.SetOnlineIndicator(false);
                    }
                }
            });
        }

        [Fact]
        public void ReturnOrderEntryView_ImplementsIScannerConsumer_ReceivesScan()
        {
            ExecuteInStaThread(() =>
            {
                var view = new ReturnOrderEntryView();
                Assert.IsAssignableFrom<IScannerConsumer>(view);

                // Simulate receiving barcode
                view.OnBarcodeScanned("8991234567890");
            });
        }

        private static void ExecuteInStaThread(Action action)
        {
            Exception exception = null;
            var thread = new Thread(() =>
            {
                try
                {
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
    }
}
