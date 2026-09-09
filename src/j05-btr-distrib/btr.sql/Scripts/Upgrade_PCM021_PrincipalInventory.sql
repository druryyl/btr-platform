-- PCM-021 Principal inventory snapshot.
-- Stores PRN-INV-001 and PRN-INV-002 mapped from Inventory Snapshot evidence.
-- Does not write PRN-SALES-001 and does not change IN01-IN05 tables.
SET NOCOUNT ON;
GO

IF OBJECT_ID(N'dbo.BTRPD_PrincipalInventoryKpi', N'U') IS NULL
BEGIN
    CREATE TABLE BTRPD_PrincipalInventoryKpi
    (
        SnapshotKey           VARCHAR(10) NOT NULL CONSTRAINT DF_BTRPD_PrincipalInventoryKpi_SnapshotKey DEFAULT('CURRENT'),
        InventoryValueKpiId   VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalInventoryKpi_InventoryValueKpiId DEFAULT('PRN-INV-001'),
        InventoryDaysKpiId    VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalInventoryKpi_InventoryDaysKpiId DEFAULT('PRN-INV-002'),
        BusinessDate          DATETIME    NOT NULL CONSTRAINT DF_BTRPD_PrincipalInventoryKpi_BusinessDate DEFAULT('3000-01-01'),
        GeneratedAt           DATETIME    NOT NULL CONSTRAINT DF_BTRPD_PrincipalInventoryKpi_GeneratedAt DEFAULT('3000-01-01'),
        LastRefreshLogId      VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_PrincipalInventoryKpi_LastRefreshLogId DEFAULT(''),

        CONSTRAINT PK_BTRPD_PrincipalInventoryKpi PRIMARY KEY CLUSTERED (SnapshotKey)
    )
END
GO

IF OBJECT_ID(N'dbo.BTRPD_PrincipalInventory', N'U') IS NULL
BEGIN
    CREATE TABLE BTRPD_PrincipalInventory
    (
        PrincipalInventoryId  VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalInventory_PrincipalInventoryId DEFAULT(''),
        SnapshotKey           VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalInventory_SnapshotKey DEFAULT('CURRENT'),
        InventoryValueKpiId   VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalInventory_InventoryValueKpiId DEFAULT('PRN-INV-001'),
        InventoryDaysKpiId    VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalInventory_InventoryDaysKpiId DEFAULT('PRN-INV-002'),
        SupplierId            VARCHAR(5)    NOT NULL CONSTRAINT DF_BTRPD_PrincipalInventory_SupplierId DEFAULT(''),
        SupplierName          VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalInventory_SupplierName DEFAULT(''),
        InventoryValue        DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PrincipalInventory_InventoryValue DEFAULT(0),
        InventoryDays         DECIMAL(18,2) NULL,
        ItemCount             INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalInventory_ItemCount DEFAULT(0),
        SortOrder             INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalInventory_SortOrder DEFAULT(0),
        BusinessDate          DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PrincipalInventory_BusinessDate DEFAULT('3000-01-01'),
        GeneratedAt           DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PrincipalInventory_GeneratedAt DEFAULT('3000-01-01'),
        LastRefreshLogId      VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalInventory_LastRefreshLogId DEFAULT(''),

        CONSTRAINT PK_BTRPD_PrincipalInventory PRIMARY KEY CLUSTERED (PrincipalInventoryId),
        CONSTRAINT UX_BTRPD_PrincipalInventory_SnapshotKey_SupplierId UNIQUE (SnapshotKey, SupplierId)
    )
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_BTRPD_PrincipalInventory_SnapshotKey_SortOrder'
      AND object_id = OBJECT_ID(N'dbo.BTRPD_PrincipalInventory'))
BEGIN
    CREATE INDEX IX_BTRPD_PrincipalInventory_SnapshotKey_SortOrder
        ON BTRPD_PrincipalInventory (SnapshotKey, SortOrder)
END
GO
