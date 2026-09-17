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
GO
