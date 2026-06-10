CREATE TABLE BTRPD_LocationTopWarehouseAtRisk

(

    LocationTopWarehouseAtRiskId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehouseAtRisk_LocationTopWarehouseAtRiskId DEFAULT(''),

    SnapshotKey                  VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehouseAtRisk_SnapshotKey DEFAULT('CURRENT'),

    Rank                         INT           NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehouseAtRisk_Rank DEFAULT(0),

    WarehouseId                  VARCHAR(5)    NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehouseAtRisk_WarehouseId DEFAULT(''),

    WarehouseName                VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehouseAtRisk_WarehouseName DEFAULT(''),

    AtRiskValue                  DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehouseAtRisk_AtRiskValue DEFAULT(0),

    PercentOfTotal               DECIMAL(9,4)  NULL,

    ReportRoute                  VARCHAR(100)  NULL,



    CONSTRAINT PK_BTRPD_LocationTopWarehouseAtRisk PRIMARY KEY CLUSTERED (LocationTopWarehouseAtRiskId),

    CONSTRAINT UX_BTRPD_LocationTopWarehouseAtRisk_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)

)

GO

