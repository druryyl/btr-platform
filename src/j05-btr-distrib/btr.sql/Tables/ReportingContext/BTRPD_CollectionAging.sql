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
GO
