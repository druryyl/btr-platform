CREATE TABLE BTRPD_CashFlowCollectionRisk
(
    CashFlowCollectionRiskId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_CashFlowCollectionRisk_CashFlowCollectionRiskId DEFAULT(''),
    SnapshotKey              VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_CashFlowCollectionRisk_SnapshotKey DEFAULT('CURRENT'),
    SortOrder                INT            NOT NULL CONSTRAINT DF_BTRPD_CashFlowCollectionRisk_SortOrder DEFAULT(0),
    RiskKey                  VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CashFlowCollectionRisk_RiskKey DEFAULT(''),
    RiskLabel                VARCHAR(60)    NOT NULL CONSTRAINT DF_BTRPD_CashFlowCollectionRisk_RiskLabel DEFAULT(''),
    EntityType               VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_CashFlowCollectionRisk_EntityType DEFAULT(''),
    EntityId                 VARCHAR(13)    NOT NULL CONSTRAINT DF_BTRPD_CashFlowCollectionRisk_EntityId DEFAULT(''),
    EntityName               VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CashFlowCollectionRisk_EntityName DEFAULT(''),
    Amount                   DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CashFlowCollectionRisk_Amount DEFAULT(0),
    DueOrAgingText           VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_CashFlowCollectionRisk_DueOrAgingText DEFAULT(''),
    RuleExplanation          VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_CashFlowCollectionRisk_RuleExplanation DEFAULT(''),
    ReportRoute              VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CashFlowCollectionRisk_ReportRoute DEFAULT(''),

    CONSTRAINT PK_BTRPD_CashFlowCollectionRisk PRIMARY KEY CLUSTERED (CashFlowCollectionRiskId)
)
GO

CREATE INDEX IX_BTRPD_CashFlowCollectionRisk_SnapshotKey_SortOrder
    ON BTRPD_CashFlowCollectionRisk (SnapshotKey, SortOrder)
GO
