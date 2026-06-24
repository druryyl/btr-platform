CREATE TABLE BTRPD_CollectionOptimizationKpi
(
    SnapshotKey                     VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationKpi_SnapshotKey DEFAULT('CURRENT'),
    GeneratedAt                       DATETIME       NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationKpi_GeneratedAt DEFAULT('3000-01-01'),
    BusinessDate                      DATETIME       NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationKpi_BusinessDate DEFAULT('3000-01-01'),
    ActionsTodayCount                 INT            NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationKpi_ActionsTodayCount DEFAULT(0),
    ImmediateCollectionCount          INT            NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationKpi_ImmediateCollectionCount DEFAULT(0),
    ProactiveReminderCount            INT            NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationKpi_ProactiveReminderCount DEFAULT(0),
    CreditReviewCount                 INT            NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationKpi_CreditReviewCount DEFAULT(0),
    SalesRecoveryCount                INT            NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationKpi_SalesRecoveryCount DEFAULT(0),
    EscalateManagementCount           INT            NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationKpi_EscalateManagementCount DEFAULT(0),
    CollectionImpactTotal             DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationKpi_CollectionImpactTotal DEFAULT(0),
    ImmediateImpactTotal              DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationKpi_ImmediateImpactTotal DEFAULT(0),
    OverdueExposure                   DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationKpi_OverdueExposure DEFAULT(0),
    DueWithin7Days                    DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationKpi_DueWithin7Days DEFAULT(0),
    RecoveryVsBillingPercent          DECIMAL(9,4)   NULL,
    DeferNoActionCount                INT            NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationKpi_DeferNoActionCount DEFAULT(0),
    PlanningConfidence                VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationKpi_PlanningConfidence DEFAULT(''),
    ExecutiveSummaryText              VARCHAR(2000)  NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationKpi_ExecutiveSummaryText DEFAULT(''),
    LastRefreshLogId                  VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_CollectionOptimizationKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO
