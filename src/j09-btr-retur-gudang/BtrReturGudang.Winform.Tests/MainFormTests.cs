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
        public void MainForm_WindowBounds_ConformTo720pSpecifications()
        {
            ExecuteInStaThread(() =>
            {
                using (var provider = CreateTestServiceProvider())
                {
                    using (var mainForm = provider.GetRequiredService<MainForm>())
                    {
                        Assert.Equal(new System.Drawing.Size(1280, 720), mainForm.Size);
                        Assert.Equal(new System.Drawing.Size(1024, 600), mainForm.MinimumSize);
                    }
                }
            });
        }

        [Fact]
        public void MainForm_LayoutBounds_AreCorrectWithoutOverlap()
        {
            ExecuteInStaThread(() =>
            {
                using (var provider = CreateTestServiceProvider())
                {
                    using (var mainForm = provider.GetRequiredService<MainForm>())
                    {
                        mainForm.ClientSize = new System.Drawing.Size(1280, 720);
                        mainForm.PerformLayout();

                        Control header = null, status = null, sidebar = null, contentHost = null;
                        foreach (Control c in mainForm.Controls)
                        {
                            if (c is StatusStrip) status = c;
                            else if (c.Dock == DockStyle.Top) header = c;
                            else if (c.Dock == DockStyle.Left) sidebar = c;
                            else if (c.Dock == DockStyle.Fill) contentHost = c;
                        }

                        Assert.NotNull(header);
                        Assert.NotNull(status);
                        Assert.NotNull(sidebar);
                        Assert.NotNull(contentHost);

                        // 1. Header spans full width across top (compact 44px height)
                        Assert.Equal(0, header.Left);
                        Assert.Equal(0, header.Top);
                        Assert.Equal(1280, header.Width);
                        Assert.Equal(44, header.Height);

                        // 2. StatusStrip spans full width across bottom
                        Assert.Equal(0, status.Left);
                        Assert.Equal(720 - status.Height, status.Top);
                        Assert.Equal(1280, status.Width);

                        // 3. Sidebar sits strictly between Header and StatusStrip on the left (compact 200px width)
                        Assert.Equal(0, sidebar.Left);
                        Assert.Equal(44, sidebar.Top);
                        Assert.Equal(200, sidebar.Width);
                        Assert.Equal(720 - 44 - status.Height, sidebar.Height);

                        // 4. ContentHost fills the remaining area to the right of the sidebar (>= 1064px net width, ~650px height)
                        Assert.Equal(200, contentHost.Left);
                        Assert.Equal(44, contentHost.Top);
                        Assert.Equal(1280 - 200, contentHost.Width);
                        Assert.Equal(720 - 44 - status.Height, contentHost.Height);
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
