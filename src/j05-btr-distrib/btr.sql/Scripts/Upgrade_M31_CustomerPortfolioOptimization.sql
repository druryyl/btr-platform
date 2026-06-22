-- M31 Customer Portfolio Optimization Dashboard — idempotent schema upgrade.
-- Adds 8 snapshot tables for portfolio optimization materialized dashboard.
-- Next Customer domain refresh populates tables; no data backfill required.
SET NOCOUNT ON;
GO

:r ..\Tables\ReportingContext\BTRPD_CustomerPortfolioKpi.sql
:r ..\Tables\ReportingContext\BTRPD_CustomerPortfolioLifecycleDist.sql
:r ..\Tables\ReportingContext\BTRPD_CustomerPortfolioTierDist.sql
:r ..\Tables\ReportingContext\BTRPD_CustomerPortfolioActionDist.sql
:r ..\Tables\ReportingContext\BTRPD_CustomerPortfolioPriority.sql
:r ..\Tables\ReportingContext\BTRPD_CustomerPortfolioCustomer.sql
:r ..\Tables\ReportingContext\BTRPD_CustomerPortfolioConcentration.sql
:r ..\Tables\ReportingContext\BTRPD_CustomerPortfolioWilayah.sql

GO
