CREATE TABLE BTRPD_PiutangKpi
(
    SnapshotKey                         VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PiutangKpi_SnapshotKey DEFAULT('CURRENT'),
    GeneratedAt                         DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PiutangKpi_GeneratedAt DEFAULT('3000-01-01'),
    TotalPiutang                        DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangKpi_TotalPiutang DEFAULT(0),
    TotalCustomer                       INT           NOT NULL CONSTRAINT DF_BTRPD_PiutangKpi_TotalCustomer DEFAULT(0),
    OverdueCustomer                     INT           NOT NULL CONSTRAINT DF_BTRPD_PiutangKpi_OverdueCustomer DEFAULT(0),
    OverduePiutang                      DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangKpi_OverduePiutang DEFAULT(0),
    AgingOver90Amount                   DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangKpi_AgingOver90Amount DEFAULT(0),
    AgingOver90Percent                  DECIMAL(9,4)  NULL,
    Top10CustomerConcentrationPercent   DECIMAL(9,4)  NULL,
    Top20CustomerConcentrationPercent   DECIMAL(9,4)  NULL,
    LastRefreshLogId                    VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PiutangKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PiutangKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO
