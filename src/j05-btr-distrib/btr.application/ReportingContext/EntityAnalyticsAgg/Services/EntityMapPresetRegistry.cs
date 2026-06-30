using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Services
{
    public static class EntityMapPresetRegistry
    {
        private static readonly IReadOnlyList<EntityMapPresetDefinition> Presets = new[]
        {
            new EntityMapPresetDefinition
            {
                PresetId = "customer-risk-map",
                EntityType = EntityTypeCode.Customer,
                DisplayName = "Customer Risk Map",
                Description = "Which customers combine sales value and receivable risk?",
                AxisXKpiId = "CU-KPI-009",
                AxisYKpiId = "CU-KPI-010",
                IsDefault = true,
                FilterDimensionKpiId = EntityAnalyticsMetaKpiIds.Wilayah
            },
            new EntityMapPresetDefinition
            {
                PresetId = "customer-growth-risk-map",
                EntityType = EntityTypeCode.Customer,
                DisplayName = "Customer Growth Risk Map",
                Description = "Which growing customers are becoming risky?",
                AxisXKpiId = "CU-KPI-009",
                AxisYKpiId = "FI-KPI-013",
                FilterDimensionKpiId = EntityAnalyticsMetaKpiIds.Wilayah
            },
            new EntityMapPresetDefinition
            {
                PresetId = "inventory-health-map",
                EntityType = EntityTypeCode.Item,
                DisplayName = "Inventory Health Map",
                Description = "Which items tie up capital without enough movement?",
                AxisXKpiId = "IN-KPI-001",
                AxisYKpiId = "IN-KPI-020",
                IsDefault = true,
                FilterDimensionKpiId = EntityAnalyticsMetaKpiIds.Category
            },
            new EntityMapPresetDefinition
            {
                PresetId = "replenishment-risk-map",
                EntityType = EntityTypeCode.Item,
                DisplayName = "Replenishment Risk Map",
                Description = "Which items need purchase attention?",
                AxisXKpiId = "IN-KPI-021",
                AxisYKpiId = "IN-KPI-020",
                FilterDimensionKpiId = EntityAnalyticsMetaKpiIds.Category
            },
            new EntityMapPresetDefinition
            {
                PresetId = "sales-performance-map",
                EntityType = EntityTypeCode.Salesman,
                DisplayName = "Sales Performance Map",
                Description = "Which reps sell well but carry receivable risk?",
                AxisXKpiId = "SF-KPI-009",
                AxisYKpiId = "SF-KPI-010",
                IsDefault = true,
                FilterDimensionKpiId = EntityAnalyticsMetaKpiIds.Wilayah
            },
            new EntityMapPresetDefinition
            {
                PresetId = "field-effectiveness-map",
                EntityType = EntityTypeCode.Salesman,
                DisplayName = "Field Effectiveness Map",
                Description = "Which reps need field execution coaching?",
                AxisXKpiId = "SF-KPI-008",
                AxisYKpiId = EntityAnalyticsMetaKpiIds.CustomerEngagement,
                FilterDimensionKpiId = EntityAnalyticsMetaKpiIds.Wilayah
            },
            new EntityMapPresetDefinition
            {
                PresetId = "purchase-exposure-map",
                EntityType = EntityTypeCode.Supplier,
                DisplayName = "Purchase Exposure Map",
                Description = "Which principals create purchase and inventory exposure?",
                AxisXKpiId = "PU-KPI-001",
                AxisYKpiId = EntityAnalyticsMetaKpiIds.InventoryValue,
                IsDefault = true,
                FilterDimensionKpiId = null
            },
            new EntityMapPresetDefinition
            {
                PresetId = "purchasing-discipline-map",
                EntityType = EntityTypeCode.Supplier,
                DisplayName = "Purchasing Discipline Map",
                Description = "Which principals have process risk?",
                AxisXKpiId = "PU-KPI-003",
                AxisYKpiId = EntityAnalyticsMetaKpiIds.AtRiskValue,
                FilterDimensionKpiId = null
            }
        };

        public static IReadOnlyList<EntityMapPresetDefinition> GetPresetsForEntityType(string entityType)
        {
            if (string.IsNullOrWhiteSpace(entityType))
                return Array.Empty<EntityMapPresetDefinition>();

            return Presets
                .Where(p => string.Equals(p.EntityType, entityType, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public static EntityMapPresetDefinition TryGetPreset(string entityType, string presetId)
        {
            if (string.IsNullOrWhiteSpace(entityType) || string.IsNullOrWhiteSpace(presetId))
                return null;

            return Presets.FirstOrDefault(p =>
                string.Equals(p.EntityType, entityType, StringComparison.OrdinalIgnoreCase)
                && string.Equals(p.PresetId, presetId, StringComparison.OrdinalIgnoreCase));
        }

        public static EntityMapPresetDefinition ResolveDefaultPreset(string entityType)
        {
            var presets = GetPresetsForEntityType(entityType);
            return presets.FirstOrDefault(p => p.IsDefault) ?? presets.FirstOrDefault();
        }
    }
}
