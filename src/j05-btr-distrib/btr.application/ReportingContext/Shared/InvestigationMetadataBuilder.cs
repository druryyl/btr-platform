using System;
using System.Collections.Generic;

namespace btr.application.ReportingContext.Shared
{
    public static class InvestigationMetadataBuilder
    {
        public const string EntityTypeCustomer = "Customer";
        public const string EntityTypeSalesman = "Salesman";
        public const string EntityTypeItem = "Item";
        public const string EntityTypePrincipal = "Principal";
        public const string EntityTypeSupplier = "Supplier";
        public const string EntityTypeWarehouse = "Warehouse";
        public const string EntityTypeCategory = "Category";
        public const string EntityTypeWilayah = "Wilayah";
        public const string EntityTypeCompany = "Company";

        public static InvestigationMetadata Build(
            string signalKey,
            string entityType,
            string entityId,
            string entityName,
            Action<InvestigationSuggestedQuery> configureQuery = null,
            string signalLabelOverride = null,
            string reportRouteOverride = null)
        {
            InvestigationRegistry.TryGet(signalKey, out var entry);

            var suggestedQuery = new InvestigationSuggestedQuery
            {
                FreeText = entityName,
                PeriodMode = entry?.DefaultPeriodMode,
                PostingFilter = entry?.DefaultPostingFilter
            };

            ApplyEntityId(suggestedQuery, entityType, entityId);
            configureQuery?.Invoke(suggestedQuery);

            return new InvestigationMetadata
            {
                SignalKey = signalKey,
                SignalLabel = signalLabelOverride ?? entry?.DefaultSignalLabel ?? signalKey,
                EntityType = entityType,
                EntityId = entityId ?? string.Empty,
                EntityName = entityName ?? string.Empty,
                DashboardRoute = entry?.DashboardRoute,
                ReportRoute = reportRouteOverride ?? entry?.ReportRoute,
                SuggestedQuery = suggestedQuery,
                InvestigationSteps = entry?.Steps != null
                    ? new List<InvestigationStep>(entry.Steps)
                    : null,
                DesktopNextStep = entry?.DesktopNextStep
            };
        }

        public static InvestigationMetadata BuildFromReportRoute(
            string signalKey,
            string entityType,
            string entityId,
            string entityName,
            string reportRoute,
            Action<InvestigationSuggestedQuery> configureQuery = null)
        {
            return Build(
                signalKey,
                entityType,
                entityId,
                entityName,
                configureQuery,
                reportRouteOverride: reportRoute);
        }

        private static void ApplyEntityId(
            InvestigationSuggestedQuery query,
            string entityType,
            string entityId)
        {
            if (string.IsNullOrWhiteSpace(entityId))
                return;

            var id = entityId.Trim();

            if (string.Equals(entityType, EntityTypeCustomer, StringComparison.OrdinalIgnoreCase))
                query.CustomerId = id;
            else if (string.Equals(entityType, EntityTypeSalesman, StringComparison.OrdinalIgnoreCase))
                query.SalesmanId = id;
            else if (string.Equals(entityType, EntityTypeItem, StringComparison.OrdinalIgnoreCase))
                query.BrgId = id;
            else if (string.Equals(entityType, EntityTypeWarehouse, StringComparison.OrdinalIgnoreCase))
                query.WarehouseId = id;
            else if (string.Equals(entityType, EntityTypePrincipal, StringComparison.OrdinalIgnoreCase)
                || string.Equals(entityType, EntityTypeSupplier, StringComparison.OrdinalIgnoreCase))
                query.SupplierId = id;
        }
    }
}
