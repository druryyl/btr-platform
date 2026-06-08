CREATE TABLE BTR_PortalDashboardCustomerSegmentation
(
    CustomerSegmentationId VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_PortalDashboardCustomerSegmentation_CustomerSegmentationId DEFAULT(''),
    SnapshotKey            VARCHAR(10) NOT NULL CONSTRAINT DF_BTR_PortalDashboardCustomerSegmentation_SnapshotKey DEFAULT('CURRENT'),
    SegmentType            VARCHAR(20) NOT NULL CONSTRAINT DF_BTR_PortalDashboardCustomerSegmentation_SegmentType DEFAULT(''),
    SegmentKey             VARCHAR(30) NOT NULL CONSTRAINT DF_BTR_PortalDashboardCustomerSegmentation_SegmentKey DEFAULT(''),
    SegmentLabel           VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_PortalDashboardCustomerSegmentation_SegmentLabel DEFAULT(''),
    CustomerCount          INT         NOT NULL CONSTRAINT DF_BTR_PortalDashboardCustomerSegmentation_CustomerCount DEFAULT(0),
    ActiveCount            INT         NOT NULL CONSTRAINT DF_BTR_PortalDashboardCustomerSegmentation_ActiveCount DEFAULT(0),
    DormantCount           INT         NOT NULL CONSTRAINT DF_BTR_PortalDashboardCustomerSegmentation_DormantCount DEFAULT(0),
    SortOrder              INT         NOT NULL CONSTRAINT DF_BTR_PortalDashboardCustomerSegmentation_SortOrder DEFAULT(0),

    CONSTRAINT PK_BTR_PortalDashboardCustomerSegmentation PRIMARY KEY CLUSTERED (CustomerSegmentationId),
    CONSTRAINT UX_BTR_PortalDashboardCustomerSegmentation_SnapshotKey_SegmentType_SegmentKey UNIQUE (SnapshotKey, SegmentType, SegmentKey)
)
GO
