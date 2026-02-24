using BtrGudang.AppTier.PackingOrderFeature;
using BtrGudang.Infrastructure.Helpers;
using BtrGudang.Infrastructure.PackingOrderFeature;
using BtrGudang.Winform.BtrGudang.Winform.Services;
using BtrGudang.Winform.Forms;
using BtrGudang.Winform.Infrastructure;
using BtrGudang.Winform.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PackingOrderDownloader;
using System;
using System.IO;
using System.Windows.Forms;

namespace BtrGudang.Winform
{
    internal static class Program
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Setup DI
            var services = new ServiceCollection();
            ConfigureServices(services, configuration);
            ServiceProvider = services.BuildServiceProvider();

            // Run your main form
            var mainForm = ServiceProvider.GetRequiredService<MainForm>();
            Application.Run(mainForm);
        }
        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // Register configuration options
            services.Configure<DatabaseOptions>(configuration.GetSection("Database"));
            services.Configure<BtradeCloudOptions>(configuration.GetSection("BtradeCloud"));
            
            //  Register services
            services.AddTransient<PackingOrderDownloaderSvc, PackingOrderDownloaderSvc>();
            services.AddScoped<IPackingOrderRepo, PackingOrderRepo>();
            services.AddScoped<IPackingOrderDal, PackingOrderDal>();
            services.AddScoped<IPackingOrderItemDal, PackingOrderItemDal>();
            services.AddScoped<IPrintLogRepo, PrintLogRepo>();
            services.AddScoped<IPrintLogDal, PrintLogDal>();
            services.AddScoped<IPrintLogPackingOrderDal, PrintLogPackingOrderDal>();




            // Register forms
            services.AddSingleton<IFormFactory, FormFactory>();
            services.AddTransient<DL1DownloaderForm>();
            services.AddTransient<DL2DownloadPackingOrderInfoForm>();
            services.AddTransient<PK1PrintPackingOrderForm>();
            services.AddTransient<MainForm>();

        }
    }
}
