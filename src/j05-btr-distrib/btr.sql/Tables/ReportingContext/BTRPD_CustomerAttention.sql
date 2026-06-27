CREATE TABLE BTRPD_CustomerAttention
(
    CustomerAttentionId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_CustomerAttention_CustomerAttentionId DEFAULT(''),
    SnapshotKey         VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_CustomerAttention_SnapshotKey DEFAULT('CURRENT'),
    CustomerId          VARCHAR(13)   NOT NULL CONSTRAINT DF_BTRPD_CustomerAttention_CustomerId DEFAULT(''),
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
GO

CREATE INDEX IX_BTRPD_CustomerAttention_SnapshotKey_SortOrder
    ON BTRPD_CustomerAttention (SnapshotKey, SortOrder)
GO
