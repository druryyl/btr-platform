using System;
using System.Windows;
using BtrReturGudang.AppTier.Contracts;
using BtrReturGudang.AppTier.Repositories;
using BtrReturGudang.AppTier.Services;
using BtrReturGudang.Infrastructure.Helpers;
using BtrReturGudang.Infrastructure.Http;
using BtrReturGudang.Infrastructure.Persistence;
using BtrReturGudang.Infrastructure.Services;
using BtrReturGudang.Wpf.Services;
using BtrReturGudang.Wpf.ViewModels;
using BtrReturGudang.Wpf.Views;
using Microsoft.Extensions.DependencyInjection;

namespace BtrReturGudang.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml.
    /// Configures Dependency Injection container and launches MainWindow.
    /// Traceable to: ARCHITECTURE §5.1, TD-002, P1-S01.
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();

            var mainWindow = ServiceProvider.GetService<MainWindow>();
            mainWindow?.Show();
        }

        public static void ConfigureServices(IServiceCollection services)
        {
            // 1. Data Access Repositories (Dapper DALs)
            services.AddSingleton<IReturnOrderRepo, ReturnOrderDal>();
            services.AddSingleton<IBarcodeRepo, BarcodeDal>();
            services.AddSingleton<IReferenceRepo, MasterDataDal>();
            services.AddSingleton<ISyncLogRepo, SyncLogDal>();

            // 2. Hardware and OS Helpers
            services.AddSingleton<IAudioHelper, AudioHelper>();
            services.AddSingleton<IRegistryHelper, RegistryHelper>();
            services.AddSingleton<WpfScannerWedgeListener>();

            // 3. Cloud API Client & Sync Service
            services.AddSingleton<IBtradeApiClient>(sp =>
                new BtradeApiClient(sp.GetRequiredService<IRegistryHelper>()));
            services.AddSingleton<ISyncService, SyncService>();

            // 4. Operator Session Validator Gate
            services.AddSingleton<OperatorSessionValidator>();

            // 5. Application Tier Orchestration Services
            services.AddSingleton<IReturnOrderService, ReturnOrderService>();
            services.AddSingleton<IBarcodeService, BarcodeService>();

            // 6. Presentation ViewModels
            services.AddTransient<MainViewModel>();
            services.AddTransient<ReturnOrderEntryViewModel>();
            services.AddTransient<ReturnOrderListViewModel>();
            services.AddTransient<BarcodeRegistryViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<ManualItemSearchViewModel>();
            services.AddTransient<QuickBarcodeRegistrationViewModel>();
            services.AddTransient<ManualCustomerSearchViewModel>();

            // 7. Presentation Views
            services.AddTransient<MainWindow>();
            services.AddTransient<BarcodeRegistryView>();
            services.AddTransient<SettingsView>();
            services.AddTransient<BarcodeRegistrationDialog>();
            services.AddTransient<OperatorSwitcherDialog>();
            services.AddTransient<ManualCustomerSearchDialog>();
        }
    }
}
