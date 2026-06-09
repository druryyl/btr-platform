CREATE TABLE BTR_PortalDashboardCollectionTopOverdueWilayah
(
    CollectionTopOverdueWilayahId VARCHAR(13)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardCollectionTopOverdueWilayah_CollectionTopOverdueWilayahId DEFAULT(''),
    SnapshotKey                   VARCHAR(10)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardCollectionTopOverdueWilayah_SnapshotKey DEFAULT('CURRENT'),
    Rank                          INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardCollectionTopOverdueWilayah_Rank DEFAULT(0),
    WilayahId                     VARCHAR(13)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardCollectionTopOverdueWilayah_WilayahId DEFAULT(''),
    WilayahName                   VARCHAR(50)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardCollectionTopOverdueWilayah_WilayahName DEFAULT(''),
    OverdueBalance                DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_PortalDashboardCollectionTopOverdueWilayah_OverdueBalance DEFAULT(0),
    PercentOfTotal                DECIMAL(9,4)  NULL,

    CONSTRAINT PK_BTR_PortalDashboardCollectionTopOverdueWilayah PRIMARY KEY CLUSTERED (CollectionTopOverdueWilayahId),
    CONSTRAINT UX_BTR_PortalDashboardCollectionTopOverdueWilayah_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)
)
GO
