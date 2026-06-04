CREATE DATABASE btrgd
CONTAINMENT = NONE
ON PRIMARY 
(
    NAME = N'btrgd', 
    FILENAME = N'D:\Database\btr-gp\btrgd_data.mdf'
)
LOG ON 
(
    NAME = N'btrgd_log', 
    FILENAME = N'D:\Database\btr-gp\btrgd_log.ldf' 
);
GO

CREATE LOGIN btrGudangLogin WITH PASSWORD = 'btrGudang123!'
GO

USE btrgd;
GO

CREATE USER btrGudangUser FOR LOGIN btrGudangLogin;
GO

sp_addrolemember N'db_datareader', N'btrGudangUser';
GO

sp_addrolemember N'db_datawriter', N'btrGudangUser';
GO

sp_addrolemember N'db_ddladmin', N'btrGudangUser';
GO
