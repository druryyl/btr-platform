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
GO

CREATE INDEX IX_BTRPD_PrincipalInventory_SnapshotKey_SortOrder
    ON BTRPD_PrincipalInventory (SnapshotKey, SortOrder)
GO
