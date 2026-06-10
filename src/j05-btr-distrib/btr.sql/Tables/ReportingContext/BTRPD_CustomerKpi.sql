CREATE TABLE BTRPD_CustomerKpi
(
    SnapshotKey               VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_CustomerKpi_SnapshotKey DEFAULT('CURRENT'),
    GeneratedAt               DATETIME      NOT NULL CONSTRAINT DF_BTRPD_CustomerKpi_GeneratedAt DEFAULT('3000-01-01'),
    PeriodYear                INT           NOT NULL CONSTRAINT DF_BTRPD_CustomerKpi_PeriodYear DEFAULT(0),
    PeriodMonth               INT           NOT NULL CONSTRAINT DF_BTRPD_CustomerKpi_PeriodMonth DEFAULT(0),
    TotalOmzet                DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CustomerKpi_TotalOmzet DEFAULT(0),
    TotalPiutang              DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CustomerKpi_TotalPiutang DEFAULT(0),
    ActiveCustomerCount       INT           NOT NULL CONSTRAINT DF_BTRPD_CustomerKpi_ActiveCustomerCount DEFAULT(0),
    DormantCustomerCount      INT           NOT NULL CONSTRAINT DF_BTRPD_CustomerKpi_DormantCustomerCount DEFAULT(0),
    OverdueCustomerCount      INT           NOT NULL CONSTRAINT DF_BTRPD_CustomerKpi_OverdueCustomerCount DEFAULT(0),
    PlafondBreachCount        INT           NOT NULL CONSTRAINT DF_BTRPD_CustomerKpi_PlafondBreachCount DEFAULT(0),
    SuspendedWithSalesCount   INT           NOT NULL CONSTRAINT DF_BTRPD_CustomerKpi_SuspendedWithSalesCount DEFAULT(0),
    AgingOver90Amount         DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CustomerKpi_AgingOver90Amount DEFAULT(0),
    TopOmzetCustomerPercent   DECIMAL(9,4)  NULL,
    TopPiutangCustomerPercent DECIMAL(9,4)  NULL,
    LastRefreshLogId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_CustomerKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_CustomerKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO
