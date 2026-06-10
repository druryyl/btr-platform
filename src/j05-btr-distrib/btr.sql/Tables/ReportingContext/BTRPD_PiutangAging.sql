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
GO
