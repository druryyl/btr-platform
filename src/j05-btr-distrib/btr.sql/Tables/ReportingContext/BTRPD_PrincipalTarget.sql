CREATE TABLE BTRPD_PrincipalTarget
(
    PrincipalTargetId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalTarget_PrincipalTargetId DEFAULT(''),
    SnapshotKey       VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalTarget_SnapshotKey DEFAULT('CURRENT'),
    KpiId             VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalTarget_KpiId DEFAULT('PRN-TGT-001'),
    PeriodYear        INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalTarget_PeriodYear DEFAULT(0),
    PeriodMonth       INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalTarget_PeriodMonth DEFAULT(0),
    SupplierId        VARCHAR(5)    NOT NULL CONSTRAINT DF_BTRPD_PrincipalTarget_SupplierId DEFAULT(''),
    SupplierName      VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalTarget_SupplierName DEFAULT(''),
    TargetAmount      DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PrincipalTarget_TargetAmount DEFAULT(0),
    SourceCount       INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalTarget_SourceCount DEFAULT(0),
    SortOrder         INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalTarget_SortOrder DEFAULT(0),
    GeneratedAt       DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PrincipalTarget_GeneratedAt DEFAULT('3000-01-01'),
    LastRefreshLogId  VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalTarget_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PrincipalTarget PRIMARY KEY CLUSTERED (PrincipalTargetId),
    CONSTRAINT UX_BTRPD_PrincipalTarget_SnapshotKey_SupplierId UNIQUE (SnapshotKey, SupplierId)
)
GO

CREATE INDEX IX_BTRPD_PrincipalTarget_SnapshotKey_SortOrder
    ON BTRPD_PrincipalTarget (SnapshotKey, SortOrder)
GO
