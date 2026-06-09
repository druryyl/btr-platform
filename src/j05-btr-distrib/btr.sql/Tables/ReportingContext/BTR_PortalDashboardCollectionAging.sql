CREATE TABLE BTR_PortalDashboardCollectionAging
(
    CollectionAgingId VARCHAR(13)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardCollectionAging_CollectionAgingId DEFAULT(''),
    SnapshotKey         VARCHAR(10)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardCollectionAging_SnapshotKey DEFAULT('CURRENT'),
    BucketKey           VARCHAR(20)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardCollectionAging_BucketKey DEFAULT(''),
    BucketLabel         VARCHAR(30)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardCollectionAging_BucketLabel DEFAULT(''),
    Amount              DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_PortalDashboardCollectionAging_Amount DEFAULT(0),
    SortOrder           INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardCollectionAging_SortOrder DEFAULT(0),

    CONSTRAINT PK_BTR_PortalDashboardCollectionAging PRIMARY KEY CLUSTERED (CollectionAgingId),
    CONSTRAINT UX_BTR_PortalDashboardCollectionAging_SnapshotKey_BucketKey UNIQUE (SnapshotKey, BucketKey)
)
GO
