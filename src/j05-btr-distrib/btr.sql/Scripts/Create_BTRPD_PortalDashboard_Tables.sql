-- Create all BTR Portal materialized dashboard tables (BTRPD_*) and refresh index.
-- Idempotent: skips objects that already exist.
-- Prerequisites: BTR_Piutang must exist (for IX_BTR_Piutang_OpenBalance).
-- Source of truth: btr.sql/Tables/ReportingContext/BTRPD_*.sql
-- After running: btr.portal.worker --domain All --triggered-by Manual
SET NOCOUNT ON;
GO

-- BTRPD_RefreshLog
IF OBJECT_ID(N'dbo.BTRPD_RefreshLog', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_RefreshLog
(
    RefreshLogId VARCHAR(26)  NOT NULL CONSTRAINT DF_BTRPD_RefreshLog_RefreshLogId DEFAULT(''),
    Domain         VARCHAR(20)  NOT NULL CONSTRAINT DF_BTRPD_RefreshLog_Domain DEFAULT(''),
    StartedAt      DATETIME     NOT NULL CONSTRAINT DF_BTRPD_RefreshLog_StartedAt DEFAULT('3000-01-01'),
    CompletedAt    DATETIME     NULL,
    Status         VARCHAR(10)  NOT NULL CONSTRAINT DF_BTRPD_RefreshLog_Status DEFAULT(''),
    DurationMs     INT          NOT NULL CONSTRAINT DF_BTRPD_RefreshLog_DurationMs DEFAULT(0),
    ErrorMessage   VARCHAR(500) NOT NULL CONSTRAINT DF_BTRPD_RefreshLog_ErrorMessage DEFAULT(''),
    TriggeredBy    VARCHAR(20)  NOT NULL CONSTRAINT DF_BTRPD_RefreshLog_TriggeredBy DEFAULT(''),

    CONSTRAINT PK_BTRPD_RefreshLog PRIMARY KEY CLUSTERED (RefreshLogId)
)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_RefreshLog_Domain_CompletedAt' AND object_id = OBJECT_ID(N'dbo.BTRPD_RefreshLog'))
CREATE INDEX IX_BTRPD_RefreshLog_Domain_CompletedAt
    ON BTRPD_RefreshLog (Domain, CompletedAt DESC)
GO

-- BTRPD_PiutangKpi
IF OBJECT_ID(N'dbo.BTRPD_PiutangKpi', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_PiutangKpi
(
    SnapshotKey                         VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PiutangKpi_SnapshotKey DEFAULT('CURRENT'),
    GeneratedAt                         DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PiutangKpi_GeneratedAt DEFAULT('3000-01-01'),
    TotalPiutang                        DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangKpi_TotalPiutang DEFAULT(0),
    TotalCustomer                       INT           NOT NULL CONSTRAINT DF_BTRPD_PiutangKpi_TotalCustomer DEFAULT(0),
    OverdueCustomer                     INT           NOT NULL CONSTRAINT DF_BTRPD_PiutangKpi_OverdueCustomer DEFAULT(0),
    OverduePiutang                      DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangKpi_OverduePiutang DEFAULT(0),
    AgingOver90Amount                   DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangKpi_AgingOver90Amount DEFAULT(0),
    AgingOver90Percent                  DECIMAL(9,4)  NULL,
    Top10CustomerConcentrationPercent   DECIMAL(9,4)  NULL,
    Top20CustomerConcentrationPercent   DECIMAL(9,4)  NULL,
    LastRefreshLogId                    VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PiutangKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PiutangKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
END
GO

-- BTRPD_PiutangAging
IF OBJECT_ID(N'dbo.BTRPD_PiutangAging', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_PiutangAging
(
    PiutangAgingId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PiutangAging_PiutangAgingId DEFAULT(''),
    SnapshotKey    VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PiutangAging_SnapshotKey DEFAULT('CURRENT'),
    BucketKey      VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PiutangAging_BucketKey DEFAULT(''),
    BucketLabel    VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PiutangAging_BucketLabel DEFAULT(''),
    SortOrder      INT           NOT NULL CONSTRAINT DF_BTRPD_PiutangAging_SortOrder DEFAULT(0),
    Amount         DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangAging_Amount DEFAULT(0),

    CONSTRAINT PK_BTRPD_PiutangAging PRIMARY KEY CLUSTERED (PiutangAgingId),
    CONSTRAINT UX_BTRPD_PiutangAging_SnapshotKey_BucketKey UNIQUE (SnapshotKey, BucketKey)
)
END
GO

-- BTRPD_PiutangTopCustomer
IF OBJECT_ID(N'dbo.BTRPD_PiutangTopCustomer', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_PiutangTopCustomer
(
    PiutangTopCustomerId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomer_PiutangTopCustomerId DEFAULT(''),
    SnapshotKey          VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomer_SnapshotKey DEFAULT('CURRENT'),
    Rank                 INT           NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomer_Rank DEFAULT(0),
    CustomerName         VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomer_CustomerName DEFAULT(''),
    OutstandingBalance   DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomer_OutstandingBalance DEFAULT(0),

    CONSTRAINT PK_BTRPD_PiutangTopCustomer PRIMARY KEY CLUSTERED (PiutangTopCustomerId),
    CONSTRAINT UX_BTRPD_PiutangTopCustomer_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)
)
END
GO

-- BTRPD_PiutangCustomerAging
IF OBJECT_ID(N'dbo.BTRPD_PiutangCustomerAging', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_PiutangCustomerAging
(
    PiutangCustomerAgingId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PiutangCustomerAging_PiutangCustomerAgingId DEFAULT(''),
    SnapshotKey            VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PiutangCustomerAging_SnapshotKey DEFAULT('CURRENT'),
    CustomerId             VARCHAR(13)   NOT NULL CONSTRAINT DF_BTRPD_PiutangCustomerAging_CustomerId DEFAULT(''),
    CustomerCode           VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PiutangCustomerAging_CustomerCode DEFAULT(''),
    CustomerName           VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PiutangCustomerAging_CustomerName DEFAULT(''),
    CurrentAmount          DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangCustomerAging_CurrentAmount DEFAULT(0),
    Aging30Amount          DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangCustomerAging_Aging30Amount DEFAULT(0),
    Aging60Amount          DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangCustomerAging_Aging60Amount DEFAULT(0),
    Aging90Amount          DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangCustomerAging_Aging90Amount DEFAULT(0),
    AgingOver90Amount      DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangCustomerAging_AgingOver90Amount DEFAULT(0),
    LastUpdate             DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PiutangCustomerAging_LastUpdate DEFAULT('3000-01-01'),

    CONSTRAINT PK_BTRPD_PiutangCustomerAging PRIMARY KEY CLUSTERED (PiutangCustomerAgingId),
    CONSTRAINT UX_BTRPD_PiutangCustomerAging_SnapshotKey_CustomerId UNIQUE (SnapshotKey, CustomerId)
)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_PiutangCustomerAging_SnapshotKey' AND object_id = OBJECT_ID(N'dbo.BTRPD_PiutangCustomerAging'))
CREATE INDEX IX_BTRPD_PiutangCustomerAging_SnapshotKey
    ON BTRPD_PiutangCustomerAging (SnapshotKey)
GO

-- BTRPD_PiutangTopCustomerRisk
IF OBJECT_ID(N'dbo.BTRPD_PiutangTopCustomerRisk', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_PiutangTopCustomerRisk
(
    PiutangTopCustomerRiskId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomerRisk_PiutangTopCustomerRiskId DEFAULT(''),
    SnapshotKey              VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomerRisk_SnapshotKey DEFAULT('CURRENT'),
    Rank                     INT           NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomerRisk_Rank DEFAULT(0),
    CustomerId               VARCHAR(13)   NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomerRisk_CustomerId DEFAULT(''),
    CustomerCode             VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomerRisk_CustomerCode DEFAULT(''),
    CustomerName             VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomerRisk_CustomerName DEFAULT(''),
    TotalPiutang             DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomerRisk_TotalPiutang DEFAULT(0),
    CurrentAmount            DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomerRisk_CurrentAmount DEFAULT(0),
    Aging30Amount            DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomerRisk_Aging30Amount DEFAULT(0),
    Aging60Amount            DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomerRisk_Aging60Amount DEFAULT(0),
    Aging90Amount            DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomerRisk_Aging90Amount DEFAULT(0),
    AgingOver90Amount        DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomerRisk_AgingOver90Amount DEFAULT(0),

    CONSTRAINT PK_BTRPD_PiutangTopCustomerRisk PRIMARY KEY CLUSTERED (PiutangTopCustomerRiskId),
    CONSTRAINT UX_BTRPD_PiutangTopCustomerRisk_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)
)
END
GO

-- BTRPD_InventoryKpi
IF OBJECT_ID(N'dbo.BTRPD_InventoryKpi', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_InventoryKpi
(
    SnapshotKey          VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_InventoryKpi_SnapshotKey DEFAULT('CURRENT'),
    GeneratedAt          DATETIME      NOT NULL CONSTRAINT DF_BTRPD_InventoryKpi_GeneratedAt DEFAULT('3000-01-01'),
    TotalInventoryValue  DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_InventoryKpi_TotalInventoryValue DEFAULT(0),
    TotalItem            INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryKpi_TotalItem DEFAULT(0),
    LastRefreshLogId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_InventoryKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_InventoryKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
END
GO

-- BTRPD_InventoryBreakdown
IF OBJECT_ID(N'dbo.BTRPD_InventoryBreakdown', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_InventoryBreakdown
(
    InventoryBreakdownId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_InventoryBreakdown_InventoryBreakdownId DEFAULT(''),
    SnapshotKey          VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_InventoryBreakdown_SnapshotKey DEFAULT('CURRENT'),
    DimensionType        VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_InventoryBreakdown_DimensionType DEFAULT(''),
    Name                 VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_InventoryBreakdown_Name DEFAULT(''),
    InventoryValue       DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_InventoryBreakdown_InventoryValue DEFAULT(0),
    IsTop10              BIT           NOT NULL CONSTRAINT DF_BTRPD_InventoryBreakdown_IsTop10 DEFAULT(0),
    Top10Rank            INT           NULL,

    CONSTRAINT PK_BTRPD_InventoryBreakdown PRIMARY KEY CLUSTERED (InventoryBreakdownId)
)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_InventoryBreakdown_SnapshotKey_DimensionType' AND object_id = OBJECT_ID(N'dbo.BTRPD_InventoryBreakdown'))
CREATE INDEX IX_BTRPD_InventoryBreakdown_SnapshotKey_DimensionType
    ON BTRPD_InventoryBreakdown (SnapshotKey, DimensionType)
GO

-- BTRPD_InventoryRiskKpi
IF OBJECT_ID(N'dbo.BTRPD_InventoryRiskKpi', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_InventoryRiskKpi
(
    SnapshotKey              VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskKpi_SnapshotKey DEFAULT('CURRENT'),
    GeneratedAt              DATETIME      NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskKpi_GeneratedAt DEFAULT('3000-01-01'),
    TotalInventoryValue      DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskKpi_TotalInventoryValue DEFAULT(0),
    TotalItem                INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskKpi_TotalItem DEFAULT(0),
    DeadStockItemCount       INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskKpi_DeadStockItemCount DEFAULT(0),
    DeadStockValue           DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskKpi_DeadStockValue DEFAULT(0),
    SlowMovingItemCount      INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskKpi_SlowMovingItemCount DEFAULT(0),
    SlowMovingValue          DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskKpi_SlowMovingValue DEFAULT(0),
    NeverSoldItemCount       INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskKpi_NeverSoldItemCount DEFAULT(0),
    NeverSoldValue           DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskKpi_NeverSoldValue DEFAULT(0),
    AtRiskInventoryValue     DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskKpi_AtRiskInventoryValue DEFAULT(0),
    AtRiskInventoryPercent   DECIMAL(9,4)  NULL,
    RequiresAttention        BIT           NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskKpi_RequiresAttention DEFAULT(0),
    LastRefreshLogId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_InventoryRiskKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
END
GO

-- BTRPD_InventoryRiskAging
IF OBJECT_ID(N'dbo.BTRPD_InventoryRiskAging', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_InventoryRiskAging
(
    InventoryRiskAgingId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAging_InventoryRiskAgingId DEFAULT(''),
    SnapshotKey          VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAging_SnapshotKey DEFAULT('CURRENT'),
    BucketKey            VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAging_BucketKey DEFAULT(''),
    BucketLabel          VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAging_BucketLabel DEFAULT(''),
    InventoryValue       DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAging_InventoryValue DEFAULT(0),
    ItemCount            INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAging_ItemCount DEFAULT(0),
    SortOrder            INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAging_SortOrder DEFAULT(0),

    CONSTRAINT PK_BTRPD_InventoryRiskAging PRIMARY KEY CLUSTERED (InventoryRiskAgingId)
)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_BTRPD_InventoryRiskAging_SnapshotKey_BucketKey' AND object_id = OBJECT_ID(N'dbo.BTRPD_InventoryRiskAging'))
CREATE UNIQUE INDEX UX_BTRPD_InventoryRiskAging_SnapshotKey_BucketKey
    ON BTRPD_InventoryRiskAging (SnapshotKey, BucketKey)
GO

-- BTRPD_InventoryRiskAttention
IF OBJECT_ID(N'dbo.BTRPD_InventoryRiskAttention', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_InventoryRiskAttention
(
    InventoryRiskAttentionId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAttention_InventoryRiskAttentionId DEFAULT(''),
    SnapshotKey              VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAttention_SnapshotKey DEFAULT('CURRENT'),
    BrgId                    VARCHAR(13)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAttention_BrgId DEFAULT(''),
    BrgCode                  VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAttention_BrgCode DEFAULT(''),
    BrgName                  VARCHAR(100)  NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAttention_BrgName DEFAULT(''),
    KategoriName             VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAttention_KategoriName DEFAULT(''),
    SupplierName             VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAttention_SupplierName DEFAULT(''),
    Qty                      INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAttention_Qty DEFAULT(0),
    InventoryValue           DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAttention_InventoryValue DEFAULT(0),
    DaysSinceLastFaktur      INT           NULL,
    SignalKey                VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAttention_SignalKey DEFAULT(''),
    SignalLabel              VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAttention_SignalLabel DEFAULT(''),
    SortOrder                INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAttention_SortOrder DEFAULT(0),

    CONSTRAINT PK_BTRPD_InventoryRiskAttention PRIMARY KEY CLUSTERED (InventoryRiskAttentionId)
)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_InventoryRiskAttention_SnapshotKey_SortOrder' AND object_id = OBJECT_ID(N'dbo.BTRPD_InventoryRiskAttention'))
CREATE INDEX IX_BTRPD_InventoryRiskAttention_SnapshotKey_SortOrder
    ON BTRPD_InventoryRiskAttention (SnapshotKey, SortOrder)
GO

-- BTRPD_InventoryRiskTopDead
IF OBJECT_ID(N'dbo.BTRPD_InventoryRiskTopDead', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_InventoryRiskTopDead
(
    InventoryRiskTopDeadId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopDead_InventoryRiskTopDeadId DEFAULT(''),
    SnapshotKey            VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopDead_SnapshotKey DEFAULT('CURRENT'),
    Rank                   INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopDead_Rank DEFAULT(0),
    BrgId                  VARCHAR(13)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopDead_BrgId DEFAULT(''),
    BrgCode                VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopDead_BrgCode DEFAULT(''),
    BrgName                VARCHAR(100)  NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopDead_BrgName DEFAULT(''),
    KategoriName           VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopDead_KategoriName DEFAULT(''),
    SupplierName           VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopDead_SupplierName DEFAULT(''),
    Qty                    INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopDead_Qty DEFAULT(0),
    InventoryValue         DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopDead_InventoryValue DEFAULT(0),
    DaysSinceLastFaktur    INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopDead_DaysSinceLastFaktur DEFAULT(0),
    PercentOfAtRisk        DECIMAL(9,4)  NULL,

    CONSTRAINT PK_BTRPD_InventoryRiskTopDead PRIMARY KEY CLUSTERED (InventoryRiskTopDeadId)
)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_BTRPD_InventoryRiskTopDead_SnapshotKey_Rank' AND object_id = OBJECT_ID(N'dbo.BTRPD_InventoryRiskTopDead'))
CREATE UNIQUE INDEX UX_BTRPD_InventoryRiskTopDead_SnapshotKey_Rank
    ON BTRPD_InventoryRiskTopDead (SnapshotKey, Rank)
GO

-- BTRPD_InventoryRiskTopSlow
IF OBJECT_ID(N'dbo.BTRPD_InventoryRiskTopSlow', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_InventoryRiskTopSlow
(
    InventoryRiskTopSlowId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopSlow_InventoryRiskTopSlowId DEFAULT(''),
    SnapshotKey            VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopSlow_SnapshotKey DEFAULT('CURRENT'),
    Rank                   INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopSlow_Rank DEFAULT(0),
    BrgId                  VARCHAR(13)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopSlow_BrgId DEFAULT(''),
    BrgCode                VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopSlow_BrgCode DEFAULT(''),
    BrgName                VARCHAR(100)  NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopSlow_BrgName DEFAULT(''),
    KategoriName           VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopSlow_KategoriName DEFAULT(''),
    SupplierName           VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopSlow_SupplierName DEFAULT(''),
    Qty                    INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopSlow_Qty DEFAULT(0),
    InventoryValue         DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopSlow_InventoryValue DEFAULT(0),
    DaysSinceLastFaktur    INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopSlow_DaysSinceLastFaktur DEFAULT(0),
    PercentOfAtRisk        DECIMAL(9,4)  NULL,

    CONSTRAINT PK_BTRPD_InventoryRiskTopSlow PRIMARY KEY CLUSTERED (InventoryRiskTopSlowId)
)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_BTRPD_InventoryRiskTopSlow_SnapshotKey_Rank' AND object_id = OBJECT_ID(N'dbo.BTRPD_InventoryRiskTopSlow'))
CREATE UNIQUE INDEX UX_BTRPD_InventoryRiskTopSlow_SnapshotKey_Rank
    ON BTRPD_InventoryRiskTopSlow (SnapshotKey, Rank)
GO

-- BTRPD_InventoryRiskBreakdown
IF OBJECT_ID(N'dbo.BTRPD_InventoryRiskBreakdown', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_InventoryRiskBreakdown
(
    InventoryRiskBreakdownId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskBreakdown_InventoryRiskBreakdownId DEFAULT(''),
    SnapshotKey              VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskBreakdown_SnapshotKey DEFAULT('CURRENT'),
    DimensionType            VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskBreakdown_DimensionType DEFAULT(''),
    Name                     VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskBreakdown_Name DEFAULT(''),
    AtRiskValue              DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskBreakdown_AtRiskValue DEFAULT(0),
    ItemCount                INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskBreakdown_ItemCount DEFAULT(0),
    Rank                     INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskBreakdown_Rank DEFAULT(0),
    PercentOfAtRisk          DECIMAL(9,4)  NULL,

    CONSTRAINT PK_BTRPD_InventoryRiskBreakdown PRIMARY KEY CLUSTERED (InventoryRiskBreakdownId)
)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_BTRPD_InventoryRiskBreakdown_SnapshotKey_DimensionType_Rank' AND object_id = OBJECT_ID(N'dbo.BTRPD_InventoryRiskBreakdown'))
CREATE UNIQUE INDEX UX_BTRPD_InventoryRiskBreakdown_SnapshotKey_DimensionType_Rank
    ON BTRPD_InventoryRiskBreakdown (SnapshotKey, DimensionType, Rank)
GO

-- BTRPD_SalesKpi
IF OBJECT_ID(N'dbo.BTRPD_SalesKpi', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_SalesKpi
(
    SnapshotKey        VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_SalesKpi_SnapshotKey DEFAULT('CURRENT'),
    GeneratedAt        DATETIME       NOT NULL CONSTRAINT DF_BTRPD_SalesKpi_GeneratedAt DEFAULT('3000-01-01'),
    PeriodYear         INT            NOT NULL CONSTRAINT DF_BTRPD_SalesKpi_PeriodYear DEFAULT(0),
    PeriodMonth        INT            NOT NULL CONSTRAINT DF_BTRPD_SalesKpi_PeriodMonth DEFAULT(0),
    TotalOmzet         DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_SalesKpi_TotalOmzet DEFAULT(0),
    TotalFaktur        INT            NOT NULL CONSTRAINT DF_BTRPD_SalesKpi_TotalFaktur DEFAULT(0),
    TotalCustomer      INT            NOT NULL CONSTRAINT DF_BTRPD_SalesKpi_TotalCustomer DEFAULT(0),
    TotalTarget        DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_SalesKpi_TotalTarget DEFAULT(0),
    TotalAchievement   DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_SalesKpi_TotalAchievement DEFAULT(0),
    AchievementPercent DECIMAL(9,4)   NULL,
    CompletedOmzet     DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_SalesKpi_CompletedOmzet DEFAULT(0),
    PipelineOmzet      DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_SalesKpi_PipelineOmzet DEFAULT(0),
    LastRefreshLogId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_SalesKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_SalesKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
END
GO

-- BTRPD_SalesWeekTrend
IF OBJECT_ID(N'dbo.BTRPD_SalesWeekTrend', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_SalesWeekTrend
(
    SalesWeekTrendId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_SalesWeekTrend_SalesWeekTrendId DEFAULT(''),
    SnapshotKey      VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_SalesWeekTrend_SnapshotKey DEFAULT('CURRENT'),
    WeekStart        DATETIME      NOT NULL CONSTRAINT DF_BTRPD_SalesWeekTrend_WeekStart DEFAULT('3000-01-01'),
    WeekEnd          DATETIME      NOT NULL CONSTRAINT DF_BTRPD_SalesWeekTrend_WeekEnd DEFAULT('3000-01-01'),
    WeekLabel        VARCHAR(30)   NOT NULL CONSTRAINT DF_BTRPD_SalesWeekTrend_WeekLabel DEFAULT(''),
    RecognizedAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_SalesWeekTrend_RecognizedAmount DEFAULT(0),

    CONSTRAINT PK_BTRPD_SalesWeekTrend PRIMARY KEY CLUSTERED (SalesWeekTrendId)
)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_SalesWeekTrend_SnapshotKey_WeekStart' AND object_id = OBJECT_ID(N'dbo.BTRPD_SalesWeekTrend'))
CREATE INDEX IX_BTRPD_SalesWeekTrend_SnapshotKey_WeekStart
    ON BTRPD_SalesWeekTrend (SnapshotKey, WeekStart)
GO

-- BTRPD_SalesTopSalesman
IF OBJECT_ID(N'dbo.BTRPD_SalesTopSalesman', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_SalesTopSalesman
(
    SalesTopSalesmanId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_SalesTopSalesman_SalesTopSalesmanId DEFAULT(''),
    SnapshotKey        VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_SalesTopSalesman_SnapshotKey DEFAULT('CURRENT'),
    Rank               INT           NOT NULL CONSTRAINT DF_BTRPD_SalesTopSalesman_Rank DEFAULT(0),
    SalesPersonName    VARCHAR(30)   NOT NULL CONSTRAINT DF_BTRPD_SalesTopSalesman_SalesPersonName DEFAULT(''),
    CompletedOmzet     DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_SalesTopSalesman_CompletedOmzet DEFAULT(0),

    CONSTRAINT PK_BTRPD_SalesTopSalesman PRIMARY KEY CLUSTERED (SalesTopSalesmanId),
    CONSTRAINT UX_BTRPD_SalesTopSalesman_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)
)
END
GO

-- BTRPD_SalesForecastKpi
IF OBJECT_ID(N'dbo.BTRPD_SalesForecastKpi', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_SalesForecastKpi
(
    SnapshotKey                 VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_SnapshotKey DEFAULT('CURRENT'),
    GeneratedAt                 DATETIME       NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_GeneratedAt DEFAULT('3000-01-01'),
    PeriodYear                  INT            NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_PeriodYear DEFAULT(0),
    PeriodMonth                 INT            NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_PeriodMonth DEFAULT(0),
    BusinessDate                DATETIME       NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_BusinessDate DEFAULT('3000-01-01'),
    DaysInMonth                 INT            NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_DaysInMonth DEFAULT(0),
    DaysElapsed                 INT            NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_DaysElapsed DEFAULT(0),
    DaysRemaining               INT            NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_DaysRemaining DEFAULT(0),
    CurrentSales                DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_CurrentSales DEFAULT(0),
    TotalTarget                 DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_TotalTarget DEFAULT(0),
    CurrentAchievementPercent   DECIMAL(9,4)   NULL,
    DailyAverageSales           DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_DailyAverageSales DEFAULT(0),
    ForecastSales               DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_ForecastSales DEFAULT(0),
    ForecastAchievementPercent  DECIMAL(9,4)   NULL,
    RequiredDailySales          DECIMAL(18,2)  NULL,
    TargetGap                   DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_TargetGap DEFAULT(0),
    ForecastVariance            DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_ForecastVariance DEFAULT(0),
    BestCaseSales               DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_BestCaseSales DEFAULT(0),
    WorstCaseSales              DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_WorstCaseSales DEFAULT(0),
    ForecastConfidence          VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_ForecastConfidence DEFAULT(''),
    ForecastRiskBand              VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_ForecastRiskBand DEFAULT(''),
    LastRefreshLogId            VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_SalesForecastKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
END
GO

-- BTRPD_SalesDailyPace
IF OBJECT_ID(N'dbo.BTRPD_SalesDailyPace', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_SalesDailyPace
(
    SalesDailyPaceId     VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_SalesDailyPace_SalesDailyPaceId DEFAULT(''),
    SnapshotKey          VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_SalesDailyPace_SnapshotKey DEFAULT('CURRENT'),
    PaceDate             DATETIME      NOT NULL CONSTRAINT DF_BTRPD_SalesDailyPace_PaceDate DEFAULT('3000-01-01'),
    DayOfMonth           INT           NOT NULL CONSTRAINT DF_BTRPD_SalesDailyPace_DayOfMonth DEFAULT(0),
    IsElapsed            BIT           NOT NULL CONSTRAINT DF_BTRPD_SalesDailyPace_IsElapsed DEFAULT(0),
    ActualAmount         DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_SalesDailyPace_ActualAmount DEFAULT(0),
    ProjectedDailyAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_SalesDailyPace_ProjectedDailyAmount DEFAULT(0),

    CONSTRAINT PK_BTRPD_SalesDailyPace PRIMARY KEY CLUSTERED (SalesDailyPaceId)
)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_SalesDailyPace_SnapshotKey_PaceDate' AND object_id = OBJECT_ID(N'dbo.BTRPD_SalesDailyPace'))
CREATE INDEX IX_BTRPD_SalesDailyPace_SnapshotKey_PaceDate
    ON BTRPD_SalesDailyPace (SnapshotKey, PaceDate)
GO

-- BTRPD_CashFlowForecastKpi
IF OBJECT_ID(N'dbo.BTRPD_CashFlowForecastKpi', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_CashFlowForecastKpi
(
    SnapshotKey                         VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_CashFlowForecastKpi_SnapshotKey DEFAULT('CURRENT'),
    GeneratedAt                         DATETIME       NOT NULL CONSTRAINT DF_BTRPD_CashFlowForecastKpi_GeneratedAt DEFAULT('3000-01-01'),
    PeriodYear                          INT            NOT NULL CONSTRAINT DF_BTRPD_CashFlowForecastKpi_PeriodYear DEFAULT(0),
    PeriodMonth                         INT            NOT NULL CONSTRAINT DF_BTRPD_CashFlowForecastKpi_PeriodMonth DEFAULT(0),
    BusinessDate                        DATETIME       NOT NULL CONSTRAINT DF_BTRPD_CashFlowForecastKpi_BusinessDate DEFAULT('3000-01-01'),
    DaysInMonth                         INT            NOT NULL CONSTRAINT DF_BTRPD_CashFlowForecastKpi_DaysInMonth DEFAULT(0),
    DaysElapsed                         INT            NOT NULL CONSTRAINT DF_BTRPD_CashFlowForecastKpi_DaysElapsed DEFAULT(0),
    DaysRemaining                       INT            NOT NULL CONSTRAINT DF_BTRPD_CashFlowForecastKpi_DaysRemaining DEFAULT(0),
    CashCollectedMtd                    DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CashFlowForecastKpi_CashCollectedMtd DEFAULT(0),
    MonthCollections                    DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CashFlowForecastKpi_MonthCollections DEFAULT(0),
    MonthFakturOmzet                    DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CashFlowForecastKpi_MonthFakturOmzet DEFAULT(0),
    DailyCashCollectionAverage          DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CashFlowForecastKpi_DailyCashCollectionAverage DEFAULT(0),
    DailyCollectionAverage              DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CashFlowForecastKpi_DailyCollectionAverage DEFAULT(0),
    ExpectedCashCollection              DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CashFlowForecastKpi_ExpectedCashCollection DEFAULT(0),
    ProjectedMonthEndTotalCollections   DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CashFlowForecastKpi_ProjectedMonthEndTotalCollections DEFAULT(0),
    CollectionForecastPercent           DECIMAL(9,4)   NULL,
    RecoveryVsBillingPercent            DECIMAL(9,4)   NULL,
    RecoveryVsBillingForecastPercent    DECIMAL(9,4)   NULL,
    RemainingCollectionTarget           DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CashFlowForecastKpi_RemainingCollectionTarget DEFAULT(0),
    RequiredDailyCollection             DECIMAL(18,2)  NULL,
    OutstandingDueRemaining             DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CashFlowForecastKpi_OutstandingDueRemaining DEFAULT(0),
    OverdueOutstanding                  DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CashFlowForecastKpi_OverdueOutstanding DEFAULT(0),
    CollectionGap                       DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CashFlowForecastKpi_CollectionGap DEFAULT(0),
    ForecastVarianceCash                DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CashFlowForecastKpi_ForecastVarianceCash DEFAULT(0),
    ExpectedCollectionRatePercent       DECIMAL(9,4)   NULL,
    BestCaseCash                        DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CashFlowForecastKpi_BestCaseCash DEFAULT(0),
    WorstCaseCash                       DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CashFlowForecastKpi_WorstCaseCash DEFAULT(0),
    ForecastConfidence                  VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_CashFlowForecastKpi_ForecastConfidence DEFAULT(''),
    ForecastRiskBand                    VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_CashFlowForecastKpi_ForecastRiskBand DEFAULT(''),
    LastRefreshLogId                    VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_CashFlowForecastKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_CashFlowForecastKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
END
GO

-- BTRPD_CashFlowDailyPace
IF OBJECT_ID(N'dbo.BTRPD_CashFlowDailyPace', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_CashFlowDailyPace
(
    CashFlowDailyPaceId      VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_CashFlowDailyPace_CashFlowDailyPaceId DEFAULT(''),
    SnapshotKey              VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_CashFlowDailyPace_SnapshotKey DEFAULT('CURRENT'),
    PaceDate                 DATETIME      NOT NULL CONSTRAINT DF_BTRPD_CashFlowDailyPace_PaceDate DEFAULT('3000-01-01'),
    DayOfMonth               INT           NOT NULL CONSTRAINT DF_BTRPD_CashFlowDailyPace_DayOfMonth DEFAULT(0),
    IsElapsed                BIT           NOT NULL CONSTRAINT DF_BTRPD_CashFlowDailyPace_IsElapsed DEFAULT(0),
    ActualCashAmount         DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CashFlowDailyPace_ActualCashAmount DEFAULT(0),
    ActualCollectionAmount   DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CashFlowDailyPace_ActualCollectionAmount DEFAULT(0),
    ProjectedDailyCashAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CashFlowDailyPace_ProjectedDailyCashAmount DEFAULT(0),

    CONSTRAINT PK_BTRPD_CashFlowDailyPace PRIMARY KEY CLUSTERED (CashFlowDailyPaceId)
)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_CashFlowDailyPace_SnapshotKey_PaceDate' AND object_id = OBJECT_ID(N'dbo.BTRPD_CashFlowDailyPace'))
CREATE INDEX IX_BTRPD_CashFlowDailyPace_SnapshotKey_PaceDate
    ON BTRPD_CashFlowDailyPace (SnapshotKey, PaceDate)
GO

-- BTRPD_CashFlowRecoveryTrend
IF OBJECT_ID(N'dbo.BTRPD_CashFlowRecoveryTrend', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_CashFlowRecoveryTrend
(
    CashFlowRecoveryTrendId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_CashFlowRecoveryTrend_CashFlowRecoveryTrendId DEFAULT(''),
    SnapshotKey             VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_CashFlowRecoveryTrend_SnapshotKey DEFAULT('CURRENT'),
    TrendDate               DATETIME      NOT NULL CONSTRAINT DF_BTRPD_CashFlowRecoveryTrend_TrendDate DEFAULT('3000-01-01'),
    DayOfMonth              INT           NOT NULL CONSTRAINT DF_BTRPD_CashFlowRecoveryTrend_DayOfMonth DEFAULT(0),
    IsElapsed               BIT           NOT NULL CONSTRAINT DF_BTRPD_CashFlowRecoveryTrend_IsElapsed DEFAULT(0),
    CumulativeCollections   DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CashFlowRecoveryTrend_CumulativeCollections DEFAULT(0),
    CumulativeBilling       DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CashFlowRecoveryTrend_CumulativeBilling DEFAULT(0),

    CONSTRAINT PK_BTRPD_CashFlowRecoveryTrend PRIMARY KEY CLUSTERED (CashFlowRecoveryTrendId)
)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_CashFlowRecoveryTrend_SnapshotKey_TrendDate' AND object_id = OBJECT_ID(N'dbo.BTRPD_CashFlowRecoveryTrend'))
CREATE INDEX IX_BTRPD_CashFlowRecoveryTrend_SnapshotKey_TrendDate
    ON BTRPD_CashFlowRecoveryTrend (SnapshotKey, TrendDate)
GO

-- BTRPD_CashFlowCollectionRisk
IF OBJECT_ID(N'dbo.BTRPD_CashFlowCollectionRisk', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_CashFlowCollectionRisk
(
    CashFlowCollectionRiskId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_CashFlowCollectionRisk_CashFlowCollectionRiskId DEFAULT(''),
    SnapshotKey              VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_CashFlowCollectionRisk_SnapshotKey DEFAULT('CURRENT'),
    SortOrder                INT            NOT NULL CONSTRAINT DF_BTRPD_CashFlowCollectionRisk_SortOrder DEFAULT(0),
    RiskKey                  VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CashFlowCollectionRisk_RiskKey DEFAULT(''),
    RiskLabel                VARCHAR(60)    NOT NULL CONSTRAINT DF_BTRPD_CashFlowCollectionRisk_RiskLabel DEFAULT(''),
    EntityType               VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_CashFlowCollectionRisk_EntityType DEFAULT(''),
    EntityId                 VARCHAR(13)    NOT NULL CONSTRAINT DF_BTRPD_CashFlowCollectionRisk_EntityId DEFAULT(''),
    EntityName               VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CashFlowCollectionRisk_EntityName DEFAULT(''),
    Amount                   DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CashFlowCollectionRisk_Amount DEFAULT(0),
    DueOrAgingText           VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_CashFlowCollectionRisk_DueOrAgingText DEFAULT(''),
    RuleExplanation          VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_CashFlowCollectionRisk_RuleExplanation DEFAULT(''),
    ReportRoute              VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CashFlowCollectionRisk_ReportRoute DEFAULT(''),

    CONSTRAINT PK_BTRPD_CashFlowCollectionRisk PRIMARY KEY CLUSTERED (CashFlowCollectionRiskId)
)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_CashFlowCollectionRisk_SnapshotKey_SortOrder' AND object_id = OBJECT_ID(N'dbo.BTRPD_CashFlowCollectionRisk'))
CREATE INDEX IX_BTRPD_CashFlowCollectionRisk_SnapshotKey_SortOrder
    ON BTRPD_CashFlowCollectionRisk (SnapshotKey, SortOrder)
GO

-- BTRPD_InventoryForecastKpi
IF OBJECT_ID(N'dbo.BTRPD_InventoryForecastKpi', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_InventoryForecastKpi
(
    SnapshotKey                     VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_SnapshotKey DEFAULT('CURRENT'),
    GeneratedAt                       DATETIME       NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_GeneratedAt DEFAULT('3000-01-01'),
    BusinessDate                      DATETIME       NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_BusinessDate DEFAULT('3000-01-01'),
    PlanningHorizonDays               INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_PlanningHorizonDays DEFAULT(0),
    CurrentInventoryValue             DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_CurrentInventoryValue DEFAULT(0),
    ProjectedInventoryValue           DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_ProjectedInventoryValue DEFAULT(0),
    BestCaseProjectedValue            DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_BestCaseProjectedValue DEFAULT(0),
    WorstCaseProjectedValue           DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_WorstCaseProjectedValue DEFAULT(0),
    AverageDailyConsumptionUnits      DECIMAL(18,4)  NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_AverageDailyConsumptionUnits DEFAULT(0),
    WeightedAverageDaysOfSupply       DECIMAL(18,2)  NULL,
    UnderstockValue                   DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_UnderstockValue DEFAULT(0),
    OverstockValue                    DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_OverstockValue DEFAULT(0),
    StockOutRiskItemCount             INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_StockOutRiskItemCount DEFAULT(0),
    InventoryCoveragePercent          DECIMAL(9,4)   NULL,
    InventoryTurnoverForecast         DECIMAL(9,4)   NULL,
    InventoryHealthScore              INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_InventoryHealthScore DEFAULT(0),
    ForecastConfidence                VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_ForecastConfidence DEFAULT(''),
    AtRiskInventoryPercent            DECIMAL(9,4)   NULL,
    ForecastConsumptionUnits          DECIMAL(18,4)  NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_ForecastConsumptionUnits DEFAULT(0),
    HeatCellLowLow                    INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_HeatCellLowLow DEFAULT(0),
    HeatCellLowMed                    INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_HeatCellLowMed DEFAULT(0),
    HeatCellLowHigh                   INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_HeatCellLowHigh DEFAULT(0),
    HeatCellMedLow                    INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_HeatCellMedLow DEFAULT(0),
    HeatCellMedMed                    INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_HeatCellMedMed DEFAULT(0),
    HeatCellMedHigh                   INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_HeatCellMedHigh DEFAULT(0),
    HeatCellHighLow                   INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_HeatCellHighLow DEFAULT(0),
    HeatCellHighMed                   INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_HeatCellHighMed DEFAULT(0),
    HeatCellHighHigh                  INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_HeatCellHighHigh DEFAULT(0),
    LastRefreshLogId                  VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_InventoryForecastKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
END
GO

-- BTRPD_InventoryForecastDailyConsumption
IF OBJECT_ID(N'dbo.BTRPD_InventoryForecastDailyConsumption', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_InventoryForecastDailyConsumption
(
    InventoryForecastDailyConsumptionId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastDailyConsumption_Id DEFAULT(''),
    SnapshotKey                         VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastDailyConsumption_SnapshotKey DEFAULT('CURRENT'),
    ConsumptionDate                     DATETIME      NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastDailyConsumption_ConsumptionDate DEFAULT('3000-01-01'),
    DayIndex                            INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastDailyConsumption_DayIndex DEFAULT(0),
    UnitsSold                           DECIMAL(18,4) NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastDailyConsumption_UnitsSold DEFAULT(0),
    AdcReference                        DECIMAL(18,4) NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastDailyConsumption_AdcReference DEFAULT(0),

    CONSTRAINT PK_BTRPD_InventoryForecastDailyConsumption PRIMARY KEY CLUSTERED (InventoryForecastDailyConsumptionId)
)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_InventoryForecastDailyConsumption_SnapshotKey_Date' AND object_id = OBJECT_ID(N'dbo.BTRPD_InventoryForecastDailyConsumption'))
CREATE INDEX IX_BTRPD_InventoryForecastDailyConsumption_SnapshotKey_Date
    ON BTRPD_InventoryForecastDailyConsumption (SnapshotKey, ConsumptionDate)
GO

-- BTRPD_InventoryForecastLevel
IF OBJECT_ID(N'dbo.BTRPD_InventoryForecastLevel', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_InventoryForecastLevel
(
    InventoryForecastLevelId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastLevel_Id DEFAULT(''),
    SnapshotKey              VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastLevel_SnapshotKey DEFAULT('CURRENT'),
    HorizonDay               INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastLevel_HorizonDay DEFAULT(0),
    ProjectedInventoryValue  DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastLevel_ProjectedInventoryValue DEFAULT(0),

    CONSTRAINT PK_BTRPD_InventoryForecastLevel PRIMARY KEY CLUSTERED (InventoryForecastLevelId)
)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_InventoryForecastLevel_SnapshotKey_Day' AND object_id = OBJECT_ID(N'dbo.BTRPD_InventoryForecastLevel'))
CREATE INDEX IX_BTRPD_InventoryForecastLevel_SnapshotKey_Day
    ON BTRPD_InventoryForecastLevel (SnapshotKey, HorizonDay)
GO

-- BTRPD_InventoryForecastRisk
IF OBJECT_ID(N'dbo.BTRPD_InventoryForecastRisk', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_InventoryForecastRisk
(
    InventoryForecastRiskId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRisk_Id DEFAULT(''),
    SnapshotKey             VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRisk_SnapshotKey DEFAULT('CURRENT'),
    SortOrder               INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRisk_SortOrder DEFAULT(0),
    SignalKey               VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRisk_SignalKey DEFAULT(''),
    SignalLabel             VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRisk_SignalLabel DEFAULT(''),
    BrgId                   VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRisk_BrgId DEFAULT(''),
    BrgCode                 VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRisk_BrgCode DEFAULT(''),
    BrgName                 VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRisk_BrgName DEFAULT(''),
    SupplierName            VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRisk_SupplierName DEFAULT(''),
    DaysOfSupply            DECIMAL(18,2)  NULL,
    StockOutDate            DATETIME       NULL,
    ValueAmount             DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRisk_ValueAmount DEFAULT(0),
    Urgency                 VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRisk_Urgency DEFAULT(''),
    RuleExplanation         VARCHAR(500)   NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRisk_RuleExplanation DEFAULT(''),
    ReportRoute             VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRisk_ReportRoute DEFAULT(''),
    EntityCode              VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRisk_EntityCode DEFAULT(''),

    CONSTRAINT PK_BTRPD_InventoryForecastRisk PRIMARY KEY CLUSTERED (InventoryForecastRiskId)
)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_InventoryForecastRisk_SnapshotKey_SortOrder' AND object_id = OBJECT_ID(N'dbo.BTRPD_InventoryForecastRisk'))
CREATE INDEX IX_BTRPD_InventoryForecastRisk_SnapshotKey_SortOrder
    ON BTRPD_InventoryForecastRisk (SnapshotKey, SortOrder)
GO

-- BTRPD_InventoryForecastRecommendation
IF OBJECT_ID(N'dbo.BTRPD_InventoryForecastRecommendation', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_InventoryForecastRecommendation
(
    InventoryForecastRecommendationId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRecommendation_Id DEFAULT(''),
    SnapshotKey                       VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRecommendation_SnapshotKey DEFAULT('CURRENT'),
    SortOrder                         INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRecommendation_SortOrder DEFAULT(0),
    BrgId                             VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRecommendation_BrgId DEFAULT(''),
    BrgCode                           VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRecommendation_BrgCode DEFAULT(''),
    BrgName                           VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRecommendation_BrgName DEFAULT(''),
    SupplierName                      VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRecommendation_SupplierName DEFAULT(''),
    ReorderDate                       DATETIME       NULL,
    RecommendedPurchaseQty            DECIMAL(18,4)  NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRecommendation_RecommendedPurchaseQty DEFAULT(0),
    AverageDailyConsumption           DECIMAL(18,4)  NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRecommendation_AverageDailyConsumption DEFAULT(0),
    CurrentQty                        DECIMAL(18,4)  NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRecommendation_CurrentQty DEFAULT(0),
    DaysOfSupply                      DECIMAL(18,2)  NULL,
    Urgency                           VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRecommendation_Urgency DEFAULT(''),
    ReportRoute                       VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRecommendation_ReportRoute DEFAULT(''),
    EntityCode                        VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRecommendation_EntityCode DEFAULT(''),

    CONSTRAINT PK_BTRPD_InventoryForecastRecommendation PRIMARY KEY CLUSTERED (InventoryForecastRecommendationId)
)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_InventoryForecastRecommendation_SnapshotKey_SortOrder' AND object_id = OBJECT_ID(N'dbo.BTRPD_InventoryForecastRecommendation'))
CREATE INDEX IX_BTRPD_InventoryForecastRecommendation_SnapshotKey_SortOrder
    ON BTRPD_InventoryForecastRecommendation (SnapshotKey, SortOrder)
GO

-- BTRPD_InventoryOptimizationKpi
IF OBJECT_ID(N'dbo.BTRPD_InventoryOptimizationKpi', N'U') IS NULL
BEGIN
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
END
GO

-- BTRPD_InventoryOptimizationAction
IF OBJECT_ID(N'dbo.BTRPD_InventoryOptimizationAction', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_InventoryOptimizationAction
(
    InventoryOptimizationActionId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationAction_Id DEFAULT(''),
    SnapshotKey                   VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationAction_SnapshotKey DEFAULT('CURRENT'),
    SortOrder                     INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationAction_SortOrder DEFAULT(0),
    PriorityScore                 INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationAction_PriorityScore DEFAULT(0),
    Category                      VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationAction_Category DEFAULT(''),
    ActionType                    VARCHAR(40)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationAction_ActionType DEFAULT(''),
    ActionLabel                   VARCHAR(80)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationAction_ActionLabel DEFAULT(''),
    BrgId                         VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationAction_BrgId DEFAULT(''),
    BrgName                       VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationAction_BrgName DEFAULT(''),
    SupplierName                  VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationAction_SupplierName DEFAULT(''),
    WarehouseFromId               VARCHAR(5)     NULL,
    WarehouseFromName             VARCHAR(50)    NULL,
    WarehouseToId                 VARCHAR(5)     NULL,
    WarehouseToName               VARCHAR(50)    NULL,
    Quantity                      DECIMAL(18,4)  NULL,
    ImpactValueIdr                DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationAction_ImpactValueIdr DEFAULT(0),
    DaysOfSupply                  DECIMAL(18,2)  NULL,
    ReasonText                    VARCHAR(500)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationAction_ReasonText DEFAULT(''),
    RuleId                        VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationAction_RuleId DEFAULT(''),
    ReportRoute                   VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationAction_ReportRoute DEFAULT(''),
    DrillDownRoute                  VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationAction_DrillDownRoute DEFAULT(''),

    CONSTRAINT PK_BTRPD_InventoryOptimizationAction PRIMARY KEY CLUSTERED (InventoryOptimizationActionId)
)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_InventoryOptimizationAction_SnapshotKey_SortOrder' AND object_id = OBJECT_ID(N'dbo.BTRPD_InventoryOptimizationAction'))
CREATE INDEX IX_BTRPD_InventoryOptimizationAction_SnapshotKey_SortOrder
    ON BTRPD_InventoryOptimizationAction (SnapshotKey, SortOrder)
GO

-- BTRPD_InventoryOptimizationReorder
IF OBJECT_ID(N'dbo.BTRPD_InventoryOptimizationReorder', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_InventoryOptimizationReorder
(
    InventoryOptimizationReorderId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationReorder_Id DEFAULT(''),
    SnapshotKey                    VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationReorder_SnapshotKey DEFAULT('CURRENT'),
    SortOrder                      INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationReorder_SortOrder DEFAULT(0),
    PriorityScore                  INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationReorder_PriorityScore DEFAULT(0),
    Category                       VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationReorder_Category DEFAULT(''),
    BrgId                          VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationReorder_BrgId DEFAULT(''),
    BrgCode                        VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationReorder_BrgCode DEFAULT(''),
    BrgName                        VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationReorder_BrgName DEFAULT(''),
    SupplierName                   VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationReorder_SupplierName DEFAULT(''),
    RecommendedPurchaseQty         DECIMAL(18,4)  NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationReorder_RecommendedPurchaseQty DEFAULT(0),
    EstimatedCostIdr               DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationReorder_EstimatedCostIdr DEFAULT(0),
    DaysOfSupply                   DECIMAL(18,2)  NULL,
    ReorderDate                    DATETIME       NULL,
    AverageDailyConsumption        DECIMAL(18,4)  NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationReorder_AverageDailyConsumption DEFAULT(0),
    CurrentQty                     DECIMAL(18,4)  NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationReorder_CurrentQty DEFAULT(0),
    ReasonText                     VARCHAR(500)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationReorder_ReasonText DEFAULT(''),
    RuleId                         VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationReorder_RuleId DEFAULT(''),
    ReportRoute                    VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationReorder_ReportRoute DEFAULT(''),
    DrillDownRoute                 VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationReorder_DrillDownRoute DEFAULT(''),

    CONSTRAINT PK_BTRPD_InventoryOptimizationReorder PRIMARY KEY CLUSTERED (InventoryOptimizationReorderId)
)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_InventoryOptimizationReorder_SnapshotKey_SortOrder' AND object_id = OBJECT_ID(N'dbo.BTRPD_InventoryOptimizationReorder'))
CREATE INDEX IX_BTRPD_InventoryOptimizationReorder_SnapshotKey_SortOrder
    ON BTRPD_InventoryOptimizationReorder (SnapshotKey, SortOrder)
GO

-- BTRPD_InventoryOptimizationTransfer
IF OBJECT_ID(N'dbo.BTRPD_InventoryOptimizationTransfer', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_InventoryOptimizationTransfer
(
    InventoryOptimizationTransferId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationTransfer_Id DEFAULT(''),
    SnapshotKey                     VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationTransfer_SnapshotKey DEFAULT('CURRENT'),
    SortOrder                       INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationTransfer_SortOrder DEFAULT(0),
    PriorityScore                   INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationTransfer_PriorityScore DEFAULT(0),
    Category                        VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationTransfer_Category DEFAULT(''),
    BrgId                           VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationTransfer_BrgId DEFAULT(''),
    BrgName                         VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationTransfer_BrgName DEFAULT(''),
    WarehouseFromId                 VARCHAR(5)     NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationTransfer_WarehouseFromId DEFAULT(''),
    WarehouseFromName               VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationTransfer_WarehouseFromName DEFAULT(''),
    WarehouseToId                   VARCHAR(5)     NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationTransfer_WarehouseToId DEFAULT(''),
    WarehouseToName                 VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationTransfer_WarehouseToName DEFAULT(''),
    TransferQty                     DECIMAL(18,4)  NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationTransfer_TransferQty DEFAULT(0),
    DestDaysOfSupply                DECIMAL(18,2)  NULL,
    ReasonText                      VARCHAR(500)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationTransfer_ReasonText DEFAULT(''),
    RuleId                          VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationTransfer_RuleId DEFAULT(''),
    ReportRoute                     VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationTransfer_ReportRoute DEFAULT(''),
    DrillDownRoute                  VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationTransfer_DrillDownRoute DEFAULT(''),

    CONSTRAINT PK_BTRPD_InventoryOptimizationTransfer PRIMARY KEY CLUSTERED (InventoryOptimizationTransferId)
)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_InventoryOptimizationTransfer_SnapshotKey_SortOrder' AND object_id = OBJECT_ID(N'dbo.BTRPD_InventoryOptimizationTransfer'))
CREATE INDEX IX_BTRPD_InventoryOptimizationTransfer_SnapshotKey_SortOrder
    ON BTRPD_InventoryOptimizationTransfer (SnapshotKey, SortOrder)
GO

-- BTRPD_InventoryOptimizationDelay
IF OBJECT_ID(N'dbo.BTRPD_InventoryOptimizationDelay', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_InventoryOptimizationDelay
(
    InventoryOptimizationDelayId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationDelay_Id DEFAULT(''),
    SnapshotKey                  VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationDelay_SnapshotKey DEFAULT('CURRENT'),
    SortOrder                    INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationDelay_SortOrder DEFAULT(0),
    PriorityScore                INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationDelay_PriorityScore DEFAULT(0),
    Category                     VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationDelay_Category DEFAULT(''),
    ActionType                   VARCHAR(40)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationDelay_ActionType DEFAULT(''),
    ActionLabel                  VARCHAR(80)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationDelay_ActionLabel DEFAULT(''),
    BrgId                        VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationDelay_BrgId DEFAULT(''),
    BrgName                      VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationDelay_BrgName DEFAULT(''),
    SupplierName                 VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationDelay_SupplierName DEFAULT(''),
    DaysOfSupply                 DECIMAL(18,2)  NULL,
    MovementClass                VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationDelay_MovementClass DEFAULT(''),
    SuggestedQty                 DECIMAL(18,4)  NULL,
    ReasonText                   VARCHAR(500)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationDelay_ReasonText DEFAULT(''),
    RuleId                       VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationDelay_RuleId DEFAULT(''),
    ReportRoute                  VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationDelay_ReportRoute DEFAULT(''),
    DrillDownRoute               VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationDelay_DrillDownRoute DEFAULT(''),

    CONSTRAINT PK_BTRPD_InventoryOptimizationDelay PRIMARY KEY CLUSTERED (InventoryOptimizationDelayId)
)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_InventoryOptimizationDelay_SnapshotKey_SortOrder' AND object_id = OBJECT_ID(N'dbo.BTRPD_InventoryOptimizationDelay'))
CREATE INDEX IX_BTRPD_InventoryOptimizationDelay_SnapshotKey_SortOrder
    ON BTRPD_InventoryOptimizationDelay (SnapshotKey, SortOrder)
GO

-- BTRPD_InventoryOptimizationClearance
IF OBJECT_ID(N'dbo.BTRPD_InventoryOptimizationClearance', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_InventoryOptimizationClearance
(
    InventoryOptimizationClearanceId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationClearance_Id DEFAULT(''),
    SnapshotKey                      VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationClearance_SnapshotKey DEFAULT('CURRENT'),
    SortOrder                        INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationClearance_SortOrder DEFAULT(0),
    PriorityScore                    INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationClearance_PriorityScore DEFAULT(0),
    Category                         VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationClearance_Category DEFAULT(''),
    BrgId                            VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationClearance_BrgId DEFAULT(''),
    BrgName                          VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationClearance_BrgName DEFAULT(''),
    InventoryValueIdr                DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationClearance_InventoryValueIdr DEFAULT(0),
    IdleDays                         INT            NULL,
    RecommendedAction                VARCHAR(80)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationClearance_RecommendedAction DEFAULT(''),
    ReasonText                       VARCHAR(500)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationClearance_ReasonText DEFAULT(''),
    RuleId                           VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationClearance_RuleId DEFAULT(''),
    ReportRoute                      VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationClearance_ReportRoute DEFAULT(''),
    DrillDownRoute                   VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationClearance_DrillDownRoute DEFAULT(''),

    CONSTRAINT PK_BTRPD_InventoryOptimizationClearance PRIMARY KEY CLUSTERED (InventoryOptimizationClearanceId)
)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_InventoryOptimizationClearance_SnapshotKey_SortOrder' AND object_id = OBJECT_ID(N'dbo.BTRPD_InventoryOptimizationClearance'))
CREATE INDEX IX_BTRPD_InventoryOptimizationClearance_SnapshotKey_SortOrder
    ON BTRPD_InventoryOptimizationClearance (SnapshotKey, SortOrder)
GO

-- BTRPD_InventoryOptimizationPriorityDist
IF OBJECT_ID(N'dbo.BTRPD_InventoryOptimizationPriorityDist', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_InventoryOptimizationPriorityDist
(
    InventoryOptimizationPriorityDistId VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationPriorityDist_Id DEFAULT(''),
    SnapshotKey                         VARCHAR(10) NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationPriorityDist_SnapshotKey DEFAULT('CURRENT'),
    Category                            VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationPriorityDist_Category DEFAULT(''),
    ActionCount                         INT         NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationPriorityDist_ActionCount DEFAULT(0),
    SortOrder                           INT         NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationPriorityDist_SortOrder DEFAULT(0),

    CONSTRAINT PK_BTRPD_InventoryOptimizationPriorityDist PRIMARY KEY CLUSTERED (InventoryOptimizationPriorityDistId)
)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_InventoryOptimizationPriorityDist_SnapshotKey_SortOrder' AND object_id = OBJECT_ID(N'dbo.BTRPD_InventoryOptimizationPriorityDist'))
CREATE INDEX IX_BTRPD_InventoryOptimizationPriorityDist_SnapshotKey_SortOrder
    ON BTRPD_InventoryOptimizationPriorityDist (SnapshotKey, SortOrder)
GO

-- BTRPD_InventoryOptimizationActionHeat
IF OBJECT_ID(N'dbo.BTRPD_InventoryOptimizationActionHeat', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_InventoryOptimizationActionHeat
(
    InventoryOptimizationActionHeatId VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationActionHeat_Id DEFAULT(''),
    SnapshotKey                       VARCHAR(10) NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationActionHeat_SnapshotKey DEFAULT('CURRENT'),
    ActionType                        VARCHAR(40) NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationActionHeat_ActionType DEFAULT(''),
    ActionLabel                       VARCHAR(80) NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationActionHeat_ActionLabel DEFAULT(''),
    Category                          VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationActionHeat_Category DEFAULT(''),
    ActionCount                       INT         NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationActionHeat_ActionCount DEFAULT(0),

    CONSTRAINT PK_BTRPD_InventoryOptimizationActionHeat PRIMARY KEY CLUSTERED (InventoryOptimizationActionHeatId)
)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_InventoryOptimizationActionHeat_SnapshotKey' AND object_id = OBJECT_ID(N'dbo.BTRPD_InventoryOptimizationActionHeat'))
CREATE INDEX IX_BTRPD_InventoryOptimizationActionHeat_SnapshotKey
    ON BTRPD_InventoryOptimizationActionHeat (SnapshotKey)
GO

-- BTRPD_PurchasingKpi
IF OBJECT_ID(N'dbo.BTRPD_PurchasingKpi', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_PurchasingKpi
(
    SnapshotKey                VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_PurchasingKpi_SnapshotKey DEFAULT('CURRENT'),
    GeneratedAt                DATETIME       NOT NULL CONSTRAINT DF_BTRPD_PurchasingKpi_GeneratedAt DEFAULT('3000-01-01'),
    PeriodYear                 INT            NOT NULL CONSTRAINT DF_BTRPD_PurchasingKpi_PeriodYear DEFAULT(0),
    PeriodMonth                INT            NOT NULL CONSTRAINT DF_BTRPD_PurchasingKpi_PeriodMonth DEFAULT(0),
    GrandTotalPurchase         DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_PurchasingKpi_GrandTotalPurchase DEFAULT(0),
    TotalInvoice               INT            NOT NULL CONSTRAINT DF_BTRPD_PurchasingKpi_TotalInvoice DEFAULT(0),
    PendingPostingInvoiceCount INT            NOT NULL CONSTRAINT DF_BTRPD_PurchasingKpi_PendingPostingInvoiceCount DEFAULT(0),
    LastRefreshLogId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_PurchasingKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PurchasingKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
END
GO

-- BTRPD_PurchasingWeekTrend
IF OBJECT_ID(N'dbo.BTRPD_PurchasingWeekTrend', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_PurchasingWeekTrend
(
    PurchasingWeekTrendId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingWeekTrend_PurchasingWeekTrendId DEFAULT(''),
    SnapshotKey           VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingWeekTrend_SnapshotKey DEFAULT('CURRENT'),
    WeekStart             DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PurchasingWeekTrend_WeekStart DEFAULT('3000-01-01'),
    WeekEnd               DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PurchasingWeekTrend_WeekEnd DEFAULT('3000-01-01'),
    WeekLabel             VARCHAR(30)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingWeekTrend_WeekLabel DEFAULT(''),
    PurchaseAmount        DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PurchasingWeekTrend_PurchaseAmount DEFAULT(0),

    CONSTRAINT PK_BTRPD_PurchasingWeekTrend PRIMARY KEY CLUSTERED (PurchasingWeekTrendId)
)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_PurchasingWeekTrend_SnapshotKey_WeekStart' AND object_id = OBJECT_ID(N'dbo.BTRPD_PurchasingWeekTrend'))
CREATE INDEX IX_BTRPD_PurchasingWeekTrend_SnapshotKey_WeekStart
    ON BTRPD_PurchasingWeekTrend (SnapshotKey, WeekStart)
GO

-- BTRPD_PurchasingPostingStatus
IF OBJECT_ID(N'dbo.BTRPD_PurchasingPostingStatus', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_PurchasingPostingStatus
(
    PurchasingPostingStatusId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingPostingStatus_PurchasingPostingStatusId DEFAULT(''),
    SnapshotKey               VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingPostingStatus_SnapshotKey DEFAULT('CURRENT'),
    StatusKey                 VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingPostingStatus_StatusKey DEFAULT(''),
    StatusLabel               VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingPostingStatus_StatusLabel DEFAULT(''),
    SortOrder                 INT           NOT NULL CONSTRAINT DF_BTRPD_PurchasingPostingStatus_SortOrder DEFAULT(0),
    PurchaseAmount            DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PurchasingPostingStatus_PurchaseAmount DEFAULT(0),

    CONSTRAINT PK_BTRPD_PurchasingPostingStatus PRIMARY KEY CLUSTERED (PurchasingPostingStatusId),
    CONSTRAINT UX_BTRPD_PurchasingPostingStatus_SnapshotKey_StatusKey UNIQUE (SnapshotKey, StatusKey)
)
END
GO

-- BTRPD_PurchasingTopPrincipal
IF OBJECT_ID(N'dbo.BTRPD_PurchasingTopPrincipal', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_PurchasingTopPrincipal
(
    PurchasingTopPrincipalId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingTopPrincipal_PurchasingTopPrincipalId DEFAULT(''),
    SnapshotKey              VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingTopPrincipal_SnapshotKey DEFAULT('CURRENT'),
    Rank                     INT           NOT NULL CONSTRAINT DF_BTRPD_PurchasingTopPrincipal_Rank DEFAULT(0),
    PrincipalName            VARCHAR(100)  NOT NULL CONSTRAINT DF_BTRPD_PurchasingTopPrincipal_PrincipalName DEFAULT(''),
    PurchaseAmount           DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PurchasingTopPrincipal_PurchaseAmount DEFAULT(0),

    CONSTRAINT PK_BTRPD_PurchasingTopPrincipal PRIMARY KEY CLUSTERED (PurchasingTopPrincipalId),
    CONSTRAINT UX_BTRPD_PurchasingTopPrincipal_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)
)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_PurchasingTopPrincipal_SnapshotKey_Rank' AND object_id = OBJECT_ID(N'dbo.BTRPD_PurchasingTopPrincipal'))
CREATE INDEX IX_BTRPD_PurchasingTopPrincipal_SnapshotKey_Rank
    ON BTRPD_PurchasingTopPrincipal (SnapshotKey, Rank)
GO

-- BTRPD_PurchasingManagementKpi
IF OBJECT_ID(N'dbo.BTRPD_PurchasingManagementKpi', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_PurchasingManagementKpi
(
    SnapshotKey                        VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementKpi_SnapshotKey DEFAULT('CURRENT'),
    GeneratedAt                        DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementKpi_GeneratedAt DEFAULT('3000-01-01'),
    PeriodYear                         INT           NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementKpi_PeriodYear DEFAULT(0),
    PeriodMonth                        INT           NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementKpi_PeriodMonth DEFAULT(0),
    QualifiedBacklogCount              INT           NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementKpi_QualifiedBacklogCount DEFAULT(0),
    QualifiedBacklogValue              DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementKpi_QualifiedBacklogValue DEFAULT(0),
    PendingPostingValue                DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementKpi_PendingPostingValue DEFAULT(0),
    PostedPercent                      DECIMAL(9,4)  NULL,
    Top1PrincipalPercent               DECIMAL(9,4)  NULL,
    Top3PrincipalPercent               DECIMAL(9,4)  NULL,
    Top1SupplierInventoryPercent       DECIMAL(9,4)  NULL,
    CompoundDependencyCount            INT           NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementKpi_CompoundDependencyCount DEFAULT(0),
    PrincipalInventoryNoPurchaseCount  INT           NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementKpi_PrincipalInventoryNoPurchaseCount DEFAULT(0),
    UnknownPrincipalCount              INT           NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementKpi_UnknownPrincipalCount DEFAULT(0),
    PurchasingInactivityFlag           BIT           NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementKpi_PurchasingInactivityFlag DEFAULT(0),
    QualifiedBacklogPrincipalCount     INT           NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementKpi_QualifiedBacklogPrincipalCount DEFAULT(0),
    PrincipalAtRiskExposureCount       INT           NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementKpi_PrincipalAtRiskExposureCount DEFAULT(0),
    LastRefreshLogId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PurchasingManagementKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
END
GO

-- BTRPD_PurchasingManagementAttention
IF OBJECT_ID(N'dbo.BTRPD_PurchasingManagementAttention', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_PurchasingManagementAttention
(
    PurchasingManagementAttentionId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementAttention_PurchasingManagementAttentionId DEFAULT(''),
    SnapshotKey                       VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementAttention_SnapshotKey DEFAULT('CURRENT'),
    EntityType                        VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementAttention_EntityType DEFAULT(''),
    EntityName                        VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementAttention_EntityName DEFAULT(''),
    SignalKey                         VARCHAR(40)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementAttention_SignalKey DEFAULT(''),
    SignalLabel                       VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementAttention_SignalLabel DEFAULT(''),
    ValueAmount                       DECIMAL(18,2) NULL,
    ValueText                         VARCHAR(100)  NULL,
    ReportRoute                       VARCHAR(100)  NULL,
    SortOrder                         INT           NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementAttention_SortOrder DEFAULT(0),

    CONSTRAINT PK_BTRPD_PurchasingManagementAttention PRIMARY KEY CLUSTERED (PurchasingManagementAttentionId)
)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_PurchasingManagementAttention_SnapshotKey_SortOrder' AND object_id = OBJECT_ID(N'dbo.BTRPD_PurchasingManagementAttention'))
CREATE INDEX IX_BTRPD_PurchasingManagementAttention_SnapshotKey_SortOrder
    ON BTRPD_PurchasingManagementAttention (SnapshotKey, SortOrder)
GO

-- BTRPD_PurchasingManagementTopPrincipal
IF OBJECT_ID(N'dbo.BTRPD_PurchasingManagementTopPrincipal', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_PurchasingManagementTopPrincipal
(
    PurchasingManagementTopPrincipalId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementTopPrincipal_PurchasingManagementTopPrincipalId DEFAULT(''),
    SnapshotKey                          VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementTopPrincipal_SnapshotKey DEFAULT('CURRENT'),
    Rank                                 INT           NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementTopPrincipal_Rank DEFAULT(0),
    PrincipalName                        VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementTopPrincipal_PrincipalName DEFAULT(''),
    MtdPurchaseAmount                    DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementTopPrincipal_MtdPurchaseAmount DEFAULT(0),
    PercentOfPurchase                    DECIMAL(9,4)  NULL,
    InventoryValue                       DECIMAL(18,2) NULL,
    PercentOfInventory                   DECIMAL(9,4)  NULL,
    AtRiskValue                          DECIMAL(18,2) NULL,
    PercentOfAtRisk                      DECIMAL(9,4)  NULL,
    IsCompoundDependency                 BIT           NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementTopPrincipal_IsCompoundDependency DEFAULT(0),
    IsInventoryNoPurchase                BIT           NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementTopPrincipal_IsInventoryNoPurchase DEFAULT(0),
    ReportRoute                          VARCHAR(100)  NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementTopPrincipal_ReportRoute DEFAULT(''),

    CONSTRAINT PK_BTRPD_PurchasingManagementTopPrincipal PRIMARY KEY CLUSTERED (PurchasingManagementTopPrincipalId)
)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_BTRPD_PurchasingManagementTopPrincipal_SnapshotKey_Rank' AND object_id = OBJECT_ID(N'dbo.BTRPD_PurchasingManagementTopPrincipal'))
CREATE UNIQUE INDEX UX_BTRPD_PurchasingManagementTopPrincipal_SnapshotKey_Rank
    ON BTRPD_PurchasingManagementTopPrincipal (SnapshotKey, Rank)
GO

-- BTRPD_CustomerKpi
IF OBJECT_ID(N'dbo.BTRPD_CustomerKpi', N'U') IS NULL
BEGIN
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
END
GO

-- BTRPD_CustomerTopOmzet
IF OBJECT_ID(N'dbo.BTRPD_CustomerTopOmzet', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_CustomerTopOmzet
(
    CustomerTopOmzetId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_CustomerTopOmzet_CustomerTopOmzetId DEFAULT(''),
    SnapshotKey        VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_CustomerTopOmzet_SnapshotKey DEFAULT('CURRENT'),
    Rank               INT           NOT NULL CONSTRAINT DF_BTRPD_CustomerTopOmzet_Rank DEFAULT(0),
    CustomerCode       VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_CustomerTopOmzet_CustomerCode DEFAULT(''),
    CustomerName       VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_CustomerTopOmzet_CustomerName DEFAULT(''),
    OmzetAmount        DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CustomerTopOmzet_OmzetAmount DEFAULT(0),
    PercentOfTotal     DECIMAL(9,4)  NULL,

    CONSTRAINT PK_BTRPD_CustomerTopOmzet PRIMARY KEY CLUSTERED (CustomerTopOmzetId),
    CONSTRAINT UX_BTRPD_CustomerTopOmzet_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)
)
END
GO

-- BTRPD_CustomerTopPiutang
IF OBJECT_ID(N'dbo.BTRPD_CustomerTopPiutang', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_CustomerTopPiutang
(
    CustomerTopPiutangId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_CustomerTopPiutang_CustomerTopPiutangId DEFAULT(''),
    SnapshotKey          VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_CustomerTopPiutang_SnapshotKey DEFAULT('CURRENT'),
    Rank                 INT           NOT NULL CONSTRAINT DF_BTRPD_CustomerTopPiutang_Rank DEFAULT(0),
    CustomerCode         VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_CustomerTopPiutang_CustomerCode DEFAULT(''),
    CustomerName         VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_CustomerTopPiutang_CustomerName DEFAULT(''),
    OutstandingBalance   DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CustomerTopPiutang_OutstandingBalance DEFAULT(0),
    PercentOfTotal       DECIMAL(9,4)  NULL,

    CONSTRAINT PK_BTRPD_CustomerTopPiutang PRIMARY KEY CLUSTERED (CustomerTopPiutangId),
    CONSTRAINT UX_BTRPD_CustomerTopPiutang_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)
)
END
GO

-- BTRPD_CustomerAttention
IF OBJECT_ID(N'dbo.BTRPD_CustomerAttention', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_CustomerAttention
(
    CustomerAttentionId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_CustomerAttention_CustomerAttentionId DEFAULT(''),
    SnapshotKey         VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_CustomerAttention_SnapshotKey DEFAULT('CURRENT'),
    CustomerCode        VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_CustomerAttention_CustomerCode DEFAULT(''),
    CustomerName        VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_CustomerAttention_CustomerName DEFAULT(''),
    SignalKey           VARCHAR(30)   NOT NULL CONSTRAINT DF_BTRPD_CustomerAttention_SignalKey DEFAULT(''),
    SignalLabel         VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_CustomerAttention_SignalLabel DEFAULT(''),
    ValueAmount         DECIMAL(18,2) NULL,
    ValueText           VARCHAR(50)   NULL,
    WilayahName         VARCHAR(30)   NOT NULL CONSTRAINT DF_BTRPD_CustomerAttention_WilayahName DEFAULT(''),
    SortOrder           INT           NOT NULL CONSTRAINT DF_BTRPD_CustomerAttention_SortOrder DEFAULT(0),

    CONSTRAINT PK_BTRPD_CustomerAttention PRIMARY KEY CLUSTERED (CustomerAttentionId)
)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_CustomerAttention_SnapshotKey_SortOrder' AND object_id = OBJECT_ID(N'dbo.BTRPD_CustomerAttention'))
CREATE INDEX IX_BTRPD_CustomerAttention_SnapshotKey_SortOrder
    ON BTRPD_CustomerAttention (SnapshotKey, SortOrder)
GO

-- BTRPD_CustomerSegmentation
IF OBJECT_ID(N'dbo.BTRPD_CustomerSegmentation', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_CustomerSegmentation
(
    CustomerSegmentationId VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_CustomerSegmentation_CustomerSegmentationId DEFAULT(''),
    SnapshotKey            VARCHAR(10) NOT NULL CONSTRAINT DF_BTRPD_CustomerSegmentation_SnapshotKey DEFAULT('CURRENT'),
    SegmentType            VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_CustomerSegmentation_SegmentType DEFAULT(''),
    SegmentKey             VARCHAR(30) NOT NULL CONSTRAINT DF_BTRPD_CustomerSegmentation_SegmentKey DEFAULT(''),
    SegmentLabel           VARCHAR(50) NOT NULL CONSTRAINT DF_BTRPD_CustomerSegmentation_SegmentLabel DEFAULT(''),
    CustomerCount          INT         NOT NULL CONSTRAINT DF_BTRPD_CustomerSegmentation_CustomerCount DEFAULT(0),
    ActiveCount            INT         NOT NULL CONSTRAINT DF_BTRPD_CustomerSegmentation_ActiveCount DEFAULT(0),
    DormantCount           INT         NOT NULL CONSTRAINT DF_BTRPD_CustomerSegmentation_DormantCount DEFAULT(0),
    SortOrder              INT         NOT NULL CONSTRAINT DF_BTRPD_CustomerSegmentation_SortOrder DEFAULT(0),

    CONSTRAINT PK_BTRPD_CustomerSegmentation PRIMARY KEY CLUSTERED (CustomerSegmentationId),
    CONSTRAINT UX_BTRPD_CustomerSegmentation_SnapshotKey_SegmentType_SegmentKey UNIQUE (SnapshotKey, SegmentType, SegmentKey)
)
END
GO

-- BTRPD_CollectionKpi
IF OBJECT_ID(N'dbo.BTRPD_CollectionKpi', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_CollectionKpi
(
    SnapshotKey                   VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_CollectionKpi_SnapshotKey DEFAULT('CURRENT'),
    GeneratedAt                   DATETIME      NOT NULL CONSTRAINT DF_BTRPD_CollectionKpi_GeneratedAt DEFAULT('3000-01-01'),
    PeriodYear                    INT           NOT NULL CONSTRAINT DF_BTRPD_CollectionKpi_PeriodYear DEFAULT(0),
    PeriodMonth                   INT           NOT NULL CONSTRAINT DF_BTRPD_CollectionKpi_PeriodMonth DEFAULT(0),
    OverdueExposure               DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CollectionKpi_OverdueExposure DEFAULT(0),
    AgingOver90Exposure           DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CollectionKpi_AgingOver90Exposure DEFAULT(0),
    OverdueConcentrationPercent   DECIMAL(9,4)  NULL,
    CashCollectedMtd              DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CollectionKpi_CashCollectedMtd DEFAULT(0),
    MonthCollections              DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CollectionKpi_MonthCollections DEFAULT(0),
    MonthFakturOmzet              DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CollectionKpi_MonthFakturOmzet DEFAULT(0),
    RecoveryVsBillingPercent      DECIMAL(9,4)  NULL,
    PaymentMixCashAmount          DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CollectionKpi_PaymentMixCashAmount DEFAULT(0),
    PaymentMixGiroAmount          DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CollectionKpi_PaymentMixGiroAmount DEFAULT(0),
    PaymentMixAdjustmentAmount    DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CollectionKpi_PaymentMixAdjustmentAmount DEFAULT(0),
    PaymentMixCashPercent         DECIMAL(9,4)  NULL,
    PaymentMixGiroPercent         DECIMAL(9,4)  NULL,
    PaymentMixAdjustmentPercent   DECIMAL(9,4)  NULL,
    LegacyDebtCount               INT           NOT NULL CONSTRAINT DF_BTRPD_CollectionKpi_LegacyDebtCount DEFAULT(0),
    ChronicOverdueCount           INT           NOT NULL CONSTRAINT DF_BTRPD_CollectionKpi_ChronicOverdueCount DEFAULT(0),
    WilayahHotspotCount           INT           NOT NULL CONSTRAINT DF_BTRPD_CollectionKpi_WilayahHotspotCount DEFAULT(0),
    LowRecoveryVsBillingCount     INT           NOT NULL CONSTRAINT DF_BTRPD_CollectionKpi_LowRecoveryVsBillingCount DEFAULT(0),
    LastRefreshLogId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_CollectionKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_CollectionKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
END
GO

-- BTRPD_CollectionAging
IF OBJECT_ID(N'dbo.BTRPD_CollectionAging', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_CollectionAging
(
    CollectionAgingId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_CollectionAging_CollectionAgingId DEFAULT(''),
    SnapshotKey         VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_CollectionAging_SnapshotKey DEFAULT('CURRENT'),
    BucketKey           VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_CollectionAging_BucketKey DEFAULT(''),
    BucketLabel         VARCHAR(30)   NOT NULL CONSTRAINT DF_BTRPD_CollectionAging_BucketLabel DEFAULT(''),
    Amount              DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CollectionAging_Amount DEFAULT(0),
    SortOrder           INT           NOT NULL CONSTRAINT DF_BTRPD_CollectionAging_SortOrder DEFAULT(0),

    CONSTRAINT PK_BTRPD_CollectionAging PRIMARY KEY CLUSTERED (CollectionAgingId),
    CONSTRAINT UX_BTRPD_CollectionAging_SnapshotKey_BucketKey UNIQUE (SnapshotKey, BucketKey)
)
END
GO

-- BTRPD_CollectionAttention
IF OBJECT_ID(N'dbo.BTRPD_CollectionAttention', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_CollectionAttention
(
    CollectionAttentionId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_CollectionAttention_CollectionAttentionId DEFAULT(''),
    SnapshotKey           VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_CollectionAttention_SnapshotKey DEFAULT('CURRENT'),
    EntityType            VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_CollectionAttention_EntityType DEFAULT(''),
    EntityId              VARCHAR(13)   NOT NULL CONSTRAINT DF_BTRPD_CollectionAttention_EntityId DEFAULT(''),
    EntityCode            VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_CollectionAttention_EntityCode DEFAULT(''),
    EntityName            VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_CollectionAttention_EntityName DEFAULT(''),
    SignalKey             VARCHAR(30)   NOT NULL CONSTRAINT DF_BTRPD_CollectionAttention_SignalKey DEFAULT(''),
    SignalLabel           VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_CollectionAttention_SignalLabel DEFAULT(''),
    ValueAmount           DECIMAL(18,2) NULL,
    ValueText             VARCHAR(100)  NULL,
    WilayahName           VARCHAR(30)   NOT NULL CONSTRAINT DF_BTRPD_CollectionAttention_WilayahName DEFAULT(''),
    ReportRoute           VARCHAR(100)  NULL,
    SortOrder             INT           NOT NULL CONSTRAINT DF_BTRPD_CollectionAttention_SortOrder DEFAULT(0),

    CONSTRAINT PK_BTRPD_CollectionAttention PRIMARY KEY CLUSTERED (CollectionAttentionId)
)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_CollectionAttention_SnapshotKey_SortOrder' AND object_id = OBJECT_ID(N'dbo.BTRPD_CollectionAttention'))
CREATE INDEX IX_BTRPD_CollectionAttention_SnapshotKey_SortOrder
    ON BTRPD_CollectionAttention (SnapshotKey, SortOrder)
GO

-- BTRPD_CollectionTopOverdueCustomer
IF OBJECT_ID(N'dbo.BTRPD_CollectionTopOverdueCustomer', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_CollectionTopOverdueCustomer
(
    CollectionTopOverdueCustomerId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueCustomer_CollectionTopOverdueCustomerId DEFAULT(''),
    SnapshotKey                      VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueCustomer_SnapshotKey DEFAULT('CURRENT'),
    Rank                             INT           NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueCustomer_Rank DEFAULT(0),
    CustomerCode                     VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueCustomer_CustomerCode DEFAULT(''),
    CustomerName                     VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueCustomer_CustomerName DEFAULT(''),
    OverdueBalance                   DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueCustomer_OverdueBalance DEFAULT(0),
    PercentOfTotal                   DECIMAL(9,4)  NULL,

    CONSTRAINT PK_BTRPD_CollectionTopOverdueCustomer PRIMARY KEY CLUSTERED (CollectionTopOverdueCustomerId),
    CONSTRAINT UX_BTRPD_CollectionTopOverdueCustomer_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)
)
END
GO

-- BTRPD_CollectionTopOverdueSalesman
IF OBJECT_ID(N'dbo.BTRPD_CollectionTopOverdueSalesman', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_CollectionTopOverdueSalesman
(
    CollectionTopOverdueSalesmanId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueSalesman_CollectionTopOverdueSalesmanId DEFAULT(''),
    SnapshotKey                    VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueSalesman_SnapshotKey DEFAULT('CURRENT'),
    Rank                           INT           NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueSalesman_Rank DEFAULT(0),
    SalesPersonId                  VARCHAR(13)   NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueSalesman_SalesPersonId DEFAULT(''),
    SalesPersonCode                VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueSalesman_SalesPersonCode DEFAULT(''),
    SalesPersonName                VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueSalesman_SalesPersonName DEFAULT(''),
    OverdueBalance                 DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueSalesman_OverdueBalance DEFAULT(0),
    PercentOfTotal                 DECIMAL(9,4)  NULL,

    CONSTRAINT PK_BTRPD_CollectionTopOverdueSalesman PRIMARY KEY CLUSTERED (CollectionTopOverdueSalesmanId),
    CONSTRAINT UX_BTRPD_CollectionTopOverdueSalesman_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)
)
END
GO

-- BTRPD_CollectionTopOverdueWilayah
IF OBJECT_ID(N'dbo.BTRPD_CollectionTopOverdueWilayah', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_CollectionTopOverdueWilayah
(
    CollectionTopOverdueWilayahId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueWilayah_CollectionTopOverdueWilayahId DEFAULT(''),
    SnapshotKey                   VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueWilayah_SnapshotKey DEFAULT('CURRENT'),
    Rank                          INT           NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueWilayah_Rank DEFAULT(0),
    WilayahId                     VARCHAR(13)   NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueWilayah_WilayahId DEFAULT(''),
    WilayahName                   VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueWilayah_WilayahName DEFAULT(''),
    OverdueBalance                DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueWilayah_OverdueBalance DEFAULT(0),
    PercentOfTotal                DECIMAL(9,4)  NULL,

    CONSTRAINT PK_BTRPD_CollectionTopOverdueWilayah PRIMARY KEY CLUSTERED (CollectionTopOverdueWilayahId),
    CONSTRAINT UX_BTRPD_CollectionTopOverdueWilayah_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)
)
END
GO

-- BTRPD_LocationKpi
IF OBJECT_ID(N'dbo.BTRPD_LocationKpi', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_LocationKpi

(

    SnapshotKey                        VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_LocationKpi_SnapshotKey DEFAULT('CURRENT'),

    GeneratedAt                        DATETIME      NOT NULL CONSTRAINT DF_BTRPD_LocationKpi_GeneratedAt DEFAULT('3000-01-01'),

    PeriodYear                         INT           NOT NULL CONSTRAINT DF_BTRPD_LocationKpi_PeriodYear DEFAULT(0),

    PeriodMonth                        INT           NOT NULL CONSTRAINT DF_BTRPD_LocationKpi_PeriodMonth DEFAULT(0),

    Top1WarehouseInventoryPercent      DECIMAL(9,4)  NULL,

    Top3WarehouseInventoryPercent      DECIMAL(9,4)  NULL,

    Top1WarehouseAtRiskPercent         DECIMAL(9,4)  NULL,

    Top1WarehouseSalesPercent          DECIMAL(9,4)  NULL,

    Top1WilayahSalesPercent            DECIMAL(9,4)  NULL,

    InactiveWarehouseWithStockCount    INT           NOT NULL CONSTRAINT DF_BTRPD_LocationKpi_InactiveWarehouseWithStockCount DEFAULT(0),

    WarehouseNoSalesWithInventoryCount INT           NOT NULL CONSTRAINT DF_BTRPD_LocationKpi_WarehouseNoSalesWithInventoryCount DEFAULT(0),

    TotalInventoryValue                DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_LocationKpi_TotalInventoryValue DEFAULT(0),

    TotalAtRiskValue                   DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_LocationKpi_TotalAtRiskValue DEFAULT(0),

    TotalOmzet                         DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_LocationKpi_TotalOmzet DEFAULT(0),

    TotalPurchase                      DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_LocationKpi_TotalPurchase DEFAULT(0),

    LastRefreshLogId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_LocationKpi_LastRefreshLogId DEFAULT(''),



    CONSTRAINT PK_BTRPD_LocationKpi PRIMARY KEY CLUSTERED (SnapshotKey)

)
END
GO

-- BTRPD_LocationTopWarehouseInventory
IF OBJECT_ID(N'dbo.BTRPD_LocationTopWarehouseInventory', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_LocationTopWarehouseInventory

(

    LocationTopWarehouseInventoryId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehouseInventory_LocationTopWarehouseInventoryId DEFAULT(''),

    SnapshotKey                     VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehouseInventory_SnapshotKey DEFAULT('CURRENT'),

    Rank                            INT           NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehouseInventory_Rank DEFAULT(0),

    WarehouseId                     VARCHAR(5)    NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehouseInventory_WarehouseId DEFAULT(''),

    WarehouseName                   VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehouseInventory_WarehouseName DEFAULT(''),

    InventoryValue                  DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehouseInventory_InventoryValue DEFAULT(0),

    PercentOfTotal                  DECIMAL(9,4)  NULL,

    ReportRoute                     VARCHAR(100)  NULL,



    CONSTRAINT PK_BTRPD_LocationTopWarehouseInventory PRIMARY KEY CLUSTERED (LocationTopWarehouseInventoryId),

    CONSTRAINT UX_BTRPD_LocationTopWarehouseInventory_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)

)
END
GO

-- BTRPD_LocationTopWarehouseAtRisk
IF OBJECT_ID(N'dbo.BTRPD_LocationTopWarehouseAtRisk', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_LocationTopWarehouseAtRisk

(

    LocationTopWarehouseAtRiskId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehouseAtRisk_LocationTopWarehouseAtRiskId DEFAULT(''),

    SnapshotKey                  VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehouseAtRisk_SnapshotKey DEFAULT('CURRENT'),

    Rank                         INT           NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehouseAtRisk_Rank DEFAULT(0),

    WarehouseId                  VARCHAR(5)    NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehouseAtRisk_WarehouseId DEFAULT(''),

    WarehouseName                VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehouseAtRisk_WarehouseName DEFAULT(''),

    AtRiskValue                  DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehouseAtRisk_AtRiskValue DEFAULT(0),

    PercentOfTotal               DECIMAL(9,4)  NULL,

    ReportRoute                  VARCHAR(100)  NULL,



    CONSTRAINT PK_BTRPD_LocationTopWarehouseAtRisk PRIMARY KEY CLUSTERED (LocationTopWarehouseAtRiskId),

    CONSTRAINT UX_BTRPD_LocationTopWarehouseAtRisk_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)

)
END
GO

-- BTRPD_LocationTopWarehouseSales
IF OBJECT_ID(N'dbo.BTRPD_LocationTopWarehouseSales', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_LocationTopWarehouseSales

(

    LocationTopWarehouseSalesId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehouseSales_LocationTopWarehouseSalesId DEFAULT(''),

    SnapshotKey                 VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehouseSales_SnapshotKey DEFAULT('CURRENT'),

    Rank                        INT           NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehouseSales_Rank DEFAULT(0),

    WarehouseId                 VARCHAR(5)    NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehouseSales_WarehouseId DEFAULT(''),

    WarehouseName               VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehouseSales_WarehouseName DEFAULT(''),

    MtdOmzet                    DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehouseSales_MtdOmzet DEFAULT(0),

    PercentOfTotal              DECIMAL(9,4)  NULL,

    ReportRoute                 VARCHAR(100)  NULL,



    CONSTRAINT PK_BTRPD_LocationTopWarehouseSales PRIMARY KEY CLUSTERED (LocationTopWarehouseSalesId),

    CONSTRAINT UX_BTRPD_LocationTopWarehouseSales_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)

)
END
GO

-- BTRPD_LocationTopWarehousePurchasing
IF OBJECT_ID(N'dbo.BTRPD_LocationTopWarehousePurchasing', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_LocationTopWarehousePurchasing

(

    LocationTopWarehousePurchasingId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehousePurchasing_LocationTopWarehousePurchasingId DEFAULT(''),

    SnapshotKey                      VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehousePurchasing_SnapshotKey DEFAULT('CURRENT'),

    Rank                             INT           NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehousePurchasing_Rank DEFAULT(0),

    WarehouseId                      VARCHAR(5)    NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehousePurchasing_WarehouseId DEFAULT(''),

    WarehouseName                    VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehousePurchasing_WarehouseName DEFAULT(''),

    MtdPurchaseAmount                DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehousePurchasing_MtdPurchaseAmount DEFAULT(0),

    PercentOfTotal                   DECIMAL(9,4)  NULL,

    ReportRoute                      VARCHAR(100)  NULL,



    CONSTRAINT PK_BTRPD_LocationTopWarehousePurchasing PRIMARY KEY CLUSTERED (LocationTopWarehousePurchasingId),

    CONSTRAINT UX_BTRPD_LocationTopWarehousePurchasing_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)

)
END
GO

-- BTRPD_LocationTopWilayahSales
IF OBJECT_ID(N'dbo.BTRPD_LocationTopWilayahSales', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_LocationTopWilayahSales

(

    LocationTopWilayahSalesId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_LocationTopWilayahSales_LocationTopWilayahSalesId DEFAULT(''),

    SnapshotKey               VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_LocationTopWilayahSales_SnapshotKey DEFAULT('CURRENT'),

    Rank                      INT           NOT NULL CONSTRAINT DF_BTRPD_LocationTopWilayahSales_Rank DEFAULT(0),

    WilayahId                 VARCHAR(5)    NULL,

    WilayahName               VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_LocationTopWilayahSales_WilayahName DEFAULT(''),

    MtdOmzet                  DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_LocationTopWilayahSales_MtdOmzet DEFAULT(0),

    PercentOfTotal            DECIMAL(9,4)  NULL,

    DashboardRoute            VARCHAR(100)  NULL,



    CONSTRAINT PK_BTRPD_LocationTopWilayahSales PRIMARY KEY CLUSTERED (LocationTopWilayahSalesId),

    CONSTRAINT UX_BTRPD_LocationTopWilayahSales_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)

)
END
GO

-- BTRPD_LocationAttention
IF OBJECT_ID(N'dbo.BTRPD_LocationAttention', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_LocationAttention

(

    LocationAttentionId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_LocationAttention_LocationAttentionId DEFAULT(''),

    SnapshotKey         VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_LocationAttention_SnapshotKey DEFAULT('CURRENT'),

    EntityType          VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_LocationAttention_EntityType DEFAULT(''),

    EntityCode          VARCHAR(5)    NULL,

    EntityName          VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_LocationAttention_EntityName DEFAULT(''),

    SignalKey           VARCHAR(40)   NOT NULL CONSTRAINT DF_BTRPD_LocationAttention_SignalKey DEFAULT(''),

    SignalLabel         VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_LocationAttention_SignalLabel DEFAULT(''),

    ValueAmount         DECIMAL(18,2) NULL,

    ValueText           VARCHAR(100)  NULL,

    ReportRoute         VARCHAR(100)  NULL,

    SortOrder           INT           NOT NULL CONSTRAINT DF_BTRPD_LocationAttention_SortOrder DEFAULT(0),



    CONSTRAINT PK_BTRPD_LocationAttention PRIMARY KEY CLUSTERED (LocationAttentionId)

)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_LocationAttention_SnapshotKey_SortOrder' AND object_id = OBJECT_ID(N'dbo.BTRPD_LocationAttention'))
CREATE INDEX IX_BTRPD_LocationAttention_SnapshotKey_SortOrder

    ON BTRPD_LocationAttention (SnapshotKey, SortOrder)
GO

-- BTRPD_SalesmanKpi
IF OBJECT_ID(N'dbo.BTRPD_SalesmanKpi', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_SalesmanKpi
(
    SnapshotKey                 VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanKpi_SnapshotKey DEFAULT('CURRENT'),
    GeneratedAt                 DATETIME      NOT NULL CONSTRAINT DF_BTRPD_SalesmanKpi_GeneratedAt DEFAULT('3000-01-01'),
    PeriodYear                  INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanKpi_PeriodYear DEFAULT(0),
    PeriodMonth                 INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanKpi_PeriodMonth DEFAULT(0),
    TotalTeamOmzet              DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_SalesmanKpi_TotalTeamOmzet DEFAULT(0),
    TotalPiutang                DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_SalesmanKpi_TotalPiutang DEFAULT(0),
    ActiveSalesmanCount         INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanKpi_ActiveSalesmanCount DEFAULT(0),
    BelowTargetCount            INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanKpi_BelowTargetCount DEFAULT(0),
    NoTargetCount               INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanKpi_NoTargetCount DEFAULT(0),
    HighOverdueExposureCount    INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanKpi_HighOverdueExposureCount DEFAULT(0),
    HighPiutangExposureCount    INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanKpi_HighPiutangExposureCount DEFAULT(0),
    CustomerConcentrationCount  INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanKpi_CustomerConcentrationCount DEFAULT(0),
    DormantPortfolioCount       INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanKpi_DormantPortfolioCount DEFAULT(0),
    TopOmzetSalesmanPercent     DECIMAL(9,4)  NULL,
    TopPiutangSalesmanPercent   DECIMAL(9,4)  NULL,
    LastRefreshLogId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_SalesmanKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
END
GO

-- BTRPD_SalesmanTopOmzet
IF OBJECT_ID(N'dbo.BTRPD_SalesmanTopOmzet', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_SalesmanTopOmzet
(
    SalesmanTopOmzetId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopOmzet_SalesmanTopOmzetId DEFAULT(''),
    SnapshotKey        VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopOmzet_SnapshotKey DEFAULT('CURRENT'),
    Rank               INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopOmzet_Rank DEFAULT(0),
    SalesPersonId      VARCHAR(13)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopOmzet_SalesPersonId DEFAULT(''),
    SalesPersonCode    VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopOmzet_SalesPersonCode DEFAULT(''),
    SalesPersonName    VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopOmzet_SalesPersonName DEFAULT(''),
    CompletedOmzet     DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopOmzet_CompletedOmzet DEFAULT(0),
    PercentOfTotal     DECIMAL(9,4)  NULL,

    CONSTRAINT PK_BTRPD_SalesmanTopOmzet PRIMARY KEY CLUSTERED (SalesmanTopOmzetId),
    CONSTRAINT UX_BTRPD_SalesmanTopOmzet_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)
)
END
GO

-- BTRPD_SalesmanTopAchievement
IF OBJECT_ID(N'dbo.BTRPD_SalesmanTopAchievement', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_SalesmanTopAchievement
(
    SalesmanTopAchievementId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopAchievement_SalesmanTopAchievementId DEFAULT(''),
    SnapshotKey              VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopAchievement_SnapshotKey DEFAULT('CURRENT'),
    Rank                     INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopAchievement_Rank DEFAULT(0),
    SalesPersonId            VARCHAR(13)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopAchievement_SalesPersonId DEFAULT(''),
    SalesPersonCode          VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopAchievement_SalesPersonCode DEFAULT(''),
    SalesPersonName          VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopAchievement_SalesPersonName DEFAULT(''),
    TargetAmount             DECIMAL(18,2) NULL,
    CompletedOmzet           DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopAchievement_CompletedOmzet DEFAULT(0),
    AchievementPercent       DECIMAL(9,4)  NULL,
    PercentOfTotal           DECIMAL(9,4)  NULL,

    CONSTRAINT PK_BTRPD_SalesmanTopAchievement PRIMARY KEY CLUSTERED (SalesmanTopAchievementId),
    CONSTRAINT UX_BTRPD_SalesmanTopAchievement_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)
)
END
GO

-- BTRPD_SalesmanTopPiutang
IF OBJECT_ID(N'dbo.BTRPD_SalesmanTopPiutang', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_SalesmanTopPiutang
(
    SalesmanTopPiutangId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopPiutang_SalesmanTopPiutangId DEFAULT(''),
    SnapshotKey          VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopPiutang_SnapshotKey DEFAULT('CURRENT'),
    Rank                 INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopPiutang_Rank DEFAULT(0),
    SalesPersonId        VARCHAR(13)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopPiutang_SalesPersonId DEFAULT(''),
    SalesPersonCode      VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopPiutang_SalesPersonCode DEFAULT(''),
    SalesPersonName      VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopPiutang_SalesPersonName DEFAULT(''),
    OutstandingBalance   DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopPiutang_OutstandingBalance DEFAULT(0),
    PercentOfTotal       DECIMAL(9,4)  NULL,

    CONSTRAINT PK_BTRPD_SalesmanTopPiutang PRIMARY KEY CLUSTERED (SalesmanTopPiutangId),
    CONSTRAINT UX_BTRPD_SalesmanTopPiutang_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)
)
END
GO

-- BTRPD_SalesmanAttention
IF OBJECT_ID(N'dbo.BTRPD_SalesmanAttention', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_SalesmanAttention
(
    SalesmanAttentionId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanAttention_SalesmanAttentionId DEFAULT(''),
    SnapshotKey         VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanAttention_SnapshotKey DEFAULT('CURRENT'),
    SalesPersonId       VARCHAR(13)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanAttention_SalesPersonId DEFAULT(''),
    SalesPersonCode     VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanAttention_SalesPersonCode DEFAULT(''),
    SalesPersonName     VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanAttention_SalesPersonName DEFAULT(''),
    SignalKey           VARCHAR(30)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanAttention_SignalKey DEFAULT(''),
    SignalLabel         VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanAttention_SignalLabel DEFAULT(''),
    ValueAmount         DECIMAL(18,2) NULL,
    ValueText           VARCHAR(100)  NULL,
    WilayahName         VARCHAR(30)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanAttention_WilayahName DEFAULT(''),
    SortOrder           INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanAttention_SortOrder DEFAULT(0),

    CONSTRAINT PK_BTRPD_SalesmanAttention PRIMARY KEY CLUSTERED (SalesmanAttentionId)
)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_SalesmanAttention_SnapshotKey_SortOrder' AND object_id = OBJECT_ID(N'dbo.BTRPD_SalesmanAttention'))
CREATE INDEX IX_BTRPD_SalesmanAttention_SnapshotKey_SortOrder
    ON BTRPD_SalesmanAttention (SnapshotKey, SortOrder)
GO

-- BTRPD_SalesmanSegmentation
IF OBJECT_ID(N'dbo.BTRPD_SalesmanSegmentation', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_SalesmanSegmentation
(
    SalesmanSegmentationId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanSegmentation_SalesmanSegmentationId DEFAULT(''),
    SnapshotKey            VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanSegmentation_SnapshotKey DEFAULT('CURRENT'),
    SegmentType            VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanSegmentation_SegmentType DEFAULT(''),
    SegmentKey             VARCHAR(30)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanSegmentation_SegmentKey DEFAULT(''),
    SegmentLabel           VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanSegmentation_SegmentLabel DEFAULT(''),
    SalesmanCount          INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanSegmentation_SalesmanCount DEFAULT(0),
    ActiveCount            INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanSegmentation_ActiveCount DEFAULT(0),
    InactiveCount          INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanSegmentation_InactiveCount DEFAULT(0),
    SortOrder              INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanSegmentation_SortOrder DEFAULT(0),

    CONSTRAINT PK_BTRPD_SalesmanSegmentation PRIMARY KEY CLUSTERED (SalesmanSegmentationId),
    CONSTRAINT UX_BTRPD_SalesmanSegmentation_SnapshotKey_SegmentType_SegmentKey UNIQUE (SnapshotKey, SegmentType, SegmentKey)
)
END
GO

-- BTRPD_CustomerRiskForecastKpi (M29)
IF OBJECT_ID(N'dbo.BTRPD_CustomerRiskForecastKpi', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_CustomerRiskForecastKpi
(
    SnapshotKey                         VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_SnapshotKey DEFAULT('CURRENT'),
    GeneratedAt                         DATETIME       NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_GeneratedAt DEFAULT('3000-01-01'),
    BusinessDate                        DATETIME       NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_BusinessDate DEFAULT('3000-01-01'),
    HorizonDays                         INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_HorizonDays DEFAULT(0),
    CustomersForecastedAtRisk           INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_CustomersForecastedAtRisk DEFAULT(0),
    HighRiskCustomerCount               INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_HighRiskCustomerCount DEFAULT(0),
    CriticalCustomerCount               INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_CriticalCustomerCount DEFAULT(0),
    ElevatedRiskReceivable              DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_ElevatedRiskReceivable DEFAULT(0),
    ElevatedRiskReceivablePercent       DECIMAL(9,4)   NULL,
    PortfolioHealthScore                DECIMAL(9,4)   NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_PortfolioHealthScore DEFAULT(0),
    TotalPiutang                        DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_TotalPiutang DEFAULT(0),
    ForecastConfidence                  VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_ForecastConfidence DEFAULT(''),
    PaymentDelaySignalCount             INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_PaymentDelaySignalCount DEFAULT(0),
    CreditLimitSignalCount              INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_CreditLimitSignalCount DEFAULT(0),
    InactivitySignalCount               INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_InactivitySignalCount DEFAULT(0),
    PurchaseDeclineSignalCount          INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_PurchaseDeclineSignalCount DEFAULT(0),
    CollectionRiskSignalCount           INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_CollectionRiskSignalCount DEFAULT(0),
    HealthyCount                        INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_HealthyCount DEFAULT(0),
    WatchCount                          INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_WatchCount DEFAULT(0),
    AttentionCount                      INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_AttentionCount DEFAULT(0),
    HighRiskCount                       INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_HighRiskCount DEFAULT(0),
    CriticalCount                       INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_CriticalCount DEFAULT(0),
    ExecutiveSummaryText                VARCHAR(2000)  NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_ExecutiveSummaryText DEFAULT(''),
    LastRefreshLogId                    VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_CustomerRiskForecastKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
END
GO

-- BTRPD_CustomerRiskForecastDist
IF OBJECT_ID(N'dbo.BTRPD_CustomerRiskForecastDist', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_CustomerRiskForecastDist
(
    CustomerRiskForecastDistId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastDist_CustomerRiskForecastDistId DEFAULT(''),
    SnapshotKey                VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastDist_SnapshotKey DEFAULT('CURRENT'),
    Category                   VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastDist_Category DEFAULT(''),
    CategoryLabel              VARCHAR(30)   NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastDist_CategoryLabel DEFAULT(''),
    CustomerCount              INT           NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastDist_CustomerCount DEFAULT(0),
    SortOrder                  INT           NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastDist_SortOrder DEFAULT(0),

    CONSTRAINT PK_BTRPD_CustomerRiskForecastDist PRIMARY KEY CLUSTERED (CustomerRiskForecastDistId)
)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_CustomerRiskForecastDist_SnapshotKey_SortOrder' AND object_id = OBJECT_ID(N'dbo.BTRPD_CustomerRiskForecastDist'))
CREATE INDEX IX_BTRPD_CustomerRiskForecastDist_SnapshotKey_SortOrder
    ON BTRPD_CustomerRiskForecastDist (SnapshotKey, SortOrder)
GO

-- BTRPD_CustomerRiskForecastWilayah
IF OBJECT_ID(N'dbo.BTRPD_CustomerRiskForecastWilayah', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_CustomerRiskForecastWilayah
(
    CustomerRiskForecastWilayahId VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastWilayah_CustomerRiskForecastWilayahId DEFAULT(''),
    SnapshotKey                   VARCHAR(10) NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastWilayah_SnapshotKey DEFAULT('CURRENT'),
    WilayahName                   VARCHAR(30) NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastWilayah_WilayahName DEFAULT(''),
    ElevatedRiskCustomerCount     INT         NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastWilayah_ElevatedRiskCustomerCount DEFAULT(0),
    SortOrder                     INT         NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastWilayah_SortOrder DEFAULT(0),

    CONSTRAINT PK_BTRPD_CustomerRiskForecastWilayah PRIMARY KEY CLUSTERED (CustomerRiskForecastWilayahId)
)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_CustomerRiskForecastWilayah_SnapshotKey_SortOrder' AND object_id = OBJECT_ID(N'dbo.BTRPD_CustomerRiskForecastWilayah'))
CREATE INDEX IX_BTRPD_CustomerRiskForecastWilayah_SnapshotKey_SortOrder
    ON BTRPD_CustomerRiskForecastWilayah (SnapshotKey, SortOrder)
GO

-- BTRPD_CustomerRiskForecastSignalMix
IF OBJECT_ID(N'dbo.BTRPD_CustomerRiskForecastSignalMix', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_CustomerRiskForecastSignalMix
(
    CustomerRiskForecastSignalMixId VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastSignalMix_CustomerRiskForecastSignalMixId DEFAULT(''),
    SnapshotKey                     VARCHAR(10) NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastSignalMix_SnapshotKey DEFAULT('CURRENT'),
    SignalFamilyKey                 VARCHAR(30) NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastSignalMix_SignalFamilyKey DEFAULT(''),
    SignalFamilyLabel               VARCHAR(50) NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastSignalMix_SignalFamilyLabel DEFAULT(''),
    CustomerCount                   INT         NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastSignalMix_CustomerCount DEFAULT(0),
    SortOrder                       INT         NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastSignalMix_SortOrder DEFAULT(0),

    CONSTRAINT PK_BTRPD_CustomerRiskForecastSignalMix PRIMARY KEY CLUSTERED (CustomerRiskForecastSignalMixId)
)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_CustomerRiskForecastSignalMix_SnapshotKey_SortOrder' AND object_id = OBJECT_ID(N'dbo.BTRPD_CustomerRiskForecastSignalMix'))
CREATE INDEX IX_BTRPD_CustomerRiskForecastSignalMix_SnapshotKey_SortOrder
    ON BTRPD_CustomerRiskForecastSignalMix (SnapshotKey, SortOrder)
GO

-- BTRPD_CustomerRiskForecastCustomer
IF OBJECT_ID(N'dbo.BTRPD_CustomerRiskForecastCustomer', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_CustomerRiskForecastCustomer
(
    CustomerRiskForecastCustomerId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_CustomerRiskForecastCustomerId DEFAULT(''),
    SnapshotKey                    VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_SnapshotKey DEFAULT('CURRENT'),
    SortOrder                      INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_SortOrder DEFAULT(0),
    RiskPriorityScore              INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_RiskPriorityScore DEFAULT(0),
    Category                       VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_Category DEFAULT(''),
    CategoryLabel                  VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_CategoryLabel DEFAULT(''),
    CustomerCode                   VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_CustomerCode DEFAULT(''),
    CustomerName                   VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_CustomerName DEFAULT(''),
    WilayahName                    VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_WilayahName DEFAULT(''),
    SalesPersonName                VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_SalesPersonName DEFAULT(''),
    OpenBalance                    DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_OpenBalance DEFAULT(0),
    OverdueBalance                 DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_OverdueBalance DEFAULT(0),
    DueWithinHorizon               DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_DueWithinHorizon DEFAULT(0),
    Plafond                        DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_Plafond DEFAULT(0),
    ProjectedOpenBalance           DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_ProjectedOpenBalance DEFAULT(0),
    MtdOmzet                       DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_MtdOmzet DEFAULT(0),
    PriorMonthOmzet                DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_PriorMonthOmzet DEFAULT(0),
    DeclineRatio                   DECIMAL(9,4)   NULL,
    DaysSinceLastFaktur            INT            NULL,
    AvgPaymentLagDays              DECIMAL(9,2)   NULL,
    PrimarySignalKey               VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_PrimarySignalKey DEFAULT(''),
    PrimarySignalLabel             VARCHAR(60)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_PrimarySignalLabel DEFAULT(''),
    ReasonText                     VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_ReasonText DEFAULT(''),
    RecommendationKey              VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_RecommendationKey DEFAULT(''),
    RecommendationLabel            VARCHAR(60)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_RecommendationLabel DEFAULT(''),
    ReportRoute                    VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_ReportRoute DEFAULT(''),
    DrillDownRoute                 VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_DrillDownRoute DEFAULT(''),

    CONSTRAINT PK_BTRPD_CustomerRiskForecastCustomer PRIMARY KEY CLUSTERED (CustomerRiskForecastCustomerId)
)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_CustomerRiskForecastCustomer_SnapshotKey_SortOrder' AND object_id = OBJECT_ID(N'dbo.BTRPD_CustomerRiskForecastCustomer'))
CREATE INDEX IX_BTRPD_CustomerRiskForecastCustomer_SnapshotKey_SortOrder
    ON BTRPD_CustomerRiskForecastCustomer (SnapshotKey, SortOrder)
GO

-- BTRPD_CustomerRiskForecastAttention
IF OBJECT_ID(N'dbo.BTRPD_CustomerRiskForecastAttention', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_CustomerRiskForecastAttention
(
    CustomerRiskForecastAttentionId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastAttention_CustomerRiskForecastAttentionId DEFAULT(''),
    SnapshotKey                       VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastAttention_SnapshotKey DEFAULT('CURRENT'),
    SortOrder                         INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastAttention_SortOrder DEFAULT(0),
    CustomerCode                      VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastAttention_CustomerCode DEFAULT(''),
    CustomerName                      VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastAttention_CustomerName DEFAULT(''),
    SignalKey                         VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastAttention_SignalKey DEFAULT(''),
    SignalLabel                       VARCHAR(60)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastAttention_SignalLabel DEFAULT(''),
    Severity                          VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastAttention_Severity DEFAULT(''),
    Amount                            DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastAttention_Amount DEFAULT(0),
    HorizonText                       VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastAttention_HorizonText DEFAULT(''),
    RuleId                            VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastAttention_RuleId DEFAULT(''),
    Explanation                       VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastAttention_Explanation DEFAULT(''),
    ReportRoute                       VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastAttention_ReportRoute DEFAULT(''),

    CONSTRAINT PK_BTRPD_CustomerRiskForecastAttention PRIMARY KEY CLUSTERED (CustomerRiskForecastAttentionId)
)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_CustomerRiskForecastAttention_SnapshotKey_SortOrder' AND object_id = OBJECT_ID(N'dbo.BTRPD_CustomerRiskForecastAttention'))
CREATE INDEX IX_BTRPD_CustomerRiskForecastAttention_SnapshotKey_SortOrder
    ON BTRPD_CustomerRiskForecastAttention (SnapshotKey, SortOrder)
GO

-- BTRPD_CustomerRiskForecastRecommendation
IF OBJECT_ID(N'dbo.BTRPD_CustomerRiskForecastRecommendation', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_CustomerRiskForecastRecommendation
(
    CustomerRiskForecastRecommendationId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastRecommendation_CustomerRiskForecastRecommendationId DEFAULT(''),
    SnapshotKey                          VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastRecommendation_SnapshotKey DEFAULT('CURRENT'),
    SortOrder                            INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastRecommendation_SortOrder DEFAULT(0),
    RecommendationKey                    VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastRecommendation_RecommendationKey DEFAULT(''),
    RecommendationLabel                  VARCHAR(60)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastRecommendation_RecommendationLabel DEFAULT(''),
    CustomerCode                         VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastRecommendation_CustomerCode DEFAULT(''),
    CustomerName                         VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastRecommendation_CustomerName DEFAULT(''),
    Category                             VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastRecommendation_Category DEFAULT(''),
    ReasonText                           VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastRecommendation_ReasonText DEFAULT(''),
    RuleId                               VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastRecommendation_RuleId DEFAULT(''),
    ReportRoute                          VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastRecommendation_ReportRoute DEFAULT(''),
    DrillDownRoute                       VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastRecommendation_DrillDownRoute DEFAULT(''),

    CONSTRAINT PK_BTRPD_CustomerRiskForecastRecommendation PRIMARY KEY CLUSTERED (CustomerRiskForecastRecommendationId)
)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_CustomerRiskForecastRecommendation_SnapshotKey_SortOrder' AND object_id = OBJECT_ID(N'dbo.BTRPD_CustomerRiskForecastRecommendation'))
CREATE INDEX IX_BTRPD_CustomerRiskForecastRecommendation_SnapshotKey_SortOrder
    ON BTRPD_CustomerRiskForecastRecommendation (SnapshotKey, SortOrder)
GO

-- BTRPD_CollectionOptimizationKpi (M30)
IF OBJECT_ID(N'dbo.BTRPD_CollectionOptimizationKpi', N'U') IS NULL
BEGIN
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
END
GO

-- BTRPD_CollectionOptimizationActionDist
IF OBJECT_ID(N'dbo.BTRPD_CollectionOptimizationActionDist', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_CollectionOptimizationActionDist
(
    CollectionOptimizationActionDistId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationActionDist_Id DEFAULT(''),
    SnapshotKey                        VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationActionDist_SnapshotKey DEFAULT('CURRENT'),
    ActionCategoryKey                  VARCHAR(30)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationActionDist_ActionCategoryKey DEFAULT(''),
    ActionCategoryLabel                VARCHAR(60)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationActionDist_ActionCategoryLabel DEFAULT(''),
    CustomerCount                      INT           NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationActionDist_CustomerCount DEFAULT(0),
    ImpactTotal                        DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationActionDist_ImpactTotal DEFAULT(0),
    SortOrder                          INT           NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationActionDist_SortOrder DEFAULT(0),

    CONSTRAINT PK_BTRPD_CollectionOptimizationActionDist PRIMARY KEY CLUSTERED (CollectionOptimizationActionDistId)
)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_CollectionOptimizationActionDist_SnapshotKey_SortOrder' AND object_id = OBJECT_ID(N'dbo.BTRPD_CollectionOptimizationActionDist'))
CREATE INDEX IX_BTRPD_CollectionOptimizationActionDist_SnapshotKey_SortOrder
    ON BTRPD_CollectionOptimizationActionDist (SnapshotKey, SortOrder)
GO

-- BTRPD_CollectionOptimizationWorkload
IF OBJECT_ID(N'dbo.BTRPD_CollectionOptimizationWorkload', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_CollectionOptimizationWorkload
(
    CollectionOptimizationWorkloadId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationWorkload_Id DEFAULT(''),
    SnapshotKey                      VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationWorkload_SnapshotKey DEFAULT('CURRENT'),
    WorkloadType                     VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationWorkload_WorkloadType DEFAULT(''),
    EntityKey                        VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationWorkload_EntityKey DEFAULT(''),
    EntityLabel                      VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationWorkload_EntityLabel DEFAULT(''),
    ActionCount                      INT            NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationWorkload_ActionCount DEFAULT(0),
    ImmediateCount                   INT            NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationWorkload_ImmediateCount DEFAULT(0),
    ImpactTotal                      DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationWorkload_ImpactTotal DEFAULT(0),
    OverdueExposure                  DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationWorkload_OverdueExposure DEFAULT(0),
    IsHotspot                        BIT            NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationWorkload_IsHotspot DEFAULT(0),
    SortOrder                        INT            NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationWorkload_SortOrder DEFAULT(0),

    CONSTRAINT PK_BTRPD_CollectionOptimizationWorkload PRIMARY KEY CLUSTERED (CollectionOptimizationWorkloadId)
)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_CollectionOptimizationWorkload_SnapshotKey_SortOrder' AND object_id = OBJECT_ID(N'dbo.BTRPD_CollectionOptimizationWorkload'))
CREATE INDEX IX_BTRPD_CollectionOptimizationWorkload_SnapshotKey_SortOrder
    ON BTRPD_CollectionOptimizationWorkload (SnapshotKey, SortOrder)
GO

-- BTRPD_CollectionOptimizationPriority
IF OBJECT_ID(N'dbo.BTRPD_CollectionOptimizationPriority', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_CollectionOptimizationPriority
(
    CollectionOptimizationPriorityId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_Id DEFAULT(''),
    SnapshotKey                      VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_SnapshotKey DEFAULT('CURRENT'),
    SortOrder                        INT            NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_SortOrder DEFAULT(0),
    CollectionPriorityScore          INT            NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_CollectionPriorityScore DEFAULT(0),
    CustomerCode                     VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_CustomerCode DEFAULT(''),
    CustomerName                     VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_CustomerName DEFAULT(''),
    WilayahName                      VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_WilayahName DEFAULT(''),
    SalesPersonName                  VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_SalesPersonName DEFAULT(''),
    Klasifikasi                      VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_Klasifikasi DEFAULT(''),
    ActionCategoryKey                VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_ActionCategoryKey DEFAULT(''),
    ActionCategoryLabel              VARCHAR(60)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_ActionCategoryLabel DEFAULT(''),
    RecommendedActionKey             VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_RecommendedActionKey DEFAULT(''),
    RecommendedActionLabel           VARCHAR(60)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_RecommendedActionLabel DEFAULT(''),
    ActionOwner                      VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_ActionOwner DEFAULT(''),
    OpenBalance                      DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_OpenBalance DEFAULT(0),
    OverdueBalance                   DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_OverdueBalance DEFAULT(0),
    DueWithin7Days                   DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_DueWithin7Days DEFAULT(0),
    CollectionImpactAmount           DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_CollectionImpactAmount DEFAULT(0),
    M29Category                      VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_M29Category DEFAULT(''),
    M29RecommendationKey             VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_M29RecommendationKey DEFAULT(''),
    M29PrimarySignalKey              VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_M29PrimarySignalKey DEFAULT(''),
    MinDaysUntilDue                  INT            NULL,
    CreditUtilizationPercent         DECIMAL(9,4)   NULL,
    SelectionReasonText              VARCHAR(500)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_SelectionReasonText DEFAULT(''),
    PriorityReasonText               VARCHAR(500)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_PriorityReasonText DEFAULT(''),
    ActionReasonText                 VARCHAR(500)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_ActionReasonText DEFAULT(''),
    TriggeredRuleIds                 VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_TriggeredRuleIds DEFAULT(''),
    ReportRoute                      VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_ReportRoute DEFAULT(''),
    DrillDownRoute                   VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_DrillDownRoute DEFAULT(''),

    CONSTRAINT PK_BTRPD_CollectionOptimizationPriority PRIMARY KEY CLUSTERED (CollectionOptimizationPriorityId)
)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_CollectionOptimizationPriority_SnapshotKey_SortOrder' AND object_id = OBJECT_ID(N'dbo.BTRPD_CollectionOptimizationPriority'))
CREATE INDEX IX_BTRPD_CollectionOptimizationPriority_SnapshotKey_SortOrder
    ON BTRPD_CollectionOptimizationPriority (SnapshotKey, SortOrder)
GO

-- BTRPD_CollectionOptimizationQueue
IF OBJECT_ID(N'dbo.BTRPD_CollectionOptimizationQueue', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_CollectionOptimizationQueue
(
    CollectionOptimizationQueueId    VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_Id DEFAULT(''),
    SnapshotKey                      VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_SnapshotKey DEFAULT('CURRENT'),
    QueueKey                         VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_QueueKey DEFAULT(''),
    SortOrder                        INT            NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_SortOrder DEFAULT(0),
    CollectionPriorityScore          INT            NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_CollectionPriorityScore DEFAULT(0),
    CustomerCode                     VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_CustomerCode DEFAULT(''),
    CustomerName                     VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_CustomerName DEFAULT(''),
    WilayahName                      VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_WilayahName DEFAULT(''),
    SalesPersonName                  VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_SalesPersonName DEFAULT(''),
    ActionCategoryKey                VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_ActionCategoryKey DEFAULT(''),
    ActionCategoryLabel              VARCHAR(60)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_ActionCategoryLabel DEFAULT(''),
    RecommendedActionKey             VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_RecommendedActionKey DEFAULT(''),
    RecommendedActionLabel           VARCHAR(60)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_RecommendedActionLabel DEFAULT(''),
    ActionOwner                      VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_ActionOwner DEFAULT(''),
    OverdueBalance                   DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_OverdueBalance DEFAULT(0),
    DueWithin7Days                   DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_DueWithin7Days DEFAULT(0),
    CollectionImpactAmount           DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_CollectionImpactAmount DEFAULT(0),
    M29Category                      VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_M29Category DEFAULT(''),
    QueueReasonText                  VARCHAR(500)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_QueueReasonText DEFAULT(''),
    ReportRoute                      VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_ReportRoute DEFAULT(''),
    DrillDownRoute                   VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_DrillDownRoute DEFAULT(''),

    CONSTRAINT PK_BTRPD_CollectionOptimizationQueue PRIMARY KEY CLUSTERED (CollectionOptimizationQueueId)
)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_CollectionOptimizationQueue_SnapshotKey_QueueKey_SortOrder' AND object_id = OBJECT_ID(N'dbo.BTRPD_CollectionOptimizationQueue'))
CREATE INDEX IX_BTRPD_CollectionOptimizationQueue_SnapshotKey_QueueKey_SortOrder
    ON BTRPD_CollectionOptimizationQueue (SnapshotKey, QueueKey, SortOrder)
GO

-- BTRPD_CollectionOptimizationImpact
IF OBJECT_ID(N'dbo.BTRPD_CollectionOptimizationImpact', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_CollectionOptimizationImpact
(
    CollectionOptimizationImpactId   VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationImpact_Id DEFAULT(''),
    SnapshotKey                      VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationImpact_SnapshotKey DEFAULT('CURRENT'),
    SortOrder                        INT            NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationImpact_SortOrder DEFAULT(0),
    CustomerCode                     VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationImpact_CustomerCode DEFAULT(''),
    CustomerName                     VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationImpact_CustomerName DEFAULT(''),
    WilayahName                      VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationImpact_WilayahName DEFAULT(''),
    SalesPersonName                  VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationImpact_SalesPersonName DEFAULT(''),
    ActionCategoryKey                VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationImpact_ActionCategoryKey DEFAULT(''),
    ActionCategoryLabel              VARCHAR(60)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationImpact_ActionCategoryLabel DEFAULT(''),
    CollectionImpactAmount           DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationImpact_CollectionImpactAmount DEFAULT(0),
    OverdueBalance                   DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationImpact_OverdueBalance DEFAULT(0),
    DueWithin7Days                   DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationImpact_DueWithin7Days DEFAULT(0),
    ReportRoute                      VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationImpact_ReportRoute DEFAULT(''),
    DrillDownRoute                   VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationImpact_DrillDownRoute DEFAULT(''),

    CONSTRAINT PK_BTRPD_CollectionOptimizationImpact PRIMARY KEY CLUSTERED (CollectionOptimizationImpactId)
)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_CollectionOptimizationImpact_SnapshotKey_SortOrder' AND object_id = OBJECT_ID(N'dbo.BTRPD_CollectionOptimizationImpact'))
CREATE INDEX IX_BTRPD_CollectionOptimizationImpact_SnapshotKey_SortOrder
    ON BTRPD_CollectionOptimizationImpact (SnapshotKey, SortOrder)
GO

-- IX_BTR_Piutang_OpenBalance
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTR_Piutang_OpenBalance' AND object_id = OBJECT_ID(N'dbo.BTR_Piutang'))
CREATE INDEX IX_BTR_Piutang_OpenBalance
    ON [dbo].[BTR_Piutang] (Sisa, PiutangId)
    INCLUDE (DueDate, Total, CustomerId)
    WHERE Sisa > 1
GO
