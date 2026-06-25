using System;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Producers;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Services
{
    public static class EntityAnalyticsReplayEntityCountHelper
    {
        public static int CountEntities(string entityType, object produceInput)
        {
            if (produceInput is null)
                return 0;

            switch (entityType)
            {
                case EntityTypeCode.Customer:
                    return Count(((CustomerEntityAnalyticsProduceInput)produceInput)?.PortfolioAggregate?.Customers);

                case EntityTypeCode.Salesman:
                    return Count(((SalesmanEntityAnalyticsProduceInput)produceInput)?.SalesmanAggregate?.Portfolio);

                case EntityTypeCode.Supplier:
                    return Count(((SupplierEntityAnalyticsProduceInput)produceInput)?.ManagementAggregate?.Portfolio);

                case EntityTypeCode.Item:
                    return Count(((ItemEntityAnalyticsProduceInput)produceInput)?.Portfolio);

                default:
                    return 0;
            }
        }

        public static EntityAnalyticsReplayRowCounts BuildRowCounts(string entityType, object bundle)
        {
            var counts = new EntityAnalyticsReplayRowCounts();

            switch (entityType)
            {
                case EntityTypeCode.Customer when bundle is CustomerReplayDataBundle customer:
                    counts.TransactionRowCount = customer.FakturRows?.Count ?? 0;
                    counts.MasterRowCount = customer.Customers?.Count ?? 0;
                    counts.SupportingRowCount = customer.PiutangRows?.Count ?? 0;
                    break;

                case EntityTypeCode.Salesman when bundle is SalesmanReplayDataBundle salesman:
                    counts.TransactionRowCount = salesman.FakturRows?.Count ?? 0;
                    counts.MasterRowCount = salesman.Salespeople?.Count ?? 0;
                    counts.SupportingRowCount = salesman.PiutangRows?.Count ?? 0;
                    break;

                case EntityTypeCode.Supplier when bundle is SupplierReplayDataBundle supplier:
                    counts.TransactionRowCount = supplier.InvoiceRows?.Count ?? 0;
                    counts.MasterRowCount = supplier.Suppliers?.Count ?? 0;
                    counts.SupportingRowCount = supplier.ItemRollupRows?.Count ?? 0;
                    break;

                case EntityTypeCode.Item when bundle is ItemReplayDataBundle item:
                    counts.TransactionRowCount = item.StokBalanceRows?.Count ?? 0;
                    counts.MasterRowCount = item.LastFakturRows?.Count ?? 0;
                    counts.SupportingRowCount = item.ConsumptionRows?.Count ?? 0;
                    break;
            }

            return counts;
        }

        private static int Count<T>(System.Collections.Generic.IList<T> rows) => rows?.Count ?? 0;
    }
}
