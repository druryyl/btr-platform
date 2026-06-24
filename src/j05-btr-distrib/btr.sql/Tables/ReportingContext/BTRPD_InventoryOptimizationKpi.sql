CREATE TABLE BTRPD_InventoryOptimizationKpi
(
    SnapshotKey                     VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationKpi_SnapshotKey DEFAULT('CURRENT'),
    GeneratedAt                       DATETIME       NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationKpi_GeneratedAt DEFAULT('3000-01-01'),
    BusinessDate                      DATETIME       NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationKpi_BusinessDate DEFAULT('3000-01-01'),
    PlanningHorizonDays               INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationKpi_PlanningHorizonDays DEFAULT(0),
    BudgetCapIdr                      DECIMAL(18,2)  NULL,
    InventoryHealthScore              INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationKpi_InventoryHealthScore DEFAULT(0),
    CriticalActionCount               INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationKpi_CriticalActionCount DEFAULT(0),
    HighActionCount                   INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationKpi_HighActionCount DEFAULT(0),
    MediumActionCount                 INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationKpi_MediumActionCount DEFAULT(0),
    LowActionCount                    INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationKpi_LowActionCount DEFAULT(0),
    PurchaseNowCount                  INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationKpi_PurchaseNowCount DEFAULT(0),
    DelayCount                        INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationKpi_DelayCount DEFAULT(0),
    TransferCount                     INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationKpi_TransferCount DEFAULT(0),
    ClearanceCount                    INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationKpi_ClearanceCount DEFAULT(0),
    PostFirstCount                    INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationKpi_PostFirstCount DEFAULT(0),
    DeferCount                        INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationKpi_DeferCount DEFAULT(0),
    RequiredPurchaseBudgetIdr         DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationKpi_RequiredPurchaseBudgetIdr DEFAULT(0),
    RecommendedPurchaseBudgetIdr      DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationKpi_RecommendedPurchaseBudgetIdr DEFAULT(0),
    DeferrableSpendIdr                DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationKpi_DeferrableSpendIdr DEFAULT(0),
    RecoverableCapitalIdr             DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationKpi_RecoverableCapitalIdr DEFAULT(0),
    LastRefreshLogId                  VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_InventoryOptimizationKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO
