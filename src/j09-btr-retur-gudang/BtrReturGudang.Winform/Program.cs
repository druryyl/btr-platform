using System;
using System.Windows.Forms;
using BtrReturGudang.AppTier.Contracts;
using BtrReturGudang.AppTier.Repositories;
using BtrReturGudang.AppTier.Services;
using BtrReturGudang.Infrastructure.Helpers;
using BtrReturGudang.Infrastructure.Http;
using BtrReturGudang.Infrastructure.Persistence;
using BtrReturGudang.Infrastructure.Services;
using BtrReturGudang.Winform.Hardware;
using BtrReturGudang.Winform.Views;
using Microsoft.Extensions.DependencyInjection;

namespace BtrReturGudang.Winform
{
    public static class Program
    {
        /// <summary>
        /// The main entry point for the desktop warehouse return application.
        /// Configures Dependency Injection container and launches MainForm (SCR-01).
        /// Traceable to: ARCHITECTURE §5.1, TD-011, P3-S07.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var services = new ServiceCollection();
            ConfigureServices(services);

            using (var serviceProvider = services.BuildServiceProvider())
            {
                var mainForm = serviceProvider.GetRequiredService<MainForm>();
                Application.Run(mainForm);
            }
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

            // 3. Hardware Wedge Listener
            services.AddSingleton<ScannerWedgeListener>();

            // 4. Cloud API Client & Sync Service
            services.AddSingleton<IBtradeApiClient>(sp => 
                new BtradeApiClient(sp.GetRequiredService<IRegistryHelper>()));
            services.AddSingleton<ISyncService, SyncService>();

            // 5. Application Tier Orchestration Services
            services.AddSingleton<IReturnOrderService, ReturnOrderService>();
            services.AddSingleton<IBarcodeService, BarcodeService>();

            // 6. Presentation Views & Shell Forms
            services.AddTransient<MainForm>();
            services.AddTransient<ReturnOrderEntryView>();
            services.AddTransient<ReturnOrderListView>();
            services.AddTransient<BarcodeRegistryView>();
            services.AddTransient<SettingsView>();
            services.AddTransient<OperatorSwitcherDialog>();
            services.AddTransient<ManualItemSearchDialog>();
            services.AddTransient<QuickBarcodeRegistrationDialog>();
        }
    }
}
