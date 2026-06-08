CREATE TABLE BTR_PortalDashboardSalesmanSegmentation
(
    SalesmanSegmentationId VARCHAR(13)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesmanSegmentation_SalesmanSegmentationId DEFAULT(''),
    SnapshotKey            VARCHAR(10)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesmanSegmentation_SnapshotKey DEFAULT('CURRENT'),
    SegmentType            VARCHAR(20)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesmanSegmentation_SegmentType DEFAULT(''),
    SegmentKey             VARCHAR(30)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesmanSegmentation_SegmentKey DEFAULT(''),
    SegmentLabel           VARCHAR(50)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesmanSegmentation_SegmentLabel DEFAULT(''),
    SalesmanCount          INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesmanSegmentation_SalesmanCount DEFAULT(0),
    ActiveCount            INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesmanSegmentation_ActiveCount DEFAULT(0),
    InactiveCount          INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesmanSegmentation_InactiveCount DEFAULT(0),
    SortOrder              INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesmanSegmentation_SortOrder DEFAULT(0),

    CONSTRAINT PK_BTR_PortalDashboardSalesmanSegmentation PRIMARY KEY CLUSTERED (SalesmanSegmentationId),
    CONSTRAINT UX_BTR_PortalDashboardSalesmanSegmentation_SnapshotKey_SegmentType_SegmentKey UNIQUE (SnapshotKey, SegmentType, SegmentKey)
)
GO
