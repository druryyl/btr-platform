-- M32.1 Entity Analytics Platform — idempotent schema upgrade.
-- Adds L0 CURRENT snapshot table for generic entity analytics metrics.
-- Entity producers populate rows on domain worker refresh (M32.2+).
SET NOCOUNT ON;
GO

:r ..\Tables\ReportingContext\BTRPD_EntityAnalytics_Current.sql

GO

:r ..\Tables\ReportingContext\BTRPD_EntityAnalytics_Monthly.sql

GO

:r ..\Tables\ReportingContext\BTRPD_EntityAnalytics_MonthClose.sql

GO

:r ..\Tables\ReportingContext\BTRPD_EntityAnalytics_Ranking.sql

GO

:r ..\Tables\ReportingContext\BTRPD_EntityAnalytics_Attention.sql

GO

:r ..\Tables\ReportingContext\BTRPD_EntityAnalytics_Relationship.sql

GO

:r ..\Tables\ReportingContext\BTRPD_EntityAnalytics_Radar.sql

GO
