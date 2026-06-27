CREATE TABLE BTRPD_FieldActivityKpi
(
    SnapshotKey               VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_FieldActivityKpi_SnapshotKey DEFAULT('CURRENT'),
    GeneratedAt               DATETIME      NOT NULL CONSTRAINT DF_BTRPD_FieldActivityKpi_GeneratedAt DEFAULT('3000-01-01'),
    ActivityDate              DATE          NOT NULL CONSTRAINT DF_BTRPD_FieldActivityKpi_ActivityDate DEFAULT('3000-01-01'),
    ActiveSalesmenCount       INT           NOT NULL CONSTRAINT DF_BTRPD_FieldActivityKpi_ActiveSalesmenCount DEFAULT(0),
    PlannedVisits             INT           NOT NULL CONSTRAINT DF_BTRPD_FieldActivityKpi_PlannedVisits DEFAULT(0),
    ActualVisits              INT           NOT NULL CONSTRAINT DF_BTRPD_FieldActivityKpi_ActualVisits DEFAULT(0),
    VisitExecutionPercent     DECIMAL(9,4)  NULL,
    EffectiveCalls            INT           NOT NULL CONSTRAINT DF_BTRPD_FieldActivityKpi_EffectiveCalls DEFAULT(0),
    EffectiveCallRate         DECIMAL(9,4)  NULL,
    MissedVisits              INT           NOT NULL CONSTRAINT DF_BTRPD_FieldActivityKpi_MissedVisits DEFAULT(0),
    UnplannedVisits           INT           NOT NULL CONSTRAINT DF_BTRPD_FieldActivityKpi_UnplannedVisits DEFAULT(0),
    GpsValidRate              DECIMAL(9,4)  NULL,
    TotalOrders               INT           NOT NULL CONSTRAINT DF_BTRPD_FieldActivityKpi_TotalOrders DEFAULT(0),
    TotalOmzet                DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_FieldActivityKpi_TotalOmzet DEFAULT(0),
    LastRefreshLogId          VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_FieldActivityKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_FieldActivityKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO
