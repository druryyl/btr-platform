IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.BTRPD_EntityAnalytics_Relationship') AND name = N'RelationshipStatus')
    ALTER TABLE BTRPD_EntityAnalytics_Relationship ADD RelationshipStatus VARCHAR(20) NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.BTRPD_EntityAnalytics_Relationship') AND name = N'LastTransactionDate')
    ALTER TABLE BTRPD_EntityAnalytics_Relationship ADD LastTransactionDate DATETIME NULL;
GO
