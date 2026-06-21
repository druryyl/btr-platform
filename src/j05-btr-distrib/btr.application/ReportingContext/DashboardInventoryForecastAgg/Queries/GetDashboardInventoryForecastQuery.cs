using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using btr.application.ReportingContext.DashboardInventoryForecastAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using MediatR;

namespace btr.application.ReportingContext.DashboardInventoryForecastAgg.Queries
{
    public class GetDashboardInventoryForecastQuery : IRequest<DashboardInventoryForecastResponse>
    {
    }

    public class DashboardInventoryForecastResponse
    {
        public bool IsAvailable { get; set; }

        public DateTime GeneratedAt { get; set; }

        public DateTime BusinessDate { get; set; }

        public int PlanningHorizonDays { get; set; }

        public decimal CurrentInventoryValue { get; set; }

        public decimal ProjectedInventoryValue { get; set; }

        public decimal BestCaseProjectedValue { get; set; }

        public decimal WorstCaseProjectedValue { get; set; }

        public decimal AverageDailyConsumptionUnits { get; set; }

        public decimal? WeightedAverageDaysOfSupply { get; set; }

        public decimal UnderstockValue { get; set; }

        public decimal OverstockValue { get; set; }

        public int StockOutRiskItemCount { get; set; }

        public decimal? InventoryCoveragePercent { get; set; }

        public decimal? InventoryTurnoverForecast { get; set; }

        public int InventoryHealthScore { get; set; }

        public string ForecastConfidence { get; set; }

        public decimal? AtRiskInventoryPercent { get; set; }

        public decimal ForecastConsumptionUnits { get; set; }

        public string ExecutiveSummary { get; set; }

        public DashboardInventoryForecastTraceability Traceability { get; set; }
            = new DashboardInventoryForecastTraceability();

        public List<DashboardInventoryForecastDailyConsumptionItem> DailyConsumption { get; set; }
            = new List<DashboardInventoryForecastDailyConsumptionItem>();

        public List<DashboardInventoryForecastLevelItem> ProjectedLevel { get; set; }
            = new List<DashboardInventoryForecastLevelItem>();

        public List<DashboardInventoryForecastHeatCellItem> HeatSummary { get; set; }
            = new List<DashboardInventoryForecastHeatCellItem>();

        public List<DashboardInventoryForecastRiskItem> TopRisks { get; set; }
            = new List<DashboardInventoryForecastRiskItem>();

        public List<DashboardInventoryForecastRecommendationItem> PurchaseRecommendations { get; set; }
            = new List<DashboardInventoryForecastRecommendationItem>();
    }

    public class DashboardInventoryForecastTraceability
    {
        public string InventoryDashboardRoute { get; set; } = "/dashboard/inventory";

        public string InventoryRiskDashboardRoute { get; set; } = "/dashboard/inventory-risk";

        public string InventoryReportRoute { get; set; } = "/reports/inventory";

        public string PurchasingManagementRoute { get; set; } = "/dashboard/purchasing";

        public string Disclaimer { get; set; } =
            "Recommended quantities are indicative. Confirm with supplier, pending postings, and in-transit stock in BTR Desktop before purchasing.";
    }

    public class DashboardInventoryForecastDailyConsumptionItem
    {
        public DateTime ConsumptionDate { get; set; }

        public int DayIndex { get; set; }

        public decimal UnitsSold { get; set; }

        public decimal AdcReference { get; set; }
    }

    public class DashboardInventoryForecastLevelItem
    {
        public int HorizonDay { get; set; }

        public decimal ProjectedInventoryValue { get; set; }
    }

    public class DashboardInventoryForecastHeatCellItem
    {
        public string DosBand { get; set; }

        public string ValueBand { get; set; }

        public int ItemCount { get; set; }
    }

    public class DashboardInventoryForecastRiskItem
    {
        public int SortOrder { get; set; }

        public string SignalKey { get; set; }

        public string SignalLabel { get; set; }

        public string BrgId { get; set; }

        public string BrgCode { get; set; }

        public string BrgName { get; set; }

        public string SupplierName { get; set; }

        public decimal? DaysOfSupply { get; set; }

        public DateTime? StockOutDate { get; set; }

        public decimal ValueAmount { get; set; }

        public string Urgency { get; set; }

        public string DosSeverity { get; set; }

        public string RuleExplanation { get; set; }

        public string ReportRoute { get; set; }

        public string EntityCode { get; set; }
    }

    public class DashboardInventoryForecastRecommendationItem
    {
        public int SortOrder { get; set; }

        public string BrgId { get; set; }

        public string BrgCode { get; set; }

        public string BrgName { get; set; }

        public string SupplierName { get; set; }

        public DateTime? ReorderDate { get; set; }

        public decimal RecommendedPurchaseQty { get; set; }

        public decimal AverageDailyConsumption { get; set; }

        public decimal CurrentQty { get; set; }

        public decimal? DaysOfSupply { get; set; }

        public string Urgency { get; set; }

        public string ReportRoute { get; set; }

        public string EntityCode { get; set; }
    }

    public class GetDashboardInventoryForecastHandler
        : IRequestHandler<GetDashboardInventoryForecastQuery, DashboardInventoryForecastResponse>
    {
        private readonly IDashboardInventoryForecastDal _dal;

        public GetDashboardInventoryForecastHandler(IDashboardInventoryForecastDal dal)
        {
            _dal = dal;
        }

        public Task<DashboardInventoryForecastResponse> Handle(
            GetDashboardInventoryForecastQuery request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_dal.GetSummary());
        }
    }

    public static class InventoryForecastExecutiveSummaryBuilder
    {
        public static string Build(DashboardInventoryForecastResponse response)
        {
            if (response is null)
                return string.Empty;

            if (!response.IsAvailable)
                return "Inventory forecast data not yet available.";

            var confidencePrefix = response.ForecastConfidence == InventoryForecastPolicy.ConfidenceLow
                ? "Limited consumption history — forecast may change significantly. "
                : string.Empty;

            var currentText = FormatCurrency(response.CurrentInventoryValue);
            var projectedText = FormatCurrency(response.ProjectedInventoryValue);
            var understockText = FormatCurrency(response.UnderstockValue);
            var topRiskNames = response.TopRisks.Count > 0
                ? string.Join(", ", response.TopRisks.Take(3).Select(r => r.BrgName))
                : "none identified";

            return confidencePrefix +
                   $"At current 30-day sales pace, {response.StockOutRiskItemCount} active items may stock out within {response.PlanningHorizonDays} days, representing {understockText} in inventory value. " +
                   $"Projected inventory value moves from {currentText} to {projectedText}. Immediate attention: {topRiskNames}.";
        }

        private static string FormatCurrency(decimal amount) =>
            amount.ToString("C0", CultureInfo.GetCultureInfo("id-ID"));
    }
}
