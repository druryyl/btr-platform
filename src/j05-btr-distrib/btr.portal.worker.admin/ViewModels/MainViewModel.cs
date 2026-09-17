using System;
using btr.portal.worker.admin.Services;

namespace btr.portal.worker.admin.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly PortalApiService _apiService;
        private readonly WorkerProcessService _workerProcessService;

        public MainViewModel()
        {
            _apiService = new PortalApiService(
                System.Configuration.ConfigurationManager.AppSettings["ApiBaseUrl"] ?? "http://localhost:8080",
                System.Configuration.ConfigurationManager.AppSettings["RefreshToken"],
                int.TryParse(System.Configuration.ConfigurationManager.AppSettings["ApiTimeoutSeconds"], out var t) ? t : 30);

            _workerProcessService = new WorkerProcessService();

            HealthViewModel = new HealthViewModel(_apiService);
            RefreshViewModel = new RefreshViewModel(_apiService, _workerProcessService);
            HistoryViewModel = new HistoryViewModel(_apiService);
            ConfigurationViewModel = new ConfigurationViewModel(_apiService);
        }

        public HealthViewModel HealthViewModel { get; }
        public RefreshViewModel RefreshViewModel { get; }
        public HistoryViewModel HistoryViewModel { get; }
        public ConfigurationViewModel ConfigurationViewModel { get; }

        public string Title => "BTR Portal Worker Admin";
        public string Version => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";

        public void Dispose()
        {
            HealthViewModel.Dispose();
            RefreshViewModel.Dispose();
            _apiService.Dispose();
            _workerProcessService.Dispose();
        }
    }
}
