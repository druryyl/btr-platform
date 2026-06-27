CREATE TABLE BTRPD_FieldActivitySalesman
(
    SnapshotKey             VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_FieldActivitySalesman_SnapshotKey DEFAULT('CURRENT'),
    SalesPersonId           VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_FieldActivitySalesman_SalesPersonId DEFAULT(''),
    SalesPersonCode         VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_FieldActivitySalesman_SalesPersonCode DEFAULT(''),
    SalesPersonName         VARCHAR(100)  NOT NULL CONSTRAINT DF_BTRPD_FieldActivitySalesman_SalesPersonName DEFAULT(''),
    WilayahName             VARCHAR(100)  NOT NULL CONSTRAINT DF_BTRPD_FieldActivitySalesman_WilayahName DEFAULT(''),
    HasEmail                BIT           NOT NULL CONSTRAINT DF_BTRPD_FieldActivitySalesman_HasEmail DEFAULT(0),
    Rank                    INT           NOT NULL CONSTRAINT DF_BTRPD_FieldActivitySalesman_Rank DEFAULT(0),
    PlannedVisits           INT           NOT NULL CONSTRAINT DF_BTRPD_FieldActivitySalesman_PlannedVisits DEFAULT(0),
    ActualVisits            INT           NOT NULL CONSTRAINT DF_BTRPD_FieldActivitySalesman_ActualVisits DEFAULT(0),
    VisitExecutionPercent   DECIMAL(9,4)  NULL,
    EffectiveCalls          INT           NOT NULL CONSTRAINT DF_BTRPD_FieldActivitySalesman_EffectiveCalls DEFAULT(0),
    EffectiveCallRate       DECIMAL(9,4)  NULL,
    MissedVisits            INT           NOT NULL CONSTRAINT DF_BTRPD_FieldActivitySalesman_MissedVisits DEFAULT(0),
    UnplannedVisits         INT           NOT NULL CONSTRAINT DF_BTRPD_FieldActivitySalesman_UnplannedVisits DEFAULT(0),
    GpsValidPercent         DECIMAL(9,4)  NULL,
    GpsValidCount           INT           NOT NULL CONSTRAINT DF_BTRPD_FieldActivitySalesman_GpsValidCount DEFAULT(0),
    GpsWarningCount         INT           NOT NULL CONSTRAINT DF_BTRPD_FieldActivitySalesman_GpsWarningCount DEFAULT(0),
    GpsSuspiciousCount      INT           NOT NULL CONSTRAINT DF_BTRPD_FieldActivitySalesman_GpsSuspiciousCount DEFAULT(0),
    OrdersCount             INT           NOT NULL CONSTRAINT DF_BTRPD_FieldActivitySalesman_OrdersCount DEFAULT(0),
    OmzetAmount             DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_FieldActivitySalesman_OmzetAmount DEFAULT(0),
    StatusCode              VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_FieldActivitySalesman_StatusCode DEFAULT(''),

    CONSTRAINT PK_BTRPD_FieldActivitySalesman PRIMARY KEY CLUSTERED (SnapshotKey, SalesPersonId)
)
GO
