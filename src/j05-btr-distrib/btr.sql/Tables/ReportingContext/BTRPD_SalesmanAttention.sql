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
    IsActive            BIT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanAttention_IsActive DEFAULT(0),

    CONSTRAINT PK_BTRPD_SalesmanAttention PRIMARY KEY CLUSTERED (SalesmanAttentionId)
)
GO

CREATE INDEX IX_BTRPD_SalesmanAttention_SnapshotKey_SortOrder
    ON BTRPD_SalesmanAttention (SnapshotKey, SortOrder)
GO
