-- M30 Collection Optimization Dashboard — idempotent schema upgrade.
-- Adds 6 snapshot tables for collection optimization materialized dashboard.
-- Next Customer domain refresh populates tables; no data backfill required.
SET NOCOUNT ON;
GO

:r ..\Tables\ReportingContext\BTRPD_CollectionOptimizationKpi.sql
:r ..\Tables\ReportingContext\BTRPD_CollectionOptimizationActionDist.sql
:r ..\Tables\ReportingContext\BTRPD_CollectionOptimizationWorkload.sql
:r ..\Tables\ReportingContext\BTRPD_CollectionOptimizationPriority.sql
:r ..\Tables\ReportingContext\BTRPD_CollectionOptimizationQueue.sql
:r ..\Tables\ReportingContext\BTRPD_CollectionOptimizationImpact.sql

GO
