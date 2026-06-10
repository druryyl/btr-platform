-- Rename portal dashboard tables from BTR_PortalDashboard* to BTRPD_*
-- Run on existing databases before deploying updated application code.
SET NOCOUNT ON;
GO

-- Step 1: Rename tables
EXEC sp_rename 'BTR_PortalDashboardCollectionAging', 'BTRPD_CollectionAging';
GO
EXEC sp_rename 'BTR_PortalDashboardCollectionAttention', 'BTRPD_CollectionAttention';
GO
EXEC sp_rename 'BTR_PortalDashboardCollectionKpi', 'BTRPD_CollectionKpi';
GO
EXEC sp_rename 'BTR_PortalDashboardCollectionTopOverdueCustomer', 'BTRPD_CollectionTopOverdueCustomer';
GO
EXEC sp_rename 'BTR_PortalDashboardCollectionTopOverdueSalesman', 'BTRPD_CollectionTopOverdueSalesman';
GO
EXEC sp_rename 'BTR_PortalDashboardCollectionTopOverdueWilayah', 'BTRPD_CollectionTopOverdueWilayah';
GO
EXEC sp_rename 'BTR_PortalDashboardCustomerAttention', 'BTRPD_CustomerAttention';
GO
EXEC sp_rename 'BTR_PortalDashboardCustomerKpi', 'BTRPD_CustomerKpi';
GO
EXEC sp_rename 'BTR_PortalDashboardCustomerSegmentation', 'BTRPD_CustomerSegmentation';
GO
EXEC sp_rename 'BTR_PortalDashboardCustomerTopOmzet', 'BTRPD_CustomerTopOmzet';
GO
EXEC sp_rename 'BTR_PortalDashboardCustomerTopPiutang', 'BTRPD_CustomerTopPiutang';
GO
EXEC sp_rename 'BTR_PortalDashboardInventoryBreakdown', 'BTRPD_InventoryBreakdown';
GO
EXEC sp_rename 'BTR_PortalDashboardInventoryKpi', 'BTRPD_InventoryKpi';
GO
EXEC sp_rename 'BTR_PortalDashboardInventoryRiskAging', 'BTRPD_InventoryRiskAging';
GO
EXEC sp_rename 'BTR_PortalDashboardInventoryRiskAttention', 'BTRPD_InventoryRiskAttention';
GO
EXEC sp_rename 'BTR_PortalDashboardInventoryRiskBreakdown', 'BTRPD_InventoryRiskBreakdown';
GO
EXEC sp_rename 'BTR_PortalDashboardInventoryRiskKpi', 'BTRPD_InventoryRiskKpi';
GO
EXEC sp_rename 'BTR_PortalDashboardInventoryRiskTopDead', 'BTRPD_InventoryRiskTopDead';
GO
EXEC sp_rename 'BTR_PortalDashboardInventoryRiskTopSlow', 'BTRPD_InventoryRiskTopSlow';
GO
EXEC sp_rename 'BTR_PortalDashboardLocationAttention', 'BTRPD_LocationAttention';
GO
EXEC sp_rename 'BTR_PortalDashboardLocationKpi', 'BTRPD_LocationKpi';
GO
EXEC sp_rename 'BTR_PortalDashboardLocationTopWarehouseAtRisk', 'BTRPD_LocationTopWarehouseAtRisk';
GO
EXEC sp_rename 'BTR_PortalDashboardLocationTopWarehouseInventory', 'BTRPD_LocationTopWarehouseInventory';
GO
EXEC sp_rename 'BTR_PortalDashboardLocationTopWarehousePurchasing', 'BTRPD_LocationTopWarehousePurchasing';
GO
EXEC sp_rename 'BTR_PortalDashboardLocationTopWarehouseSales', 'BTRPD_LocationTopWarehouseSales';
GO
EXEC sp_rename 'BTR_PortalDashboardLocationTopWilayahSales', 'BTRPD_LocationTopWilayahSales';
GO
EXEC sp_rename 'BTR_PortalDashboardPiutangAging', 'BTRPD_PiutangAging';
GO
EXEC sp_rename 'BTR_PortalDashboardPiutangKpi', 'BTRPD_PiutangKpi';
GO
EXEC sp_rename 'BTR_PortalDashboardPiutangTopCustomer', 'BTRPD_PiutangTopCustomer';
GO
EXEC sp_rename 'BTR_PortalDashboardPurchasingKpi', 'BTRPD_PurchasingKpi';
GO
EXEC sp_rename 'BTR_PortalDashboardPurchasingManagementAttention', 'BTRPD_PurchasingManagementAttention';
GO
EXEC sp_rename 'BTR_PortalDashboardPurchasingManagementKpi', 'BTRPD_PurchasingManagementKpi';
GO
EXEC sp_rename 'BTR_PortalDashboardPurchasingManagementTopPrincipal', 'BTRPD_PurchasingManagementTopPrincipal';
GO
EXEC sp_rename 'BTR_PortalDashboardPurchasingPostingStatus', 'BTRPD_PurchasingPostingStatus';
GO
EXEC sp_rename 'BTR_PortalDashboardPurchasingTopPrincipal', 'BTRPD_PurchasingTopPrincipal';
GO
EXEC sp_rename 'BTR_PortalDashboardPurchasingWeekTrend', 'BTRPD_PurchasingWeekTrend';
GO
EXEC sp_rename 'BTR_PortalDashboardRefreshLog', 'BTRPD_RefreshLog';
GO
EXEC sp_rename 'BTR_PortalDashboardSalesKpi', 'BTRPD_SalesKpi';
GO
EXEC sp_rename 'BTR_PortalDashboardSalesmanAttention', 'BTRPD_SalesmanAttention';
GO
EXEC sp_rename 'BTR_PortalDashboardSalesmanKpi', 'BTRPD_SalesmanKpi';
GO
EXEC sp_rename 'BTR_PortalDashboardSalesmanSegmentation', 'BTRPD_SalesmanSegmentation';
GO
EXEC sp_rename 'BTR_PortalDashboardSalesmanTopAchievement', 'BTRPD_SalesmanTopAchievement';
GO
EXEC sp_rename 'BTR_PortalDashboardSalesmanTopOmzet', 'BTRPD_SalesmanTopOmzet';
GO
EXEC sp_rename 'BTR_PortalDashboardSalesmanTopPiutang', 'BTRPD_SalesmanTopPiutang';
GO
EXEC sp_rename 'BTR_PortalDashboardSalesTopSalesman', 'BTRPD_SalesTopSalesman';
GO
EXEC sp_rename 'BTR_PortalDashboardSalesWeekTrend', 'BTRPD_SalesWeekTrend';
GO

