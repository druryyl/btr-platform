CREATE TABLE BTRPD_CollectionOptimizationImpact
(
    CollectionOptimizationImpactId   VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationImpact_Id DEFAULT(''),
    SnapshotKey                      VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationImpact_SnapshotKey DEFAULT('CURRENT'),
    SortOrder                        INT            NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationImpact_SortOrder DEFAULT(0),
    CustomerCode                     VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationImpact_CustomerCode DEFAULT(''),
    CustomerName                     VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationImpact_CustomerName DEFAULT(''),
    WilayahName                      VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationImpact_WilayahName DEFAULT(''),
    SalesPersonName                  VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationImpact_SalesPersonName DEFAULT(''),
    ActionCategoryKey                VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationImpact_ActionCategoryKey DEFAULT(''),
    ActionCategoryLabel              VARCHAR(60)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationImpact_ActionCategoryLabel DEFAULT(''),
    CollectionImpactAmount           DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationImpact_CollectionImpactAmount DEFAULT(0),
    OverdueBalance                   DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationImpact_OverdueBalance DEFAULT(0),
    DueWithin7Days                   DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationImpact_DueWithin7Days DEFAULT(0),
    ReportRoute                      VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationImpact_ReportRoute DEFAULT(''),
    DrillDownRoute                   VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationImpact_DrillDownRoute DEFAULT(''),

    CONSTRAINT PK_BTRPD_CollectionOptimizationImpact PRIMARY KEY CLUSTERED (CollectionOptimizationImpactId)
)
GO

CREATE INDEX IX_BTRPD_CollectionOptimizationImpact_SnapshotKey_SortOrder
    ON BTRPD_CollectionOptimizationImpact (SnapshotKey, SortOrder)
GO
