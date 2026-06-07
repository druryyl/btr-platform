CREATE TABLE BTR_PortalDashboardPiutangAging
(
    PiutangAgingId VARCHAR(13)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardPiutangAging_PiutangAgingId DEFAULT(''),
    SnapshotKey    VARCHAR(10)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardPiutangAging_SnapshotKey DEFAULT('CURRENT'),
    BucketKey      VARCHAR(20)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardPiutangAging_BucketKey DEFAULT(''),
    BucketLabel    VARCHAR(50)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardPiutangAging_BucketLabel DEFAULT(''),
    SortOrder      INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardPiutangAging_SortOrder DEFAULT(0),
    Amount         DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_PortalDashboardPiutangAging_Amount DEFAULT(0),

    CONSTRAINT PK_BTR_PortalDashboardPiutangAging PRIMARY KEY CLUSTERED (PiutangAgingId),
    CONSTRAINT UX_BTR_PortalDashboardPiutangAging_SnapshotKey_BucketKey UNIQUE (SnapshotKey, BucketKey)
)
GO
