CREATE TABLE BTRPD_EntityAnalytics_BackfillLock
(
    EntityType    VARCHAR(30) NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillLock_EntityType DEFAULT(''),
    BackfillJobId VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillLock_JobId DEFAULT(''),
    AcquiredAt    DATETIME    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillLock_AcquiredAt DEFAULT('3000-01-01'),

    CONSTRAINT PK_BTRPD_EntityAnalytics_BackfillLock PRIMARY KEY CLUSTERED (EntityType)
)
GO