-- Step 2: Rename constraints (PK, DF, UX, IX)
EXEC sp_rename 'DF_BTR_PortalDashboardCollectionAging_CollectionAgingId', 'DF_BTRPD_CollectionAging_CollectionAgingId', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCollectionAging_SnapshotKey', 'DF_BTRPD_CollectionAging_SnapshotKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCollectionAging_BucketKey', 'DF_BTRPD_CollectionAging_BucketKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCollectionAging_BucketLabel', 'DF_BTRPD_CollectionAging_BucketLabel', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCollectionAging_Amount', 'DF_BTRPD_CollectionAging_Amount', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCollectionAging_SortOrder', 'DF_BTRPD_CollectionAging_SortOrder', 'OBJECT';
GO
EXEC sp_rename 'PK_BTR_PortalDashboardCollectionAging', 'PK_BTRPD_CollectionAging', 'OBJECT';
GO
EXEC sp_rename 'UX_BTR_PortalDashboardCollectionAging_SnapshotKey_BucketKey', 'UX_BTRPD_CollectionAging_SnapshotKey_BucketKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCollectionAttention_CollectionAttentionId', 'DF_BTRPD_CollectionAttention_CollectionAttentionId', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCollectionAttention_SnapshotKey', 'DF_BTRPD_CollectionAttention_SnapshotKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCollectionAttention_EntityType', 'DF_BTRPD_CollectionAttention_EntityType', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCollectionAttention_EntityId', 'DF_BTRPD_CollectionAttention_EntityId', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCollectionAttention_EntityCode', 'DF_BTRPD_CollectionAttention_EntityCode', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCollectionAttention_EntityName', 'DF_BTRPD_CollectionAttention_EntityName', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCollectionAttention_SignalKey', 'DF_BTRPD_CollectionAttention_SignalKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCollectionAttention_SignalLabel', 'DF_BTRPD_CollectionAttention_SignalLabel', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCollectionAttention_WilayahName', 'DF_BTRPD_CollectionAttention_WilayahName', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCollectionAttention_SortOrder', 'DF_BTRPD_CollectionAttention_SortOrder', 'OBJECT';
GO
EXEC sp_rename 'PK_BTR_PortalDashboardCollectionAttention', 'PK_BTRPD_CollectionAttention', 'OBJECT';
GO
EXEC sp_rename 'IX_BTR_PortalDashboardCollectionAttention_SnapshotKey_SortOrder', 'IX_BTRPD_CollectionAttention_SnapshotKey_SortOrder', 'INDEX';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCollectionKpi_SnapshotKey', 'DF_BTRPD_CollectionKpi_SnapshotKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCollectionKpi_GeneratedAt', 'DF_BTRPD_CollectionKpi_GeneratedAt', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCollectionKpi_PeriodYear', 'DF_BTRPD_CollectionKpi_PeriodYear', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCollectionKpi_PeriodMonth', 'DF_BTRPD_CollectionKpi_PeriodMonth', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCollectionKpi_OverdueExposure', 'DF_BTRPD_CollectionKpi_OverdueExposure', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCollectionKpi_AgingOver90Exposure', 'DF_BTRPD_CollectionKpi_AgingOver90Exposure', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCollectionKpi_CashCollectedMtd', 'DF_BTRPD_CollectionKpi_CashCollectedMtd', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCollectionKpi_MonthCollections', 'DF_BTRPD_CollectionKpi_MonthCollections', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCollectionKpi_MonthFakturOmzet', 'DF_BTRPD_CollectionKpi_MonthFakturOmzet', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCollectionKpi_PaymentMixCashAmount', 'DF_BTRPD_CollectionKpi_PaymentMixCashAmount', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCollectionKpi_PaymentMixGiroAmount', 'DF_BTRPD_CollectionKpi_PaymentMixGiroAmount', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCollectionKpi_PaymentMixAdjustmentAmount', 'DF_BTRPD_CollectionKpi_PaymentMixAdjustmentAmount', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCollectionKpi_LegacyDebtCount', 'DF_BTRPD_CollectionKpi_LegacyDebtCount', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCollectionKpi_ChronicOverdueCount', 'DF_BTRPD_CollectionKpi_ChronicOverdueCount', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCollectionKpi_WilayahHotspotCount', 'DF_BTRPD_CollectionKpi_WilayahHotspotCount', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCollectionKpi_LowRecoveryVsBillingCount', 'DF_BTRPD_CollectionKpi_LowRecoveryVsBillingCount', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCollectionKpi_LastRefreshLogId', 'DF_BTRPD_CollectionKpi_LastRefreshLogId', 'OBJECT';
GO
EXEC sp_rename 'PK_BTR_PortalDashboardCollectionKpi', 'PK_BTRPD_CollectionKpi', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCollectionTopOverdueCustomer_CollectionTopOverdueCustomerId', 'DF_BTRPD_CollectionTopOverdueCustomer_CollectionTopOverdueCustomerId', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCollectionTopOverdueCustomer_SnapshotKey', 'DF_BTRPD_CollectionTopOverdueCustomer_SnapshotKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCollectionTopOverdueCustomer_Rank', 'DF_BTRPD_CollectionTopOverdueCustomer_Rank', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCollectionTopOverdueCustomer_CustomerCode', 'DF_BTRPD_CollectionTopOverdueCustomer_CustomerCode', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCollectionTopOverdueCustomer_CustomerName', 'DF_BTRPD_CollectionTopOverdueCustomer_CustomerName', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCollectionTopOverdueCustomer_OverdueBalance', 'DF_BTRPD_CollectionTopOverdueCustomer_OverdueBalance', 'OBJECT';
GO
EXEC sp_rename 'PK_BTR_PortalDashboardCollectionTopOverdueCustomer', 'PK_BTRPD_CollectionTopOverdueCustomer', 'OBJECT';
GO
EXEC sp_rename 'UX_BTR_PortalDashboardCollectionTopOverdueCustomer_SnapshotKey_Rank', 'UX_BTRPD_CollectionTopOverdueCustomer_SnapshotKey_Rank', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCollectionTopOverdueSalesman_CollectionTopOverdueSalesmanId', 'DF_BTRPD_CollectionTopOverdueSalesman_CollectionTopOverdueSalesmanId', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCollectionTopOverdueSalesman_SnapshotKey', 'DF_BTRPD_CollectionTopOverdueSalesman_SnapshotKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCollectionTopOverdueSalesman_Rank', 'DF_BTRPD_CollectionTopOverdueSalesman_Rank', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCollectionTopOverdueSalesman_SalesPersonId', 'DF_BTRPD_CollectionTopOverdueSalesman_SalesPersonId', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCollectionTopOverdueSalesman_SalesPersonCode', 'DF_BTRPD_CollectionTopOverdueSalesman_SalesPersonCode', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCollectionTopOverdueSalesman_SalesPersonName', 'DF_BTRPD_CollectionTopOverdueSalesman_SalesPersonName', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCollectionTopOverdueSalesman_OverdueBalance', 'DF_BTRPD_CollectionTopOverdueSalesman_OverdueBalance', 'OBJECT';
GO
EXEC sp_rename 'PK_BTR_PortalDashboardCollectionTopOverdueSalesman', 'PK_BTRPD_CollectionTopOverdueSalesman', 'OBJECT';
GO
EXEC sp_rename 'UX_BTR_PortalDashboardCollectionTopOverdueSalesman_SnapshotKey_Rank', 'UX_BTRPD_CollectionTopOverdueSalesman_SnapshotKey_Rank', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCollectionTopOverdueWilayah_CollectionTopOverdueWilayahId', 'DF_BTRPD_CollectionTopOverdueWilayah_CollectionTopOverdueWilayahId', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCollectionTopOverdueWilayah_SnapshotKey', 'DF_BTRPD_CollectionTopOverdueWilayah_SnapshotKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCollectionTopOverdueWilayah_Rank', 'DF_BTRPD_CollectionTopOverdueWilayah_Rank', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCollectionTopOverdueWilayah_WilayahId', 'DF_BTRPD_CollectionTopOverdueWilayah_WilayahId', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCollectionTopOverdueWilayah_WilayahName', 'DF_BTRPD_CollectionTopOverdueWilayah_WilayahName', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCollectionTopOverdueWilayah_OverdueBalance', 'DF_BTRPD_CollectionTopOverdueWilayah_OverdueBalance', 'OBJECT';
GO
EXEC sp_rename 'PK_BTR_PortalDashboardCollectionTopOverdueWilayah', 'PK_BTRPD_CollectionTopOverdueWilayah', 'OBJECT';
GO
EXEC sp_rename 'UX_BTR_PortalDashboardCollectionTopOverdueWilayah_SnapshotKey_Rank', 'UX_BTRPD_CollectionTopOverdueWilayah_SnapshotKey_Rank', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCustomerAttention_CustomerAttentionId', 'DF_BTRPD_CustomerAttention_CustomerAttentionId', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCustomerAttention_SnapshotKey', 'DF_BTRPD_CustomerAttention_SnapshotKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCustomerAttention_CustomerCode', 'DF_BTRPD_CustomerAttention_CustomerCode', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCustomerAttention_CustomerName', 'DF_BTRPD_CustomerAttention_CustomerName', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCustomerAttention_SignalKey', 'DF_BTRPD_CustomerAttention_SignalKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCustomerAttention_SignalLabel', 'DF_BTRPD_CustomerAttention_SignalLabel', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCustomerAttention_WilayahName', 'DF_BTRPD_CustomerAttention_WilayahName', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCustomerAttention_SortOrder', 'DF_BTRPD_CustomerAttention_SortOrder', 'OBJECT';
GO
EXEC sp_rename 'PK_BTR_PortalDashboardCustomerAttention', 'PK_BTRPD_CustomerAttention', 'OBJECT';
GO
EXEC sp_rename 'IX_BTR_PortalDashboardCustomerAttention_SnapshotKey_SortOrder', 'IX_BTRPD_CustomerAttention_SnapshotKey_SortOrder', 'INDEX';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCustomerKpi_SnapshotKey', 'DF_BTRPD_CustomerKpi_SnapshotKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCustomerKpi_GeneratedAt', 'DF_BTRPD_CustomerKpi_GeneratedAt', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCustomerKpi_PeriodYear', 'DF_BTRPD_CustomerKpi_PeriodYear', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCustomerKpi_PeriodMonth', 'DF_BTRPD_CustomerKpi_PeriodMonth', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCustomerKpi_TotalOmzet', 'DF_BTRPD_CustomerKpi_TotalOmzet', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCustomerKpi_TotalPiutang', 'DF_BTRPD_CustomerKpi_TotalPiutang', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCustomerKpi_ActiveCustomerCount', 'DF_BTRPD_CustomerKpi_ActiveCustomerCount', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCustomerKpi_DormantCustomerCount', 'DF_BTRPD_CustomerKpi_DormantCustomerCount', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCustomerKpi_OverdueCustomerCount', 'DF_BTRPD_CustomerKpi_OverdueCustomerCount', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCustomerKpi_PlafondBreachCount', 'DF_BTRPD_CustomerKpi_PlafondBreachCount', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCustomerKpi_SuspendedWithSalesCount', 'DF_BTRPD_CustomerKpi_SuspendedWithSalesCount', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCustomerKpi_AgingOver90Amount', 'DF_BTRPD_CustomerKpi_AgingOver90Amount', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCustomerKpi_LastRefreshLogId', 'DF_BTRPD_CustomerKpi_LastRefreshLogId', 'OBJECT';
GO
EXEC sp_rename 'PK_BTR_PortalDashboardCustomerKpi', 'PK_BTRPD_CustomerKpi', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCustomerSegmentation_CustomerSegmentationId', 'DF_BTRPD_CustomerSegmentation_CustomerSegmentationId', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCustomerSegmentation_SnapshotKey', 'DF_BTRPD_CustomerSegmentation_SnapshotKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCustomerSegmentation_SegmentType', 'DF_BTRPD_CustomerSegmentation_SegmentType', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCustomerSegmentation_SegmentKey', 'DF_BTRPD_CustomerSegmentation_SegmentKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCustomerSegmentation_SegmentLabel', 'DF_BTRPD_CustomerSegmentation_SegmentLabel', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCustomerSegmentation_CustomerCount', 'DF_BTRPD_CustomerSegmentation_CustomerCount', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCustomerSegmentation_ActiveCount', 'DF_BTRPD_CustomerSegmentation_ActiveCount', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCustomerSegmentation_DormantCount', 'DF_BTRPD_CustomerSegmentation_DormantCount', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCustomerSegmentation_SortOrder', 'DF_BTRPD_CustomerSegmentation_SortOrder', 'OBJECT';
GO
EXEC sp_rename 'PK_BTR_PortalDashboardCustomerSegmentation', 'PK_BTRPD_CustomerSegmentation', 'OBJECT';
GO
EXEC sp_rename 'UX_BTR_PortalDashboardCustomerSegmentation_SnapshotKey_SegmentType_SegmentKey', 'UX_BTRPD_CustomerSegmentation_SnapshotKey_SegmentType_SegmentKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCustomerTopOmzet_CustomerTopOmzetId', 'DF_BTRPD_CustomerTopOmzet_CustomerTopOmzetId', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCustomerTopOmzet_SnapshotKey', 'DF_BTRPD_CustomerTopOmzet_SnapshotKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCustomerTopOmzet_Rank', 'DF_BTRPD_CustomerTopOmzet_Rank', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCustomerTopOmzet_CustomerCode', 'DF_BTRPD_CustomerTopOmzet_CustomerCode', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCustomerTopOmzet_CustomerName', 'DF_BTRPD_CustomerTopOmzet_CustomerName', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCustomerTopOmzet_OmzetAmount', 'DF_BTRPD_CustomerTopOmzet_OmzetAmount', 'OBJECT';
GO
EXEC sp_rename 'PK_BTR_PortalDashboardCustomerTopOmzet', 'PK_BTRPD_CustomerTopOmzet', 'OBJECT';
GO
EXEC sp_rename 'UX_BTR_PortalDashboardCustomerTopOmzet_SnapshotKey_Rank', 'UX_BTRPD_CustomerTopOmzet_SnapshotKey_Rank', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCustomerTopPiutang_CustomerTopPiutangId', 'DF_BTRPD_CustomerTopPiutang_CustomerTopPiutangId', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCustomerTopPiutang_SnapshotKey', 'DF_BTRPD_CustomerTopPiutang_SnapshotKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCustomerTopPiutang_Rank', 'DF_BTRPD_CustomerTopPiutang_Rank', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCustomerTopPiutang_CustomerCode', 'DF_BTRPD_CustomerTopPiutang_CustomerCode', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCustomerTopPiutang_CustomerName', 'DF_BTRPD_CustomerTopPiutang_CustomerName', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardCustomerTopPiutang_OutstandingBalance', 'DF_BTRPD_CustomerTopPiutang_OutstandingBalance', 'OBJECT';
GO
EXEC sp_rename 'PK_BTR_PortalDashboardCustomerTopPiutang', 'PK_BTRPD_CustomerTopPiutang', 'OBJECT';
GO
EXEC sp_rename 'UX_BTR_PortalDashboardCustomerTopPiutang_SnapshotKey_Rank', 'UX_BTRPD_CustomerTopPiutang_SnapshotKey_Rank', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryBreakdown_InventoryBreakdownId', 'DF_BTRPD_InventoryBreakdown_InventoryBreakdownId', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryBreakdown_SnapshotKey', 'DF_BTRPD_InventoryBreakdown_SnapshotKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryBreakdown_DimensionType', 'DF_BTRPD_InventoryBreakdown_DimensionType', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryBreakdown_Name', 'DF_BTRPD_InventoryBreakdown_Name', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryBreakdown_InventoryValue', 'DF_BTRPD_InventoryBreakdown_InventoryValue', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryBreakdown_IsTop10', 'DF_BTRPD_InventoryBreakdown_IsTop10', 'OBJECT';
GO
EXEC sp_rename 'PK_BTR_PortalDashboardInventoryBreakdown', 'PK_BTRPD_InventoryBreakdown', 'OBJECT';
GO
EXEC sp_rename 'IX_BTR_PortalDashboardInventoryBreakdown_SnapshotKey_DimensionType', 'IX_BTRPD_InventoryBreakdown_SnapshotKey_DimensionType', 'INDEX';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryKpi_SnapshotKey', 'DF_BTRPD_InventoryKpi_SnapshotKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryKpi_GeneratedAt', 'DF_BTRPD_InventoryKpi_GeneratedAt', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryKpi_TotalInventoryValue', 'DF_BTRPD_InventoryKpi_TotalInventoryValue', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryKpi_TotalItem', 'DF_BTRPD_InventoryKpi_TotalItem', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryKpi_LastRefreshLogId', 'DF_BTRPD_InventoryKpi_LastRefreshLogId', 'OBJECT';
GO
EXEC sp_rename 'PK_BTR_PortalDashboardInventoryKpi', 'PK_BTRPD_InventoryKpi', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskAging_InventoryRiskAgingId', 'DF_BTRPD_InventoryRiskAging_InventoryRiskAgingId', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskAging_SnapshotKey', 'DF_BTRPD_InventoryRiskAging_SnapshotKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskAging_BucketKey', 'DF_BTRPD_InventoryRiskAging_BucketKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskAging_BucketLabel', 'DF_BTRPD_InventoryRiskAging_BucketLabel', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskAging_InventoryValue', 'DF_BTRPD_InventoryRiskAging_InventoryValue', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskAging_ItemCount', 'DF_BTRPD_InventoryRiskAging_ItemCount', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskAging_SortOrder', 'DF_BTRPD_InventoryRiskAging_SortOrder', 'OBJECT';
GO
EXEC sp_rename 'PK_BTR_PortalDashboardInventoryRiskAging', 'PK_BTRPD_InventoryRiskAging', 'OBJECT';
GO
EXEC sp_rename 'UX_BTR_PortalDashboardInventoryRiskAging_SnapshotKey_BucketKey', 'UX_BTRPD_InventoryRiskAging_SnapshotKey_BucketKey', 'INDEX';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskAttention_InventoryRiskAttentionId', 'DF_BTRPD_InventoryRiskAttention_InventoryRiskAttentionId', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskAttention_SnapshotKey', 'DF_BTRPD_InventoryRiskAttention_SnapshotKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskAttention_BrgId', 'DF_BTRPD_InventoryRiskAttention_BrgId', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskAttention_BrgCode', 'DF_BTRPD_InventoryRiskAttention_BrgCode', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskAttention_BrgName', 'DF_BTRPD_InventoryRiskAttention_BrgName', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskAttention_KategoriName', 'DF_BTRPD_InventoryRiskAttention_KategoriName', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskAttention_SupplierName', 'DF_BTRPD_InventoryRiskAttention_SupplierName', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskAttention_Qty', 'DF_BTRPD_InventoryRiskAttention_Qty', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskAttention_InventoryValue', 'DF_BTRPD_InventoryRiskAttention_InventoryValue', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskAttention_SignalKey', 'DF_BTRPD_InventoryRiskAttention_SignalKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskAttention_SignalLabel', 'DF_BTRPD_InventoryRiskAttention_SignalLabel', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskAttention_SortOrder', 'DF_BTRPD_InventoryRiskAttention_SortOrder', 'OBJECT';
GO
EXEC sp_rename 'PK_BTR_PortalDashboardInventoryRiskAttention', 'PK_BTRPD_InventoryRiskAttention', 'OBJECT';
GO
EXEC sp_rename 'IX_BTR_PortalDashboardInventoryRiskAttention_SnapshotKey_SortOrder', 'IX_BTRPD_InventoryRiskAttention_SnapshotKey_SortOrder', 'INDEX';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskBreakdown_InventoryRiskBreakdownId', 'DF_BTRPD_InventoryRiskBreakdown_InventoryRiskBreakdownId', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskBreakdown_SnapshotKey', 'DF_BTRPD_InventoryRiskBreakdown_SnapshotKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskBreakdown_DimensionType', 'DF_BTRPD_InventoryRiskBreakdown_DimensionType', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskBreakdown_Name', 'DF_BTRPD_InventoryRiskBreakdown_Name', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskBreakdown_AtRiskValue', 'DF_BTRPD_InventoryRiskBreakdown_AtRiskValue', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskBreakdown_ItemCount', 'DF_BTRPD_InventoryRiskBreakdown_ItemCount', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskBreakdown_Rank', 'DF_BTRPD_InventoryRiskBreakdown_Rank', 'OBJECT';
GO
EXEC sp_rename 'PK_BTR_PortalDashboardInventoryRiskBreakdown', 'PK_BTRPD_InventoryRiskBreakdown', 'OBJECT';
GO
EXEC sp_rename 'UX_BTR_PortalDashboardInventoryRiskBreakdown_SnapshotKey_DimensionType_Rank', 'UX_BTRPD_InventoryRiskBreakdown_SnapshotKey_DimensionType_Rank', 'INDEX';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskKpi_SnapshotKey', 'DF_BTRPD_InventoryRiskKpi_SnapshotKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskKpi_GeneratedAt', 'DF_BTRPD_InventoryRiskKpi_GeneratedAt', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskKpi_TotalInventoryValue', 'DF_BTRPD_InventoryRiskKpi_TotalInventoryValue', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskKpi_TotalItem', 'DF_BTRPD_InventoryRiskKpi_TotalItem', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskKpi_DeadStockItemCount', 'DF_BTRPD_InventoryRiskKpi_DeadStockItemCount', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskKpi_DeadStockValue', 'DF_BTRPD_InventoryRiskKpi_DeadStockValue', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskKpi_SlowMovingItemCount', 'DF_BTRPD_InventoryRiskKpi_SlowMovingItemCount', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskKpi_SlowMovingValue', 'DF_BTRPD_InventoryRiskKpi_SlowMovingValue', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskKpi_NeverSoldItemCount', 'DF_BTRPD_InventoryRiskKpi_NeverSoldItemCount', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskKpi_NeverSoldValue', 'DF_BTRPD_InventoryRiskKpi_NeverSoldValue', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskKpi_AtRiskInventoryValue', 'DF_BTRPD_InventoryRiskKpi_AtRiskInventoryValue', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskKpi_RequiresAttention', 'DF_BTRPD_InventoryRiskKpi_RequiresAttention', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskKpi_LastRefreshLogId', 'DF_BTRPD_InventoryRiskKpi_LastRefreshLogId', 'OBJECT';
GO
EXEC sp_rename 'PK_BTR_PortalDashboardInventoryRiskKpi', 'PK_BTRPD_InventoryRiskKpi', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskTopDead_InventoryRiskTopDeadId', 'DF_BTRPD_InventoryRiskTopDead_InventoryRiskTopDeadId', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskTopDead_SnapshotKey', 'DF_BTRPD_InventoryRiskTopDead_SnapshotKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskTopDead_Rank', 'DF_BTRPD_InventoryRiskTopDead_Rank', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskTopDead_BrgId', 'DF_BTRPD_InventoryRiskTopDead_BrgId', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskTopDead_BrgCode', 'DF_BTRPD_InventoryRiskTopDead_BrgCode', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskTopDead_BrgName', 'DF_BTRPD_InventoryRiskTopDead_BrgName', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskTopDead_KategoriName', 'DF_BTRPD_InventoryRiskTopDead_KategoriName', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskTopDead_SupplierName', 'DF_BTRPD_InventoryRiskTopDead_SupplierName', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskTopDead_Qty', 'DF_BTRPD_InventoryRiskTopDead_Qty', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskTopDead_InventoryValue', 'DF_BTRPD_InventoryRiskTopDead_InventoryValue', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskTopDead_DaysSinceLastFaktur', 'DF_BTRPD_InventoryRiskTopDead_DaysSinceLastFaktur', 'OBJECT';
GO
EXEC sp_rename 'PK_BTR_PortalDashboardInventoryRiskTopDead', 'PK_BTRPD_InventoryRiskTopDead', 'OBJECT';
GO
EXEC sp_rename 'UX_BTR_PortalDashboardInventoryRiskTopDead_SnapshotKey_Rank', 'UX_BTRPD_InventoryRiskTopDead_SnapshotKey_Rank', 'INDEX';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskTopSlow_InventoryRiskTopSlowId', 'DF_BTRPD_InventoryRiskTopSlow_InventoryRiskTopSlowId', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskTopSlow_SnapshotKey', 'DF_BTRPD_InventoryRiskTopSlow_SnapshotKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskTopSlow_Rank', 'DF_BTRPD_InventoryRiskTopSlow_Rank', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskTopSlow_BrgId', 'DF_BTRPD_InventoryRiskTopSlow_BrgId', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskTopSlow_BrgCode', 'DF_BTRPD_InventoryRiskTopSlow_BrgCode', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskTopSlow_BrgName', 'DF_BTRPD_InventoryRiskTopSlow_BrgName', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskTopSlow_KategoriName', 'DF_BTRPD_InventoryRiskTopSlow_KategoriName', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskTopSlow_SupplierName', 'DF_BTRPD_InventoryRiskTopSlow_SupplierName', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskTopSlow_Qty', 'DF_BTRPD_InventoryRiskTopSlow_Qty', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskTopSlow_InventoryValue', 'DF_BTRPD_InventoryRiskTopSlow_InventoryValue', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardInventoryRiskTopSlow_DaysSinceLastFaktur', 'DF_BTRPD_InventoryRiskTopSlow_DaysSinceLastFaktur', 'OBJECT';
GO
EXEC sp_rename 'PK_BTR_PortalDashboardInventoryRiskTopSlow', 'PK_BTRPD_InventoryRiskTopSlow', 'OBJECT';
GO
EXEC sp_rename 'UX_BTR_PortalDashboardInventoryRiskTopSlow_SnapshotKey_Rank', 'UX_BTRPD_InventoryRiskTopSlow_SnapshotKey_Rank', 'INDEX';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardLocationAttention_LocationAttentionId', 'DF_BTRPD_LocationAttention_LocationAttentionId', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardLocationAttention_SnapshotKey', 'DF_BTRPD_LocationAttention_SnapshotKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardLocationAttention_EntityType', 'DF_BTRPD_LocationAttention_EntityType', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardLocationAttention_EntityName', 'DF_BTRPD_LocationAttention_EntityName', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardLocationAttention_SignalKey', 'DF_BTRPD_LocationAttention_SignalKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardLocationAttention_SignalLabel', 'DF_BTRPD_LocationAttention_SignalLabel', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardLocationAttention_SortOrder', 'DF_BTRPD_LocationAttention_SortOrder', 'OBJECT';
GO
EXEC sp_rename 'PK_BTR_PortalDashboardLocationAttention', 'PK_BTRPD_LocationAttention', 'OBJECT';
GO
EXEC sp_rename 'IX_BTR_PortalDashboardLocationAttention_SnapshotKey_SortOrder', 'IX_BTRPD_LocationAttention_SnapshotKey_SortOrder', 'INDEX';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardLocationKpi_SnapshotKey', 'DF_BTRPD_LocationKpi_SnapshotKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardLocationKpi_GeneratedAt', 'DF_BTRPD_LocationKpi_GeneratedAt', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardLocationKpi_PeriodYear', 'DF_BTRPD_LocationKpi_PeriodYear', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardLocationKpi_PeriodMonth', 'DF_BTRPD_LocationKpi_PeriodMonth', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardLocationKpi_InactiveWarehouseWithStockCount', 'DF_BTRPD_LocationKpi_InactiveWarehouseWithStockCount', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardLocationKpi_WarehouseNoSalesWithInventoryCount', 'DF_BTRPD_LocationKpi_WarehouseNoSalesWithInventoryCount', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardLocationKpi_TotalInventoryValue', 'DF_BTRPD_LocationKpi_TotalInventoryValue', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardLocationKpi_TotalAtRiskValue', 'DF_BTRPD_LocationKpi_TotalAtRiskValue', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardLocationKpi_TotalOmzet', 'DF_BTRPD_LocationKpi_TotalOmzet', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardLocationKpi_TotalPurchase', 'DF_BTRPD_LocationKpi_TotalPurchase', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardLocationKpi_LastRefreshLogId', 'DF_BTRPD_LocationKpi_LastRefreshLogId', 'OBJECT';
GO
EXEC sp_rename 'PK_BTR_PortalDashboardLocationKpi', 'PK_BTRPD_LocationKpi', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardLocationTopWarehouseAtRisk_LocationTopWarehouseAtRiskId', 'DF_BTRPD_LocationTopWarehouseAtRisk_LocationTopWarehouseAtRiskId', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardLocationTopWarehouseAtRisk_SnapshotKey', 'DF_BTRPD_LocationTopWarehouseAtRisk_SnapshotKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardLocationTopWarehouseAtRisk_Rank', 'DF_BTRPD_LocationTopWarehouseAtRisk_Rank', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardLocationTopWarehouseAtRisk_WarehouseId', 'DF_BTRPD_LocationTopWarehouseAtRisk_WarehouseId', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardLocationTopWarehouseAtRisk_WarehouseName', 'DF_BTRPD_LocationTopWarehouseAtRisk_WarehouseName', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardLocationTopWarehouseAtRisk_AtRiskValue', 'DF_BTRPD_LocationTopWarehouseAtRisk_AtRiskValue', 'OBJECT';
GO
EXEC sp_rename 'PK_BTR_PortalDashboardLocationTopWarehouseAtRisk', 'PK_BTRPD_LocationTopWarehouseAtRisk', 'OBJECT';
GO
EXEC sp_rename 'UX_BTR_PortalDashboardLocationTopWarehouseAtRisk_SnapshotKey_Rank', 'UX_BTRPD_LocationTopWarehouseAtRisk_SnapshotKey_Rank', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardLocationTopWarehouseInventory_LocationTopWarehouseInventoryId', 'DF_BTRPD_LocationTopWarehouseInventory_LocationTopWarehouseInventoryId', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardLocationTopWarehouseInventory_SnapshotKey', 'DF_BTRPD_LocationTopWarehouseInventory_SnapshotKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardLocationTopWarehouseInventory_Rank', 'DF_BTRPD_LocationTopWarehouseInventory_Rank', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardLocationTopWarehouseInventory_WarehouseId', 'DF_BTRPD_LocationTopWarehouseInventory_WarehouseId', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardLocationTopWarehouseInventory_WarehouseName', 'DF_BTRPD_LocationTopWarehouseInventory_WarehouseName', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardLocationTopWarehouseInventory_InventoryValue', 'DF_BTRPD_LocationTopWarehouseInventory_InventoryValue', 'OBJECT';
GO
EXEC sp_rename 'PK_BTR_PortalDashboardLocationTopWarehouseInventory', 'PK_BTRPD_LocationTopWarehouseInventory', 'OBJECT';
GO
EXEC sp_rename 'UX_BTR_PortalDashboardLocationTopWarehouseInventory_SnapshotKey_Rank', 'UX_BTRPD_LocationTopWarehouseInventory_SnapshotKey_Rank', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardLocationTopWarehousePurchasing_LocationTopWarehousePurchasingId', 'DF_BTRPD_LocationTopWarehousePurchasing_LocationTopWarehousePurchasingId', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardLocationTopWarehousePurchasing_SnapshotKey', 'DF_BTRPD_LocationTopWarehousePurchasing_SnapshotKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardLocationTopWarehousePurchasing_Rank', 'DF_BTRPD_LocationTopWarehousePurchasing_Rank', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardLocationTopWarehousePurchasing_WarehouseId', 'DF_BTRPD_LocationTopWarehousePurchasing_WarehouseId', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardLocationTopWarehousePurchasing_WarehouseName', 'DF_BTRPD_LocationTopWarehousePurchasing_WarehouseName', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardLocationTopWarehousePurchasing_MtdPurchaseAmount', 'DF_BTRPD_LocationTopWarehousePurchasing_MtdPurchaseAmount', 'OBJECT';
GO
EXEC sp_rename 'PK_BTR_PortalDashboardLocationTopWarehousePurchasing', 'PK_BTRPD_LocationTopWarehousePurchasing', 'OBJECT';
GO
EXEC sp_rename 'UX_BTR_PortalDashboardLocationTopWarehousePurchasing_SnapshotKey_Rank', 'UX_BTRPD_LocationTopWarehousePurchasing_SnapshotKey_Rank', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardLocationTopWarehouseSales_LocationTopWarehouseSalesId', 'DF_BTRPD_LocationTopWarehouseSales_LocationTopWarehouseSalesId', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardLocationTopWarehouseSales_SnapshotKey', 'DF_BTRPD_LocationTopWarehouseSales_SnapshotKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardLocationTopWarehouseSales_Rank', 'DF_BTRPD_LocationTopWarehouseSales_Rank', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardLocationTopWarehouseSales_WarehouseId', 'DF_BTRPD_LocationTopWarehouseSales_WarehouseId', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardLocationTopWarehouseSales_WarehouseName', 'DF_BTRPD_LocationTopWarehouseSales_WarehouseName', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardLocationTopWarehouseSales_MtdOmzet', 'DF_BTRPD_LocationTopWarehouseSales_MtdOmzet', 'OBJECT';
GO
EXEC sp_rename 'PK_BTR_PortalDashboardLocationTopWarehouseSales', 'PK_BTRPD_LocationTopWarehouseSales', 'OBJECT';
GO
EXEC sp_rename 'UX_BTR_PortalDashboardLocationTopWarehouseSales_SnapshotKey_Rank', 'UX_BTRPD_LocationTopWarehouseSales_SnapshotKey_Rank', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardLocationTopWilayahSales_LocationTopWilayahSalesId', 'DF_BTRPD_LocationTopWilayahSales_LocationTopWilayahSalesId', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardLocationTopWilayahSales_SnapshotKey', 'DF_BTRPD_LocationTopWilayahSales_SnapshotKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardLocationTopWilayahSales_Rank', 'DF_BTRPD_LocationTopWilayahSales_Rank', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardLocationTopWilayahSales_WilayahName', 'DF_BTRPD_LocationTopWilayahSales_WilayahName', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardLocationTopWilayahSales_MtdOmzet', 'DF_BTRPD_LocationTopWilayahSales_MtdOmzet', 'OBJECT';
GO
EXEC sp_rename 'PK_BTR_PortalDashboardLocationTopWilayahSales', 'PK_BTRPD_LocationTopWilayahSales', 'OBJECT';
GO
EXEC sp_rename 'UX_BTR_PortalDashboardLocationTopWilayahSales_SnapshotKey_Rank', 'UX_BTRPD_LocationTopWilayahSales_SnapshotKey_Rank', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPiutangAging_PiutangAgingId', 'DF_BTRPD_PiutangAging_PiutangAgingId', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPiutangAging_SnapshotKey', 'DF_BTRPD_PiutangAging_SnapshotKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPiutangAging_BucketKey', 'DF_BTRPD_PiutangAging_BucketKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPiutangAging_BucketLabel', 'DF_BTRPD_PiutangAging_BucketLabel', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPiutangAging_SortOrder', 'DF_BTRPD_PiutangAging_SortOrder', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPiutangAging_Amount', 'DF_BTRPD_PiutangAging_Amount', 'OBJECT';
GO
EXEC sp_rename 'PK_BTR_PortalDashboardPiutangAging', 'PK_BTRPD_PiutangAging', 'OBJECT';
GO
EXEC sp_rename 'UX_BTR_PortalDashboardPiutangAging_SnapshotKey_BucketKey', 'UX_BTRPD_PiutangAging_SnapshotKey_BucketKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPiutangKpi_SnapshotKey', 'DF_BTRPD_PiutangKpi_SnapshotKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPiutangKpi_GeneratedAt', 'DF_BTRPD_PiutangKpi_GeneratedAt', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPiutangKpi_TotalPiutang', 'DF_BTRPD_PiutangKpi_TotalPiutang', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPiutangKpi_TotalCustomer', 'DF_BTRPD_PiutangKpi_TotalCustomer', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPiutangKpi_OverdueCustomer', 'DF_BTRPD_PiutangKpi_OverdueCustomer', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPiutangKpi_LastRefreshLogId', 'DF_BTRPD_PiutangKpi_LastRefreshLogId', 'OBJECT';
GO
EXEC sp_rename 'PK_BTR_PortalDashboardPiutangKpi', 'PK_BTRPD_PiutangKpi', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPiutangTopCustomer_PiutangTopCustomerId', 'DF_BTRPD_PiutangTopCustomer_PiutangTopCustomerId', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPiutangTopCustomer_SnapshotKey', 'DF_BTRPD_PiutangTopCustomer_SnapshotKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPiutangTopCustomer_Rank', 'DF_BTRPD_PiutangTopCustomer_Rank', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPiutangTopCustomer_CustomerName', 'DF_BTRPD_PiutangTopCustomer_CustomerName', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPiutangTopCustomer_OutstandingBalance', 'DF_BTRPD_PiutangTopCustomer_OutstandingBalance', 'OBJECT';
GO
EXEC sp_rename 'PK_BTR_PortalDashboardPiutangTopCustomer', 'PK_BTRPD_PiutangTopCustomer', 'OBJECT';
GO
EXEC sp_rename 'UX_BTR_PortalDashboardPiutangTopCustomer_SnapshotKey_Rank', 'UX_BTRPD_PiutangTopCustomer_SnapshotKey_Rank', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPurchasingKpi_SnapshotKey', 'DF_BTRPD_PurchasingKpi_SnapshotKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPurchasingKpi_GeneratedAt', 'DF_BTRPD_PurchasingKpi_GeneratedAt', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPurchasingKpi_PeriodYear', 'DF_BTRPD_PurchasingKpi_PeriodYear', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPurchasingKpi_PeriodMonth', 'DF_BTRPD_PurchasingKpi_PeriodMonth', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPurchasingKpi_GrandTotalPurchase', 'DF_BTRPD_PurchasingKpi_GrandTotalPurchase', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPurchasingKpi_TotalInvoice', 'DF_BTRPD_PurchasingKpi_TotalInvoice', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPurchasingKpi_PendingPostingInvoiceCount', 'DF_BTRPD_PurchasingKpi_PendingPostingInvoiceCount', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPurchasingKpi_LastRefreshLogId', 'DF_BTRPD_PurchasingKpi_LastRefreshLogId', 'OBJECT';
GO
EXEC sp_rename 'PK_BTR_PortalDashboardPurchasingKpi', 'PK_BTRPD_PurchasingKpi', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPurchasingManagementAttention_PurchasingManagementAttentionId', 'DF_BTRPD_PurchasingManagementAttention_PurchasingManagementAttentionId', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPurchasingManagementAttention_SnapshotKey', 'DF_BTRPD_PurchasingManagementAttention_SnapshotKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPurchasingManagementAttention_EntityType', 'DF_BTRPD_PurchasingManagementAttention_EntityType', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPurchasingManagementAttention_EntityName', 'DF_BTRPD_PurchasingManagementAttention_EntityName', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPurchasingManagementAttention_SignalKey', 'DF_BTRPD_PurchasingManagementAttention_SignalKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPurchasingManagementAttention_SignalLabel', 'DF_BTRPD_PurchasingManagementAttention_SignalLabel', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPurchasingManagementAttention_SortOrder', 'DF_BTRPD_PurchasingManagementAttention_SortOrder', 'OBJECT';
GO
EXEC sp_rename 'PK_BTR_PortalDashboardPurchasingManagementAttention', 'PK_BTRPD_PurchasingManagementAttention', 'OBJECT';
GO
EXEC sp_rename 'IX_BTR_PortalDashboardPurchasingManagementAttention_SnapshotKey_SortOrder', 'IX_BTRPD_PurchasingManagementAttention_SnapshotKey_SortOrder', 'INDEX';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPurchasingManagementKpi_SnapshotKey', 'DF_BTRPD_PurchasingManagementKpi_SnapshotKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPurchasingManagementKpi_GeneratedAt', 'DF_BTRPD_PurchasingManagementKpi_GeneratedAt', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPurchasingManagementKpi_PeriodYear', 'DF_BTRPD_PurchasingManagementKpi_PeriodYear', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPurchasingManagementKpi_PeriodMonth', 'DF_BTRPD_PurchasingManagementKpi_PeriodMonth', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPurchasingManagementKpi_QualifiedBacklogCount', 'DF_BTRPD_PurchasingManagementKpi_QualifiedBacklogCount', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPurchasingManagementKpi_QualifiedBacklogValue', 'DF_BTRPD_PurchasingManagementKpi_QualifiedBacklogValue', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPurchasingManagementKpi_PendingPostingValue', 'DF_BTRPD_PurchasingManagementKpi_PendingPostingValue', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPurchasingManagementKpi_CompoundDependencyCount', 'DF_BTRPD_PurchasingManagementKpi_CompoundDependencyCount', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPurchasingManagementKpi_PrincipalInventoryNoPurchaseCount', 'DF_BTRPD_PurchasingManagementKpi_PrincipalInventoryNoPurchaseCount', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPurchasingManagementKpi_UnknownPrincipalCount', 'DF_BTRPD_PurchasingManagementKpi_UnknownPrincipalCount', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPurchasingManagementKpi_PurchasingInactivityFlag', 'DF_BTRPD_PurchasingManagementKpi_PurchasingInactivityFlag', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPurchasingManagementKpi_QualifiedBacklogPrincipalCount', 'DF_BTRPD_PurchasingManagementKpi_QualifiedBacklogPrincipalCount', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPurchasingManagementKpi_PrincipalAtRiskExposureCount', 'DF_BTRPD_PurchasingManagementKpi_PrincipalAtRiskExposureCount', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPurchasingManagementKpi_LastRefreshLogId', 'DF_BTRPD_PurchasingManagementKpi_LastRefreshLogId', 'OBJECT';
GO
EXEC sp_rename 'PK_BTR_PortalDashboardPurchasingManagementKpi', 'PK_BTRPD_PurchasingManagementKpi', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPurchasingManagementTopPrincipal_PurchasingManagementTopPrincipalId', 'DF_BTRPD_PurchasingManagementTopPrincipal_PurchasingManagementTopPrincipalId', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPurchasingManagementTopPrincipal_SnapshotKey', 'DF_BTRPD_PurchasingManagementTopPrincipal_SnapshotKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPurchasingManagementTopPrincipal_Rank', 'DF_BTRPD_PurchasingManagementTopPrincipal_Rank', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPurchasingManagementTopPrincipal_PrincipalName', 'DF_BTRPD_PurchasingManagementTopPrincipal_PrincipalName', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPurchasingManagementTopPrincipal_MtdPurchaseAmount', 'DF_BTRPD_PurchasingManagementTopPrincipal_MtdPurchaseAmount', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPurchasingManagementTopPrincipal_IsCompoundDependency', 'DF_BTRPD_PurchasingManagementTopPrincipal_IsCompoundDependency', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPurchasingManagementTopPrincipal_IsInventoryNoPurchase', 'DF_BTRPD_PurchasingManagementTopPrincipal_IsInventoryNoPurchase', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPurchasingManagementTopPrincipal_ReportRoute', 'DF_BTRPD_PurchasingManagementTopPrincipal_ReportRoute', 'OBJECT';
GO
EXEC sp_rename 'PK_BTR_PortalDashboardPurchasingManagementTopPrincipal', 'PK_BTRPD_PurchasingManagementTopPrincipal', 'OBJECT';
GO
EXEC sp_rename 'UX_BTR_PortalDashboardPurchasingManagementTopPrincipal_SnapshotKey_Rank', 'UX_BTRPD_PurchasingManagementTopPrincipal_SnapshotKey_Rank', 'INDEX';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPurchasingPostingStatus_PurchasingPostingStatusId', 'DF_BTRPD_PurchasingPostingStatus_PurchasingPostingStatusId', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPurchasingPostingStatus_SnapshotKey', 'DF_BTRPD_PurchasingPostingStatus_SnapshotKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPurchasingPostingStatus_StatusKey', 'DF_BTRPD_PurchasingPostingStatus_StatusKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPurchasingPostingStatus_StatusLabel', 'DF_BTRPD_PurchasingPostingStatus_StatusLabel', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPurchasingPostingStatus_SortOrder', 'DF_BTRPD_PurchasingPostingStatus_SortOrder', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPurchasingPostingStatus_PurchaseAmount', 'DF_BTRPD_PurchasingPostingStatus_PurchaseAmount', 'OBJECT';
GO
EXEC sp_rename 'PK_BTR_PortalDashboardPurchasingPostingStatus', 'PK_BTRPD_PurchasingPostingStatus', 'OBJECT';
GO
EXEC sp_rename 'UX_BTR_PortalDashboardPurchasingPostingStatus_SnapshotKey_StatusKey', 'UX_BTRPD_PurchasingPostingStatus_SnapshotKey_StatusKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPurchasingTopPrincipal_PurchasingTopPrincipalId', 'DF_BTRPD_PurchasingTopPrincipal_PurchasingTopPrincipalId', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPurchasingTopPrincipal_SnapshotKey', 'DF_BTRPD_PurchasingTopPrincipal_SnapshotKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPurchasingTopPrincipal_Rank', 'DF_BTRPD_PurchasingTopPrincipal_Rank', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPurchasingTopPrincipal_PrincipalName', 'DF_BTRPD_PurchasingTopPrincipal_PrincipalName', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPurchasingTopPrincipal_PurchaseAmount', 'DF_BTRPD_PurchasingTopPrincipal_PurchaseAmount', 'OBJECT';
GO
EXEC sp_rename 'PK_BTR_PortalDashboardPurchasingTopPrincipal', 'PK_BTRPD_PurchasingTopPrincipal', 'OBJECT';
GO
EXEC sp_rename 'UX_BTR_PortalDashboardPurchasingTopPrincipal_SnapshotKey_Rank', 'UX_BTRPD_PurchasingTopPrincipal_SnapshotKey_Rank', 'OBJECT';
GO
EXEC sp_rename 'IX_BTR_PortalDashboardPurchasingTopPrincipal_SnapshotKey_Rank', 'IX_BTRPD_PurchasingTopPrincipal_SnapshotKey_Rank', 'INDEX';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPurchasingWeekTrend_PurchasingWeekTrendId', 'DF_BTRPD_PurchasingWeekTrend_PurchasingWeekTrendId', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPurchasingWeekTrend_SnapshotKey', 'DF_BTRPD_PurchasingWeekTrend_SnapshotKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPurchasingWeekTrend_WeekStart', 'DF_BTRPD_PurchasingWeekTrend_WeekStart', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPurchasingWeekTrend_WeekEnd', 'DF_BTRPD_PurchasingWeekTrend_WeekEnd', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPurchasingWeekTrend_WeekLabel', 'DF_BTRPD_PurchasingWeekTrend_WeekLabel', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardPurchasingWeekTrend_PurchaseAmount', 'DF_BTRPD_PurchasingWeekTrend_PurchaseAmount', 'OBJECT';
GO
EXEC sp_rename 'PK_BTR_PortalDashboardPurchasingWeekTrend', 'PK_BTRPD_PurchasingWeekTrend', 'OBJECT';
GO
EXEC sp_rename 'IX_BTR_PortalDashboardPurchasingWeekTrend_SnapshotKey_WeekStart', 'IX_BTRPD_PurchasingWeekTrend_SnapshotKey_WeekStart', 'INDEX';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardRefreshLog_RefreshLogId', 'DF_BTRPD_RefreshLog_RefreshLogId', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardRefreshLog_Domain', 'DF_BTRPD_RefreshLog_Domain', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardRefreshLog_StartedAt', 'DF_BTRPD_RefreshLog_StartedAt', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardRefreshLog_Status', 'DF_BTRPD_RefreshLog_Status', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardRefreshLog_DurationMs', 'DF_BTRPD_RefreshLog_DurationMs', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardRefreshLog_ErrorMessage', 'DF_BTRPD_RefreshLog_ErrorMessage', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardRefreshLog_TriggeredBy', 'DF_BTRPD_RefreshLog_TriggeredBy', 'OBJECT';
GO
EXEC sp_rename 'PK_BTR_PortalDashboardRefreshLog', 'PK_BTRPD_RefreshLog', 'OBJECT';
GO
EXEC sp_rename 'IX_BTR_PortalDashboardRefreshLog_Domain_CompletedAt', 'IX_BTRPD_RefreshLog_Domain_CompletedAt', 'INDEX';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesKpi_SnapshotKey', 'DF_BTRPD_SalesKpi_SnapshotKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesKpi_GeneratedAt', 'DF_BTRPD_SalesKpi_GeneratedAt', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesKpi_PeriodYear', 'DF_BTRPD_SalesKpi_PeriodYear', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesKpi_PeriodMonth', 'DF_BTRPD_SalesKpi_PeriodMonth', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesKpi_TotalOmzet', 'DF_BTRPD_SalesKpi_TotalOmzet', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesKpi_TotalFaktur', 'DF_BTRPD_SalesKpi_TotalFaktur', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesKpi_TotalCustomer', 'DF_BTRPD_SalesKpi_TotalCustomer', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesKpi_TotalTarget', 'DF_BTRPD_SalesKpi_TotalTarget', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesKpi_TotalAchievement', 'DF_BTRPD_SalesKpi_TotalAchievement', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesKpi_CompletedOmzet', 'DF_BTRPD_SalesKpi_CompletedOmzet', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesKpi_PipelineOmzet', 'DF_BTRPD_SalesKpi_PipelineOmzet', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesKpi_LastRefreshLogId', 'DF_BTRPD_SalesKpi_LastRefreshLogId', 'OBJECT';
GO
EXEC sp_rename 'PK_BTR_PortalDashboardSalesKpi', 'PK_BTRPD_SalesKpi', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesmanAttention_SalesmanAttentionId', 'DF_BTRPD_SalesmanAttention_SalesmanAttentionId', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesmanAttention_SnapshotKey', 'DF_BTRPD_SalesmanAttention_SnapshotKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesmanAttention_SalesPersonId', 'DF_BTRPD_SalesmanAttention_SalesPersonId', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesmanAttention_SalesPersonCode', 'DF_BTRPD_SalesmanAttention_SalesPersonCode', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesmanAttention_SalesPersonName', 'DF_BTRPD_SalesmanAttention_SalesPersonName', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesmanAttention_SignalKey', 'DF_BTRPD_SalesmanAttention_SignalKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesmanAttention_SignalLabel', 'DF_BTRPD_SalesmanAttention_SignalLabel', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesmanAttention_WilayahName', 'DF_BTRPD_SalesmanAttention_WilayahName', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesmanAttention_SortOrder', 'DF_BTRPD_SalesmanAttention_SortOrder', 'OBJECT';
GO
EXEC sp_rename 'PK_BTR_PortalDashboardSalesmanAttention', 'PK_BTRPD_SalesmanAttention', 'OBJECT';
GO
EXEC sp_rename 'IX_BTR_PortalDashboardSalesmanAttention_SnapshotKey_SortOrder', 'IX_BTRPD_SalesmanAttention_SnapshotKey_SortOrder', 'INDEX';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesmanKpi_SnapshotKey', 'DF_BTRPD_SalesmanKpi_SnapshotKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesmanKpi_GeneratedAt', 'DF_BTRPD_SalesmanKpi_GeneratedAt', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesmanKpi_PeriodYear', 'DF_BTRPD_SalesmanKpi_PeriodYear', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesmanKpi_PeriodMonth', 'DF_BTRPD_SalesmanKpi_PeriodMonth', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesmanKpi_TotalTeamOmzet', 'DF_BTRPD_SalesmanKpi_TotalTeamOmzet', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesmanKpi_TotalPiutang', 'DF_BTRPD_SalesmanKpi_TotalPiutang', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesmanKpi_ActiveSalesmanCount', 'DF_BTRPD_SalesmanKpi_ActiveSalesmanCount', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesmanKpi_BelowTargetCount', 'DF_BTRPD_SalesmanKpi_BelowTargetCount', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesmanKpi_NoTargetCount', 'DF_BTRPD_SalesmanKpi_NoTargetCount', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesmanKpi_HighOverdueExposureCount', 'DF_BTRPD_SalesmanKpi_HighOverdueExposureCount', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesmanKpi_HighPiutangExposureCount', 'DF_BTRPD_SalesmanKpi_HighPiutangExposureCount', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesmanKpi_CustomerConcentrationCount', 'DF_BTRPD_SalesmanKpi_CustomerConcentrationCount', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesmanKpi_DormantPortfolioCount', 'DF_BTRPD_SalesmanKpi_DormantPortfolioCount', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesmanKpi_LastRefreshLogId', 'DF_BTRPD_SalesmanKpi_LastRefreshLogId', 'OBJECT';
GO
EXEC sp_rename 'PK_BTR_PortalDashboardSalesmanKpi', 'PK_BTRPD_SalesmanKpi', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesmanSegmentation_SalesmanSegmentationId', 'DF_BTRPD_SalesmanSegmentation_SalesmanSegmentationId', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesmanSegmentation_SnapshotKey', 'DF_BTRPD_SalesmanSegmentation_SnapshotKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesmanSegmentation_SegmentType', 'DF_BTRPD_SalesmanSegmentation_SegmentType', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesmanSegmentation_SegmentKey', 'DF_BTRPD_SalesmanSegmentation_SegmentKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesmanSegmentation_SegmentLabel', 'DF_BTRPD_SalesmanSegmentation_SegmentLabel', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesmanSegmentation_SalesmanCount', 'DF_BTRPD_SalesmanSegmentation_SalesmanCount', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesmanSegmentation_ActiveCount', 'DF_BTRPD_SalesmanSegmentation_ActiveCount', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesmanSegmentation_InactiveCount', 'DF_BTRPD_SalesmanSegmentation_InactiveCount', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesmanSegmentation_SortOrder', 'DF_BTRPD_SalesmanSegmentation_SortOrder', 'OBJECT';
GO
EXEC sp_rename 'PK_BTR_PortalDashboardSalesmanSegmentation', 'PK_BTRPD_SalesmanSegmentation', 'OBJECT';
GO
EXEC sp_rename 'UX_BTR_PortalDashboardSalesmanSegmentation_SnapshotKey_SegmentType_SegmentKey', 'UX_BTRPD_SalesmanSegmentation_SnapshotKey_SegmentType_SegmentKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesmanTopAchievement_SalesmanTopAchievementId', 'DF_BTRPD_SalesmanTopAchievement_SalesmanTopAchievementId', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesmanTopAchievement_SnapshotKey', 'DF_BTRPD_SalesmanTopAchievement_SnapshotKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesmanTopAchievement_Rank', 'DF_BTRPD_SalesmanTopAchievement_Rank', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesmanTopAchievement_SalesPersonId', 'DF_BTRPD_SalesmanTopAchievement_SalesPersonId', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesmanTopAchievement_SalesPersonCode', 'DF_BTRPD_SalesmanTopAchievement_SalesPersonCode', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesmanTopAchievement_SalesPersonName', 'DF_BTRPD_SalesmanTopAchievement_SalesPersonName', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesmanTopAchievement_CompletedOmzet', 'DF_BTRPD_SalesmanTopAchievement_CompletedOmzet', 'OBJECT';
GO
EXEC sp_rename 'PK_BTR_PortalDashboardSalesmanTopAchievement', 'PK_BTRPD_SalesmanTopAchievement', 'OBJECT';
GO
EXEC sp_rename 'UX_BTR_PortalDashboardSalesmanTopAchievement_SnapshotKey_Rank', 'UX_BTRPD_SalesmanTopAchievement_SnapshotKey_Rank', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesmanTopOmzet_SalesmanTopOmzetId', 'DF_BTRPD_SalesmanTopOmzet_SalesmanTopOmzetId', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesmanTopOmzet_SnapshotKey', 'DF_BTRPD_SalesmanTopOmzet_SnapshotKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesmanTopOmzet_Rank', 'DF_BTRPD_SalesmanTopOmzet_Rank', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesmanTopOmzet_SalesPersonId', 'DF_BTRPD_SalesmanTopOmzet_SalesPersonId', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesmanTopOmzet_SalesPersonCode', 'DF_BTRPD_SalesmanTopOmzet_SalesPersonCode', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesmanTopOmzet_SalesPersonName', 'DF_BTRPD_SalesmanTopOmzet_SalesPersonName', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesmanTopOmzet_CompletedOmzet', 'DF_BTRPD_SalesmanTopOmzet_CompletedOmzet', 'OBJECT';
GO
EXEC sp_rename 'PK_BTR_PortalDashboardSalesmanTopOmzet', 'PK_BTRPD_SalesmanTopOmzet', 'OBJECT';
GO
EXEC sp_rename 'UX_BTR_PortalDashboardSalesmanTopOmzet_SnapshotKey_Rank', 'UX_BTRPD_SalesmanTopOmzet_SnapshotKey_Rank', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesmanTopPiutang_SalesmanTopPiutangId', 'DF_BTRPD_SalesmanTopPiutang_SalesmanTopPiutangId', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesmanTopPiutang_SnapshotKey', 'DF_BTRPD_SalesmanTopPiutang_SnapshotKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesmanTopPiutang_Rank', 'DF_BTRPD_SalesmanTopPiutang_Rank', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesmanTopPiutang_SalesPersonId', 'DF_BTRPD_SalesmanTopPiutang_SalesPersonId', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesmanTopPiutang_SalesPersonCode', 'DF_BTRPD_SalesmanTopPiutang_SalesPersonCode', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesmanTopPiutang_SalesPersonName', 'DF_BTRPD_SalesmanTopPiutang_SalesPersonName', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesmanTopPiutang_OutstandingBalance', 'DF_BTRPD_SalesmanTopPiutang_OutstandingBalance', 'OBJECT';
GO
EXEC sp_rename 'PK_BTR_PortalDashboardSalesmanTopPiutang', 'PK_BTRPD_SalesmanTopPiutang', 'OBJECT';
GO
EXEC sp_rename 'UX_BTR_PortalDashboardSalesmanTopPiutang_SnapshotKey_Rank', 'UX_BTRPD_SalesmanTopPiutang_SnapshotKey_Rank', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesTopSalesman_SalesTopSalesmanId', 'DF_BTRPD_SalesTopSalesman_SalesTopSalesmanId', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesTopSalesman_SnapshotKey', 'DF_BTRPD_SalesTopSalesman_SnapshotKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesTopSalesman_Rank', 'DF_BTRPD_SalesTopSalesman_Rank', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesTopSalesman_SalesPersonName', 'DF_BTRPD_SalesTopSalesman_SalesPersonName', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesTopSalesman_CompletedOmzet', 'DF_BTRPD_SalesTopSalesman_CompletedOmzet', 'OBJECT';
GO
EXEC sp_rename 'PK_BTR_PortalDashboardSalesTopSalesman', 'PK_BTRPD_SalesTopSalesman', 'OBJECT';
GO
EXEC sp_rename 'UX_BTR_PortalDashboardSalesTopSalesman_SnapshotKey_Rank', 'UX_BTRPD_SalesTopSalesman_SnapshotKey_Rank', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesWeekTrend_SalesWeekTrendId', 'DF_BTRPD_SalesWeekTrend_SalesWeekTrendId', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesWeekTrend_SnapshotKey', 'DF_BTRPD_SalesWeekTrend_SnapshotKey', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesWeekTrend_WeekStart', 'DF_BTRPD_SalesWeekTrend_WeekStart', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesWeekTrend_WeekEnd', 'DF_BTRPD_SalesWeekTrend_WeekEnd', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesWeekTrend_WeekLabel', 'DF_BTRPD_SalesWeekTrend_WeekLabel', 'OBJECT';
GO
EXEC sp_rename 'DF_BTR_PortalDashboardSalesWeekTrend_RecognizedAmount', 'DF_BTRPD_SalesWeekTrend_RecognizedAmount', 'OBJECT';
GO
EXEC sp_rename 'PK_BTR_PortalDashboardSalesWeekTrend', 'PK_BTRPD_SalesWeekTrend', 'OBJECT';
GO
EXEC sp_rename 'IX_BTR_PortalDashboardSalesWeekTrend_SnapshotKey_WeekStart', 'IX_BTRPD_SalesWeekTrend_SnapshotKey_WeekStart', 'INDEX';
GO

-- Optional: remove obsolete portal ParamNo prefixes
DELETE FROM BTR_ParamNo WHERE Prefix LIKE 'PD%';
GO
